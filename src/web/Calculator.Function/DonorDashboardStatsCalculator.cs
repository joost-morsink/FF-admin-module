using System.Collections.Immutable;
using FfAdmin.Calculator.Core;
using FfAdmin.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.Calculator.Function;

public class DonorDashboardStatsCalculator : BaseCalculator
{
    public DonorDashboardStatsCalculator(CalculatorDependencies dependencies) : base(dependencies) { }
    
    [Function("SingleDonorDashboardStats")]
    public Task<HttpResponseData> GetSingleDonorDashboardStats(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donor-dashboard-stats/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? at)
        => Handle<DonorDashboardStats2.Stat, string>(request, branchName, at, id);
    [Function("SingleDonorDashboardStatsTheory")]
    public Task<HttpResponseData> PostSingleDonorDashboardStats(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{branchName}/donor-dashboard-stats/{id}")]
        HttpRequestData request,
        string branchName,
        string id,
        FunctionContext executionContext,
        int? @base)
        => Handle<DonorDashboardStats2.Stat, string>(request, branchName, @base, id);

    [Function("DonorDashboard")]
    public async Task<HttpResponseData> GetDonorDashboard(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{branchName}/donor-dashboard/{donor}")]
        HttpRequestData request,
        string branchName,
        int? baseSequence,
        string donor,
        FunctionContext executionContext)
    {
        var stream = await GetEventStream(branchName, baseSequence, null);
        var (charities, options) = await (GetModel<Charities>(stream), GetModel<Options>(stream));
        
        return await Handle<DonorDashboardStats2.Stat, string>(request, branchName, baseSequence, donor,
            data => new DonorDashboard(donor, data, options, charities));
    }
}

public class DonorDashboard
{
    public DonorDashboard(string donor, DonorDashboardStats2.Stat stat, Options options, Charities charities)
    {
        Donations = stat.Donations.Select(kvp => new DonationRow(donor, kvp.Value, options, charities)).ToList();
        DonationHistory = new DonationHistoryCalc(stat.Donations.Values, options, charities).Details;
    }
    public IReadOnlyList<DonationRow> Donations { get; }
    public IReadOnlyList<DonationHistoryCalc.Detail> DonationHistory { get; }

    public class DonationRow
    {
        public DonationRow(string donor, DonorDashboardStats2.StatDetail detail, Options options, Charities charities)
        {
            Donor = donor;
            Donation = detail.Donation;
            Donated = detail.Donation.Amount;
            Worth = detail.LastRecord().Worth;
            Allocated = detail.Records.Select(x => x.Allocation?.Amount ?? 0).Sum();
            Currency = options.Values.GetValueOrDefault(Donation.OptionId)?.Currency ?? "???";
            Charity = charities.Values.GetValueOrDefault(Donation.CharityId)?.Name ?? Donation.CharityId;
        }

        public decimal Donated { get; }
        public decimal Profit => Worth + Allocated - Donated;
        public decimal Worth { get; }
        public decimal Allocated { get; }
        public string Donor { get; }
        public Donation Donation { get; }
        public string Currency { get; }
        public string Charity { get; }
    }

    


    public class DonationHistoryCalc
    {
        public DonationHistoryCalc(IEnumerable<DonorDashboardStats2.StatDetail> detail, Options options, Charities charities)
        {
            var q = from d in detail
                select new Detail(d.Donation,
                    d.Records
                        .Select(r => new Record(r.Timestamp, r.Worth, r.Worth - d.Donation.Amount, r.Allocation?.Amount ?? 0,
                            CollectionExtensions.GetValueOrDefault(options.Values, d.Donation.OptionId)?.Currency ?? "???",
                            CollectionExtensions.GetValueOrDefault(charities.Values, d.Donation.CharityId)?.Name ?? d.Donation.CharityId))
                        .OrderBy(r => r.Timestamp)
                        .ToImmutableList());
            Details = q.ToImmutableList();
        }

        public ImmutableList<Detail> Details { get; }

        public record Detail(Donation Donation, ImmutableList<Record> Records);
        public record Record(DateTimeOffset Timestamp, decimal Worth, decimal Profit, decimal Allocated, string Currency, string Charity);
    }
}


