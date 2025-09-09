namespace FfAdmin.Calculator;

public record Donations(ImmutableDictionary<string, Donation> Values) : IModel<Donations>
{
    public static IMetaModel<Donations> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

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
    
public record Donations2(int NumberOfDonations) : IModel<Donations2, string, Donations2.Details>
{
    public static IMetaModel<Donations2, string, Details> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();
    
    private class Meta : IMetaModel<Donations2, string, Details>
    {
        public static Meta Instance { get; } = new();
        public int MaskBits(Donations2 header)
            => Math.Max(4, (int)Math.Floor(Math.Log2((header.NumberOfDonations - 1.0) / 16 + 1)));
        
        public Donations2 Empty => new(0);
        public Details EmptyDetail => new(ImmutableDictionary<string, Donation>.Empty);

        public Bucket? GetBucket(Donations2 header, string key)
            => GetBucket(key, MaskBits(header));
        private Bucket GetBucket(string key, int maskBits)
            => new (key.GetNumeric(), maskBits);
        public IEnumerable<string> GetKeysForEvent(Event e)
            => e switch
            {
                NewDonation nd => [nd.Donation],
                CancelDonation cd => [cd.Donation],
                UpdateCharityForDonation ucd => [ucd.Donation],
                _ => []
            };
    
        public IEventProcessor<Donations2> GetProcessor(IServiceProvider serviceProvider)
            => HeaderProcessor.Instance;

        public IEventProcessor<Details> GetDetailProcessor(IServiceProvider serviceProvider)
            => DetailsProcessor.Instance;

        public Details CleanDetail(Details detail, Bucket bucket)
            => new(detail.Values.Where(kvp => GetBucket(kvp.Key, bucket.MaskBits) == bucket).ToImmutableDictionary());

    }
    public Donations2 Increment() => new(NumberOfDonations + 1);
    public Donations2 Decrement() => new(NumberOfDonations - 1);
    public record Details(ImmutableDictionary<string, Donation> Values);
    
    private class HeaderProcessor : EventProcessor<Donations2>
    {
        public static EventProcessor<Donations2> Instance { get; } = new HeaderProcessor();
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new HeaderCalculation(previousContext, currentContext);
        }

        private sealed class HeaderCalculation(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override ValueTask<Donations2> CancelDonation(Donations2 model, CancelDonation e)
                => new(model.Decrement());

            protected override ValueTask<Donations2> NewDonation(Donations2 model, NewDonation e)
                => new(model.Increment());
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
    
}
