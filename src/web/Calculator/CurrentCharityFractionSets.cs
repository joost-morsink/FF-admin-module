using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;


public record CurrentCharityFractionSets(ImmutableDictionary<string, CharityFractionSetsForOption> Sets) : IModel<CurrentCharityFractionSets>
{
    public static IMetaModel<CurrentCharityFractionSets> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();

    private class Meta : IModel<CurrentCharityFractionSets>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override CurrentCharityFractionSets Empty { get; } = new(ImmutableDictionary<string, CharityFractionSetsForOption>.Empty);

        public override IEventProcessor<CurrentCharityFractionSets> GetProcessor(IServiceProvider services)
            => ActivatorUtilities.CreateInstance<Impl>(services);
    }

    public class Impl(IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths, IContext<Donations> cDonations) : ContextualCalculator<CurrentCharityFractionSets>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptions, cOptionWorths, cDonations);
        }

        protected class Calc(IContext previousContext, IContext currentContext, IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths, IContext<Donations> cDonations)
            : BaseCalculation(previousContext, currentContext)
        {
            public ValueTask<Options> CurrentOptions => GetCurrent(cOptions);
            public ValueTask<OptionWorths> CurrentOptionWorths => GetCurrent(cOptionWorths);
            public ValueTask<Donations> CurrentDonations => GetCurrent(cDonations);
            protected override async ValueTask<CurrentCharityFractionSets> Default(Event e)
            {
                var currentDonations = await CurrentDonations;
                var currentOptionWorths = await CurrentOptionWorths;
                var fractionSets = (await CurrentOptions).Values
                    .Select(o =>
                        (o.Key, CharityFractionSetsForOption.Create(currentDonations, currentOptionWorths.Worths[o.Key])))
                    .ToImmutableDictionary();
                return new(fractionSets);
            }
        }
    }
}
public record CharityFractionSetsForOption(FractionSet CharityFractions,
    ImmutableDictionary<string, FractionSet> DonationFractions)
{
    public static CharityFractionSetsForOption Create(Donations currentDonations, OptionWorth optionWorth)
    {
        {
            var optionDonationFractions = optionWorth.DonationFractions;
            var donations = currentDonations.Values;
            var charityFractions = optionDonationFractions.Aggregate(d => donations[d].CharityId);
            var charityDonationFractions = optionDonationFractions.Group(d => donations[d].CharityId);
            return new CharityFractionSetsForOption(charityFractions, charityDonationFractions);
        }
    }
}
