using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FfAdmin.AdminModule
{
    public interface IOptionRepository
    {
        Task<Option[]> GetOptions();
        Task<Option> GetOption(int optionId);
        Task<decimal> GetLoanableCash(int optionId, DateTime at);
    }
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class Option
    {
        public int Option_id { get; set; }
        public string Option_ext_id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Reinvestment_fraction { get; set; }
        public decimal FutureFund_fraction { get; set; }
        public decimal Charity_fraction { get; set; }
        public decimal Bad_year_fraction { get; set; }

        public decimal Invested_amount { get; set; }
        public decimal Cash_amount { get; set; }

        public DateTime Last_exit { get; set; }
        public decimal Exit_actual_valuation { get; set; }
        public decimal Exit_ideal_valuation { get; set; }
    }
    public class OptionRepository : IOptionRepository
    {
        private readonly IDatabase _db;

        public OptionRepository(IDatabase db)
        {
            _db = db;
        }

        public Task<Option[]> GetOptions()
            => _db.Query<Option>(@"select * from ff.option;");
        public Task<Option> GetOption(int optionId)
            => _db.QueryFirst<Option>(@"select * from ff.option where option_id = @opt;", new
            {
                opt = optionId
            });
        public Task<decimal> GetLoanableCash(int optionId, DateTime at)
            => _db.QueryFirst<decimal>(@"select ff.loanable_pre_enter_money(@opt, @at);", new
            {
                opt = optionId,
                at
            });
    }
}
