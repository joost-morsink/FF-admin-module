using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;

namespace FfAdmin.EventStore.AzureSql;

public class AzureSqlEventStore : IEventStore
{
    private readonly IEventStoreDatabase _database;

    public AzureSqlEventStore(IEventStoreDatabase database)
    {
        _database = database;
    }
    public async Task<string[]> GetBranchNames()
    {
        using var connection = await _database.OpenConnection();
        var result = await connection.QueryAsync<string>("select [Name] from [Branches]");
        return result.ToArray();
    }

    public Task CreateEmptyBranch(string branchName)
    {
        throw new System.NotImplementedException();
    }

    public Task CreateNewBranchFrom(string newBranchName, string sourceBranchName)
    {
        throw new System.NotImplementedException();
    }

    public Task<Event[]> GetEvents(string branchName, int start, int? count)
    {
        throw new System.NotImplementedException();
    }

    public Task AddEvents(string branchName, Event[] events)
    {
        throw new System.NotImplementedException();
    }

    public Task Rebase(string branchName, string onBranchName)
    {
        throw new System.NotImplementedException();
    }

    public Task FastForward(string branchName, string sourceBranchName)
    {
        throw new System.NotImplementedException();
    }
}
