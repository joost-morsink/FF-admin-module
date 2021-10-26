namespace FfAdmin.AdminModule
{
    public record Audit
    {
        public int Audit_id { get; set; }
        public string Hashcode { get; set; } = "";
        public int Num_events { get; set; }
        public int Num_unprocessed_events { get; set; }
        public int Num_donations { get; set; }
        public int Num_charities { get; set; }
        public int Num_donors { get; set; }
    }
}
