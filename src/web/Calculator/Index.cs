using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record Index(int Value) : IModel<Index>
{
    public static Index Empty { get; } = new(0);
    public static IEventProcessor<Index> GetProcessor(IServiceProvider services) => new Impl();

    private class Impl : EventProcessor<Index>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
            => new Calc(previousContext, currentContext);

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override Index Default(Index model, Event e)
                => new(model.Value + 1);
        }
    }
}
