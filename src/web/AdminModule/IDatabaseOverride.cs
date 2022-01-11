using System;
using System.Linq;
using System.Threading.Tasks;
namespace FfAdmin.AdminModule
{
    public interface IDatabaseOverride : IAsyncDisposable
    {
        Task Init();
        string Name { get; }
        private class Impl : IDatabaseOverride
        {
            private readonly IDatabase _database;

            public Impl(IDatabase database, string name)
            {
                _database = database;
                Name = name;
            }
            public string Name { get; }
            public async Task Init()
            {
                var current = await _database.QueryFirst<string>("select current_database();");
                if (current == Name)
                    return;
                var databases = await _database.Query<string>("select datname from pg_database;");
                if(!databases.Contains(Name))
                    await _database.Execute($"create database {Name};");
            }
            public async ValueTask DisposeAsync()
            {
                if (_database.Override == this)
                    await _database.RemoveOverride();
            }
        }
        public static async Task<IDatabaseOverride> Create(IDatabase database, string name)
        {
            var res = new Impl(database, name);
            await res.Init();
            return res;
        }
    }
}
