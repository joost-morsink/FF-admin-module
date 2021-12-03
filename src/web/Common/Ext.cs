using System;
using System.Collections.Generic;

namespace FfAdmin.Common
{
    public static class Ext
    {
        public static IEnumerable<T> SelectValues<T>(this IEnumerable<T?> src)
        {
            foreach (var x in src)
                if (x != null)
                    yield return x;
        }
    }
}
