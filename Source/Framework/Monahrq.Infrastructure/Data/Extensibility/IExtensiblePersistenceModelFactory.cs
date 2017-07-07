using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using Monahrq.Infrastructure.FileSystem;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Monahrq.Infrastructure.Data.Extensibility
{
    public interface IExtensiblePersistenceModelFactory
    {
        AutoPersistenceModel CreateModel();
    }


    [Export(typeof(IExtensiblePersistenceModelFactory))]
    public class ExtensiblePersistenceModelFactory : IExtensiblePersistenceModelFactory
    {
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        ITypeSource TypeSource { get; set; }

        [Import]
        IUserFolder UserFolder { get; set; }

        [ImportMany]
        IEnumerable<IExtensibilityAlteration> Alterations { get; set; }

        [ImportMany]
        IEnumerable<IExtensibilityConvention> Conventions {get;set;}

        public AutoPersistenceModel CreateModel()
        {
            var conventions = Conventions.ToArray<IConvention>();
            var theTypes = TypeSource.GetTypes();
            return AutoMap.Source(TypeSource)
                // Ensure that namespaces of types are never auto-imported, so that 
                // identical type names from different namespaces can be mapped without ambiguity
                .Conventions.Setup(x => x.Add(AutoImport.Never()))
                .Conventions.Add(conventions)
                .Alterations(alt =>
                {
                    foreach (var recordAssembly in theTypes.Select(x => x.Assembly).Union(new[] { typeof(IExtensiblePersistenceModelFactory).Assembly }).Distinct())
                    {
                        alt.Add(new AutoMappingOverrideAlteration(recordAssembly));
                    }
                    foreach (var custAlt in Alterations)
                    {
                        alt.Add(custAlt);
                    }
                });
        }

    }

}
