namespace FfAdmin.Calculator;

public record AmountsToTransfer(ImmutableDictionary<string, Real> Values) : IModel<AmountsToTransfer>
{
    public static AmountsToTransfer Empty { get; } = new(ImmutableDictionary<string, decimal>.Empty);
    public static IEventProcessor<AmountsToTransfer> Processor { get; } = new Impl();

    private class Impl : EventProcessor<AmountsToTransfer>
    {
        public override AmountsToTransfer Start => Empty;

        protected override AmountsToTransfer NewCharity(AmountsToTransfer model, IContext context, NewCharity e)
        {
            var newValues = model.Values.SetItem(e.Code, (Real)0);
            return new(newValues);
        }

        protected override AmountsToTransfer ConvTransfer(AmountsToTransfer model, IContext context, ConvTransfer e)
        {
            var newValues = model.Values.SetItem(e.Charity, model.Values[e.Charity] - (Real)e.Amount);
            return new(newValues);
        }

        protected override AmountsToTransfer ConvExit(AmountsToTransfer model, IContext context, ConvExit e)
        {
            var option = context.GetContext<Options>().Values[e.Option];
            var charities = context.GetContext<Charities>();
            var charityFractionSet = context.GetContext<CurrentCharityFractionSets>().Sets[e.Option]!;

            var newValues = AddAmountToCharity(charityFractionSet.CharityFractions.Aggregate(model.Values,
                    (acc, frac) =>
                        AddAmountToCharity(acc, charities, charities.Values[frac.Key],
                            frac.Value * option.CharityFraction * e.Amount /
                            (option.G4gFraction + option.CharityFraction)))
                , charities, charities.Values["FF"],
                e.Amount * option.G4gFraction / (option.G4gFraction + option.CharityFraction));

            return new(newValues);
        }

        private ImmutableDictionary<string, decimal> AddAmountToCharity(ImmutableDictionary<string, decimal> values,
            Charities charities, Charity charity, decimal amount)
        {
            if (charity.Fractions is not null)
                return charity.Fractions.Aggregate(values,
                    (acc, fr) => 
                        AddAmountToCharity(acc, charities, charities.Values[fr.Key], amount * fr.Value));

            return values.Mutate(charity.Id, a => a + amount);
        }
    }
}
