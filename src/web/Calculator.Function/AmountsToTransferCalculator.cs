using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class AmountsToTransferCalculator : BaseCalculator
{
    public AmountsToTransferCalculator(CalculatorDependencies dependencies) : base(dependencies) { }

    [Function("AmountsToTransfer")]
    public Task<HttpResponseData> GetAmountsToTransfer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/amounts-to-transfer")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<AmountsToTransfer>(request, branchName, at,
            data => data.Values
                .Select(kvp => (kvp.Key, kvp.Value.Trim(0.01m)))
                .Where(kvp => !kvp.Item2.IsEmpty())
                .ToImmutableDictionary());
    
    [Function("AmountsToTransferTheory")]
    public Task<HttpResponseData> PostAmountsToTransfer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/amounts-to-transfer")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<AmountsToTransfer>(request, branchName, @base,
            data => data.Values
                .Select(kvp => (kvp.Key, kvp.Value.Trim(0.01m)))
                .Where(kvp => !kvp.Item2.IsEmpty())
                .ToImmutableDictionary());
}
