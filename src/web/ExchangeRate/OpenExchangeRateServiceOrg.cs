using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FfAdmin.ExchangeRate;

public class OpenExchangeRateServiceOrg : IExchangeRateService
{
    private readonly HttpClient _client;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<OpenExchangeRatesOrgOptions> _options;

    public OpenExchangeRateServiceOrg(HttpClient client, IMemoryCache memoryCache, IOptions<OpenExchangeRatesOrgOptions> options)
    {
        _client = client;
        _memoryCache = memoryCache;
        _options = options;
    }

    public async Task<ExchangeRate?> GetExchangeRate(string from, string to, DateOnly date)
    {
        var url = ConstructUrl(from, to, date);
        var data = await _memoryCache.GetOrCreateAsync(url, async entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromHours(1));
            var response = await _client.GetStringAsync(url);
            return JsonSerializer.Deserialize<Response>(response); 
        });

        if (data?.Rates.TryGetValue(from, out var fromRate) == true
            && data.Rates.TryGetValue(to, out var toRate))
            return new ExchangeRate(from, to, toRate / fromRate);
        return null;
    }

    private string ConstructUrl(string from, string to, DateOnly date)
        => $"historical/{date:yyyy-MM-dd}.json?app_id={_options.Value.AppId}&show_alternative=false&prettyprint=false";

    public class Response
    {
        [JsonPropertyName("rates")] public Dictionary<string, double> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase); 
    }
}