using System;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
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
    }
}
