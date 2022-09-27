// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FocusCenterPRChecker.ConnectionReferenceTool.Models;
using FocusCenterPRChecker.DuplicatesComponentTool.Models;
using FocusCenterPRChecker.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FocusCenterPRChecker.ConnectionReferenceTool
{
    public static class Solution
    {
        static ConnectionReferences _connectionReferences;

        // For distinct changed solution, get missing connection references
        static public string ProcessSolutionToFindMissingCR(List<ComponentModel> changedComponents)
        {
            GetConnectionReferences();
            var solutionsOfChanges = changedComponents.Where(changedComponent => changedComponent.SolutionName != null).Select(changedComponent => $"{changedComponent.SolutionName}").Distinct().ToList();

            var report = ConnectionReferencesReportGenerator.GenerateLogForMissingCR(_connectionReferences.GetMissingConnectionReferences(solutionsOfChanges));

            return report;
        }

        // For distinct changed solution, get duplicate connection references
        static public string ProcessSolutionToFindDuplicatesCR(List<ComponentModel> changedComponents)
        {
            if (_connectionReferences == null)
            {
                GetConnectionReferences();
            }

            var solutionsOfChanges = changedComponents.Where(changedComponent => changedComponent.SolutionName != null).Select(changedComponent => $"{changedComponent.SolutionName}").Distinct().ToList();

            var report = ConnectionReferencesReportGenerator.GenerateLogForDuplicateCR(_connectionReferences.GetDuplicateConnectionReferences(solutionsOfChanges));

            return report;
        }

        // For distinct changed solution, check if solution pblisher prefix, unique name and publisher option value prefix is with in defined list in variable
        public static string CheckSolutionsPublisher(List<ComponentModel> changedComponents)
        {
            var solutionsOfChanges = changedComponents.Where(changedComponent => changedComponent.SolutionName != null).Select(changedComponent => $"{ConfigManager.PathToRepository}\\{changedComponent.SolutionName}").Distinct().ToList();
            HashSet<string> solutions = new HashSet<string>();
            var commentText = $"Please ensure publisher values are set as follows for ";
            var publisherValues = $"CustomizationPrefix = {ConfigManager.SolutionPublisherPrefix}<br/>" +
                                  $"UniqueName = {ConfigManager.SolutionPublisherUniqueName}<br/>" +
                                  $"CustomizationOptionValuePrefix = {ConfigManager.SolutionPublisherOptionValuePrefix}";

            foreach (var solutionPath in solutionsOfChanges)
            {
                if (!IsCorrectSolutionPublisher(solutionPath))
                {
                    solutions.Add(solutionPath.Split('\\').LastOrDefault());
                }
            }

            if (solutions.Count == 0)
            {
                Console.WriteLine("\tPublisher for solutions is correct");
            }

            var content = $"{commentText}{string.Join(", ", solutions)} solution{(solutions.Count > 1 ? "s" : "")}:<br/>{publisherValues}";

            return solutions.Count > 0 ? content : "";
        }

        //Checks for correct Solution publisher, prefix and option value prefix
        private static bool IsCorrectSolutionPublisher(string solutionPath)
        {
            XElement solutionFile = XElement.Load($"{solutionPath}{ConfigManager.SolutionFilesPath}\\Other\\Solution.xml");
            var publisher = solutionFile.Descendants("Publisher").FirstOrDefault();

            if (publisher == null)
                return false;

            var uniqueName = publisher.Descendants("UniqueName")?.FirstOrDefault().Value;
            var prefix = publisher.Descendants("CustomizationPrefix")?.FirstOrDefault().Value;
            var optionValuePrefix = publisher.Descendants("CustomizationOptionValuePrefix")?.FirstOrDefault().Value;

            if (ConfigManager.SolutionPublisherUniqueName.ToLower().Split(',').Contains(uniqueName.ToLower())
               && ConfigManager.SolutionPublisherPrefix.ToLower().Split(',').Contains(prefix.ToLower())
               && ConfigManager.SolutionPublisherOptionValuePrefix.ToLower().Split(',').Contains(optionValuePrefix.ToLower()))
            {
                return true;
            }

            return false;
        }

        // Extract solution name from the path
        public static string GetSolutionName(string path)
        {
            Regex regex = new Regex($@"^\/{ConfigManager.PathToRepository.Split('\\').LastOrDefault()}[^\/]*\/([^\/]*)");
            if (regex.IsMatch(path))
            {
                return regex.Match(path).Groups[1].Value;
            }

            return null;
        }

        // Check if solution.xml file have the correct version
        public static async Task<string> CheckSolutionsVersion(List<ComponentModel> changedComponents)
        {
            var solutionsOfChanges = changedComponents.Where(changedComponent => changedComponent.SolutionName != null).Select(changedComponent => $"{ConfigManager.PathToRepository}\\{changedComponent.SolutionName}").Distinct().ToList();
            HashSet<string> solutions = new HashSet<string>();
            var commentText = $"Please update the solution.xml file to have the correct version for the solution(s): ";

            foreach (var solutionPath in solutionsOfChanges)
            {
                XElement solutionFile = XElement.Load($"{solutionPath}{ConfigManager.SolutionFilesPath}\\Other\\Solution.xml");
                var newVersionString = solutionFile.Descendants("Version").FirstOrDefault().Value;

                string masterSolutionUrl = $"{ConfigManager.AzureDevOpsRepositoryUrl}/items?path={ConfigManager.SolutionsRepoPath}{solutionPath.Split('\\').LastOrDefault()}{ConfigManager.SolutionFilesPath.Replace("\\", "/")}/Other/Solution.xml&api-version=6.0";

                string masterSolutionFileContent = await HttpManager.SendGetRequest(masterSolutionUrl, "text/plain");

                if (masterSolutionFileContent == null)
                {
                    continue;
                }

                var oldVersionString = XDocument.Parse(masterSolutionFileContent).Root.Descendants("Version").FirstOrDefault().Value;

                var newVersion = Version.Parse(newVersionString);
                var oldVersion = Version.Parse(oldVersionString);

                var isCorrectVersion = oldVersion.CompareTo(newVersion) < 0;

                if (!isCorrectVersion)
                {
                    solutions.Add(solutionPath.Split('\\').LastOrDefault());
                }
            }

            if (solutions.Count == 0)
            {
                Console.WriteLine("\tVersion for solutions is correct");
            }

            var content = $"{commentText}{string.Join(", ", solutions)}";

            return solutions.Count > 0 ? content : "";
        }

        private static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
        {
            return date.AddDays((dayOfWeek <= date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
        }

        // Add conenction references to its instance
        private static void GetConnectionReferences()
        {
            _connectionReferences = new ConnectionReferences();

            foreach (var solution in Directory.GetDirectories(ConfigManager.PathToRepository).ToList())
            {
                if (Directory.Exists($"{solution}{ConfigManager.SolutionFilesPath}\\Workflows"))
                {
                    Flow.RetrieveConnectionReferences(_connectionReferences, $"{solution}");
                }
            }

            foreach (var solution in Directory.GetDirectories(ConfigManager.PathToRepository).ToList())
            {
                XElement solutionFile = XElement.Load($"{solution}{ConfigManager.SolutionFilesPath}\\Other\\Customizations.xml");

                _connectionReferences.CheckConnectionReferencesInSolution(solution.Split('\\').LastOrDefault(), solutionFile.Descendants("connectionreference").ToList());
            }
        }
    }
}
