using System.Net;
using FfAdmin.Calculator.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class DonationRecordsCalculator : BaseCalculator
{
    public DonationRecordsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("DonationRecord")]
    public Task<HttpResponseData> GetDonationRecord(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donation-records/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<DonationRecords2.Value, string>(request, branchName, at, id, data => data.Records);
    [Function("DonationRecordTheory")]
    public Task<HttpResponseData> PostDonationRecord(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donation-records/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => Handle<DonationRecords2.Value, string>(request, branchName, @base, id, data => data.Records);
}
