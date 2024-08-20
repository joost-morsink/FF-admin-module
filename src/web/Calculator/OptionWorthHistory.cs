using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record OptionWorthHistory(ImmutableDictionary<string, ImmutableList<OptionWorthRecord>> Options) : IModel<OptionWorthHistory>
{
    public static implicit operator OptionWorthHistory(ImmutableDictionary<string, ImmutableList<OptionWorthRecord>> options)
        => new(options);

    public static OptionWorthHistory Empty { get; } =
        new(ImmutableDictionary<string, ImmutableList<OptionWorthRecord>>.Empty);

    public static IEventProcessor<OptionWorthHistory> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    public OptionWorthHistory Mutate(string key, Func<ImmutableList<OptionWorthRecord>, ImmutableList<OptionWorthRecord>> mutator)
        => new(Options.SetItem(key, mutator(Options[key])));

    public OptionWorthHistory Add(string key, OptionWorthRecord record)
        => Mutate(key, x => x.Add(record));

    private class Impl(IContext<OptionWorths> cOptionWorths, IContext<CumulativeInterest> cCumulativeInterest) : EventProcessor<OptionWorthHistory>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptionWorths, cCumulativeInterest);
        }

        private sealed class Calc(
            IContext previousContext,
            IContext currentContext,
            IContext<OptionWorths> cOptionWorths,
            IContext<CumulativeInterest> cCumulativeInterest)
            : BaseCalculation(previousContext, currentContext)
        {
            public OptionWorths CurrentOptionWorths => GetCurrent(cOptionWorths);
            public OptionWorths PreviousOptionWorths => GetPrevious(cOptionWorths);
            public CumulativeInterest CurrentCumulativeInterest => GetCurrent(cCumulativeInterest);
            public CumulativeInterest PreviousCumulativeInterest => GetPrevious(cCumulativeInterest);

            protected override OptionWorthHistory NewOption(OptionWorthHistory model, NewOption e)
            {
                return model.Options.Add(e.Code, ImmutableList.Create(
                    new OptionWorthRecord(e.Type, e.Timestamp, new Worth(0, 0, 0, 1), new Worth(0, 0, 0, 1))));
            }

            protected override OptionWorthHistory ConvEnter(OptionWorthHistory model, ConvEnter e)
                => AddRecord(model, e.Option, e);

            protected override OptionWorthHistory ConvInvest(OptionWorthHistory model, ConvInvest e)
                => AddRecord(model, e.Option, e);

            protected override OptionWorthHistory ConvLiquidate(OptionWorthHistory model, ConvLiquidate e)
                => AddRecord(model, e.Option, e);

            protected override OptionWorthHistory ConvExit(OptionWorthHistory model, ConvExit e)
                => AddRecord(model, e.Option, e);

            protected override OptionWorthHistory PriceInfo(OptionWorthHistory model, PriceInfo e)
                => AddRecord(model, e.Option, e);

            protected override OptionWorthHistory IncreaseCash(OptionWorthHistory model, IncreaseCash e)
                => AddRecord(model, e.Option, e);

            private OptionWorth GetCurrentWorth(string option)
                => CurrentOptionWorths.Worths[option];

            private OptionWorth GetPreviousWorth(string option)
                => PreviousOptionWorths.Worths[option];

            private CumulativeInterest.DataPoint GetCurrentCumulativeInterest(string option)
                => CurrentCumulativeInterest.Options[option];

            private CumulativeInterest.DataPoint GetPreviousCumulativeInterest(string option)
                => PreviousCumulativeInterest.Options[option];

            private OptionWorthHistory AddRecord(OptionWorthHistory model, string option, Event e)
            {
                var old = GetPreviousWorth(option);
                var @new = GetCurrentWorth(option);
                var oldCi = GetPreviousCumulativeInterest(option);
                var newCi = GetCurrentCumulativeInterest(option);
                return AddRecord(model, option, e, old, @new, oldCi.Value, newCi.Value);
            }

            private OptionWorthHistory AddRecord(OptionWorthHistory model, string option, Event e, OptionWorth old, OptionWorth @new, Real oldCi, Real newCi)
                => model.Add(option, new(e.Type, e.Timestamp,
                    new(old.Cash, old.Invested, old.UnenteredDonations.Where(d => d.Timestamp <= e.Timestamp).Sum(d => d.Amount), oldCi),
                    new(@new.Cash, @new.Invested, @new.UnenteredDonations.Where(d => d.Timestamp <= e.Timestamp).Sum(d => d.Amount), newCi)));
        }
    }
}

public record Worth(Real Cash, Real Invested, Real Unentered, Real CumulativeInterest);

public record OptionWorthRecord(EventType EventType, DateTimeOffset Timestamp, Worth Old, Worth New);
