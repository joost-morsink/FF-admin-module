namespace FfAdmin.Calculator.Core;

public interface IContext
{
    T? GetContextOrNull<T>()
        where T : class;

    T GetContext<T>()
        where T : class;
    object? GetContext(Type type);
    IEnumerable<Type> AvailableContexts { get; }
    IContext Previous { get; }
    Event Event { get; }
}

public interface ICalculatingContext : IContext
{
    ICalculatingContext AddEvent(Event @event);
    bool IsEvaluated<T>();
    bool IsEvaluated(Type type);
}
