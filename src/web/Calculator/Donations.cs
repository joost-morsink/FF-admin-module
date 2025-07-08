namespace FfAdmin.Calculator;

public record Donations(ImmutableDictionary<string, Donation> Values) : IModel<Donations>
{
    public static IMetaModel<Donations> GetMetaModel()
        => Meta.Instance;

    private class Meta : IModel<Donations>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override Donations Empty => new(ImmutableDictionary<string, Donation>.Empty);

        public override IEventProcessor<Donations> GetProcessor(IServiceProvider serviceProvider)
            => new Impl();
    }
    public static implicit operator Donations(ImmutableDictionary<string, Donation> values)
        => new(values);

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

            protected override async ValueTask<Donations> CancelDonation(Donations model, CancelDonation e)
                => new(model.Values.Remove(e.Donation));

            protected override async ValueTask<Donations> NewDonation(Donations model, NewDonation e)
                => new(model.Values.Add(e.Donation,
                    new Donation(e.Donation, e.Timestamp, e.Execute_timestamp, e.Option, e.Charity, (Real)e.Exchanged_amount, e.Currency, (Real)e.Amount)));

            protected override async ValueTask<Donations> UpdateCharityForDonation(Donations model, UpdateCharityForDonation e)
                => new(model.Values.SetItem(e.Donation,
                    model.Values[e.Donation] with {CharityId = e.Charity}));
        }
    }
}
public record Donation(string Id, DateTimeOffset Timestamp, DateTimeOffset ExecuteTimestamp, string OptionId,
    string CharityId, Real Amount, string OriginalCurrency, Real OriginalAmount);
    
