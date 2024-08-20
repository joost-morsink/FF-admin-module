using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record AmountsToTransfer(ImmutableDictionary<string, MoneyBag> Values) : IModel<AmountsToTransfer>
{
    public static implicit operator AmountsToTransfer(ImmutableDictionary<string, MoneyBag> values)
        => new(values);
    public static AmountsToTransfer Empty { get; } = new(ImmutableDictionary<string, MoneyBag>.Empty);

    public static IEventProcessor<AmountsToTransfer> GetProcessor(IServiceProvider services)
        => ActivatorUtilities.CreateInstance<Impl>(services);

    private class Impl(IContext<Options> cOptions, IContext<Charities> cCharities, IContext<CurrentCharityFractionSets> cCurrentCharityFractionSets) : EventProcessor<AmountsToTransfer>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext context)
        {
            return new Calc(previousContext, context, cOptions, cCharities, cCurrentCharityFractionSets);
        }


        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Options> cOptions, IContext<Charities> cCharities, IContext<CurrentCharityFractionSets> cCurrentCharityFractionSets) : BaseCalculation(previousContext, currentContext)
        {
            public Options CurrentOptions => GetCurrent(cOptions);
            public Charities CurrentCharities => GetCurrent(cCharities);
            public CurrentCharityFractionSets CurrentCharityFractionSets => GetCurrent(cCurrentCharityFractionSets);
            protected override AmountsToTransfer NewCharity(AmountsToTransfer model, NewCharity e)
            {
                var newValues = model.Values.SetItem(e.Code, MoneyBag.Empty);
                return new(newValues);
            }

            protected override AmountsToTransfer ConvTransfer(AmountsToTransfer model, ConvTransfer e)
            {
                var newValues = model.Values.SetItem(e.Charity, model.Values[e.Charity].Add(e.Currency, -(Real)e.Amount));
                return new(newValues);
            }

            protected override AmountsToTransfer ConvExit(AmountsToTransfer model,  ConvExit e)
            {
                var option = CurrentOptions.Values[e.Option];
                var charities = CurrentCharities;
                var charityFractionSet = CurrentCharityFractionSets.Sets[e.Option]!;

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
}
