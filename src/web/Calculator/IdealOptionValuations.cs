using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record IdealOptionValuations(ImmutableDictionary<string, IdealValuation> Valuations)
{
    public static IdealOptionValuations Empty { get; } = new(ImmutableDictionary<string, IdealValuation>.Empty);

    public static IEventProcessor<IdealOptionValuations> Processor { get; } = new Impl();
    public IdealOptionValuations Mutate(string option, Func<IdealValuation, IdealValuation> mutator,
        DateTimeOffset defaultTimestamp)
        => new(Valuations.SetItem(option,
            mutator(Valuations.TryGetValue(option, out var valuation)
                ? valuation
                : IdealValuation.Empty with {Timestamp = defaultTimestamp})));

    private class Impl : EventProcessor<IdealOptionValuations>
    {
        public override IdealOptionValuations Start => IdealOptionValuations.Empty;

        // On ConvEnter, the cash is added to the real value and the ideal value.
        protected override IdealOptionValuations ConvEnter(IdealOptionValuations model,
            IHistoricContext historicContext,
            ConvEnter e)
        {
            var addedCash = historicContext.Current.GetContext<OptionWorths>()
                .Worths[e.Option].Cash - historicContext.Previous.GetContext<OptionWorths>().Worths[e.Option].Cash;
            return model.Mutate(e.Option,
                option => option with
                {
                    RealValue = option.RealValue + addedCash, IdealValue = option.IdealValue + addedCash
                }, e.Timestamp);
        }

        // On ConvInvest, the added (or subtracted if negative) worth is added to the real value.
        // The ideal value should change according to the reinvestment fraction for the option.
        protected override IdealOptionValuations ConvInvest(IdealOptionValuations model,
            IHistoricContext historicContext,
            ConvInvest e)
        {
            var addedWorth = historicContext.Current.GetContext<OptionWorths>()
                                 .Worths[e.Option].TotalWorth -
                             historicContext.Previous.GetContext<OptionWorths>().Worths[e.Option].TotalWorth;
            var reinvestmentFraction = historicContext.Current.GetContext<Options>()
                .Values[e.Option].ReinvestmentFraction;
            return model.Mutate(e.Option,
                option => option with
                {
                    RealValue = option.RealValue + addedWorth,
                    IdealValue = option.IdealValue + addedWorth * reinvestmentFraction
                }, e.Timestamp);
        }

        // On ConvLiquidate, the added (or subtracted if negative) worth is added from the real value.
        // The ideal value should change according to the reinvestment fraction for the option.
        protected override IdealOptionValuations ConvLiquidate(IdealOptionValuations model,
            IHistoricContext historicContext, ConvLiquidate e)
        {
            var addedWorth = historicContext.Current.GetContext<OptionWorths>()
                                 .Worths[e.Option].TotalWorth -
                             historicContext.Previous.GetContext<OptionWorths>().Worths[e.Option].TotalWorth;
            var reinvestmentFraction = historicContext.Current.GetContext<Options>()
                .Values[e.Option].ReinvestmentFraction;

            return model.Mutate(e.Option,
                option => option with
                {
                    RealValue = option.RealValue + addedWorth,
                    IdealValue = option.IdealValue + addedWorth * reinvestmentFraction
                }, e.Timestamp);
        }

        // On ConvExit, the cash is subtracted from the real value, but not the ideal value. Ideally to equalize the real and ideal values.
        protected override IdealOptionValuations ConvExit(IdealOptionValuations model, IHistoricContext historicContext,
            ConvExit e)
        {
            var subtractedCash = e.Amount;
            return model.Mutate(e.Option,
                option => option with {Timestamp = e.Timestamp, RealValue = option.RealValue - subtractedCash},
                e.Timestamp);
        }
    }
}

public record IdealValuation(DateTimeOffset Timestamp, Real RealValue, Real IdealValue)
{
    public static IdealValuation Empty { get; } = new(DateTimeOffset.MinValue, (Real)0, (Real)0);
}
