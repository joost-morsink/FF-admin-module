using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FfAdmin.Calculator;
using FfAdmin.Common;
using Charity = FfAdmin.Calculator.Charity;

namespace Calculator.ApiClient;

public class CalculatorClient : ICalculatorClient
{
    private readonly HttpClient _client;

    public CalculatorClient(HttpClient client)
    {
        _client = client;
    }

    private async Task<T> GenericGet<T>(string endpoint, string branch, int? at, IEnumerable<Event>? theory)
        where T : class
    {
        var parts = new List<string> {"/api/", branch, "/", endpoint};
        if (at.HasValue)
            parts.Add($"?at={at.Value}");
        var response = await (theory is null
            ? _client.GetAsync(string.Concat(parts))
            : _client.PostAsJsonAsync(string.Concat(parts), theory));
        response.EnsureSuccessStatusCode();
#if DEBUG
        var resultStr = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(resultStr);
#else
        var result = await response.Content.ReadFromJsonAsync<T>();
#endif
        if (result is null)
            throw new Exception();
        return result;
    }

    public async Task<AmountsToTransfer> GetAmountsToTransfer(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, MoneyBag>>("amounts-to-transfer", branch, at, theory);

    public async Task<Charities> GetCharities(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, Charity>>("charities", branch, at, theory);

    public Task<CharityBalance> GetCharityBalance(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<CharityBalance>("charity-balance", branch, at, theory);

    public Task<CharityFractionSetsForOption> GetCharityFractionSetsForOption(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => GenericGet<CharityFractionSetsForOption>("current-charity-fraction-sets", branch, at, theory);

    public async Task<CumulativeInterest> GetCumulativeInterest(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string,CumulativeInterest.DataPoint>>("cumulative-interest", branch, at, theory);

    public async Task<Donations> GetDonations(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string,Donation>>("donations", branch, at, theory);

    public async Task<DonationRecords> GetDonationRecords(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string,ImmutableList<DonationRecord>>>("donation-records", branch, at, theory);

    public async Task<HistoryHash> GetHistoryHash(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<string>("history-hash", branch, at, theory);

    public async Task<IdealOptionValuations> GetIdealOptionValuations(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string,IdealValuation>>("ideal-option-valuations", branch, at, theory);

    public async Task<MinimalExits> GetMinimalExits(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, decimal>>("minimal-exits", branch, at, theory);

    public async Task<Options> GetOptions(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, Option>>("options", branch, at, theory);

    public async Task<OptionWorths> GetOptionWorths(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, OptionWorth>>("option-worths", branch, at, theory);

    public Task<ValidationErrors> GetValidationErrors(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<ValidationErrors>("validation-errors", branch, at, theory);

    public async Task<DonationStatistics> GetDonationStatistics(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, DonationStatistic>>("donation-statistics", branch, at, theory);

    public async Task<AuditHistory> GetAuditHistory(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableList<AuditMoment>>("audit-history", branch, at, theory);
}
