using System.Text.Json.Serialization;

namespace FfAdmin.Calculator;

public record OptionWorthHistory(ImmutableDictionary<string, ImmutableList<OptionWorthRecord>> Options) : IModel<OptionWorthHistory>
{
    public static implicit operator OptionWorthHistory(ImmutableDictionary<string, ImmutableList<OptionWorthRecord>> options)
        => new(options);
    public static OptionWorthHistory Empty { get; } =
        new(ImmutableDictionary<string, ImmutableList<OptionWorthRecord>>.Empty);

    public static IEventProcessor<OptionWorthHistory> Processor { get; } = new Impl();

    public OptionWorthHistory Mutate(string key, Func<ImmutableList<OptionWorthRecord>, ImmutableList<OptionWorthRecord>> mutator)
        => new(Options.SetItem(key, mutator(Options[key])));

    public OptionWorthHistory Add(string key, OptionWorthRecord record)
        => Mutate(key, x => x.Add(record));
    private class Impl : EventProcessor<OptionWorthHistory>
    {
        public override OptionWorthHistory Start => Empty;

        protected override OptionWorthHistory NewOption(OptionWorthHistory model, IContext context, NewOption e)
        {
            return model.Options.Add(e.Code, ImmutableList.Create(
                new OptionWorthRecord(e.Type, e.Timestamp, new Worth(0,0,0, 1), new Worth(0,0,0,1))));
        }

        protected override OptionWorthHistory ConvEnter(OptionWorthHistory model, IContext previousContext, IContext context, ConvEnter e)
            => AddRecord(model, e.Option, e, previousContext, context);

        protected override OptionWorthHistory ConvInvest(OptionWorthHistory model, IContext previousContext,
            IContext context, ConvInvest e)
            => AddRecord(model, e.Option, e, previousContext, context);

        protected override OptionWorthHistory ConvLiquidate(OptionWorthHistory model, IContext previousContext,
            IContext context, ConvLiquidate e)
            => AddRecord(model, e.Option, e, previousContext, context);

        protected override OptionWorthHistory ConvExit(OptionWorthHistory model, IContext previousContext,
            IContext context, ConvExit e)
            => AddRecord(model, e.Option, e, previousContext, context);

        protected override OptionWorthHistory PriceInfo(OptionWorthHistory model, IContext previousContext,
            IContext context, PriceInfo e)
            => AddRecord(model, e.Option, e, previousContext, context);

        protected override OptionWorthHistory IncreaseCash(OptionWorthHistory model, IContext previousContext,
            IContext context, IncreaseCash e)
            => AddRecord(model, e.Option, e, previousContext, context);
        
        private OptionWorth GetWorth(IContext context, string option)
            => context.GetContext<OptionWorths>().Worths[option];
        private CumulativeInterest.DataPoint GetCumulativeInterest(IContext context, string option)
            => context.GetContext<CumulativeInterest>().Options[option];
        private OptionWorthHistory AddRecord(OptionWorthHistory model, string option, Event e, IContext previousContext,
            IContext context)
        {
            var old = GetWorth(previousContext, option);
            var @new = GetWorth(context, option);
            var oldCi = GetCumulativeInterest(previousContext, option);
            var newCi = GetCumulativeInterest(context, option);
            return AddRecord(model, option, e, old, @new, oldCi.Value, newCi.Value);
        }
        private OptionWorthHistory AddRecord(OptionWorthHistory model, string option, Event e, OptionWorth old, OptionWorth @new, Real oldCi, Real newCi)
            => model.Add(option, new(e.Type, e.Timestamp,
                new(old.Cash, old.Invested, old.UnenteredDonations.Where(d => d.Timestamp <= e.Timestamp).Sum(d => d.Amount), oldCi),
                new(@new.Cash, @new.Invested, @new.UnenteredDonations.Where(d => d.Timestamp <= e.Timestamp).Sum(d => d.Amount), newCi)));
    }
}

public record Worth(Real Cash, Real Invested, Real Unentered, Real CumulativeInterest);

public record OptionWorthRecord(EventType EventType, DateTimeOffset Timestamp, Worth Old, Worth New);
