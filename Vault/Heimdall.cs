using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
namespace Asgard
{
    public class Heimdall : IHeimdall
    {
        private readonly SecretClient client = null;
        public Heimdall()
        {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                        {
                            Delay= TimeSpan.FromSeconds(2),
                            MaxDelay = TimeSpan.FromSeconds(16),
                            MaxRetries = 5,
                            Mode = RetryMode.Exponential
                         }
            };
            client = new SecretClient(new Uri("https://paperworks-0-kv.vault.azure.net/"), new DefaultAzureCredential(), options);
        }

        public string GetSecretValue(string key)
        {
            KeyVaultSecret secret =  client.GetSecret(key);
            string secretValue = secret.Value;
            return secretValue;
        }
    }
}
