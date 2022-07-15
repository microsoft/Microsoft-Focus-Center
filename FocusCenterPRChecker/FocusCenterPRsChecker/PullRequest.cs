using FocusCenterPRsChecker.DuplicatesComponentTool;
using FocusCenterPRsChecker.Managers;
using FocusCenterPRsChecker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FocusCenterPRsChecker.CheckFieldInFlows;
using FocusCenterPRsChecker.CheckPermissionsOfRole;
using FocusCenterPRsChecker.ConnectionReferenceTool;
using FocusCenterPRsChecker.DuplicatesComponentTool.Models;
using System.Text.RegularExpressions;

namespace FocusCenterPRsChecker
{
    public static class PullRequest
    {
        private static int _pullRequestId;
        private static List<string> _commentsToPost = new List<string>();

        public static List<string> CommentsToPost
        {
            get => _commentsToPost;
        }

        public static async Task ProcessPRAsync(string pullRequestIdInput, string personalaccesstoken)
        {
            if (int.TryParse(pullRequestIdInput, out _pullRequestId))
            {
                HttpManager.SetAuthHeader(personalaccesstoken);

                Console.WriteLine($"Getting changes for PR: {_pullRequestId}");

                var changesComponent = await GetChangesOfPRAsync();

                // Points out if any component is duplicate
                if (ConfigManager.CheckDuplicateComponents)
                {
                    Console.WriteLine("Searching duplicates for changed components");

                    var duplicatesResult = await DuplicateComponents.FindDuplicateComponents(changesComponent);

                    Console.WriteLine($"Duplicates Comment: {(String.IsNullOrEmpty(duplicatesResult) ? "No Comment(s)" : duplicatesResult)}");

                    CommentsToPost.Add(duplicatesResult);
                }

                // Points out if any connection reference is missing
                if (ConfigManager.CheckMissingConnectionReferences)
                {
                    Console.WriteLine("Checking missing connection references");

                    var missingConnectionReferences = Solution.ProcessSolutionToFindMissingCR(changesComponent);
                    
                    Console.WriteLine($"MissingConnectionReferences Comment: {(String.IsNullOrEmpty(missingConnectionReferences) ? "No Comment(s)" : missingConnectionReferences)}");

                    CommentsToPost.Add(missingConnectionReferences);
                }

                // Points out if any connection reference is duplicate
                if (ConfigManager.CheckDuplicateConnectionReferences)
                {
                    Console.WriteLine("Searching duplicates of connection references");

                    var duplicatesConnectionReferences = Solution.ProcessSolutionToFindDuplicatesCR(changesComponent);

                    Console.WriteLine($"DuplicatesConnectionReferences Comment: {(String.IsNullOrEmpty(duplicatesConnectionReferences) ? "No Comment(s)" : duplicatesConnectionReferences)}");

                    CommentsToPost.Add(duplicatesConnectionReferences);
                }

                // It shows table of changed security roles. Create, Read, Write, Delete priveleges are shown as column.
                if (ConfigManager.CheckSecurityRoles)
                {
                    Console.WriteLine("Checking privileges for Security Roles");

                    await RolesPermissionsTool.CheckPermissionsOfRoles(changesComponent);
                }

                // Points out if publisher is not from specified list
                if (ConfigManager.CheckPublisher)
                {

                    Console.WriteLine("Checking publisher of solutions");

                    var solutionsResult = Solution.CheckSolutionsPublisher(changesComponent);

                    Console.WriteLine($"CheckSolutionsPublisher Comment: {(String.IsNullOrEmpty(solutionsResult) ? "No Comment(s)" : solutionsResult)}");

                    CommentsToPost.Add(solutionsResult);
                }

                // Points out if solution version is no matching current solution version
                if (ConfigManager.CheckSolutionVersion)
                {
                    Console.WriteLine("Checking version of solutions");

                    var solutionsVersionResult = await Solution.CheckSolutionsVersion(changesComponent);

                    Console.WriteLine($"CheckSolutionsVersion Comment: {(String.IsNullOrEmpty(solutionsVersionResult) ? "No Comment(s)" : solutionsVersionResult)}");

                    CommentsToPost.Add(solutionsVersionResult);
                }

                // Points out if any flow has not set last updated process field for auditing purposes
                if (ConfigManager.CheckLastUpdatedProcessField)
                {
                    Console.WriteLine($"Checking {ConfigManager.FlowAuditingAttribute} field is updated in flows");

                    var resultOfChecking = CheckLastUpdatedProceesFieldTool.ProcessFlows(changesComponent);

                    Console.WriteLine($"CheckLastUpdatedProceesField Comment: {(String.IsNullOrEmpty(resultOfChecking)?"No Comment(s)":resultOfChecking)}");

                    CommentsToPost.Add(resultOfChecking);
                }
                else
                {
                    Console.WriteLine("CheckLastUpdatedProcessField field is disabled");
                }

                // if pipeline variable "EnableCommentsToPullRequest" is set true then Fetch PR comments and add only unique comments to PR
                // otherwise, add comments in Console only
                if (ConfigManager.EnableCommentsToPullRequest)
                {
                    var existingComments = Regex.Unescape(JObject.Parse(await HttpManager.SendGetRequest($"{ConfigManager.AzureDevOpsRepositoryUrl}/pullRequests/{_pullRequestId}/threads?api-version=6.0")).ToString());
                    IList<string> result = new List<string>();
                    foreach (var comment in CommentsToPost)
                    {
                        if (!existingComments.Contains(comment))
                        {
                            string response = await PostResultsToPR(comment);
                            if(response != null)
                            {
                                result.Add(response);
                            }
                        }
                    }
                    if(result.Count > 0)
                    {
                        Console.WriteLine("Comment(s) added to PR.");
                    }
                }
                else
                {
                    Console.WriteLine("Checking EnableCommentsToPullRequest field is false. Comments will be added to Console only.");
                }
            }
            else
            {
                Console.WriteLine("Pull Request ID is empty");
            }
        }

