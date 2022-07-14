using System;
using System.Threading.Tasks;
using FocusCenterPRsChecker.Managers;

namespace FocusCenterPRsChecker
{
    class Program
    {
        static int Main(string[] args)
        {
            Environment.SetEnvironmentVariable("BUILD_REPOSITORY_NAME", "CXPDevops", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_TEAMPROJECT", "CXP", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_TEAMPROJECTID", "05a6f21b-d177-4392-9d65-65817dcbfaef", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("BUILD_REPOSITORY_ID", "1ca10e2e-edc5-40e4-a312-01e47e8514a7", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_COLLECTIONURI", "https://dev.azure.com/dynamicscrm/", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI", "https://dynamicscrm@dev.azure.com/dynamicscrm/CXP/_git/EXP/", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH", "refs/heads/master", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checklastupdatedprocessfield", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionpublisheruniquename", "MicrosoftSuccessHub", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionpublisherprefix", "mssh", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionpublisheroptionvalueprefix", "28041", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("flowauditingattribute", "mssh_LastUpdatedByProcess", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("exclusionlistpath", @"DuplicatesComponentTool\ExclusionList.xml", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkduplicatecomponents", "false", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkduplicateconnectionreferences", "false", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkmissingconnectionreferences", "false", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checkpublisher", "false", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checksecurityroles", "false", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("checksolutionversion", "true", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("enablecommentstopullrequest", "false", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("solutionsrepopath", "/Extracted/", EnvironmentVariableTarget.Process);

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
