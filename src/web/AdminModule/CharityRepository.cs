using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace FfAdmin.AdminModule
{
    public interface ICharityRepository
    {
        Task<Charity[]> GetCharities();
        Task<OpenTransfer[]> GetOpenTransfers();
    }
    public class Charity
    {
        public int Charity_id { get; set; }
        public string Charity_ext_id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Bank_name { get; set; }
        public string? Bank_account_no { get; set; }
        public string? Bank_bic { get; set; }
    }
    public class OpenTransfer {
        public int Charity_id { get; set; }
        public string Charity_ext_id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; }
    }
    public class CharityRepository : ICharityRepository
    {
        private readonly IDatabase _db;

        public CharityRepository(IDatabase db)
        {
            _db = db;
        }

        public Task<Charity[]> GetCharities()
            => _db.Query<Charity>("select * from ff.charity");

        public Task<OpenTransfer[]> GetOpenTransfers()
            => _db.Query<OpenTransfer>(@"select charity_id, charity_ext_id, name, currency, amount
                    from ff.charity
                    natural join ff.calculate_open_transfers();");
    }
}
