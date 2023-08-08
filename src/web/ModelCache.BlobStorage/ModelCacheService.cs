using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FfAdmin.ModelCache.Abstractions;

namespace FfAdmin.ModelCache.BlobStorage;

public class ModelCacheService : IModelCacheService
{
    private readonly StorageClient _storageClient;

    public ModelCacheService(StorageClient storageClient)
    {
        _storageClient = storageClient;
    }

    public async Task RemoveBranch(string branchName)
    {
        var blobClient = HashBlobClient(branchName);
        await blobClient.DeleteIfExistsAsync();
    }

    private static string HashBlobName(string branchName)
        => $"hashes/{branchName}";

    private BlobClient HashBlobClient(string branchName)
        => GetBlobClient(HashBlobName(branchName));

    private static string DataBlobName(byte[] hash, string type)
        => $"data/{hash.ToHexString()}/{type}";
    
    private BlobClient DataBlobClient(byte[] hash, string type)
        => GetBlobClient(DataBlobName(hash, type));

    private BlobClient GetBlobClient(string path)
        => _storageClient.Client.GetBlobClient(path);
    
    private static async Task<byte[]?> Read(BlobClient blobClient)
    {
        var blob = await blobClient.DownloadAsync();
        if (!blob.HasValue)
            return null;
        await using var unzip = new GZipStream(blob.Value.Content, CompressionMode.Decompress);
        using var ms = new MemoryStream();
        await unzip.CopyToAsync(ms);
        return ms.ToArray();
    }

    private static async Task Write(BlobClient blobClient, byte[] data)
    {
        using var ms = new MemoryStream();
        await using var zip = new GZipStream(ms, CompressionMode.Compress, true);
        await zip.WriteAsync(data);
        zip.Close();
        ms.Position = 0;
        var _ = await blobClient.UploadAsync(ms, true);
    }

    public async Task<HashesForBranch> GetHashesForBranch(string branchName)
    {
        var blobClient = HashBlobClient(branchName);
        if(!(await blobClient.ExistsAsync()).Value)
            return HashesForBranch.Empty;
        var content = await Read(blobClient);
        return JsonSerializer.Deserialize<HashesForBranch>(content) ?? HashesForBranch.Empty;
    }

    public Task PutHashesForBranch(string branchName, HashesForBranch data)
    {
        var blobClient = HashBlobClient(branchName);
        var content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        return Write(blobClient, content);
    }

    public Task<byte[]?> GetData(byte[] hash, string type)
    {
        var blobClient = DataBlobClient(hash, type);
        return Read(blobClient);
    }

    public Task PutData(byte[] hash, string type, byte[] data)
    {
        var blobClient = DataBlobClient(hash, type);
        return Write(blobClient, data);
    }
}
