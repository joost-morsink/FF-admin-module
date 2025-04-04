using System.Reflection;
using Calculator.ApiClient;
using External.GiveWp.ApiClient;
using External.Mollie.ApiClient;
using External.Stripe.ApiClient;
using FfAdmin.EventImport.Function;
using FfAdmin.EventStore.ApiClient;
using FfAdmin.ExchangeRate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((host, config) =>
        {
            if (host.HostingEnvironment.IsDevelopment())
                config.AddUserSecrets(Assembly.GetExecutingAssembly(), false);
        })
    .ConfigureServices((host, services) =>
    {
        services
            .AddMemoryCache()
            .AddCalculatorClient().BindConfiguration("ApiClient:Calculator").Services
            .AddEventStoreClient().BindConfiguration("ApiClient:EventStore").Services
            .AddGiveWpClient().BindConfiguration("ApiClient:GiveWp").Services
            .AddMollieClient().BindConfiguration("ApiClient:Mollie").Services
            .AddStripeClient().BindConfiguration("ApiClient:Stripe").Services
            .AddOpenExchangeRates().BindConfiguration("ApiClient:OpenExchangeRates").Services
            .AddOptions<EventImportOptions>().BindConfiguration("EventImport").Services
            .AddScoped<IEventImportService, EventImportService>();
    })
    .Build();

host.Run();
