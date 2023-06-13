using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public record Options(ImmutableDictionary<string, Option> Values) : IModel<Options>
{
    public static Options Empty { get; } = new(ImmutableDictionary<string, Option>.Empty);
    public static IEventProcessor<Options> Processor { get; } = new Impl();
    
    public bool Contains(string id)
        => Values.ContainsKey(id);

    private class Impl : EventProcessor<Options>
    {
        public override Options Start { get; } = Options.Empty;

        protected override Options NewOption(Options model, IContext context, NewOption e)
            => new(model.Values.Add(e.Code,
                new Option(e.Code, e.Name, (Real)e.Charity_fraction, (Real)e.Reinvestment_fraction,
                    (Real)e.FutureFund_fraction, (Real)e.Bad_year_fraction)));

        protected override Options UpdateFractions(Options model, IContext context, UpdateFractions e)
            => new Options(Values: model.Values.SetItem(e.Code,
                model.Values[e.Code] with
                {
                    CharityFraction = (Real)e.Charity_fraction,
                    ReinvestmentFraction = (Real)e.Reinvestment_fraction,
                    G4gFraction = (Real)e.FutureFund_fraction,
                    BadYearFraction = (Real)e.Bad_year_fraction
                }));
    }

}
public record Option(string Id, string Name, Real CharityFraction, Real ReinvestmentFraction, Real G4gFraction,
    Real BadYearFraction);

