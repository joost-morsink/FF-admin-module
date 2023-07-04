using System.Collections.Immutable;
using System.Net;
using FfAdmin.Calculator.Core;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class Calculator
{
    private readonly IEventStore _eventStore;
    private readonly IModelCacheFactory _modelCacheFactory;

    public Calculator(IEventStore eventStore, IModelCacheFactory modelCacheFactory)
    {
        _eventStore = eventStore;
        _modelCacheFactory = modelCacheFactory;
    }

    private EventStream CreateEventStream(string branchName)
    {
        return new EventStream(new IEventProcessor[] {HistoryHash.Processor}, _modelCacheFactory,
            new EventStoreRepository(_eventStore, branchName));
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
}
