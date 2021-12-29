using System;
using System.Diagnostics.CodeAnalysis;

namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public record AuditFinancial
    {
        public int Audit_id { get; set; }
        public string Currency { get; set; } = "";
        public decimal Donation_amount { get; set; }
        public decimal Cancelled_donation_amount { get; set; }
        public decimal Unentered_donation_amount { get; set; }
        public decimal Invested_amount { get; set; }
        public decimal Cash_amount { get; set; }
        public decimal Allocated_amount { get; set; }
        public decimal Transferred_amount { get; set; }
        public AuditTransfers[] Transfers { get; set; } = Array.Empty<AuditTransfers>();
    }
}
