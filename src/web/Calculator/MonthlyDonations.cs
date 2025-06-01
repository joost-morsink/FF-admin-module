using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record MonthlyDonations(ImmutableDictionary<string, ImmutableSortedDictionary<DateOnly, MonthlyDonations.Data>> Donations) : IModel<MonthlyDonations>
{
    private static DateOnly ToMonth(DateOnly date)
        => new (date.Year, date.Month, 1);
    private static DateOnly ToMonth(DateTimeOffset date)
        => ToMonth(date.ToUniversalTime().DateTime);
    private static DateOnly ToMonth(DateTime date)
        => new(date.Year, date.Month,1);
    
    public record Data(MoneyBag Donated, Real Exchanged)
    {
        public static Data Empty { get; } = new Data(MoneyBag.Empty, 0);
    }

    public MonthlyDonations Add(string option, DateOnly month, string currency, Real amount, Real exchanged)
    {
        month = ToMonth(month);
        var perOption = Donations.GetValueOrDefault(option) ?? ImmutableSortedDictionary<DateOnly, Data>.Empty;
        var perMonth = perOption.GetValueOrDefault(month) ?? Data.Empty;
        var newDonations = perOption.SetItem(month, perMonth with
        {
            Donated = perMonth.Donated.Add(currency, amount),
            Exchanged = perMonth.Exchanged + exchanged
        });
        return Donations.SetItem(option, newDonations);
    }
    public static implicit operator MonthlyDonations(ImmutableDictionary<string, ImmutableSortedDictionary<DateOnly, Data>> donations)
        => new (donations);

    public static MonthlyDonations Empty { get; } = new(ImmutableDictionary<string, ImmutableSortedDictionary<DateOnly, Data>>.Empty);

    public static IEventProcessor<MonthlyDonations> GetProcessor(IServiceProvider serviceProvider)
        => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);

    private class Impl(IContext<Options> cOptions, IContext<Donations> cDonations) : EventProcessor<MonthlyDonations>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calculation(cOptions, cDonations, previousContext, currentContext);
        }

        private sealed class Calculation(IContext<Options> cOptions, IContext<Donations> cDonations, IContext previous, IContext current) : BaseCalculation(previous, current)
        {
            public ValueTask<Options> CurrentOptions => GetCurrent(cOptions);
            public ValueTask<Donations> PreviousDonations => GetPrevious(cDonations);
            protected override async ValueTask<MonthlyDonations> NewDonation(MonthlyDonations model, NewDonation e)
            {
                var option = (await CurrentOptions).Values[e.Option];

                return model.Add(e.Option, ToMonth(e.Timestamp), e.Currency, e.Amount, e.Exchanged_amount);
            }

            protected override async ValueTask<MonthlyDonations> CancelDonation(MonthlyDonations model, CancelDonation e)
            {
                var donation = (await PreviousDonations).Values[e.Donation];
                var option = (await CurrentOptions).Values[donation.OptionId];
                return model.Add(option.Id, ToMonth(donation.Timestamp), donation.OriginalCurrency, -donation.OriginalAmount, -donation.Amount);
            }
        }
    }
}
