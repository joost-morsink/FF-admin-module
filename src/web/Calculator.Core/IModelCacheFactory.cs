using System.IO.Compression;
using System.Reflection;

namespace FfAdmin.Calculator.Core;

public interface IModelCacheFactory
{
    IModelCache Create(Type forType, object start)
        => (IModelCache)typeof(IModelCacheFactory).GetMethods()
            .First(m => m.Name==nameof(Create) && m.GetGenericArguments().Length==1)
            .MakeGenericMethod(forType)
            .Invoke(this, new[] {start})!;

    IModelCache<T> Create<T>(T start)
        where T : class;
}
