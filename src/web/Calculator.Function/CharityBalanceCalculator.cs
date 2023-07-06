using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class CharityBalanceCalculator : BaseCalculator
{
    public CharityBalanceCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("CharityBalance")]
    public Task<HttpResponseData> GetCharityBalance(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/charity-balance")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<CharityBalance>(request, branchName, at);
    
    [Function("CharityBalanceTheory")]
    public Task<HttpResponseData> PostCharityBalance(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/charity-balance")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<CharityBalance>(request, branchName, @base);
}