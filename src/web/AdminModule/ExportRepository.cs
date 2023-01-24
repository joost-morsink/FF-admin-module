using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule
{
    public interface IExportRepository
    {
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public class ExportRow
        {
            public string Donation_id { get; set; } = "";
            public string Donor_id { get; set; } = "";
            public string Option_id { get; set; } = "";
            public string Charity_id { get; set; } = "";
            public string Currency { get; set; } = "";
            public decimal Exchanged_amount { get; set; }
            public bool Has_entered { get; set; }
            public decimal Worth { get; set; }
            public decimal Allocated { get; set; }
            public decimal Transferred { get; set; }
            public decimal Ff_allocated { get; set; }
            public decimal Ff_transferred { get; set; }
        }
        Task<ExportRow[]> GetExportRows();
    }
    public class ExportRepository : IExportRepository
    {
        private readonly IDatabase _database;

        public ExportRepository(IDatabase database)
        {
            _database = database;
        }

        public Task<IExportRepository.ExportRow[]> GetExportRows()
            => _database.Query<IExportRepository.ExportRow>(
                @"select d.donation_ext_id donation_id
                    , d.donor_id
                    , o.option_ext_id option_id
                    , c.charity_ext_id charity_id
                    , we.currency
                    , we.exchanged_amount
                    , we.has_entered
                    , we.worth
                    , we.allocated
                    , we.transferred
                    , we.ff_allocated
                    , we.ff_transferred
                from ff.web_export we
                join ff.donation d on we.donation_id = d.donation_ext_id
                join ff.option o on we.option_id = o.option_ext_id
                join ff.charity c on we.charity_id = c.charity_ext_id");
    }
}
