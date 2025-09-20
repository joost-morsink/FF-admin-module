using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record AmountsToTransfer(ImmutableDictionary<string, MoneyBag> Values) : IModel<AmountsToTransfer>
{
    public static IMetaModel<AmountsToTransfer> GetMetaModel()
        => Meta.Instance;
    static IMetaModel IModel.GetMetaModel()
        => GetMetaModel();
    
    private class Meta : IModel<AmountsToTransfer>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override AmountsToTransfer Empty => new(ImmutableDictionary<string, MoneyBag>.Empty);
        public override IEventProcessor<AmountsToTransfer> GetProcessor(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<Impl>(serviceProvider);
    }
    public static implicit operator AmountsToTransfer(ImmutableDictionary<string, MoneyBag> values)
        => new(values);

    private class Impl(IContext<Options> cOptions, IContext<Charities> cCharities, IContext<CharityFractionSets> cCharityFractionSets) : EventProcessor<AmountsToTransfer>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext context)
        {
            return new Calc(previousContext, context, cOptions, cCharities, cCharityFractionSets);
        }


        private sealed class Calc(IContext previousContext, IContext currentContext, IContext<Options> cOptions, IContext<Charities> cCharities, IContext<CharityFractionSets> cCharityFractionSets) : BaseCalculation(previousContext, currentContext)
        {
            public ValueTask<Options> CurrentOptions => GetCurrent(cOptions);
            public ValueTask<Charities> CurrentCharities => GetCurrent(cCharities);
            public ValueTask<CharityFractionSets> CurrentCharityFractionSets => GetCurrent(cCharityFractionSets);
            protected override async ValueTask<AmountsToTransfer> NewCharity(AmountsToTransfer model, NewCharity e)
            {
                var newValues = model.Values.SetItem(e.Code, MoneyBag.Empty);
                return new(newValues);
            }

            protected override async ValueTask<AmountsToTransfer> ConvTransfer(AmountsToTransfer model, ConvTransfer e)
            {
                var newValues = model.Values.SetItem(e.Charity, model.Values[e.Charity].Add(e.Currency, -(Real)e.Amount));
                return new(newValues);
            }

            protected override async ValueTask<AmountsToTransfer> ConvExit(AmountsToTransfer model,  ConvExit e)
            {
                var option = (await CurrentOptions).Values[e.Option];
                var charities = await CurrentCharities;
                FractionSet charityFractionSet = (await CurrentCharityFractionSets).Shares[e.Option]!;
                
                var newValues = AddAmountToCharity(charityFractionSet.Aggregate(model.Values,
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
