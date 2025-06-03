namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    private class ContextImpl : ICalculatingContext
    {
        private readonly ImmutableArray<IEventProcessor> _processors;
        private readonly EventStream _parent;
        private readonly Lazy<ValueTask<IContext>> _previous;
        private readonly Event _event;
        private readonly int _index;
        private TypedDictionary _values;

        public ContextImpl(EventStream parent, Func<ValueTask<IContext>> previous, Event @event, int index)
        {
            _processors = parent._processors;
            _parent = parent;
            _previous = new Lazy<ValueTask<IContext>>(previous);
            _event = @event;
            _index = index;
            _values = TypedDictionary.Empty;
        }
        private async ValueTask<object> Calculate(Type type)
        {
            ICalculatingContext? current = this;
            foreach (var proc in _processors.Where(p => p.ModelType == type))
            {
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

            throw new ArgumentException($"Cannot find processor for model type {type}.");
        }

        public async ValueTask<object?> GetContext(Type type)
        {
            (_values, var res) = await _values.GetOrAddAsync(type, async () =>
            {
                var res = await Calculate(type);
                _parent.OnCalculated(_index, type, res);
                return res;
            });
            return res;
        }

        public IEnumerable<Type> AvailableContexts => _processors.Select(p => p.ModelType);

        public ICalculatingContext AddEvent(Event @event)
            => new ContextImpl(_parent, () => new(this), @event, _index + 1);

        public bool IsEvaluated<T>()
            => _values.Contains(typeof(T));

        public bool IsEvaluated(Type type)
            => _values.Contains(type);

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
