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

    private class Impl(
        IContext<OptionWorths> cOptionWorths,
        IContext<CumulativeInterest> cCumulativeInterest,
        IContext<IdealOptionValuations> cIdealOptionValuations) : EventProcessor<OptionWorthHistory>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptionWorths, cCumulativeInterest, cIdealOptionValuations);
        }

        private sealed class Calc(
            IContext previousContext,
            IContext currentContext,
            IContext<OptionWorths> cOptionWorths,
            IContext<CumulativeInterest> cCumulativeInterest,
            IContext<IdealOptionValuations> cIdealOptionValuations)
            : BaseCalculation(previousContext, currentContext)
        {
            public ValueTask<OptionWorths> CurrentOptionWorths => GetCurrent(cOptionWorths);
            public ValueTask<OptionWorths> PreviousOptionWorths => GetPrevious(cOptionWorths);
            public ValueTask<CumulativeInterest> CurrentCumulativeInterest => GetCurrent(cCumulativeInterest);
            public ValueTask<CumulativeInterest> PreviousCumulativeInterest => GetPrevious(cCumulativeInterest);
            public ValueTask<IdealOptionValuations> CurrentIdealOptionValuations => GetCurrent(cIdealOptionValuations);
            public ValueTask<IdealOptionValuations> PreviousIdealOptionValuations => GetPrevious(cIdealOptionValuations);

            protected override async ValueTask<OptionWorthHistory> NewOption(OptionWorthHistory model, NewOption e)
            {
                return model.Options.Add(e.Code, ImmutableList.Create(
                    new OptionWorthRecord(e.Type, e.Timestamp, new Worth(0, 0, 0, 1, 0, 0), new Worth(0, 0, 0, 1, 0, 0))));
            }

            protected override ValueTask<OptionWorthHistory> ConvEnter(OptionWorthHistory model, ConvEnter e)
                => AddRecord(model, e.Option, e);

            protected override ValueTask<OptionWorthHistory> ConvInvest(OptionWorthHistory model, ConvInvest e)
                => AddRecord(model, e.Option, e);

            protected override ValueTask<OptionWorthHistory> ConvLiquidate(OptionWorthHistory model, ConvLiquidate e)
                => AddRecord(model, e.Option, e);

            protected override ValueTask<OptionWorthHistory> ConvExit(OptionWorthHistory model, ConvExit e)
                => AddRecord(model, e.Option, e);

            protected override ValueTask<OptionWorthHistory> ConvInflation(OptionWorthHistory model, ConvInflation e)
                => AddRecord(model, e.Option, e);

            protected override ValueTask<OptionWorthHistory> PriceInfo(OptionWorthHistory model, PriceInfo e)
                => AddRecord(model, e.Option, e);

            protected override ValueTask<OptionWorthHistory> IncreaseCash(OptionWorthHistory model, IncreaseCash e)
                => AddRecord(model, e.Option, e);

            private async ValueTask<OptionWorth> GetCurrentWorth(string option)
                => (await CurrentOptionWorths).Worths[option];

            private async ValueTask<OptionWorth> GetPreviousWorth(string option)
                => (await PreviousOptionWorths).Worths[option];

            private async ValueTask<CumulativeInterest.DataPoint> GetCurrentCumulativeInterest(string option)
                => (await CurrentCumulativeInterest).Options[option];

            private async ValueTask<CumulativeInterest.DataPoint> GetPreviousCumulativeInterest(string option)
                => (await PreviousCumulativeInterest).Options[option];

            private async ValueTask<IdealValuation> GetCurrentIdealValuation(string option)
                => (await CurrentIdealOptionValuations).Valuations[option];

            private async ValueTask<IdealValuation> GetPreviousIdealValuation(string option)
                => (await PreviousIdealOptionValuations).Valuations.GetValueOrDefault(option)
                   ?? new (DateTimeOffset.MinValue, 0,0);

            private async ValueTask<OptionWorthHistory> AddRecord(OptionWorthHistory model, string option, Event e)
            {
                var old = await GetPreviousWorth(option);
                var @new = await GetCurrentWorth(option);
                var oldCi = await GetPreviousCumulativeInterest(option);
                var newCi = await GetCurrentCumulativeInterest(option);
                var oldIv = await GetPreviousIdealValuation(option);
                var newIv = await GetCurrentIdealValuation(option);
                return AddRecord(model, option, e, old, @new, oldCi.Value, newCi.Value, oldIv, newIv);
            }

            private OptionWorthHistory AddRecord(OptionWorthHistory model, string option, Event e, OptionWorth old, OptionWorth @new, Real oldCi, Real newCi,
                IdealValuation oldIv, IdealValuation newIv)
                => model.Add(option, new(e.Type, e.Timestamp,
                    new(old.Cash, old.Invested, old.UnenteredDonations.Where(d => d.Timestamp <= e.Timestamp).Sum(d => d.Amount), oldCi, oldIv.IdealValue,
                        oldIv.RealValue),
                    new(@new.Cash, @new.Invested, @new.UnenteredDonations.Where(d => d.Timestamp <= e.Timestamp).Sum(d => d.Amount), newCi, newIv.IdealValue,
                        newIv.RealValue)));
        }
    }
}

public record Worth(Real Cash, Real Invested, Real Unentered, Real CumulativeInterest, Real IdealValue, Real Value);

public record OptionWorthRecord(EventType EventType, DateTimeOffset Timestamp, Worth Old, Worth New);
