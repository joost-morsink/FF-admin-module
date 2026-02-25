using Azure.Identity;
using Azure.Storage.Blobs;

namespace FfAdmin.PublicApi.Function;

public class StorageClientProvider : IStorageClientProvider
{
    public BlobContainerClient GetStorageClient()
    {
        var c = new BlobServiceClient(new Uri("https://give4goodpublic.blob.core.windows.net"),
            new DefaultAzureCredential());
        return c.GetBlobContainerClient("data");
    }
}