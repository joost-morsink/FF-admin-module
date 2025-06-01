namespace FfAdmin.Calculator.Core;

public interface IContextualCalculator<T> : IEventProcessor<T>
    where T : class
{
    bool IEventProcessor.IsRecursive => false;
    ValueTask<T> Process(IContext previousContext, IContext context, Event e);
    ValueTask<T> IEventProcessor<T>.Process(T model, IContext previousContext, IContext context, Event e)
        => Process(previousContext, context, e);
}
