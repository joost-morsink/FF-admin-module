using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class HistoryHashCalculator : BaseCalculator
{
    public HistoryHashCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("Hash")]
    public Task<HttpResponseData> GetHash(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/history-hash")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<HistoryHash>(request, branchName, at, x => x.Hash);

    [Function("HashTheory")]
    public Task<HttpResponseData> PostHash(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/history-hash")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<HistoryHash>(request, branchName, @base, x => x.Hash);
}