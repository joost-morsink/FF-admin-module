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

public record Donations2(int NumberOfDonations) : IPartitionedModel<Donations2, string, Donations2.Details>, IPartitioningParameter
{
    public Donations2 Increment() => new(NumberOfDonations + 1);
    public Donations2 Decrement() => new(NumberOfDonations - 1);
    public record Details(ImmutableDictionary<string, Donation> Values);
    
    private int MaskBits => (int)Math.Log2(Math.Min(2, NumberOfDonations / 4));
    int IPartitioningParameter.MaskBits => MaskBits;
    public static Donations2 Empty => new(0);
    public static Details EmptyDetail => new(ImmutableDictionary<string, Donation>.Empty);
    public static Bucket? GetBucket(Donations2 header, string key)
    {
        return new (key.GetNumeric(), header.MaskBits);
    }
    
    public static string? GetKeyForEvent(Event e)
        => e switch
        {
            NewDonation nd => nd.Donation,
            CancelDonation cd => cd.Donation,
            UpdateCharityForDonation ucd => ucd.Donation,
            _ => null
        };
    
    public static IEventProcessor<Donations2> GetProcessor(IServiceProvider serviceProvider)
        => HeaderProcessor.Instance;
    private class HeaderProcessor : EventProcessor<Donations2>
    {
        public static EventProcessor<Donations2> Instance { get; } = new HeaderProcessor();
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new HeaderCalculation(previousContext, currentContext);
        }

        private sealed class HeaderCalculation(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override Donations2 CancelDonation(Donations2 model, CancelDonation e)
                => model.Decrement();

            protected override Donations2 NewDonation(Donations2 model, NewDonation e)
                => model.Increment();
        }
    }

    private class DetailsProcessor : EventProcessor<Details>
    {
        public static EventProcessor<Details> Instance { get; } = new DetailsProcessor();
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new DetailCalculation(previousContext, currentContext);
        }
        private sealed class DetailCalculation(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override ValueTask<Details> CancelDonation(Details model, CancelDonation e)
                => new(model with {Values = model.Values.Remove(e.Donation)});

            protected override ValueTask<Details> NewDonation(Details model, NewDonation e)
                => new(model with {Values = model.Values.Add(e.Donation,
                    new Donation(e.Donation, e.Timestamp, e.Execute_timestamp, e.Option, e.Charity, (Real)e.Exchanged_amount, e.Currency, (Real)e.Amount))});

            protected override ValueTask<Details> UpdateCharityForDonation(Details model, UpdateCharityForDonation e)
                => new(model with {Values = model.Values.SetItem(e.Donation,
                    model.Values[e.Donation] with {CharityId = e.Charity})});
        }
    }

    public static IEventProcessor<Details> GetDetailProcessor(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
    
}
public record Donation(string Id, DateTimeOffset Timestamp, DateTimeOffset ExecuteTimestamp, string OptionId,
    string CharityId, Real Amount, string OriginalCurrency, Real OriginalAmount);
    
