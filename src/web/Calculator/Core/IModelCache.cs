namespace FfAdmin.Calculator.Core;

public interface IModelCache
{
    Task<object?> GetAtPosition(int position);
    Task SetAtPosition(int position, object value);
    Task<int[]> GetAvailablePositions();
    Task<int> GetBasePosition(int position);
}

public interface IModelCache<T> : IModelCache
    where T : class
{
    new Task<T?> GetAtPosition(int position);
    Task SetAtPosition(int position, T value);

    async Task<object?> IModelCache.GetAtPosition(int position)
        => await GetAtPosition(position);

    Task IModelCache.SetAtPosition(int position, object value)
        => SetAtPosition(position, (T)value);

}

public class InMemoryModelCache<T> : IModelCache<T>
    where T : class
{
    private ImmutableList<int> _availablePositions = ImmutableList<int>.Empty;
    private ImmutableDictionary<int, T> _values = ImmutableDictionary<int, T>.Empty;

    public Task<T?> GetAtPosition(int position)
    {
        return Task.FromResult(_values.GetValueOrDefault(position));
    }

    public Task SetAtPosition(int position, T value)
    {
        var loc = _availablePositions.BinarySearch(position);
        if (loc < 0)
            _availablePositions = _availablePositions.Insert(~loc, position);

        _values = _values.SetItem(position, value);
        return Task.CompletedTask;
    }

    public Task<int[]> GetAvailablePositions()
    {
        return Task.FromResult(_availablePositions.ToArray());
    }
    public Task<int> GetBasePosition(int position)
    {
        var loc = _availablePositions.BinarySearch(position);
        if (loc < 0)
            return Task.FromResult(_availablePositions[~loc]);
        return Task.FromResult(position);
    }
}
