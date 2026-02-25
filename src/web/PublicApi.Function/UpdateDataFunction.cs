using Calculator.ApiClient;
using FfAdmin.Calculator;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace FfAdmin.PublicApi.Function;

public class UpdateDataFunction(IStorageClientProvider provider, ICalculatorClient client)
{
    [Function("Warmup")]
    public async Task Warmup([TimerTrigger("0 59 4 * * *")] TimerInfo timer)
    {
        await client.IsOnline();
    }

    [Function("UpdateData")]
    public async Task Run([TimerTrigger("0 0 5 * * *")] TimerInfo timer)
    {
        var blobClient = provider.GetStorageClient();
        var options = await client.GetOptions(BRANCH);

        await blobClient.GetBlobClient(Options()).UploadAsync(options.ToJsonStream(), true);
        
        foreach (var id in options.Values.Keys)
        {
            var history = await client.GetOptionWorthHistory(BRANCH, id);
            var chart = await client.GetOptionWorthHistoryChart(BRANCH, id);

            await blobClient.GetBlobClient(OptionWorthHistory(id)).UploadAsync(Deduplicate(history).ToArray().ToJsonStream(), true);
            await blobClient.GetBlobClient(OptionChart(id)).UploadAsync(chart.ToStream(), true);
        }
    }

    private IEnumerable<OptionWorthRecord> Deduplicate(IEnumerable<OptionWorthRecord> history)
    {
        var previousDate = DateOnly.MinValue;
        OptionWorthRecord? previous = null;
        foreach (var current in history)
        {
            var currentDate = current.Timestamp.ToDateOnly();
            if (currentDate > previousDate)
            {
                if (previous is not null)
                    yield return previous;
                previousDate = currentDate;
            }

            previous = current;
        }
        if (previous is not null)
            yield return previous;
    }   
    
#if DEBUG || !DEBUG
    [Function("Test")]
    public async Task Test([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test")] HttpRequestData request)
    {
        await Run(null!);
    }
    #endif
}
