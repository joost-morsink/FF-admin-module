using System.Collections.Generic;
using System.Linq;

namespace FfAdmin.AdminModule;

public static class Ext
{
    public static Common.ValidationMessage ToMessage(this Calculator.ValidationError error, int subtract = 0)
        => new((error.Position - subtract).ToString(), error.Message);

    public static IEnumerable<Common.ValidationMessage> ToMessages(this IEnumerable<Calculator.ValidationError> errors,
        int subtract = 0)
        => errors.Select(e => e.ToMessage(subtract));
}
