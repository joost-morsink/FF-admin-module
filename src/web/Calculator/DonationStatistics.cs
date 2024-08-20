using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record DonationStatistics(ImmutableDictionary<string, DonationStatistic> Statistics) : IModel<DonationStatistics>
{
    public static implicit operator DonationStatistics(ImmutableDictionary<string, DonationStatistic> statistics)
        => new(statistics);

    public static DonationStatistics Empty { get; } = new(ImmutableDictionary<string, DonationStatistic>.Empty);

    public static IEventProcessor<DonationStatistics> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    public DonationStatistics Mutate(string currency, Func<DonationStatistic, DonationStatistic> mutator)
        => new(Statistics.SetItem(currency,
            mutator(Statistics.GetValueOrDefault(currency, DonationStatistic.Empty(currency)))));

    private class Impl(IContext<Donations> cDonations, IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths) : EventProcessor<DonationStatistics>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cDonations, cOptions, cOptionWorths);
        }

        private class Calc(IContext previousContext, IContext currentContext,IContext<Donations> cDonations, IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths) : BaseCalculation(previousContext, currentContext)
        {
            public Donations CurrentDonations => GetCurrent(cDonations);
            public Options CurrentOptions => GetCurrent(cOptions);
            public OptionWorths CurrentOptionWorths => GetCurrent(cOptionWorths);
            
            protected override DonationStatistics NewDonation(DonationStatistics model, NewDonation e)
            {
                var option = CurrentOptions.Values.GetValueOrDefault(e.Option);
                if (option is null)
                    return model;
                return model.Mutate(option.Currency,
                    s => s with {Worth = s.Worth + e.Exchanged_amount, Amount = s.Amount + e.Exchanged_amount});
            }

            protected override DonationStatistics CancelDonation(DonationStatistics model, CancelDonation e)
            {
                var donation = CurrentDonations.Values.GetValueOrDefault(e.Donation);
                if (donation is null)
                    return model;
                var option = CurrentOptions.Values.GetValueOrDefault(donation.OptionId);
                if (option is null)
                    return model;
                return model.Mutate(option.Currency,
                    s => s with {Worth = s.Worth - donation.Amount, Amount = s.Amount - donation.Amount});
            }

            protected override DonationStatistics ConvExit(DonationStatistics model, ConvExit e)
            {
                var option = CurrentOptions.Values.GetValueOrDefault(e.Option);
                if (option is null)
                    return model;
                return UpdateWorth(model.Mutate(option.Currency, s => s with {Allocated = s.Allocated + e.Amount})
                    , option.Currency);
            }

            protected override DonationStatistics ConvTransfer(DonationStatistics model,ConvTransfer e)
            {
                return model.Mutate(e.Currency, s => s with {Transferred = s.Transferred + e.Amount});
            }

            protected override DonationStatistics ConvEnter(DonationStatistics model, ConvEnter e)
                => UpdateWorthByOptionId(model, e.Option);

            protected override DonationStatistics ConvLiquidate(DonationStatistics model, ConvLiquidate e)
                => UpdateWorthByOptionId(model, e.Option);

            protected override DonationStatistics ConvInvest(DonationStatistics model, ConvInvest e)
                => UpdateWorthByOptionId(model, e.Option);

            protected override DonationStatistics PriceInfo(DonationStatistics model, PriceInfo e)
                => UpdateWorthByOptionId(model, e.Option);

            private DonationStatistics UpdateWorthByOptionId(DonationStatistics model, string optionId)
            {
                if (!CurrentOptions.Values.TryGetValue(optionId, out var option))
                    return model;
                return UpdateWorth(model, option.Currency);
            }

            private DonationStatistics UpdateWorth(DonationStatistics model, string currency)
            {
                var totalWorth =
                    (from w in CurrentOptionWorths.Worths.Values
                        join o in CurrentOptions.Values.Values on w.Id equals o.Id
                        where o.Currency == currency
                        select w.TotalWorth + w.UnenteredDonations.Sum(ud => ud.Amount)).Sum();
                return model.Mutate(currency, s => s with {Worth = totalWorth});
            }
        }
    }
}

public record DonationStatistic(string Currency, Real Amount, Real Worth, Real Allocated, Real Transferred,
    Real FfAllocated, Real FfTransferred)
{
    public static DonationStatistic Empty(string currency) => new(currency, 0, 0, 0, 0, 0, 0);
}
