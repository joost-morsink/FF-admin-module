using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace FfAdmin.ServiceBus;

public class ServiceBusSender<T> : IServiceBusQueueSender<T>, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;

    public ServiceBusSender(ServiceBusSender sender)
    {
        _sender = sender;
    }

    public async Task Send(T item)
    {
        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, item);
        var content = ms.ToArray();
        var msg = new ServiceBusMessage(content.AsMemory());
        await _sender.SendMessageAsync(msg);
    }

    public ValueTask DisposeAsync()
        => _sender.DisposeAsync();
}