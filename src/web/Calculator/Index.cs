namespace FfAdmin.Calculator;

public record Index(int Value) : IModel<Index>
{
    public static IMetaModel<Index> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<Index>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override Index Empty { get; } = new(0);
        public override IEventProcessor<Index> GetProcessor(IServiceProvider services) => new Impl();
    }

    private class Impl : EventProcessor<Index>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
            => new Calc(previousContext, currentContext);

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override async ValueTask<Index> Default(Index model, Event e)
                => new(model.Value + 1);
        }
    }
}
