using System;
using System.Globalization;
using FfAdmin.Common;

namespace FfAdmin.External.GiveWp
{
    public class GiveExportRow
    {
        public string Donation_id { get; set; }
        public string Donation_number { get; set; }
        public decimal Donation_total { get; set; }
        public string Currency_code { get; set; }
        public string Donation_status { get; set; }
        public string Donation_date { get; set; }
        public string Donation_time { get; set; }
        public string Payment_gateway { get; set; }
        public string Payment_mode { get; set; }
        public string Payment_type { get; set; }
        public string Form_id { get; set; }
        public string Form_title { get; set; }
        public string Donor_id { get; set; }
        public string Fund_id { get; set; }
        public string Fund_title { get; set; }
        public DateTimeOffset GetTimestamp()
        {
            var date = DateTime.ParseExact(Donation_date, "dd-MM-yy", CultureInfo.InvariantCulture);
            var time = TimeSpan.Parse(Donation_time);
            return new DateTimeOffset(date + time).ToUniversalTime();
        }
        internal Event ToNewDonation() =>
            new NewDonation
            {
                Timestamp = GetTimestamp(),
                Donation = Donation_id,
                Donor = Donor_id,
                Charity = Form_id,
                Option = Fund_id,
                Currency = Currency_code,
                Amount = Donation_total
            };
    }
}
