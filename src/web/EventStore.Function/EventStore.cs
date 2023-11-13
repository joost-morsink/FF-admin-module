using System.Text.Json;
using FfAdmin.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.EventStore.Function;

public class EventStore
{
    private readonly IEventStore _eventStore;

    public EventStore(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    [Function("GetAllBranches")]
    public async Task<HttpResponseData> GetAllBranches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches")]
        HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        var branches = await _eventStore.GetBranchNames();
        await response.WriteAsJsonAsync(branches);
        return response;
    }

    private record NewBranchRequest(string? Source);

    [Function("NewBranch")]
    public async Task<HttpResponseData> NewBranch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "branches/{branchName}/new")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext)
    {
        var response = request.CreateResponse(HttpStatusCode.Created);
#if DEBUG
        var str = await request.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<NewBranchRequest>(str!);
#else
        var body = await ReadFromJsonAsync<NewBranchRequest>(request);
#endif
        if (string.IsNullOrWhiteSpace(body?.Source))
            await _eventStore.CreateEmptyBranch(branchName);
        else
            await _eventStore.CreateNewBranchFrom(branchName, body.Source);

        return response;
    }

    [Function("RemoveBranch")]
    public async Task<HttpResponseData> RemoveBranch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "branches/{branchName}")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext)
    {
        var response = request.CreateResponse(HttpStatusCode.NoContent);
        await _eventStore.RemoveBranch(branchName);
        return response;
    }

    [Function("GetEvents")]
    public async Task<HttpResponseData> GetEvents(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches/{branchName}/events")]
        HttpRequestData request,
        string branchName,
        FunctionContext functionContext,
        int? skip, int? limit)
    {
        if (!await _eventStore.BranchExists(branchName))
            return request.CreateResponse(HttpStatusCode.NotFound);
        var response = request.CreateResponse(HttpStatusCode.OK);
        var events = await _eventStore.GetEvents(branchName, skip ?? 0, limit);
        response.Headers.Add("Content-type", "application/json");
        await response.WriteStringAsync($"[{string.Join(",\r\n", events.Select(e => e.ToJsonString()))}]");
        return response;
    }

    [Function("GetCount")]
    public async Task<HttpResponseData> GetCount(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches/{branchName}/events/count")]
        HttpRequestData request,
        string branchName,
        FunctionContext functionContext)
    {
        if (!await _eventStore.BranchExists(branchName))
            return request.CreateResponse(HttpStatusCode.NotFound);
        var response = request.CreateResponse(HttpStatusCode.OK);
        var events = await _eventStore.GetCount(branchName);
        await response.WriteAsJsonAsync(events);
        return response;
    }

    [Function("AddEvents")]
    public async Task<HttpResponseData> AddEvents(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "branches/{branchName}/events")]
        HttpRequestData request,
        string branchName,
        FunctionContext functionContext)
    {
        if (!await _eventStore.BranchExists(branchName))
            return request.CreateResponse(HttpStatusCode.NotFound);

        var docs = await JsonSerializer.DeserializeAsync<JsonDocument[]>(request.Body);
        if (docs is null)
            return request.CreateResponse(HttpStatusCode.BadRequest);

        var events = docs.Select(d => Event.ReadFrom(d)).ToArray();
        await _eventStore.AddEvents(branchName, events);

        var response = request.CreateResponse(HttpStatusCode.Created);
        return response;
    }

    private record RebaseRequest(string On);

    [Function("Rebase")]
    public async Task<HttpResponseData> Rebase(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "branches/{branchName}/rebase")]
        HttpRequestData request,
        string branchName,
        FunctionContext functionContext)
    {
        if (!await _eventStore.BranchExists(branchName))
            return request.CreateResponse(HttpStatusCode.NotFound);

        var on = (await ReadFromJsonAsync<RebaseRequest>(request))?.On;
        if (on is null || !await _eventStore.BranchExists(on))
            return request.CreateResponse(HttpStatusCode.BadRequest);

        await _eventStore.Rebase(branchName, on);

        return request.CreateResponse(HttpStatusCode.OK);
    }

    private record FastForwardRequest(string To);

    [Function("FastForward")]
    public async Task<HttpResponseData> FastForward(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "branches/{branchName}/fast-forward")]
        HttpRequestData request,
        string branchName,
        FunctionContext functionContext)
    {
        if (!await _eventStore.BranchExists(branchName))
            return request.CreateResponse(HttpStatusCode.NotFound);
        
        var to = (await ReadFromJsonAsync<FastForwardRequest>(request))?.To;
        if (to is null || !await _eventStore.BranchExists(to))
            return request.CreateResponse(HttpStatusCode.BadRequest);

        await _eventStore.FastForward(branchName, to);

        return request.CreateResponse(HttpStatusCode.OK);
    }

    private async ValueTask<T?> ReadFromJsonAsync<T>(HttpRequestData request)
    {
        var content = await request.ReadAsStringAsync();
        if (content is null)
            return default;
        return JsonSerializer.Deserialize<T>(content);
    }
}
