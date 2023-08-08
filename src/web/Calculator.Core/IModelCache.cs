namespace FfAdmin.Calculator.Core;

public interface IModelCache
{
    Task<int[]> GetIndexes();
    Task<int?> GetIndexLowerThanOrEqual(int index);
    Task<int?> GetIndexGreaterThanOrEqual(int index);
    Task<object?> Get(int index, Type type);
    async Task<T?> Get<T>(int index) 
        where T : class
        => (T?) await Get(index, typeof(T));
    Task Put(int index, Type type, object model);
    Task Put<T>(int index, T model) 
        where T : class
        => Put(index, typeof(T), model);

    public static IModelCache Empty { get; } = new EmptyImpl();
    private class EmptyImpl : IModelCache
    {
        public Task<int[]> GetIndexes()
            => Task.FromResult(Array.Empty<int>());

        public Task<int?> GetIndexLowerThanOrEqual(int index)
            => Task.FromResult(default(int?));

        public Task<int?> GetIndexGreaterThanOrEqual(int index)
            => Task.FromResult(default(int?));

        public Task<object?> Get(int index, Type type)
            => Task.FromResult<object?>(null);

        public Task Put(int index, Type type, object model)
            => Task.CompletedTask;
    }
}
