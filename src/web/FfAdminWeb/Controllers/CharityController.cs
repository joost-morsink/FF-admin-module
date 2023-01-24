using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.External.Banking;
using FfAdminWeb.Services;
using FfAdminWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/charities")]
    public class CharityController : Controller
    {
        private readonly ICharityRepository _repository;
        private readonly IEventingSystem _eventingSystem;

        public CharityController(ICharityRepository repository, IEventingSystem eventingSystem)
        {
            _repository = repository;
            _eventingSystem = eventingSystem;
        }

        public class CharityGridRow
        {
            public int Id { get; init; }
            public string Code { get; init; } = "";
            public string Name { get; init; } = "";
            public string? Bank_name { get; init; }
            public string? Bank_account_no { get; init; }
            public string? Bank_bic { get; init; }

            public static CharityGridRow Create(Charity c)
                => new ()
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
            public string Charity { get; init; } = "";
            public string Name { get; init; } = "";
            public string Currency { get; init; } = "";
            public decimal Amount { get; init; }

            public static OpenTransferGridRow Create(OpenTransfer ot)
                => new ()
                {
                    Charity = ot.Charity_ext_id, Name = ot.Name, Currency = ot.Currency, Amount = ot.Amount
                };
        }

        [HttpGet]
        public async Task<IEnumerable<CharityGridRow>> GetCharities()
        {
            var res = await _repository.GetCharities();
            return res.Select(CharityGridRow.Create);
        }

        [HttpGet("{id}/partitions")]
        public async Task<IEnumerable<FractionSpec>> GetPartitions(int id)
        {
            var res = await _repository.GetPartitions(id);
            return res;    
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

            if (!_eventingSystem.HasSession)
                return BadRequest(new ValidationMessage[]
                {
                    new("main", "No session")
                });

            var payments = await GetConvTransfers(xml);

            await _eventingSystem.ImportEvents(payments);

            return Ok();
        }

        private async Task<IEnumerable<ConvTransfer>> GetConvTransfers(XElement xml)
        {
            var charities =
                (await _repository.GetCharities()).Where(c => !string.IsNullOrWhiteSpace(c.Bank_account_no));
            var entries = xml.GetCamtEntries();
            var payments = from c in charities
                           join e in entries
                               on c.Bank_account_no equals e.Recipient
                           let amt = e.Amount
                           let dt = e.Booking
                           where amt.HasValue && dt.HasValue
                           select new ConvTransfer
                           {
                               Charity = c.Charity_ext_id,
                               Currency = e.Currency ?? "EUR",
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
            var xml = res.GetPain(charities);

            return File(Encoding.UTF8.GetBytes(xml.ToString()), "application/xml");
        }
    }
}
