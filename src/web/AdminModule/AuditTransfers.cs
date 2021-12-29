using System.Diagnostics.CodeAnalysis;
namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public record AuditTransfers
    {
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Original_amount { get; set; }
    }
}
