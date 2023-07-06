using FfAdmin.Calculator;
using FfAdmin.Calculator.Core;
using FfAdmin.Calculator.Function;
using Microsoft.Extensions.Hosting;
using FfAdmin.EventStore.ApiClient;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => 
        services
            .AddMemoryCache()
            .AddEventStoreClient("https://g4g-event-store.azurewebsites.net")
            .AddOptions<PagingEventRepositoryOptions>().Services
            .AddScoped<CalculatorDependencies>()
            .AddScoped<IModelCacheFactory, ModelCacheFactory>()
            .AddModelProcessor<HistoryHash>()
            .AddModelProcessor<FfAdmin.Calculator.Index>()
            
            .AddModelProcessor<Donations>()
            .AddModelProcessor<DonationRecords>()
            .AddModelProcessor<Charities>()
            .AddModelProcessor<Options>()
            .AddModelProcessor<CharityBalance>()
            .AddModelProcessor<ValidationErrors>()
        
            .AddModelProcessor<OptionWorths>()
            .AddModelProcessor<MinimalExits>()
            .AddModelProcessor<CurrentCharityFractionSets>()
            .AddModelProcessor<IdealOptionValuations>()
            .AddModelProcessor<AmountsToTransfer>()
        )
    .Build();

host.Run();

