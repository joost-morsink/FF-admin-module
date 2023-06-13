namespace FfAdmin.Calculator.Core;

public interface IEventProcessor
{ 
    object Start { get; }
    object Process(object model, IHistoricContext historicContext, Event e);
    Type ModelType { get; }
    Delegate PositionalModelCreator(EventStream stream);
}

public interface IEventProcessor<T> : IEventProcessor
    where T : class
{
    new T Start { get; }
    T Process(T model, IHistoricContext context, Event e);
    new Func<int, T> PositionalModelCreator(EventStream stream);
}
