using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;

namespace FfAdmin.EventStore.ApiClient;

public class EventStoreApiClient : IEventStore
{
    private readonly HttpClient _client;

    public EventStoreApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<string[]> GetBranchNames()
    {
        var response = await _client.GetAsync("/api/branches");
        response.EnsureSuccessStatusCode();
        var branches = await response.Content.ReadFromJsonAsync<string[]>();
        return branches ?? Array.Empty<string>();
    }

    public async Task<bool> BranchExists(string branchName)
        => (await GetBranchNames()).Contains(branchName);

    public Task CreateEmptyBranch(string branchName)
    {
        throw new NotImplementedException();
    }

    public Task CreateNewBranchFrom(string newBranchName, string sourceBranchName)
    {
        throw new NotImplementedException();
    }

    public Task RemoveBranch(string branchName)
    {
        throw new NotImplementedException();
    }

    public async Task<Event[]> GetEvents(string branchName, int start, int? count)
    {
        var response = await _client.GetAsync($"/api/branches/{branchName}/events?skip={start}{(count.HasValue ? $"&limit={count.Value}" : "")}");
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<JsonDocument[]>() ?? Array.Empty<JsonDocument>();
        return events.Select(d => Event.ReadFrom(d)).ToArray();
    }

    public Task AddEvents(string branchName, Event[] events)
    {
        throw new NotImplementedException();
    }

    public Task Rebase(string branchName, string onBranchName)
    {
        throw new NotImplementedException();
    }

    public Task FastForward(string branchName, string toBranchName)
    {
        throw new NotImplementedException();
    }
}
