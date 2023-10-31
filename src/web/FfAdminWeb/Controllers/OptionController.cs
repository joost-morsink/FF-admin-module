using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Calculator;
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
            public string Id { get; set; } = "";
            public string? Name { get; set; }
            public string? Currency { get; set; }
            public decimal Reinvestment_fraction { get; set; }
            public decimal FutureFund_fraction { get; set; }
            public decimal Charity_fraction { get; set; }
            public decimal Bad_year_fraction { get; set; }
            public decimal Cash_amount { get; set; }
            public decimal Invested_amount { get; set; }

            public static OptionGridRow Create(Option o, OptionWorth w)
                => new()
                {
                    Id = o.Id,
                    Name = o.Name,
                    Currency = o.Currency,
                    Reinvestment_fraction = o.ReinvestmentFraction,
                    FutureFund_fraction = o.G4gFraction,
                    Charity_fraction = o.CharityFraction,
                    Bad_year_fraction = o.BadYearFraction,
                    Cash_amount = w.Cash,
                    Invested_amount = w.Invested
                };
        }

        [HttpGet]
        public async Task<IEnumerable<OptionGridRow>> GetOptions()
        {
            return from o in await _repository.GetOptions()
                    join w in await _repository.GetOptionWorths() on o.Id equals w.Id
                    select OptionGridRow.Create(o, w);
        }

        [HttpGet("{optionId}")]
        public async Task<ActionResult<OptionGridRow>> GetOption(string optionId)
        {
            var o = await _repository.GetOption(optionId);
            var w = await _repository.GetOptionWorth(optionId);
            if (o is null || w is null)
                return new NotFoundResult();
            return OptionGridRow.Create(o, w);
        }

        [HttpGet("{optionId}/loanable-cash")]
        public async Task<decimal> GetLoanableCash(string optionId, DateTime at)
            => await _repository.GetLoanableCash(optionId, at);
    }
}
