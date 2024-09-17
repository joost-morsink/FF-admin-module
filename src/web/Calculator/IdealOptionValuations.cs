using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record IdealOptionValuations(ImmutableDictionary<string, IdealValuation> Valuations) : IModel<IdealOptionValuations>
{
    public static implicit operator IdealOptionValuations(ImmutableDictionary<string, IdealValuation> values)
        => new(values);
    public static IdealOptionValuations Empty { get; } = new(ImmutableDictionary<string, IdealValuation>.Empty);

    public static IEventProcessor<IdealOptionValuations> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);
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
            public Options CurrentOptions => GetCurrent(cOptions);
            public OptionWorths CurrentOptionWorths => GetCurrent(cOptionWorths);
            public OptionWorths PreviousOptionWorths => GetPrevious(cOptionWorths);
            
            // On ConvEnter, the cash is added to the real value and the ideal value.
            protected override IdealOptionValuations ConvEnter(IdealOptionValuations model, ConvEnter e)
            {
                var addedCash = CurrentOptionWorths.Worths[e.Option].Cash
                                - PreviousOptionWorths.Worths[e.Option].Cash;
                return model.Mutate(e.Option,
                    option => option with {RealValue = option.RealValue + addedCash, IdealValue = option.IdealValue + addedCash}, e.Timestamp);
            }

            // On ConvInvest, the added (or subtracted if negative) worth is added to the real value.
            // The ideal value should change according to the reinvestment fraction for the option.
            protected override IdealOptionValuations ConvInvest(IdealOptionValuations model, ConvInvest e)
            {
                var addedWorth = CurrentOptionWorths.Worths[e.Option].TotalWorth
                                 - PreviousOptionWorths.Worths[e.Option].TotalWorth;
                var reinvestmentFraction = CurrentOptions.Values[e.Option].ReinvestmentFraction;
                
                return model.Mutate(e.Option,
                    option => option with {RealValue = option.RealValue + addedWorth, IdealValue = option.IdealValue + addedWorth * reinvestmentFraction},
                    e.Timestamp);
            }

            protected override IdealOptionValuations ConvLiquidate(IdealOptionValuations model, ConvLiquidate e)
                => RecalculateValuations(model, e.Option, e.Timestamp);

            // On ConvLiquidate and PriceInfo, the added (or subtracted if negative) worth is added from the real value.
            // The ideal value should change according to the reinvestment fraction for the option.
            private IdealOptionValuations RecalculateValuations(IdealOptionValuations model, string option, DateTimeOffset timestamp)
            {
                var addedWorth = AddedWorth(option);
                var reinvestmentFraction = CurrentOptions.Values[option].ReinvestmentFraction;

                return model.Mutate(option,
                    option => option with {RealValue = option.RealValue + addedWorth, IdealValue = option.IdealValue + addedWorth * reinvestmentFraction},
                    timestamp);
            }

            private decimal AddedWorth(string option)
            {
                return CurrentOptionWorths.Worths[option].TotalWorth
                       - PreviousOptionWorths.Worths[option].TotalWorth;
            }

            protected override IdealOptionValuations PriceInfo(IdealOptionValuations model, PriceInfo e)
                => RecalculateValuations(model, e.Option, e.Timestamp);

            // On ConvExit, the cash is subtracted from the real value, but not the ideal value. Ideally to equalize the real and ideal values.
            protected override IdealOptionValuations ConvExit(IdealOptionValuations model, ConvExit e)
            {
                var subtractedCash = e.Amount;
                return model.Mutate(e.Option,
                    option => option with {Timestamp = e.Timestamp, RealValue = option.RealValue - subtractedCash},
                    e.Timestamp);
            }

            // On ConvInflation, the ideal value is multiplied by the inflation factor.
            protected override IdealOptionValuations ConvInflation(IdealOptionValuations model, ConvInflation e)
            {
                var iov = RecalculateValuations(model, e.Option, e.Timestamp);
                return iov.Mutate(e.Option, iv => iv with {IdealValue = iv.IdealValue * e.Inflation_factor}, e.Timestamp);
            }
        }
    }
}

public record IdealValuation(DateTimeOffset Timestamp, Real RealValue, Real IdealValue)
{
    public static IdealValuation Empty { get; } = new(DateTimeOffset.MinValue, (Real)0, (Real)0);
}
