using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace FfAdmin.External.Banking
{
    public static class Camt
    {
        public class Entry
        {
            public string? Currency { get; set; }
            public decimal? Amount { get; set; }
            public string? Recipient { get; set; }
            public string? Reference { get; set; }
            public DateTime? Booking { get; set; }
        }

        public static IEnumerable<Entry> GetCamtEntries(this XElement xml)
        {
            return from entry in xml.Elements("BkToCstmrStmt").Elements("Stmt").Elements("Ntry")
                where entry.Element("CdtDbtInd")?.Value == "DBIT"
                from tx in entry.Elements("NtryDtls").Elements("TxDtls").Take(1)
                select new Entry
                {
                    Currency = tx.Elements("AmtDtls").Elements("TxAmt").Elements("Amt")
                        .Select(a => a.Attribute("Ccy")?.Value)
                        .FirstOrDefault(),
                    Amount = tx.Elements("AmtDtls").Elements("TxAmt").Elements("Amt").Select(a => a.Value)
                        .FirstOrDefault().ToDecimal(),
                    Recipient = tx.Elements("RltdPties").Elements("CdtrAcct").Elements("Id").Elements("IBAN")
                        .Select(i => i.Value).FirstOrDefault(),
                    Reference = tx.Elements("RmtInf").Elements("Ustrd").Select(t => t.Value).FirstOrDefault(),
                    Booking = entry.Elements("BookgDt").Elements("Dt").Select(d => d.Value).FirstOrDefault().ToDate()
                };
        }

        private static decimal? ToDecimal(this string? str)
            => str == null ? null
                : decimal.TryParse(str, out var res) ? res : null;
        private static DateTime? ToDate(this string? str)
            => str == null ? null
                : DateTime.TryParseExact(str, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var res) ? res : null;
    }
}