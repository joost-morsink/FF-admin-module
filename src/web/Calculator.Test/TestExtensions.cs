using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FfAdmin.Calculator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator.Test;

public static class TestExtensions
{
    public static async ValueTask<T> GetContext<T>(this IContext context)
        where T : class
        => (T)(await context.GetContext(typeof(T)))!;

    public static async Task<ImmutableDictionary<int, T>> GetValues<T>(this EventStream stream, params int[] positions)
        where T : class
    {
        var contexts = await Task.WhenAll(positions.Select(p => stream.GetAtPosition(p)));
        var contextVals = await WhenAll(contexts.Select(c => c.GetContext<T>()));
        return positions.Zip(contextVals, (p, c) => (p, c)).ToImmutableDictionary(t => t.p, t => t.c);
    }
    public static async ValueTask<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks)
    {
        var results = new List<T>();
        foreach (var task in tasks)
            results.Add(await task);
        
        return results.ToArray();
    }
    public static ImmutableList<T> ToListOrderedByKey<T>(this ImmutableDictionary<int, T> dictionary)
        => dictionary.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToImmutableList();

    public static IServiceCollection AddContext<T>(this IServiceCollection services)
        where T:class, IModel
        => services.AddSingleton(IContext<T>.Instance).AddSingleton(T.GetMetaModel());
}
