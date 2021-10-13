using System.Collections.Generic;
using System.Linq;
using FfAdmin.Common;

namespace FfAdmin.External.GiveWp
{
    public static class GiveExportRows
    {
        public static IEnumerable<GiveExportRow> FromCsv(string csv)
            => csv.ParseCsv<GiveExportRow>(CaseInsensitiveEqualityComparer.Instance);

        public static Event[] ToEvents(this IEnumerable<GiveExportRow> rows, IEnumerable<string> charities, IEnumerable<string> options)
        {
            return EnumerateEvents().ToArray();
            IEnumerable<Event> EnumerateEvents()
            {
                var cs = new HashSet<string>(charities);
                var os = new HashSet<string>(options);
                foreach (var row in rows.OrderBy(r => r.GetTimestamp()))
                {
                    if (!os.Contains(row.Fund_id))
                    {
                        yield return new NewOption
                        {
                            Timestamp = row.GetTimestamp().AddSeconds(-1),
                            Code = row.Fund_id,
                            Currency = row.Currency_code,
                            Name = row.Fund_title
                        };
                        os.Add(row.Fund_id);
                    }
                    if (!cs.Contains(row.Form_id))
                    {
                        yield return new NewCharity
                        {
                            Timestamp = row.GetTimestamp().AddSeconds(-1),
                            Code = row.Form_id,
                            Name = row.Form_title
                        };
                        cs.Add(row.Form_id);
                    }
                    if(row.Donation_status == "Complete")
                        yield return row.ToNewDonation();
                }
            }
        }
    }
}
