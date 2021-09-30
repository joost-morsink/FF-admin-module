using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace AdminModule
{
    public interface ICharityRepository
    {
        Task<Charity[]> GetCharities();
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
    public class CharityRepository : ICharityRepository
    {
        private readonly IDatabase _db;

        public CharityRepository(IDatabase db)
        {
            _db = db;
        }

        public Task<Charity[]> GetCharities()
            => _db.Run(async conn =>
            {
                var res = await conn.QueryAsync<Charity>
                    ("select * from ff.charity");
                return res.ToArray();
            });
    }
}
