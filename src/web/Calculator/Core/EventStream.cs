namespace FfAdmin.Calculator.Core;

public class EventStream
{
    public static EventStream Empty(IEnumerable<IEventProcessor> processors)
        => new(processors, IEventRepository.Empty);

    public static EventStream Empty(params IEventProcessor[] processors)
        => Empty((IEnumerable<IEventProcessor>)processors);

    private EventStream(IEnumerable<IEventProcessor> processors, IEventRepository events)
    {
        _processors = processors.ToImmutableArray();
        _cache = _processors.ToImmutableDictionary(p => p.ModelType,
            p =>
            {
                var res = (IModelCache)Activator.CreateInstance(
                    typeof(InMemoryModelCache<>).MakeGenericType(p.ModelType))!;
                res.SetAtPosition(0, p.Start);
                return res;
            });
            
        Events = events;
    }

    public IEventRepository Events { get; }
    private readonly ImmutableArray<IEventProcessor> _processors;
    private readonly ImmutableDictionary<Type, IModelCache> _cache;

    public EventStream AddEvents(IEnumerable<Event> events)
        => new(_processors, Events.AddEvents(events));

    private async Task<IContext> CreateContextForPosition(int position)
    {
        if (position <= 0)
            return new ZeroContext(_processors);
        
        var e = await Events.GetEvent(position - 1);
        if (e is null)
            return await GetAtPosition(await Events.Count());
        
        return new ContextImpl(_processors, new Lazy<IContext>(() => GetAtPosition(position - 1).Result), e);
    }

    public Task<IContext> GetAtPosition(int index)
    {
        return CreateContextForPosition(index);
    }

    public async Task<IContext> GetLast() => await GetAtPosition(await Events.Count());

    private class ContextImpl : ICalculatingContext
    {
        private readonly ImmutableArray<IEventProcessor> _processors;
        private readonly Lazy<IContext> _previous;
        private readonly Event _event;
        private TypedDictionary _values;

        public ContextImpl(ImmutableArray<IEventProcessor> processors, Lazy<IContext> previous, Event @event)
        {
            _processors = processors;
            _previous = previous;
            _event = @event;
            _values = TypedDictionary.Empty;
        }

        private T Calculate<T>()
            where T : class
            => (T)Calculate(typeof(T));
        
        private object Calculate(Type type)
        {
            ICalculatingContext current = this;
            foreach (var proc in _processors.Where(p => p.ModelType == type))
            {
                var todo = new Stack<ICalculatingContext>();
                while (!current.IsEvaluated(type))
                {
                    if(current != this)
                        todo.Push(current);
                    if (current.Previous is not ICalculatingContext cc)
                    {
                        var prev = current.Previous.GetContext(type);
                        if (prev is null)
                            throw new InvalidOperationException($"Cannot find previous model for {type}");
                        break;
                    }

                    current = cc;
                }
                while(todo.TryPop(out current!))
                    current.GetContext(type);
                return proc.Process(
                    Previous.GetContext(type) ?? throw new ArgumentException($"Cannot find previous model for {type}"),
                    Previous, this, Event);
            }

            throw new ArgumentException($"Cannot find processor for model type {type}.");
        }

        public T? GetContextOrNull<T>() where T : class
            => (T?)GetContext(typeof(T));
        
        public T GetContext<T>() where T : class
            => GetContextOrNull<T>() ?? throw new ArgumentException("EventProcessor for {typeof(T)} not found");

        public object? GetContext(Type type)
        {
            (_values, var res) = _values.GetOrAdd(type, () => Calculate(type));
            return res;
        }

        public IEnumerable<Type> AvailableContexts => _processors.Select(p => p.ModelType);

        public ICalculatingContext AddEvent(Event @event)
            => new ContextImpl(_processors, new Lazy<IContext>(() => this), @event);

        public bool IsEvaluated<T>()
            => _values.Contains(typeof(T));

        public bool IsEvaluated(Type type)
            => _values.Contains(type);

        public IContext Previous => _previous.Value;
        public Event Event => _event;
    }

    private class ZeroContext : IContext
    {
        private ImmutableArray<IEventProcessor> _processors;

        public ZeroContext(ImmutableArray<IEventProcessor> processors)
        {
            _processors = processors;
        }

        public T? GetContextOrNull<T>() where T : class
            => _processors.OfType<IEventProcessor<T>>().Select(p => p.Start).FirstOrDefault();

        public T GetContext<T>() where T : class
            => GetContextOrNull<T>() ?? throw new ArgumentException("EventProcessor for {typeof(T)} not found");

        public object? GetContext(Type type)
            => _processors.Where(p => p.ModelType == type).Select(p => p.Start).FirstOrDefault();

        public IEnumerable<Type> AvailableContexts => _processors.Select(p => p.ModelType);
        public IContext Previous => this;
        public Event Event => NoneEvent.Instance;
    }
}
