using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FfAdmin.Common;

namespace FfAdmin.External.GiveWp
{
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public static class GiveExportRows
    {
        public static IEnumerable<GiveExportRow> FromCsv(string csv)
            => csv.ParseCsv<GiveExportRow>(CaseInsensitiveEqualityComparer.Instance);

        public static Event[] ToEvents(this IEnumerable<GiveExportRow> rows, IEnumerable<MollieExportRow> mollieRows, IEnumerable<string> charities, IEnumerable<string> options)
        {
            return EnumerateEvents().ToArray();
            IEnumerable<Event> EnumerateEvents()
            {
                rows = rows.ToList();
                var cs = new HashSet<string>(charities);
                var os = new HashSet<string>(options);
                var mollie = mollieRows.ToDictionary(m => m.Id);
                var messages = rows.Select((row, index) => row.Validate().Select(m => new ValidationMessage($"{index}.{m.Key}", m.Message))).SelectMany(x => x).ToArray();
                if (messages.Length > 0)
                    throw new ValidationException(messages);
                foreach (var row in rows.OrderBy(r => r.GetTimestamp()))
                {
                    if (!os.Contains(row.Fund_id!))
                    {
                        yield return new NewOption
                        {
                            Timestamp = (row.GetTimestamp() ?? throw new System.Exception("Invalid timestamp")).AddSeconds(-1),
                            Code = row.Fund_id!,
                            Currency = row.Currency_code ?? "EUR",
                            Name = row.Fund_title ?? "Unknown fund"
                        };
                        os.Add(row.Fund_id!);
                    }
                    if (!cs.Contains(row.Form_id!))
                    {
                        yield return new NewCharity
                        {
                            Timestamp = row.GetTimestamp()!.Value.AddSeconds(-1),
                            Code = row.Form_id!,
                            Name = row.Form_title ?? "Unknown charity"
                        };
                        cs.Add(row.Form_id!);
                    }
                    if(row.Donation_status == "Complete")
                        yield return row.ToNewDonation(row.Transaction_id == null
                            ? null
                            : mollie.GetValueOrDefault(row.Transaction_id))!;
                }
            }
        }
    }
}
