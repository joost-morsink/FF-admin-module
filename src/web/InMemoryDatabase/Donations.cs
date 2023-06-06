namespace FfAdmin.InMemoryDatabase;

public record Donations(ImmutableDictionary<string, Donation> Values)
{
    public static Donations Empty { get; } = new(ImmutableDictionary<string, Donation>.Empty);
}
public record Donation(string Id, DateTimeOffset Timestamp, DateTimeOffset ExecuteTimestamp, string OptionId,
    string CharityId, Real Amount);
public class DonationsEventProcessor : EventProcessor<Donations>
{
    public override Donations Start { get; } = Donations.Empty;

    protected override Donations CancelDonation(Donations model, IContext context, CancelDonation e)
        => new(model.Values.Remove(e.Donation));

    protected override Donations NewDonation(Donations model, IContext context, NewDonation e)
        => new(model.Values.Add(e.Donation,
            new Donation(e.Donation, e.Timestamp, e.Execute_timestamp, e.Option, e.Charity, (Real)e.Exchanged_amount)));
    protected override Donations UpdateCharityForDonation(Donations model, IContext context, UpdateCharityForDonation e)
        => new(model.Values.SetItem(e.Donation,
            model.Values[e.Donation] with { CharityId = e.Charity}));
}
