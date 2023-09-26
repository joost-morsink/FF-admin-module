using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class DonationsCalculator : BaseCalculator
{
    public DonationsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }

    [Function("Donations")]
    public Task<HttpResponseData> GetDonations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donations")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<Donations>(request, branchName, at, data => data.Values);
    [Function("DonationsTheory")]
    public Task<HttpResponseData> PostDonations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donations")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<Donations>(request, branchName, @base, data => data.Values);
    [Function("Donation")]
    public Task<HttpResponseData> GetDonation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donations/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<Donations>(request, branchName, at, data => data.Values.GetValueOrDefault(id));
    [Function("DonationTheory")]
    public Task<HttpResponseData> PostDonation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donations/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<Donations>(request, branchName, @base, data => data.Values.GetValueOrDefault(id));
}
