namespace InMemoryDatabase.Test;

[TestClass]
public class BasicModelTests
{
    private static DateTimeOffset _current = new DateTimeOffset(2023, 6, 6, 0, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset GetCurrent(TimeSpan? span = null) =>
        _current = _current + (span ?? TimeSpan.FromSeconds(1));

    public static DateTimeOffset Days(int days) => _current = _current.AddDays(days).Date;

    public readonly Event[] TestEvents =
    {
        new NewOption
        {
            Code = "1",
            Name = "Default option",
            Timestamp = GetCurrent(),
            Charity_fraction = 0.475m,
            Reinvestment_fraction = 0.475m,
            FutureFund_fraction = 0.05m,
            Bad_year_fraction = 0.01m
        },
        new NewCharity {Code = "FF", Name = "Give for Good", Timestamp = GetCurrent()},
        new NewCharity {Code = "1", Name = "WWF", Timestamp = GetCurrent()},
        new NewDonation
        {
            Timestamp = GetCurrent(),
            Donation = "1",
            Donor = "1",
            Amount = 10m,
            Exchanged_amount = 10m,
            Currency = "EUR",
            Charity = "1",
            Option = "1",
            Execute_timestamp = GetCurrent(TimeSpan.Zero)
        }
    };

    [TestMethod]
    public void TestMethod1()
    {
        var stream = EventStream.Empty(Processors.Create(
            new OptionsEventProcessor(),
            new CharitiesEventProcessor(),
            new DonationsEventProcessor())).AddEvents(TestEvents);
        var context = stream.GetLast();
        var options = context.GetContext<Options>();
        options.Should().NotBeNull();
        options!.Values.Should().ContainKey("1");
        var charities = context.GetContext<Charities>();
        charities.Should().NotBeNull();
        charities!.Values.Should().HaveCountGreaterOrEqualTo(2);
        charities.Values.Should().ContainKey("1").WhoseValue.Name.Should().Be("WWF");
        var donations = context.GetContext<Donations>();
        donations.Should().NotBeNull();
        donations!.Values.Should().ContainKey("1").WhoseValue.Should().BeEquivalentTo(new
        {
            OptionId = "1", CharityId = "1", Amount = 10m
        });
    }
}
