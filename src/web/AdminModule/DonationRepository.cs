using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule
{
    public interface IDonationRepository
    {
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public record DonationAggregation
        {
            public string Currency { get; set; } = "";
            public decimal Amount { get; set; }
            public decimal Worth { get; set; }
            public decimal Allocated { get; set; }
            public decimal Transferred { get; set; }
            public decimal Ff_allocated { get; set; }
            public decimal Ff_transferred { get; set; }
            public decimal GetTotalAllocated()
                => Allocated + Ff_allocated;
            public decimal GetTotalTransferred()
                => Transferred + Ff_transferred;
            public decimal GetTotalAllocatedAndTransferred()
                => GetTotalAllocated() + GetTotalTransferred();
            
            public DonationAggregation Round(int decimals)
            {
                Amount = decimal.Round(Amount, decimals);
                Worth = decimal.Round(Worth, decimals);
                Allocated = decimal.Round(Allocated, decimals);
                Transferred = decimal.Round(Transferred, decimals);
                Ff_allocated = decimal.Round(Ff_allocated, decimals);
                Ff_transferred = decimal.Round(Ff_transferred, decimals);
                return this;
            }
        }
        Task<DonationAggregation[]> GetAggregations();

        public record AlreadyImportedDonation
        {
            public string DonationId { get; set; } = "";
            public string CharityId { get; set; } = "";
        }
        Task<AlreadyImportedDonation[]> GetAlreadyImported(IEnumerable<string> extIds);
        
        
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
                    , sum(ff_allocated) ff_allocated
                    , sum(ff_transferred) ff_transferred
                    from ff.web_export
                    group by currency");

        public Task<IDonationRepository.AlreadyImportedDonation[]> GetAlreadyImported(IEnumerable<string> extIds)
            => _database.Query<IDonationRepository.AlreadyImportedDonation>(@"select donation_id DonationId, charity_id CharityId from unnest(@ids) extId
                                            join core.event on extId = donation_id", new
            {
                ids = extIds.ToArray()
            });

    }
}
