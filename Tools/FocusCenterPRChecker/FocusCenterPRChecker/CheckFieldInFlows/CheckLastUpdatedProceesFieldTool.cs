// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusCenterPRChecker.CheckFieldInFlows.Models;
using FocusCenterPRChecker.ConnectionReferenceTool;
using FocusCenterPRChecker.ConnectionReferenceTool.Models;
using FocusCenterPRChecker.DuplicatesComponentTool.Enums;
using FocusCenterPRChecker.DuplicatesComponentTool.Models;
using FocusCenterPRChecker.Helpers;
using FocusCenterPRChecker.Managers;
using Newtonsoft.Json.Linq;

namespace FocusCenterPRChecker.CheckFieldInFlows
{
    // Points out if any flow has not set last updated process field for auditing purposes
    public static class CheckLastUpdatedProceesFieldTool
    {
        // Get changed flows and list flows that has not set last updated process field for auditing purposes
        public static string ProcessFlows(List<ComponentModel> changedComponentModels)
        {
            Console.WriteLine("\t Getting all changed components from Workflows folder");

            var changedWorkflows = changedComponentModels.Where(changedComponent =>
                changedComponent.ComponentFolder == ComponentFolder.Workflows.ToString()).ToList();

            Console.WriteLine("\t Getting only flows");

            var changedFlows = GetFlows(changedWorkflows);

            if (changedFlows.Count == 0)
            {
                Console.WriteLine("\t There are no changed Flows");
                return "";
            }

            Console.WriteLine("\t Parsing Flows");

            var parsedFlows = ParseFlows(changedFlows);

            Console.WriteLine($"\t Getting flows which don't update {ConfigManager.FlowAuditingAttribute} field");

            var checkedFlowResult = CheckFlows(parsedFlows);

            Console.WriteLine("\t Getting rows for result table");

            var rows = GetRowsForTable(checkedFlowResult);

            Console.WriteLine("\t Returning the result");

            return GetResultTableForFlows(rows);

        }

        // Create rows for output with flow name and action
        private static List<Row> GetRowsForTable(HashSet<TableFlowModel> resultModels)
        {
            List<Row> rows = new List<Row>();
            
            foreach (var resultFlowModel in resultModels)
            {
                Row row = new Row();

                row.Cells.AddRange(new List<Cell>()
                {
                    new Cell(resultFlowModel.FlowName),
                    new Cell(resultFlowModel.Actions)
                });

                rows.Add(row);
            }

            return rows;
        }

        // create HTML table for PR comments with missing auditing attribute name and its action
        private static string GetResultTableForFlows(List<Row> rows)
        {
            if (rows.Count == 0)
            {
                return "";
            }
            
            var commentText = $"Please populate the \"{ConfigManager.FlowAuditingAttribute}\" attribute with the current Flow Name in all the Dataverse Create/Update actions listed below. We need this for record auditing purposes in the Environment:";

            StringBuilder sb = new StringBuilder();

            sb.Append($"{commentText}<br/><table style='width:{TableStyleSettings.TableWidthSetting}'>");
            sb.Append("<tr>" +
                      $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Flows missing {ConfigManager.FlowAuditingAttribute} Information</th>" +
                      $"<th style='padding:{TableStyleSettings.PaddingStyle}'>Action Name</th>");

            foreach (var row in rows)
            {
                sb.Append(row.ToHTML());
            }

            sb.Append("</table>");

            return sb.ToString();
        }

        // Get flows from changed components workflows
        private static List<FlowModel> GetFlows(List<ComponentModel> changedWorkflows)
        {
            List<FlowModel> flows = new List<FlowModel>();

            foreach (var file in changedWorkflows)
            {
                string pathToDataConfig = file.Path;

                if (!file.ComponentName.EndsWith("json.data.xml"))
                {
                    pathToDataConfig = $"{file.Path}.data.xml";
                }

                var flowModel = GetFlowModelFromDataConfig(pathToDataConfig, file.SolutionName);

                if (flowModel != null && !flows.Exists(f => f.FlowName == flowModel.FlowName))
                    flows.Add(flowModel);
            }

            return flows;
        }

        // create flow model with flow name and path
        private static FlowModel GetFlowModelFromDataConfig(string path, string solutionName)
        {
            var filePath = $"{ConfigManager.PathToRepository}{ path.Replace("/", "\\")}";
            Console.WriteLine($"FlowModel file path: {filePath}");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"\t File {filePath} doesn't exist in the folder");
                return null;
            }

            XElement worflowDataFile = XElement.Load($"{filePath}");
            var category = worflowDataFile.Descendants("Category").FirstOrDefault()?.Value;

            if (category == null)
                return null;

            var workflowCategory = (WorkflowCategory) int.Parse(category);

            if (workflowCategory != WorkflowCategory.ModernFlow) 
                return null;

            var jsonFileName = worflowDataFile.Descendants("JsonFileName").FirstOrDefault()?.Value.Split('/').LastOrDefault();

