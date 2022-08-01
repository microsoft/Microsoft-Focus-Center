using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnonymizationFunctionApp
{
    public static class KeyVaultManager
    {
        /// <summary>
        /// To Retrieve the Secret key from Azure Key Vault
        /// </summary>
        /// <param name="secretConnectionString"></param>
        /// <returns></returns>
        public static Task<SecretBundle> RetrieveSecret(string secretConnectionString)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            return keyVaultClient.GetSecretAsync(secretConnectionString);
        }
    }
}
