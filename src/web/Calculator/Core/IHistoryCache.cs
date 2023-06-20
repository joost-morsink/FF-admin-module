using System.Runtime.CompilerServices;

namespace FfAdmin.Calculator.Core;

public interface IHistoryCache
{
    object GetAtPosition(int position);
}

public interface IHistoryCache<T> : IHistoryCache
    where T : class
{
    object IHistoryCache.GetAtPosition(int position)
        => GetAtPosition(position);

    new T GetAtPosition(int position);
}
