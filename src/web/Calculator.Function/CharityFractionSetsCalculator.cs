using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class CharityFractionSetsCalculator : BaseCalculator
{
    public CharityFractionSetsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("CurrentCharityFractionSets")]
    public Task<HttpResponseData> GetCurrentCharityFractionSets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/current-charity-fraction-sets")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<CurrentCharityFractionSets>(request, branchName, at, data => data.Sets);
    
    [Function("CurrentCharityFractionSetsTheory")]
    public Task<HttpResponseData> PostCurrentCharityFractionSets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/current-charity-fraction-sets")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<CurrentCharityFractionSets>(request, branchName, @base, data => data.Sets);
}