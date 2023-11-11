using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FfAdmin.Common;

namespace FfAdmin.External.Banking
{
    public static class Pain
    {
        private static readonly XNamespace Ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.09";

        public static XElement GetPain(this IEnumerable<OpenTransfer> transfers, IEnumerable<Charity> charities)
        {
            var res = transfers.ToArray();
            foreach (var t in res)
                t.Amount = Math.Floor(t.Amount * 100) / 100;

            var exDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var msgId = Guid.NewGuid().ToString().Replace("-", "");
            var xml = new XElement(Ns + "Document",
                new XElement(Ns + "CstmrCdtTrfInitn",
                    new XElement(Ns + "GrpHdr",
                        new XElement(Ns + "MsgId", msgId),
                        new XElement(Ns + "CreDtTm", DateTime.Now),
                        new XElement(Ns + "NbOfTxs", res.Length),
                        new XElement(Ns + "CtrlSum", res.Sum(x => x.Amount)),
                        new XElement(Ns + "InitgPty",
                            new XElement(Ns + "Nm", "Give for Good")),
                        new XElement(Ns + "PmtInf",
                            new XElement(Ns + "PmtInfId", 1),
                            new XElement(Ns + "PmtMtd", "TRF"),
                            new XElement(Ns + "NbOfTxs", res.Length),
                            new XElement(Ns + "CtrlSum", res.Sum(x => x.Amount)),
                            new XElement(Ns + "PmtTpInf",
                                new XElement(Ns + "SvcLvl",
                                    new XElement(Ns + "Cd", "SEPA"))),
                            new XElement(Ns + "ReqExctnDt", exDate),
                            new XElement(Ns + "Dbtr",
                                new XElement(Ns + "Nm", "Give for good")),
                            new XElement(Ns + "DbtrAcct",
                                new XElement(Ns + "Id",
                                    new XElement(Ns + "IBAN", "NLxxABNAxxxxxxxxxx"))),
                            new XElement(Ns + "DbtrAgt",
                                new XElement(Ns + "FinInstnId",
                                    new XElement(Ns + "BIC", "ABNANL2A"))),
                            new XElement(Ns + "ChrgBr", "SLEV"),
                            from ot in res
                            join ch in charities on ot.Charity_id equals ch.Charity_id.ToString()
                            select MakePaymentInfo(ot, exDate, ch)))));
            return xml;
        }
        private static XElement MakePaymentInfo(OpenTransfer t, string exDate, Charity charity)
        {
            return new XElement(Ns + "CdtTrfTxInf",
                new XElement(Ns + "PmtId",
                    new XElement(Ns + "EndToEndId", $"Onze referentie: {exDate.Replace("-", "")}{charity.Charity_id}")),
                new XElement(Ns + "Amt",
                    new XElement(Ns + "InstdAmt", new XAttribute("Ccy", t.Currency), t.Amount)),
                new XElement(Ns + "Cdtr",
                    new XElement(Ns + "Nm", charity.Bank_name)),
                new XElement(Ns + "CdtrAcct",
                    new XElement(Ns + "Id",
                        new XElement(Ns + "IBAN", charity.Bank_account_no))),
                new XElement(Ns + "RmtInf",
                    new XElement(Ns + "Ustrd", $"Ref. {exDate.Replace("-", "")}{charity.Charity_id}")));
        }
    }
}
