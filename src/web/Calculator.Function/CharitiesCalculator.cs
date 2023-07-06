using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class CharitiesCalculator : BaseCalculator
{
    public CharitiesCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("Charities")]
    public Task<HttpResponseData> GetCharities(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/charities")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<Charities>(request, branchName, at, data => data.Values);
    
    [Function("CharitiesTheory")]
    public Task<HttpResponseData> PostCharities(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/charities")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<Charities>(request, branchName, @base, data => data.Values);
    
    [Function("Charity")]
    public Task<HttpResponseData> GetCharity(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/charities/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<Charities>(request, branchName, at, data => data.Values.GetValueOrDefault(id));
    
    [Function("CharityTheory")]
    public Task<HttpResponseData> PostCharity(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/charities/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<Charities>(request, branchName, @base, data => data.Values.GetValueOrDefault(id));
}