using FfAdmin.Calculator.Core;
using Microsoft.Extensions.Hosting;
using FfAdmin.EventStore.ApiClient;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder => 
        builder.Services.AddEventStoreClient("https://g4g-event-store.azurewebsites.net")
            .AddScoped<IModelCacheFactory, ModelCacheFactory>())
    
    .Build();

host.Run();
