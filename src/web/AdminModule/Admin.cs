using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Calculator.ApiClient;
using FfAdmin.Common;

namespace FfAdmin.AdminModule
{
    public interface IAdmin
    {
        Task<decimal> CalculateExit(string optionId, decimal extracash, decimal currentInvested, DateTimeOffset timestamp);

    }
    public class Admin : IAdmin
    {
        private readonly ICalculatorClient _calculator;
        private readonly IContext<Branch> _branch;

        public Admin(ICalculatorClient calculator, IContext<Branch> branch)
        {
            _calculator = calculator;
            _branch = branch;
        }

        public async Task<decimal> CalculateExit(string optionId, decimal extracash, decimal currentInvested, DateTimeOffset timestamp)
        {
            var theory = new Event[]
            {
                new PriceInfo
                {
                    Invested_amount = currentInvested + extracash,
                    Option = optionId,
                    Timestamp = timestamp.ToUniversalTime().DateTime
                }
            };
            if (!(await _calculator.GetIdealOptionValuations(_branch.Value, theory: theory)).Valuations.TryGetValue(
                    optionId, out var ideal)
                || !(await _calculator.GetMinimalExits(_branch.Value, theory:theory)).Exits.TryGetValue(
                    optionId, out var exit))
                return 0m;
            return Math.Max(ideal.RealValue - ideal.IdealValue, exit);
        }
    }
}
