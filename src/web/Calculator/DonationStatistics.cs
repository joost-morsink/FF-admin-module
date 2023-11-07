using System.Security.Cryptography;

namespace FfAdmin.Calculator;

public record DonationStatistics(ImmutableDictionary<string, DonationStatistic> Statistics) : IModel<DonationStatistics>
{
    public static implicit operator DonationStatistics(ImmutableDictionary<string, DonationStatistic> statistics)
        => new(statistics);

    public static DonationStatistics Empty { get; } = new(ImmutableDictionary<string, DonationStatistic>.Empty);
    public static IEventProcessor<DonationStatistics> Processor { get; } = new Impl();

    public DonationStatistics Mutate(string currency, Func<DonationStatistic, DonationStatistic> mutator)
        => new(Statistics.SetItem(currency,
            mutator(Statistics.GetValueOrDefault(currency, DonationStatistic.Empty(currency)))));

    private class Impl : EventProcessor<DonationStatistics>
    {
        public override DonationStatistics Start => Empty;

        protected override DonationStatistics NewDonation(DonationStatistics model, IContext context, NewDonation e)
        {
            var option = context.GetContext<Options>().Values.GetValueOrDefault(e.Option);
            if (option is null)
                return model;
            return model.Mutate(option.Currency,
                s => s with {Worth = s.Worth + e.Exchanged_amount, Amount = s.Amount + e.Exchanged_amount});
        }

        protected override DonationStatistics CancelDonation(DonationStatistics model, IContext context,
            CancelDonation e)
        {
            var donation = context.GetContext<Donations>().Values.GetValueOrDefault(e.Donation);
            if (donation is null)
                return model;
            var option = context.GetContext<Options>().Values.GetValueOrDefault(donation.OptionId);
            if (option is null)
                return model;
            return model.Mutate(option.Currency,
                s => s with {Worth = s.Worth - donation.Amount, Amount = s.Amount - donation.Amount});
        }

        protected override DonationStatistics ConvExit(DonationStatistics model, IContext context, ConvExit e)
        {
            var option = context.GetContext<Options>().Values.GetValueOrDefault(e.Option);
            if (option is null)
                return model;
            return UpdateWorth(model.Mutate(option.Currency, s => s with {Allocated = s.Allocated + e.Amount})
                , context, option.Currency);
        }

        protected override DonationStatistics ConvTransfer(DonationStatistics model, IContext context, ConvTransfer e)
        {
            return model.Mutate(e.Currency, s => s with {Transferred = s.Transferred + e.Amount});
        }
        
        protected override DonationStatistics ConvEnter(DonationStatistics model, IContext context, ConvEnter e)
            => UpdateWorthByOptionId(model, context, e.Option);

        protected override DonationStatistics ConvLiquidate(DonationStatistics model, IContext context, ConvLiquidate e)
            => UpdateWorthByOptionId(model, context, e.Option);

        protected override DonationStatistics ConvInvest(DonationStatistics model, IContext context, ConvInvest e)
            => UpdateWorthByOptionId(model, context, e.Option);

        protected override DonationStatistics PriceInfo(DonationStatistics model, IContext context, PriceInfo e)
            => UpdateWorthByOptionId(model, context, e.Option);

        private DonationStatistics UpdateWorthByOptionId(DonationStatistics model, IContext context, string optionId)
        {
            if (!context.GetContext<Options>().Values.TryGetValue(optionId, out var option))
                return model;
            return UpdateWorth(model, context, option.Currency);
        }
        private DonationStatistics UpdateWorth(DonationStatistics model, IContext context, string currency)
        {
            var totalWorth =
                (from w in context.GetContext<OptionWorths>().Worths.Values
                    join o in context.GetContext<Options>().Values.Values on w.Id equals o.Id
                    where o.Currency == currency
                    select w.TotalWorth + w.UnenteredDonations.Sum(ud => ud.Amount)).Sum();
            return model.Mutate(currency, s => s with {Worth = totalWorth});
        }
    }
}

public record DonationStatistic(string Currency, Real Amount, Real Worth, Real Allocated, Real Transferred,
    Real FfAllocated, Real FfTransferred)
{
    public static DonationStatistic Empty(string currency) => new(currency, 0, 0, 0, 0, 0, 0);
}
