namespace FfAdmin.Calculator.Core;

public interface IEventProcessor
{
    bool IsRecursive { get; }
    object Start { get; }
    object Process(object model, IHistoricContext historicContext, Event e);
    Type ModelType { get; }
    Delegate PositionalModelCreator(EventStream stream);
}

public interface IEventProcessor<T> : IEventProcessor
    where T : class
{
    bool IEventProcessor.IsRecursive => true;
    new T Start { get; }
    T Process(T model, IHistoricContext context, Event e);
    new Func<int, T> PositionalModelCreator(EventStream stream);
}

public interface IContextualCalculator<T> : IEventProcessor<T>
    where T : class
{
    bool IEventProcessor.IsRecursive => false;
    T Process(IHistoricContext context, Event e);
    T IEventProcessor<T>.Process(T model, IHistoricContext context, Event e)
        => Process(context, e);
}
