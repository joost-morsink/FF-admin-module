using System;
using System.IO;
using System.Threading.Tasks;
using FfAdmin.ModelCache.Abstractions;
using FfAdmin.ModelCache.BlobStorage;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.ModelCache.Function;

public static class Exts
{
    
    public static IServiceCollection AddBlobStorageModelCacheService(this IServiceCollection services)
    {
        services.AddScoped<StorageClient>();
        services.AddScoped<IModelCacheService, ModelCacheService>();
        return services;
    }

    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}
