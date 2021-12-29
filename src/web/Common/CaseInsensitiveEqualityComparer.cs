using System;
using System.Collections.Generic;

namespace FfAdmin.Common
{
    public class CaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        public static CaseInsensitiveEqualityComparer Instance { get; } = new ();

        public bool Equals(string? x, string? y)
            => ReferenceEquals(x, y) || x != null && x.Equals(y, StringComparison.InvariantCultureIgnoreCase);

        public int GetHashCode(string obj)
            => obj.ToLowerInvariant().GetHashCode();
    }
}
