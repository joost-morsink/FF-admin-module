using FfAdmin.Calculator.Core;

namespace FfAdmin.Calculator;

internal static class Validator
{
    internal static bool IsCharityKnown(this IContext context, string charity)
    {
        var charities = context.GetContext<Charities>();
        return charities.Contains(charity);
    }

    internal static bool AreCharitiesKnown(this IContext context, IEnumerable<string> charities)
    {
        var ctx = context.GetContext<Charities>();
        return charities.All(ctx.Contains);
    }
    
    internal static bool IsOptionKnown(this IContext context, string option)
    {
        var options = context.GetContext<Options>();
        return options.Contains(option);
    }
    
    internal static bool IsDonationKnown(this IContext context, string donation)
    {
        var donations = context.GetContext<Donations>();
        return donations.Contains(donation);
    }
}
