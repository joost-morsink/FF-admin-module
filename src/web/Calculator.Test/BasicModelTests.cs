namespace FfAdmin.Calculator.Test;

[TestClass]
public class BasicModelTests : VerifyBase
{
    private static DateTimeOffset _current = new DateTimeOffset(2023, 6, 6, 0, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset GetCurrent(TimeSpan? span = null) =>
        _current = _current + (span ?? TimeSpan.FromSeconds(1));

    public static DateTimeOffset Days(int days) => _current = _current.AddDays(days).Date;

    public static readonly Event[] TestEvents =
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
        new NewCharity {Code = "2", Name = "Amnesty", Timestamp = GetCurrent()},
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
        },
        new NewDonation
        {
            Timestamp = GetCurrent(),
            Donation = "2",
            Donor = "1",
            Amount = 10m,
            Exchanged_amount = 5m,
            Currency = "HEUR",
            Charity = "2",
            Option = "1",
            Execute_timestamp = GetCurrent(TimeSpan.Zero)
        },
        new NewDonation
        {
            Timestamp = GetCurrent(),
            Donation = "3",
            Donor = "1",
            Amount = 10m,
            Exchanged_amount = 10m,
            Currency = "EUR",
            Charity = "2",
            Option = "1",
            Execute_timestamp = GetCurrent(TimeSpan.Zero).AddDays(1)
        },
        // 7
        new ConvEnter {Timestamp = GetCurrent(), Invested_amount = 0, Option = "1"},
        new ConvInvest {Timestamp = GetCurrent(), Cash_amount = 2.25m, Invested_amount = 12.50m, Option = "1"},
        new NewDonation
        {
            Timestamp = GetCurrent(),
            Donation = "4",
            Donor = "1",
            Amount = 10m,
            Exchanged_amount = 10m,
            Currency = "EUR",
            Charity = "1",
            Option = "1",
            Execute_timestamp = GetCurrent(TimeSpan.Zero)
        },
        new ConvLiquidate
        {
            Timestamp = GetCurrent(TimeSpan.FromDays(180)),
            Cash_amount = 2.25m,
            Invested_amount = 15.25m,
            Option = "1"
        },
        new ConvExit {Timestamp = GetCurrent(), Amount = decimal.Floor(2.50m * 52.5m) / 100m, Option = "1"},
        // 12
        new ConvEnter {Timestamp = GetCurrent(), Invested_amount = 15.25m, Option = "1"},
        new ConvInvest {Timestamp = GetCurrent(), Invested_amount = 35.25m, Cash_amount = 0.94m, Option = "1"},
        new ConvLiquidate
        {
            Timestamp = GetCurrent(TimeSpan.FromDays(180)),
            Invested_amount = 34.25m,
            Cash_amount = 0.94m,
            Option = "1"
        },
        new ConvExit {Timestamp = GetCurrent(), Amount = 0.17m, Option = "1"},
        // 16
        new ConvTransfer
        {
            Timestamp = GetCurrent(),
            Amount = 0.97m,
            Charity = "1",
            Exchanged_amount = 0.97m,
            Currency = "EUR",
            Exchanged_currency = "EUR"
        },
        new ConvTransfer
        {
            Timestamp = GetCurrent(),
            Amount = 0.51m,
            Charity = "2",
            Exchanged_amount = 0.51m,
            Currency = "EUR",
            Exchanged_currency = "EUR"
        }

