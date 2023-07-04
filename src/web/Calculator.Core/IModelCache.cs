namespace FfAdmin.Calculator.Core;

public interface IModelCache
{
    Task<object?> GetAtPosition(int position);
    Task SetAtPosition(int position, object value);
    Task<int[]> GetAvailablePositions();
    Task<int> GetBasePosition(int position);
    IModelCache Clone();
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

    new IModelCache<T> Clone();
    IModelCache IModelCache.Clone() 
        => Clone();
}
