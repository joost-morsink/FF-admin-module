using System.Collections.Immutable;
using System.Linq;

namespace InMemoryDatabase.Test;

public static class TestExtensions
{
    public static ImmutableDictionary<int, T> Add<T>(this ImmutableDictionary<int, T> dictionary, EventStream stream,
        int position)
        where T : class
        => dictionary.Add(position, stream.GetAtPosition(position).GetContext<T>()!);

    public static ImmutableDictionary<int, T> GetValues<T>(this EventStream stream, params int[] positions)
        where T : class
        => positions.ToImmutableDictionary(p => p, p => stream.GetAtPosition(p).GetContext<T>()!);

    public static ImmutableList<T> ToListOrderedByKey<T>(this ImmutableDictionary<int, T> dictionary)
        => dictionary.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToImmutableList();
}
