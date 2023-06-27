using System;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.EventStore.ApiClient;

public static class Ext
{
    public static IServiceCollection AddEventStoreClient(this IServiceCollection services)
    {
        return services
            .AddOptions<EventStoreApiClientOptions>().Services
            .AddHttpClient<EventStoreApiClient>((provider,client) => client.BaseAddress = provider.GetRequiredService<IOptions<EventStoreApiClientOptions>>().Value.BaseUri).Services
            .AddScoped<IEventStore>(sp => sp.GetRequiredService<EventStoreApiClient>());
    }
    public static IServiceCollection AddEventStoreClient(this IServiceCollection services, string baseAddress)
    {
        return services.AddEventStoreClient()
            .Configure<EventStoreApiClientOptions>(options => options.BaseUri = new Uri(baseAddress));
    }
}
