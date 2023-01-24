using System.Collections.Generic;

namespace FfAdmin.Common;

public class FractionSpec
{
    public string Holder { get; set; } = "";
    public decimal Fraction { get; set; } = 1m;

    public IEnumerable<ValidationMessage> Validate()
    {
        if (string.IsNullOrWhiteSpace(Holder))
            yield return new ValidationMessage(nameof(Holder), "Holder is required.");
        if (Fraction <= 0m)
            yield return new ValidationMessage(nameof(Fraction), "Fraction should be positive.");
    }
}
