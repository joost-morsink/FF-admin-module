namespace FfAdmin.AdminModule
{
    public record AuditFinancial
    {
        public int Audit_id { get; set; }
        public string Currency { get; set; } = "";
        public decimal Donation_amount { get; set; }
        public decimal Unentered_donation_amount { get; set; }
        public decimal Invested_amount { get; set; }
        public decimal Cash_amount { get; set; }
        public decimal Allocated_amount { get; set; }
        public decimal Transferred_amount { get; set; }
    }
}
