using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record DonationRecords(ImmutableDictionary<string, ImmutableList<DonationRecord>> Values) : IModel<DonationRecords>
{
    public static implicit operator DonationRecords(ImmutableDictionary<string, ImmutableList<DonationRecord>> values)
        => new(values);
    public static DonationRecords Empty { get; } = new(ImmutableDictionary<string, ImmutableList<DonationRecord>>.Empty);

    public static IEventProcessor<DonationRecords> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    private class Impl(IContext<Donations> cDonations, IContext<OptionWorths> cOptionWorths) : EventProcessor<DonationRecords>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cDonations, cOptionWorths);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Donations> cDonations, IContext<OptionWorths> cOptionWorths)
            : BaseCalculation(previousContext, currentContext)
        {
            public Donations CurrentDonations => GetCurrent(cDonations);
            public OptionWorths CurrentOptionWorths => GetCurrent(cOptionWorths);


        protected override DonationRecords NewDonation(DonationRecords model, NewDonation e)
        {
            var rec = new DonationRecord(e.Timestamp, e.Exchanged_amount, null);
            var newValues = model.Values.Add(e.Donation, rec);
            return new(newValues);
        }

        protected override DonationRecords CancelDonation(DonationRecords model, CancelDonation e)
        {
            var newValues = model.Values.SetItem(e.Donation, model.Values[e.Donation].Add(new DonationRecord(e.Timestamp, 0, null)));
            return new(newValues);
        }

        protected override DonationRecords ConvExit(DonationRecords model, ConvExit e)
        {
            var donations = CurrentDonations.Values;
            var option = CurrentOptionWorths.Worths[e.Option]!;
            var newModelValues = option.DonationFractions.Aggregate(model.Values,
                (acc, donationFraction) => acc.Add(donationFraction.Key,
                    new DonationRecord(e.Timestamp, donationFraction.Value * option.TotalWorth,
                        new Allocation(donations[donationFraction.Key].CharityId, donationFraction.Value * e.Amount))));
            return new(newModelValues);
        }

        private DonationRecords Rebalance(DonationRecords model, DateTimeOffset timestamp, string optionId)
        {
            var donations = CurrentDonations.Values;
            var option = CurrentOptionWorths.Worths[optionId]!;
            var newModelValues = option.DonationFractions.Aggregate(model.Values,
                (acc, donationFraction) => acc.Add(donationFraction.Key,
                    new DonationRecord(timestamp, donationFraction.Value * option.TotalWorth, null)));
            
            return new(newModelValues);
        }

        protected override DonationRecords ConvEnter(DonationRecords model, ConvEnter e)
            => Rebalance(model, e.Timestamp, e.Option);

        protected override DonationRecords ConvInvest(DonationRecords model, ConvInvest e)
            => Rebalance(model, e.Timestamp, e.Option);

        protected override DonationRecords ConvLiquidate(DonationRecords model, ConvLiquidate e)
            => Rebalance(model, e.Timestamp, e.Option);

        protected override DonationRecords PriceInfo(DonationRecords model, PriceInfo e)
            => Rebalance(model, e.Timestamp, e.Option);
        }
    }
}

public record DonationRecord(DateTimeOffset Timestamp, Real Worth, Allocation? Allocation);
public record Allocation(string Charity, Real Amount);

