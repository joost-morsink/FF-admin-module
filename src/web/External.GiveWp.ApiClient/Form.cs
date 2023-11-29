using System.Text.Json.Serialization;

namespace External.GiveWp.ApiClient;

public class Form
{
    [JsonPropertyName("id"), JsonConverter(typeof(StringConverter))] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}