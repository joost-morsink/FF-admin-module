using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.Calculator;

public record Allocation(string Charity, Real Amount);

public class DonationRecords2(
    IContext<OptionWorthHistory> optionWorthHistory,
    IContext<OptionWorths2, string, OptionWorths2.Details> cOptionWorths,
    IContext<Donations2, string, Donations2.Details> cDonations)
    : IModelCalculator<DonationRecords2.Value, string>
{
    public static IModelCalculator<Value, string> Create(IServiceProvider serviceProvider)
        => ActivatorUtilities.CreateInstance<DonationRecords2>(serviceProvider);

    public async ValueTask<Value> Calculate(IContext context, string parameter)
    {
        var worthsHeader = await cOptionWorths.GetValue(context);
        var donationHeader = await cDonations.GetValue(context);
        var donation = (await cDonations.GetValue(context, donationHeader, parameter)).Values[parameter];
        var enteredDonation = (await cOptionWorths.GetValue(context, worthsHeader, parameter)).Shares[parameter];
        var history = (await optionWorthHistory.GetValue(context)).Options[donation.OptionId];

        var result = ImmutableList<DonationRecord2>.Empty.ToBuilder();
        result.Add(new (EventType.DONA_NEW, donation.Timestamp, false, donation.Amount, null));
        
        foreach (var owr in history.Where(x => x.Timestamp > enteredDonation.Timestamp || x.Timestamp == enteredDonation.Timestamp && x.EventType is not EventType.CONV_EXIT and not EventType.CONV_LIQUIDATE))
        {
            var allocation = owr.EventType == EventType.CONV_EXIT && owr.Timestamp > enteredDonation.Timestamp
                ? new Allocation(donation.CharityId, (owr.Old.Value - owr.New.Value) * enteredDonation.Share / owr.New.Divisor)
                : null;
            result.Add(new (owr.EventType, owr.Timestamp, true, owr.New.Value * enteredDonation.Share / owr.New.Divisor, allocation));
        }

        return new(result.ToImmutable());
    }

    public record Value(ImmutableList<DonationRecord2> Records);
}
public record DonationRecord2(EventType eventType, DateTimeOffset Timestamp, bool Entered, Real Worth, Allocation? Allocation);
