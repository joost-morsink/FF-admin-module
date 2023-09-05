using System;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace FfAdmin.ServiceBus;

public class ServiceBusQueues : IServiceBusQueues
{
    private readonly ServiceBusOptions _options;

    public ServiceBusQueues(IOptions<ServiceBusOptions> options)
    {
        _options = options.Value;
    }

    private ServiceBusSender? GetQueueSender(string name)
    {
        var clientOptions = new ServiceBusClientOptions
        { 
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };
        var client = new ServiceBusClient(
            _options.Namespace,
            new DefaultAzureCredential(),
            clientOptions);
        var sender = client.CreateSender(name);

        return sender;
    }
    public IServiceBusQueueSender<CleanBranch> CreateCleanBranchSender()
    {
        var sender = GetQueueSender("cache-clean-branch");
        if (sender is null)
            throw new InvalidOperationException("Could not find queue 'clean-branch");
        return new ServiceBusSender<CleanBranch>(sender);
    }
}
