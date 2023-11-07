using FfAdmin.Calculator;
using FfAdmin.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calculator.ApiClient;

public interface ICalculatorClient
{
    Task<AmountsToTransfer> GetAmountsToTransfer(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<Charities> GetCharities(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<CharityBalance> GetCharityBalance(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<CharityFractionSetsForOption> GetCharityFractionSetsForOption(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<CumulativeInterest> GetCumulativeInterest(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<Donations> GetDonations(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<DonationRecords> GetDonationRecords(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<HistoryHash> GetHistoryHash(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<IdealOptionValuations> GetIdealOptionValuations(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<MinimalExits> GetMinimalExits(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<Options> GetOptions(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<OptionWorths> GetOptionWorths(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<ValidationErrors> GetValidationErrors(string branch, int? at = null, IEnumerable<Event>? theory = null);
    Task<DonationStatistics> GetDonationStatistics(string branch, int? at = null, IEnumerable<Event>? theory = null);
}
