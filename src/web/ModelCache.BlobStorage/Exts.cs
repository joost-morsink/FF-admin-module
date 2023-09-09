using System.Linq;
using FfAdmin.ModelCache.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.ModelCache.BlobStorage;

public static class Exts
{
    public static IServiceCollection AddBlobStorageModelCacheService(this IServiceCollection services)
    {
        services.AddSingleton<StorageClient>();
        services.AddScoped<IModelCacheService, ModelCacheService>();
        return services;
    }
}
