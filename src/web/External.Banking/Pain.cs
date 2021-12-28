using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FfAdmin.Common;

namespace FfAdmin.External.Banking
{
    public static class Pain
    {
        private static readonly XNamespace NS = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.09";

        public static XElement GetPain(this IEnumerable<OpenTransfer> transfers, IEnumerable<Charity> charities)
        {
            var n = 0;
            var res = transfers.ToArray();
            foreach (var t in res)
                t.Amount = Math.Floor(t.Amount * 100) / 100;

            string exDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
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
                                select MakePaymentInfo(ot, exDate, ch)))));
            return xml;
        }
        private static XElement MakePaymentInfo(OpenTransfer t, string exDate, Charity charity)
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