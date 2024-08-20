namespace FfAdmin.Calculator;

public record CharityBalance(Real Amount) : IModel<CharityBalance>
{
    public static CharityBalance Empty { get; } = new((Real)0);
    public static IEventProcessor<CharityBalance> GetProcessor(IServiceProvider services) => new Impl();

    private class Impl : EventProcessor<CharityBalance>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override CharityBalance ConvExit(CharityBalance model, ConvExit e)
            {
                return new(model.Amount + e.Amount);
            }

            protected override CharityBalance ConvTransfer(CharityBalance model, ConvTransfer e)
            {
                return new(model.Amount - e.Amount);
            }
        }
    }
}
