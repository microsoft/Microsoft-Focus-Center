using AnonymizationFunctionApp.Interfaces;
using AnonymizationFunctionApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Threading.Tasks;

namespace AnonymizationFunctionApp
{
    public class CrmConnectionManager : ICrmConnectionManager
    {
        private ServiceClient _crmServiceClient;
        private CrmCredentials _credentials;
        private readonly ILogger<CrmConnectionManager> _log;
        private readonly object _lock = new object();
        private bool _isInvalid, _disposed;

        public string EntityName { get; set; }

        public CrmConnectionManager(ILogger<CrmConnectionManager> log)
        {
            _log = log;
        }

        /// <summary>
        /// Asynchronous CRM Connection Get Method 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public async ValueTask<ServiceClient> GetClient(TimeSpan timeout, string host = "")
        {
            ServiceClient.MaxConnectionTimeout = new TimeSpan(Math.Max(ServiceClient.MaxConnectionTimeout.Ticks, timeout.Ticks));

            _log.LogInformation($"Getting Crm Service Client for {this.EntityName}");

            lock (_lock)
            {
                if (_crmServiceClient != null && VerifyIsAlive())
                {
                    return _crmServiceClient;
                }
            }

            var credentials = await RetrieveCredentials(host);

            lock (_lock)
            {
                if (_crmServiceClient != null && VerifyIsAlive())
                {
                    return _crmServiceClient;
                }

                _crmServiceClient = CreateCrmServiceClient(credentials);
                _isInvalid = false;

                return _crmServiceClient;
            }
        }

        /// <summary>
        /// To Retrieve the Crdentials from Azure for CRM Connection
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        private async ValueTask<CrmCredentials> RetrieveCredentials(string hostname)
        {
            var host = !string.IsNullOrEmpty(hostname) ? hostname : AppSettings.CrmUrl;

            if (!string.IsNullOrEmpty(hostname))
            {
                _credentials = null;
            }

            CrmCredentials credentials = _credentials;

            lock (_lock)
            {
                if (credentials != null)
                {
                    if (credentials.CreatedAt > DateTime.Now - TimeSpan.FromDays(1))
                        return credentials;
                }
            }

            _log.LogInformation($"Retrieving credentials for {host}.");
            string clientSecret;
            try
            {
                var pwdSecret = await KeyVaultManager.RetrieveSecret(AppSettings.CrmAppClientSecret);
                clientSecret = pwdSecret.Value;
            }
            catch (Exception ex)
            {
                _log.LogError($"clientSecret retrieve error :"+ex.Message, ex);
                throw;
            }

            credentials = new CrmCredentials
            {
                Host = host,
                ClientId = AppSettings.CrmAppClientId,
                ClientSecret = clientSecret
            };

            lock (_lock)
            {
                if (_credentials is null)
                {
                    _credentials = credentials;

                    _log.LogInformation($"Credentials for {host} retrieved.");
                }
            }

            return _credentials;
        }

        /// <summary>
        /// To Create the CRM Connection 
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private static ServiceClient CreateCrmServiceClient(CrmCredentials credentials)
        {
            var connection = $@"AuthType=ClientSecret;url={credentials.Host};ClientId={credentials.ClientId};ClientSecret={credentials.ClientSecret}";

            var crmServiceClient = new ServiceClient(connection);

            crmServiceClient.EnableAffinityCookie = false;

            return crmServiceClient;
        }

        private void VerifyAvailability()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// TO Verify the connection is available or not.
        /// </summary>
        /// <returns></returns>
        private bool VerifyIsAlive()
        {
            VerifyAvailability();

            if (_crmServiceClient == null && !_crmServiceClient.IsReady)
            {

                DisposeAndClearInstances();
                return false;

            }

            return true;
        }

        /// <summary>
        /// Renew CRM connection 
        /// </summary>
        public void Renew()
        {
            lock (_lock)
            {
                DisposeAndClearInstances();
            }
        }

        /// <summary>
        /// To Enable Invalidate to True and Clear Old Connection and instances
        /// </summary>
        public void Invalidate()
        {
            lock (_lock)
            {
                _isInvalid = true;
                DisposeAndClearInstances();
            }
        }

        /// <summary>
        /// Dispose Old CRM connection and clear the instances
        /// </summary>
        private void DisposeAndClearInstances()
        {
            _crmServiceClient?.Dispose();
            _crmServiceClient = null;
        }

        /// <summary>
        /// Dispose old CRM connection 
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            if (_crmServiceClient == null)
                return;

            if (_isInvalid || _credentials == null)
            {
                DisposeAndClearInstances();
            }

            _disposed = true;
        }
    }
}
