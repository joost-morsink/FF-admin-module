namespace FfAdmin.Common
{
    public class Charity
    {
        public int Charity_id { get; set; }
        public string Charity_ext_id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Bank_name { get; set; }
        public string? Bank_account_no { get; set; }
        public string? Bank_bic { get; set; }
    }
}