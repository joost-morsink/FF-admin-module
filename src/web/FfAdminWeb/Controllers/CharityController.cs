using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/charities")]
    public class CharityController : Controller
    {
        private readonly ICharityRepository _repository;

        public CharityController(ICharityRepository repository)
        {
            _repository = repository;
        }
        public class CharityGridRow
        {
            public int Id { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Bank_name { get; set; }
            public string? Bank_account_no { get; set; }
            public string? Bank_bic { get; set; }
            public static CharityGridRow Create(Charity c)
                => new CharityGridRow
                {
                    Id = c.Charity_id,
                    Code = c.Charity_ext_id,
                    Name = c.Name,
                    Bank_name = c.Bank_name,
                    Bank_account_no = c.Bank_account_no,
                    Bank_bic = c.Bank_bic
                };
        }
        public class OpenTransferGridRow
        {
            public string Charity { get; set; } = "";
            public string Name { get; set; } = "";
            public string Currency { get; set; } = "";
            public decimal Amount { get; set; }
            public static OpenTransferGridRow Create(OpenTransfer ot)
                => new OpenTransferGridRow
                {
                    Charity = ot.Charity_ext_id,
                    Name = ot.Name,
                    Currency = ot.Currency,
                    Amount = ot.Amount
                };
        }

        [HttpGet]
        public async Task<IEnumerable<CharityGridRow>> GetCharities()
        {
            var res = await _repository.GetCharities();
            return res.Select(CharityGridRow.Create);
        }
        [HttpGet("opentransfers")]
        public async Task<IEnumerable<OpenTransferGridRow>> GetOpenTransfers()
        {
            var res = await _repository.GetOpenTransfers();
            return res.Select(OpenTransferGridRow.Create);
        }
    }
}
