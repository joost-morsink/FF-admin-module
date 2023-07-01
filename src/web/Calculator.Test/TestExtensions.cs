using System.Collections.Immutable;
using System.Linq;
using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator.Test;

public static class TestExtensions
{

    public static async Task<ImmutableDictionary<int, T>> GetValues<T>(this EventStream stream, params int[] positions)
        where T : class
    {
        var contexts = await Task.WhenAll(positions.Select(p => stream.GetAtPosition(p)));
        return positions.Zip(contexts, (p, c) => (p, c)).ToImmutableDictionary(t => t.p, t => t.c.GetContext<T>()!);
    }

    public static ImmutableList<T> ToListOrderedByKey<T>(this ImmutableDictionary<int, T> dictionary)
        => dictionary.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToImmutableList();
}
