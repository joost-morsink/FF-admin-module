using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record IdealOptionValuations(ImmutableDictionary<string, IdealValuation> Valuations) : IModel<IdealOptionValuations>
{
    public static IMetaModel<IdealOptionValuations> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<IdealOptionValuations>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override IdealOptionValuations Empty => new(ImmutableDictionary<string, IdealValuation>.Empty);
        public override IEventProcessor<IdealOptionValuations> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
    }
    public static implicit operator IdealOptionValuations(ImmutableDictionary<string, IdealValuation> values)
        => new(values);
    public IdealOptionValuations Mutate(string option, Func<IdealValuation, IdealValuation> mutator,
        DateTimeOffset defaultTimestamp)
        => new(Valuations.SetItem(option,
            mutator(Valuations.TryGetValue(option, out var valuation)
                ? valuation
                : IdealValuation.Empty with {Timestamp = defaultTimestamp})));

    private class Impl(IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths) : EventProcessor<IdealOptionValuations>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptions, cOptionWorths);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths)
            : BaseCalculation(previousContext, currentContext)
        {
            public ValueTask<Options> CurrentOptions => GetCurrent(cOptions);
            public ValueTask<OptionWorths> CurrentOptionWorths => GetCurrent(cOptionWorths);
            public ValueTask<OptionWorths> PreviousOptionWorths => GetPrevious(cOptionWorths);
            
            // On ConvEnter, the cash is added to the real value and the ideal value.
            // But we also register price information on the invested amount.
            protected override async ValueTask<IdealOptionValuations> ConvEnter(IdealOptionValuations model, ConvEnter e)
            {
                var currentOptionWorths = await CurrentOptionWorths;
                var previousOptionWorths = await PreviousOptionWorths;
                var addedCash = currentOptionWorths.Worths[e.Option].Cash
                                - previousOptionWorths.Worths[e.Option].Cash;
                var addedWorth = currentOptionWorths.Worths[e.Option].TotalWorth
                                 - previousOptionWorths.Worths[e.Option].TotalWorth;
                var profit = addedWorth - addedCash;
                var reinvestmentFraction = (await CurrentOptions).Values[e.Option].ReinvestmentFraction;

                return model.Mutate(e.Option,
                    option => option with
                    {
                        RealValue = option.RealValue + addedCash + profit, 
                        IdealValue = option.IdealValue + addedCash + profit * reinvestmentFraction
                    }, e.Timestamp);
            }

            // On ConvInvest, the added (or subtracted if negative) worth is added to the real value.
            // The ideal value should change according to the reinvestment fraction for the option.
            protected override async ValueTask<IdealOptionValuations> ConvInvest(IdealOptionValuations model, ConvInvest e)
            {
                var currentOptionWorths = await CurrentOptionWorths;
                var previousOptionWorths = await PreviousOptionWorths;

                var addedWorth = currentOptionWorths.Worths[e.Option].TotalWorth
                                 - previousOptionWorths.Worths[e.Option].TotalWorth;
                var reinvestmentFraction = (await CurrentOptions).Values[e.Option].ReinvestmentFraction;
                
                return model.Mutate(e.Option,
                    option => option with {RealValue = option.RealValue + addedWorth, IdealValue = option.IdealValue + addedWorth * reinvestmentFraction},
                    e.Timestamp);
            }

            protected override ValueTask<IdealOptionValuations> ConvLiquidate(IdealOptionValuations model, ConvLiquidate e)
                => RecalculateValuations(model, e.Option, e.Timestamp);
            protected override ValueTask<IdealOptionValuations> IncreaseCash(IdealOptionValuations model, IncreaseCash e)
                => RecalculateValuations(model, e.Option, e.Timestamp);
            // On ConvLiquidate and PriceInfo, the added (or subtracted if negative) worth is added from the real value.
            // The ideal value should change according to the reinvestment fraction for the option.
            private async ValueTask<IdealOptionValuations> RecalculateValuations(IdealOptionValuations model, string option, DateTimeOffset timestamp)
            {
                var addedWorth = await AddedWorth(option);
                var reinvestmentFraction = (await CurrentOptions).Values[option].ReinvestmentFraction;

                return model.Mutate(option,
                    option => option with {RealValue = option.RealValue + addedWorth, IdealValue = option.IdealValue + addedWorth * reinvestmentFraction},
                    timestamp);
            }

            private async ValueTask<decimal> AddedWorth(string option)
            {
                return (await CurrentOptionWorths).Worths[option].TotalWorth
                       - (await PreviousOptionWorths).Worths[option].TotalWorth;
            }

            protected override ValueTask<IdealOptionValuations> PriceInfo(IdealOptionValuations model, PriceInfo e)
                => RecalculateValuations(model, e.Option, e.Timestamp);

            // On ConvExit, the cash is subtracted from the real value, but not the ideal value. Ideally to equalize the real and ideal values.
            protected override async ValueTask<IdealOptionValuations> ConvExit(IdealOptionValuations model, ConvExit e)
            {
                var subtractedCash = e.Amount;
                return model.Mutate(e.Option,
                    option => option with {Timestamp = e.Timestamp, RealValue = option.RealValue - subtractedCash},
                    e.Timestamp);
            }

            // On ConvInflation, the ideal value is multiplied by the inflation factor.
            protected override async ValueTask<IdealOptionValuations> ConvInflation(IdealOptionValuations model, ConvInflation e)
            {
                var iov = await RecalculateValuations(model, e.Option, e.Timestamp);
                return iov.Mutate(e.Option, iv => iv with {IdealValue = iv.IdealValue * e.Inflation_factor}, e.Timestamp);
            }
        }
    }
}

public record IdealValuation(DateTimeOffset Timestamp, Real RealValue, Real IdealValue)
{
    public static IdealValuation Empty { get; } = new(DateTimeOffset.MinValue, (Real)0, (Real)0);
}
