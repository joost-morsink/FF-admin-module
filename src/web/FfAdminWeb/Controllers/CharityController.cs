using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.EventStore;
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
        [HttpPost("opentransfers/resolve/camt")]
        public async Task<IActionResult> ResolveOpenTransfersInCamt()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
                return BadRequest(new ValidationMessage[] { new("", "No file uploaded") });
            var content = await file.ReadFormFile();
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(new ValidationMessage[] { new("", "File is empty") });
            var xml = XElement.Parse(content).RemoveNamespaces();

            if (!_eventingSystem.HasSession)
                return BadRequest(new ValidationMessage[] {
                    new("main","No session")
                });

            var payments = await GetConvTransfers(xml);

            await _eventingSystem.ImportEvents(payments);

            return Ok();
        }

        private async Task<IEnumerable<ConvTransfer>> GetConvTransfers(XElement xml)
        {
            var charities = (await _repository.GetCharities()).Where(c => !string.IsNullOrWhiteSpace(c.Bank_account_no));
            var entries = from entry in xml.Elements("BkToCstmrStmt").Elements("Stmt").Elements("Ntry")
                where entry.Element("CdtDbtInd")?.Value == "DBIT"
                from tx in entry.Elements("NtryDtls").Elements("TxDtls").Take(1)
                select new
                {
                    Currency = tx.Elements("AmtDtls").Elements("TxAmt").Elements("Amt").Select(a => a.Attribute("Ccy")?.Value)
                        .FirstOrDefault(),
                    Amount = tx.Elements("AmtDtls").Elements("TxAmt").Elements("Amt").Select(a => a.Value).FirstOrDefault(),
                    Recipient = tx.Elements("RltdPties").Elements("CdtrAcct").Elements("Id").Elements("IBAN")
                        .Select(i => i.Value).FirstOrDefault(),
                    Reference = tx.Elements("RmtInf").Elements("Ustrd").Select(t => t.Value).FirstOrDefault(),
                    Booking = entry.Elements("BookgDt").Elements("Dt").Select(d => d.Value).FirstOrDefault()
                };

            var payments = from c in charities
                join e in entries
                    on c.Bank_account_no equals e.Recipient
                let amt = decimal.TryParse(e.Amount, out var r) ? r : -1m
                let dt = DateTime.TryParseExact(e.Booking, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var d)
                    ? d
                    : DateTime.MinValue
                where amt > 0m && dt > DateTime.MinValue
                select new ConvTransfer
                {
                    Charity = c.Charity_ext_id,
                    Currency = e.Currency,
                    Amount = decimal.Parse(e.Amount),
                    Transaction_reference = e.Reference,
                    Timestamp = dt
                };
            return payments;
        }

        [HttpGet("opentransfers/pain")]
        public async Task<IActionResult> GetOpenTransfersInPain(string currency, decimal cutoff)
        {
            var n = 0;
            var res = (await _repository.GetOpenTransfers()).Where(t => t.Amount >= cutoff && t.Currency == currency).ToArray();
            foreach (var t in res)
                t.Amount = Math.Floor(t.Amount * 100) / 100;

            var charities = await _repository.GetCharities();
            string exDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            XNamespace NS = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.09";
            var msgId = Guid.NewGuid().ToString().Replace("-", "");
            var xml = new XElement(NS + "Document",
                new XElement(NS + "CstmrCdtTrfInitn",
                    new XElement(NS + "GrpHdr",
                        new XElement(NS + "MsgId", msgId),
                        new XElement(NS + "CreDtTm", DateTime.Now),
                        new XElement(NS + "NbOfTxs", res.Length),
                        new XElement(NS + "CtrlSum", res.Sum(x => x.Amount)),
                        new XElement(NS + "InitgPty",
                            new XElement(NS + "Nm", "Give for Good")),
                        new XElement(NS + "PmtInf",
                            new XElement(NS + "PmtInfId", n++),
                            new XElement(NS + "PmtMtd", "TRF"),
                            new XElement(NS + "NbOfTxs", res.Length),
                            new XElement(NS + "CtrlSum", res.Sum(x => x.Amount)),
                            new XElement(NS + "PmtTpInf",
                                new XElement(NS + "SvcLvl",
                                    new XElement(NS + "Cd", "SEPA"))),
                            new XElement(NS + "ReqExctnDt", exDate),
                            new XElement(NS + "Dbtr",
                                new XElement(NS + "Nm", "Give for good")),
                            new XElement(NS + "DbtrAcct",
                                new XElement(NS + "Id",
                                    new XElement(NS + "IBAN", "NLxxABNAxxxxxxxxxx"))),
                            new XElement(NS + "DbtrAgt",
                                new XElement(NS + "FinInstnId",
                                    new XElement(NS + "BIC", "ABNANL2A"))),
                            new XElement(NS + "ChrgBr", "SLEV"),
                                from ot in res
                                join ch in charities on ot.Charity_id equals ch.Charity_id
                                select MakePaymentInfo(ot, ch)))));
            return File(Encoding.UTF8.GetBytes(xml.ToString()), "application/xml");
            XElement MakePaymentInfo(OpenTransfer t, Charity charity)
            {
                return new XElement(NS + "CdtTrfTxInf",
                    new XElement(NS + "PmtId",
                        new XElement(NS + "EndToEndId", $"Onze referentie: {exDate.Replace("-", "")}{charity.Charity_ext_id}")),
                    new XElement(NS + "Amt",
                        new XElement(NS + "InstdAmt", new XAttribute("Ccy", t.Currency), t.Amount)),
                    new XElement(NS + "Cdtr",
                        new XElement(NS + "Nm", charity.Bank_name)),
                    new XElement(NS + "CdtrAcct",
                        new XElement(NS + "Id",
                            new XElement(NS + "IBAN", charity.Bank_account_no))),
                    new XElement(NS + "RmtInf",
                        new XElement(NS + "Ustrd", $"Ref. {exDate.Replace("-", "")}{charity.Charity_ext_id}")));
            }
        }
    }
}
