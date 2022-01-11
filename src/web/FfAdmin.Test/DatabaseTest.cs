using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FluentAssertions;
using NUnit.Framework;

namespace FfAdmin.Test
{
    public abstract class DatabaseTest : ServiceConsumerTest
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            Scope = Services.CreateScope();
            var tempdb = Get<ITemporaryDatabase>(); 
            await tempdb.UseTemporaryDatabase("test");
            var db = Get<IDatabaseRepository>();
            await db.DropStructure();
            await db.UpdateStructure();
        }

        [OneTimeTearDown]
        public ValueTask Teardown()
            => ((IAsyncDisposable)Scope).DisposeAsync();
        public async Task ReadImportAndProcess(DateTime date, int num)
        {
            var events = await this.GetType().ReadEvents(num);
            await ImportAndProcess(date, events);
        }
        public async Task ImportAndProcess(DateTime date, IEnumerable<Event> events)
        {
            var msg = await Get<IEventRepository>().Import(date, events);
            msg.Status.Should().Be(0);

            msg = await Get<IEventRepository>().ProcessEvents(date);
            msg.Status.Should().Be(0);
        }
    }
}
