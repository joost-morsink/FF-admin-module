namespace FfAdmin.Calculator;

public record struct MoneyBag(ImmutableDictionary<string, Real> Amounts)
{
    public static MoneyBag Empty { get; } = new MoneyBag(ImmutableDictionary<string, Real>.Empty);

    public MoneyBag Add(string currency, Real amount)
        => new (Amounts.SetItem(currency,Amounts.GetValueOrDefault(currency)));

    public MoneyBag Trim(Real amount)
        => new(Amounts.Where(kvp => Math.Abs(kvp.Value) >= amount).ToImmutableDictionary());
}
