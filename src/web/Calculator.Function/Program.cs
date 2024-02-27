using FfAdmin.Calculator;
using FfAdmin.Calculator.Core;
using FfAdmin.Calculator.Function;
using Microsoft.Extensions.Hosting;
using FfAdmin.EventStore.AzureSql;
using FfAdmin.ModelCache.BlobStorage;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => 
        services
            .AddMemoryCache()
            //.AddEventStoreClient().BindConfiguration("EventStoreApi").Services
            //.AddModelCacheClient(true).BindConfiguration("ModelCacheApi").Services
            .AddAzureSqlEventStore()
            .AddBlobStorageModelCacheService()
            .AddOptions<PagingEventRepositoryOptions>().Services
            .AddOptions<ModelCacheOptions>()
            //.Configure(o => o.PutEnabled = false)
            .Services
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
            .AddModelProcessor<OptionWorthHistory>()
            .AddModelProcessor<MinimalExits>()
            .AddModelProcessor<CurrentCharityFractionSets>()
            .AddModelProcessor<IdealOptionValuations>()
            .AddModelProcessor<AmountsToTransfer>()
            
            .AddModelProcessor<CumulativeInterest>()
            .AddModelProcessor<DonationStatistics>()
            .AddModelProcessor<AuditHistory>()
            
            .AddModelProcessor<Donors>()
            .AddModelProcessor<DonorDashboardStats>()
            
        )
    .Build();

host.Run();

