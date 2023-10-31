using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calculator.ApiClient;
using FfAdmin.Calculator;
using FfAdmin.Common;
using Charity = FfAdmin.Calculator.Charity;

namespace FfAdmin.AdminModule
{
    public interface ICharityRepository
    {
        Task<Charity[]> GetCharities();
        Task<FractionSet> GetPartitions(string id);
        Task<Charity?> GetCharity(string id);
        Task<Common.OpenTransfer[]> GetOpenTransfers();
    }

    public class CharityRepository : ICharityRepository
    {
        private readonly IDatabase _db;
        private readonly ICalculatorClient _calculator;
        private readonly IContext<Branch> _branch;

        public CharityRepository(IDatabase db, ICalculatorClient calculatorClient, IContext<Branch> branch)
        {
            _db = db;
            _calculator = calculatorClient;
            _branch = branch;
        }

        public async Task<Charity[]> GetCharities()
            => (await _calculator.GetCharities(_branch.Value)).Values.Values.ToArray();

        public async Task<FractionSet> GetPartitions(string id)
            => (await _calculator.GetCharities(_branch.Value)).Values.GetValueOrDefault(id)?.Fractions ??
               FractionSet.Empty.Add(id, 1);

        public async Task<OpenTransfer[]> GetOpenTransfers()
        {
            return (from charity in (await _calculator.GetCharities(_branch.Value)).Values.Values
                join att in (await _calculator.GetAmountsToTransfer(_branch.Value)).Values
                    on charity.Id equals att.Key
                from amt in att.Value.Amounts
                select new OpenTransfer
                {
                    Charity_id = att.Key, Currency = amt.Key, Name = charity.Name, Amount = amt.Value
                }).ToArray();
        }

        public async Task<Charity?> GetCharity(string id)
            => (await _calculator.GetCharities(_branch.Value)).Values.GetValueOrDefault(id);
    }
}
