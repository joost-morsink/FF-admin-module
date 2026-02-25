using Azure.Storage.Blobs;

namespace FfAdmin.PublicApi.Function;

public interface IStorageClientProvider
{
    BlobContainerClient GetStorageClient();
}