            return new FlowModel
            {
                FlowName = worflowDataFile.Attribute("Name")?.Value.Trim(), 
                FlowPath = $"{ConfigManager.PathToRepository}\\{solutionName}{ConfigManager.SolutionFilesPath}\\Workflows\\{jsonFileName}"
            };

        }

        // create flow model with flow name and actions
        private static List<FlowJsonModel> ParseFlows(List<FlowModel> flows)
        {
            List<FlowJsonModel> flowJsonModels = new List<FlowJsonModel>();

            foreach (var flow in flows)
            {
                using (StreamReader r = new StreamReader(flow.FlowPath))
                {
                    var json = r.ReadToEnd();
                    var jobj = JObject.Parse(json);

                    var actionsJTokens = jobj.SelectToken("properties").SelectToken("definition").SelectToken("actions");

                    var actions = ParseActions(actionsJTokens);

                    flowJsonModels.Add(new FlowJsonModel() {FlowName = flow.FlowName, Actions = actions });
                }
            }

            return flowJsonModels;
        }

        private static List<KeyValuePair<string, ActionModel>> ParseActions(JToken actionsJToken)
        {
            List<KeyValuePair<string, ActionModel>> newActions = new List<KeyValuePair<string, ActionModel>>();

            foreach (JToken token in actionsJToken.Children())
            {
                var tokenInnerActions = token.First?.SelectToken("actions");
                var tokenInnerElse = token.First?.SelectToken("else");

                if (tokenInnerActions != null || tokenInnerElse != null)
                {
                    if (tokenInnerActions != null)
                        newActions.AddRange(ParseActions(tokenInnerActions));
                    if (tokenInnerElse != null)
                        newActions.AddRange(ParseActions(tokenInnerElse.SelectToken("actions")));
                }
                else
                {
                    if (token.First?.SelectToken("inputs.host") != null &&
                        token.First?.SelectToken("inputs.parameters") != null)
                    { 
                        var parsedAction = token.First.ToObject<ActionModel>();
                        var name = token.ToObject<JProperty>()?.Name;

                        newActions.Add(new KeyValuePair<string, ActionModel>(name, parsedAction));
                    }
                }
            }

            return newActions;
        }

        // Get flow action with Dataverse Create/Update/Post/Patch actions 
        private static List<KeyValuePair<string, ActionModel>> GetOnlyCreateAndUpdateActions(FlowJsonModel flow)
        {
            List<string> operations = new List<string>()
            {
                "CreateRecord",
                "UpdateRecord",
                "PostItem_V2",
                "PatchItem_V2"
            };

            var createOrUpdateActions = flow.Actions.Where(action =>
                action.Value.Inputs.Host.ConnectionName.Contains("shared_commondataservice")
                && operations.Contains(action.Value.Inputs.Host.OperationId)).ToList();

            return createOrUpdateActions;
        }

        // If Auditing attribute field is not set then return flow name, action name and entity name
        private static HashSet<TableFlowModel> CheckFlows(List<FlowJsonModel> parsedFlows)
        {
            HashSet<TableFlowModel> flowResultModels = new HashSet<TableFlowModel>();

            foreach (var flow in parsedFlows)
            {
                var createOrUpdateActions = GetOnlyCreateAndUpdateActions(flow);

                if(createOrUpdateActions.Count == 0)
                {
                    Console.WriteLine($"\t Flow '{flow.FlowName}' doesn't have Dataverse Create/Update actions");
                }
                else
                {
                    var resultOfChecking = CheckActionsOfFlow(createOrUpdateActions, flow.FlowName);

                    if (resultOfChecking.Count > 0)
                    {
                        flowResultModels.Add(new TableFlowModel()
                        {
                            FlowName = flow.FlowName,
                            Actions = string.Join(",<br/>", resultOfChecking.Keys)
                        });
                    }
                }
            }

            return flowResultModels;
        }

        // If Auditing attribute field is not set then add action name and entity name in dictionary
        private static Dictionary<string, string> CheckActionsOfFlow(List<KeyValuePair<string, ActionModel>> createOrUpdateActions, string flowName)
        {
            var result = new Dictionary<string, string>(); // key - action name, value - entity name

            foreach (var action in createOrUpdateActions)
            {
                if (!IsLastUpdatedByProcessFieldPresent(action,flowName))
                {
                    result.Add(action.Key, GetEntityNameFromAction(action));
                }
            }

            return result;
        }

        // Get Entity Name From Action
        private static string GetEntityNameFromAction(KeyValuePair<string, ActionModel> action)
        {
            return action.Value.Inputs.Parameters.ContainsKey("entityName") ?
                action.Value.Inputs.Parameters["entityName"].ToString() :
                "";
        }

        // Check if Flow Auditing Attribute of Dataverse Create/Update actions is set with the current flow name
        private static bool IsLastUpdatedByProcessFieldPresent(KeyValuePair<string, ActionModel> action, string flowName)
        {
            return action.Value.Inputs.Parameters.ContainsKey($"item/{ConfigManager.FlowAuditingAttribute}") && 
                action.Value.Inputs.Parameters[$"item/{ConfigManager.FlowAuditingAttribute}"].ToString() == flowName;
        }
    }
}
