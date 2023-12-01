using System.Text.Json.Serialization;

namespace External.Mollie.ApiClient;

public class Payment
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("amount")] public Money Amount { get; set; } = new();
    [JsonPropertyName("amountRefunded")] public Money AmountRefunded { get; set; } = new();
    [JsonPropertyName("settlementAmount")] public Money SettlementAmount { get; set; } = new();
}