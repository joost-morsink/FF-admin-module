using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class DonationStatisticsCalculator : BaseCalculator
{
    public DonationStatisticsCalculator(CalculatorDependencies dependencies) : base(dependencies)
    {
    }

    [Function("DonationStatistics")]
    public Task<HttpResponseData> GetDonationStatistics(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donation-statistics")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<DonationStatistics>(request, branchName, at, data => data.Statistics);

    [Function("DonationStatisticsTheory")]
    public Task<HttpResponseData> PostDonationStatistics(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donation-statistics")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<DonationStatistics>(request, branchName, @base, data => data.Statistics);
}
