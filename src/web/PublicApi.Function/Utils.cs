using System.Diagnostics.CodeAnalysis;

namespace FfAdmin.PublicApi.Function;

internal static class Utils
{
    public static Stream ToStream(this string str)
    {
        var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, leaveOpen: true);
        writer.Write(str);
        writer.Flush();
        ms.Position = 0;
        return ms;
    }

    public static Stream ToJsonStream(this object o)
    {
        var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, leaveOpen: true);
        writer.Write(System.Text.Json.JsonSerializer.Serialize(o));
        writer.Flush();
        ms.Position = 0;
        return ms;
    }

    public static DateOnly ToDateOnly(this DateTimeOffset dt)
    {
        var normalized = dt.ToUniversalTime();
        return new(dt.Year, dt.Month, dt.Day);
    }
}
