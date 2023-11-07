using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Calculator.ApiClient;

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
        private readonly ICalculatorClient _calculatorClient;
        private readonly IContext<Branch> _branch;

        public DonationRepository(ICalculatorClient calculatorClient, IContext<Branch> branch)
        {
            _calculatorClient = calculatorClient;
            _branch = branch;
        }

        public async Task<IDonationRepository.DonationAggregation[]> GetAggregations()
            => (from s in (await _calculatorClient.GetDonationStatistics(_branch.Value)).Statistics.Values
                select new IDonationRepository.DonationAggregation()
                {
                    Currency = s.Currency,
                    Amount = (decimal)s.Amount,
                    Allocated = (decimal)s.Allocated,
                    Transferred = (decimal)s.Transferred,
                    Worth = (decimal)s.Worth
                }).ToArray();
        
        public async Task<IDonationRepository.AlreadyImportedDonation[]> GetAlreadyImported(IEnumerable<string> extIds)
        {
            var donations = await _calculatorClient.GetDonations(_branch.Value);
            return (from id in extIds
                    let don = donations.Values.GetValueOrDefault(id)
                    where don is not null
                    select new IDonationRepository.AlreadyImportedDonation {DonationId = id, CharityId = don.CharityId})
                .ToArray();
        }
    }
}
