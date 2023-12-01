using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace External.Mollie.ApiClient;

public static class Ext
{
    public static OptionsBuilder<MollieClientOptions> AddMollieClient(this IServiceCollection services)
    {
        return services
            .AddScoped<MollieMessageHandler>()
            .AddHttpClient<MollieClient>((sp,client) =>
                client.BaseAddress = sp.GetRequiredService<IOptions<MollieClientOptions>>().Value.BaseUri)
            .AddHttpMessageHandler<MollieMessageHandler>().Services
            .AddOptions<MollieClientOptions>();
        
    }
}