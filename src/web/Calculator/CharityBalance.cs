namespace FfAdmin.Calculator;

public record CharityBalance(Real Amount) : IModel<CharityBalance>
{
    public static CharityBalance Empty { get; } = new((Real)0);
    public static IEventProcessor<CharityBalance> Processor { get; } = new ProcessorImpl();

    private class ProcessorImpl : EventProcessor<CharityBalance>
    {
        public override CharityBalance Start => Empty;

        protected override CharityBalance ConvExit(CharityBalance model, IContext context, ConvExit e)
        {
            return new(model.Amount + e.Amount);
        }

        protected override CharityBalance ConvTransfer(CharityBalance model, IContext context, ConvTransfer e)
        {
            return new(model.Amount - e.Amount);
        }
    }
}
