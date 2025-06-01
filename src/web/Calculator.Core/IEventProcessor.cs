namespace FfAdmin.Calculator.Core;

public interface IEventProcessor
{
    bool IsRecursive { get; }
    object Start { get; }
    ValueTask<object> Process(object model, IContext previousContext, IContext context, Event e);
    Type ModelType { get; }
    IEnumerable<Type> Dependencies { get; }
}

public interface IEventProcessor<T> : IEventProcessor
    where T : class
{
    bool IEventProcessor.IsRecursive => true;
    object IEventProcessor.Start => Start;
    Type IEventProcessor.ModelType => typeof(T);
    async ValueTask<object> IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => await Process((T)model, previousContext, context, e);
    new T Start { get; }
    ValueTask<T> Process(T model, IContext previousContext, IContext context, Event e);
}
