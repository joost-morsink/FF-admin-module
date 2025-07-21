using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using InvalidOperationException = System.InvalidOperationException;

namespace FfAdmin.Calculator.Core;

public partial class EventStream
{
    public static EventStream Empty(IServiceProvider serviceProvider, IModelCacheStrategy modelCacheStrategy)
        => new(serviceProvider, serviceProvider.GetRequiredService<MetaModels>(), IEventRepository.Empty, IModelCache.Empty, modelCacheStrategy);

    public EventStream(IServiceProvider serviceProvider, MetaModels metaModels,
        IEventRepository events, IModelCache modelCache, IModelCacheStrategy modelCacheStrategy)
    { 
        _serviceProvider = serviceProvider;
        _metaModels = metaModels;
        _modelCache = modelCache;
        _modelCacheStrategy = modelCacheStrategy;
        Events = events;
        _contexts = new();
    }

    public IEventRepository Events { get; }
    private readonly IServiceProvider _serviceProvider;
    private readonly MetaModels _metaModels;
    private readonly IModelCache _modelCache;
    private readonly IModelCacheStrategy _modelCacheStrategy;
    private readonly ConcurrentDictionary<int, IContext> _contexts;

    private async ValueTask<IContext> GetContextAtPosition(int index)
        => _contexts.TryGetValue(index, out var context)
            ? context
            : await CreateContextForPosition(index);

    public EventStream AddEvents(IEnumerable<Event> events)
        => new(_serviceProvider, _metaModels, Events.AddEvents(events), _modelCache, _modelCacheStrategy);

    public EventStream Prefix(int count)
        => new(_serviceProvider, _metaModels, Events.Prefixed(count), _modelCache.GetPrefix(count), _modelCacheStrategy);

    private async Task<IContext> CreateContextForPosition(int position)
    {
        if (position <= 0)
            return new ZeroContext(_metaModels);

        var e = await Events.GetEvent(position - 1);
        if (e is null)
            return await GetAtPosition(await Events.Count());
        
        var res = new ContextImpl(this, () => GetContextAtPosition(position - 1), e, position);
        if ((await _calculationPositions.Value.Positions).Contains(position))
            return new CachedContext(res, () => GetContextAtPosition(position - 1), this, e, position); 
        
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
    private static ConcurrentQueue<(int, IMetaModel, Bucket?, object)> _calculationQueue = new();
    private static readonly SemaphoreSlim _calculationSemaphore = new(1);
    private void OnCalculated(int index, IMetaModel metamodel, Bucket? bucket, object model)
    {
        _calculationQueue.Enqueue((index, metamodel, bucket, model));
        ProcessCalculationQueue().Ignore();
    }

    private async Task ProcessCalculationQueue()
    {
        await _calculationSemaphore.WaitAsync();
        try
        {
            _calculationPositions.Value = new(_modelCache.GetStoredIndexes(), Events.StoredCount());
            var positions = _modelCacheStrategy.Optimize(await _calculationPositions.Value.Positions, await _calculationPositions.Value.Count);

            while (_calculationQueue.TryDequeue(out var item))
            {
                var (index, metaModel, bucket, model) = item;
                if (_modelCacheStrategy.ShouldCache(positions,
                        await _calculationPositions.Value.Count, index))
                {
                    if (bucket is null)
                        await _modelCache.Put(index, metaModel, model);
                    else
                        await _modelCache.Put(index, metaModel, bucket.Value, metaModel.CleanDetail(model, bucket.Value));
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
                return (T?)await context.GetContext(typeof(T)) ?? throw new InvalidOperationException($"Eventprocessor for {typeof(T)} not found");
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
    public async Task<D> Get<T,K,D>(int index, K key)
        where T : class, IModel<T,K,D>
        where K : notnull
        where D : class
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
                var header = (T?)await context.GetContext(typeof(T)) ?? throw new InvalidOperationException($"Eventprocessor for {typeof(T)} not found");
                var detail = await context.GetContext(header, key);
                
                return (D?)detail ?? throw new InvalidOperationException($"Eventprocessor for {typeof(D)} not found");
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
