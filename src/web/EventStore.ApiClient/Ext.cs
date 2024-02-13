using System;
using FfAdmin.EventStore.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.EventStore.ApiClient;

public static class Ext
{
    public static OptionsBuilder<EventStoreApiClientOptions> AddEventStoreClient(this IServiceCollection services)
    {
        return services
            .AddHttpClient<EventStoreApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<EventStoreApiClientOptions>>().Value;
                client.BaseAddress = options.BaseUri;
                client.Timeout = options.Timeout;
            }).AddHttpMessageHandler<AddEventStoreTokenDelegatingHandler>()
            .Services
            .AddScoped<AddEventStoreTokenDelegatingHandler>()
            .AddScoped<IEventStore>(sp => sp.GetRequiredService<EventStoreApiClient>())
            .AddScoped<IEventStoreTokenProvider, EventStoreTokenProvider>()
            .AddOptions<EventStoreApiClientOptions>();
    }

    public static IServiceCollection AddEventStoreClient(this IServiceCollection services, string baseAddress)
    {
        return services.AddEventStoreClient().Configure(options => options.BaseUri = new Uri(baseAddress)).Services;
    }
}
