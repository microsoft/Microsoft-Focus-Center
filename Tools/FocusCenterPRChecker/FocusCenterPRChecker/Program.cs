// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.using System;

using System;
using System.Threading.Tasks;
using FocusCenterPRChecker.Managers;

namespace FocusCenterPRChecker
{
    class Program
    {
        static int Main(string[] args)
        {
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
