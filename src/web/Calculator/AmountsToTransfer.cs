namespace FfAdmin.Calculator;

public record AmountsToTransfer(ImmutableDictionary<string, MoneyBag> Values) : IModel<AmountsToTransfer>
{
    public static implicit operator AmountsToTransfer(ImmutableDictionary<string, MoneyBag> values)
        => new(values);
    public static AmountsToTransfer Empty { get; } = new(ImmutableDictionary<string, MoneyBag>.Empty);
    public static IEventProcessor<AmountsToTransfer> Processor { get; } = new Impl();

    private class Impl : EventProcessor<AmountsToTransfer>
    {
        public override AmountsToTransfer Start => Empty;

        protected override AmountsToTransfer NewCharity(AmountsToTransfer model, IContext context, NewCharity e)
        {
            var newValues = model.Values.SetItem(e.Code, MoneyBag.Empty);
            return new(newValues);
        }

        protected override AmountsToTransfer ConvTransfer(AmountsToTransfer model, IContext context, ConvTransfer e)
        {
            var newValues = model.Values.SetItem(e.Charity, model.Values[e.Charity].Add(e.Currency, -(Real)e.Amount));
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
                            option.Currency,
                            frac.Value * option.CharityFraction * e.Amount /
                            (option.G4gFraction + option.CharityFraction)))
                , charities, charities.Values["FF"], option.Currency,
                e.Amount * option.G4gFraction / (option.G4gFraction + option.CharityFraction));

            return new(newValues);
        }

        private ImmutableDictionary<string, MoneyBag> AddAmountToCharity(ImmutableDictionary<string, MoneyBag> values,
            Charities charities, Charity charity, string currency, Real amount)
        {
            if (charity.Fractions is not null)
                return charity.Fractions.Aggregate(values,
                    (acc, fr) => 
                        AddAmountToCharity(acc, charities, charities.Values[fr.Key], currency, amount * fr.Value));

            return values.SetItem(charity.Id, values[charity.Id].Add(currency, amount));
        }
    }
}
