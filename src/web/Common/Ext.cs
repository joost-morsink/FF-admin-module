using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace FfAdmin.Common
{
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static class Ext
    {
        public static IEnumerable<T> SelectValues<T>(this IEnumerable<T?> src)
        {
            foreach (var x in src)
                if (x != null)
                    yield return x;
        }
        public static XNode RemoveNamespaces(this XNode node)
            => node switch
            {
                XElement e => e.RemoveNamespaces(),
                _ => node
            };

        public static XElement RemoveNamespaces(this XElement node)
            => new (node.Name.LocalName, node.Attributes(), node.Nodes().Select(x => x.RemoveNamespaces()));

    }
}
