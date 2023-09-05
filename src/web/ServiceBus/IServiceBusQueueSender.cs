using System.Threading.Tasks;

namespace FfAdmin.ServiceBus;

public interface IServiceBusQueueSender<T>
{
    Task Send(T item);
}