using System;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FluentAssertions;
using NUnit.Framework;

namespace FfAdmin.Test
{
    [TestFixture]
    [NonParallelizable]
    public class Tests : DatabaseTest
    {
        [Test]
        public async Task TrivialTest()
        {
            var eventRepository = Get<IEventRepository>();
            var msg = await eventRepository.Import(DateTime.UtcNow, new NewOption[] { new ()
            {
                Code = "DEF",
                Name = "Default investment option",
                Currency = "EUR",
                Bad_year_fraction = 0.01m,
                FutureFund_fraction = 0.10m,
                Reinvestment_fraction = 0.45m,
                Charity_fraction = 0.45m,
                Timestamp = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.FromHours(1))
            }});
            msg.Status.Should().Be(0);
            
            var options = await Get<IOptionRepository>().GetOptions();
            options.Should().BeEmpty();
            msg = await eventRepository.ProcessEvents(DateTime.UtcNow);

            msg.Status.Should().Be(0);
            options = await Get<IOptionRepository>().GetOptions();
            options.Should().HaveCount(1).And.Subject.ElementAt(0).Id.Should().Be("DEF");
        }
    }
}
