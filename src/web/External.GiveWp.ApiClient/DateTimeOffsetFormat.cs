using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace External.GiveWp.ApiClient;

internal class DateTimeOffsetFormat : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTimeOffset.TryParseExact(reader.GetString(), "yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,out var date))
                return date;
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}