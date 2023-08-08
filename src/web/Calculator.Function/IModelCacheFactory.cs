using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator.Function;

public interface IModelCacheFactory
{
    IModelCache CreateForBranch(string branch);
}
