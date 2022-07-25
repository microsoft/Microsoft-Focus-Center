// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FocusCenterPRChecker.Managers
{
    public static class HttpManager
    {
        private static AuthenticationHeaderValue _authenticationHeader;

        public static void SetAuthHeader(string personalaccesstoken)
        {
            var token = Convert.ToBase64String(
                   System.Text.ASCIIEncoding.ASCII.GetBytes(
                    string.Format("{0}:{1}", "", personalaccesstoken)));

            _authenticationHeader = new AuthenticationHeaderValue("Basic", token);
        }
        public static async Task<string> SendGetRequest(string url, string contentType = "application/json")
        {
            Console.WriteLine($"Request Url: {url}");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigManager.AzureDevOpsOrganizationUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                client.DefaultRequestHeaders.Add("User-Agent", "ManagedClientConsoleAppSample");
                client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                client.DefaultRequestHeaders.Authorization = _authenticationHeader;

                // connect to the REST endpoint            
                HttpResponseMessage response = await client.GetAsync(url);

                // check to see if we have a succesfull response
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;

                    return result.ToString();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    Console.WriteLine("{0}:{1}", response.StatusCode, response.ReasonPhrase);
                }

                return null;
            }
        }

        public static async Task<string> SendPostRequest(string url, StringContent data)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigManager.AzureDevOpsOrganizationUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "ManagedClientConsoleAppSample");
                client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                client.DefaultRequestHeaders.Authorization = _authenticationHeader;

                // connect to the REST endpoint            
                HttpResponseMessage response = await client.PostAsync(url, data);

                // check to see if we have a succesfull response
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;

                    return result.ToString();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    Console.WriteLine("{0}:{1}", response.StatusCode, response.ReasonPhrase);
                }
                return null;
            }
        }
    }
}
