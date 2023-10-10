using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FfAdmin.Calculator;
using FfAdmin.Common;

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
        var result = await response.Content.ReadFromJsonAsync<T>();
        if (result is null)
            throw new Exception();
        return result;
    }

    public Task<AmountsToTransfer> GetAmountsToTransfer(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<AmountsToTransfer>("amounts-to-transfer", branch, at, theory);
    public Task<Charities> GetCharities(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<Charities>("charities", branch, at, theory);
    public Task<CharityBalance> GetCharityBalance(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<CharityBalance>("charity-balance", branch, at, theory);
    public Task<CharityFractionSetsForOption> GetCharityFractionSetsForOption(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<CharityFractionSetsForOption>("current-charity-fraction-sets", branch, at, theory);
    public Task<CumulativeInterest> GetCumulativeInterest(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<CumulativeInterest>("cumulative-interest", branch, at, theory);
    public Task<Donations> GetDonations(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<Donations>("donations", branch, at, theory);
    public Task<DonationRecords> GetDonationRecords(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<DonationRecords>("donation-records", branch, at, theory);
    public Task<HistoryHash> GetHistoryHash(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<HistoryHash>("history-hash", branch, at, theory);
    public Task<IdealOptionValuations> GetIdealOptionValuations(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<IdealOptionValuations>("ideal-option-valuations", branch, at, theory);
    public Task<MinimalExits> GetMinimalExits(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<MinimalExits>("minimal-exits", branch, at, theory);
    public Task<Options> GetOptions(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<Options>("options", branch, at, theory);
    public Task<OptionWorths> GetOptionWorths(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<OptionWorths>("option-worths", branch, at, theory);
    public Task<ValidationErrors> GetValidationErrors(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<ValidationErrors>("validation-errors", branch, at, theory);
}
