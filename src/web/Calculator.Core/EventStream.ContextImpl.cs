using System.Threading;

namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    private class ContextImpl : ICalculatingContext
    {
        private readonly EventStream _parent;
        private readonly Lazy<ValueTask<IContext>> _previous;
        private readonly Event _event;
        private readonly int _index;
        private TypedDictionary _values;
        private TypedBucketDictionary _bucketValues;
        private MetaModels _metaModels => _parent._metaModels;
        private IServiceProvider _serviceProvider => _parent._serviceProvider;
        public ContextImpl(EventStream parent, Func<ValueTask<IContext>> previous, Event @event, int index)
        {
            _parent = parent;
            _previous = new Lazy<ValueTask<IContext>>(previous);
            _event = @event;
            _index = index;
            _values = TypedDictionary.Empty;
            _bucketValues = TypedBucketDictionary.Empty;
        }
        private async ValueTask<object> Calculate(Type type)
        {
            ICalculatingContext? current = this;
            if(_metaModels.Get(type) is not {} metaModel)
                throw new ArgumentException($"Cannot find processor for model type {type}.");

            var proc = metaModel.GetProcessor(_serviceProvider);
            var todo = new Stack<ICalculatingContext>();
            while (!current.IsEvaluated(type))
            {
                if (current != this)
                    todo.Push(current);
                var curPrevious = await current.Previous;
                if (curPrevious is not ICalculatingContext cc)
                {
                    var prev = await curPrevious.GetContext(type);
                    if (prev is null)
                        throw new MissingDataException(_index, type);
                    break;
                }

                current = cc;
            }

            while (todo.TryPop(out current))
                await current.GetContext(type);
            var previous = await Previous;
            return await proc.Process(
                await previous.GetContext(type) ?? throw new MissingDataException(_index, type),
                previous, this, Event);
        }
        
        private async ValueTask<object> Calculate(object header, object key)
        {
            ICalculatingContext? current = this;
            var type = header.GetType();
            if(_metaModels.Get(type) is not {} metaModel)
                throw new ArgumentException($"Cannot find processor for model type {type}.");

            var proc = metaModel.GetDetailProcessor(_serviceProvider);
            var todo = new Stack<(ICalculatingContext ctx, object header)>();
            var curHeader = header;
            while (!current.IsEvaluated(header, key))
            {
                if (current != this)
                    todo.Push((current, curHeader!));
                var curPrevious = await current.Previous;
                curHeader = await curPrevious.GetContext(type); 
                if (curPrevious is not ICalculatingContext cc)
                {
                    var prev = await curPrevious.GetContext(curHeader!, key);
                    if (prev is null)
                        throw new MissingDataException(_index, type);
                    break;
                }

                current = cc;
            }

            while (todo.TryPop(out var item))
                await item.ctx.GetContext(item.header, key);
            var previous = await Previous;
            var prevHeader = await previous.GetContext(type);
            var prevDetail = await previous.GetContext(prevHeader!, key);
            var bucket = metaModel.GetBucket(header, key);
            var eventKey = metaModel.GetKeyForEvent(Event);
            if (bucket is null || eventKey is null)
                return prevDetail!;
            var eventBucket = metaModel.GetBucket(header, eventKey);
            return metaModel.IsMegaEvent(Event) || bucket == eventBucket
                ? await proc.Process(
                    prevDetail?? throw new MissingDataException(_index, type),
                    previous, this, Event)
                : prevDetail!;

        }

        public async ValueTask<object?> GetContext(Type type)
        {
            (_values, var res) = await _values.GetOrAddAsync(type, async () =>
            {
                var res = await Calculate(type);
                _parent.OnCalculated(_index, _metaModels.Get(type)!,null,  res);
                return res;
            });
            return res;
        }

        public async ValueTask<object?> GetContext(object header, object key)
        {
            var type = header.GetType();
            if(_metaModels.Get(type) is not {} metaModel)
                throw new ArgumentException($"Cannot find processor for model type {type}.");
            var bucket = metaModel.GetBucket(header, key);
            if (bucket is null)
                return GetContext(type);
            (_bucketValues, var res) = await _bucketValues.GetOrAddAsync(type, bucket.Value, async () =>
            {
                var res = await Calculate(header, key);
                _parent.OnCalculated(_index, metaModel, bucket.Value, res);
                return res;
            });
            return res;
        }

        public IEnumerable<Type> AvailableContexts => _metaModels.AvailableTypes;

        public ICalculatingContext AddEvent(Event @event)
            => new ContextImpl(_parent, () => new(this), @event, _index + 1);

        public bool IsEvaluated<T>()
            => _values.Contains(typeof(T));

        public bool IsEvaluated(Type type)
            => _values.Contains(type);

        bool ICalculatingContext.IsEvaluated(object header, object key)
            => IsEvaluated(header, key);
        
        private bool IsEvaluated(object header, object key)
        {
            var type = header.GetType();
            if(_metaModels.Get(type) is not { } metaModel)
                throw new ArgumentException($"Cannot find processor for model type {type}.");
            
            var bucket = metaModel.GetBucket(header, key);
            return bucket is null 
                ? _values.Contains(type)
                : _bucketValues.Contains(type, bucket.Value);
        }

        public ValueTask<IContext> Previous => _previous.Value;
        public Event Event => _event;

        public void SetContext(Type type, object model)
        {
            if (!type.IsInstanceOfType(model))
                throw new InvalidOperationException("Invalid type");
            _values = _values.Set(type, model);
        }
    }
}
