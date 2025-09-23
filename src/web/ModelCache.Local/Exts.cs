using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.ModelCache.Local;

public static class Exts {
    public static OptionsBuilder<LocalModelCacheServiceOptions> AddLocalModelCacheService(this IServiceCollection services)
    {
        return services.AddSingleton<IModelCacheService, LocalModelCacheService>()
            .AddOptions<LocalModelCacheServiceOptions>();
    }
}
