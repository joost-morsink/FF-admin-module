using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public interface IModel<T>
    where T : class
{
    static abstract T Empty { get; }
    static abstract IEventProcessor<T> GetProcessor(IServiceProvider serviceProvider);
}

public interface IPartitionedModel<THeader, TKey, TDetail> : IModel<THeader>
    where THeader : class, IPartitioningParameter
    where TKey : notnull
    where TDetail : class
{
    static abstract TDetail EmptyDetail { get; }
    static abstract Bucket? GetBucket(THeader header, TKey key);
    static abstract TKey? GetKeyForEvent(Event e);
    static abstract IEventProcessor<TDetail> GetDetailProcessor(IServiceProvider serviceProvider);
    
}


public interface IPartitioningParameter
{
    int MaskBits { get; }
}

public readonly record struct Bucket
{
    public int MaskBits { get; }
    private readonly int _index;

    public Bucket(int index, int maskBits)
    {
        MaskBits = maskBits;
        _index = index;
    }
    public int Index => _index & Mask;

    public bool Equals(Bucket other)
        => MaskBits == other.MaskBits && Index == other.Index; 
    
    public override int GetHashCode()
        => HashCode.Combine(Index & Mask, MaskBits);
    
    public int Mask => (1 << MaskBits) - 1;
    
    public string Name
        => (Index & Mask).ToString("X").PadLeft((MaskBits - 1) / 4 + 1, '0');
}