        // 18
    };

    private static readonly EventStream Stream = EventStream.Empty(
            Index.Processor,
            Options.Processor,
            Charities.Processor,
            Donations.Processor,
            OptionWorths.Processor,
            IdealOptionValuations.Processor,
            MinimalExits.Processor,
            ValidationErrors.Processor,
            AmountsToTransfer.Processor,
            CurrentCharityFractionSets.Processor,
            DonationRecords.Processor,
            HistoryHash.Processor)
        .AddEvents(TestEvents);

    [TestMethod]
    public void RepoTest()
    {
        var context = Stream.GetLast();
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

    [TestMethod]
    public void CursorTest()
    {
        var context = Stream.GetHistoricContext(2).Previous;
        var options = context.GetContext<Options>();
        options.Should().NotBeNull();
        options!.Values.Should().ContainKey("1");
        var charities = context.GetContext<Charities>();
        charities.Should().NotBeNull();
        charities!.Values.Should().BeEmpty();
        var donations = context.GetContext<Donations>();
        donations.Should().NotBeNull();
        donations!.Values.Should().BeEmpty();
    }

    [TestMethod]
    public void UnenteredTest()
    {
        var context = Stream.GetAtPosition(7);
        var options = context.GetContext<Options>();
        options.Should().NotBeNull();
        var worths = context.GetContext<OptionWorths>();
        worths.Should().NotBeNull();
        var option = worths!.Worths.Should().ContainKey("1").WhoseValue;
        option.Should().BeEquivalentTo(new {Cash = 0m, Invested = 0m});
        option.UnenteredDonations.Should().HaveCount(3);
    }

    [TestMethod]
    public void EnterTest()
    {
        var context = Stream.GetAtPosition(8);
        var options = context.GetContext<Options>();
        options.Should().NotBeNull();
        var worths = context.GetContext<OptionWorths>();
        worths.Should().NotBeNull();
        var option = worths!.Worths.Should().ContainKey("1").WhoseValue;
        option.Should().BeEquivalentTo(new {Cash = 15m, Invested = 0m});
        option.UnenteredDonations.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task OptionsTest()
    {
        var context = Stream.GetLast();
        await Verify(context.GetContext<Options>());
    }

    [TestMethod]
    public async Task CharitiesTest()
    {
        var context = Stream.GetLast();
        await Verify(context.GetContext<Charities>());
    }

    [TestMethod]
    public async Task DonationsTest()
    {
        var context = Stream.GetLast();
        await Verify(context.GetContext<Donations>());
    }

    [TestMethod]
    public async Task OptionWorthsTest()
    {
        var contexts = Stream.GetValues<OptionWorths>(7, 8, 9, 12).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public async Task IdealOptionValuationsGoodYearTest()
    {
        var contexts = Stream.GetValues<IdealOptionValuations>(7, 8, 9, 11, 12).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public async Task IdealOptionValuationsBadYearTest()
    {
        var contexts = Stream.GetValues<IdealOptionValuations>(12, 13, 14, 15, 16).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public async Task MinimalExitsTest()
    {
        var contexts = Stream.GetValues<MinimalExits>(10, 11, 14, 15).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public void ValidationErrorIndicesTest()
    {
        var context = Stream.GetAtPosition(15).GetContext<ValidationErrors>();
        context.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task AmountsToTransferTest()
    {
        var contexts = Stream.GetValues<AmountsToTransfer>(12, 16, 18).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public async Task CurrentCharityFractionSetsTest()
    {
        var contexts = Stream.GetValues<CurrentCharityFractionSets>(8, 13).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public async Task DonationRecordsTest()
    {
        var contexts = Stream.GetValues<DonationRecords>(17).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public void DonationRecordsTotalWorthTest()
    {
        for (int i = 0; i < 17; i++)
        {
            var context = Stream.GetAtPosition(i);
            var donations = context.GetContext<Donations>().Values;
            var worths = context.GetContext<OptionWorths>().Worths;
            var records = context.GetContext<DonationRecords>().Values;
            var q =
                from dr in records
                let dw = dr.Value[^1].Worth
                group dw by donations[dr.Key].OptionId
                into g
                select new {OptionId = g.Key, TotalWorth = g.Sum()}
                into dw
                join w in worths on dw.OptionId equals w.Key
                select Math.Abs(dw.TotalWorth - w.Value.TotalWorth - w.Value.UnenteredDonations.Sum(x => x.Amount));
            if (q.Any())
                q.Should().AllSatisfy(x => x.Should().BeApproximately(0m, 0.000000000001m));
        }
    }

    [TestMethod]
    public async Task HashTest()
    {
        var contexts = Stream.GetValues<HistoryHash>(Enumerable.Range(0,18).ToArray()).ToListOrderedByKey();

        await Verify(contexts);
    }

    [TestMethod]
    public void BulkTest()
    {
        var stream = EventStream.Empty(Donations.Processor)
            .AddEvents(Enumerable.Range(0, 1000).Select(x => new NewDonation
            {
                Timestamp = GetCurrent(),
                Donation = x.ToString(),
                Donor = "1",
                Amount = 10m,
                Exchanged_amount = 10m,
                Currency = "EUR",
                Charity = "1",
                Option = "1",
                Execute_timestamp = GetCurrent(TimeSpan.Zero)
            }));
        for (int i = 0; i < 1000; i += 906) // 906 threw a stackoverflow, fixed now
            stream.GetAtPosition(i).GetContext<Donations>();
        var context = stream.GetLast();
        var donations = context.GetContext<Donations>();
        donations.Should().NotBeNull();
    }
}
