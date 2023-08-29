using FfAdmin.Calculator.Core;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Function;

public class ModelCacheFactory : IModelCacheFactory
{
    private readonly IModelCacheService _service;
    private readonly ModelCacheOptions _options;

    public ModelCacheFactory(IModelCacheService service, IOptions<ModelCacheOptions> options)
    {
        _service = service;
        _options = options.Value;
    }

    public IModelCache CreateForBranch(string branch)
        => new ModelCache(_service, branch, _options);
}
