using FfAdmin.EventStore.AzureSql;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context,services)=>services.AddAzureSqlEventStore())
    .Build();

host.Run();
