namespace FfAdmin.InMemoryDatabase;

public record MinimalExits(ImmutableDictionary<string, Real> Exits);

public class MinimalExitsEventProcessor : EventProcessor<MinimalExits>
{
    public override MinimalExits Start { get; } = new(ImmutableDictionary<string, decimal>.Empty);

    protected override MinimalExits ConvLiquidate(MinimalExits model, IContext context, ConvLiquidate e)
    {
        var option = context.GetContext<Options>().Values[e.Option];
        var valuations = context.GetContext<IdealOptionValuations>().Valuations[e.Option];
        var yearsSinceLastExit = (e.Timestamp - valuations.Timestamp).TotalDays / 365.25;
        var percentage = Math.Pow(1 + (double)option.BadYearFraction, yearsSinceLastExit) - 1;
        var minExit = valuations.RealValue * (Real)percentage;
        return new(model.Exits.SetItem(e.Option, minExit));
    }
}
