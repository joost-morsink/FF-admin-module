using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FfAdmin.Calculator;
using FfAdmin.Common;
using Microsoft.Extensions.Logging;
using Charity = FfAdmin.Calculator.Charity;

namespace Calculator.ApiClient;

public class CalculatorClient : ICalculatorClient, ICheckOnline
{
    private readonly HttpClient _client;
    private readonly ILogger<CalculatorClient> _logger;

    public CalculatorClient(HttpClient client, ILogger<CalculatorClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    private async Task<string> GetString(string endpoint, string branch, int? at, IEnumerable<Event>? theory)
    {
        var parts = new List<string> {"/api/", branch, "/", endpoint};
        if (at.HasValue)
            parts.Add($"?at={at.Value}");
        var response = await (theory is null
            ? _client.GetAsync(string.Concat(parts))
            : PostAsJsonAsync(string.Concat(parts), theory));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    private async Task<T> GenericGet<T>(string endpoint, string branch, int? at, IEnumerable<Event>? theory)
        where T : class
    {
        var parts = new List<string> {"/api/", branch, "/", endpoint};
        if (at.HasValue)
            parts.Add($"?at={at.Value}");
        var response = await (theory is null
            ? _client.GetAsync(string.Concat(parts))
            : PostAsJsonAsync(string.Concat(parts), theory));
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

    private static JsonSerializerOptions DefaultJsonOptions { get; } = new()
    {
        Converters = {new JsonStringEnumConverter()},
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private async Task<HttpResponseMessage> PostAsJsonAsync(string address, IEnumerable<Event> events)
    {
        var content = $"[{string.Join(",", events.Select(e => e.ToJsonString()))}]";
        return await SendAsJsonAsync(address, content);
    }

    private async Task<HttpResponseMessage> SendAsJsonAsync(string address, string content)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = _client.BaseAddress is null ? new Uri(address) : new Uri(_client.BaseAddress, address),
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
        var response = await _client.SendAsync(request);
        return response;
    }

    private async Task<HttpResponseMessage> PostAsJsonAsync<T>(string address, T item)
    {
        var content = JsonSerializer.Serialize(item);
        return await SendAsJsonAsync(address, content);

    }

    public async Task<bool> IsOnline()
    {
        try
        {
            var response = await _client.GetAsync("api/health");
            _logger.Log(response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Warning, "Calculator API health check returned {StatusCode}", response.StatusCode);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public async Task<AmountsToTransfer> GetAmountsToTransfer(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, MoneyBag>>("amounts-to-transfer", branch, at, theory);

    public async Task<Charities> GetCharities(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, Charity>>("charities", branch, at, theory);

    public Task<CharityBalance> GetCharityBalance(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<CharityBalance>("charity-balance", branch, at, theory);
    
    public async Task<CumulativeInterest> GetCumulativeInterest(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string,CumulativeInterest.DataPoint>>("cumulative-interest", branch, at, theory);
    
    public async Task<HistoryHash> GetHistoryHash(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<string>("history-hash", branch, at, theory);

    public async Task<IdealOptionValuations> GetIdealOptionValuations(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string,IdealValuation>>("ideal-option-valuations", branch, at, theory);

    public async Task<MinimalExits> GetMinimalExits(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, decimal>>("minimal-exits", branch, at, theory);

    public async Task<Options> GetOptions(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, Option>>("options", branch, at, theory);

    public async Task<OptionWorths2.Header[]> GetOptionWorths(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<OptionWorths2.Header[]>("option-worths", branch, at, theory);

    public async Task<OptionWorthRecord[]> GetOptionWorthHistory(string branch, string id, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<OptionWorthRecord[]>($"option-worth-history/{id}", branch, at, theory);
    
    public async Task<string> GetOptionWorthHistoryChart(string branch, string id, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GetString($"option-worth-history-chart/{id}", branch, at, theory);

    public Task<ValidationErrors> GetValidationErrors(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => GenericGet<ValidationErrors>("validation-errors", branch, at, theory);

    public async Task<DonationStatistics> GetDonationStatistics(string branch, int? at = null,
        IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableDictionary<string, DonationStatistic>>("donation-statistics", branch, at, theory);

    public async Task<AuditHistory> GetAuditHistory(string branch, int? at = null, IEnumerable<Event>? theory = null)
        => await GenericGet<ImmutableList<AuditMoment>>("audit-history", branch, at, theory);
    
    public async Task<(string[] exists, string[] notExists)> SplitDonationsOnExistence(string branch, IEnumerable<string> ids)
    {
        var response = await PostAsJsonAsync($"api/{branch}/non-existing-donations", ids);
        response.EnsureSuccessStatusCode();
        var resultStr = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string,string[]>>(resultStr);
        return (result?.GetValueOrDefault("exists") ?? Array.Empty<string>(),
            result?.GetValueOrDefault("not_exists") ?? Array.Empty<string>());

    }
}
