using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class AuditHistoryCalculator : BaseCalculator
{
    public AuditHistoryCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("AuditHistory")]
    public Task<HttpResponseData> GetAuditHistory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/audit-history")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<AuditHistory>(request, branchName, at, data => data.Moments);
    
    [Function("AuditHistoryTheory")]
    public Task<HttpResponseData> PostAuditHistory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/audit-history")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<AuditHistory>(request, branchName, @base, data => data.Moments);
}
