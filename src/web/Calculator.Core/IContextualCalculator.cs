namespace FfAdmin.Calculator.Core;

public interface IContextualCalculator<T> : IEventProcessor<T>
    where T : class
{
    bool IEventProcessor.IsRecursive => false;
    T Process(IContext previousContext, IContext context, Event e);
    T IEventProcessor<T>.Process(T model, IContext previousContext, IContext context, Event e)
        => Process(previousContext, context, e);
}
