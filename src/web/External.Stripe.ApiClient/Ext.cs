using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace External.Stripe.ApiClient;

public static class Ext
{
    public static OptionsBuilder<StripeApiOptions> AddStripeClient(this IServiceCollection services)
    {
        services.AddScoped<IStripeService, StripeService>();

        return services.AddOptions<StripeApiOptions>();
    }
}