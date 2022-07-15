using FocusCenterPRChecker.Managers;
using FocusCenterPRChecker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FocusCenterPRChecker.ConnectionReferenceTool;
using FocusCenterPRChecker.DuplicatesComponentTool.Enums;
using FocusCenterPRChecker.DuplicatesComponentTool.Models;

namespace FocusCenterPRChecker.DuplicatesComponentTool
{
    public static class DuplicateComponents
    {
        // For distinct changed solution, get duplicate components
        public async static Task<string> FindDuplicateComponents(List<ComponentModel> changesComponent)
        {
            var componentsForSearch = GetComponentsForSearch(changesComponent);

            var duplicatesCollection = await SearchForDuplicates(componentsForSearch);

            var contentTable = GetContentTable(duplicatesCollection);

            return contentTable;
        }

        // Get unique components not in exclusion list
        private static List<ComponentModel> GetComponentsForSearch(List<ComponentModel> changedComponents)
        {
            ExclusionList exlusionList = new ExclusionList(ConfigManager.ExclusionListPath);

            changedComponents.ForEach(component => component.ComponentName = SetCorrectNameForComponents(component));

            var distinctComponentModels = changedComponents.GroupBy(c => c.ComponentName).Select(c => c.First()).ToList();

            return distinctComponentModels.Where(
                                                    changedComponent => !exlusionList.ContainsFolder(changedComponent.Path)
                                                    && !exlusionList.ContainsFileExtension(Path.GetExtension(changedComponent.ComponentName))
                                                    && !exlusionList.ContainsComponentName(changedComponent.ComponentName)).ToList();
        }

        // Post request to Azure DevOps Search api to get code result containing information of the searched files and its metadata
        private static async Task<SearchResponseModel> SearchADO(string searchString)
        {
            var json = $@"{{
                              'searchText': '{searchString}',
                              '$skip': 0,
                              '$top': 10,
                              'filters': {{
                                        'Project': ['{ConfigManager.ProjectName}'],                     
                                        'Repository': ['{ConfigManager.RepositoryName}'],
                                        'Path': ['{ConfigManager.PathToRepository.Split('\\').LastOrDefault()}'],
                                        'Branch': ['{ConfigManager.BranchName.Split('/').LastOrDefault()}']
                                        }},
                              '$orderBy': [
                                            {{
                                              'field': 'filename',
                                              'sortOrder': 'ASC'
                                            }}
                                          ],
                              'includeFacets': true
                            }}";

            var data = new StringContent(json, Encoding.UTF8, "application/json");
            Console.WriteLine($"Search URL: {ConfigManager.AzureDevOpsSearchUrl} \n Search Json: {json}");

            var res = await HttpManager.SendPostRequest(ConfigManager.AzureDevOpsSearchUrl, data);

            return JsonConvert.DeserializeObject<SearchResponseModel>(res);
        }

        // set correct name for components
        private static string SetCorrectNameForComponents(ComponentModel componentModel)
        {
            if (componentModel.ComponentFolder != ComponentFolder.AppModules.ToString() &&
                componentModel.ComponentFolder != ComponentFolder.AppModuleSiteMaps.ToString() &&
                componentModel.ComponentFolder != ComponentFolder.Controls.ToString()) 

                return componentModel.ComponentName;

            var pathSplit = componentModel.Path.Split('/');

            return pathSplit[pathSplit.Length - 2];


        }

        // Get component folder from the path
        public static string GetComponentFolder(string path)
        {
            Regex regex = new Regex($@"^\/{ConfigManager.PathToRepository.Split('\\').LastOrDefault()}[^\/]*\/[^\/]*\/(?<type>[^\/]*)");

            if (!regex.IsMatch(path)) return "";

            var type = regex.Match(path).Groups["type"].Value;

            if (type == "Entities")
            {
                type = ParseEntitiesPath(path);
            }

            var isDefinedInEnum = Enum.IsDefined(typeof(ComponentFolder), type);

            return isDefinedInEnum ? Enum.Parse(typeof(ComponentFolder), type).ToString() : "";
        }

        // Get extity.xml component folder from given path
        private static string ParseEntitiesPath(string path)
        {
            Regex regex = new Regex($@"^\/{ConfigManager.PathToRepository.Split('\\').LastOrDefault()}[^\/]*\/[^\/]*\/[^\/]*\/[^\/]*\/(?<type>[^\/]*)");

            if (!regex.IsMatch(path)) return "";

            return path.EndsWith("Entity.xml") ? ComponentFolder.Entity.ToString() : regex.Match(path).Groups["type"].Value;
        }

        // For distinct changed solution, get duplicate components
        private static async Task<Dictionary<string, string>> SearchForDuplicates(List<ComponentModel> componentsForSearch)
        {
            Dictionary<string, string> duplicatesDictionary = new Dictionary<string, string>();

            foreach (var component in componentsForSearch)
            {
                var searchResult = await SearchADO(component.ComponentName);

                if (searchResult.Count > 0)
                {
                    HashSet<string> solutions = new HashSet<string>();

                    var anotherSolutions = searchResult.Results
                        .Where(foundComponent => GetComponentFolder(foundComponent.Path) == component.ComponentFolder
                               && foundComponent.FileName == component.ComponentName)
                        .Select(f => Solution.GetSolutionName(f.Path))
                        .Where(r => r != component.SolutionName).ToList();

                    if (anotherSolutions.Count > 0)
                    {
                        solutions.Add(component.SolutionName);
                        solutions.UnionWith(anotherSolutions);

                        if (!duplicatesDictionary.ContainsKey(component.ComponentName))
                        {
                            duplicatesDictionary.Add(component.ComponentName, string.Join(", ", solutions));
                        }
                    }
                }
            }
            
            return duplicatesDictionary;
        }

        // Create HTML table for duplicate components
        private static string GetContentTable(Dictionary<string, string> duplicatesResult) 
        {
            var tbody = string.Concat(duplicatesResult.Select(r => $"<tr><td>{r.Key}</td><td>{r.Value}</td></tr>"));
            var commentText = "The below components are part of multiple solutions. Please review them and ensure that each component is part of a single solution.";

            return string.IsNullOrEmpty(tbody) ? "" : $"{commentText}<table style='width:{TableStyleSettings.TableWidthSetting}'><tr><th>Component</th><th>Solution</th></tr>{tbody}</table>";
        }
    }
}
