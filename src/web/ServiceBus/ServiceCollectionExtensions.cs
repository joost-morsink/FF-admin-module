using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FfAdmin.ServiceBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddG4gServiceBus(this IServiceCollection services, Action<ServiceBusOptions>? configureOptions = null)
    {
        services.AddScoped<IServiceBusQueues, ServiceBusQueues>();
        services.AddOptions<ServiceBusOptions>().Configure<IConfiguration>((o, config) =>
        {
            var val = config.GetSection("ServiceBus")?.GetSection("fullyQualifiedNamespace")?.Value;
            if (val is not null)
                o.Namespace = val;
        });
            
        services.AddScoped<IServiceBusQueueSender<CleanBranch>>(sp =>
            sp.GetRequiredService<IServiceBusQueues>().CreateCleanBranchSender());
        
        if (configureOptions is not null)
            services.Configure(configureOptions);
        
        return services;
    }
}
