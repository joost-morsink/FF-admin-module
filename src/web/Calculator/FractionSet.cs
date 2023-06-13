namespace FfAdmin.Calculator;

public class FractionSet
{
    public ImmutableDictionary<string, Real> Fractions { get; }
    public Real Divisor { get; }

    private FractionSet(ImmutableDictionary<string, Real> fractions, Real divisor)
    {
        Fractions = fractions;
        Divisor = divisor;
    }
    public static FractionSet Empty { get; } = new(ImmutableDictionary<string, Real>.Empty, (Real)0);
    
    public FractionSet Add(string key, Real fraction)
    {
        var negate = Fractions.TryGetValue(key, out var old) ? old : 0;
        var newFractions = Fractions.SetItem(key, fraction);
        var newDivisor = Divisor + fraction - negate;
        return new (newFractions, newDivisor);
    }

    public FractionSet AddRange(IEnumerable<(string, Real)> entries)
        => entries.Aggregate(this, (acc, e) => acc.Add(e.Item1, e.Item2));

    public FractionSet Remove(string key)
    {
        var negate = Fractions.TryGetValue(key, out var old) ? old : 0;
        var newFractions = Fractions.Remove(key);
        var newDivisor = Divisor - negate;
        return new (newFractions, newDivisor);
    }

    public FractionSet RemoveRange(IEnumerable<string> keys)
        => keys.Aggregate(this, (acc, k) => acc.Remove(k));

    public FractionSet Normalize()
    {
        if(Divisor == 0 || Math.Abs(Divisor-1) < (Real)0.000000001)
            return this;
        var newFractions = Fractions.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value / Divisor);
        var newDivisor = Fractions.Values.Sum();
        return new FractionSet(newFractions, newDivisor);
    }

    public Real this[string key] => Fractions.TryGetValue(key, out var frac) ? frac / Divisor : 0;
}
