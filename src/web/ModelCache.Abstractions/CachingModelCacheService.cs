using System;
using System.Threading.Tasks;
using FfAdmin.Common;
using Microsoft.Extensions.Caching.Memory;

namespace FfAdmin.ModelCache.Abstractions;

public class CachingModelCacheService : IModelCacheService
{
    private const string MODELCACHEDATA = "ModelCacheData";
    private const string TYPESFORHASH = "TypesForHash";
    private readonly IModelCacheService _service;
    private readonly IMemoryCache _cache;
    
    public CachingModelCacheService(IModelCacheService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }
    private static object ModelCacheDataKey(HashValue hash, string type)
        => (MODELCACHEDATA, hash, type);

    private static object TypesForHashKey(HashValue hash)
        => (TYPESFORHASH, hash);

    public Task<string[]> GetBranches()
        => _service.GetBranches();
    public Task ClearCache()
        => _service.ClearCache();

    public Task RemoveBranch(string branchName)
        => _service.RemoveBranch(branchName);

    public Task RemoveModel(string type)
        => _service.RemoveModel(type);

    public Task<HashesForBranch> GetHashesForBranch(string branchName)
        => _service.GetHashesForBranch(branchName);

    public Task PutHashesForBranch(string branchName, HashesForBranch data)
        => _service.PutHashesForBranch(branchName, data);

    public async Task<string[]> GetTypesForHash(HashValue hash)
    {
        return await _cache.GetOrCreateAsync(TypesForHashKey(hash), ce =>
        {
            ce.SetAbsoluteExpiration(TimeSpan.FromMinutes(5.0));
            return _service.GetTypesForHash(hash);
        }) ?? Array.Empty<string>();
    }

    public async Task<byte[]?> GetData(HashValue hash, string type)
    {
        var key = ModelCacheDataKey(hash, type);
        if (_cache.TryGetValue<byte[]>(key, out var cacheData))
            return cacheData;

        var result = await _service.GetData(hash, type);
        if (result is not null)
            _cache.Set(key, result, TimeSpan.FromMinutes(5.0));
        return result;
    }

    public Task PutData(HashValue hash, string type, byte[] data)
    {
        _cache.Remove(TypesForHashKey(hash));
        var key = ModelCacheDataKey(hash, type);
        _cache.Set(key, data, TimeSpan.FromMinutes(5.0));
        return _service.PutData(hash, type, data);
    }

    public Task<bool> RunGarbageCollection()
    {
        return _service.RunGarbageCollection();
    }
}
