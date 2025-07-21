using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

public class Unit
{
    private Unit() { }
    public static Unit Value { get; } = new();
}

public interface IModel
{
    static abstract IMetaModel GetMetaModel();
}
public interface IModel<THeader, TKey, TDetail> : IModel
    where THeader : class, IModel<THeader, TKey, TDetail>
    where TKey : notnull
    where TDetail : class
{
    static new abstract IMetaModel<THeader, TKey, TDetail> GetMetaModel();
}

public class MetaModels
{
    public MetaModels(IEnumerable<IMetaModel> metaModels)
    {
        _metaModelDictionary = metaModels.ToImmutableDictionary(mm => mm.HeaderType);
    }

    private readonly ImmutableDictionary<Type, IMetaModel> _metaModelDictionary;

    public IMetaModel? Get(Type type)
        => _metaModelDictionary.GetValueOrDefault(type);
    public IEnumerable<Type> AvailableTypes
        => _metaModelDictionary.Keys;
}

public interface IMetaModel
{
    Type HeaderType { get; }
    Type? KeyType { get; }
    Type DetailType { get; }
    int MaskBits(object header);
    object Empty { get; }
    object EmptyDetail { get; }
    IEventProcessor GetProcessor(IServiceProvider serviceProvider);
    IEventProcessor GetDetailProcessor(IServiceProvider serviceProvider);
    Bucket? GetBucket(object header, object key);
    object? GetKeyForEvent(Event e);
    object CleanDetail(object detail, Bucket bucket);
}
public interface IMetaModel<THeader, TKey, TDetail> : IMetaModel
    where THeader : class, IModel<THeader, TKey, TDetail>
    where TKey : notnull
    where TDetail : class
{
    Type IMetaModel.HeaderType => typeof(THeader);
    Type? IMetaModel.KeyType => typeof(TKey) == typeof(Unit) ? null : typeof(TKey);
    Type IMetaModel.DetailType => typeof(TDetail);
    int MaskBits(THeader header);
    int IMetaModel.MaskBits(object header)
        => MaskBits((THeader)header);
    new THeader Empty { get; }
    object IMetaModel.Empty => Empty;
    new IEventProcessor<THeader> GetProcessor(IServiceProvider serviceProvider);
    IEventProcessor IMetaModel.GetProcessor(IServiceProvider serviceProvider)
        => GetProcessor(serviceProvider);
    new TDetail EmptyDetail { get; }
    object IMetaModel.EmptyDetail => EmptyDetail;
    Bucket? GetBucket(THeader header, TKey key);
    Bucket? IMetaModel.GetBucket(object header, object key)
        => GetBucket((THeader)header, (TKey)key);

    new TKey? GetKeyForEvent(Event e);
    object? IMetaModel.GetKeyForEvent(Event e)
        => GetKeyForEvent(e);
    
    new IEventProcessor<TDetail> GetDetailProcessor(IServiceProvider serviceProvider);
    IEventProcessor IMetaModel.GetDetailProcessor(IServiceProvider serviceProvider)
        => GetDetailProcessor(serviceProvider);
    
    TDetail CleanDetail(TDetail detail, Bucket bucket);
    object IMetaModel.CleanDetail(object detail, Bucket bucket)
        => CleanDetail((TDetail)detail, bucket);
}

public interface IMetaModel<T> : IMetaModel<T, Unit, T>
    where T : class, IModel<T, Unit, T>
{
    T IMetaModel<T, Unit, T>.EmptyDetail => Empty;
    Bucket? IMetaModel<T, Unit, T>.GetBucket(T header, Unit key)
        => null;
    Unit IMetaModel<T, Unit, T>.GetKeyForEvent(Event e)
        => Unit.Value;
    IEventProcessor<T> IMetaModel<T, Unit, T>.GetDetailProcessor(IServiceProvider serviceProvider)
        => GetProcessor(serviceProvider);
}

public interface IModel<T> : IModel<T, Unit, T>
    where T : class, IModel<T, Unit, T>
{  
    static new abstract IMetaModel<T> GetMetaModel();
    static IMetaModel<T, Unit, T> IModel<T,Unit,T>.GetMetaModel() => T.GetMetaModel();
    public abstract class BaseSimpleMetaModel : IMetaModel<T>
    {
        public int MaskBits(T header) => 0;
        
        public abstract T Empty { get; }
        
        public abstract IEventProcessor<T> GetProcessor(IServiceProvider serviceProvider);

        public T EmptyDetail => Empty;

        public Bucket? GetBucket(T header, Unit key)
            => new Bucket(0, 0);

        public Unit GetKeyForEvent(Event e)
            => Unit.Value;

        public IEventProcessor<T> GetDetailProcessor(IServiceProvider serviceProvider)
            => GetProcessor(serviceProvider);

        public T CleanDetail(T detail, Bucket bucket)
            => detail;
    }
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
