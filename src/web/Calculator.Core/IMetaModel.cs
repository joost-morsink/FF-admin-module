using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

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
    bool IsMegaEvent(Event e) 
        => false;
    object CleanDetail(object detail, Bucket bucket);
    
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
