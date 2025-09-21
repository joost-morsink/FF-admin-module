using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public class DonorDashboardStats2(IContext<Donors2,string, Donors2.Details> cDonors,
    IContext<Donations2, string, Donations2.Details> cDonations,
    IModelCalculator<DonationRecords2.Value, string> cDonationRecords) : IModelCalculator<DonorDashboardStats2.Stat, string>
{
    public record Stat(ImmutableDictionary<string, StatDetail> Donations);

    public record StatDetail(Donation Donation, ImmutableList<DonationRecord2> Records)
    {
        public DonationRecord2 LastRecord()
            => Records.OrderByDescending(r => r.Timestamp).First();
    }
    public async ValueTask<Stat> Calculate(IContext context, string parameter)
    {
        var donations = (await cDonors.GetValue(context, parameter)).Values[parameter];
        var donationRecords = await Task.WhenAll(
            donations.Select(async d => ((await cDonations.GetValue(context,d)).Values[d], await cDonationRecords.Calculate(context, d))));
        var result = donationRecords.ToImmutableDictionary(t => t.Item1.Id, t => new StatDetail(t.Item1, t.Item2.Records));
        return new(result);
    }
}
