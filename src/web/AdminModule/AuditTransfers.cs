namespace FfAdmin.AdminModule
{
    public record AuditTransfers
    {
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Original_amount { get; set; } 
    }
}
