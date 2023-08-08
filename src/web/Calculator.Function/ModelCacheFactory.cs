using FfAdmin.Calculator.Core;
using FfAdmin.ModelCache.Abstractions;

namespace FfAdmin.Calculator.Function;

public class ModelCacheFactory : IModelCacheFactory
{
    private readonly IModelCacheService _service;

    public ModelCacheFactory(IModelCacheService service)
    {
        _service = service;
    }

    public IModelCache CreateForBranch(string branch)
        => new ModelCache(_service, branch);
}
