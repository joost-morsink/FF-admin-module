using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record CumulativeInterest(ImmutableDictionary<string, CumulativeInterest.DataPoint> Options) : IModel<CumulativeInterest>
{
    public static IMetaModel<CumulativeInterest> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<CumulativeInterest>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override CumulativeInterest Empty => new(ImmutableDictionary<string, DataPoint>.Empty);

        public override IEventProcessor<CumulativeInterest> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<ProcessorImpl>(serviceProvider);
    }
    public static implicit operator CumulativeInterest(ImmutableDictionary<string, DataPoint> dictionary)
        => new(dictionary);
    
    private class ProcessorImpl(IContext<OptionWorths2> cOptionWorths): EventProcessor<CumulativeInterest>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptionWorths);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<OptionWorths2> cOptionWorths) : BaseCalculation(previousContext, currentContext)
        {

            public ValueTask<OptionWorths2> CurrentOptionWorths => GetCurrent(cOptionWorths);
            public ValueTask<OptionWorths2> PreviousOptionWorths => GetPrevious(cOptionWorths);
            
            protected override async ValueTask<CumulativeInterest> NewOption(CumulativeInterest model, NewOption e)
            {
                return model.Options.Add(e.Code, new(1, e.Timestamp));
            }

            protected override async ValueTask<CumulativeInterest> ConvEnter(CumulativeInterest model, ConvEnter e)
            {
                var prevWorth = (await PreviousOptionWorths).Worths[e.Option];
                if (prevWorth.TotalWorth() == 0)
                    return model;
                var addedInterest = (e.Invested_amount - prevWorth.Invested) / prevWorth.TotalWorth();
                return model.Options.SetItem(e.Option, new(model.Options[e.Option].Value * (1 + addedInterest), e.Timestamp));
            }

            protected override ValueTask<CumulativeInterest> ConvInvest(CumulativeInterest model, ConvInvest e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }

            private async ValueTask<CumulativeInterest> CumulativeInterestBetweenContexts(CumulativeInterest model, string option, DateTimeOffset timestamp, Real difference)
            {
                var prevWorth = (await PreviousOptionWorths).Worths[option];
                var currWorth = (await CurrentOptionWorths).Worths[option];
                var addedInterest = (currWorth.TotalWorth() - difference - prevWorth.TotalWorth()) / prevWorth.TotalWorth();
                return model.Options.SetItem(option, new(model.Options[option].Value * (1 + addedInterest), timestamp));
            }

            protected override ValueTask<CumulativeInterest> PriceInfo(CumulativeInterest model, PriceInfo e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }
            protected override ValueTask<CumulativeInterest> ConvInflation(CumulativeInterest model, ConvInflation e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }

            protected override ValueTask<CumulativeInterest> ConvLiquidate(CumulativeInterest model, ConvLiquidate e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }

            protected override ValueTask<CumulativeInterest> ConvExit(CumulativeInterest model, ConvExit e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, -e.Amount);
            }

            protected override ValueTask<CumulativeInterest> IncreaseCash(CumulativeInterest model, IncreaseCash e)
            {
                return CumulativeInterestBetweenContexts(model, e.Option, e.Timestamp, 0);
            }
        }
    }
    public record DataPoint(Real Value, DateTimeOffset Timestamp);
}
