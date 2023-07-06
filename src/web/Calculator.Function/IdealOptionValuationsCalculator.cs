using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class IdealOptionValuationsCalculator : BaseCalculator
{
    public IdealOptionValuationsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("IdealOptionValuations")]
    public Task<HttpResponseData> GetIdealOptionValuations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/ideal-option-valuations")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<IdealOptionValuations>(request, branchName, at, data => data.Valuations);
    
    [Function("IdealOptionValuationsTheory")]
    public Task<HttpResponseData> PostIdealOptionValuations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/ideal-option-valuations")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<IdealOptionValuations>(request, branchName, @base, data => data.Valuations);
}