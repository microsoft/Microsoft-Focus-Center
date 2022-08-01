using AnonymizationFunctionApp.Interfaces;
using AnonymizationFunctionApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AnonymizationFunctionApp
{
    public class BulkRequestManager : IBulkRequestManager
    {
        private readonly ILogger<BulkRequestManager> _log;
        private readonly int _parallelCrmConnectionsCount = AppSettings.CrmMaxParallelConnections;
        private readonly int _maxRequestsCountInBulk = AppSettings.CrmMaxRequestsCountInBulk;
        private bool _isDisposed;

        public string EntityName { get; set; }

        public ICrmConnectionManager ConnectionManager { get; }

        public BulkRequestManager(ICrmConnectionManager crmConnectionManager, ILogger<BulkRequestManager> log)
        {
            ConnectionManager = crmConnectionManager;
            _log = log;
        }

        /// <summary>
        /// Sends requests as a bulk to CRM
        /// </summary>
        /// <returns></returns>
        public async Task SendRequests(IEnumerable<OrganizationRequest> requests)
        {
            VerifyAvailability();

            //Change max connections from .NET to a remote service default: 2
            System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
            //Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4 
            System.Threading.ThreadPool.SetMinThreads(100, 100);
            //Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server 
            System.Net.ServicePointManager.Expect100Continue = false;
            //Can decrease overall transmission overhead but can cause delay in data packet arrival
            System.Net.ServicePointManager.UseNagleAlgorithm = false;

            var timeout = AppSettings.CrmConnectionTimeout;

            var svc = await ConnectionManager.GetClient(timeout);
            svc.EnableAffinityCookie = false;

            Parallel.ForEach(requests,
              new ParallelOptions { MaxDegreeOfParallelism = _parallelCrmConnectionsCount },
              () =>
              {
                  var org = svc.Clone();
                  org.EnableAffinityCookie = false;
                  return new
                  {
                      Service = org,
                      EMR = new ExecuteMultipleRequest
                      {
                          Requests = new OrganizationRequestCollection(),
                          Settings = new ExecuteMultipleSettings
                          {
                              ContinueOnError = true,
                              ReturnResponses = false
                          }
                      }
                  };
              },
              (request, loopState, index, threadLocalState) =>
              {
                  threadLocalState.EMR.Requests.Add(request);
                  if (threadLocalState.EMR.Requests.Count == _maxRequestsCountInBulk)
                  {
                        try
                        {
                            _log.LogInformation($"Trying to process {_maxRequestsCountInBulk} operation(s) in 1 bulk request for {this.EntityName}.");
                            var sw = Stopwatch.StartNew();

                            threadLocalState.Service.Execute(threadLocalState.EMR);

                            sw.Stop();
                            _log.LogInformation($"Operation processed successfully in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds):g} for {this.EntityName}.");
                        }
                        catch (Exception e)
                        {
                            _log.LogInformation(e.Message, e);
                        }

                        threadLocalState.EMR.Requests.Clear();
                  }
                  return threadLocalState;
              },
              (threadLocalState) =>
              {
                  if (threadLocalState.EMR.Requests.Count > 0)
                  {
                        try
                        {
                            _log.LogInformation($"Trying to process {_maxRequestsCountInBulk} operation(s) in 1 bulk request for {this.EntityName}.");
                            var sw = Stopwatch.StartNew();

                            threadLocalState.Service.Execute(threadLocalState.EMR);

                            sw.Stop();
                            _log.LogInformation($"Operation processed successfully in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds):g} for {this.EntityName}.");
                        }
                        catch (Exception e)
                        {
                            _log.LogInformation(e.Message, e);
                        }
                  }

                  threadLocalState.Service.Dispose();
              });
        }

        /// <summary>
        /// Dispose Connection method
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
        }

        /// <summary>
        /// To verify the Connection Availiblity
        /// </summary>
        private void VerifyAvailability()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().Name);
        }
    }
}
