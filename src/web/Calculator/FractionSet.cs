using System.Text.Json.Serialization;

namespace FfAdmin.Calculator;

[JsonConverter(typeof(FractionSetJsonConverter))]
public class FractionSet : IReadOnlyDictionary<string, Real>
{
    internal ImmutableDictionary<string, Real> Fractions { get; }
    internal Real Divisor { get; }

    private FractionSet(ImmutableDictionary<string, Real> fractions, Real divisor)
    {
        Fractions = fractions;
        Divisor = divisor;
    }

    public static FractionSet Empty { get; } = new(ImmutableDictionary<string, Real>.Empty, (Real)0);

    public FractionSet Add(string key, Real fraction)
    {
        var negate = Fractions.TryGetValue(key, out var old) ? old : 0;
        var added = Divisor == 0 ? 1 : fraction * Divisor;
        var newFractions = Fractions.Add(key, added);
        var newDivisor = Divisor + added;
        return new(newFractions, newDivisor);
    }

    public FractionSet AddRange(IEnumerable<(string, Real)> entries)
    {
        var e = entries.ToArray();
        var divisor = Divisor;
        if (Divisor == 0)
        {
            var newFractions = e.Aggregate(Fractions,
                (acc, f) => acc.Add(f.Item1, f.Item2));
            var newDivisor = e.Sum(f => f.Item2);
            return new(newFractions, newDivisor);
        }
        else
        {
            var newFractions = e.Aggregate(Fractions,
                (acc, f) => acc.Add(f.Item1, Divisor * f.Item2));
            var newDivisor = Divisor * (1 + e.Sum(f => f.Item2));
            return new(newFractions, newDivisor);
        }
    }


    public FractionSet Remove(string key)
    {
        var negate = Fractions.TryGetValue(key, out var old) ? old : 0;
        var newFractions = Fractions.Remove(key);
        var newDivisor = Divisor - negate;
        return new(newFractions, newDivisor);
    }

    public FractionSet RemoveRange(IEnumerable<string> keys)
        => keys.Aggregate(this, (acc, k) => acc.Remove(k));

    public bool ContainsKey(string key)
        => Fractions.ContainsKey(key);

    public bool TryGetValue(string key, out decimal value)
    {
        if (Fractions.TryGetValue(key, out var val))
        {
            value = val / Divisor;
            return true;
        }

        value = 0m;
        return false;
    }

    public Real this[string key] => Fractions.TryGetValue(key, out var frac) ? frac / Divisor : 0;
    public IEnumerable<string> Keys => Fractions.Keys;
    public IEnumerable<decimal> Values => Fractions.Values.Select(x => x / Divisor);

    public IEnumerator<KeyValuePair<string, decimal>> GetEnumerator()
    {
        foreach (var (key, value) in Fractions)
            yield return new KeyValuePair<string, decimal>(key, value / Divisor);
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public int Count => Fractions.Count;

    public FractionSet Aggregate(Func<string, string> group)
    {
        var q = Fractions.GroupBy(f => group(f.Key), f => f.Value)
            .ToImmutableDictionary(g => g.Key, g => g.Sum());
        return new(q, Divisor);
    }

    public ImmutableDictionary<string, FractionSet> Group(Func<string, string> group)
    {
        var q = Fractions.GroupBy(f => group(f.Key))
            .Select(g => (g.Key, Dictionary: g
                .ToImmutableDictionary(
                    f => f.Key,
                    f => f.Value)))
            .ToImmutableDictionary(
                d => d.Key,
                d => new FractionSet(d.Dictionary, d.Dictionary.Values.Sum()));
        return q;
    }
    public FractionSet Filter(Func<string, bool> filter)
    {
        var q = Fractions.Where(f => filter(f.Key)).ToImmutableDictionary();
        return new(q, q.Values.Sum());
    }

    public static implicit operator FractionSet(ImmutableDictionary<string, Real> dict)
    {
        var sum = dict.Values.Sum();
        return new(dict, sum);
    }
}
