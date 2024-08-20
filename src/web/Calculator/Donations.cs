using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record Donations(ImmutableDictionary<string, Donation> Values) : IModel<Donations>
{
    public static implicit operator Donations(ImmutableDictionary<string, Donation> values)
        => new(values);
    public static Donations Empty { get; } = new(ImmutableDictionary<string, Donation>.Empty);

    public static IEventProcessor<Donations> GetProcessor(IServiceProvider services)
        => new Impl();

    public bool Contains(string id)
        => Values.ContainsKey(id);
    
    private class Impl : EventProcessor<Donations>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {

            protected override Donations CancelDonation(Donations model, CancelDonation e)
                => new(model.Values.Remove(e.Donation));

            protected override Donations NewDonation(Donations model, NewDonation e)
                => new(model.Values.Add(e.Donation,
                    new Donation(e.Donation, e.Timestamp, e.Execute_timestamp, e.Option, e.Charity, (Real)e.Exchanged_amount)));

            protected override Donations UpdateCharityForDonation(Donations model, UpdateCharityForDonation e)
                => new(model.Values.SetItem(e.Donation,
                    model.Values[e.Donation] with {CharityId = e.Charity}));
        }
    }
}
public record Donation(string Id, DateTimeOffset Timestamp, DateTimeOffset ExecuteTimestamp, string OptionId,
    string CharityId, Real Amount);
    
