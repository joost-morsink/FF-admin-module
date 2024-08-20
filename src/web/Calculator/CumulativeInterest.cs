using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record CumulativeInterest(ImmutableDictionary<string, CumulativeInterest.DataPoint> Options) : IModel<CumulativeInterest>
{
    public static implicit operator CumulativeInterest(ImmutableDictionary<string, DataPoint> dictionary)
        => new(dictionary);

    public static CumulativeInterest Empty { get; } = new(ImmutableDictionary<string, DataPoint>.Empty);

    public static IEventProcessor<CumulativeInterest> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<ProcessorImpl>(services);

    private class ProcessorImpl(IContext<OptionWorths> cOptionWorths): EventProcessor<CumulativeInterest>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptionWorths);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<OptionWorths> cOptionWorths) : BaseCalculation(previousContext, currentContext)
        {

            public OptionWorths CurrentOptionWorths => GetCurrent(cOptionWorths);
            public OptionWorths PreviousOptionWorths => GetPrevious(cOptionWorths);
            
            protected override CumulativeInterest NewOption(CumulativeInterest model, NewOption e)
            {
                return model.Options.Add(e.Code, new(1, e.Timestamp));
            }

            protected override CumulativeInterest ConvEnter(CumulativeInterest model, ConvEnter e)
            {
                var prevWorth = PreviousOptionWorths.Worths[e.Option];
                if (prevWorth.TotalWorth == 0)
                    return model;
                var addedInterest = (e.Invested_amount - prevWorth.Invested) / prevWorth.TotalWorth;
                return model.Options.SetItem(e.Option, new(model.Options[e.Option].Value * (1 + addedInterest), e.Timestamp));
            }

            protected override CumulativeInterest ConvInvest(CumulativeInterest model, ConvInvest e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }

            private CumulativeInterest CumulativeInterestBetweenContexts(CumulativeInterest model, string option, DateTimeOffset timestamp, Real difference)
            {
                var prevWorth = PreviousOptionWorths.Worths[option];
                var currWorth = CurrentOptionWorths.Worths[option];
                var addedInterest = (currWorth.TotalWorth - difference - prevWorth.TotalWorth) / prevWorth.TotalWorth;
                return model.Options.SetItem(option, new(model.Options[option].Value * (1 + addedInterest), timestamp));
            }

            protected override CumulativeInterest PriceInfo(CumulativeInterest model, PriceInfo e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }

            protected override CumulativeInterest ConvLiquidate(CumulativeInterest model, ConvLiquidate e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }

            protected override CumulativeInterest ConvExit(CumulativeInterest model, ConvExit e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, -e.Amount);
            }

            protected override CumulativeInterest IncreaseCash(CumulativeInterest model, IncreaseCash e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }
        }
    }
    public record DataPoint(Real Value, DateTimeOffset Timestamp);
}
