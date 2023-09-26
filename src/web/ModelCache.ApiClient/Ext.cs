using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.XPath;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.ModelCache.ApiClient;

public static class Ext
{
    public static OptionsBuilder<ModelCacheApiClientOptions> AddModelCacheClient(this IServiceCollection services, bool cached)
    {
        services
            .AddHttpClient<ModelCacheApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<ModelCacheApiClientOptions>>().Value;
                client.BaseAddress = options.BaseUri;
            }).AddHttpMessageHandler<AddModelCacheTokenDelegatingHandler>().Services
            .AddScoped<AddModelCacheTokenDelegatingHandler>()
            .AddScoped<IModelCacheService>(sp =>
            {
                var client = sp.GetRequiredService<ModelCacheApiClient>();
                if (cached)
                    return new CachingModelCacheService(client, sp.GetRequiredService<IMemoryCache>());
                return client;
            })
            .AddScoped<IModelCacheTokenProvider, ModelCacheTokenProvider>();
        return services.AddOptions<ModelCacheApiClientOptions>();
    }
    public static IServiceCollection AddModelCacheClient(this IServiceCollection services, bool cached, string baseAddress)
    {
        return services.AddModelCacheClient(cached).Configure(options => options.BaseUri = new Uri(baseAddress)).Services;
    }
    
    public static async Task<T?> GetData<T>(this IModelCacheService service, byte[] hash)
    {
        var data = await service.GetData(hash, typeof(T).Name);
        return data is null ? default : JsonSerializer.Deserialize<T>(data);
    }
    public static async Task PutData<T>(this IModelCacheService service, byte[] hash, T data)
    {
        await service.PutData(hash, typeof(T).Name, JsonSerializer.SerializeToUtf8Bytes(data));
    }
}
