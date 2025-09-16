namespace FfAdmin.Calculator.Core;

public interface IContext
{
    ValueTask<object?> GetContext(Type type);
    ValueTask<object?> GetContext(object header, object key);
    IEnumerable<Type> AvailableContexts { get; }
    ValueTask<IContext> Previous { get; }
    Event Event { get; }
}

public interface IContext<T>
    where T:class
{
    public static IContext<T> Instance { get; } = new Impl();
    public async ValueTask<T> GetValue(IContext context) => await GetValueOrNull(context) ?? throw new ArgumentException($"EventProcessor for {typeof(T)} not found");
    ValueTask<T?> GetValueOrNull(IContext context);

    protected class Impl : IContext<T>
    {
        public async ValueTask<T?> GetValueOrNull(IContext context)
            => (T?)await context.GetContext(typeof(T));
    }
}

public interface IContext<THeader, TKey, TDetail> : IContext<THeader>
    where THeader : class, IModel<THeader, TKey, TDetail>
    where TKey : notnull
    where TDetail : class
   
{
    public static new IContext<THeader, TKey, TDetail> Instance { get; } = new Impl();
    public async ValueTask<TDetail> GetValue(IContext context, THeader header, TKey key)
        => await GetValueOrNull(context, header, key) ?? throw new ArgumentException($"EventProcessor for {typeof(TDetail)} not found");
    public async ValueTask<TDetail> GetValue(IContext context, TKey key)
        => await GetValue(context, await GetValue(context), key);
    ValueTask<TDetail?> GetValueOrNull(IContext context, THeader header, TKey key);
    
    private new class Impl : IContext<THeader>.Impl, IContext<THeader, TKey, TDetail>
    {
        public async ValueTask<TDetail?> GetValueOrNull(IContext context, THeader header, TKey key)
            => (TDetail?)await context.GetContext(header, key);
    }
}

public interface ICalculatingContext : IContext
{
    ICalculatingContext AddEvent(Event @event);
    bool IsEvaluated<T>();
    bool IsEvaluated(Type type);
    bool IsEvaluated(object header, object key);
}
