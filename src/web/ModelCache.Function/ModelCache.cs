using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FfAdmin.ModelCache.Abstractions;
using FfAdmin.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FfAdmin.Common;

namespace FfAdmin.ModelCache.Function;

public class ModelCache
{
    private readonly IModelCacheService _service;

    public ModelCache(IModelCacheService service)
    {
        _service = service;
    }

    [Function("GetBranches")]
    public async Task<HttpResponseData> GetBranches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches")]
        HttpRequestData request,
        FunctionContext executionContext)
    {
        var result = await _service.GetBranches();
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
    
    [Function("GetHashesForBranch")]
    public async Task<HttpResponseData> GetHashesForBranch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "branches/{branchName}/hashes")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext)
    {
        var result = await _service.GetHashesForBranch(branchName);
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
    
    [Function("PutHashesForBranch")]
    public async Task<HttpResponseData> PutHashesForBranch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "branches/{branchName}/hashes")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext)
    {
        var data = await request.ReadFromJsonAsync<HashesForBranch>();
        if (data is null)
            return request.CreateResponse(HttpStatusCode.BadRequest);
        await _service.PutHashesForBranch(branchName, data);
        var response = request.CreateResponse(HttpStatusCode.OK);
        return response;
    }
    [Function("ClearCache")]
    public async Task<HttpResponseData> ClearCache(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "branches")]
        HttpRequestData request,
        FunctionContext executionContext)
    {
        await _service.ClearCache();
        var response = request.CreateResponse(HttpStatusCode.OK);
        return response;
    }
    [Function("RemoveBranch")]
    public async Task<HttpResponseData> RemoveBranch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "branches/{branchName}")]
        HttpRequestData request,
        string branchName,
        FunctionContext executionContext)
    {
        await _service.RemoveBranch(branchName);
        var response = request.CreateResponse(HttpStatusCode.OK);
        return response;
    }

    [Function("GetAvailableData")]
    public async Task<HttpResponseData> GetAvailableData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "data/{hash}")]
        HttpRequestData request,
        string hash,
        FunctionContext executionContext)
    {
        var result = await _service.GetTypesForHash(hash.ToByteArray());
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
    [Function("GetData")]
    public async Task<HttpResponseData> GetData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "data/{hash}/{type}")]
        HttpRequestData request,
        string hash,
        string type,
        FunctionContext executionContext)
    {
        var result = await _service.GetData(hash.ToByteArray(), type);
        if (result is null)
            return request.CreateResponse(HttpStatusCode.NotFound);
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/octet-stream");
        await response.WriteBytesAsync(result);
        return response;
    }
    
    [Function("PutData")]
    public async Task<HttpResponseData> PutData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "data/{hash}/{type}")]
        HttpRequestData request,
        string hash,
        string type,
        FunctionContext executionContext)
    {
        var data = await request.Body.ReadAllBytesAsync();
        await _service.PutData(hash.ToByteArray(), type, data);
        var response = request.CreateResponse(HttpStatusCode.OK);
        return response;
    }
    
    [Function("RunGarbageCollection")]
    public async Task<HttpResponseData> RunGarbageCollection(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gc")]
        HttpRequestData request,
        FunctionContext executionContext)
    {
        var result = await _service.RunGarbageCollection();
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }

    [Function("CleanBranch")]
    public async Task CleanBranch(
        [ServiceBusTrigger("cache-clean-branch", Connection = "ServiceBus")] string item,
        Int32 deliveryCount,
        DateTime enqueuedTimeUtc,
        string messageId,
        ILogger log)
    {
        var data = JsonSerializer.Deserialize<CleanBranch>(item);
        if (data is null)
            throw new InvalidOperationException("Could not deserialize CleanBranch message");
        await _service.RemoveBranch(data.BranchName);
    }

    [Function("Overview")]
    public async Task<HttpResponseData> Overview(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "overview")]
        HttpRequestData request,
        FunctionContext executionContext)
    {
        var data = new Dictionary<string, (int,string)[]>();
        var branches = await _service.GetBranches();
        foreach(var branch in branches)
        {
            var hashes = await _service.GetHashesForBranch(branch);
            data.Add(branch, hashes.Hashes.Select(h => (h.Key,Convert.ToHexString(h.Value))).ToArray());
        }

        var result = data.SelectMany(kvp => kvp.Value.Select(t => (kvp.Key, t.Item1, t.Item2)))
            .ToLookup(x => x.Item3, x => $"{x.Key}@{x.Item2}")
            .ToDictionary(x => x.Key, x => string.Join(", ",x)); 
        var response =  request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}
