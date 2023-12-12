using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace External.GiveWp.ApiClient;

internal class DateTimeOffsetFormat : JsonConverter<DateTimeOffset>
{
    private static readonly DateTime SWITCH_TO_UTC = new DateTime(2023, 12, 05, 15, 37,0);
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTime.TryParseExact(reader.GetString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return new DateTimeOffset(date,  date < SWITCH_TO_UTC ? TimeSpan.FromHours(2) : TimeSpan.Zero);
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}
