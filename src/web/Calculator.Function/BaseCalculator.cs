using System.Net;
using System.Text.Json;
using FfAdmin.Calculator.Core;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.Calculator.Function;

public abstract class BaseCalculator
{
    protected readonly IEventStore _eventStore;
    protected readonly IMemoryCache _memoryCache;
    protected readonly IOptions<PagingEventRepositoryOptions> _pagingOptions;
    private readonly IModelCacheFactory _modelCacheFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly MetaModels _metaModels;


    public BaseCalculator(CalculatorDependencies dependencies)
    {
        _eventStore = dependencies.EventStore;
        _serviceProvider = dependencies.ServiceProvider;
        _memoryCache = dependencies.MemoryCache;
        _pagingOptions = dependencies.PagingOptions;
        _modelCacheFactory = dependencies.ModelCacheFactory;
        _metaModels = dependencies.MetaModels;
    }

    protected EventStream CreateEventStream(string branchName, IModelCacheStrategy modelCacheStrategy)
    {
        return new EventStream(_serviceProvider,
            _metaModels,
            new PagingEventRepository(new EventStoreRepository(_eventStore, branchName),
                branchName, _memoryCache, _pagingOptions),
            _modelCacheFactory.CreateForBranch(branchName), modelCacheStrategy);
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
        => await Handle(request, GetModel<T>, branchName, baseSequence, projection, onResponse);

    protected async ValueTask<HttpResponseData> BadRequest(HttpRequestData request,
        IEnumerable<ValidationMessage> messages)
    {
        var response = request.CreateResponse();
        await response.WriteAsJsonAsync(messages);
        response.StatusCode = HttpStatusCode.BadRequest;
        return response;
    }

    protected async Task<HttpResponseData> Handle<M>(
        HttpRequestData request,
        Func<EventStream, Task<M>> modelCreator,
        string branchName,
        int? baseSequence,
        Func<M, object?>? projection = null,
        Action<HttpResponseData>? onResponse = null)
    {
        try
        {
            var eventJson = request.Method == "POST" ? await request.ReadAsStringAsync() : null;
            var events = eventJson is null ? [] : ParseEvents(eventJson).ToArray();
            var validationErrors =
                events.Select((e, i) => e.Validate().Select(m => m with {Key = $"{i}.{m.Key}"}))
                    .SelectMany(x => x)
                    .ToList();
            if (validationErrors.Count > 0)
                return await BadRequest(request, validationErrors);
            
            var eventStream = await GetEventStream(branchName, baseSequence, events);
            var model = await modelCreator(eventStream);
            var result = projection?.Invoke(model) ?? model;
            var response = request.CreateResponse(HttpStatusCode.OK);
            onResponse?.Invoke(response);
            await response.WriteAsJsonAsync(result);
            return response;
        } 
        catch (Exception ex) when (ex is KeyNotFoundException or JsonException)
        {
            return await BadRequest(request, new[] {new ValidationMessage("", ex.Message)});
        }
    }

    protected async Task<HttpResponseData> Handle<T>(
        HttpRequestData request,
        string branchName,
        int? baseSequence,
        Func<T, object?>? projection = null,
        Action<HttpResponseData>? onResponse = null)
        where T : class
        => await Handle(request, GetModel<T>, branchName, baseSequence, projection, onResponse);

    protected async Task<HttpResponseData> Handle<T,K,D>(
        HttpRequestData request,
        string branchName,
        int? baseSequence,
        K key,
        Func<D, K, object?>? projection = null)
        where T : class, IModel<T,K,D>
        where K : notnull
        where D : class
        => await Handle(request, s => GetModel<T,K,D>(s, key), branchName, baseSequence, d => projection?.Invoke(d,key));
    
    protected async Task<HttpResponseData> Handle<M,P>(
        HttpRequestData request,
        string branchName,
        int? baseSequence,
        P param,
        Func<M, object?>? projection = null)
        => await Handle(request, s => GetCalculatedModel<M, P>(s,param), branchName, baseSequence, projection);
    protected async Task<HttpResponseData> HandlePost<T,K,D>(
        HttpRequestData request,
        string branchName,
        int? baseSequence,
        K key,
        Func<D, K, object?>? projection = null,
        Action<HttpResponseData>? onResponse = null)
        where T : class, IModel<T,K,D>
        where K : notnull
        where D : class
        => await Handle<D>(request, s => GetModel<T, K, D>(s, key), branchName, baseSequence, d => projection?.Invoke(d, key), onResponse);
    
    protected async Task<T> GetModel<T>(EventStream stream)
        where T : class
        => await stream.Get<T>(await stream.Events.Count());
    protected async Task<D> GetModel<T,K,D>(EventStream stream, K key)
        where T : class, IModel<T,K,D>
        where K : notnull
        where D : class
        => await stream.Get<T,K,D>(await stream.Events.Count(), key);
    protected async Task<M> GetCalculatedModel<M,P>(EventStream stream, P param)
        => await _serviceProvider.GetRequiredService<IModelCalculator<M,P>>().Calculate(await stream.GetLast(), param);
    

    protected async Task<M> GetCalculatedModel<M, P>(string branchName, int? baseSequence, IEnumerable<Event>? events, P param)
    {
        var calculator = _serviceProvider.GetRequiredService<IModelCalculator<M, P>>();
        var str = await GetEventStream(branchName, baseSequence, events);
        var context = await str.GetLast();
        return await calculator.Calculate(context, param);
    }
    protected async Task<EventStream> GetEventStream(string branchName, int? baseSequence, IEnumerable<Event>? events)
    {
        var str = CreateEventStream(branchName, IModelCacheStrategy.Default);
        if (baseSequence.HasValue)
            str = str.Prefix(baseSequence.Value);
        if (events is not null)
            str = str.AddEvents(events);

        var index = await str.Events.Count();
        await str.Get<HistoryHash>(index);
        return str;
    }
}
