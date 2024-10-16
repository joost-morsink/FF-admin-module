using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FfAdmin.Common;
using FfAdmin.ModelCache.Abstractions;

namespace FfAdmin.ModelCache.BlobStorage;

public class ModelCacheService : IModelCacheService
{
    private readonly StorageClient _storageClient;

    public ModelCacheService(StorageClient storageClient)
    {
        _storageClient = storageClient;
    }

    public async Task ClearCache()
    {
        var client = GetContainerClient();
        var pageable = client.GetBlobsAsync();
        await foreach(var item in pageable)
            await client.DeleteBlobAsync(item.Name);
    }

    public async Task RemoveBranch(string branchName)
    {
        var blobClient = HashBlobClient(branchName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task RemoveModel(string type)
    {
        var client = GetContainerClient();
        var pageable = client.GetBlobsByHierarchyAsync(prefix: "data/");
        
        await foreach (var item in pageable)
        {
            var parts = item.Blob.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && parts[2] == type)
            {
                var blobClient = GetBlobClient(item.Blob.Name);
                await blobClient.DeleteIfExistsAsync();
            }
        }
    }

    private static string HashBlobName(string branchName)
        => $"hashes/{branchName}";

    private BlobClient HashBlobClient(string branchName)
        => GetBlobClient(HashBlobName(branchName));

    private static string DataBlobName(HashValue hash, string type)
        => $"data/{hash.AsSpan().ToHexString()}/{type}";
    
    private BlobClient DataBlobClient(HashValue hash, string type)
        => GetBlobClient(DataBlobName(hash, type));

    private BlobClient GetBlobClient(string path)
        => _storageClient.Client.GetBlobClient(path);

    private BlobContainerClient GetContainerClient()
        => _storageClient.Client;
    private static async Task<byte[]?> Read(BlobClient blobClient)
    {
        try
        {
            var blob = await blobClient.DownloadAsync();
            if (!blob.HasValue)
                return null;
            await using var unzip = new GZipStream(blob.Value.Content, CompressionMode.Decompress);
            using var ms = new MemoryStream();
            await unzip.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (RequestFailedException) 
        {
            return null;
        }
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

    public async Task<string[]> GetBranches()
        => await GetAllBranches().ToArrayAsync();
    
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

    public async Task<string[]> GetTypesForHash(HashValue hash)
    {
        var client = GetContainerClient();
        var pageable = client.GetBlobsByHierarchyAsync(prefix: $"data/{hash.AsSpan().ToHexString()}");
        var result = new List<string>();
        await foreach(var item in pageable)
            result.Add(item.Blob.Name.Substring(item.Blob.Name.LastIndexOf('/') + 1));
        return result.ToArray();
    }

    public Task<byte[]?> GetData(HashValue hash, string type)
    {
        var blobClient = DataBlobClient(hash, type);
        return Read(blobClient);
    }

    public Task PutData(HashValue hash, string type, byte[] data)
    {
        var blobClient = DataBlobClient(hash, type);
        return Write(blobClient, data);
    }

    public IAsyncEnumerable<string> GetAllBranches()
    {
        var client = GetContainerClient();
        IAsyncEnumerable<BlobHierarchyItem> pageable = client.GetBlobsByHierarchyAsync(prefix: "hashes/");
        return from item in pageable.AsAsyncEnumerable()
            select item.Blob.Name.Substring(item.Blob.Name.LastIndexOf('/') + 1);
    }
    private async Task<HashSet<HashValue>> GetAllHashes()
    {
        return new HashSet<HashValue>(await (from branch in GetAllBranches()
            from hfb in GetHashesForBranch(branch).ToAsyncEnumerable()
            from hash in hfb.Hashes.Values.ToAsyncEnumerable()
            select (HashValue)hash).Distinct().ToListAsync());
    }

    public async Task<bool> RunGarbageCollection()
    {
        var start = DateTime.UtcNow;
        var usedHashes = await GetAllHashes();
        var client = GetContainerClient();
        var pageable = client.GetBlobsByHierarchyAsync(prefix: "data/");
        
        await foreach (var item in pageable)
        {
            var parts = item.Blob.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && !usedHashes.Contains(parts[1].ToByteArray()))
            {
                var blobClient = GetBlobClient(item.Blob.Name);
                await blobClient.DeleteIfExistsAsync();
            }
            if(DateTime.UtcNow - start > TimeSpan.FromSeconds(30))
                return false;
        }
        
        return true;
    }
}
