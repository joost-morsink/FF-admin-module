using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record MinimalExits(ImmutableDictionary<string, Real> Exits) : IModel<MinimalExits>
{
    public static IMetaModel<MinimalExits> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<MinimalExits>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override MinimalExits Empty => new(ImmutableDictionary<string, Real>.Empty);
        public override IEventProcessor<MinimalExits> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
    }
    public static implicit operator MinimalExits(ImmutableDictionary<string, Real> exits)
        => new(exits);
    
    private class Impl(IContext<Options> cOptions, IContext<IdealOptionValuations> cIdealOptionValuations) : EventProcessor<MinimalExits>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptions, cIdealOptionValuations);
        }

        private sealed class Calc(
            IContext previousContext,
            IContext currentContext,
            IContext<Options> cOptions,
            IContext<IdealOptionValuations> cIdealOptionValuations) : BaseCalculation(previousContext, currentContext)
        {
            public ValueTask<Options> CurrentOptions => GetCurrent(cOptions);
            public ValueTask<IdealOptionValuations> CurrentIdealOptionValuations => GetCurrent(cIdealOptionValuations);

            protected override ValueTask<MinimalExits> ConvLiquidate(MinimalExits model, ConvLiquidate e) 
                => CalculateNewMinimalExits(model, e.Option, e.Timestamp);

            protected override ValueTask<MinimalExits> PriceInfo(MinimalExits model, PriceInfo e)
                => CalculateNewMinimalExits(model, e.Option, e.Timestamp);

            protected override ValueTask<MinimalExits> IncreaseCash(MinimalExits model, IncreaseCash e)
                => CalculateNewMinimalExits(model, e.Option, e.Timestamp);

            private async ValueTask<MinimalExits> CalculateNewMinimalExits(MinimalExits model, string optionId, DateTimeOffset timestamp)
            {
                var option = (await CurrentOptions).Values[optionId];
                var valuations = (await CurrentIdealOptionValuations).Valuations[optionId];
                var yearsSinceLastExit = (timestamp - valuations.Timestamp).TotalDays / 365.25;
                var percentage = Math.Pow(1 + (double)option.BadYearFraction, yearsSinceLastExit) - 1;
                var minExit = valuations.RealValue * (Real)percentage;
                return new(model.Exits.SetItem(optionId, minExit));
            }
        }
    }
}

