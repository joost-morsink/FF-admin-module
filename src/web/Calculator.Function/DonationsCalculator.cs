using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace FfAdmin.Calculator.Function;

public class DonationsCalculator : BaseCalculator
{
    public DonationsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }

    [Function("Donation")]
    public Task<HttpResponseData> GetDonation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donations/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<Donations2, string, Donations2.Details>(request, branchName, at, id,(data,key) => data.Values.GetValueOrDefault(key));
    [Function("DonationTheory")]
    public Task<HttpResponseData> PostDonation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donations/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<Donations2, string, Donations2.Details>(request, branchName, @base, id, (data, key) => data.Values.GetValueOrDefault(key));
    [Function("SplitDonationOnExistence")]
    public async Task<HttpResponseData> SplitDonationOnExistence(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/non-existing-donations")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
    {
        var body = await request.ReadAsStringAsync();
        var ids = JsonSerializer.Deserialize<string[]>(body!) ?? [];
        var donations = await GetCalculatedModel<DonationExistence.Result, IEnumerable<string>>(branchName, at, null, ids);
        var response = request.CreateResponse(System.Net.HttpStatusCode.OK);
        var dict = new Dictionary<string,string[]>()
            {
                ["exists"] = donations.Existing.ToArray(),
                ["not_exists"] = donations.NotExisting.ToArray()
            };
        await response.WriteAsJsonAsync(dict);
        return response;
    }
}
