
namespace FfAdmin.Calculator.Core;
public partial class EventStream
{
    private class CachedContext : IContext
    {
        private readonly IContext _inner;
        private readonly Lazy<ValueTask<IContext>> _previous;
        private readonly EventStream _parent;
        private readonly int _index;
        private TypedDictionary _values = TypedDictionary.Empty;
        private TypedBucketDictionary _bucketValues = TypedBucketDictionary.Empty;

        public CachedContext(IContext inner, Func<ValueTask<IContext>> previous, EventStream parent, Event @event, int index)
        {
            _inner = inner;
            _previous = new Lazy<ValueTask<IContext>>(previous);
            _parent = parent;
            Event = @event;
            _index = index;
        }

        public async ValueTask<object?> GetContext(Type type)
        {
            if(await _parent._modelCache.Get(_index, _parent._metaModels.Get(type)!) is { } model)
            {
                _values = _values.Set(type, model);
                return model;
            }
            return await _inner.GetContext(type);
        }

        public async ValueTask<object?> GetContext(object header, object key)
        {
            var type = header.GetType();
            var metaModel = _parent._metaModels.Get(type);
            if (metaModel is null)
                throw new ArgumentException($"Cannot find metamodel for model type {type}.");
            var bucket = metaModel.GetBucket(header, key);
            if (bucket is null)
                throw new ArgumentException($"Cannot find bucket for model type {type} with key {key}.");
            if(await _parent._modelCache.Get(_index, metaModel, bucket.Value) is { } model)
            {
                _bucketValues = _bucketValues.Set(header.GetType(), bucket.Value, model);
                return model;
            }
            
            return await _inner.GetContext(header, key);
        }

        public IEnumerable<Type> AvailableContexts => _parent._metaModels.AvailableTypes;
        public ValueTask<IContext> Previous => _previous.Value;
        public Event Event { get; }
    }
}
