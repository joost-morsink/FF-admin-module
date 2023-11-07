using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Calculator.ApiClient;
using FfAdmin.Calculator;
using FfAdmin.Common;
using FfAdmin.EventStore.Abstractions;

namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public interface IAuditRepository
    {
        Task<AuditMoment[]> GetReports();
        Task<AuditReport> GetReport(int id);
        Task<AuditReport> GetRecentReport();
        Task AddAuditMoment(DateTimeOffset timestamp = default);
        public class AuditReport
        {
            public required AuditMoment Moment { get; init; }
        }
    }

    public class AuditRepository : IAuditRepository
    {
        private readonly ICalculatorClient _calculator;
        private readonly IEventStore _eventStore;
        private readonly IContext<Branch> _branch;

        public AuditRepository(ICalculatorClient calculator, IEventStore eventStore, IContext<Branch> branch)
        {
            _calculator = calculator;
            _eventStore = eventStore;
            _branch = branch;
        }

        public async Task<IAuditRepository.AuditReport> GetRecentReport()
        {
            var last = (await _calculator.GetAuditHistory(_branch.Value)).Moments.Last();
            return new() {Moment = last};
        }

        public async Task<IAuditRepository.AuditReport> GetReport(int id)
        {
            var moments = (await _calculator.GetAuditHistory(_branch.Value)).Moments;
            return new() {Moment = moments[id]};
        }

        public async Task AddAuditMoment(DateTimeOffset timestamp)
        {
            if (timestamp == default)
                timestamp = DateTimeOffset.UtcNow;

            var count = await _eventStore.GetCount(_branch.Value);

            var (auditHistory, hash) = await (_calculator.GetAuditHistory(_branch.Value, count),
                _calculator.GetHistoryHash(_branch.Value, count));

            var last = auditHistory.Moments.LastOrDefault();
            var @event = new Common.Audit
            {
                Timestamp = timestamp,
                EventCount = count,
                Hashcode = Convert.ToBase64String(hash.Hash),
                PreviousCount = last?.EventCount,
                PreviousHashCode = last?.HashCode
            };
            await _eventStore.AddEvents(_branch.Value, new Event[] {@event});
        }

        public async Task<AuditMoment[]> GetReports()
        {
            return (await _calculator.GetAuditHistory(_branch.Value)).Moments.ToArray();
        }
    }
}
