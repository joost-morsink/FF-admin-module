using System;
using System.Diagnostics.CodeAnalysis;

namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public record Audit
    {
        public int Audit_id { get; set; }
        public string Hashcode { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UnixEpoch;
        public int Num_events { get; set; }
        public int Num_processed_events { get; set; }
        public int Num_donations { get; set; }
        public int Num_charities { get; set; }
        public int Num_donors { get; set; }
    }
}
