using System.Text.Json;
using System.Text.Json.Serialization;

namespace FfAdmin.Calculator;

public class FractionSetJsonConverter : JsonConverter<FractionSet>
{
    public override FractionSet? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var sd = JsonSerializer.Deserialize<FractionSet.SerializationData>(ref reader, options);
        return sd?.ToFractionSet() ?? FractionSet.Empty;
    }

    public override void Write(Utf8JsonWriter writer, FractionSet value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToSerializationData(), options);
    }
}
