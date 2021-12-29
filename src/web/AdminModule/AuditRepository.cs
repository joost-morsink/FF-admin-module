using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public interface IAuditRepository
    {
        Task<AuditReportInfo[]> GetReports();
        Task<AuditReport> GetReport(int id);
        Task<AuditReport> GetRecentReport();
#nullable disable
        public class AuditReportInfo
        {
            public int Id { get; set; }
            public DateTimeOffset Timestamp { get; set; }
            public string Hashcode { get; set; }
        }
        public class AuditReportPart
        {
            public Audit Main { get; init; }
            public AuditFinancial[] Financials { get; init; }
        }
        public class AuditReport
        {
            public AuditReportPart Current { get; init; }
            public AuditReportPart Previous { get; init; }
        }
#nullable restore
    }

    public class AuditRepository : IAuditRepository
    {
        private readonly IDatabase _database;

        public AuditRepository(IDatabase database)
        {
            _database = database;
        }
        private static IAuditRepository.AuditReport Create(IReadOnlyList<IAuditRepository.AuditReportPart> parts)
            => new ()
            {
                Current = parts[0],
                Previous = parts.Count == 2 ? parts[1] : new IAuditRepository.AuditReportPart
                {
                    Main = new Audit(), Financials = Array.Empty<AuditFinancial>()
                }
            };
        public async Task<IAuditRepository.AuditReport> GetRecentReport()
        {
            var dbres = await _database.Query<IAuditRepository.AuditReportPart>(
                "select main, financials from audit.select_audit() order by (main).audit_id desc limit 2");
            return Create(dbres);
        }

        public async Task<IAuditRepository.AuditReport> GetReport(int id)
        {
            var dbres = await _database.Query<IAuditRepository.AuditReportPart>(
                "select main, financials from audit.select_audit() where (main).audit_id <= @aid order by (main).audit_id desc limit 2", new
                {
                    aid = id
                });
            return Create(dbres);
        }

        public Task<IAuditRepository.AuditReportInfo[]> GetReports()
            => _database.Query<IAuditRepository.AuditReportInfo>(
                "select audit_id as id, timestamp::timestamp with time zone, hashcode from audit.main order by timestamp desc");
    }
}
