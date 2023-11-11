using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Core;
using Calculator.ApiClient;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.External.Banking;
using FfAdminWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/charities")]
    public class CharityController : Controller
    {
        private readonly ICharityRepository _repository;
        private readonly IEventRepository _eventRepository;

        public CharityController(ICharityRepository repository, IEventRepository eventRepository)
        {
            _repository = repository;
            _eventRepository = eventRepository;
        }

        public class CharityGridRow
        {
            public string Code { get; init; } = "";
            public string Name { get; init; } = "";
            public string? Bank_name { get; init; }
            public string? Bank_account_no { get; init; }
            public string? Bank_bic { get; init; }

            public static CharityGridRow Create(FfAdmin.Calculator.Charity c)
                => new ()
                {
                    Code = c.Id,
                    Name = c.Name,
                    Bank_name = c.Bank.Name,
                    Bank_account_no = c.Bank.Account,
                    Bank_bic = c.Bank.Bic
                };
        }

        public class OpenTransferGridRow
        {
            public string Charity { get; init; } = "";
            public string Name { get; init; } = "";
            public string Currency { get; init; } = "";
            public decimal Amount { get; init; }

            public static OpenTransferGridRow Create(OpenTransfer ot)
                => new ()
                {
                    Charity = ot.Charity_id, Name = ot.Name, Currency = ot.Currency, Amount = ot.Amount
                };
        }

        [HttpGet]
        public async Task<IEnumerable<CharityGridRow>> GetCharities()
        {
            var res = await _repository.GetCharities();
            return res.Select(CharityGridRow.Create);
        }

        [HttpGet("{id}/partitions")]
        public async Task<IEnumerable<FractionSpec>> GetPartitions(string id)
        {
            var res = await _repository.GetPartitions(id);
            return res.Select(kvp => new FractionSpec
            {
                Holder = kvp.Key,
                Fraction = kvp.Value
            });    
        }

        [HttpGet("opentransfers")]
        public async Task<IEnumerable<OpenTransferGridRow>> GetOpenTransfers()
        {
            var res = await _repository.GetOpenTransfers();
            return res.Select(OpenTransferGridRow.Create);
        }

        [HttpPost("opentransfers/resolve/camt")]
        public async Task<IActionResult> ResolveOpenTransfersInCamt()
        {
 #pragma warning disable CA1826
            var file = Request.Form.Files.FirstOrDefault();
 #pragma warning restore CA1826
            if (file == null)
                return BadRequest(new ValidationMessage[]
                {
                    new("", "No file uploaded")
                });
            var content = await file.ReadFormFile();
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(new ValidationMessage[]
                {
                    new("", "File is empty")
                });
            var xml = XElement.Parse(content).RemoveNamespaces();

            var payments = await GetConvTransfers(xml);

            await _eventRepository.Import(payments);

            return Ok();
        }

        private async Task<IEnumerable<ConvTransfer>> GetConvTransfers(XElement xml)
        {
            var charities =
                (await _repository.GetCharities()).Where(c => !string.IsNullOrWhiteSpace(c.Bank.Account));
            var entries = xml.GetCamtEntries();
            var payments = from c in charities
                           join e in entries
                               on c.Bank.Account equals e.Recipient
                           let amt = e.Amount
                           let dt = e.Booking
                           where amt.HasValue && dt.HasValue
                           select new ConvTransfer
                           {
                               Charity = c.Id,
                               Currency = e.Currency ?? "NO CURRENCY",
                               Amount = amt.Value,
                               Transaction_reference = e.Reference ?? "",
                               Timestamp = dt.Value
                           };
            return payments;
        }

        [HttpGet("opentransfers/pain")]
        public async Task<IActionResult> GetOpenTransfersInPain(string currency, decimal cutoff)
        {
            var res = (await _repository.GetOpenTransfers()).Where(t => t.Amount >= cutoff && t.Currency == currency);
            var charities = await _repository.GetCharities();
            var xml = res.GetPain(charities.Select(Convert));

            return File(Encoding.UTF8.GetBytes(xml.ToString()), "application/xml");
        }

        private static Charity Convert(FfAdmin.Calculator.Charity charity)
            => new()
            {
                Charity_id = charity.Id,
                Name = charity.Name,
                Bank_account_no = charity.Bank.Account,
                Bank_bic = charity.Bank.Bic,
                Bank_name = charity.Bank.Name
            };
    }
}
