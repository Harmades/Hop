using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;

namespace Hop
{
    public class Hop : IDisposable
    {
        public List<IModule> Modules { get; }

        private CompositionContainer Container { get; }

        public Hop()
        {
            Container =
                new CompositionContainer(
                    new AggregateCatalog(
                        new ComposablePartCatalog[] { new DirectoryCatalog("./modules") }
                        )
                    );
            Modules = Container.GetExportedValues<IModule>().ToList();
        }

        public void Dispose() => Container.Dispose();

        public void LogExeption(Exception exception) => Trace.WriteLine($"[{DateTime.Now}] {exception.Message} :{Environment.NewLine}{exception.Message}");

        private IEnumerable<Item> QueryModule(Query query, IModule module)
        {
            try
            {
                return module.Query(query);
            }
            catch (Exception exception)
            {
                this.LogExeption(exception);
                return Enumerable.Empty<Item>();
            }
        }

        public IEnumerable<Item> Query(Query query) => this.Modules.SelectMany(module => this.QueryModule(query, module));
    }
}
