using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Calculator.ApiClient;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;

namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public interface IEventRepository
    {
        Task Import(IEnumerable<Event> e);
        Task<Statistics> GetStatistics();
        Task<Event[]> GetEvents(int start, int? count);
        Task<string[]> GetBranchNames();
        Task CreateEmptyBranch(string branchName);
        Task Branch(string newBranchName);
        Task RemoveBranch(string branchName);
        Task FastForward(string branchName);
        Task Rebase(string onBranchName);
        public class Statistics
        {
            public int Processed { get; set; }
            public DateTimeOffset? LastProcessed { get; set; }
            public int Unprocessed { get; set; }
            public DateTimeOffset? FirstUnprocessed { get; set; }
        }
    }

    public class EventRepository : IEventRepository
    {
        private readonly ICalculatorClient _calculator;
        private readonly IEventStore _eventStore;
        private readonly IContext<Branch> _branchContext;

        public EventRepository(ICalculatorClient calculator, IEventStore eventStore, IContext<Branch> branchContext)
        {
            _calculator = calculator;
            _eventStore = eventStore;
            _branchContext = branchContext;
        }

        public async Task Import(IEnumerable<Event> events)
        {
            events = events.ToArray();
            
            var errors = await _calculator.GetValidationErrors(_branchContext.Value, theory: events);
            if (!errors.IsValid)
            {
                var count = await _eventStore.GetCount(_branchContext.Value);
                
                throw new ValidationException(errors.Errors.ToMessages(count));
            }

            await _eventStore.AddEvents(_branchContext.Value, events.ToArray());
        }

        public async Task<IEventRepository.Statistics> GetStatistics()
        {
            var count = await _eventStore.GetCount(_branchContext.Value);
            var ev = await _eventStore.GetEvents(_branchContext.Value, count - 1, 1);
            return new() {FirstUnprocessed = null, Unprocessed = 0, LastProcessed = ev.FirstOrDefault()?.Timestamp, Processed = count};
        }
        public Task<Event[]> GetEvents(int start, int? count)
            => _eventStore.GetEvents(_branchContext.Value, start, count);

        public Task<string[]> GetBranchNames()
            => _eventStore.GetBranchNames();
        
        public Task CreateEmptyBranch(string branchName)
            => _eventStore.CreateEmptyBranch(branchName);

        public Task Branch(string newBranchName)
            => _eventStore.CreateNewBranchFrom(newBranchName, _branchContext.Value);
        
        public Task RemoveBranch(string branchName)
            => _eventStore.RemoveBranch(branchName);
        
        public Task FastForward(string branchName)
            => _eventStore.FastForward(_branchContext.Value, branchName);
        
        public Task Rebase(string onBranchName)
            => _eventStore.Rebase(_branchContext.Value, onBranchName);
    }
}
