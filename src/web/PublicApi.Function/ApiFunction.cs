using System.Net;
using System.Text.Json;
using Azure;
using FfAdmin.Calculator;
using FfAdmin.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Memory;

namespace FfAdmin.PublicApi.Function;

public class ApiFunction(IStorageClientProvider provider, IMemoryCache memoryCache)
{
    private async Task<Options> FetchOptions()
    {
        var content = await FetchBlob(Options());
        var options = JsonSerializer.Deserialize<Options>(content);
        return options ?? throw new InvalidOperationException("Failed to fetch options");
    }
    

    private Task<byte[]?> FetchBlob(string path)
    {
        return memoryCache.GetOrCreateAsync($"Blob.{path}", async ce =>
        {
            try
            {
                ce.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                var blobClient = provider.GetStorageClient();

                using var stream = await blobClient.GetBlobClient(path).OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
            catch (RequestFailedException rfe) when (rfe.Status == 404)
            {
                return null;
            }
        });
    }
    private async Task<T> FetchJson<T>(string path)
    {
        var content = await FetchBlob(path);
        if (content is null)
            throw new FileNotFoundException($"Blob not found: {path}");
        return JsonSerializer.Deserialize<T>(content) ?? throw new InvalidOperationException($"Failed to deserialize blob: {path}");
    }

    private async Task<HttpResponseData> FetchBlob(HttpRequestData request, string path, string contentType)
    {
        var contents = await FetchBlob(path);
        if (contents is null)
            return request.CreateResponse(HttpStatusCode.NotFound);
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", contentType);
        response.Headers.Add("Cache-Control","public, max-age=3600");
        await response.Body.WriteAsync(contents);
        await response.Body.FlushAsync();
        return response;
    }

    private Task<HttpResponseData> FetchJson(HttpRequestData request, string path)
        => FetchBlob(request, path, "application/json");

    private Task<HttpResponseData> FetchSvg(HttpRequestData request, string path)
        => FetchBlob(request, path, "image/svg+xml");

    [Function("OptionWorthHistory")]
    public Task<HttpResponseData> GetOptionWorthHistory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "option-worth-history/{id}")]
        HttpRequestData request,
        string id)
        => FetchJson(request, OptionWorthHistory(id));

    [Function("OptionWorthHistoryTable")]
    public async Task<HttpResponseData> GetOptionWorthHistoryTable(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "option-worth-history-table/{id}")]
        HttpRequestData request,
        string id)
    {
        var owh = await FetchJson<OptionWorthRecord[]>(OptionWorthHistory(id));
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/csv");
        response.Headers.Add("Cache-Control","public, max-age=3600");
        await response.WriteStringAsync("Date,Invested,Cash,Value,Allocated,Unentered\n");
        decimal allocated = 0m;
        foreach (var entry in owh)
        {
            if (entry.EventType == EventType.CONV_EXIT)
                allocated += entry.Old.Cash - entry.New.Cash;
            await response.WriteStringAsync($"{entry.Timestamp:yyyy-MM-dd},{entry.New.Invested},{entry.New.Cash},{entry.New.Value},{allocated},{entry.New.Unentered}\n");
        }

        return response;
    }

    
    [Function("OptionWorthHistoryChart")]
    public Task<HttpResponseData> GetOptionWorthHistoryChart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "option-worth-history-chart/{id}")]
        HttpRequestData request, string id)
        => FetchSvg(request, OptionChart(id));

    [Function("Index")]
    public async Task<HttpResponseData> Index(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "index")]
        HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        var options = await FetchOptions();
        response.Headers.Add("Content-Type", "text/html");
        response.Headers.Add("Cache-Control","public, max-age=3600");
        await response.WriteStringAsync($"""
                                         <html><body><h1>Give4Good Public API</h1>
                                         <table border="1">
                                         <tr><th>Option</th><th>Worth History</th><th>Worth History Table</th><th>Worth History Chart</th></tr>
                                         {string.Join("\n", options.Values.Values.Select(o => $"""
                                                                                               <tr><td>{o.Name}</td>
                                                                                               <td><a href="option-worth-history/{o.Id}">option-worth-history/{o.Id}</a></td>
                                                                                               <td><a href="option-worth-history-table/{o.Id}">option-worth-history-table/{o.Id}</a></td>
                                                                                               <td><a href="option-worth-history-chart/{o.Id}">option-worth-history-chart/{o.Id}</a></td></tr>
                                                                                               """))}
                                         </table>
                                         """);
        return response;
    }
}