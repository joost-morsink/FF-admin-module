using System;
using FfAdmin.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Calculator.ApiClient;

public static class Ext
{
    public static OptionsBuilder<CalculatorClientOptions> AddCalculatorClient(this IServiceCollection services)
    {
        return services
            .AddHttpClient<CalculatorClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<CalculatorClientOptions>>().Value;
                client.BaseAddress = options.BaseUri;
                client.Timeout = options.Timeout;
            })
            .Services
            .AddScoped<ICalculatorClient>(sp => sp.GetRequiredService<CalculatorClient>())
            .AddScoped<ICheckOnline>(sp => sp.GetRequiredService<CalculatorClient>())
            .AddOptions<CalculatorClientOptions>();
    }

    public static IServiceCollection AddCalculatorClient(this IServiceCollection services, string baseAddress)
    {
        return services.AddCalculatorClient().Configure(options => options.BaseUri = new Uri(baseAddress)).Services;
    }
}
