using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FfAdmin.Common;

namespace FfAdminWeb.Utils
{
    public class EventConverter : JsonConverter<Event>
    {
        public static EventConverter Instance { get; } = new ();

        public override bool CanConvert(Type typeToConvert)
            => typeof(Event).IsAssignableFrom(typeToConvert);

        public override Event Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            return Event.ReadFrom(doc, options.Without(this));
        }

        public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options.Without(this));
        }

    }

}
