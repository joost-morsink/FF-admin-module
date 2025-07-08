namespace FfAdmin.Calculator;

public record Options(ImmutableDictionary<string, Option> Values) : IModel<Options>
{
    public static IMetaModel<Options> GetMetaModel()
        => Meta.Instance;

    private class Meta : IModel<Options>.BaseSimpleMetaModel
    {
        public static Meta Instance { get; } = new();
        public override Options Empty => new(ImmutableDictionary<string, Option>.Empty);

        public override IEventProcessor<Options> GetProcessor(IServiceProvider serviceProvider)
            => new Impl();
    }
    public static implicit operator Options(ImmutableDictionary<string, Option> dict)
        => new(dict);
    
    public bool Contains(string id)
        => Values.ContainsKey(id);

    private class Impl : EventProcessor<Options>
    {
        protected override BaseCalculation GetCalculation(IContext previousContext, IContext context)
            => new Calc(previousContext, context);

        private sealed class Calc(IContext previousContext, IContext context) : BaseCalculation(previousContext, context)
        {
            protected override async ValueTask<Options> NewOption(Options model, NewOption e)
                => new(model.Values.Add(e.Code,
                    new Option(e.Code, e.Name, e.Currency, (Real)e.Charity_fraction, (Real)e.Reinvestment_fraction,
                        (Real)e.FutureFund_fraction, (Real)e.Bad_year_fraction)));

            protected override async ValueTask<Options> UpdateFractions(Options model, UpdateFractions e)
                => new (Values: model.Values.SetItem(e.Code,
                    model.Values[e.Code] with
                    {
                        CharityFraction = (Real)e.Charity_fraction,
                        ReinvestmentFraction = (Real)e.Reinvestment_fraction,
                        G4gFraction = (Real)e.FutureFund_fraction,
                        BadYearFraction = (Real)e.Bad_year_fraction
                    }));
        }
    }

}
public record Option(string Id, string Name, string Currency, Real CharityFraction, Real ReinvestmentFraction, Real G4gFraction,
    Real BadYearFraction);

