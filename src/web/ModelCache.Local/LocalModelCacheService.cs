using System.Text.Json;
using FfAdmin.Common;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.Options;

namespace FfAdmin.ModelCache.Local;

public class LocalModelCacheService : IModelCacheService
{
    private readonly LocalModelCacheServiceOptions _options;
    private string RootDir => _options.Directory;
    private string HashesDir => Path.Combine(RootDir, "hashes");
    private string DataDir => Path.Combine(RootDir, "data");
    
    public LocalModelCacheService(IOptions<LocalModelCacheServiceOptions> options)
    {
        _options = options.Value;
        foreach (var x in new [] {RootDir, HashesDir, DataDir})
            if (!Directory.Exists(x))
                Directory.CreateDirectory(x);
    }

    private void SilentlyDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // Ignore errors.
        }
    }
    private void SilentlyDeleteFiles(IEnumerable<string> paths)
    {
        foreach(var path in paths)
            SilentlyDeleteFile(path);
    }
    public Task<string[]> GetBranches()
        => Task.FromResult(Directory.EnumerateFiles(HashesDir).ToArray());
    public Task ClearCache()
    {
        SilentlyDeleteFiles(Directory.EnumerateFiles(RootDir, "*", SearchOption.AllDirectories));
        
        return Task.CompletedTask;
    }

    public Task RemoveBranch(string branchName)
    {
        var path = Path.Combine(HashesDir, branchName);
        SilentlyDeleteFile(path);
        
        return Task.CompletedTask;
    }

    public Task RemoveModel(string type)
    {
        SilentlyDeleteFiles(Directory.EnumerateFiles(DataDir, $"{type}", SearchOption.AllDirectories));
        SilentlyDeleteFiles(Directory.EnumerateFiles(DataDir, $"{type}_*", SearchOption.AllDirectories));
        
        return Task.CompletedTask;
    }

    public async Task<HashesForBranch> GetHashesForBranch(string branchName)
    {
        var path = Path.Combine(HashesDir, branchName);
        if(!File.Exists(path))
            return HashesForBranch.Empty;
        var content = await File.ReadAllBytesAsync(path);
        return JsonSerializer.Deserialize<HashesForBranch>(content) ?? HashesForBranch.Empty;
    }

    public async Task PutHashesForBranch(string branchName, HashesForBranch data)
    {
        var path = Path.Combine(HashesDir, branchName);
        var content = JsonSerializer.SerializeToUtf8Bytes(data);
        await File.WriteAllBytesAsync(path, content);
    }

    public Task<string[]> GetTypesForHash(HashValue hash)
    {
        var path = Path.Combine(HashesDir, hash.AsSpan().ToArray().ToSafeBase64String());
        if (!Directory.Exists(path))
            return Task.FromResult(Array.Empty<string>());
        return Task.FromResult(Directory.EnumerateFiles(path).ToArray());
    }

    public async Task<byte[]?> GetData(HashValue hash, string type)
    {
        var filePath = Path.Combine(DataDir, hash.AsSpan().ToArray().ToSafeBase64String(), type);
        if (!File.Exists(filePath))
            return null;
        return await File.ReadAllBytesAsync(filePath);
    }

    public Task PutData(HashValue hash, string type, byte[] data)
    {
        var dirPath = Path.Combine(DataDir, hash.AsSpan().ToArray().ToSafeBase64String());
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        var filePath = Path.Combine(dirPath, type);
        return File.WriteAllBytesAsync(filePath, data);
    }

    public async Task<bool> RunGarbageCollection()
    {       
        var allHashes = from branch in GetBranches().ToAsyncEnumerable()
            from hfb in GetHashesForBranch(branch).ToAsyncEnumerable()
            from hash in hfb.Hashes.Values.ToAsyncEnumerable()
            select (HashValue)hash;
        await foreach (var dataDir in Directory.GetDirectories(DataDir).ToAsyncEnumerable().Except(allHashes.Select(h => Path.Combine(DataDir, h.AsSpan().ToArray().ToSafeBase64String()))))
        {
            if (Directory.Exists(dataDir))
            {
                SilentlyDeleteFiles(Directory.EnumerateFiles(dataDir));
                Directory.Delete(dataDir);
            }
        }

        return true;
    }
}
