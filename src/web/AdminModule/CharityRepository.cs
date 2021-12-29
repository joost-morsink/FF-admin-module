using System.Threading.Tasks;
using FfAdmin.Common;

namespace FfAdmin.AdminModule
{
    public interface ICharityRepository
    {
        Task<Charity[]> GetCharities();
        Task<Charity> GetCharity(int id);
        Task<OpenTransfer[]> GetOpenTransfers();
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
        public Task<Charity> GetCharity(int id)
            => _db.QueryFirst<Charity>("select * from ff.charity where charity_id = @id", new { id });
    }
}
