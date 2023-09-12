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

    public async Task CreateEmptyBranch(string branchName)
    {
        var response = await _client.PostAsJsonAsync($"/api/branches/{branchName}/new", new object());
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateNewBranchFrom(string newBranchName, string sourceBranchName)
    {
        var response = await _client.PostAsJsonAsync($"/api/branches/{newBranchName}/new", new { Source = sourceBranchName });
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveBranch(string branchName)
    {
        var response = await _client.DeleteAsync($"/api/branches/{branchName}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<Event[]> GetEvents(string branchName, int start, int? count)
    {
        var response = await _client.GetAsync($"/api/branches/{branchName}/events?skip={start}{(count.HasValue ? $"&limit={count.Value}" : "")}");
        response.EnsureSuccessStatusCode();
        var events = await response.Content.ReadFromJsonAsync<JsonDocument[]>() ?? Array.Empty<JsonDocument>();
        return events.Select(d => Event.ReadFrom(d)).ToArray();
    }

    public async Task<int> GetCount(string branchName)
    {
        var response = await _client.GetAsync($"/api/branches/{branchName}/events/count");
        response.EnsureSuccessStatusCode();
        var count = await response.Content.ReadFromJsonAsync<int>();
        return count;
    }
    
    public async Task AddEvents(string branchName, Event[] events)
    {
        var documents = $"[{string.Join(",",events.Select(e => e.ToJsonString()))}]";
        var response = await _client.PostAsync($"/api/branches/{branchName}/events", new StringContent(documents));
        response.EnsureSuccessStatusCode();
    }

    public async Task Rebase(string branchName, string onBranchName)
    {
       var response = await _client.PostAsJsonAsync($"/api/branches/{branchName}/rebase", new { On = onBranchName });
       response.EnsureSuccessStatusCode();
    }

    public async Task FastForward(string branchName, string toBranchName)
    {
        var response = await _client.PostAsJsonAsync($"/api/branches/{branchName}/fastforward", new { To = toBranchName });
        response.EnsureSuccessStatusCode();
    }
}
