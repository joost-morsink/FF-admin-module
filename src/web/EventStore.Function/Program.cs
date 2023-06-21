using Azure.Core.Serialization;
using FfAdmin.Common;
using FfAdmin.EventStore.AzureSql;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(options =>
        options.Serializer = new JsonObjectSerializer(Event.DefaultJsonOptions))
    .ConfigureServices((context, services) => services.AddAzureSqlEventStore())
    .Build();

host.Run();
