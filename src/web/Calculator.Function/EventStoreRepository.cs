using FfAdmin.Calculator.Core;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;

namespace FfAdmin.Calculator.Function;

public class EventStoreRepository : IEventRepository
{
    private readonly IEventStore _eventStore;
    private readonly string _branchName;
    private readonly ValueTask<int> _initialCount;

    public EventStoreRepository(IEventStore eventStore, string branchName)
    {
        _eventStore = eventStore;
        _branchName = branchName;
        _initialCount = Count();
    }

    public async ValueTask<int> StoredCount()
        => await _eventStore.GetCount(_branchName);
    
    public async ValueTask<int> Count()
        => await _eventStore.GetCount(_branchName);

    public async ValueTask<Event[]> GetEvents(int start, int? count)
        => await _eventStore.GetEvents(_branchName, start, count);

    public async ValueTask<bool> CanUpdate()
        => await _initialCount != await Count(); 
    
}
