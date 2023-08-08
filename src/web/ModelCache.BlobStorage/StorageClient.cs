using System;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace FfAdmin.ModelCache.BlobStorage;

public class StorageClient
{
    private readonly BlobContainerClient _client;

    public StorageClient()
    {
        var c = new BlobServiceClient(new Uri("https://modelcache.blob.core.windows.net"), new DefaultAzureCredential());
        _client = c.GetBlobContainerClient("model-cache");
    }
    
    public BlobContainerClient Client => _client;
}
