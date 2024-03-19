using System.Text.Json;
using System.Text.Json.Serialization;

namespace FfAdmin.Calculator;

[JsonConverter(typeof(AggregatedDonationsAndTransfers.JsonConverter))]
public record AggregatedDonationsAndTransfers(ImmutableDictionary<AggregatedDonationsAndTransfers.Key, AggregatedDonationsAndTransfers.Value> Data)
    : IModel<AggregatedDonationsAndTransfers>
{
    public record struct Key(int Year, string Charity);

    public record struct Value(MoneyBag Donated, MoneyBag Transferred)
    {
        public static Value Empty => new Value(MoneyBag.Empty, MoneyBag.Empty);
    }

    public AggregatedDonationsAndTransfers Manipulate(Key key, Func<Value, Value> manipulator)
        => new(Data.SetItem(key, manipulator(Data.GetValueOrDefault(key, Value.Empty))));

    public AggregatedDonationsAndTransfers AddDonated(Key key, string currency, Real amount)
        => Manipulate(key, v => v with {Donated = v.Donated.Add(currency, amount)});

    public AggregatedDonationsAndTransfers AddTransferred(Key key, string currency, Real amount)
        => Manipulate(key, v => v with {Transferred = v.Transferred.Add(currency, amount)});


    public static AggregatedDonationsAndTransfers Empty { get; } = new(ImmutableDictionary<Key, Value>.Empty);
    public static IEventProcessor<AggregatedDonationsAndTransfers> Processor { get; } = new Impl();

    public class Impl : EventProcessor<AggregatedDonationsAndTransfers>
    {
        public override AggregatedDonationsAndTransfers Start => Empty;

        protected override AggregatedDonationsAndTransfers NewDonation(AggregatedDonationsAndTransfers model, IContext context, NewDonation e)
        {
            var option = context.GetContext<Options>().Values[e.Option];
            var key = new Key(e.Timestamp.Year, e.Charity);
            return model.AddDonated(key, option.Currency, e.Exchanged_amount);
        }

        protected override AggregatedDonationsAndTransfers CancelDonation(AggregatedDonationsAndTransfers model, IContext previousContext, IContext context,
            CancelDonation e)
        {
            var donation = previousContext.GetContext<Donations>().Values[e.Donation];
            var option = context.GetContext<Options>().Values[donation.OptionId];
            return model.AddDonated(new(e.Timestamp.Year, donation.CharityId), option.Currency, -donation.Amount);
        }

        protected override AggregatedDonationsAndTransfers UpdateCharityForDonation(AggregatedDonationsAndTransfers model, IContext previousContext,
            IContext context, UpdateCharityForDonation e)
        {
            var donation = previousContext.GetContext<Donations>().Values[e.Donation];
            var option = context.GetContext<Options>().Values[donation.OptionId];
            return model.AddDonated(new(e.Timestamp.Year, donation.CharityId), option.Currency, -donation.Amount)
                .AddDonated(new Key(e.Timestamp.Year, e.Charity), option.Currency, donation.Amount);
        }

        protected override AggregatedDonationsAndTransfers ConvTransfer(AggregatedDonationsAndTransfers model, IContext context, ConvTransfer e)
        {
            var key = new Key(e.Timestamp.Year, e.Charity);
            return model.AddTransferred(key, e.Currency, e.Amount);
        }
    }

    public class JsonConverter : System.Text.Json.Serialization.JsonConverter<AggregatedDonationsAndTransfers>
    {
        public override AggregatedDonationsAndTransfers? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dict = JsonSerializer.Deserialize<ImmutableDictionary<int, ImmutableDictionary<string, Value>>>(ref reader, options);
            var q = (from kvp in dict
                from kvp2 in kvp.Value
                select (new Key(kvp.Key, kvp2.Key), kvp2.Value)).ToImmutableDictionary();
            return new(q);
        }

        public override void Write(Utf8JsonWriter writer, AggregatedDonationsAndTransfers value, JsonSerializerOptions options)
        {
            var q = value.Data.GroupBy(kvp => kvp.Key.Year)
                .Select(g1 => (g1.Key, g1.ToImmutableDictionary(x => x.Key.Charity, x => x.Value)))
                .ToImmutableDictionary();
            JsonSerializer.Serialize(writer, q, options);
        }
    }
}
