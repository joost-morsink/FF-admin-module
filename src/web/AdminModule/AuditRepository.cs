using System;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule
{
    public interface IAuditRepository
    {
        Task<AuditReportInfo[]> GetReports();
        Task<AuditReport> GetReport(int id);
        Task<AuditReport> GetRecentReport();
#nullable disable
        public class AuditReportInfo
        {
            public int Id { get; set; }
            public string Hashcode { get; set; }
        }
        public class AuditReportPart
        {
            public Audit Main { get; set; }
            public AuditFinancial[] Financials { get; set; }
        }
        public class AuditReport
        {
            public AuditReportPart Current { get; set; }
            public AuditReportPart Previous { get; set; }
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
        private IAuditRepository.AuditReport Create(IAuditRepository.AuditReportPart[] parts)
            => new IAuditRepository.AuditReport
            {
                Current = parts[0],
                Previous = parts.Length == 2 ? parts[1] : new IAuditRepository.AuditReportPart
                {
                    Main = new Audit(),
                    Financials = new AuditFinancial[0]
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
                "select main, financials from audit.select_audit() where (main).audit_id <= @aid order by (main).audit_id desc limit 2", new { aid = id });
            return Create(dbres);
        }

        public Task<IAuditRepository.AuditReportInfo[]> GetReports()
            => _database.Query<IAuditRepository.AuditReportInfo>("select audit_id as id, hashcode from audit.main");
    }
}
