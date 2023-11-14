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
    object IEventProcessor.Start => Start;
    Type IEventProcessor.ModelType => typeof(T);
    object IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => Process((T)model, previousContext, context, e);
    new T Start { get; }
    T Process(T model, IContext previousContext, IContext context, Event e);
}
