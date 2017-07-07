using FluentNHibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Data.Extensibility
{
    public interface IExtensionRegistry
    {
        void RegisterTarget(Type targetType); 
    }

    [Export(typeof(IExtensionRegistry))]
    [Export(typeof(ITypeSource))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class ExtensionRegistry :
        ITypeSource, IExtensionRegistry
    {

        public ExtensionRegistry()
        {
            Targets = new List<Type>();
            Migrations = new List<Type>();
            LazyTypes = new Lazy<IEnumerable<Type>>(() => Targets.Distinct());
        }

        Lazy<IEnumerable<Type>> LazyTypes { get; set; }


        IList<Type> Targets { get; set; }
        IList<Type> Migrations { get; set; }

        public string GetIdentifier()
        {
            return "MONAHRQ Extensibility";
        }

        public IEnumerable<Type> GetTypes()
        {
            return LazyTypes.Value;
        }

        public void LogSource(FluentNHibernate.Diagnostics.IDiagnosticLogger logger)
        {

        }

        public void RegisterTarget(Type targetType)
        {
            Targets.Add(targetType);
        }

      
    }

}
