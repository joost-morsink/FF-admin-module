using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record DonationRecords(ImmutableDictionary<string, ImmutableList<DonationRecord>> Values) : IModel<DonationRecords>
{
    public static DonationRecords Empty { get; } = new(ImmutableDictionary<string, ImmutableList<DonationRecord>>.Empty);
    public static IEventProcessor<DonationRecords> Processor { get; } = new Impl();

    private class Impl : EventProcessor<DonationRecords>
    {
        public override DonationRecords Start => Empty;
        protected override DonationRecords NewDonation(DonationRecords model, IContext context, NewDonation e)
        {
            var rec = new DonationRecord(e.Timestamp, e.Exchanged_amount, null);
            var newValues = model.Values.Add(e.Donation, rec);
            return new(newValues);
        }
       
        protected override DonationRecords ConvExit(DonationRecords model, IContext context, ConvExit e)
        {
            var donations = context.GetContext<Donations>().Values;
            var option = context.GetContext<OptionWorths>().Worths[e.Option]!;
            var newModelValues = option.DonationFractions.Aggregate(model.Values,
                (acc, donationFraction) => acc.Add(donationFraction.Key,
                    new DonationRecord(e.Timestamp, donationFraction.Value * option.TotalWorth,
                        new Allocation(donations[donationFraction.Key].CharityId, donationFraction.Value * e.Amount))));
            return new(newModelValues);
        }

        protected virtual DonationRecords Rebalance(DonationRecords model, IContext context, DateTimeOffset timestamp, string optionId)
        {
            var donations = context.GetContext<Donations>().Values;
            var option = context.GetContext<OptionWorths>().Worths[optionId]!;
            var newModelValues = option.DonationFractions.Aggregate(model.Values,
                (acc, donationFraction) => acc.Add(donationFraction.Key,
                    new DonationRecord(timestamp, donationFraction.Value * option.TotalWorth, null)));
            
            return new(newModelValues);
        }

        protected override DonationRecords ConvEnter(DonationRecords model, IContext context, ConvEnter e)
            => Rebalance(model, context, e.Timestamp, e.Option);

        protected override DonationRecords ConvInvest(DonationRecords model, IContext context, ConvInvest e)
            => Rebalance(model, context, e.Timestamp, e.Option);

        protected override DonationRecords ConvLiquidate(DonationRecords model, IContext context, ConvLiquidate e)
            => Rebalance(model, context, e.Timestamp, e.Option);

        protected override DonationRecords PriceInfo(DonationRecords model, IContext context, PriceInfo e)
            => Rebalance(model, context, e.Timestamp, e.Option);
    }
}

public record DonationRecord(DateTimeOffset Timestamp, Real Worth, Allocation? Allocation);
public record Allocation(string Charity, Real Amount);

