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
using FfAdmin.ExchangeRate;
using Microsoft.Extensions.Options;
using Stripe;
using Event = FfAdmin.Common.Event;
using Options = FfAdmin.Calculator.Options;

namespace FfAdmin.EventImport.Function;

public class EventImportService : IEventImportService
{
    private const string OPTION_ID = "1";
    private const string COMPLETE = "Complete";
    private const string SUBSCRIPTION = "Subscription";
    private const string CANCELLED = "Cancelled";
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
    private readonly IExchangeRateService _exchangeRateService;

    public EventImportService(ICalculatorClient calculator, IEventStore eventStore, MollieClient mollie, IStripeService stripe,
        IExchangeRateService exchangeRateService,
        IOptions<EventImportOptions> options)
    {
        _calculator = calculator;
        _eventStore = eventStore;
        _mollie = mollie;
        _stripe = stripe;
        _exchangeRateService = exchangeRateService;
        _options = options.Value;
    }

    public async Task ProcessGiveWpDonations(GiveWpDonation[] donations)
    {
        var newDonations = donations.Where(d => d.Status is COMPLETE or SUBSCRIPTION).ToArray();
        var cancelledDonations = donations.Where(d => d.Status is CANCELLED).ToArray();
        if (newDonations.Length + cancelledDonations.Length == 0)
            return;
        var ((existing, nonExisting), charities, options) = await (
            _calculator.SplitDonationsOnExistence(_options.Branch, newDonations.Select(d => d.Id.ToString())),
            _calculator.GetCharities(_options.Branch),
            _calculator.GetOptions(_options.Branch)
        );

        var events = await CreateEventsForDonations(from d in newDonations
                join ne in nonExisting on d.Id.ToString() equals ne
                orderby d.Date
                select d, charities, options)
            .Concat(CreateEventsForCancellations(
                from d in cancelledDonations                
                join e in existing on d.Id.ToString() equals e
                orderby d.Date
                select d).ToAsyncEnumerable()).ToArrayAsync();
            
        await _eventStore.AddEvents(_options.Branch, events);
    }

    private IEnumerable<Event> CreateEventsForCancellations(IEnumerable<GiveWpDonation> giveWpDonations)
    {
        return giveWpDonations.Select(d => new CancelDonation {Donation = d.Id.ToString(), Timestamp = d.Date});
    }

    private IAsyncEnumerable<Event> CreateEventsForDonations(IEnumerable<GiveWpDonation> giveWpDonations, Charities charities,
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
        
        var molliePayment = string.Equals(donation.Gateway, MOLLIE, StringComparison.OrdinalIgnoreCase)
            ? await _mollie.GetPayment(donation.TransactionId)
            : null;

        if (donation.PaymentMeta.Currency != option.Currency)
        {
            if (string.Equals(donation.Gateway, MOLLIE, StringComparison.OrdinalIgnoreCase))
            {
                var payment = molliePayment;
                if (payment is not null
                    && payment.Status is PAID)
                {
                    if(string.Equals(payment.SettlementAmount.Currency, option.Currency, StringComparison.OrdinalIgnoreCase))
                        yield return MakeDonation(d =>
                        {
                            d.Exchange_reference = $"{MOLLIE}-{payment.Id}";
                            d.Exchanged_amount = payment.SettlementAmount.Amount;
                        });
                    else // Unable to retrieve exchange settlement from Mollie, use external service
                    if (await _exchangeRateService.GetExchangeRate(payment.Amount.Currency, option.Currency,
                            DateOnly.FromDateTime(donation.Date.Date))
                        is { } rate)
                        yield return MakeDonation(d =>
                        {
                            d.Exchange_reference = $"{MOLLIE}-{payment.Id}";
                            d.Exchanged_amount = decimal.Floor(d.Amount * 100 / (decimal)rate.Factor) / 100;
                        });
                }
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
            yield return MakeDonation(d =>
            {
                d.Exchanged_amount = d.Amount;
            });
        }

        NewDonation MakeDonation(Action<NewDonation>? action = null)
        {
            var possibleFraud = molliePayment is not null
                                && string.Equals(molliePayment.Method, "creditcard", StringComparison.OrdinalIgnoreCase)
                                && string.Equals(molliePayment.Amount.Currency, "EUR", StringComparison.OrdinalIgnoreCase)
                                && decimal.TryParse(molliePayment.Amount.Value, CultureInfo.InvariantCulture, out var amt) && amt >= 50;
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
            if (possibleFraud)
                result.Execute_timestamp = result.Execute_timestamp.AddMonths(6);
            action?.Invoke(result);
            return result;
        }
    }
}
