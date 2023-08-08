using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FfAdmin.Common;

public class SortedImmutableDictionary<K, V> : IImmutableDictionary<K, V>
    where K : notnull
{
    private readonly ImmutableList<K> _keys;
    private readonly ImmutableSortedDictionary<K, V> _values;
    private readonly IComparer<K> _comparer;

    public static SortedImmutableDictionary<K, V> Empty { get; } = new(ImmutableList<K>.Empty,
        ImmutableSortedDictionary<K, V>.Empty, Comparer<K>.Default);

    public static SortedImmutableDictionary<K, V> Create(IComparer<K> comparer)
        => new(ImmutableList<K>.Empty, ImmutableSortedDictionary<K, V>.Empty, comparer);

    private SortedImmutableDictionary(ImmutableList<K> keys, ImmutableSortedDictionary<K, V> values,
        IComparer<K> comparer)
    {
        _keys = keys;
        _values = values;
        _comparer = comparer;
    }

    public SortedImmutableDictionary<K, V> Add(K key, V value)
    {
        var index = _keys.BinarySearch(key, _comparer);
        if (index >= 0)
            throw new ArgumentException("Key already exists", nameof(key));
        index = ~index;
        return new SortedImmutableDictionary<K, V>(_keys.Insert(index, key), _values.Add(key, value), _comparer);
    }

    public SortedImmutableDictionary<K, V> AddRange(IEnumerable<KeyValuePair<K, V>> pairs)
        => pairs.Aggregate(this, (current, pair) => current.Add(pair.Key, pair.Value));

    public IImmutableDictionary<K, V> Clear()
        => Empty;

    public bool Contains(KeyValuePair<K, V> pair)
    {
        return _values.Contains(pair);
    }

    public SortedImmutableDictionary<K, V> Remove(K key)
    {
        var index = _keys.BinarySearch(key);
        return index >= 0 ? new(_keys.RemoveAt(index), _values.Remove(key), _comparer) : this;
    }

    public SortedImmutableDictionary<K, V> RemoveRange(IEnumerable<K> keys)
        => keys.Aggregate(this, (current, key) => current.Remove(key));

    public SortedImmutableDictionary<K, V> SetItem(K key, V value)
    {
        var index = _keys.BinarySearch(key);
        if (index < 0)
            return Add(key, value);
        else
            return new SortedImmutableDictionary<K, V>(_keys, _values.SetItem(key, value), _comparer);
    }

    public SortedImmutableDictionary<K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items)
        => items.Aggregate(this, (current, kvp) => current.SetItem(kvp.Key, kvp.Value));

    public bool TryGetKey(K equalKey, out K actualKey)
    {
        return _values.TryGetKey(equalKey, out actualKey);
    }

    public K? FirstKeyLowerThanOrEqual(K key)
    {
        var index = _keys.BinarySearch(key, _comparer);
        if (index >= 0)
            return _keys[index];
        index = ~index;
        if (index == 0)
            return default;
        return _keys[index - 1];
    }

    public K? FirstKeyGreaterThanOrEqual(K key)
    {
        var index = _keys.BinarySearch(key, _comparer);
        if (index >= 0)
            return _keys[index];
        index = ~index;
        if (index == Count)
            return default;
        return _keys[index];
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public int Count => _values.Count;

    public bool ContainsKey(K key)
        => _values.ContainsKey(key);


    public bool TryGetValue(K key, [MaybeNullWhen(returnValue: false)] out V value)
    {
        return _values.TryGetValue(key, out value);
    }

    public V this[K key] => _values[key];

    public IEnumerable<K> Keys => _values.Keys;

    public IEnumerable<V> Values => _values.Values;

    IImmutableDictionary<K, V> IImmutableDictionary<K, V>.Add(K key, V value)
        => Add(key, value);

    IImmutableDictionary<K, V> IImmutableDictionary<K, V>.AddRange(IEnumerable<KeyValuePair<K, V>> pairs)
        => AddRange(pairs);

    IImmutableDictionary<K, V> IImmutableDictionary<K, V>.Remove(K key)
        => _values.Remove(key);

    IImmutableDictionary<K, V> IImmutableDictionary<K, V>.RemoveRange(IEnumerable<K> keys)
        => RemoveRange((keys));

    IImmutableDictionary<K, V> IImmutableDictionary<K, V>.SetItem(K key, V value)
        => SetItem(key, value);

    IImmutableDictionary<K, V> IImmutableDictionary<K, V>.SetItems(IEnumerable<KeyValuePair<K, V>> pairs)
        => SetItems(pairs);
}
