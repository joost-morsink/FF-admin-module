using System.Net;
using FfAdmin.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class DonorDashboardStatsCalculator : BaseCalculator
{
    public DonorDashboardStatsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("DonorDashboardStats")]
    public Task<HttpResponseData> GetDonorDashboardStats(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donor-dashboard-stats")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? at)
        => Handle<DonorDashboardStats>(request, branchName, at, data => data.Donors);
    [Function("DonorDashboardStatsTheory")]
    public Task<HttpResponseData> PostDonorDashboardStats(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donor-dashboard-stats")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<DonorDashboardStats>(request, branchName, @base, data => data.Donors);
    [Function("SingleDonorDashboardStats")]
    public Task<HttpResponseData> GetSingleDonorDashboardStats(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donor-dashboard-stats/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<DonorDashboardStats>(request, branchName, at, data => data.Donors.GetValueOrDefault(id));
    [Function("SingleDonorDashboardStatsTheory")]
    public Task<HttpResponseData> PostSingleDonorDashboardStats(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donor-dashboard-stats/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => HandlePost<DonorDashboardStats>(request, branchName, @base, data => data.Donors.GetValueOrDefault(id));
    
    [Function("DonorDashboard")]
    public async Task<HttpResponseData> GetDonorDashboard(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donor-dashboard/{donor}")]
        HttpRequestData request,
        string branchName,
        string donor,
        FunctionContext executionContext)
    {
        var allStats = await GetModel<DonorDashboardStats>(branchName, null, null);
        var stats = allStats.Donors.GetValueOrDefault(donor);
        
        if(stats is null)
            return request.CreateResponse(HttpStatusCode.NotFound);

        var result = CreateHtml(stats);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        response.Headers.Add("Cache-Control", "public, max-age=43200");
        await response.WriteStringAsync(result);
        return response;
    }

    private string CreateHtml(DonorDashboardStat stat)
        => $@"<div class='donor-dashboard'>
{TotalTable(stat)}
</div>";

    private string TotalTable(DonorDashboardStat stat)
    {
        var totalDonations = stat.Donations.Select(d => d.Value.Select(x => x.Worth).FirstOrDefault()).Sum();
        var totalWorth = stat.Donations.Select(d => d.Value.Select(x => x.Worth).LastOrDefault()).Sum();
        var totalAllocated = stat.Donations
            .Select(d => d.Value.Select(x => x.Allocation).SelectValues().Sum(a => a.Amount)).Sum();
        return TableWriter.Build(t =>
            t.FirstColumnHeaderRow("Donated", totalDonations.ToString("N2"))
                .FirstColumnHeaderRow("Profit", (totalWorth+totalAllocated-totalDonations).ToString("N2"))
                .FirstColumnHeaderRow("Worth", totalWorth.ToString("N2"))
                .FirstColumnHeaderRow("Allocated", totalAllocated.ToString("N2")));

    }
}
