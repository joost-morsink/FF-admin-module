namespace FfAdmin.Calculator.Core;

public interface IModelCache
{
    Task<int[]> GetIndexes();
    Task<int?> GetIndexLowerThanOrEqual(int index);
    Task<int?> GetIndexGreaterThanOrEqual(int index);
    Task<(Type, object)[]> GetAvailableData(IEnumerable<Type> types, int index);
    Task<object?> Get(int index, Type type);
    Task<object?> Get(int index, Type type, Bucket bucket);
    async Task<T?> Get<T>(int index) 
        where T : class
        => (T?) await Get(index, typeof(T));
    async Task<T?> Get<T>(int index, Bucket bucket)
        where T : class
        => (T?) await Get(index, typeof(T), bucket);
    Task Put(int index, Type type, object model);
    Task Put(int index, Type type, Bucket bucket, object model);
    Task Put<T>(int index, T model) 
        where T : class
        => Put(index, typeof(T), model);

    Task Put<T>(int index, Bucket bucket, T model)
        where T : class
        => Put(index, typeof(T), model);
    
    IModelCache GetPrefix(int count) => Prefixed.From(this, count);

    public static IModelCache Empty { get; } = new EmptyImpl();
    private class EmptyImpl : IModelCache
    {
        public Task<int[]> GetIndexes()
            => Task.FromResult(Array.Empty<int>());

        public Task<int?> GetIndexLowerThanOrEqual(int index)
            => Task.FromResult(default(int?));

        public Task<int?> GetIndexGreaterThanOrEqual(int index)
            => Task.FromResult(default(int?));

        public Task<(Type, object)[]> GetAvailableData(IEnumerable<Type> types, int index)
            => Task.FromResult(Array.Empty<(Type, object)>());

        public Task<object?> Get(int index, Type type)
            => Task.FromResult<object?>(null);
        public Task<object?> Get(int index, Type type, Bucket bucket)
            => Task.FromResult<object?>(null);
        
        public Task Put(int index, Type type, object model)
            => Task.CompletedTask;
        public Task Put(int index, Type type, Bucket bucket, object model)
            => Task.CompletedTask;
    }

    private class Prefixed : IModelCache
    {
        public static Prefixed From(IModelCache cache, int count)
        {
            if (cache is Prefixed p && p._count >= count)
                return new(p._inner, count);
            return new(cache, count);
        }
        private readonly IModelCache _inner;
        private readonly int _count;

        public Prefixed(IModelCache inner, int count)
        {
            _inner = inner;
            _count = count;
        }

        public async Task<int[]> GetIndexes()
            => (await _inner.GetIndexes()).TakeWhile(x => x < _count).ToArray();

        public Task<int?> GetIndexLowerThanOrEqual(int index)
            => _inner.GetIndexLowerThanOrEqual(Math.Min(index, _count - 1));

        public Task<int?> GetIndexGreaterThanOrEqual(int index)
            => _inner.GetIndexGreaterThanOrEqual(Math.Min(index, _count - 1));

        public Task<(Type, object)[]> GetAvailableData(IEnumerable<Type> types, int index)
            => _inner.GetAvailableData(types, index);

        public async Task<object?> Get(int index, Type type)
            => index >= _count ? null : await _inner.Get(index, type);
        public async Task<object?> Get(int index, Type type, Bucket bucket)
            => index >= _count ? null : await _inner.Get(index, type, bucket);

        public async Task Put(int index, Type type, object model)
        {
            if (index < _count)
                await _inner.Put(index, type, model);
        }
        public async Task Put(int index, Type type, Bucket bucket, object model)
        {
            if (index < _count)
                await _inner.Put(index, type, bucket, model);
        }
    }
}

