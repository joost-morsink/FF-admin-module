namespace FfAdmin.Calculator.Core;

public interface IContext
{
    ValueTask<object?> GetContext(Type type);
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

    private class Impl : IContext<T>
    {
        public async ValueTask<T?> GetValueOrNull(IContext context)
            => (T?)await context.GetContext(typeof(T));
    }
}

public interface ICalculatingContext : IContext
{
    ICalculatingContext AddEvent(Event @event);
    bool IsEvaluated<T>();
    bool IsEvaluated(Type type);
}
