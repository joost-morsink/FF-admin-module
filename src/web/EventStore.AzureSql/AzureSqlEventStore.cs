using System.Linq;
using System.Text.Json;
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
        await using var connection = await _database.OpenConnection();
        var result = await connection.QueryAsync<string>("select [Name] from [Branch]");
        return result.ToArray();
    }
    
    public async Task<bool> BranchExists(string branchName)
    {
        await using var connection = await _database.OpenConnection();
        var result = await connection.QueryAsync<string>("select [Name] from [Branch] where [Name] = @branchName",
            new {branchName});
        return result.Any();
    }
    
    public async Task CreateEmptyBranch(string branchName)
    {
        await using var connection = await _database.OpenConnection();
        await connection.ExecuteAsync(@"insert into [Branch] ([Name]) values (@branchName)", new {branchName});
    }

    public async Task CreateNewBranchFrom(string newBranchName, string sourceBranchName)
    {
        await using var connection = await _database.OpenConnection();
        await connection.ExecuteAsync("exec [CreateBranchFrom] (@newBranchName, @sourceBranchName)",
            new {newBranchName, sourceBranchName});
    }

    public async Task RemoveBranch(string branchName)
    {
        await using var connection = await _database.OpenConnection();
        await connection.ExecuteAsync("exec [RemoveBranch] @branchName", new {branchName});
        
    }

    private record GetEventResult(int Sequence, string Content);
    public async Task<Event[]> GetEvents(string branchName, int start, int? count)
    {
        await using var connection = await _database.OpenConnection();
        var records = await connection.QueryAsync<GetEventResult>(@"select [Sequence], [Content] from [ConsolidatedEvents]
            where [Branch] = @branchName and [Sequence] >= @start
            and (@count is null or [Sequence] < @start + @count)
            order by [Sequence] asc", new {start, count, branchName});
        return records.Select(r => JsonDocument.Parse(r.Content))
            .Select(d => Event.ReadFrom(d))
            .ToArray();
    }

    public async Task<int> GetCount(string branchName)
    {
        await using var connection = await _database.OpenConnection();
        var result = await connection.QueryFirstAsync<int?>("select max([Sequence]) from [ConsolidatedEvents] where [Branch] = @branchName", new {branchName});
        return result.HasValue ? result.Value + 1 : 0;
    }
    
    public async Task AddEvents(string branchName, Event[] events)
    {
        await using var connection = await _database.OpenConnection();
        foreach (var @event in events)
        {
            var content = @event.ToJsonString();
            await connection.ExecuteAsync("exec [AddEvent] @branchName, @content", new {branchName, content});
        }
    }

    public async Task Rebase(string branchName, string onBranchName)
    {
        await using var connection = await _database.OpenConnection();
        await connection.ExecuteAsync("exec [Rebase] @branchName, @onBranchName", new {branchName, onBranchName});
    }

    public async Task FastForward(string branchName, string toBranchName)
    {
        await using var connection = await _database.OpenConnection();
        await connection.ExecuteAsync("exec [FastForward] @branchName, @toBranchName", new {branchName, toBranchName});
    }
}
