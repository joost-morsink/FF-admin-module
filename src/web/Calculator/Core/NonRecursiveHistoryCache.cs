namespace FfAdmin.Calculator.Core;

public class NonRecursiveHistoryCache<T> : IHistoryCache<T>
    where T : class
{
    private readonly Func<int, T> _creator;
    private ImmutableDictionary<int, T> _cache;
    public NonRecursiveHistoryCache(Func<int, T> creator)
    {
        _creator = creator;
        _cache = ImmutableDictionary<int, T>.Empty;
    }

    public T GetAtPosition(int position)
    {
        if (!_cache.TryGetValue(position, out var x))
            _cache = _cache.Add(position, x = _creator(position));
        return x;
    }
}
