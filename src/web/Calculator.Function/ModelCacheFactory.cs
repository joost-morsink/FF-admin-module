using FfAdmin.Calculator.Core;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Function;

public class ModelCacheFactory(IModelCacheService service, IMemoryCache memoryCache, IOptions<ModelCacheOptions> options) : IModelCacheFactory
{
    private readonly ModelCacheOptions _options = options.Value;

    public IModelCache CreateForBranch(string branch)
        => new CachedModelCache(branch, new ModelCache(service, branch, _options), memoryCache); 
}
