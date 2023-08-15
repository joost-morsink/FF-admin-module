using System.Collections.Concurrent;
using System.Threading;

namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    public static EventStream Empty(IEnumerable<IEventProcessor> processors)
        => new(processors, IEventRepository.Empty, IModelCache.Empty);

    public static EventStream Empty(params IEventProcessor[] processors)
        => Empty((IEnumerable<IEventProcessor>)processors);

    public EventStream(IEnumerable<IEventProcessor> processors, IEventRepository events, IModelCache modelCache)
        : this(processors.ToImmutableArray(), events, modelCache)
    {
    }

    private EventStream(ImmutableArray<IEventProcessor> processors,
        IEventRepository events, IModelCache modelCache)
    {
        _processors = processors;
        _modelCache = modelCache;
        Events = events;
        _contexts = new();
    }

    public IEventRepository Events { get; }
    private readonly ImmutableArray<IEventProcessor> _processors;
    private readonly IModelCache _modelCache;
    private readonly ConcurrentDictionary<int, IContext> _contexts;

    private IContext GetContextAtPosition(int index)
        => _contexts.TryGetValue(index, out var context)
            ? context
            : throw new MissingDataException(index, typeof(object));

    public EventStream AddEvents(IEnumerable<Event> events)
        => new(_processors, Events.AddEvents(events), _modelCache);

    public EventStream Prefix(int count)
        => new(_processors, Events.Prefixed(count), _modelCache.GetPrefix(count));

    private async Task<IContext> CreateContextForPosition(int position)
    {
        if (position <= 0)
            return new ZeroContext(_processors);

        var e = await Events.GetEvent(position - 1);
        if (e is null)
            return await GetAtPosition(await Events.Count());
        var res = new ContextImpl(this, () => GetContextAtPosition(position - 1), e, position);

        if ((await _cacheInfo.Value.Positions).Contains(position))
            foreach (var (modelType, model) in (await Task.WhenAll(_processors.Select(async p =>
                         (p.ModelType, await _modelCache.Get(position, p.ModelType)))))
                     .Where(x => x.Item2 is not null))
                res.SetContext(modelType, model!);
        return res;
    }

    private async Task LoadContexts(int from, int to)
    {
        for (int i = from; i <= to; i++)
        {
            if (!_contexts.ContainsKey(i))
            {
                _contexts.TryAdd(i, await CreateContextForPosition(i));
            }
        }
    }

    public record struct CacheInfo(Task<int[]> Positions, ValueTask<int> Count);

    private static readonly AsyncLocal<CacheInfo> _cacheInfo = new();

    public async Task<IContext> GetAtPosition(int index)
    {
        _cacheInfo.Value = new(_modelCache.GetIndexes(), Events.StoredCount());
        var lowerbound = await _modelCache.GetIndexLowerThanOrEqual(index) ?? 0;
        await LoadContexts(lowerbound, index);
        return _contexts[index];
    }

    public async Task<T> Get<T>(int index)
        where T : class
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be non-negative");
        int cont = index;
        while (true)
        {
            try
            {
                var context = await GetAtPosition(index);
                return context.GetContext<T>();
            }
            catch (MissingDataException mde)
            {
                if (mde.Index >= cont)
                    throw;
                var newLowerBound = await _modelCache.GetIndexLowerThanOrEqual(mde.Index - 1);
                cont = newLowerBound ?? 0;
                await LoadContexts(await _modelCache.GetIndexLowerThanOrEqual(mde.Index - 1) ?? 0, mde.Index);
            }
        }
    }

    public async Task<IContext> GetLast() => await GetAtPosition(await Events.Count());
}
