// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using FocusCenterPRChecker.Managers;

namespace FocusCenterPRChecker
{
    class Program
    {
        static int Main(string[] args)
        {
            Environment.SetEnvironmentVariable("BUILD_REPOSITORY_NAME", "DynamicsSource", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_TEAMPROJECT", "DynamicsSource", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_TEAMPROJECTID", "21580fff-a788-4d71-a2ba-ed9c81de25c0", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("BUILD_REPOSITORY_ID", "c4e218b0-091f-4e8f-897b-273e68345cab", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_COLLECTIONURI", "https://dev.azure.com/prchecker/", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI", "https://dev.azure.com/prchecker/DynamicsSource/_git/DynamicsSource", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH", "refs/heads/main", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checklastupdatedprocessfield", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionpublisheruniquename", "MicrosoftSuccessHub", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionpublisherprefix", "mssh", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionpublisheroptionvalueprefix", "28041", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("flowauditingattribute", "mssh_lastupdatedbyprocess", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("exclusionlistpath", @"DuplicatesComponentTool\ExclusionList.xml", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkduplicatecomponents", "false", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkduplicateconnectionreferences", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkmissingconnectionreferences", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkpublisher", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checksecurityroles", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checksolutionversion", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("enablecommentstopullrequest", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkplugincomponents", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkentitylevelbusinessrules", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkwebresourcesfiletypes", "js,html,css", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checksolutiondependencies", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("enablecommentstopullrequest", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("masterbranchname", "master", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkschemachanges", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("teamname", "EXP", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionfilespath", @"", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionsrepopath", "/Solutions/", EnvironmentVariableTarget.Process);

            Task<int> t = Task.Run(async () =>
            {
                Console.WriteLine($"Repository Name: {ConfigManager.RepositoryName}");
                ConfigManager.PathToRepository = args[0];
                Console.WriteLine($"Repository Path: {ConfigManager.PathToRepository}");

                var pat = args[1];

                var pullRequestId = args.Length >= 3 ? args[2] : string.Empty;

                try
                {
                    await PullRequest.ProcessPRAsync(pullRequestId, pat);

                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}\n{e.StackTrace}");
                    return 1;
                }
                
            });

            return t.Result;
        }
    }
}
