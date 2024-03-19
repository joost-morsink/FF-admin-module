using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FfAdmin.Common;
using FfAdmin.ModelCache.Abstractions;

namespace FfAdmin.ModelCache.ApiClient;

public class ModelCacheApiClient : IModelCacheService, ICheckOnline
{
    private readonly HttpClient _client;

    public ModelCacheApiClient(HttpClient client)
    {
        _client = client;
    }

    private async Task<HttpResponseMessage> PutAsJsonAsync<T>(string address, T item)
    {
        var content = JsonSerializer.Serialize(item);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put, 
            RequestUri = _client.BaseAddress is null ? new Uri(address) : new Uri(_client.BaseAddress, address), 
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
        var response = await _client.SendAsync(request);
        return response;
    }

    public async Task<bool> IsOnline()
    {
        try
        {
            var response = await _client.GetAsync("api/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task ClearCache()
    {
        var response = await _client.DeleteAsync("/api/branches");
        response.EnsureSuccessStatusCode();
    }
    
    public async Task RemoveBranch(string branchName)
    {
        var response = await _client.DeleteAsync($"/api/branches/{branchName}");
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveModel(string type)
    {
        var response = await _client.DeleteAsync($"/api/data/all/{type}");
        response.EnsureSuccessStatusCode();
    }
    
    public async Task<HashesForBranch> GetHashesForBranch(string branchName)
    {
        var response = await _client.GetAsync($"/api/branches/{branchName}/hashes");
        response.EnsureSuccessStatusCode();
        var hashes = await response.Content.ReadFromJsonAsync<HashesForBranch>();
        return hashes ?? HashesForBranch.Empty;
    }

    public async Task PutHashesForBranch(string branchName, HashesForBranch data)
    {
        var response = await PutAsJsonAsync($"/api/branches/{branchName}/hashes", data);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string[]> GetTypesForHash(HashValue hash)
    {
        var response = await _client.GetAsync($"/api/data/{hash.AsSpan().ToHexString()}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return Array.Empty<string>();
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<string[]>() ?? Array.Empty<string>();
    }
    
    public async Task<byte[]?> GetData(HashValue hash, string type)
    {
        var response = await _client.GetAsync($"/api/data/{hash.AsSpan().ToHexString()}/{type}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task PutData(HashValue hash, string type, byte[] data)
    {
        var response = await _client.PutAsync($"/api/data/{hash.AsSpan().ToHexString()}/{type}", new ByteArrayContent(data));
        response.EnsureSuccessStatusCode();
    }
    
    public async Task<bool> RunGarbageCollection()
    {
        var response = await _client.PostAsync("/api/gc", new ByteArrayContent(Array.Empty<byte>()));
        response.EnsureSuccessStatusCode();
        var completed = await response.Content.ReadFromJsonAsync<bool>();
        return completed;
    }
    
    public async Task<string[]> GetBranches()
    {
        var response = await _client.GetAsync("/api/branches");
        response.EnsureSuccessStatusCode();
        var branches = await response.Content.ReadFromJsonAsync<string[]>();
        return branches ?? Array.Empty<string>();
    }
}
