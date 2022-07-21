// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FocusCenterPRChecker.ConnectionReferenceTool.Models;
using Newtonsoft.Json.Linq;

namespace FocusCenterPRChecker.ConnectionReferenceTool
{
    // Get flows and parse them
    public static class Flow
    {
        static string _solutionPath;

        // Extract connection references from workflows folder
        public static void RetrieveConnectionReferences(ConnectionReferences connectionReferences, string folderPath)
        {
            _solutionPath = folderPath;

            List<string> workflows = Directory.GetFiles($"{folderPath}\\Workflows").ToList();

            List<FlowModel> flows = GetFlows(workflows);

            GetConnectionReferences(connectionReferences,flows);
        }

        // Extract the json file name of flows and create flow model with flow name and flow path
        private static List<FlowModel> GetFlows(List<string> workflows)
        {
            List<FlowModel> flows = new List<FlowModel>();

            foreach (var file in workflows.Where(w => w.Contains(".data")).ToList())
            {
                XElement worflowDataFile = XElement.Load($"{file}");
                var category = (WorkflowCategory)int.Parse(worflowDataFile.Descendants("Category").FirstOrDefault().Value);

                if (category == WorkflowCategory.ModernFlow)
                {
                    var jsonFileName = worflowDataFile.Descendants("JsonFileName").FirstOrDefault().Value.Split('/').LastOrDefault();

                    flows.Add(new FlowModel { FlowName = worflowDataFile.Attribute("Name").Value, FlowPath = $"{_solutionPath}\\Workflows\\{jsonFileName}" });
                }
            }

            return flows;
        }

        // Read flow json file and add the flow in usage of that connection refrences
        private static void GetConnectionReferences(ConnectionReferences connectionReferences,List<FlowModel> flows)
        {
            foreach (var flow in flows)
            {
                using (StreamReader r = new StreamReader(flow.FlowPath))
                {
                    var json = r.ReadToEnd();
                    var jobj = JObject.Parse(json);

                    foreach(var connectionReference in jobj.SelectToken("properties").SelectToken("connectionReferences").ToObject<JObject>())
                    {
                        var connectionReferenceLogicalName = connectionReference.Value.SelectToken("connection").SelectToken("connectionReferenceLogicalName");

                        if(connectionReferenceLogicalName != null)
                        {
                            connectionReferences.AddUsage(connectionReferenceLogicalName.Value<string>(), flow.FlowName, _solutionPath.Split('\\').LastOrDefault());
                        }
                            
                    }
                }
            }

        }
    }
}
