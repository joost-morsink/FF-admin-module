using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class MinimalExitsCalculator : BaseCalculator
{
    public MinimalExitsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("MinimalExits")]
    public Task<HttpResponseData> GetMinimalExits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/minimal-exits")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<MinimalExits>(request, branchName, at, data => data.Exits);
    
    [Function("MinimalExitsTheory")]
    public Task<HttpResponseData> PostMinimalExits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/minimal-exits")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<MinimalExits>(request, branchName, @base, data => data.Exits);
}