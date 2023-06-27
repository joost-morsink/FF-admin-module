using Microsoft.Extensions.Hosting;
using FfAdmin.EventStore.ApiClient;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder => 
        builder.Services.AddEventStoreClient("https://g4g-event-store.azurewebsites.net"))
    .Build();

host.Run();
