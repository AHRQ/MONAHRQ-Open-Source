using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Integration
{
    public interface ILifetimeController
    {
        Lazy<T> Create<T>();
        void Release<T>(Lazy<T> export);
    }

    [Export(typeof(ILifetimeController))]
    public class LifetimeController : ILifetimeController
    {
        CompositionContainer Container { get; set; }

        public LifetimeController(Func<CompositionContainer> factory)
        {
            Container = factory();
        }

        public Lazy<T> Create<T>()
        {
            return Container.GetExport<T>();
        }

        public void Release<T>(Lazy<T> export)
        {
            Container.ReleaseExport<T>(export);
        }
    }
}