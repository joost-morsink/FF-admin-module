using System;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace FfAdmin.ModelCache.BlobStorage;

public class StorageClient
{
    public StorageClient()
    {
        var c = new BlobServiceClient(new Uri("https://modelcache.blob.core.windows.net"), new DefaultAzureCredential());
        Client = c.GetBlobContainerClient("model-cache");
    }

    public BlobContainerClient Client { get; }
}
