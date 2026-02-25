using System.Globalization;
using Calculator.ApiClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FfAdmin.PublicApi.Function;

var builder = FunctionsApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton<IStorageClientProvider, StorageClientProvider>()
    .AddCalculatorClient().BindConfiguration("CalculatorApi").Services
    .AddMemoryCache();

builder.Build().Run();

