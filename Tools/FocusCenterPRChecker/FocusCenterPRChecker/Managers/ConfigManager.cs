using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace FocusCenterPRChecker.Managers
{
    public static class ConfigManager
    {
        public static string PathToRepository { get; set; }

        //PIPELINE VARIABLES
        public static bool CheckLastUpdatedProcessField => bool.Parse(GetVariableValue("checklastupdatedprocessfield"));
        public static string SolutionPublisherUniqueName => GetVariableValue("solutionpublisheruniquename");
        public static string SolutionPublisherPrefix => GetVariableValue("solutionpublisherprefix");
        public static string SolutionPublisherOptionValuePrefix => GetVariableValue("solutionpublisheroptionvalueprefix");
        public static string ExclusionListPath => GetVariableValue("exclusionlistpath");
        //Ex. DuplicatesComponentTool\ExclusionList.xml
        public static string FlowAuditingAttribute => GetVariableValue("flowauditingattribute");
        public static bool CheckDuplicateComponents => bool.Parse(GetVariableValue("checkduplicatecomponents"));
        public static bool CheckMissingConnectionReferences => bool.Parse(GetVariableValue("checkmissingconnectionreferences"));
        public static bool CheckDuplicateConnectionReferences => bool.Parse(GetVariableValue("checkduplicateconnectionreferences"));
        public static bool CheckSecurityRoles => bool.Parse(GetVariableValue("checksecurityroles"));
        public static bool CheckPublisher => bool.Parse(GetVariableValue("checkpublisher"));
        public static bool CheckSolutionVersion => bool.Parse(GetVariableValue("checksolutionversion"));
        public static bool EnableCommentsToPullRequest => bool.Parse(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("enablecommentstopullrequest", EnvironmentVariableTarget.Process)) ? Environment.GetEnvironmentVariable("enablecommentstopullrequest", EnvironmentVariableTarget.Process) : "false");
        //Set to default if null

        //SYSTEM DEFINED ENVIRONMENT VARIABLES
        public static string RepositoryName => Environment.GetEnvironmentVariable("BUILD_REPOSITORY_NAME", EnvironmentVariableTarget.Process);
        public static string ProjectName => Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT", EnvironmentVariableTarget.Process);
        public static string ProjectId => Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECTID", EnvironmentVariableTarget.Process);
        // Not available in TFS 2015 | link: https://docs.microsoft.com/en-us/azure/devops/pipelines/release/variables?view=azure-devops&tabs=batch#system
        public static string RepositoryId => Environment.GetEnvironmentVariable("BUILD_REPOSITORY_ID", EnvironmentVariableTarget.Process);
        public static string OrganizationURI => Environment.GetEnvironmentVariable("SYSTEM_COLLECTIONURI", EnvironmentVariableTarget.Process);
        //https://dev.azure.com/test292956/
        public static string OrganizationName => $"{OrganizationURI.Split('/').Reverse().Skip(1).FirstOrDefault()}";
        //test292956
        public static string PullRequestRepoURL => Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI", EnvironmentVariableTarget.Process);
        //https://test292956@dev.azure.com/test292956/demo/_git/demo
        public static string BranchName => Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH", EnvironmentVariableTarget.Process);
        // "refs/heads/master"
        public static string AzureDevOpsOrganizationUrl => $"{OrganizationURI}{ProjectId}/";
        public static string AzureDevOpsRepositoryUrl => $"{OrganizationURI}{ProjectName}/_apis/git/repositories/{RepositoryId}";
        //https://docs.microsoft.com/en-us/rest/api/azure/devops/git/repositories/get-repository?view=azure-devops-rest-7.1
        public static string AzureDevOpsSearchUrl => $"https://almsearch.dev.azure.com/{OrganizationName}/_apis/search/codesearchresults?api-version=7.1-preview.1";
        public static string SolutionsRepoPath => GetVariableValue("solutionsrepopath");

        //If pipeline variables are not set, show error to configure them first
        private static string GetVariableValue(string variableName) 
        {
            string variableValue = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(variableValue))
            {
                throw new ArgumentNullException(variableName, $"Variable {variableName} doesn't have the value configured. Please refer to the document for further details.");
            }

            return variableValue;
        }
    }
}