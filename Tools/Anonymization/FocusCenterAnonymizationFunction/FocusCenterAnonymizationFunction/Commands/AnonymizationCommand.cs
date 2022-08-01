using AnonymizationFunctionApp.Interfaces;
using AnonymizationFunctionApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace AnonymizationFunctionApp.Commands
{
    internal class AnonymizationCommand : IAnonymizationCommand
    {
        private InputRequestModel _inputRequestModel;
        private object lockObj = new object();
        private readonly ILogger<AnonymizationCommand> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Parameterized constructor to Initialize service Scope Factory Object.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serviceScopeFactory"></param>
        public AnonymizationCommand(ILogger<AnonymizationCommand> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Main Execute Methed Which is called from Entry point Function.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CommandResult> Execute(InputRequestModel input)
        {
            try
            {
                _inputRequestModel = input;
                await BulkAnonymizationProcessAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Function failed with a following error message:\n{e.Message}");
                return new CommandResult(CommandStatusCode.Failed);

            }
            return new CommandResult(CommandStatusCode.Succeeded);
        }

        /// <summary>
        /// Bulk Anonymization Method called from Execute
        /// </summary>
        /// <returns></returns>
        private async Task BulkAnonymizationProcessAsync()
            {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            try
            {
                long totalRecordCount = 0;

                List<Task<long>> processEntityQuery = new List<Task<long>>();
                foreach (var entity in _inputRequestModel.Entities)
                {
                    processEntityQuery.Add(Task.Run(() => ProcessSingleEntityAsync(entity)));

                    var result = await Task.WhenAll(processEntityQuery);
                    foreach (var item in result)
                    {
                        totalRecordCount += item;
                    }

                    _logger.LogInformation($"Anonymization of {totalRecordCount} records finished after {timer.ElapsedMilliseconds / 1000} seconds");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} and {ex.InnerException.Message}");
                _logger.LogInformation(ex.StackTrace);
            }

            timer.Stop();

            _logger.Flush();
        }

        /// <summary>
        /// Method For processing Anonymization for single Entity 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task<long> ProcessSingleEntityAsync(InputEntityData entity)
        {
            Stopwatch timer = new Stopwatch();
            var pageNumber = 1;
            long recordCount = 0;
            var timeout = AppSettings.CrmConnectionTimeout;
            var moreRecords = true;
            TimeSpan currentElapsed = new TimeSpan(0);
            StringBuilder fetchingAttributes = new StringBuilder();

            timer.Start();

            _logger.LogInformation($"Starting anonymization of {entity.EntityName} for {string.Join(", ", entity.Attributes.Select(a => a.Name))} attributes \n");
            _logger.LogInformation($"Receiving records of {entity.EntityName} entity\n");

            try
            {
                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    foreach (var attribute in entity.Attributes)
                    {
                        var fetchAttribute = $@"<attribute name='{ attribute.Name}' />";
                        fetchingAttributes.Append(fetchAttribute);
                    }

                    var bulkRequestManager = scope.ServiceProvider.GetService<IBulkRequestManager>();
                    bulkRequestManager.EntityName = entity.EntityName;
                    ICrmConnectionManager crmConnectionManager = bulkRequestManager.ConnectionManager;
                    crmConnectionManager.EntityName = entity.EntityName;
                    var entities = new List<Entity>();

                    while (moreRecords)
                    {
                        var query = $@"
                            <fetch page='{pageNumber}' >
                                <entity name='{entity.EntityName}'>
                                    <attribute name='{entity.EntityIdName}' />
                                    { entity.Filter}
                                </entity>
                            </fetch>";

                        var client = await crmConnectionManager.GetClient(timeout);

                        if (client is null)
                        {
                            _logger.LogError($"Error: {client.LastError}");
                            _logger.LogError($"Exception: {client.LastException}");
                            continue;
                        }

                        var recordPortion = client.RetrieveMultiple(new FetchExpression(query));

                        moreRecords = recordPortion.MoreRecords;

                        _logger.LogInformation($"======>Recieved {recordPortion.Entities.Count} records entity {crmConnectionManager.EntityName} after {new TimeSpan(timer.Elapsed.Ticks - currentElapsed.Ticks)}");

                        entities.AddRange(recordPortion.Entities);

                        _logger.LogInformation($"Total number of records: {entities.Count}");

                        currentElapsed = timer.Elapsed;

                        recordCount += recordPortion.Entities.Count;

                        pageNumber++;
                    }
                    List<UpdateRequest> updateRequests = new List<UpdateRequest>();

                    for (int i = 0; i < entities.Count; i++)
                    {
                        updateRequests.Add(GetUpdateRequest(entities[i], entity.Attributes, recordCount + (i + 1)));
                    }

                    await bulkRequestManager.SendRequests(updateRequests);
                }

                timer.Stop();

                _logger.LogInformation($"Anonymization of {recordCount} records of {entity.EntityName} took {timer.Elapsed}\n Avarage speed was: { recordCount / GetDaysFromStopwatch(timer)} records/day");
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    _logger.LogError($"{e.Message} and {e.InnerException.Message}");
                }
                else
                {
                    _logger.LogError($"{e.Message}");
                }
                _logger.LogInformation(e.StackTrace);
            }
            return recordCount;
        }

        /// <summary>
        /// Method for Anonymization for String type Records with Adding Counter Suffix.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attributes"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        private static UpdateRequest GetUpdateRequest(Entity entity, Attributes[] attributes, long counter)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.Counter)
                {
                    entity.Attributes[$"{attribute.Name}"] = attribute.Value + counter;
                }
                else
                {
                    entity.Attributes[$"{attribute.Name}"] = attribute.Value;
                }

            }

            return new UpdateRequest
            {
                Target = entity
            };
        }

        /// <summary>
        /// Get Total Spend Hours from Stop watch
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <returns></returns>
        private double GetHoursFromStopwatch(Stopwatch stopwatch) => stopwatch.ElapsedMilliseconds == 0 ? 1 : (double)stopwatch.ElapsedMilliseconds / (1000 * 60 * 60);

        /// <summary>
        /// Get total Days Spend in Process 
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <returns></returns>
        private double GetDaysFromStopwatch(Stopwatch stopwatch) => stopwatch.ElapsedMilliseconds == 0 ? 1 : (double)GetHoursFromStopwatch(stopwatch) / 24;
    }
}
