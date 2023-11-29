using System.Text.Json.Serialization;

namespace External.GiveWp.ApiClient;

public class PaymentMeta
{
    [JsonPropertyName("_give_payment_currency")]public string Currency { get; set; } = "";
    [JsonPropertyName("_give_payment_donor_id"), JsonConverter(typeof(StringConverter))]public string DonorId { get; set; } = "";
    [JsonPropertyName("_give_payment_transaction_id"), JsonConverter(typeof(StringConverter))]public string TransactionId { get; set; } = "";
}