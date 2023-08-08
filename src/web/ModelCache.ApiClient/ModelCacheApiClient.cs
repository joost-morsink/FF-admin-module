using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FfAdmin.Common;
using FfAdmin.ModelCache.Abstractions;

namespace FfAdmin.ModelCache.ApiClient;

public class ModelCacheApiClient : IModelCacheService
{
    private readonly HttpClient _client;

    public ModelCacheApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task RemoveBranch(string branchName)
    {
        var response = await _client.DeleteAsync($"/api/branches/{branchName}");
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
        var response = await _client.PutAsJsonAsync($"/api/branches/{branchName}/hashes", data);
        response.EnsureSuccessStatusCode();
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
}
