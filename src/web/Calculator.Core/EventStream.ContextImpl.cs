namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    private class ContextImpl : ICalculatingContext
    {
        private readonly ImmutableArray<IEventProcessor> _processors;
        private readonly EventStream _parent;
        private readonly Func<IContext> _previous;
        private readonly Event _event;
        private readonly int _index;
        private TypedDictionary _values;

        public ContextImpl(EventStream parent, Func<IContext> previous, Event @event, int index)
        {
            _processors = parent._processors;
            _parent = parent;
            _previous = previous;
            _event = @event;
            _index = index;
            _values = TypedDictionary.Empty;
        }

        private object Calculate(Type type)
        {
            ICalculatingContext? current = this;
            foreach (var proc in _processors.Where(p => p.ModelType == type))
            {
                var todo = new Stack<ICalculatingContext>();
                while (!current.IsEvaluated(type))
                {
                    if (current != this)
                        todo.Push(current);
                    if (current.Previous is not ICalculatingContext cc)
                    {
                        var prev = current.Previous.GetContext(type);
                        if (prev is null)
                            throw new MissingDataException(_index, type);
                        break;
                    }

                    current = cc;
                }

                while (todo.TryPop(out current))
                    current.GetContext(type);
                return proc.Process(
                    Previous.GetContext(type) ?? throw new MissingDataException(_index, type),
                    Previous, this, Event);
            }

            throw new ArgumentException($"Cannot find processor for model type {type}.");
        }

        public T? GetContextOrNull<T>() where T : class
            => (T?)GetContext(typeof(T));

        public T GetContext<T>() where T : class
            => GetContextOrNull<T>() ?? throw new ArgumentException($"EventProcessor for {typeof(T)} not found");

        public object? GetContext(Type type)
        {
            (_values, var res) = _values.GetOrAdd(type, () =>
            {
                var res = Calculate(type);
                _parent.OnCalculated(_index, type, res);
                return res;
            });
            return res;
        }

        public IEnumerable<Type> AvailableContexts => _processors.Select(p => p.ModelType);

        public ICalculatingContext AddEvent(Event @event)
            => new ContextImpl(_parent, () => this, @event, _index + 1);

        public bool IsEvaluated<T>()
            => _values.Contains(typeof(T));

        public bool IsEvaluated(Type type)
            => _values.Contains(type);

        public IContext Previous => _previous();
        public Event Event => _event;

        public void SetContext(Type type, object model)
        {
            if (!type.IsInstanceOfType(model))
                throw new InvalidOperationException("Invalid type");
            _values = _values.Set(type, model);
        }
    }
}
