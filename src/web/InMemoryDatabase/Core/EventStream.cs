namespace FfAdmin.InMemoryDatabase;

public class EventStream
{
    public static EventStream Empty(Processors processors) 
        => new(processors, ImmutableList<Event>.Empty, ImmutableSortedDictionary<int, IContext>.Empty);

    private EventStream(Processors processors, ImmutableList<Event> events, ImmutableSortedDictionary<int, IContext> cache)
    {
        _processors = processors;
        Events = events;
        _cache = cache;
    }

    public ImmutableList<Event> Events { get; }
    private ImmutableSortedDictionary<int, IContext> _cache;
    private readonly Processors _processors;

    public EventStream AddEvents(IEnumerable<Event> events)
        => new(_processors, Events.AddRange(events), _cache);

    private IContext CreateContextForPosition(int position)
    {
        if (position == 0)
            return _processors.Applicators.Aggregate(TypedDictionary.Empty, (acc, proc) => proc.Start(acc));
        else
        {
            var previousContext = _cache.TryGetValue(position - 1, out var cached)
                ? cached
                : CreateContextForPosition(position - 1);
            var context = TypedDictionary.Empty;

            // ReSharper disable once AccessToModifiedClosure
            context = _processors.Applicators.Aggregate(context,
                (acc, proc) => proc.Process(acc, () => context, previousContext, Events[position - 1]));
            return context;
        }
    }

    public IContext GetAtPosition(int index)
    {
        if (_cache.TryGetValue(index, out var cached))
            return cached;
        _cache = _cache.Add(index, CreateContextForPosition(index));
        return _cache[index];
    }

    public IContext GetLast() => GetAtPosition(Events.Count);
}
