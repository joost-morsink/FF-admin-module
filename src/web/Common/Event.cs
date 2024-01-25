using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FfAdmin.Common
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public abstract class Event
    {
        public abstract EventType Type { get; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public static async Task<Event[]> ReadAll(Stream stream, JsonSerializerOptions? options = null)
        {
            var opts = options ??= DefaultJsonOptions;
            var res = new List<Event>();
            using var rdr = new StreamReader(stream);
            string? str;
            while ((str = await rdr.ReadLineAsync()) != null)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    var e = JsonSerializer.Deserialize<JsonDocument>(str, opts);
                    if (e != null)
                        res.Add(ReadFrom(e, options));
                }
            }

            return res.ToArray();
        }

        public static Event ReadFrom(JsonDocument doc, JsonSerializerOptions? options = null)
            => ReadFrom(doc.RootElement, options);

        public static Event ReadFrom(JsonElement element, JsonSerializerOptions? options = null)
        {
            options ??= DefaultJsonOptions;
            var type = element.GetProperty("type").GetString();
            var json = element.ToString() ?? "{}";
            if (Enum.TryParse<EventType>(type, out var eventType))
            {
                return eventType switch
                {
                    EventType.META_NEW_OPTION => JsonSerializer.Deserialize<NewOption>(json, options)!,
                    EventType.META_UPDATE_FRACTIONS => JsonSerializer.Deserialize<UpdateFractions>(json, options)!,
                    EventType.META_NEW_CHARITY => JsonSerializer.Deserialize<NewCharity>(json, options)!,
                    EventType.META_UPDATE_CHARITY => JsonSerializer.Deserialize<UpdateCharity>(json, options)!,
                    EventType.META_CHARITY_PARTITION => JsonSerializer.Deserialize<CharityPartition>(json, options)!,
                    EventType.DONA_NEW => JsonSerializer.Deserialize<NewDonation>(json, options)!,
                    EventType.DONA_UPDATE_CHARITY => JsonSerializer.Deserialize<UpdateCharityForDonation>(json, options)!,
                    EventType.DONA_CANCEL => JsonSerializer.Deserialize<CancelDonation>(json, options)!,
                    EventType.CONV_LIQUIDATE => JsonSerializer.Deserialize<ConvLiquidate>(json, options)!,
                    EventType.CONV_EXIT => JsonSerializer.Deserialize<ConvExit>(json, options)!,
                    EventType.CONV_TRANSFER => JsonSerializer.Deserialize<ConvTransfer>(json, options)!,
                    EventType.CONV_ENTER => JsonSerializer.Deserialize<ConvEnter>(json, options)!,
                    EventType.CONV_INVEST => JsonSerializer.Deserialize<ConvInvest>(json, options)!,
                    EventType.CONV_INCREASE_CASH => JsonSerializer.Deserialize<IncreaseCash>(json, options)!,
                    EventType.AUDIT => JsonSerializer.Deserialize<Audit>(json, options)!,
                    EventType.PRICE_INFO => JsonSerializer.Deserialize<PriceInfo>(json, options)!,
                    _ => throw new InvalidDataException("Invalid event type")
                };
            }
            else
                throw new InvalidDataException("Invalid event type");
        }

        public static JsonSerializerOptions DefaultJsonOptions { get; } = new()
        {
            Converters = {new JsonStringEnumConverter()},
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public abstract IEnumerable<ValidationMessage> Validate();

        public string ToJsonString(JsonSerializerOptions? options = null)
        {
            Timestamp = Timestamp.ToUniversalTime();
            return JsonSerializer.Serialize(this, GetType(), options ?? DefaultJsonOptions);
        }
    }

    public class NoneEvent : Event
    {
        public static Event Instance { get; } = new NoneEvent();
        public override EventType Type => EventType.NONE;

        public override IEnumerable<ValidationMessage> Validate()
        {
            yield break;
        }
    }

    public class NewOption : Event
    {
        public override EventType Type => EventType.META_NEW_OPTION;
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Reinvestment_fraction { get; set; } = 0.45m;
        public decimal FutureFund_fraction { get; set; } = 0.1m;
        public decimal Charity_fraction { get; set; } = 0.45m;
        public decimal Bad_year_fraction { get; set; } = 0.01m;

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (Reinvestment_fraction is < 0 or > 1)
                yield return new ValidationMessage(nameof(Reinvestment_fraction),
                    "Reinvestment fraction out of range.");
            if (FutureFund_fraction is < 0 or > 1)
                yield return new ValidationMessage(nameof(FutureFund_fraction), "Future Fund fraction out of range.");
            if (Charity_fraction is < 0 or > 1)
                yield return new ValidationMessage(nameof(Charity_fraction), "Charity fraction fraction out of range.");
            if (Bad_year_fraction is < 0 or > 0.1m)
                yield return new ValidationMessage(nameof(Bad_year_fraction),
                    "Bad year fraction fraction out of range.");

            if (string.IsNullOrWhiteSpace(Code))
                yield return new ValidationMessage(nameof(Code), "Code is required.");
            if (Reinvestment_fraction + FutureFund_fraction + Charity_fraction != 1)
                yield return new ValidationMessage(nameof(Reinvestment_fraction), "Fractions should add up to 1");
        }
    }

    public class UpdateFractions : Event
    {
        public override EventType Type => EventType.META_UPDATE_FRACTIONS;
        public string Code { get; set; } = "";
        public decimal Reinvestment_fraction { get; set; }
        public decimal FutureFund_fraction { get; set; }
        public decimal Charity_fraction { get; set; }
        public decimal Bad_year_fraction { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (Reinvestment_fraction is < 0 or > 1)
                yield return new ValidationMessage(nameof(Reinvestment_fraction),
                    "Reinvestment fraction out of range.");
            if (FutureFund_fraction is < 0 or > 1)
                yield return new ValidationMessage(nameof(FutureFund_fraction), "Future Fund fraction out of range.");
            if (Charity_fraction is < 0 or > 1)
                yield return new ValidationMessage(nameof(Charity_fraction), "Charity fraction fraction out of range.");
            if (Bad_year_fraction is < 0 or > 0.1m)
                yield return new ValidationMessage(nameof(Bad_year_fraction),
                    "Bad year fraction fraction out of range.");

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
                yield return new ValidationMessage(nameof(Code), "Field is required.");
            if (string.IsNullOrWhiteSpace(Name))
                yield return new ValidationMessage(nameof(Name), "Field is required.");
        }
    }

    public class UpdateCharity : Event
    {
        public override EventType Type => EventType.META_UPDATE_CHARITY;
        public string Code { get; set; } = "";
        public string? Name { get; set; }
        public string? Bank_account_no { get; set; }
        public string? Bank_name { get; set; }
        public string? Bank_bic { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Code))
                yield return new ValidationMessage(nameof(Code), "Code is required.");
        }
    }

    public class CharityPartition : Event
    {
        public override EventType Type => EventType.META_CHARITY_PARTITION;
        public string Charity { get; set; } = "";
        public FractionSpec[] Partitions { get; set; } = Array.Empty<FractionSpec>();

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Charity))
                yield return new ValidationMessage(nameof(Charity), "Charity is required.");
            foreach (var msg in Partitions.SelectMany(p => p.Validate()))
                yield return msg;
        }
    }

    public class NewDonation : Event
    {
        public override EventType Type => EventType.DONA_NEW;
        public DateTimeOffset Execute_timestamp { get; set; }
        public string Donation { get; set; } = "";
        public string Donor { get; set; } = "";
        public string Charity { get; set; } = "";
        public string Option { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Exchanged_amount { get; set; }
        public string Transaction_reference { get; set; } = "";
        public string? Exchange_reference { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Donation))
                yield return new ValidationMessage(nameof(Donation), "Field is required");
            if (string.IsNullOrWhiteSpace(Donor))
                yield return new ValidationMessage(nameof(Donor), "Field is required");
            if (string.IsNullOrWhiteSpace(Charity))
                yield return new ValidationMessage(nameof(Charity), "Field is required");
            if (string.IsNullOrWhiteSpace(Option))
                yield return new ValidationMessage(nameof(Option), "Field is required");
            if (string.IsNullOrWhiteSpace(Currency))
                yield return new ValidationMessage(nameof(Currency), "Field is required");
            if (Amount <= 0)
                yield return new ValidationMessage(nameof(Amount), "Donation should have positive Amount");
        }
    }

    public class UpdateCharityForDonation : Event
    {
        public override EventType Type => EventType.DONA_UPDATE_CHARITY;
        public string Donation { get; set; } = "";
        public string Charity { get; set; } = "";

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Donation))
                yield return new ValidationMessage(nameof(Donation), "Field is required");
            if (string.IsNullOrWhiteSpace(Charity))
                yield return new ValidationMessage(nameof(Charity), "Field is required");
        }
    }

    public class CancelDonation : Event
    {
        public override EventType Type => EventType.DONA_CANCEL;
        public string Donation { get; set; } = "";

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Donation))
                yield return new ValidationMessage(nameof(Donation), "Field is required");
        }
    }

    public class ConvLiquidate : Event
    {
        public override EventType Type => EventType.CONV_LIQUIDATE;
        public string Option { get; set; } = "";
        public decimal Invested_amount { get; set; }
        public decimal Cash_amount { get; set; }
        public string Transaction_reference { get; set; } = "";

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Option))
                yield return new ValidationMessage(nameof(Option), "Field is required");
        }
    }

    public class ConvExit : Event
    {
        public override EventType Type => EventType.CONV_EXIT;
        public string Option { get; set; } = "";
        public decimal Amount { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Option))
                yield return new ValidationMessage(nameof(Option), "Field is required");
            if (Amount <= 0)
                yield return new ValidationMessage(nameof(Amount), "Amount must be positive");
        }
    }

    public class ConvTransfer : Event
    {
        public override EventType Type => EventType.CONV_TRANSFER;
        public string Charity { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; }
        public string Exchanged_currency { get; set; } = "";
        public decimal? Exchanged_amount { get; set; }
        public string Transaction_reference { get; set; } = "";
        public string Exchange_reference { get; set; } = "";

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Charity))
                yield return new ValidationMessage(nameof(Charity), "Field is required");
            if (string.IsNullOrWhiteSpace(Currency))
                yield return new ValidationMessage(nameof(Currency), "Field is required");
            if (Amount <= 0)
                yield return new ValidationMessage(nameof(Amount), "Amount must be positive");
            if (Exchanged_amount <= 0)
                yield return new ValidationMessage(nameof(Amount), "Amount must be positive");
        }
    }

    public class IncreaseCash : Event
    {
        public override EventType Type => EventType.CONV_INCREASE_CASH;
        public string Option { get; set; } = "";
        public decimal Amount { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Option))
                yield return new ValidationMessage(nameof(Option), "Field is required");
            if (Amount < 0)
                yield return new ValidationMessage(nameof(Amount), "Amount must not be negative");
        }
    }

    public class ConvEnter : Event
    {
        public override EventType Type => EventType.CONV_ENTER;
        public string Option { get; set; } = "";
        public decimal Invested_amount { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Option))
                yield return new ValidationMessage(nameof(Option), "Field is required");
            if (Invested_amount < 0)
                yield return new ValidationMessage(nameof(Invested_amount), "Amount must not be negative");
        }
    }

    public class ConvInvest : Event
    {
        public override EventType Type => EventType.CONV_INVEST;
        public string Option { get; set; } = "";
        public decimal Invested_amount { get; set; }
        public decimal Cash_amount { get; set; }
        public string Transaction_reference { get; set; } = "";

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Option))
                yield return new ValidationMessage(nameof(Option), "Field is required");
            if (Invested_amount < 0)
                yield return new ValidationMessage(nameof(Invested_amount), "Amount must not be negative");
        }
    }

    public class Audit : Event
    {
        public override EventType Type => EventType.AUDIT;

        public string? PreviousHashCode { get; set; }
        public int? PreviousCount { get; set; }
        public string Hashcode { get; set; } = "";
        public int EventCount { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Hashcode))
                yield return new ValidationMessage(nameof(Hashcode), "Field is required");
            if (!string.IsNullOrWhiteSpace(PreviousHashCode) && !PreviousCount.HasValue)
                yield return new ValidationMessage(nameof(PreviousCount),
                    "PreviousCount is required when PreviousHashCode is not empty.");
            if (PreviousCount.HasValue && PreviousCount.Value >= EventCount)
                yield return new ValidationMessage(nameof(PreviousCount),
                    "PreviousCount must be less than EventCount.");
        }
    }

    public class PriceInfo : Event
    {
        public override EventType Type => EventType.PRICE_INFO;
        public string Option { get; set; } = "";
        public decimal Invested_amount { get; set; }

        public override IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Option))
                yield return new(nameof(Option), "Field is required");
            if (Invested_amount < 0m)
                yield return new(nameof(Invested_amount), "Invested amount cannot be negative");
        }
    }
}
