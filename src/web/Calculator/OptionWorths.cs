using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record OptionWorths(ImmutableDictionary<string, OptionWorth> Worths) : IModel<OptionWorths>
{
    public static IMetaModel<OptionWorths> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<OptionWorths>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override OptionWorths Empty => new(ImmutableDictionary<string, OptionWorth>.Empty);
        public override IEventProcessor<OptionWorths> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
    }
    public static implicit operator OptionWorths(ImmutableDictionary<string, OptionWorth> dict)
        => new(dict);

    public OptionWorths Mutate(string key, Func<OptionWorth, OptionWorth> mutator)
        => new(Worths.SetItem(key, mutator(Worths[key])));

    private class Impl(IContext<Donations> cDonations) : EventProcessor<OptionWorths>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cDonations);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Donations> cDonations)
            : BaseCalculation(previousContext, currentContext)
        {
            public ValueTask<Donations> CurrentDonations => GetCurrent(cDonations);
            public ValueTask<Donations> PreviousDonations => GetPrevious(cDonations);
            
            protected override async ValueTask<OptionWorths> NewDonation(OptionWorths model, NewDonation e)
            {
                return model.Mutate(e.Option, option =>
                {
                    var donation = new Donation(e.Donation, e.Timestamp, e.Execute_timestamp, e.Option, e.Charity,
                        (Real)e.Exchanged_amount, e.Currency, (Real)e.Amount);

                    return option with {UnenteredDonations = option.UnenteredDonations.Add(donation)};
                });
            }

            protected override async ValueTask<OptionWorths> CancelDonation(OptionWorths model, CancelDonation e)
            {
                var donation = (await PreviousDonations).Values.GetValueOrDefault(e.Donation);
                if (donation is null)
                    return model;

                return model.Mutate(donation.OptionId, option =>
                {
                    var unenteredDonation = option.UnenteredDonations.FirstOrDefault(d => d.Id == e.Donation);
                    if (unenteredDonation is null)
                    {
                        if (option.DonationFractions.ContainsKey(e.Donation))
                            return option with {Cash = option.Cash - donation.Amount, DonationFractions = option.DonationFractions.Remove(e.Donation)};
                        return option;
                    }

                    return option with {UnenteredDonations = option.UnenteredDonations.Remove(unenteredDonation)};
                });
            }

            protected override async ValueTask<OptionWorths> NewOption(OptionWorths model, NewOption e)
            {
                var worth = new OptionWorth(e.Code, e.Timestamp, (Real)0, (Real)0, FractionSet.Empty,
                    ImmutableList<Donation>.Empty);
                return model with {Worths = model.Worths.Add(e.Code, worth)};
            }

            protected override async ValueTask<OptionWorths> ConvEnter(OptionWorths model, ConvEnter e)
            {
                return model.Mutate(e.Option, option =>
                {
                    var oldWorth = (Real)(e.Invested_amount + option.Cash);

                    var donations = option.UnenteredDonations.ToLookup(ue => ue.ExecuteTimestamp <= e.Timestamp);
                    var newCash = donations[true].Sum(ue => ue.Amount);

                    var fractions = model.Worths[e.Option].DonationFractions;
                    fractions = fractions.AddRange(from don in donations[true]
                        select (don.Id, don.Amount / (oldWorth == 0 ? 1 : oldWorth)));

                    return option with
                    {
                        Cash = option.Cash + newCash,
                        Timestamp = e.Timestamp,
                        DonationFractions = fractions,
                        Invested = e.Invested_amount,
                        UnenteredDonations = donations[false].ToImmutableList()
                    };
                });
            }

            protected override async ValueTask<OptionWorths> ConvInvest(OptionWorths model, ConvInvest e)
                => model.Mutate(e.Option, option =>
                    option with {Timestamp = e.Timestamp, Invested = e.Invested_amount, Cash = e.Cash_amount});

            protected override async ValueTask<OptionWorths> ConvLiquidate(OptionWorths model, ConvLiquidate e)
                => model.Mutate(e.Option, option =>
                    option with {Timestamp = e.Timestamp, Cash = e.Cash_amount, Invested = e.Invested_amount});

            protected override async ValueTask<OptionWorths> ConvExit(OptionWorths model, ConvExit e)
                => model.Mutate(e.Option, option =>
                    option with {Timestamp = e.Timestamp, Cash = option.Cash - e.Amount});

            protected override async ValueTask<OptionWorths> PriceInfo(OptionWorths model, PriceInfo e)
                => model.Mutate(e.Option, option =>
                    option with {Timestamp = e.Timestamp, Invested = e.Invested_amount});

            protected override async ValueTask<OptionWorths> IncreaseCash(OptionWorths model, IncreaseCash e)
                => model.Mutate(e.Option, option =>
                    option with {Timestamp = e.Timestamp, Cash = option.Cash + e.Amount});
            
            protected override async ValueTask<OptionWorths> ConvInflation(OptionWorths model, ConvInflation e)
                => model.Mutate(e.Option, option =>
                    option with {Timestamp = e.Timestamp, Invested = e.Invested_amount});
        }
    }
}

