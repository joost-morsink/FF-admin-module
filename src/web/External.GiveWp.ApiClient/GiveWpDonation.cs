using System;
using System.Text.Json.Serialization;

namespace External.GiveWp.ApiClient;

public class GiveWpDonation
{
    [JsonPropertyName("ID")] public long Id { get; set; } 
    [JsonPropertyName("transaction_id"), JsonConverter(typeof(StringConverter))] public string TransactionId { get; set; }= "";
    [JsonPropertyName("total"), JsonConverter(typeof(StringConverter))] public string Total { get; set; }= "";
    [JsonPropertyName("status")] public string Status { get; set; }= "";
    [JsonPropertyName("gateway")] public string Gateway { get; set; }= "";
    [JsonPropertyName("date"), JsonConverter(typeof(DateTimeOffsetFormat))] public DateTimeOffset Date { get; set; }
    [JsonPropertyName("payment_meta")] public PaymentMeta PaymentMeta { get; set; } = new PaymentMeta();
    [JsonPropertyName("form")] public Form Form { get; set; } = new Form();
}