using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data.Extensibility;
using System.ComponentModel.Composition;
using FluentNHibernate;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.Conventions;

namespace Monahrq.Infrastructure.Test.Integration.Services
{
    [Export(typeof(ITypeProvider))]
    class ClassMappingTypeProvider : TypeProvider
    {
        enum classtype { none, classmap, subclass, convention }

        public ClassMappingTypeProvider()
        {
            var conventionList = new List<Type>();
            var mapList = new List<Type>();

            var assys = AppDomain.CurrentDomain.GetAssemblies()
                .Where( assy=> !assy.IsDynamic );
            var types = assys.SelectMany( assy => assy.GetExportedTypes())
                .Where(type => 
                            !type.IsAbstract
                            && type.GetCustomAttributes<ExportAttribute>()
                                .Any( attr=> 
                                    typeof(IMappingProvider).IsAssignableFrom(attr.ContractType)
                                    || typeof(IMappingProvider).IsAssignableFrom(attr.ContractType)
                                    || typeof(IIndeterminateSubclassMappingProvider).IsAssignableFrom(attr.ContractType))
                        ).ToList();
                        

            types.ForEach((t) =>
                {

                    var attrs = t.GetCustomAttributes<ExportAttribute>();
                    attrs.Where( attr=> typeof(IMappingProvider).IsAssignableFrom(attr.ContractType))
                        .ToList().ForEach(attr=> mapList.Add(t));
                    attrs.Where( attr=> typeof(IIndeterminateSubclassMappingProvider).IsAssignableFrom(attr.ContractType))
                            .ToList().ForEach(attr=> mapList.Add(t));
                    attrs.Where( attr=> typeof(IConvention).IsAssignableFrom(attr.ContractType))
                            .ToList().ForEach(attr=> conventionList.Add(t));
                });
            ConventionTypes = conventionList;
            ProviderTypes = mapList;
        }
    }
}
