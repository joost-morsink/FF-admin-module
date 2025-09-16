using FfAdmin.Calculator;
using FfAdmin.Calculator.Core;
using FfAdmin.Calculator.Function;
using FfAdmin.Common;
using Microsoft.Extensions.Hosting;
using FfAdmin.EventStore.AzureSql;
#if !DEBUG
using FfAdmin.ModelCache.BlobStorage;
#endif
using Microsoft.Extensions.DependencyInjection;
#if DEBUG
using FfAdmin.ModelCache.Local;
#endif
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => 
        services
            .AddMemoryCache()
            //.AddEventStoreClient().BindConfiguration("EventStoreApi").Services
            //.AddModelCacheClient(true).BindConfiguration("ModelCacheApi").Services
            .AddAzureSqlEventStore()
#if DEBUG
            .AddLocalModelCacheService()
                .Configure(o => o.Directory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"ff-model-cache"))
                .Services
#else
            .AddBlobStorageModelCacheService()
#endif
            .AddOptions<PagingEventRepositoryOptions>().Services
            .AddOptions<ModelCacheOptions>()
            //.Configure(o => o.PutEnabled = false)
            .Services
            .AddScoped<CalculatorDependencies>()
            .AddScoped<IModelCacheFactory, ModelCacheFactory>()
            .AddSingleton<MetaModels>()
            
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
            .AddModelProcessor<AggregatedDonationsAndTransfers>()
            
            .AddModelProcessor<Donors>()
            .AddModelProcessor<DonorDashboardStats>()
        
            .AddModelProcessor<Donations2, string, Donations2.Details>()
            .AddModelProcessor<OptionWorths2, string, OptionWorths2.Details>()
            .AddModelProcessor<Donors2, string, Donors2.Details>()
            .AddModelProcessor<CharityFractionSets>()
            .AddModelProcessor<Allocations>()
        
            .AddModelCalculator<DonationRecords2, DonationRecords2.Value, string>()
        )
    .Build();

host.Run();

