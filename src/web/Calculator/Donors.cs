using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record Donors(ImmutableDictionary<string, ImmutableList<string>> Values) : IModel<Donors>
{
    public static IMetaModel<Donors> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<Donors>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override Donors Empty => new(ImmutableDictionary<string, ImmutableList<string>>.Empty);
        public override IEventProcessor<Donors> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
    }
    public static implicit operator Donors(ImmutableDictionary<string, ImmutableList<string>> dict)
        => new(dict);

    public Donors Add(string donor, string donation)
        => Values.SetItem(donor, Values.GetValueOrDefault(donor, ImmutableList<string>.Empty).Add(donation));
    private class Impl : EventProcessor<Donors>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
            => new Calc(previousContext, currentContext);

        private sealed class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override async ValueTask<Donors> NewDonation(Donors model, NewDonation e)
                => model.Add(e.Donor, e.Donation);
        }
    }
}

public record Donors2(int DonorCount, int DonationCount)
    : IModel<Donors2, string, Donors2.Details>
{
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();
    public static IMetaModel<Donors2, string, Details> GetMetaModel()
        => Meta.Instance;
    
    private class Meta : IMetaModel<Donors2, string, Details>
    {
        public static Meta Instance { get; } = new();

        public int MaskBits(Donors2 header)
            => MetaModels.MaskBitsForCount(header.DonorCount);

        public Donors2 Empty => new(0, 0);
        public IEventProcessor<Donors2> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);

        public IEventProcessor<Details> GetDetailProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<DetailsImpl>(serviceProvider); 

        public Details CleanDetail(Details detail, Bucket bucket)
        {
            var cleanedValues = detail.Values
                .Where(kvp => GetBucket(kvp.Key, bucket.MaskBits) == bucket)
                .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
            return new Details(cleanedValues);
        }

        public Details EmptyDetail => new(ImmutableDictionary<string, ImmutableList<string>>.Empty);
        public Bucket? GetBucket(Donors2 header, string key)
            => GetBucket(key, MaskBits(header));
        private Bucket GetBucket(string key, int maskBits)
            => new (key.GetNumeric(), maskBits);
        public string? GetKeyForEvent(Event e)
            => e switch
            {
                NewDonation nd => nd.Donor,
                _ => null
            };
    }
    public record Details(ImmutableDictionary<string, ImmutableList<string>> Values);
    private class Impl(IContext<Donors2,string,Details> cDonors) : EventProcessor<Donors2> 
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
            => new Calc(cDonors, previousContext, currentContext);

        private sealed class Calc(IContext<Donors2, string, Details> cDonors, IContext previousContext, IContext currentContext)
            : BaseCalculation(previousContext, currentContext)
        {
            public async ValueTask<bool> HasPreviousDonor(string id)
                => (await GetPrevious(cDonors,await GetPrevious(cDonors), id)).Values.ContainsKey(id);

            protected override async ValueTask<Donors2> NewDonation(Donors2 model, NewDonation e)
            {
                var isNewDonor = !await HasPreviousDonor(e.Donor);
                return isNewDonor
                    ? new(model.DonorCount + 1, model.DonationCount + 1)
                    : model with {DonationCount = model.DonationCount + 1};
            }
        }
    }

    private class DetailsImpl : EventProcessor<Details>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext);
        }
        private class Calc(IContext previousContext, IContext currentContext) : BaseCalculation(previousContext, currentContext)
        {
            protected override async ValueTask<Details> NewDonation(Details model, NewDonation e)
            {
                return model with
                {
                    Values = model.Values.SetItem(e.Donor,
                        model.Values.GetValueOrDefault(e.Donor, ImmutableList<string>.Empty).Add(e.Donation))
                };
            }
        }
    }
}
