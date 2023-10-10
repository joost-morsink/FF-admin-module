using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Calculator.ApiClient;

public static class Ext
{
    public static OptionsBuilder<CalculatorClientOptions> AddCalculatorClient(this IServiceCollection services)
    {
        return services
            .AddHttpClient<ICalculatorClient, CalculatorClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<CalculatorClientOptions>>().Value;
                client.BaseAddress = options.BaseUri;
            })
            .Services
            .AddOptions<CalculatorClientOptions>();
    }

    public static IServiceCollection AddCalculatorClient(this IServiceCollection services, string baseAddress)
    {
        return services.AddCalculatorClient().Configure(options => options.BaseUri = new Uri(baseAddress)).Services;
    }
}
