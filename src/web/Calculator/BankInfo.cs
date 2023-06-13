namespace FfAdmin.Calculator;

public record BankInfo(string Name, string Account, string Bic)
{
    public static BankInfo Empty { get; } = new(string.Empty, string.Empty, string.Empty);
}
