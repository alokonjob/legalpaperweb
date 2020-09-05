using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace Asgard
{
    public class Heimdall : IHeimdall
    {
        private readonly IConfiguration configuration;

        // private readonly SecretClient client = null;
        public Heimdall(IConfiguration configuration)
        {
            this.configuration = configuration;
            #region MyFailedApproach_ResearchNeeded_YFailed
            //SecretClientOptions options = new SecretClientOptions()
            //{
            //    Retry =
            //            {
            //                Delay= TimeSpan.FromSeconds(2),
            //                MaxDelay = TimeSpan.FromSeconds(16),
            //                MaxRetries = 5,
            //                Mode = RetryMode.Exponential
            //             }
            //};
            //client = new SecretClient(new Uri("https://paperworks-0-kv.vault.azure.net/"), new DefaultAzureCredential(), options);
            #endregion
        }

        public string GetSecretValue(string key)
        {
            return configuration[key];

            #region MyFailedApproach_ResearchNeeded_YFailed
            //try
            //{
            //    SecretClient client = null;
            //    SecretClientOptions options = new SecretClientOptions()
            //    {
            //        Retry =
            //            {
            //                Delay= TimeSpan.FromSeconds(2),
            //                MaxDelay = TimeSpan.FromSeconds(16),
            //                MaxRetries = 5,
            //                Mode = RetryMode.Exponential
            //             }
            //    };
            //    client = new SecretClient(new Uri("https://paperworks-0-kv.vault.azure.net/"), new DefaultAzureCredential(), options);
            //    KeyVaultSecret secret = client.GetSecret(key);
            //    string secretValue = secret.Value;
            //    return secretValue;
            //}
            //catch (Exception error)
            //{
            //    return $"Vault error {error.Message}";
            //}

#endregion
        }
    }
}
