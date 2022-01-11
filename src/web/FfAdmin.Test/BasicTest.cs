using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FfAdmin.AdminModule;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.TestExecutor;
using NUnit.Framework;
namespace FfAdmin.Test
{
    [TestFixture]
    [NonParallelizable]
    public class BasicTest : DatabaseTest
    {
        private class FractionRow
        {
            public string Donation_ext_id { get; set; } = "";
            public decimal Fraction { get; set; } = 0m;
        }
        private DateTime _date = new DateTime(2022, 1, 1, 0, 0, 0);
        [Test, Order(1)]
        public async Task MetadataTest()
        {
            await ReadImportAndProcess(_date, 0);

            var options = await Get<IOptionRepository>().GetOptions();
            options.Should().HaveCount(1);

            var charities = await Get<ICharityRepository>().GetCharities();
            charities.Should().HaveCount(3);

            var donations = await Get<IDonationRepository>().GetAggregations();
            donations.Should().ContainSingle(x => x.Currency == "EUR")
                .Which.Should().Match((IDonationRepository.DonationAggregation d) =>
                    d.GetTotalAllocatedAndTransferred() == 0m && d.Amount == 100m && d.Worth == 100m);

        }
        [Test, Order(2)]
        public async Task OptionFractionTest()
        {
            await ReadImportAndProcess(_date, 1);

            var fractions = await Get<IDatabase>().Query<FractionRow>(
                @"select donation_ext_id, fraction  from ff.fraction f
                join ff.option o on f.fractionset_id = o.fractionset_id
                join ff.donation d on f.donation_id = d.donation_id
                where option_ext_id = '1';");

            fractions.Should().HaveCount(4);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "1")
                .Which.Fraction.Should().Be(0.3m);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "2")
                .Which.Fraction.Should().Be(0.2m);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "3")
                .Which.Fraction.Should().Be(0.4m);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "4")
                .Which.Fraction.Should().Be(0.1m);
        }
        [Test, Order(3)]
        public async Task OpenTransfersTest()
        {
            await ReadImportAndProcess(_date, 2);

            var openTransfers = await Get<ICharityRepository>().GetOpenTransfers();

            openTransfers.Should().HaveCount(3);
            openTransfers.Should().ContainSingle(ot => ot.Charity_ext_id == "FF")
                .Which.Amount.Should().Be(10m);
            openTransfers.Should().ContainSingle(ot => ot.Charity_ext_id == "3048")
                .Which.Amount.Should().Be(22.5m);
            openTransfers.Should().ContainSingle(ot => ot.Charity_ext_id == "3055")
                .Which.Amount.Should().Be(22.5m);
        }
        [Test,Order(4)]
        public async Task DonationAggregationsTest()
        {
            var donations = await Get<IDonationRepository>().GetAggregations();
            donations.Should().ContainSingle(x => x.Currency == "EUR")
                .Which.Should().Match((IDonationRepository.DonationAggregation d) =>
                    d.GetTotalAllocated() == 55m && d.Amount == 100m && d.GetTotalTransferred() == 0m && d.Worth == 145m);
        }
        [Test, Order(5)]
        public async Task CharityFractionTest()
        {
            var fractions = await Get<IDatabase>().Query<FractionRow>(
                @"select donation_ext_id, fraction  from ff.fraction f
                join ff.allocation a on f.fractionset_id = a.fractionset_id
                join ff.charity c on a.charity_id = c.charity_id 
                join ff.donation d on f.donation_id = d.donation_id
                where charity_ext_id = '3048';");

            fractions.Should().HaveCount(2);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "1")
                .Which.Fraction.Should().Be(0.6m);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "2")
                .Which.Fraction.Should().Be(0.4m);
            
            fractions = await Get<IDatabase>().Query<FractionRow>(
                @"select donation_ext_id, fraction  from ff.fraction f
                join ff.allocation a on f.fractionset_id = a.fractionset_id
                join ff.charity c on a.charity_id = c.charity_id 
                join ff.donation d on f.donation_id = d.donation_id
                where charity_ext_id = '3055';");
            
            fractions.Should().HaveCount(2);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "3")
                .Which.Fraction.Should().Be(0.8m);
            fractions.Should().ContainSingle(x => x.Donation_ext_id == "4")
                .Which.Fraction.Should().Be(0.2m);
        }
        [Test, Order(6)]
        public async Task TransferTest()
        {
            await ReadImportAndProcess(_date, 3);

            var openTransfers = await Get<ICharityRepository>().GetOpenTransfers();

            openTransfers.Should().HaveCount(3);
            openTransfers.Should().ContainSingle(ot => ot.Charity_ext_id == "FF")
                .Which.Amount.Should().Be(5m);
            openTransfers.Should().ContainSingle(ot => ot.Charity_ext_id == "3048")
                .Which.Amount.Should().Be(12.5m);
            openTransfers.Should().ContainSingle(ot => ot.Charity_ext_id == "3055")
                .Which.Amount.Should().Be(2.5m);
            
        }
    }
}
