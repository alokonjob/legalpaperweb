using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaperWorks
{
    public class Program
    {
        private static string GetKeyVaultEndpoint() => "";//"https://onjob-jv-dev-01.vault.azure.net/";
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        //this one saved my life bcoz i was not able to connect with any other approach from
        //Azure App Service and i had tried IAM approach and approach in heimdall module

        //IMP https://cloudskills.io/blog/aspnet-core-azure-key-vault
        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, builder) =>
                  {
                      //var keyVaultEndpoint = GetKeyVaultEndpoint();
                      //if (!string.IsNullOrEmpty(keyVaultEndpoint))
                      //{
                      //    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                      //    var keyVaultClient = new KeyVaultClient(
                      //       new KeyVaultClient.AuthenticationCallback(
                      //          azureServiceTokenProvider.KeyVaultTokenCallback));
                      //    builder.AddAzureKeyVault(
                      //       keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                      //}
                  }
                )
           .ConfigureWebHostDefaults(webBuilder =>
           {
               webBuilder.UseStartup<Startup>();
           })
           .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddAzureWebAppDiagnostics();
                });
    }
}