        // Post comments to PR
        public static async Task<string> PostResultsToPR(string content)
        {
            if(!string.IsNullOrEmpty(content))
            {
                CommentJsonModel resultJson = new CommentJsonModel()
                {
                    Comments = new Comment[]
                    {
                    new Comment()
                    {
                        Content = content,
                        ParentCommentId = 0,
                        CommentType = 1
                    }
                    },
                    Status = 1
                };

                var table = JsonConvert.SerializeObject(resultJson);

                var data = new StringContent(table, Encoding.UTF8, "application/json");

                string url = $"{ConfigManager.AzureDevOpsRepositoryUrl}/pullrequests/{_pullRequestId}/threads?api-version=6.0";

                string result = await HttpManager.SendPostRequest(url, data);

                return result;
            }
            else
            {
                Console.WriteLine("Content is empty");
                return String.Empty;
            }
        }

        //Based on last commit in given PullRequest Id, fetch changes with object type "blob" and change type "add, edit" etc except "delete"
        private static async Task<List<ComponentModel>> GetChangesOfPRAsync()
        {
            var commitId = JObject.Parse(await HttpManager.SendGetRequest($"{ConfigManager.AzureDevOpsRepositoryUrl}/pullRequests/{_pullRequestId}")).SelectToken("lastMergeCommit").SelectToken("commitId");
            
            Console.WriteLine($"Last merge commit: {commitId}");

            var changesJson = JsonConvert.DeserializeObject<ChangesJsonModel>(await HttpManager.SendGetRequest($"{ConfigManager.AzureDevOpsRepositoryUrl}/commits/{commitId}/changes"));

            var fileNames = changesJson.Changes
                .Where(i => i.Item.GitObjectType == "blob" && !i.ChangeType.Contains("delete"))
                .Select(i => new ComponentModel()
                {
                    ComponentName = i.Item.Path.Split('/').LastOrDefault(),
                    Path = String.Join("/", i.Item.Path.Split('/').Where(x => x != i.Item.Path.Split('/')[1]).ToArray()),
                    SolutionName = Solution.GetSolutionName(i.Item.Path),
                    ComponentFolder = DuplicateComponents.GetComponentFolder(i.Item.Path),
                    IsNewComponent = i.ChangeType.ToLower().Contains("add")
                }).ToList();

            Console.WriteLine($"Count of Changes: {fileNames.Count}");

            return fileNames;
        }
    }
}
