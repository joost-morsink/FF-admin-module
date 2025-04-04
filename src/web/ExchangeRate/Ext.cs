using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.ExchangeRate;

public static class Ext
{
    public static OptionsBuilder<OpenExchangeRatesOrgOptions> AddOpenExchangeRates(this IServiceCollection services)
    { 
        var result = services.AddOptions<OpenExchangeRatesOrgOptions>();
        services.AddHttpClient<OpenExchangeRateServiceOrg>((sp, client) =>
                client.BaseAddress = sp.GetRequiredService<IOptions<OpenExchangeRatesOrgOptions>>().Value.BaseUri)
            .Services
            .AddTransient<IExchangeRateService>(sp => sp.GetRequiredService<OpenExchangeRateServiceOrg>());
        return result;
    }
}