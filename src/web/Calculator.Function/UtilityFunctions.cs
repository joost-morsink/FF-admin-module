using System.Net;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class UtilityFunctions
{
    private readonly IEventStore _eventStore;

    public UtilityFunctions(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }
    [Function("GetBranches")]
    public async Task<HttpResponseData> GetBranches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches")]
        HttpRequestData req,
        FunctionContext executionContext)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
    
        var branches = await _eventStore.GetBranchNames();
    
        await response.WriteAsJsonAsync(branches);
    
        return response;
    }
}