using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Store
{

    public class FileDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream Content { get; set; }
    }
    public class Storage
    {
        public string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=onjobdev;AccountKey=LWB0QiYx3UyGPrXDIQ3nU50rPv4CVnBqiUbxnz/TE/bIQ+4TAuv8Mow0ewnzK9mVYJytmeJDOUwfCAI9axx6QA==;EndpointSuffix=core.windows.net";
        public async Task ConnectionStringAsync()
        {
            // Get a connection string to our Azure Storage account.  You can
            // obtain your connection string from the Azure Portal (click
            // Access Keys under Settings in the Portal Storage account blade)
            // or using the Azure CLI with:
            //
            //     az storage account show-connection-string --name <account_name> --resource-group <resource_group>
            //
            // And you can provide the connection string to your application
            // using an environment variable.
            string connectionString = ConnectionString;

            // Create a client that can authenticate with a connection string
            BlobServiceClient service = new BlobServiceClient(connectionString);

            // Make a service request to verify we've successfully authenticated
            var xx = await service.GetPropertiesAsync();
        }
        public async Task AnonymousAuthAsync()
        {
            BlobContainerClient container = new BlobContainerClient(ConnectionString, ("sample-container"));
            try
            {
                // Create a blob that can be accessed publicly
                await container.CreateAsync(PublicAccessType.Blob);
                BlobClient blob = container.GetBlobClient(("sample-blob"));
                await blob.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes("Blob Content")));

                // Anonymously access a blob given its URI
                Uri endpoint = blob.Uri;
                BlobClient anonymous = new BlobClient(endpoint);

                // Make a service request to verify we've successfully authenticated
                var xx = await anonymous.GetPropertiesAsync();
            }
            finally
            {
                await container.DeleteAsync();
            }
        }

        public async Task SharedKeyAuthAsync()
        {
            // Get a Storage account name, shared key, and endpoint Uri.
            //
            // You can obtain both from the Azure Portal by clicking Access
            // Keys under Settings in the Portal Storage account blade.
            //
            // You can also get access to your account keys from the Azure CLI
            // with:
            //
            //     az storage account keys list --account-name <account_name> --resource-group <resource_group>
            //
            string accountName = "onjobdev";
            string accountKey = "LWB0QiYx3UyGPrXDIQ3nU50rPv4CVnBqiUbxnz/TE/bIQ+4TAuv8Mow0ewnzK9mVYJytmeJDOUwfCAI9axx6QA==";
            string uri = "https://onjobdev.blob.core.windows.net/sample-container/sample-blob";
            Uri serviceUri = new Uri(uri);

            // Create a SharedKeyCredential that we can use to authenticate
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Create a client that can authenticate with a connection string
            BlobServiceClient service = new BlobServiceClient(serviceUri, credential);

            // Make a service request to verify we've successfully authenticated
            var xx = await service.GetPropertiesAsync();
        }


        public async Task Upload(string caseId, List<IFormFile> files)
        {
            string connectionString = ConnectionString;
            string containerName = caseId;
            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            if (!await container.ExistsAsync())
            {
                await container.CreateAsync();
            }
            foreach (var formFile in files)
            {
                BlobClient blob = container.GetBlobClient(formFile.FileName);
                using (var data = formFile.OpenReadStream())
                {
                    await blob.UploadAsync(data);
                }

            }

        }


        public async Task<List<string>> List(string CaseId)
        {
            string connectionString = ConnectionString;
            string containerName = CaseId;


            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            if (!container.Exists())
            {
                container.Create();
            }

            // Print out all the blob names
            foreach (BlobItem blob in container.GetBlobs())
            {
                Console.WriteLine(blob.Name);
            }

            List<string> names = new List<string>();
            await foreach (BlobItem blob in container.GetBlobsAsync())
            {
                names.Add(blob.Name);
            }
            return names;
        }

        public async Task Delete(string CaseId, string fileName)
        {
            string connectionString = ConnectionString;
            string containerName = CaseId;


            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            await container.DeleteBlobIfExistsAsync(fileName);
        }

        public async Task<FileDto> Download(string CaseId, string fileName)
        {
            string connectionString = ConnectionString;
            string containerName = CaseId;


            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            // Get a reference to a blob named "sample-file"
            BlobClient blob = container.GetBlobClient(fileName);

            // First upload something the blob so we have something to download

            // Download the blob's contents and save it to a file
            BlobDownloadInfo download = await blob.DownloadAsync();
            return new FileDto() { FileName = blob.Name, Content = download.Content,ContentType = download.ContentType };
            //using (FileStream downloadFileStream = File.OpenWrite(fileName))
            //{
            //    await download.Content.CopyToAsync(downloadFileStream);
            //    downloadFileStream.Close();
            //}


        }


    }
}
