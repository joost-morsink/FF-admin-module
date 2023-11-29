using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Calculator.ApiClient;
using External.GiveWp.ApiClient;
using FfAdmin.Calculator;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Extensions.Options;
using Options = FfAdmin.Calculator.Options;

namespace FfAdmin.EventImport.Function;

public class EventImportService : IEventImportService
{
    private readonly ICalculatorClient _calculator;
    private readonly IEventStore _eventStore;
    private readonly EventImportOptions _options;

    public EventImportService(ICalculatorClient calculator, IEventStore eventStore,
        IOptions<EventImportOptions> options)
    {
        _calculator = calculator;
        _eventStore = eventStore;
        _options = options.Value;
    }

    public async Task ImportGiveWpDonations(GiveWpDonation[] donations)
    {
        donations = donations.Where(d => d.Status is "Complete" or "Subscription").ToArray();
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
        await Task.Yield();
        var option = options.Values["1"];
        if (donation.PaymentMeta.Currency != option.Currency)
        {
        }
        else
        {
            var amount = decimal.Parse(donation.Total, CultureInfo.InvariantCulture);
            yield return new NewDonation
            {
                Timestamp = donation.Date,
                Execute_timestamp =
                    donation.Status is "Complete" ? donation.Date : donation.Date.AddDays(-56), // Storno
                Donation = donation.Id.ToString(),
                Charity = donation.Form.Id,
                Amount = amount,
                Exchanged_amount = amount,
                Currency = donation.PaymentMeta.Currency,
                Donor = donation.PaymentMeta.DonorId,
                Option = "1"
            };
        }
    }
}