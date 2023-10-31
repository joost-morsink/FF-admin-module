using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Calculator.ApiClient;
using FfAdmin.Calculator;

namespace FfAdmin.AdminModule
{
    public interface IOptionRepository
    {
        Task<Option[]> GetOptions();
        Task<Option?> GetOption(string optionId);

        Task<OptionWorth[]> GetOptionWorths();
        Task<OptionWorth?> GetOptionWorth(string optionId);
        
        Task<decimal> GetLoanableCash(string optionId, DateTime at);
    }

    public class OptionRepository : IOptionRepository
    {
        private readonly IDatabase _db;
        private readonly ICalculatorClient _calculator;
        private readonly IContext<Branch> _branch;

        public OptionRepository(IDatabase db, ICalculatorClient calculatorClient, IContext<Branch> branch)
        {
            _db = db;
            _calculator = calculatorClient;
            _branch = branch;
        }

        public async Task<Option[]> GetOptions()
            => (await _calculator.GetOptions(_branch.Value)).Values.Values.ToArray();
        public async Task<Option?> GetOption(string optionId)
            => (await _calculator.GetOptions(_branch.Value)).Values.GetValueOrDefault(optionId);
        
        public async Task<OptionWorth[]> GetOptionWorths()
            => (await _calculator.GetOptionWorths(_branch.Value)).Worths.Values.ToArray();
        public async Task<OptionWorth?> GetOptionWorth(string optionId)
            => (await _calculator.GetOptionWorths(_branch.Value)).Worths.GetValueOrDefault(optionId);
        
        public Task<decimal> GetLoanableCash(string optionId, DateTime at)
            => _db.QueryFirst<decimal>(@"select ff.loanable_pre_enter_money(@opt, @at);", new
            {
                opt = optionId,
                at
            });
    }
}
