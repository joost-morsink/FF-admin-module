using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pidgin;
namespace FfAdmin.Common
{
    public class CaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        public static CaseInsensitiveEqualityComparer Instance { get; } = new CaseInsensitiveEqualityComparer();

        public bool Equals(string? x, string? y)
            => ReferenceEquals(x, y) || x != null && x.Equals(y, StringComparison.InvariantCultureIgnoreCase);

        public int GetHashCode([DisallowNull] string obj)
            => obj.ToLowerInvariant().GetHashCode();
    }
}