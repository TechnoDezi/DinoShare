using Microsoft.AspNetCore.Hosting;
using DinoShare.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;

namespace DinoShare.Helpers
{
    public class AzureStorageHelperFunctions
    {
        internal IHostingEnvironment _env;
        internal SecurityOptions _securityOptions;

        public BlobProperties DownloadBlobProperties { get; set; }

        private CloudStorageAccount GetStorageAccount()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_securityOptions.BlobStorageConnectionString);

            return storageAccount;
        }

        private CloudBlobClient CreateBlobClient()
        {
            CloudBlobClient blobClient = GetStorageAccount().CreateCloudBlobClient();

            return blobClient;
        }

        private async Task<CloudBlobContainer> GetBlobContainer()
        {
            // Create the blob client.
            CloudBlobClient blobClient = CreateBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(_securityOptions.BlobContainerReference);

            // Create the container if it doesn't already exist.
            await container.CreateIfNotExistsAsync();

            return container;
        }

        public async Task UploadBlob(byte[] blobData, string blobName)
        {
            if (_securityOptions.UseFileStorage == false)
            {
                // Retrieve reference to a blob named "myblob".
                var container = await GetBlobContainer();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                await blockBlob.UploadFromByteArrayAsync(blobData, 0, blobData.Length);
            }
            else
            {
                string filename = GetFileNameCreateFolder(blobName);
                File.WriteAllBytes(filename, blobData);
            }
        }

        public async Task<byte[]> DownloadBlob(string blobName)
        {
            if (_securityOptions.UseFileStorage == false)
            {
                // Retrieve reference to a blob named "myblob".
                var container = await GetBlobContainer();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                using (var memoryStream = new MemoryStream())
                {
                    await blockBlob.DownloadToStreamAsync(memoryStream);
                    DownloadBlobProperties = blockBlob.Properties;

                    return memoryStream.ToArray();
                }
            }
            else
            {
                string filename = GetFileNameCreateFolder(blobName);
                if (File.Exists(filename))
                {
                    return File.ReadAllBytes(filename);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task DeleteBlob(string blobName)
        {
            if (!string.IsNullOrEmpty(blobName))
            {
                if (_securityOptions.UseFileStorage == false)
                {
                    // Retrieve reference to a blob named "myblob".
                    var container = await GetBlobContainer();
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                    await blockBlob.DeleteIfExistsAsync();
                }
                else
                {
                    string filename = GetFileNameCreateFolder(blobName);
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                }
            }
        }

        private string GetFileNameCreateFolder(string blobname)
        {
            string folderName = _securityOptions.FolderLocation;
            if (folderName.StartsWith("~"))
            {
                folderName = Path.Combine(_env.ContentRootPath, folderName.Replace("/", "").Replace("~", ""));
            }
            folderName = Path.Combine(folderName, _securityOptions.BlobContainerReference);

            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            return Path.Combine(folderName, blobname);
        }
    }
}
