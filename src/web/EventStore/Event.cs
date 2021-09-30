using System;
using System.IO;
using System.Text.Json;

namespace EventStore
{
    public abstract class Event
    {
        public abstract EventType Type { get; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public static Event ReadFrom(JsonDocument doc)
        {
            var type = doc.RootElement.GetProperty("Type").GetString();
            var json = doc.RootElement.ToString() ?? "{}";
            if (Enum.TryParse<EventType>(type, out var eventType))
            {
                return eventType switch
                {
                    EventType.META_NEW_OPTION => JsonSerializer.Deserialize<NewOption>(json)!,
                    EventType.META_NEW_CHARITY => JsonSerializer.Deserialize<NewCharity>(json)!,
                    _ => throw new InvalidDataException("Invalid event type")
                };
            }
            else
                throw new InvalidDataException("Invalid event type");
        }
    }
    public class NewOption : Event
    {
        public override EventType Type => EventType.META_NEW_OPTION;
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Reinvestment_fraction { get; set; }
        public decimal FutureFund_fraction { get; set; }
        public decimal Charity_fraction { get; set; }
        public decimal Bad_year_fraction { get; set; }
    }
    public class NewCharity : Event
    {
        public override EventType Type => EventType.META_NEW_CHARITY;
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
