namespace FfAdmin.InMemoryDatabase;

public readonly struct TypedDictionary : IEnumerable<KeyValuePair<Type, Lazy<object?>>>, IContext
{
    public static TypedDictionary Empty { get; } = new(ImmutableDictionary<Type, Lazy<object?>>.Empty);

    private TypedDictionary(ImmutableDictionary<Type, Lazy<object?>> values)
    {
        Values = values;
    }

    public ImmutableDictionary<Type, Lazy<object?>> Values { get; }

    public TypedDictionary Set<T>(Func<T?> valueFactory)
        where T : class
        => new(Values.SetItem(typeof(T), new Lazy<object?>(valueFactory)));
    public TypedDictionary Set(Type type, Func<object?> valueFactory)
        => new(Values.SetItem(type, new Lazy<object?>(valueFactory)));

    public T? Get<T>()
        where T : class
        => Values.TryGetValue(typeof(T), out var value) ? (T?)value.Value : default;

    public object? Get(Type type)
        => Values.TryGetValue(type, out var value) ? value.Value : default;
    
    public IEnumerator<KeyValuePair<Type, Lazy<object?>>> GetEnumerator()
        => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    T? IContext.GetContextOrNull<T>()
        where T : class
        => Get<T>();

    T IContext.GetContext<T>()
        where T : class
        => Get<T>() ?? throw new InvalidOperationException($"Context of type {typeof(T)} not found");

    object? IContext.GetContext(Type type)
        => Values.TryGetValue(type, out var value) ? value.Value : null;

    IEnumerable<Type> IContext.AvailableContexts => Values.Keys;
}


