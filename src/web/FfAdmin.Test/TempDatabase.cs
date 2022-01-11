using System.Threading.Tasks;
using FfAdmin.AdminModule;
namespace FfAdmin.Test
{
    public class TempDatabase : ITemporaryDatabase
    {
        private readonly IDatabase _database;
        private bool _temp;
        public TempDatabase(IDatabase database)
        {
            _database = database;
        }
        public Task UseTemporaryDatabase(string name)
        {
            _temp = true;
            return _database.ApplyOverride(name);
        }
        public ValueTask DisposeAsync()
            => _temp ? new (_database.RemoveOverride()) : new ();
        public void Dispose()
            => DisposeAsync().GetAwaiter().GetResult();
    }
}
