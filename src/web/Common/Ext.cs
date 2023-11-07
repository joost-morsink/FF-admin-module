using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FfAdmin.Common;

[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
public static class Ext
{
    public static IEnumerable<T> SelectValues<T>(this IEnumerable<T?> src)
    {
        foreach (var x in src)
            if (x != null)
                yield return x;
    }
    public static XNode RemoveNamespaces(this XNode node)
        => node switch
        {
            XElement e => e.RemoveNamespaces(),
            _ => node
        };

    public static XElement RemoveNamespaces(this XElement node)
        => new (node.Name.LocalName, node.Attributes(), node.Nodes().Select(x => x.RemoveNamespaces()));

    public static SortedImmutableDictionary<K, V> ToSortedImmutableDictionary<T, K, V>(this IEnumerable<T> src,
        Func<T, K> keySelector, Func<T, V> valueSelector)
        where K : notnull
        => src.Aggregate(SortedImmutableDictionary<K, V>.Empty,
            (result, t) => result.Add(keySelector(t), valueSelector(t)));

    public static void Ignore(this Task t)
    {
    }
    public static async Task<(T,U)> Parallel<T,U>(this (Task<T>, Task<U>) tasks)
        => (await tasks.Item1, await tasks.Item2);
    public static async Task<(T,U,V)> Parallel<T,U,V>(this (Task<T>, Task<U>, Task<V>) tasks)
        => (await tasks.Item1, await tasks.Item2, await tasks.Item3);
    public static TaskAwaiter<(T,U)> GetAwaiter<T,U>(this (Task<T>, Task<U>) tasks)
        => tasks.Parallel().GetAwaiter();
    public static TaskAwaiter<(T,U,V)> GetAwaiter<T,U,V>(this (Task<T>, Task<U>, Task<V>) tasks)
        => tasks.Parallel().GetAwaiter();
}
