namespace FfAdmin.Calculator.Core;

public interface IEventProcessor
{
    bool IsRecursive { get; }
    object Start { get; }
    object Process(object model, IContext previousContext, IContext context, Event e);
    Type ModelType { get; }
    IEnumerable<Type> Dependencies { get; }
}

public interface IEventProcessor<T> : IEventProcessor
    where T : class
{
    bool IEventProcessor.IsRecursive => true;
    new T Start { get; }
    T Process(T model, IContext previousContext, IContext context, Event e);
}
