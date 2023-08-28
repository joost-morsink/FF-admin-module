namespace FfAdmin.Calculator;

public record CumulativeInterest(ImmutableDictionary<string, CumulativeInterest.DataPoint> Options) : IModel<CumulativeInterest>
{
    public static implicit operator CumulativeInterest(ImmutableDictionary<string, DataPoint> dictionary)
        => new(dictionary);

    public static CumulativeInterest Empty { get; } = new(ImmutableDictionary<string, DataPoint>.Empty);
    public static IEventProcessor<CumulativeInterest> Processor { get; } = new ProcessorImpl();

    private class ProcessorImpl : EventProcessor<CumulativeInterest>
    {
        public override CumulativeInterest Start => Empty;

        protected override CumulativeInterest NewOption(CumulativeInterest model, IContext context, NewOption e)
        {
            return model.Options.Add(e.Code, new (1,e.Timestamp));
        }

        protected override CumulativeInterest ConvEnter(CumulativeInterest model, IContext previousContext,
            IContext context, ConvEnter e)
        {
            var prevWorth = previousContext.GetContext<OptionWorths>().Worths[e.Option];
            if (prevWorth.TotalWorth == 0)
                return model;
            var addedInterest = (e.Invested_amount - prevWorth.Invested) / prevWorth.TotalWorth;
            return model.Options.SetItem(e.Option, new(model.Options[e.Option].Value * (1 + addedInterest), e.Timestamp));
        }

        protected override CumulativeInterest ConvInvest(CumulativeInterest model, IContext previousContext,
            IContext context, ConvInvest e)
        {
            return CumulativeInterestBetweenContexts(model, previousContext, context, e.Option, e.Timestamp,0);
        }

        private static CumulativeInterest CumulativeInterestBetweenContexts(CumulativeInterest model,
            IContext previousContext,
            IContext context, string option, DateTimeOffset timestamp, Real difference)
        {
            var prevWorth = previousContext.GetContext<OptionWorths>().Worths[option];
            var currWorth = context.GetContext<OptionWorths>().Worths[option];
            var addedInterest = (currWorth.TotalWorth - difference - prevWorth.TotalWorth) / prevWorth.TotalWorth;
            return model.Options.SetItem(option, new (model.Options[option].Value * (1 + addedInterest), timestamp));
        }

        protected override CumulativeInterest ConvLiquidate(CumulativeInterest model, IContext previousContext,
            IContext context, ConvLiquidate e)
        {
            return CumulativeInterestBetweenContexts(model, previousContext, context, e.Option, e.Timestamp, 0);
        }

        protected override CumulativeInterest ConvExit(CumulativeInterest model, IContext previousContext,
            IContext context, ConvExit e)
        {
            return CumulativeInterestBetweenContexts(model, previousContext, context, e.Option, e.Timestamp, -e.Amount);
        }
    }
    public record DataPoint(Real Value, DateTimeOffset Timestamp);
}
