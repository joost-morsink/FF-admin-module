using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;

namespace PostEvent.Function;

public class PostEvent
{
    public PostEvent(IConfiguration config)
    {
        Configuration = config;
    }

    public IConfiguration Configuration { get; }

    private static readonly ImmutableArray<string> environments = new [] { "dev", "test", "acc", "prod" }.ToImmutableArray();
    [Function("PostEvent")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", 
            Route = "{environment}")] HttpRequestData req, string environment,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("PostEvent");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        if (!environments.Contains(environment))
            return req.CreateResponse(HttpStatusCode.NotFound);
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();
        var response = req.CreateResponse(body.Length == 0 ? HttpStatusCode.OK : HttpStatusCode.Accepted);
        if(string.IsNullOrWhiteSpace(body))
            await PostOnServiceBus(environment, body);
        
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString($"Welcome to the {environment} environment of Azure Functions! [{Configuration["Test"]}] [{Configuration["Abc:Def"]}]");
        
        return response;
    }

    public async Task PostOnServiceBus(string environment, string message)
    { 
        var client = new ServiceBusClient(Configuration["Servicebus"]);
        var sender = client.CreateSender($"{environment}-event");
        await sender.SendMessageAsync(new ServiceBusMessage(message));

    }
}
