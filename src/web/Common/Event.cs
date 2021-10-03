using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace FfAdmin.Common
{
    public abstract class Event
    {
        public abstract EventType Type { get; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public static Event ReadFrom(JsonDocument doc, JsonSerializerOptions options)
        {
            var type = doc.RootElement.GetProperty("type").GetString();
            var json = doc.RootElement.ToString() ?? "{}";
            if (Enum.TryParse<EventType>(type, out var eventType))
            {
                return eventType switch
                {
                    EventType.META_NEW_OPTION => JsonSerializer.Deserialize<NewOption>(json, options)!,
                    EventType.META_NEW_CHARITY => JsonSerializer.Deserialize<NewCharity>(json, options)!,
                    _ => throw new InvalidDataException("Invalid event type")
                };
            }
            else
                throw new InvalidDataException("Invalid event type");
        }
        public abstract IEnumerable<ValidationMessage> Validate();
        public string ToJsonString(JsonSerializerOptions options = null)
            => JsonSerializer.Serialize(this, GetType(), options ?? new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = false
            });
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
        public override IEnumerable<ValidationMessage> Validate()
        {
            if (Reinvestment_fraction < 0 || Reinvestment_fraction > 1)
                yield return new ValidationMessage(nameof(Reinvestment_fraction), "Reinvestment fraction out of range.");
            if (FutureFund_fraction < 0 || FutureFund_fraction > 1)
                yield return new ValidationMessage(nameof(FutureFund_fraction), "Future Fund fraction out of range.");
            if (Charity_fraction < 0 || Charity_fraction > 1)
                yield return new ValidationMessage(nameof(Charity_fraction), "Charity fraction fraction out of range.");
            if (Bad_year_fraction < 0 || Bad_year_fraction > 0.1m)
                yield return new ValidationMessage(nameof(Bad_year_fraction), "Bad year fraction fraction out of range.");

            if (string.IsNullOrWhiteSpace(Code))
                yield return new ValidationMessage(nameof(Code), "Code is required.");
            if (Reinvestment_fraction + FutureFund_fraction + Charity_fraction != 1)
                yield return new ValidationMessage(nameof(Reinvestment_fraction), "Fractions should add up to 1");
        }
    }
    public class NewCharity : Event
    {
        public override EventType Type => EventType.META_NEW_CHARITY;
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Code))
                yield return new ValidationMessage(nameof(Code), "Code is required.");
        }
    }
}
