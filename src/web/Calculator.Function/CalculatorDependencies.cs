using FfAdmin.Calculator.Core;
using FfAdmin.EventStore.Abstractions;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Function;

public record CalculatorDependencies(IServiceProvider ServiceProvider, 
    MetaModels MetaModels,
    IEventStore EventStore,
    IModelCacheFactory ModelCacheFactory,
    IMemoryCache MemoryCache,
    IOptions<PagingEventRepositoryOptions> PagingOptions);
