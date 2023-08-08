using System.Net;
using System.Text.Json;
using FfAdmin.Calculator.Core;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;
using FfAdmin.ModelCache.ApiClient;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Function;

public abstract class BaseCalculator
{
    protected readonly IEventStore _eventStore;
    protected readonly IEnumerable<IEventProcessor> _processors;
    protected readonly IMemoryCache _memoryCache;
    protected readonly IOptions<PagingEventRepositoryOptions> _pagingOptions;
    private readonly IModelCacheFactory _modelCacheFactory;

    public BaseCalculator(CalculatorDependencies dependencies)
    {
        _eventStore = dependencies.EventStore;
        _processors = dependencies.Processors;
        _memoryCache = dependencies.MemoryCache;
        _pagingOptions = dependencies.PagingOptions;
        _modelCacheFactory = dependencies.ModelCacheFactory;
    }

    protected EventStream CreateEventStream(string branchName)
    {
        return new EventStream(_processors,
            new PagingEventRepository(new EventStoreRepository(_eventStore, branchName),
                branchName, _memoryCache, _pagingOptions),
            _modelCacheFactory.CreateForBranch(branchName));
    }
    
    protected IEnumerable<Event> ParseEvents(string? json)
    {
        if (json is null)
            return Enumerable.Empty<Event>();
        JsonElement element;
        using (var doc = JsonDocument.Parse(json))
            element = doc.RootElement.Clone();
        if (element.ValueKind == JsonValueKind.Array)
            return element.EnumerateArray().Select(x => Event.ReadFrom(x));
        if (element.ValueKind == JsonValueKind.Object)
            return new[] {Event.ReadFrom(element)};
        return Enumerable.Empty<Event>();
    }

    protected async Task<HttpResponseData> HandlePost<T>(
        HttpRequestData request,
        string branchName,
        int? baseSequence,
        Func<T, object?>? projection = null,
        Action<HttpResponseData>? onResponse = null)
        where T : class
    {
        var json = await request.ReadAsStringAsync();
        try
        {
            var events = ParseEvents(json);
            var validationErrors =
                events.Select((e, i) => e.Validate().Select(m => m with {Key = $"{i}.{m.Key}"}))
                    .SelectMany(x => x)
                    .ToList();
            if (validationErrors.Count > 0)
                return await BadRequest(request, validationErrors);

            return await Handle(request, branchName, baseSequence, projection, events, onResponse);
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is JsonException)
        {
            return await BadRequest(request, new[] {new ValidationMessage("", ex.Message)});
        }
    }

    protected async ValueTask<HttpResponseData> BadRequest(HttpRequestData request,
        IEnumerable<ValidationMessage> messages)
    {
        var response = request.CreateResponse();
        await response.WriteAsJsonAsync(messages);
        response.StatusCode = HttpStatusCode.BadRequest;
        return response;
    }

    protected async Task<HttpResponseData> Handle<T>(
        HttpRequestData request,
        string branchName,
        int? baseSequence,
        Func<T, object?>? projection = null,
        IEnumerable<Event>? events = null,
        Action<HttpResponseData>? onResponse = null)
        where T : class
    {
        var str = CreateEventStream(branchName);
        if (baseSequence.HasValue)
            str = str.Prefix(baseSequence.Value);
        if (events is not null)
            str = str.AddEvents(events);
        var context = baseSequence.HasValue ? await str.GetAtPosition(baseSequence.Value) : await str.GetLast();
        var model = context.GetContext<T>();

        var result = projection is null ? model : projection(model);
        if (result is null)
            return request.CreateResponse(HttpStatusCode.NotFound);

        var response = request.CreateResponse(HttpStatusCode.OK);
        onResponse?.Invoke(response);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}
