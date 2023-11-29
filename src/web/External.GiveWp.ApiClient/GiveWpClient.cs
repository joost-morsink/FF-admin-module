using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace External.GiveWp.ApiClient;

public class GiveWpClient
{
    private readonly HttpClient _client;

    public GiveWpClient(HttpClient client)
    {
        _client = client;
    }
    public async Task<GiveWpDonation[]> GetDonations(DateOnly starting)
    {
        var response = await _client.GetAsync($"donations/?number=-1&date=range&startdate={starting:yyyyMMdd}&enddate={DateTime.UtcNow:yyyyMMdd}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error getting donations: {response.StatusCode}");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GiveWpDonations>(json)?.Donations ?? Array.Empty<GiveWpDonation>();
    }
}
