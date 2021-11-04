using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FfAdmin.Common;

namespace FfAdmin.External.GiveWp
{
    public class GiveExportRow
    {
        public string? Donation_id { get; set; }
        public string? Donation_number { get; set; }
        public decimal Donation_total { get; set; }
        public string? Currency_code { get; set; }
        public string? Donation_status { get; set; }
        public string? Donation_date { get; set; }
        public string? Donation_time { get; set; }
        public string? Payment_gateway { get; set; }
        public string? Payment_mode { get; set; }
        public string? Payment_type { get; set; }
        public string? Form_id { get; set; }
        public string? Form_title { get; set; }
        public string? Donor_id { get; set; }
        public string? Fund_id { get; set; }
        public string? Fund_title { get; set; }
        public DateTimeOffset? GetTimestamp()
        {
            if (Donation_date == null || Donation_time == null)
                return null;
            var date = DateTime.ParseExact(Donation_date, "dd-MM-yy", CultureInfo.InvariantCulture);
            var time = TimeSpan.Parse(Donation_time);
            return new DateTimeOffset(date + time).ToUniversalTime();
        }
        public IEnumerable<ValidationMessage> Validate()
        {
            if (string.IsNullOrWhiteSpace(Donation_date))
                yield return new ValidationMessage(nameof(Donation_date), "Field is required");
            if (string.IsNullOrWhiteSpace(Donation_time))
                yield return new ValidationMessage(nameof(Donation_time), "Field is required");
            if (string.IsNullOrWhiteSpace(Donation_id))
                yield return new ValidationMessage(nameof(Donation_id), "Field is required");
            if (string.IsNullOrWhiteSpace(Donor_id))
                yield return new ValidationMessage(nameof(Donor_id), "Field is required");
            if (string.IsNullOrWhiteSpace(Form_id))
                yield return new ValidationMessage(nameof(Form_id), "Field is required");
            if (string.IsNullOrWhiteSpace(Fund_id))
                yield return new ValidationMessage(nameof(Fund_id), "Field is required");
            if (string.IsNullOrWhiteSpace(Currency_code))
                yield return new ValidationMessage(nameof(Currency_code), "Field is required");
        }

        private bool IsDataValid() => !Validate().Any();

        internal Event? ToNewDonation()
            => IsDataValid()
                ? new NewDonation
                {
                    Timestamp = GetTimestamp()!.Value,
                    Execute_timestamp = GetTimestamp()!.Value.AddDays(56),
                    Donation = Donation_id!,
                    Donor = Donor_id!,
                    Charity = Form_id!,
                    Option = Fund_id!,
                    Currency = Currency_code!,
                    Amount = Donation_total
                }
                : null;
    }
}
