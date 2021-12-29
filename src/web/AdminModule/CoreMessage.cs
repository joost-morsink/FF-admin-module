using System.Diagnostics.CodeAnalysis;
namespace FfAdmin.AdminModule
{
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public record CoreMessage
    {
        public string Key { get; set; } = "";
        public int Status { get; set; }
        public string Message { get; set; } = "";
    }
}
