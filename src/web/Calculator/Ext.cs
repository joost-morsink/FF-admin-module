namespace FfAdmin.Calculator;

public static class Ext
{
    public static ImmutableDictionary<string, Real> Mutate(this ImmutableDictionary<string, Real> dict, string key,
        Func<Real, Real> mutator)
        => dict.TryGetValue(key, out var old) ? dict.SetItem(key, mutator(old)) : dict.Add(key, mutator(0));

    public static ImmutableDictionary<string, ImmutableList<T>> Add<T>(
        this ImmutableDictionary<string, ImmutableList<T>> dict, string key, T value)
        => dict.TryGetValue(key, out var old)
            ? dict.SetItem(key, old.Add(value))
            : dict.Add(key, ImmutableList.Create(value));

    public static ImmutableDictionary<K, V> ToImmutableDictionary<K, V>(this IEnumerable<(K, V)> items)
        where K : notnull
        => items.ToImmutableDictionary(x => x.Item1, x => x.Item2);
}
