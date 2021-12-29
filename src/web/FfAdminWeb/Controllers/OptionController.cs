using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/options")]
    public class OptionController
    {
        private readonly IOptionRepository _repository;

        public OptionController(IOptionRepository repository)
        {
            _repository = repository;
        }
        public class OptionGridRow
        {
            public int Id { get; set; }
            public string? Code { get; set; }
            public string? Name { get; set; }
            public string? Currency { get; set; }
            public decimal Reinvestment_fraction { get; set; }
            public decimal FutureFund_fraction { get; set; }
            public decimal Charity_fraction { get; set; }
            public decimal Bad_year_fraction { get; set; }
            public decimal Cash_amount { get; set; }
            public decimal Invested_amount { get; set; }

            public static OptionGridRow Create(Option o)
                => new()
                {
                    Id = o.Option_id,
                    Code = o.Option_ext_id,
                    Name = o.Name,
                    Currency = o.Currency,
                    Reinvestment_fraction = o.Reinvestment_fraction,
                    FutureFund_fraction = o.FutureFund_fraction,
                    Charity_fraction = o.Charity_fraction,
                    Bad_year_fraction = o.Bad_year_fraction,
                    Cash_amount = o.Cash_amount,
                    Invested_amount = o.Invested_amount
                };
        }

        [HttpGet]
        public async Task<IEnumerable<OptionGridRow>> GetOptions()
            => (await _repository.GetOptions()).Select(OptionGridRow.Create);
        [HttpGet("{optionId:int}")]
        public async Task<OptionGridRow> GetOption(int optionId)
            => OptionGridRow.Create(await _repository.GetOption(optionId));
    }
}
