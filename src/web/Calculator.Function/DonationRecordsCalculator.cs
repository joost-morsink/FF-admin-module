using System.Net;
using FfAdmin.Calculator.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class DonationRecordsCalculator : BaseCalculator
{
    public DonationRecordsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("DonationRecords")]
    public Task<HttpResponseData> GetDonationRecords(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donation-records")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<DonationRecords>(request, branchName, at, data => data.Values);
    [Function("DonationRecordsTheory")]
    public Task<HttpResponseData> PostDonationRecords(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donation-records")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<DonationRecords>(request, branchName, @base, data => data.Values);
    [Function("DonationRecord")]
    public Task<HttpResponseData> GetDonationRecord(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donation-records/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<DonationRecords>(request, branchName, at, data => data.Values.GetValueOrDefault(id));
    [Function("DonationRecordTheory")]
    public Task<HttpResponseData> PostDonationRecord(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donation-records/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<DonationRecords>(request, branchName, @base, data => data.Values.GetValueOrDefault(id));
}