public record OptionWorth(string Id, DateTimeOffset Timestamp, Real Invested, Real Cash,
    FractionSet DonationFractions, ImmutableList<Donation> UnenteredDonations)
{
    public Real TotalWorth => Invested + Cash;
}

public record OptionWorths2(int NumberOfDonations, Real TotalUnentered, ImmutableDictionary<string, OptionWorths2.Header> Worths)
    : IModel<OptionWorths2, string, OptionWorths2.Details>
{
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();
    public static IMetaModel<OptionWorths2, string, Details> GetMetaModel()
        => Meta.Instance;
    private class Meta : IMetaModel<OptionWorths2, string, Details>
    {
        public static Meta Instance { get; } = new();
        public int MaskBits(OptionWorths2 header)
            => Math.Max(4, (int)Math.Floor(Math.Log2((header.NumberOfDonations - 1.0) / 16 + 1)));

        public OptionWorths2 Empty => new(0, 0,ImmutableDictionary<string, Header>.Empty);
        public Details EmptyDetail => new(ImmutableDictionary<string, Real>.Empty);
        
        public Bucket? GetBucket(OptionWorths2 header, string key)
            => GetBucket(key, MaskBits(header));
        private Bucket GetBucket(string key, int maskBits)
            => new (key.GetNumeric(), maskBits);
        public string? GetKeyForEvent(Event e)
        {
            return e switch
            {
                NewDonation nd => nd.Donation,
                CancelDonation cd => cd.Donation,
                UpdateCharityForDonation ucd => ucd.Donation,
                _ => null
            };
        }
        
        public IEventProcessor<OptionWorths2> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
        public IEventProcessor<Details> GetDetailProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<DetailsImpl>(serviceProvider);
        
        public Details CleanDetail(Details detail, Bucket bucket)
            => new(detail.Shares.Where(kvp => GetBucket(kvp.Key, bucket.MaskBits) == bucket).ToImmutableDictionary());

        public bool IsMegaEvent(Event e)
            => e is ConvEnter;
    }

    public record Header(
        string Id,
        DateTimeOffset Timestamp,
        Real Invested,
        Real Cash,
        Real DonationFractionDivisor,
        ImmutableList<Donation> UnenteredDonations)
    {
        public Real CalculateFactor()
            => DonationFractionDivisor == 0 ? 1 : (Invested + Cash) / DonationFractionDivisor;
    }
    
    public record Details(ImmutableDictionary<string, Real> Shares);
    
    private class Impl : EventProcessor<OptionWorths2>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext);
        }

        private sealed class Calc(IContext previousContext, IContext currentContext)
            : BaseCalculation(previousContext, currentContext)
        {
            protected override ValueTask<OptionWorths2> NewOption(OptionWorths2 model, NewOption e)
                => new(model with
                {
                    Worths = model.Worths.Add(e.Code,
                        new Header(e.Code, e.Timestamp, 0, 0, 0, ImmutableList<Donation>.Empty))
                });

            protected override ValueTask<OptionWorths2> NewDonation(OptionWorths2 model, NewDonation e)
                => new(new OptionWorths2(
                    model.NumberOfDonations + 1,
                    model.TotalUnentered + (Real)e.Exchanged_amount,
                    model.Worths.SetItem(e.Option,
                        model.Worths[e.Option] with
                        {
                            UnenteredDonations = model.Worths[e.Option].UnenteredDonations.Add(
                                new Donation(e.Donation, e.Timestamp, e.Execute_timestamp, e.Option, e.Charity,
                                    (Real)e.Exchanged_amount, e.Currency, (Real)e.Amount))
                        })));

            protected override ValueTask<OptionWorths2> CancelDonation(OptionWorths2 model, CancelDonation e)
            {
                var search = (from w in model.Worths.Keys
                    from ud in model.Worths[w].UnenteredDonations
                    where ud.Id == e.Donation
                    select (option: w, donation: ud)).FirstOrDefault();
                if (search is (null, _) or (_,null))
                    return new(model);

                var updatedWorth = model.Worths[search.option] with
                {
                    UnenteredDonations = model.Worths[search.option].UnenteredDonations.Remove(search.donation)
                }; 

                return new(model with
                {
                    NumberOfDonations = model.NumberOfDonations - 1,
                    Worths = model.Worths.SetItem(search.option, updatedWorth),
                    TotalUnentered = model.TotalUnentered - search.donation.Amount
                });
            }

            protected override ValueTask<OptionWorths2> ConvEnter(OptionWorths2 model, ConvEnter e)
            {
                var worth = model.Worths[e.Option];
                var split = worth.UnenteredDonations.ToLookup(d => d.ExecuteTimestamp <= e.Timestamp);
                var entering = split[true].Sum(d => d.Amount);
                if (entering == 0m)
                    return new(model);

                var factor = (worth with { Invested = e.Invested_amount }).CalculateFactor();

                return new(model with
                {
                    TotalUnentered = model.TotalUnentered - entering,
                    Worths = model.Worths.SetItem(e.Option,
                        model.Worths[e.Option] with
                        {
                            Timestamp = e.Timestamp,
                            Invested = e.Invested_amount,
                            Cash = worth.Cash + entering,
                            DonationFractionDivisor = worth.DonationFractionDivisor + entering / factor,
                            UnenteredDonations = split[false].ToImmutableList()
                        })
                });
            }

            protected override ValueTask<OptionWorths2> ConvExit(OptionWorths2 model, ConvExit e)
                => UpdateAmounts(model, e.Option, e.Timestamp, cash: c => c - e.Amount);

            protected override ValueTask<OptionWorths2> ConvInvest(OptionWorths2 model, ConvInvest e)
                => UpdateAmounts(model, e.Option, e.Timestamp, _ => e.Invested_amount, _ => e.Cash_amount);

            protected override ValueTask<OptionWorths2> ConvLiquidate(OptionWorths2 model, ConvLiquidate e)
                => UpdateAmounts(model, e.Option, e.Timestamp, _ => e.Invested_amount, _ => e.Cash_amount);

            protected override ValueTask<OptionWorths2> PriceInfo(OptionWorths2 model, PriceInfo e)
                => UpdateAmounts(model, e.Option, e.Timestamp, _ => e.Invested_amount);

            protected override ValueTask<OptionWorths2> IncreaseCash(OptionWorths2 model, IncreaseCash e)
                => UpdateAmounts(model, e.Option, e.Timestamp, cash: c => c + e.Amount);
            
            private ValueTask<OptionWorths2> UpdateAmounts(OptionWorths2 model, string option, DateTimeOffset timestamp, Func<Real, Real>? invested = null,
                Func<Real, Real>? cash = null)
            {
                invested ??= x => x;
                cash ??= x => x;
                var worth = model.Worths[option];
                return new(model with
                {
                    Worths = model.Worths.SetItem(option,
                        worth with {Timestamp = timestamp, Invested = invested(worth.Invested), Cash = cash(worth.Cash)})
                });
            }
        }
    }

    private class DetailsImpl(IContext<OptionWorths2> cOptionWorths, IContext<Donations2, string, Donations2.Details> cDonations) : EventProcessor<Details>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(cOptionWorths, cDonations, previousContext, currentContext);
        }
        private class Calc(IContext<OptionWorths2> cOptionWorths, IContext<Donations2, string, Donations2.Details> cDonations, IContext previousContext, IContext currentContext)
            : BaseCalculation(previousContext, currentContext)
        {
            public async ValueTask<Header> CurrentOptionWorthFor(string optionId)
                => (await GetCurrent(cOptionWorths)).Worths[optionId];
            public async ValueTask<Donation> PreviousDonationFor(string donationId)
                => (await GetPrevious(cDonations, await GetPrevious(cDonations), donationId)).Values[donationId];
            public async ValueTask<Donation> CurrentDonationFor(string donationId)
                => (await GetCurrent(cDonations, await GetPrevious(cDonations), donationId)).Values[donationId];
            
            protected override ValueTask<Details> NewDonation(Details model, NewDonation e)
                => new(new Details(model.Shares.Add(e.Donation, 0)));

            protected override ValueTask<Details> CancelDonation(Details model, CancelDonation e)
                => new(new Details(model.Shares.Remove(e.Donation)));

            protected override async ValueTask<Details> ConvEnter(Details model, ConvEnter e)
            {
                var optionWorth = await CurrentOptionWorthFor(e.Option);
                var factor = optionWorth.CalculateFactor();
                var newDetails = ImmutableDictionary<string, Real>.Empty.ToBuilder();
                foreach (var share in model.Shares)
                {
                    if (share.Value > 0) // Already entered
                    {
                        newDetails[share.Key] = share.Value;
                        continue;
                    }
                    var donation = await CurrentDonationFor(share.Key);
                    if(donation.OptionId != e.Option  // Not the current option
                        || donation.ExecuteTimestamp > e.Timestamp) // Not to be executed yet
                    {
                        newDetails[share.Key] = share.Value;
                        continue;
                    }

                    newDetails[share.Key] = donation.Amount / factor;
                }

                return new(newDetails.ToImmutable());
            }
        }
    }
}
