using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace External.Mollie.ApiClient;

public class MollieClient
{
    private readonly HttpClient _client;

    public MollieClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<Payment?> GetPayment(string id)
    {
        var payment = await _client.GetAsync($"v2/payments/{id}");
        if(payment.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        payment.EnsureSuccessStatusCode();
        var json = await payment.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Payment>(json);
    }
}
