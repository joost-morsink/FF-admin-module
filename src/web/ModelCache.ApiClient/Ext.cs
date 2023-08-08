using System;
using System.Text.Json;
using System.Threading.Tasks;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.ModelCache.ApiClient;

public static class Ext
{
    public static OptionsBuilder<ModelCacheApiClientOptions> AddModelCacheClient(this IServiceCollection services)
    {
        return services
            .AddHttpClient<ModelCacheApiClient>((provider,client) => client.BaseAddress = provider.GetRequiredService<IOptions<ModelCacheApiClientOptions>>().Value.BaseUri).Services
            .AddScoped<IModelCacheService>(sp => sp.GetRequiredService<ModelCacheApiClient>())
            .AddOptions<ModelCacheApiClientOptions>();
    }
    public static IServiceCollection AddModelCacheClient(this IServiceCollection services, string baseAddress)
    {
        return services.AddModelCacheClient().Configure(options => options.BaseUri = new Uri(baseAddress)).Services;
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


