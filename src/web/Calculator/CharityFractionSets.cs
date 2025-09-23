using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record CharityFractionSets(ImmutableDictionary<string, ImmutableDictionary<string, Real>> Shares) : IModel<CharityFractionSets>
{
    public static IMetaModel<CharityFractionSets> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();
    
    private class Meta : IModel<CharityFractionSets>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override CharityFractionSets Empty { get; } = new(ImmutableDictionary<string, ImmutableDictionary<string, Real>>.Empty);
        public override IEventProcessor<CharityFractionSets> GetProcessor(IServiceProvider services)
            => ActivatorUtilities.CreateInstance<Impl>(services);
    }
    public static implicit operator CharityFractionSets(ImmutableDictionary<string, ImmutableDictionary<string, Real>> fractions)
        => new(fractions);
    
    private class Impl(IContext<OptionWorths2, string, OptionWorths2.Details> cOptionWorths,
            IContext<Donations2, string, Donations2.Details> cDonations)
        : EventProcessor<CharityFractionSets>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(cOptionWorths, cDonations, previousContext, currentContext);
        }

        private sealed class Calc(IContext<OptionWorths2, string, OptionWorths2.Details> cOptionWorths,
            IContext<Donations2, string, Donations2.Details> cDonations,
            IContext previousContext, IContext currentContext)
            : BaseCalculation(previousContext, currentContext)
        {
            public async ValueTask<OptionWorths2> CurrentOptionWorths()
                => await GetCurrent(cOptionWorths);
            public async ValueTask<OptionWorths2.Details> CurrentOptionWorths(string id)
                => await GetCurrent(cOptionWorths, await CurrentOptionWorths(), id);

            public async ValueTask<Donation> CurrentDonation(string id)
                => (await GetCurrent(cDonations, await GetCurrent(cDonations), id)).Values[id];

            protected override ValueTask<CharityFractionSets> NewOption(CharityFractionSets model, NewOption e)
                => new(model.Shares.Add(e.Code, ImmutableDictionary<string, Real>.Empty));
            

            protected override async ValueTask<CharityFractionSets> ConvEnter(CharityFractionSets model, ConvEnter e)
            {
                var ow = await CurrentOptionWorths();
                var entering = ow.Worths[e.Option].EnteringDonations;
                if (entering is null) // No donations have entered
                    return model;
                var addedFractions = from d in entering.Donations
                    group d by d.CharityId
                    into g
                    select (g.Key, AddedFraction: g.Sum(d => d.Amount) / entering.Factor);
                var option = model.Shares[e.Option];
                var newOption = addedFractions.Aggregate(option, (acc, af) =>
                    acc.ContainsKey(af.Key)
                        ? acc.SetItem(af.Key, acc[af.Key] + af.AddedFraction)
                        : acc.Add(af.Key, af.AddedFraction));
                return model.Shares.SetItem(e.Option, newOption);
            }

            protected override async ValueTask<CharityFractionSets> UpdateCharityForDonation(CharityFractionSets model, UpdateCharityForDonation e)
            {
                var ow = await CurrentOptionWorths(e.Donation);
                var donationShare = ow.Shares[e.Donation];
                if (!donationShare.IsEntered)
                    return model;

                var donation = await CurrentDonation(e.Donation);
                if (donation.CharityId == e.Charity)
                    return model;
                var option = model.Shares[donation.OptionId];

                var newOption = option
                    .Mutate(donation.CharityId, x => x - donationShare.Share)
                    .Mutate(e.Charity, x => x + donationShare.Share);
                
                return model.Shares.SetItem(donation.OptionId, newOption);
            }
        }
    }
}
