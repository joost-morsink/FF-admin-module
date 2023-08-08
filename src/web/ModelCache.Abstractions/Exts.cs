using System;
using System.Linq;

namespace FfAdmin.ModelCache.Abstractions;

public static class Exts
{
    public static string ToHexString(this byte[] bytes)
        => Convert.ToHexString(bytes);
    public static string ToBase64String(this byte[] bytes)
        => Convert.ToBase64String(bytes);
    public static byte[] ToByteArray(this string str)
        => Convert.FromHexString(str);
}
