using System;
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
                await _database.Execute($"drop database if exists {Name}; create database {Name};");
            }
            public ValueTask DisposeAsync()
                => new (_database.Execute($"drop database if exists {Name};"));
        }
        public static async Task<IDatabaseOverride> Create(IDatabase database, string name)
        {
            var res = new Impl(database, name);
            await res.Init();
            return res;
        }
    }
}
