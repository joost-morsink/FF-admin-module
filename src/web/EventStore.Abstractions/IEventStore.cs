
namespace FfAdmin.EventStore.Abstractions;

public interface IEventStore
{
    Task<string[]> GetBranchNames();
    Task<bool> BranchExists(string branchName);
    
    Task CreateEmptyBranch(string branchName);
    
    Task CreateNewBranchFrom(string newBranchName, string sourceBranchName);

    Task RemoveBranch(string branchName);
    
    Task<Event[]> GetEvents(string branchName, int start, int? count);

    Task AddEvents(string branchName, Event[] events);
    
    Task Rebase(string branchName, string onBranchName);
    
    Task FastForward(string branchName, string toBranchName);
}
