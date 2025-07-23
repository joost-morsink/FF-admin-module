using System.Collections.Generic;
using System.Collections.Immutable;
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
        where T : class, IModel
    {
        var meta = T.GetMetaModel();
        services.AddSingleton(meta);
        services.Add(ContextInstance(meta.HeaderType));
        if (meta.KeyType is not null)
            services.Add(ContextInstance(meta.HeaderType, meta.KeyType, meta.DetailType));
        return services;
    }

    private static ServiceDescriptor ContextInstance(Type type)
    {
        var svcType = typeof(IContext<>).MakeGenericType(type);
        return new ServiceDescriptor(svcType, svcType.GetProperty(nameof(IContext<object>.Instance))!.GetValue(null)!);
    }

    private static ServiceDescriptor ContextInstance(Type header, Type key, Type detail)
    {

        var svcType = typeof(IContext<,,>).MakeGenericType(header, key, detail);
        return new ServiceDescriptor(svcType, svcType.GetProperty(nameof(IContext<Dummy, Dummy, Dummy>.Instance))!.GetValue(null)!);
    }

    private class Dummy : IModel<Dummy,Dummy, Dummy> {
        public static IMetaModel<Dummy, Dummy, Dummy> GetMetaModel()
        {
            throw new NotImplementedException();
        }

        static IMetaModel IModel.GetMetaModel()
        {
            return GetMetaModel();
        }
    }
}
