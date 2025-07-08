namespace FfAdmin.Calculator;

public record CharityBalance(Real Amount) : IModel<CharityBalance>
{
    public static IMetaModel<CharityBalance> GetMetaModel()
        => Meta.Instance;

    private class Meta : IModel<CharityBalance>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override CharityBalance Empty { get; } = new((Real)0);
        public override IEventProcessor<CharityBalance> GetProcessor(IServiceProvider services) => new Impl();
    }

    private class Impl : EventProcessor<CharityBalance>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override async ValueTask<CharityBalance> ConvExit(CharityBalance model, ConvExit e)
            {
                return new(model.Amount + e.Amount);
            }

            protected override async ValueTask<CharityBalance> ConvTransfer(CharityBalance model, ConvTransfer e)
            {
                return new(model.Amount - e.Amount);
            }
        }
    }
}
