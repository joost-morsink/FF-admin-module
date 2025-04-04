using System;
using System.Threading.Tasks;

namespace FfAdmin.ExchangeRate;

public interface IExchangeRateService
{
    Task<ExchangeRate?> GetExchangeRate(string from, string to, DateOnly date);
}