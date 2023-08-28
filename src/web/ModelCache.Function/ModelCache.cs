using System.Net;
using System.Threading.Tasks;
using FfAdmin.Common;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FfAdmin.ModelCache.Function;

public class ModelCache
{
    private readonly IModelCacheService _service;

    public ModelCache(IModelCacheService service)
    {
        _service = service;
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
}
