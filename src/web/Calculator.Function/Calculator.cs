using System.Collections.Immutable;
using System.Net;
using FfAdmin.Calculator.Core;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Function;

public class Calculator
{
    private readonly IEventStore _eventStore;
    private readonly IModelCacheFactory _modelCacheFactory;
    private readonly IEnumerable<IEventProcessor> _processors;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<PagingEventRepositoryOptions> _pagingOptions;

    public Calculator(IEventStore eventStore, IModelCacheFactory modelCacheFactory, IEnumerable<IEventProcessor> processors, IMemoryCache memoryCache, IOptions<PagingEventRepositoryOptions> pagingOptions)
    {
        _eventStore = eventStore;
        _modelCacheFactory = modelCacheFactory;
        _processors = processors;
        _memoryCache = memoryCache;
        _pagingOptions = pagingOptions;
    }

    private EventStream CreateEventStream(string branchName)
    {
        return new EventStream(_processors, _modelCacheFactory,
            new PagingEventRepository(new EventStoreRepository(_eventStore, branchName),
                 branchName, _memoryCache, _pagingOptions));
    }
    [Function("GetBranches")]
    public async Task<HttpResponseData> GetBranches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);

        var branches = await _eventStore.GetBranchNames();

        await response.WriteAsJsonAsync(branches);

        return response;
    }

    [Function("Hash")]
    public async Task<HttpResponseData> GetHash(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/history-hash")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
    {
        var str = CreateEventStream(branchName);
        var context = at.HasValue ? await str.GetAtPosition(at.Value) : await str.GetLast();
        var hash = context.GetContext<HistoryHash>().Hash;

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(Convert.ToBase64String(hash));
        return response;
    }
    [Function("ValidationErrors")]
    public async Task<HttpResponseData> GetValidationErrors(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/validation-error")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
    {
        var str = CreateEventStream(branchName);
        var context = at.HasValue ? await str.GetAtPosition(at.Value) : await str.GetLast();
        var errors = context.GetContext<ValidationErrors>();

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(errors);
        return response;
    }
}
 
