using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Hop
{
    [InheritedExport]
    public interface IModule
    {
        string Name { get; }

        IEnumerable<Item> Query(Query query);
    }
}
