using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record OptionWorths(ImmutableDictionary<string, OptionWorth> Worths) : IModel<OptionWorths>
{
    public static implicit operator OptionWorths(ImmutableDictionary<string, OptionWorth> dict)
        => new(dict);
    public static OptionWorths Empty { get; } =
        new(ImmutableDictionary<string, OptionWorth>.Empty);

    public static IEventProcessor<OptionWorths> Processor { get; } = new Impl();

    public OptionWorths Mutate(string key, Func<OptionWorth, OptionWorth> mutator)
        => new(Worths.SetItem(key, mutator(Worths[key])));

    private class Impl : EventProcessor<OptionWorths>
    {
        public override OptionWorths Start => Empty;

        protected override OptionWorths NewDonation(OptionWorths model, IContext context, NewDonation e)
        {
            return model.Mutate(e.Option, option =>
            {
                var donation = new Donation(e.Donation, e.Timestamp, e.Execute_timestamp, e.Option, e.Charity,
                    (Real)e.Exchanged_amount);

                return option with {UnenteredDonations = option.UnenteredDonations.Add(donation)};
            });
        }
        protected override OptionWorths CancelDonation(OptionWorths model, IContext previous, IContext context, CancelDonation e)
        {
            var donation = previous.GetContext<Donations>().Values.GetValueOrDefault(e.Donation);
            if (donation is null)
                return model;
            
            return model.Mutate(donation.OptionId, option =>
            {
                var unenteredDonation = option.UnenteredDonations.FirstOrDefault(d => d.Id == e.Donation);
                if (unenteredDonation is null)
                {
                    if (option.DonationFractions.ContainsKey(e.Donation))
                        return option with {Cash = option.Cash - donation.Amount, DonationFractions = option.DonationFractions.Remove(e.Donation)}; 
                    return option;
                }
                return option with
                {
                    UnenteredDonations = option.UnenteredDonations.Remove(unenteredDonation)
                };
            });
        }

        protected override OptionWorths NewOption(OptionWorths model, IContext context, NewOption e)
        {
            var worth = new OptionWorth(e.Code, e.Timestamp, (Real)0, (Real)0, FractionSet.Empty,
                ImmutableList<Donation>.Empty);
            return model with {Worths = model.Worths.Add(e.Code, worth)};
        }

        protected override OptionWorths ConvEnter(OptionWorths model, IContext context, ConvEnter e)
        {
            return model.Mutate(e.Option, option =>
            {
                var oldWorth = (Real)(e.Invested_amount + option.Cash);

                var donations = option.UnenteredDonations.ToLookup(ue => ue.ExecuteTimestamp <= e.Timestamp);
                var newCash = donations[true].Sum(ue => ue.Amount);

                var fractions = model.Worths[e.Option].DonationFractions;
                fractions = fractions.AddRange(from don in donations[true]
                    select (don.Id, don.Amount / (oldWorth == 0 ? 1 : oldWorth)));

                return option with
                {
                    Cash = option.Cash + newCash,
                    Timestamp = e.Timestamp,
                    DonationFractions = fractions,
                    Invested = e.Invested_amount,
                    UnenteredDonations = donations[false].ToImmutableList()
                };
            });
        }

        protected override OptionWorths ConvInvest(OptionWorths model, IContext context, ConvInvest e)
            => model.Mutate(e.Option, option =>
                option with {Timestamp = e.Timestamp, Invested = e.Invested_amount, Cash = e.Cash_amount});

        protected override OptionWorths ConvLiquidate(OptionWorths model, IContext context, ConvLiquidate e)
            => model.Mutate(e.Option, option =>
                option with {Timestamp = e.Timestamp, Cash = e.Cash_amount, Invested = e.Invested_amount});

        protected override OptionWorths ConvExit(OptionWorths model, IContext context, ConvExit e)
            => model.Mutate(e.Option, option =>
                option with {Timestamp = e.Timestamp, Cash = option.Cash - e.Amount});

        protected override OptionWorths PriceInfo(OptionWorths model, IContext context, PriceInfo e)
            => model.Mutate(e.Option, option =>
                option with {Timestamp = e.Timestamp, Invested = e.Invested_amount});
        protected override OptionWorths IncreaseCash(OptionWorths model, IContext context, IncreaseCash e)
            => model.Mutate(e.Option, option =>
                option with {Timestamp = e.Timestamp, Cash = option.Cash + e.Amount});
    }
}

public record OptionWorth(string Id, DateTimeOffset Timestamp, Real Invested, Real Cash,
    FractionSet DonationFractions, ImmutableList<Donation> UnenteredDonations)
{
    public Real TotalWorth => Invested + Cash;
}
