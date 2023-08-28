using System.Text.Json;
using System.Text.Json.Serialization;

namespace FfAdmin.Calculator;

public class FractionSetJsonConverter : JsonConverter<FractionSet>
{
    public override FractionSet? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, Real>>(ref reader, options);
        
        return dict is null 
            ? FractionSet.Empty
            : FractionSet.Empty.AddRange(dict.Select(x => (x.Key, x.Value)));
    }

    public override void Write(Utf8JsonWriter writer, FractionSet value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var (key, val) in value)
            writer.WriteNumber(key, val);
        writer.WriteEndObject();
    }
}
