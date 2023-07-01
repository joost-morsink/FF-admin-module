namespace FfAdmin.Calculator.Core;

public readonly struct TypedDictionary : IEnumerable<KeyValuePair<Type, object?>>
{
    public static TypedDictionary Empty { get; } = new(ImmutableDictionary<Type, object?>.Empty);

    private TypedDictionary(ImmutableDictionary<Type, object?> values)
    {
        Values = values;
    }

    public ImmutableDictionary<Type, object?> Values { get; }

    public TypedDictionary Set<T>(T? value)
        where T : class
        => new(Values.SetItem(typeof(T), value));

    public TypedDictionary Set(Type type, object? value)
        => new(Values.SetItem(type, value));

    public T? Get<T>()
        where T : class
        => Values.TryGetValue(typeof(T), out var value) ? (T?)value : default;

    public object? Get(Type type)
        => Values.TryGetValue(type, out var value) ? value : default;

    public IEnumerator<KeyValuePair<Type, object?>> GetEnumerator()
        => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    
    public (TypedDictionary,T?) GetOrAdd<T>(Func<T?> creator)
        where T : class
    {
        if (Values.TryGetValue(typeof(T), out var value))
            return (this,(T?)value);
        var res = creator();
        
        return (Set(res),res);
    }

    public (TypedDictionary,object?) GetOrAdd(Type type, Func<object?> creator)
    {
        if (Values.TryGetValue(type, out var value))
            return (this,value);
        var res = creator();

        return (Set(type, res), res);
    }

    public bool Contains<T>()
        where T : class
        => Contains(typeof(T));

    public bool Contains(Type type)
        => Values.ContainsKey(type);

}
