namespace FfAdmin.Calculator.Core;

public interface IEventProcessor
{
    bool IsRecursive { get; }
    ValueTask<object> Process(object model, IContext previousContext, IContext context, Event e);
    Type ModelType { get; }
    IEnumerable<Type> Dependencies { get; }
}

public interface IEventProcessor<T> : IEventProcessor
    where T : class
{
    bool IEventProcessor.IsRecursive => true;
    Type IEventProcessor.ModelType => typeof(T);
    async ValueTask<object> IEventProcessor.Process(object model, IContext previousContext, IContext context, Event e)
        => await Process((T)model, previousContext, context, e);
    ValueTask<T> Process(T model, IContext previousContext, IContext context, Event e);
}
