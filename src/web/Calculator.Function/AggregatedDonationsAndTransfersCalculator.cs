using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class AggregatedDonationsAndTransfersCalculator : BaseCalculator
{
    public AggregatedDonationsAndTransfersCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("AggregatedDonationsAndTransfers")]
    public Task<HttpResponseData> GetAggregatedDonationsAndTransfers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/aggregated-donations-and-transfers")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<AggregatedDonationsAndTransfers>(request, branchName, at);
    [Function("AggregatedDonationsAndTransfersTheory")]
    public Task<HttpResponseData> PostAggregatedDonationsAndTransfers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/aggregated-donations-and-transfers")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<AggregatedDonationsAndTransfers>(request, branchName, @base);
}
