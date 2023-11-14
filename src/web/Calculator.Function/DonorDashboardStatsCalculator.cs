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
        var charities = await GetModel<Charities>(branchName, null, null);
        
        return await Handle<DonorDashboardStats>(request, branchName, null,
            data => data.Donors.TryGetValue(donor, out var stat)
                ? new DonorDashboard(donor, stat, charities)
                : null);
    }
}

public class DonorDashboard
{
    public DonorDashboard(string donor, DonorDashboardStat stat, Charities charities)
    {
        Donations = stat.Donations.Select(kvp => new DonationRow(donor, kvp.Key, kvp.Value)).ToList();
        DonationHistory = stat.Donations.SelectMany(kvp => kvp.Value.Select(x => new DonationHistoryRow(donor, kvp.Key, x, charities))).ToList();
    }
    public IList<DonationRow> Donations { get; }
    public IList<DonationHistoryRow> DonationHistory { get; }

    public class DonationRow
    {
        public DonationRow(string donor, string donation, IReadOnlyList<DonationRecord> donationRecords)
        {
            Donor = donor;
            Donation = donation;
            Donated = donationRecords[0].Worth;
            Worth = donationRecords[^1].Worth;
            Allocated = donationRecords.Select(x => x.Allocation?.Amount ?? 0).Sum();
        }

        public decimal Donated { get; }
        public decimal Profit => Worth + Allocated - Donated;
        public decimal Worth { get; }
        public decimal Allocated { get; }
        public string Donor { get; }
        public string Donation { get; }
    }

    public class DonationHistoryRow
    {
        public DonationHistoryRow(string donor, string donation, DonationRecord donationRecord, Charities charities)
        {
            Donor = donor;
            Donation = donation;
            Timestamp = donationRecord.Timestamp;
            Worth = donationRecord.Worth;
            Allocated = donationRecord.Allocation?.Amount ?? 0;
            var c = donationRecord.Allocation?.Charity ?? "";
            Charity = charities.Values.TryGetValue(c, out var charity) ? charity.Name : c;
        }
        public string Donor { get; }
        public string Donation { get; }
        public DateTimeOffset Timestamp { get; }
        public decimal Worth { get; }
        public decimal Allocated { get; }
        public string Charity { get; }
    }
}

