using FfAdmin.Calculator.Core;
using Microsoft.Extensions.Caching.Memory;

namespace FfAdmin.Calculator.Function;

public class CachedModelCache(string prefix, IModelCache inner, IMemoryCache memoryCache) : IModelCache
{
    private Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory)
    {
        return memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
            var res =  await factory();
            if (res is null)
                entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(5));
            return res;
        });
    }

    private void Set(string key, object value)
    {
        memoryCache.Set(key, value, TimeSpan.FromMinutes(15));
    }
    public Task<int[]> GetIndexes()
        => inner.GetIndexes();
    public Task<int[]> GetStoredIndexes()
        => inner.GetStoredIndexes();
    public Task<int?> GetIndexLowerThanOrEqual(int index)
        => inner.GetIndexLowerThanOrEqual(index);

    public Task<int?> GetIndexGreaterThanOrEqual(int index)
        => inner.GetIndexGreaterThanOrEqual(index);

    public Task<(Type, object)[]> GetAvailableData(IEnumerable<Type> types, int index)
        => inner.GetAvailableData(types, index);

    public Task<object?> Get(int index, IMetaModel type)
        => GetOrCreateAsync($"{prefix}.{index}.{type.HeaderType.Name}", () => inner.Get(index, type));

    public Task<object?> Get(int index, IMetaModel model, Bucket bucket)
        => GetOrCreateAsync($"{prefix}.{index}.{model.HeaderType.Name}.{bucket.Name}", () => inner.Get(index, model, bucket));

    public Task Put(int index, IMetaModel metamodel, object model)
    {
        Set($"{prefix}.{index}.{metamodel.HeaderType.Name}", model);
        return inner.Put(index, metamodel, model);
    }

    public Task Put(int index, IMetaModel metamodel, Bucket bucket, object model)
    {
        Set($"{prefix}.{index}.{metamodel.HeaderType.Name}.{bucket.Name}", model);
        return inner.Put(index, metamodel, bucket, model);
    }
}
