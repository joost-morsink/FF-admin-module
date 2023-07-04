namespace FfAdmin.Calculator.Core;

public class ModelCacheFactory : IModelCacheFactory
{
    public IModelCache<T> Create<T>(T start)
        where T : class
    {
        var res = new InMemoryModelCache<T>();
        res.SetAtPosition(0, start);
        return res;
    }
}
