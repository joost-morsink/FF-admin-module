using FfAdmin.EventStore.Abstractions;
using FfAdmin.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace FfAdmin.EventStore.AzureSql;

public static class Ext
{
    public static IServiceCollection AddAzureSqlEventStore(this IServiceCollection services, string dbName = "g4g")
    {
        services.AddSingleton<IEventStoreDatabase>(new EventStoreDatabase(dbName));
        services.AddSingleton<IEventStore, AzureSqlEventStore>();
        services.AddG4gServiceBus();
        
        return services;
    }
}
