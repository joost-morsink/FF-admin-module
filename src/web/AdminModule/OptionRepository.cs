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

        Task<OptionWorths2.Header[]> GetOptionWorths();

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

        public async Task<OptionWorths2.Header[]> GetOptionWorths()
            => await _calculator.GetOptionWorths(_branch.Value);
        public async Task<OptionWorths2.Header?> GetOptionWorth(string optionId)
            => (await _calculator.GetOptionWorths(_branch.Value)).FirstOrDefault(x=> x.Id == optionId);

        public async Task<decimal> GetLoanableCash(string optionId, DateTime at)
        {
            if ((await _calculator.GetOptionWorths(_branch.Value)).FirstOrDefault(x => x.Id == optionId) is not {} option)
                return 0;
            return option.UnenteredDonations.Where(d => d.ExecuteTimestamp <= at).Sum(d => d.Amount);
        }
    }
}
