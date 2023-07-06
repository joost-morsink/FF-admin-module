using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class OptionsCalculator : BaseCalculator
{
    public OptionsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("Options")]
    public Task<HttpResponseData> GetOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/options")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<Options>(request, branchName, at, data => data.Values);
    
    [Function("Option")]
    public Task<HttpResponseData> GetOption(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/options/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<Options>(request, branchName, at, data => data.Values.GetValueOrDefault(id));
    
    [Function("OptionsTheory")]
    public Task<HttpResponseData> PostOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/options")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<Options>(request, branchName, @base, data => data.Values);
    
    [Function("OptionTheory")]
    public Task<HttpResponseData> PostOption(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/options/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<Options>(request, branchName, @base, data => data.Values.GetValueOrDefault(id));
}