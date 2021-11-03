using System;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule
{
    public interface IDonationRepository
    {
        public record DonationAggregation
        {
            public string Currency { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal Worth { get; set; }
            public decimal Allocated { get; set; }
            public decimal Transferred { get; set; }
            public DonationAggregation Round(int decimals)
            {
                Amount = decimal.Round(Amount, decimals);
                Worth = decimal.Round(Worth, decimals);
                Allocated = decimal.Round(Allocated, decimals);
                Transferred = decimal.Round(Transferred, decimals);
                return this;
            }
        }
        Task<DonationAggregation[]> GetAggregations();
    }
    public class DonationRepository : IDonationRepository
    {
        private readonly IDatabase _database;

        public DonationRepository(IDatabase database)
        {
            _database = database;
        }

        public Task<IDonationRepository.DonationAggregation[]> GetAggregations()
            => _database.Query<IDonationRepository.DonationAggregation>(
                @"select currency
                    , sum(exchanged_amount) amount
                    , sum(worth) worth
                    , sum(allocated) allocated
                    , sum(transferred) transferred
                    from ff.web_export
                    group by currency");
    }
}
