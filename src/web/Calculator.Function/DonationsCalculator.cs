using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

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
    [Function("NonExistingDonations")]
    public async Task<HttpResponseData> GetNonExistingDonations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/non-existing-donations")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
    {
        var donations = await GetModel<Donations>(branchName, at, null);
        var body = await request.ReadAsStringAsync();
        var ids = JsonSerializer.Deserialize<string[]>(body!);
        var response = request.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ids!.Where(id => !donations.Values.ContainsKey(id)));
        return response;
    }
}
