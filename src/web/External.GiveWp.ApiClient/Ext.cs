using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace External.GiveWp.ApiClient;

public static class Ext
{
    public static OptionsBuilder<GiveWpClientOptions> AddGiveWpClient(this IServiceCollection services)
    {
        services.AddHttpClient<GiveWpClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GiveWpClientOptions>>().Value;
                client.BaseAddress = options.BaseUri;
            })
            .AddHttpMessageHandler<GiveWpMessageHandler>();
        services.AddSingleton<GiveWpMessageHandler>();
        return services.AddOptions<GiveWpClientOptions>();
    }
}