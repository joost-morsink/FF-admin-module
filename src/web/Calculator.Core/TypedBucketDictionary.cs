namespace FfAdmin.Calculator.Core;

public readonly struct TypedBucketDictionary : IEnumerable<KeyValuePair<(Type, Bucket), object?>>
{
    public static TypedBucketDictionary Empty { get; } = new(ImmutableDictionary<(Type, Bucket), object?>.Empty);

    private TypedBucketDictionary(ImmutableDictionary<(Type, Bucket), object?> values)
    {
        Values = values;
    }

    public ImmutableDictionary<(Type, Bucket), object?> Values { get; }

    public TypedBucketDictionary Set<T>(Bucket bucket, T? value)
        where T : class
        => new(Values.SetItem((typeof(T), bucket), value));

    public TypedBucketDictionary Set(Type type, Bucket bucket, object? value)
        => new(Values.SetItem((type, bucket), value));

    public T? Get<T>(Bucket bucket)
        where T : class
        => Values.GetValueOrDefault((typeof(T), bucket)) as T;

    public object? Get(Type type, Bucket bucket)
        => Values.GetValueOrDefault((type,bucket));

    public IEnumerator<KeyValuePair<(Type,Bucket), object?>> GetEnumerator()
        => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public (TypedBucketDictionary, T?) GetOrAdd<T>(Bucket bucket, Func<T?> creator)
        where T : class
    {
        if (Values.TryGetValue((typeof(T), bucket), out var value))
            return (this, (T?)value);
        
        var res = creator();
        return (Set(bucket, res), res);
    }

    public (TypedBucketDictionary, object?) GetOrAdd(Type type, Bucket bucket, Func<object?> creator)
    {
        if(Values.TryGetValue((type, bucket), out var value))
            return (this, value);
        
        var res = creator();
        return (Set(type, bucket, res), res);
    }

    public async ValueTask<(TypedBucketDictionary, object?)> GetOrAddAsync(Type type, Bucket bucket, Func<ValueTask<object?>> creator)
    {
        if (Values.TryGetValue((type,bucket), out var value))
            return (this, value);
        
        var res = await creator();
        return (Set(type, bucket, res), res);
    }

    public bool Contains<T>(Bucket bucket)
        where T : class
        => Contains(typeof(T), bucket);

    public bool Contains(Type type, Bucket bucket)
        => Values.ContainsKey((type,bucket));
}
