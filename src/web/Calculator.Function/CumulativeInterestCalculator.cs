using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class CumulativeInterestCalculator : BaseCalculator
{
    public CumulativeInterestCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("CumulativeInterest")]
    public Task<HttpResponseData> GetCumulativeInterest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/cumulative-interest")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<CumulativeInterest>(request, branchName, at,
            data => data.Options);
    
    [Function("CumulativeInterestTheory")]
    public Task<HttpResponseData> PostCumulativeInterest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/cumulative-interest")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<CumulativeInterest>(request, branchName, @base,
            data => data.Options);
}
