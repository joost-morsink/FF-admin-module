using System.Collections.Immutable;
using System.Text.Json;
using FfAdmin.Calculator.Core;
using FfAdmin.Common;
using FfAdmin.ModelCache.Abstractions;

namespace FfAdmin.Calculator.Function;

public class ModelCache : IModelCache
{
    private readonly IModelCacheService _service;
    private readonly string _branch;
    private readonly ModelCacheOptions _options;
    private Task<(SortedImmutableDictionary<int, HashValue>, ImmutableDictionary<HashValue,int>)> _hashes;

    private async Task<SortedImmutableDictionary<int, HashValue>> Forward()
        => (await _hashes).Item1;
    
    private async Task<ImmutableDictionary<HashValue, int>> Reverse()
        => (await _hashes).Item2;
    
    public ModelCache(IModelCacheService service, string branch, ModelCacheOptions options)
    {
        _service = service;
        _branch = branch;
        _options = options;
        _hashes = GetHashes();
    }
    private async Task<(SortedImmutableDictionary<int, HashValue>, ImmutableDictionary<HashValue, int>)> GetHashes()
    {
        var hashes = await _service.GetHashesForBranch(_branch);
        return (hashes.Hashes.ToSortedImmutableDictionary(h => h.Key, h => (HashValue)h.Value),
            hashes.Hashes.ToImmutableDictionary(h => (HashValue)h.Value, h => h.Key));
    }

    public async Task<int[]> GetIndexes()
        => (await Forward()).Keys.ToArray();

    public async Task<int?> GetIndexLowerThanOrEqual(int index)
        => (await Forward()).FirstKeyLowerThanOrEqual(index);

    public async Task<int?> GetIndexGreaterThanOrEqual(int index)
        => (await Forward()).FirstKeyGreaterThanOrEqual(index);

    public async Task<(Type, object)[]> GetAvailableData(IEnumerable<Type> types, int index)
    {
        if (!_options.GetEnabled)
            return Array.Empty<(Type, object)>();
        var indexes = await Forward();
        if(!indexes.TryGetValue(index, out var hash))
            return Array.Empty<(Type, object)>();
        var availableData = await _service.GetTypesForHash(hash);
        var result = (await Task.WhenAll(
            from type in types
            join data in availableData on type.Name equals data
            select InnerGet(type))).Prepend((typeof(HistoryHash), new HistoryHash(hash.AsSpan().ToArray())));
        
        return result.ToArray();
        
        async Task<(Type, object)> InnerGet(Type type)
        {
            var data = await _service.GetData(hash, type.Name);
            if (data is null)
                throw new InvalidOperationException("Data not found");
            return (type, FromJson(type, data));
        }
    }
    public async Task<object?> Get(int index, Type type)
    {
        if(!_options.GetEnabled)
            return null;
        if (type == typeof(HistoryHash))
        {
            var indexes = await _service.GetHashesForBranch(_branch);
            if (indexes.Hashes.TryGetValue(index, out var hash))
                return new HistoryHash(hash);
            return null;
        }
        else
        {
            var indexes = await Forward();
            if (!indexes.TryGetValue(index, out var hash))
                return null;
            var data = await _service.GetData(hash, type.Name);
            if (data is null)
                return null;
            return FromJson(type, data);
        }
    }

    public async Task Put(int index, Type type, object model)
    {
        if(!_options.PutEnabled)
            return;
        if (type == typeof(HistoryHash))
        {
            var indexes = await _service.GetHashesForBranch(_branch);
            if (indexes.Hashes.TryGetValue(index, out var hash) && HashValue.Equals(hash, ((HistoryHash)model).Hash))
                return;
            var result = indexes.Hashes.Where(x => x.Key < index).ToImmutableDictionary();
            result = result.Add(index, ((HistoryHash)model).Hash);
            await _service.PutHashesForBranch(_branch, new HashesForBranch(result));
            _hashes = GetHashes();
        }
        else
        {
            var indexes = await Forward();
            if (!indexes.TryGetValue(index, out var hash))
                return;
            var data = ToJson(type, model);
            await _service.PutData(hash, type.Name, data);
        }
    }
    
    private object FromJson(Type type, byte[] data)
    {
        return JsonSerializer.Deserialize(data.AsSpan(), type) ?? throw new InvalidOperationException("Not valid JSON");
    }

    private byte[] ToJson(Type type, object model)
    {
        using var ms = new MemoryStream();
        JsonSerializer.Serialize(ms, model, type);
        return ms.ToArray();
    }
}
