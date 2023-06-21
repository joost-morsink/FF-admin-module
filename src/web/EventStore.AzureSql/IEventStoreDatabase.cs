using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace FfAdmin.EventStore.AzureSql;

public interface IEventStoreDatabase
{
    Task<SqlConnection> OpenConnection();
}

public class EventStoreDatabase : IEventStoreDatabase
{
    private readonly string _connectionString;

    public EventStoreDatabase(string dbName)
    {
        _connectionString = $"Server=tcp:{dbName}.database.windows.net,1433;Initial Catalog=EventStore;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\"";
    }
    public async Task<SqlConnection> OpenConnection()
    {
        var con = new SqlConnection(_connectionString);
        await con.OpenAsync();
        return con;
    }
}
