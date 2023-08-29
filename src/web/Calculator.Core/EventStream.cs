using System.Collections.Concurrent;
using System.Threading;

namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    public static EventStream Empty(IEnumerable<IEventProcessor> processors, IModelCacheStrategy modelCacheStrategy)
        => new(processors, IEventRepository.Empty, IModelCache.Empty, modelCacheStrategy);

    public static EventStream Empty(IModelCacheStrategy modelCacheStrategy, params IEventProcessor[] processors)
        => Empty(processors, modelCacheStrategy);

    public EventStream(IEnumerable<IEventProcessor> processors, IEventRepository events, IModelCache modelCache,
        IModelCacheStrategy modelCacheStrategy)
        : this(processors.ToImmutableArray(), events, modelCache, modelCacheStrategy)
    {
    }

    private EventStream(ImmutableArray<IEventProcessor> processors,
        IEventRepository events, IModelCache modelCache, IModelCacheStrategy modelCacheStrategy)
    {
        _processors = processors;
        _modelCache = modelCache;
        _modelCacheStrategy = modelCacheStrategy;
        Events = events;
        _contexts = new();
    }

    public IEventRepository Events { get; }
    private readonly ImmutableArray<IEventProcessor> _processors;
    private readonly IModelCache _modelCache;
    private readonly IModelCacheStrategy _modelCacheStrategy;
    private readonly ConcurrentDictionary<int, IContext> _contexts;

    private IContext GetContextAtPosition(int index)
        => _contexts.TryGetValue(index, out var context)
            ? context
            : throw new MissingDataException(index, typeof(object));

    public EventStream AddEvents(IEnumerable<Event> events)
        => new(_processors, Events.AddEvents(events), _modelCache, _modelCacheStrategy);

    public EventStream Prefix(int count)
        => new(_processors, Events.Prefixed(count), _modelCache.GetPrefix(count), _modelCacheStrategy);

    private async Task<IContext> CreateContextForPosition(int position)
    {
        if (position <= 0)
            return new ZeroContext(_processors);

        var e = await Events.GetEvent(position - 1);
        if (e is null)
            return await GetAtPosition(await Events.Count());
        var res = new ContextImpl(this, () => GetContextAtPosition(position - 1), e, position);

        if ((await _calculationPositions.Value.Positions).Contains(position))
            foreach (var (modelType, model) in await _modelCache.GetAvailableData(_processors.Select(x => x.ModelType),
                         position))
                res.SetContext(modelType, model);


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

    public async Task<IContext> GetAtPosition(int index)
    {
        _calculationPositions.Value = new(_modelCache.GetIndexes(), Events.StoredCount());
        var lowerbound = await _modelCache.GetIndexLowerThanOrEqual(index) ?? 0;
        await LoadContexts(lowerbound, index);
        return _contexts[index];
    }

    private record struct CalculationValues(Task<int[]> Positions, ValueTask<int> Count);

    private static readonly AsyncLocal<CalculationValues> _calculationPositions = new();
    private static ConcurrentQueue<(int, Type, object)> _calculationQueue = new();
    private static readonly SemaphoreSlim _calculationSemaphore = new(1);
    private void OnCalculated(int index, Type type, object model)
    {
        _calculationQueue.Enqueue((index, type, model));
        ProcessCalculationQueue().Ignore();
    }

    private async Task ProcessCalculationQueue()
    {
        await _calculationSemaphore.WaitAsync();
        try
        {
            while (_calculationQueue.TryDequeue(out var item))
            {
                _calculationPositions.Value = new(_modelCache.GetIndexes(), Events.StoredCount());
                var (index, type, model) = item;
                var positions = await _calculationPositions.Value.Positions;
                if (_modelCacheStrategy.ShouldCache(positions,
                        await _calculationPositions.Value.Count, index))
                {
                    await _modelCache.Put(index, type, model);
                }
            }
        }
        finally
        {
            _calculationSemaphore.Release();
        }
    }

    public async Task<T> Get<T>(int index)
        where T : class
    {
        _calculationPositions.Value = new(_modelCache.GetIndexes(), Events.StoredCount());

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
