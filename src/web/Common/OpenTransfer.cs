using System.Diagnostics.CodeAnalysis;
namespace FfAdmin.Common
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class OpenTransfer {
        public int Charity_id { get; set; }
        public string Charity_ext_id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; }
    }
}
