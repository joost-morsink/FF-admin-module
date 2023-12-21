using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Calculator.ApiClient;
using External.GiveWp.ApiClient;
using External.Mollie.ApiClient;
using External.Stripe.ApiClient;
using FfAdmin.Calculator;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Extensions.Options;
using Options = FfAdmin.Calculator.Options;

namespace FfAdmin.EventImport.Function;

public class EventImportService : IEventImportService
{
    private const string OPTION_ID = "1";
    private const string COMPLETE = "Complete";
    private const string SUBSCRIPTION = "Subscription";
    private const string MOLLIE = "mollie";
    private const string PAID = "paid";
    private const string STRIPE = "stripe";
    private const string STRIPE_CHECKOUT = "stripe_checkout";
    private const int STORNO_DAYS = 56;
    
    private readonly ICalculatorClient _calculator;
    private readonly IEventStore _eventStore;
    private readonly MollieClient _mollie;
    private readonly EventImportOptions _options;
    private readonly IStripeService _stripe;

    public EventImportService(ICalculatorClient calculator, IEventStore eventStore, MollieClient mollie, IStripeService stripe,
        IOptions<EventImportOptions> options)
    {
        _calculator = calculator;
        _eventStore = eventStore;
        _mollie = mollie;
        _stripe = stripe;
        _options = options.Value;
    }

    public async Task ImportGiveWpDonations(GiveWpDonation[] donations)
    {
        donations = donations.Where(d => d.Status is COMPLETE or SUBSCRIPTION).ToArray();
        if (donations.Length == 0)
            return;
        var (nonExisting, charities, options) = await (
            _calculator.GetNonExistingDonations(_options.Branch, donations.Select(d => d.Id.ToString())),
            _calculator.GetCharities(_options.Branch),
            _calculator.GetOptions(_options.Branch)
        );

        var events = await CreateEvents(from d in donations
            join ne in nonExisting on d.Id.ToString() equals ne
            orderby d.Date
            select d, charities, options).ToArrayAsync();
        await _eventStore.AddEvents(_options.Branch, events);
    }

    private IAsyncEnumerable<Event> CreateEvents(IEnumerable<GiveWpDonation> giveWpDonations, Charities charities,
        Options options)
    {
        giveWpDonations = giveWpDonations.ToArray();
        var newCharities = giveWpDonations
            .Where(d => !charities.Values.ContainsKey(d.Form.Id))
            .DistinctBy(d => d.Form.Id)
            .Select(d => new NewCharity {Timestamp = d.Date.AddSeconds(-1), Code = d.Form.Id, Name = d.Form.Name});
        var newOptions = Enumerable.Empty<Event>();
        var newDonations = giveWpDonations.ToAsyncEnumerable()
            .SelectMany(d => NewDonation(d, options));
        return newCharities.Concat(newOptions).ToAsyncEnumerable().Concat(newDonations);
    }

    private async IAsyncEnumerable<Event> NewDonation(GiveWpDonation donation, Options options)
    {
        var option = options.Values[OPTION_ID];
        var amount = decimal.Parse(donation.Total, CultureInfo.InvariantCulture);

        if (donation.PaymentMeta.Currency != option.Currency)
        {
            if (string.Equals(donation.Gateway, MOLLIE, StringComparison.OrdinalIgnoreCase))
            {
                var payment = await _mollie.GetPayment(donation.TransactionId);
                if (payment is not null
                    && payment.Status is PAID
                    && string.Equals(payment.SettlementAmount.Currency, option.Currency, StringComparison.OrdinalIgnoreCase))
                    yield return MakeDonation(d =>
                    {
                        d.Exchange_reference = $"{MOLLIE}-{payment.Id}";
                        d.Exchanged_amount = payment.SettlementAmount.Amount;
                    });
            }

            if (string.Equals(donation.Gateway, STRIPE_CHECKOUT, StringComparison.OrdinalIgnoreCase))
            {
                var info = await _stripe.GetConvertedAmount(donation.TransactionId);
                if (info.Charge?.Paid == true && info.Charge?.Refunded == false && info.ConvertedAmount.HasValue)
                    yield return MakeDonation(d =>
                    {
                        d.Exchange_reference = $"{STRIPE}-{donation.TransactionId}";
                        d.Exchanged_amount = info.ConvertedAmount.Value;
                    });
            }
        }
        else
        {
            yield return MakeDonation(d => d.Exchanged_amount = d.Amount);
        }

        NewDonation MakeDonation(Action<NewDonation>? action = null)
        {
            var result = new NewDonation
            {
                Timestamp = donation.Date,
                Execute_timestamp =
                    donation.Status is COMPLETE ? donation.Date : donation.Date.AddDays(STORNO_DAYS), // Storno for subscriptions
                Donation = donation.Id.ToString(),
                Charity = donation.Form.Id,
                Amount = amount,
                Currency = donation.PaymentMeta.Currency,
                Donor = donation.PaymentMeta.DonorId,
                Transaction_reference = donation.TransactionId,
                Option = OPTION_ID
            };
            action?.Invoke(result);
            return result;
        }
    }
}
