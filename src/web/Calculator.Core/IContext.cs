namespace FfAdmin.Calculator.Core;

public interface IContext
{
    object? GetContext(Type type);
    IEnumerable<Type> AvailableContexts { get; }
    IContext Previous { get; }
    Event Event { get; }
}

public interface IContext<T>
    where T:class
{
    
    public static IContext<T> Instance { get; } = new Impl();
    public T GetValue(IContext context) => GetValueOrNull(context) ?? throw new ArgumentException($"EventProcessor for {typeof(T)} not found");
    T? GetValueOrNull(IContext context);

    private class Impl : IContext<T>
    {
        public T? GetValueOrNull(IContext context)
            => (T?)context.GetContext(typeof(T));
    }
}

public interface ICalculatingContext : IContext
{
    ICalculatingContext AddEvent(Event @event);
    bool IsEvaluated<T>();
    bool IsEvaluated(Type type);
}
