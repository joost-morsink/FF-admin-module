using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record Index(int Value) : IModel<Index>
{
    public static Index Empty { get; } = new(0);
    public static IEventProcessor<Index> Processor { get; } = new Impl();

    private class Impl : EventProcessor<Index>
    {
        public override Index Start => Empty;

        protected override Index Default(Index model, IContext context, Event e)
            => new(model.Value + 1);
    }
}
