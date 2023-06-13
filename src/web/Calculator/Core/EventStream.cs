namespace FfAdmin.Calculator.Core;

public class EventStream
{
    public static EventStream Empty(IEnumerable<IEventProcessor> processors)
        => new(processors, ImmutableList<Event>.Empty);
    public static EventStream Empty(params IEventProcessor[] processors)
        => Empty((IEnumerable<IEventProcessor>)processors);
    private EventStream(IEnumerable<IEventProcessor> processors, ImmutableList<Event> events)
    {
        _processors = processors.ToImmutableArray();
        Events = events;
        _cache = _processors
            .ToImmutableDictionary(t => t.ModelType,
                t => (IHistoryCache)Activator.CreateInstance(typeof(HistoryCache<>).MakeGenericType(t.ModelType),
                    new object[] {t.PositionalModelCreator(this)})!);
    }

    private readonly ImmutableDictionary<Type, IHistoryCache> _cache;
    public ImmutableList<Event> Events { get; }
    private readonly ImmutableArray<IEventProcessor> _processors;

    public EventStream AddEvents(IEnumerable<Event> events)
        => new(_processors, Events.AddRange(events));

    private IContext CreateContextForPosition(int position)
    {
        var context = new ContextImpl(this, position);
        return context;
    }

    public IContext GetAtPosition(int index)
    {
        return CreateContextForPosition(index);
    }

    public IContext GetLast() => GetAtPosition(Events.Count);

    public IHistoricContext GetHistoricContext(int position) => new HistoricContextImpl(this, position);
    public IHistoricContext GetLastHistoricContext() => GetHistoricContext(Events.Count);

    private class ContextImpl : IContext
    {
        public ContextImpl(EventStream parent, int position)
        {
            _parent = parent;
            _position = position;
        }

        private readonly EventStream _parent;
        private readonly int _position;

        public T? GetContextOrNull<T>() where T : class
            => _parent._cache.TryGetValue(typeof(T), out var cache)
                ? ((HistoryCache<T>)cache).GetAtPosition(_position)
                : null;

        public T GetContext<T>() where T : class
            => GetContextOrNull<T>() ?? throw new InvalidOperationException($"Context of type {typeof(T)} not found");

        public object? GetContext(Type type)
            => _parent._cache.TryGetValue(type, out var cache)
                ? cache.GetAtPosition(_position)
                : null;

        public IEnumerable<Type> AvailableContexts
            => _parent._cache.Keys;
    }

    private class HistoricContextImpl : IHistoricContext
    {
        public HistoricContextImpl(EventStream stream, int position)
        {
            _stream = stream;
            _position = position;
        }

        private readonly EventStream _stream;
        private readonly int _position;
        public IContext GetByAge(int age) => _stream.GetAtPosition(_position - age);
    }
}
