using System;

namespace FfAdmin.ModelCache.Abstractions;

public static class Exts
{
    public static string ToHexString(this ReadOnlySpan<byte> bytes)
        => Convert.ToHexString(bytes);
    public static string ToHexString(this byte[] bytes)
        => Convert.ToHexString(bytes);
    public static string ToBase64String(this byte[] bytes)
        => Convert.ToBase64String(bytes);
    public static string ToSafeBase64String(this byte[] bytes)
        => bytes.ToBase64String().TrimEnd('=').Replace('+','-').Replace('/','_');
    public static byte[] ToByteArray(this string str)
        => Convert.FromHexString(str);
}
