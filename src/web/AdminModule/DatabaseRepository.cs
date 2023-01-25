using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
namespace FfAdmin.AdminModule
{
    public interface IDatabaseRepository
    {
        Task DropStructure();
        Task UpdateStructure();
        Task<string> CurrentDatabase();
        
    }

    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly IDatabase _database;

        public DatabaseRepository(IDatabase database)
        {
            _database = database;
        }
        public Task DropStructure()
            => _database.Execute(@"drop schema if exists report cascade;
                                       drop schema if exists audit cascade;
                                       drop schema if exists ff cascade;
                                       drop schema if exists core cascade;");

        public async Task UpdateStructure()
        {
            await RunIdempotentDatabaseScript();
            await Reset();
        }
        public Task<string> CurrentDatabase()
            => _database.QueryFirst<string>("select current_database();");
        private async Task RunIdempotentDatabaseScript()
        {
            await using var str = typeof(DatabaseRepository).Assembly.GetManifestResourceStream("FfAdmin.AdminModule.database.sql");
            if (str == null)
                throw new NullReferenceException("Resource stream database.sql could not be found.");
            using var rdr = new StreamReader(str);
            var script = await rdr.ReadToEndAsync();
            await _database.Execute(script);
        }
        private Task Reset()
            => _database.Reset();
        
    }
}
