using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        private readonly ICalculatorClient _calculator;
        private readonly IContext<Branch> _branch;

        public OptionRepository(ICalculatorClient calculatorClient, IContext<Branch> branch)
        {
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

        public async Task<decimal> GetLoanableCash(string optionId, DateTime at)
        {
            if (!(await _calculator.GetOptionWorths(_branch.Value)).Worths.TryGetValue(optionId, out var option))
                return 0;
            return option.UnenteredDonations.Where(d => d.ExecuteTimestamp <= at).Sum(d => d.Amount);
        }
    }
}
