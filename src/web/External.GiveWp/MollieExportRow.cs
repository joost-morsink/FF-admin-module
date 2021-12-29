using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FfAdmin.Common;

namespace FfAdmin.External.GiveWp
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class MollieExportRow
    {
        public MollieExportRow(string id, string valuta, decimal bedrag, string uitbetalingsvaluta, decimal uitbetalingsbedrag, string uitbetalingsreferentie)
        {
            Id = id;
            Valuta = valuta;
            Bedrag = bedrag;
            Uitbetalingsvaluta = uitbetalingsvaluta;
            Uitbetalingsbedrag = uitbetalingsbedrag;
            Uitbetalingsreferentie = uitbetalingsreferentie;
        }

        public string Id { get; set; }
        public string Valuta { get; set; }
        public decimal Bedrag { get; set; }
        public string Uitbetalingsvaluta { get; set; }
        public decimal Uitbetalingsbedrag { get; set; }
        public string? Uitbetalingsreferentie { get; internal set; }

        public class Dto
        {
            public string? Id { get; set; }
            public string? Valuta { get; set; }
            public decimal? Bedrag { get; set; }
            public string? Uitbetalingsvaluta { get; set; }
            public decimal? Uitbetalingsbedrag { get; set; }
            public string? Uitbetalingsreferentie { get; set; }
            public MollieExportRow? Create()
            {
                if (string.IsNullOrWhiteSpace(Id)
                    || string.IsNullOrWhiteSpace(Valuta)
                    || string.IsNullOrWhiteSpace(Uitbetalingsvaluta)
                    || !Bedrag.HasValue
                    || !Uitbetalingsbedrag.HasValue)
                    return null;
                else
                    return new MollieExportRow(Id, Valuta, Bedrag.Value, Uitbetalingsvaluta, Uitbetalingsbedrag.Value, Uitbetalingsreferentie ?? "");
            }
        }
    }
    public static class MollieExportRows
    {
        public static IEnumerable<MollieExportRow> FromCsv(string csv)
            => csv.ParseCsv<MollieExportRow.Dto>(CaseInsensitiveEqualityComparer.Instance)
                .Select(r => r.Create())
                .SelectValues();
    }
}
