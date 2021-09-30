using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore;

namespace FfAdminWeb.Utils
{
    public class EventConverter : JsonConverter<Event>
    {
        public static EventConverter Instance { get; } = new EventConverter();
        public EventConverter()
        {
        }

        public override bool CanConvert(Type typeToConvert)
            => typeof(Event).IsAssignableFrom(typeToConvert);
        
        public override Event? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            return Event.ReadFrom(doc);
        }

        public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
        {
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Remove(this);
            JsonSerializer.Serialize(writer, value, value.GetType(), newOptions);
        }
    }
}
