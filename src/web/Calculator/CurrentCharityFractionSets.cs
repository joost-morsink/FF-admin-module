using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;


public record CurrentCharityFractionSets(ImmutableDictionary<string, CharityFractionSetsForOption> Sets) : IModel<CurrentCharityFractionSets>
{
    public static CurrentCharityFractionSets Empty { get; } = new(ImmutableDictionary<string, CharityFractionSetsForOption>.Empty);

    public static IEventProcessor<CurrentCharityFractionSets> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    public class Impl(IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths, IContext<Donations> cDonations) : ContextualCalculator<CurrentCharityFractionSets>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext currentContext)
        {
            return new Calc(previousContext, currentContext, cOptions, cOptionWorths, cDonations);
        }

        protected class Calc(IContext previousContext, IContext currentContext, IContext<Options> cOptions, IContext<OptionWorths> cOptionWorths, IContext<Donations> cDonations)
            : BaseCalculation(previousContext, currentContext)
        {
            public Options CurrentOptions => GetCurrent(cOptions);
            public OptionWorths CurrentOptionWorths => GetCurrent(cOptionWorths);
            public Donations CurrentDonations => GetCurrent(cDonations);
            protected override CurrentCharityFractionSets Default(Event e)
            {
                var fractionSets = CurrentOptions.Values
                    .Select(o =>
                        (o.Key, CharityFractionSetsForOption.Create(CurrentDonations, CurrentOptionWorths.Worths[o.Key])))
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
