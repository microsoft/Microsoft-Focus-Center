// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.using System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusCenterPRChecker.Managers;

namespace FocusCenterPRChecker.ConnectionReferenceTool
{
    public static class ConnectionReferencesReportGenerator
    {
        public static string GenerateLogForMissingCR(List<KeyValuePair<string, string>> missingConnectionReferences)
        {
            GenerateLogForMissingCRConsole(missingConnectionReferences);

            return GetContentTableForMissingConnectionReferences(missingConnectionReferences);
        }

        // Create HTML table for missing connection references
        private static string GetContentTableForMissingConnectionReferences(List<KeyValuePair<string, string>> connectionReferences)
        {
            var tbody = string.Concat(connectionReferences.Select(connectionReference => $"<tr><td>{connectionReference.Key}</td><td>{connectionReference.Value}</td></tr>"));
            var commentText = "Please review the below Connection References for potentials issues with Flows.";

            return string.IsNullOrEmpty(tbody) ? "" : $"{commentText}<table style='width:{TableStyleSettings.TableWidthSetting}'><tr><th>Missing Connection Reference</th><th>Used in flows</th></tr>{tbody}</table>";
        }

        // Add missing connection references to console log
        private static void GenerateLogForMissingCRConsole(List<KeyValuePair<string, string>> missingConnectionReferences)
        {
            if(missingConnectionReferences.Count > 0)  
                Console.WriteLine("\t##[error] Missing Connection References:");

            foreach (var connectionReference in missingConnectionReferences)
            { 
                Console.WriteLine($"\t {connectionReference.Key} connection reference which is used in flows: ");
                Console.WriteLine($"\t\t {connectionReference.Value}");
            }
        }

        // Add duplicate connection references to console log
        public static string GenerateLogForDuplicateCR(HashSet<KeyValuePair<string, ConnectionReference>> duplicatesConnectionReferences)
        {
            if(duplicatesConnectionReferences.Count > 0)
                Console.WriteLine("\t##[error] Duplicates Connection References:");

            var commentText = "The below Connection References are part of multiple solutions. Please review them and ensure that each component is part of a single solution.";

            StringBuilder tbody = new StringBuilder();

            foreach (var connectionReference in duplicatesConnectionReferences)
            {
                Console.WriteLine($"\t {connectionReference.Key} is used in several solutions:");
                Console.WriteLine($"\t\t {string.Join(",\n\t\t ", connectionReference.Value.Solutions.Keys)}");
                Console.WriteLine($"\n\t It is used in flows: \n\t\t{string.Join(",\n\t\t ", connectionReference.Value.GetFlowsFromSolutions())}");

                tbody.Append($"<tr><td>{connectionReference.Key}</td><td>{string.Join("<br/><br/>", connectionReference.Value.GetFlowsFromSolutions())}</td><td>{string.Join(" ", connectionReference.Value.Solutions.Keys)}</td></tr>");
            }

            return string.IsNullOrWhiteSpace(tbody.ToString())? "" : $"{commentText}<table style='width:{TableStyleSettings.TableWidthSetting}'><tr><th>Duplicate Connection Reference</th><th>Used in flows</th><th>Solutions</th></tr>{tbody}</table>";
        }
    }
}
