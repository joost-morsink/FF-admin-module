using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;


public record CurrentCharityFractionSets(ImmutableDictionary<string, CharityFractionSetsForOption> Sets) : IModel<CurrentCharityFractionSets>
{
    public static CurrentCharityFractionSets Empty { get; } = new(ImmutableDictionary<string, CharityFractionSetsForOption>.Empty);
    public static IEventProcessor<CurrentCharityFractionSets> Processor { get; } = new Impl();

    public class Impl : ContextualCalculator<CurrentCharityFractionSets>
    {
        public override CurrentCharityFractionSets Start => Empty;

        protected override CurrentCharityFractionSets Default(IContext context, Event e)
        {
            var fractionSets = context.GetContext<Options>().Values
                .Select(o =>
                    (o.Key, CharityFractionSetsForOption.Create(context, o.Key)))
                .ToImmutableDictionary();
            return new(fractionSets);
        }
    }
}
public record CharityFractionSetsForOption(FractionSet CharityFractions,
    ImmutableDictionary<string, FractionSet> DonationFractions)
{
    public static CharityFractionSetsForOption Create(IContext context, string optionId)
    {
        {
            var optionDonationFractions = context.GetContext<OptionWorths>().Worths[optionId].DonationFractions;
            var donations = context.GetContext<Donations>().Values;
            var charityFractions = optionDonationFractions.Aggregate(d => donations[d].CharityId);
            var charityDonationFractions = optionDonationFractions.Group(d => donations[d].CharityId);
            return new CharityFractionSetsForOption(charityFractions, charityDonationFractions);
        }
    }
}
