namespace FfAdmin.AdminModule
{
    public record CoreMessage
    {
        public string Key { get; set; } = "";
        public int Status { get; set; }
        public string Message { get; set; } = "";
    }
}
