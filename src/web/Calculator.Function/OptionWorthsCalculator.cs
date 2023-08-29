using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class OptionWorthsCalculator : BaseCalculator
{
    public OptionWorthsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("OptionWorths")]
    public Task<HttpResponseData> GetOptionWorths(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/option-worths")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<OptionWorths>(request, branchName, at, data => data.Worths);
    
    [Function("OptionWorthsTheory")]
    public Task<HttpResponseData> PostOptionWorths(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/option-worths")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<OptionWorths>(request, branchName, @base, data => data.Worths);
}
public class OptionWorthHistoryCalculator : BaseCalculator
{
    public OptionWorthHistoryCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("OptionWorthHistory")]
    public Task<HttpResponseData> GetOptionWorthHistory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/option-worth-history")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<OptionWorthHistory>(request, branchName, at, data => data.Options);
    
    [Function("OptionWorthHistoryTheory")]
    public Task<HttpResponseData> PostOptionWorthHistory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/option-worth-history")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<OptionWorthHistory>(request, branchName, @base, data => data.Options);
}
