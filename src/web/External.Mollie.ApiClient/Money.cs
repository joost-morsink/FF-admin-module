using System.Globalization;
using System.Text.Json.Serialization;

namespace External.Mollie.ApiClient;

public class Money
{
    [JsonPropertyName("value")] public string Value { get; set; } = "0";
    [JsonIgnore]public decimal Amount => decimal.Parse(Value, CultureInfo.InvariantCulture);
    [JsonPropertyName("currency")] public string Currency { get; set; } = "";
}
