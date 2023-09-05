namespace FfAdmin.ServiceBus;

public interface IServiceBusQueues
{
    IServiceBusQueueSender<CleanBranch> CreateCleanBranchSender();
}