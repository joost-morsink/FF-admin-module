using FfAdmin.Calculator.Core;
using FfAdmin.EventStore.Abstractions;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Function;

public record CalculatorDependencies(IEventStore EventStore,
    IModelCacheFactory ModelCacheFactory,
    IEnumerable<IEventProcessor> Processors, 
    IMemoryCache MemoryCache,
    IOptions<PagingEventRepositoryOptions> PagingOptions);
