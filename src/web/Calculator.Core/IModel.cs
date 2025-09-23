using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

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
