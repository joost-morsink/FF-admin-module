namespace FfAdmin.Calculator;

public record AmountsToTransfer(ImmutableDictionary<string, Real> Values) : IModel<AmountsToTransfer>
{
    public static AmountsToTransfer Empty { get; } = new(ImmutableDictionary<string, decimal>.Empty);
    public static IEventProcessor<AmountsToTransfer> Processor { get; } = new Impl();

    private class Impl : EventProcessor<AmountsToTransfer>
    {
        public override AmountsToTransfer Start => Empty;

        protected override AmountsToTransfer ConvTransfer(AmountsToTransfer model, IContext context, ConvTransfer e)
        {
            var newValues = model.Values.SetItem(e.Charity, model.Values[e.Charity] - (Real)e.Amount);
            return new(newValues);
        }

        protected override AmountsToTransfer ConvExit(AmountsToTransfer model, IContext context, ConvExit e)
        {
            var charityFractionSet = context.GetContext<CurrentCharityFractionSets>().Sets[e.Option]!;

            var newValues = charityFractionSet.CharityFractions.Aggregate(model.Values, (acc, frac) =>
                acc.Mutate(frac.Key, v => v + frac.Value * e.Amount));
            return new(newValues);
        }
    }
}


