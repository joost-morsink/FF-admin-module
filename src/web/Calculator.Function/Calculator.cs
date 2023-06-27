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

    public Calculator(IEventStore eventStore)
    {
        _eventStore = eventStore;
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
        var events = await _eventStore.GetEvents(branchName, 0,null);
        var str = EventStream.Empty(HistoryHash.Processor).AddEvents(events.ToImmutableList());
        var context = at.HasValue ? str.GetAtPosition(at.Value) : str.GetLast();
        var hash = context.GetContext<HistoryHash>().Hash;

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(Convert.ToBase64String(hash));
        return response;
    }
}
