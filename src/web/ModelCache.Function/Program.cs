using FfAdmin.ModelCache.BlobStorage;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => services.AddBlobStorageModelCacheService())
    .Build();

host.Run();
