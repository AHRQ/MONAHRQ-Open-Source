using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate;
using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping.Providers;

namespace Monahrq.Infrastructure.Data.Extensibility
{
    
    public interface ITypeProvider
    {
        IEnumerable<Type> ProviderTypes {get;}
        IEnumerable<Type> ConventionTypes { get; } 
    }


    public abstract class TypeProvider : ITypeProvider
    {
        protected TypeProvider() { }

        public TypeProvider(IEnumerable<Type> providerTypes, IEnumerable<Type> conventionTypes)
        {
            ProviderTypes = providerTypes;
            ConventionTypes = conventionTypes;
        }
       
        public IEnumerable<Type> ProviderTypes { get; protected set; }
        public IEnumerable<Type> ConventionTypes { get; protected set; }
      
    }

    [Export(typeof(ITypeProvider))]
    public class MappingProviderTypeProvider : TypeProvider
    {
        static IEnumerable<Type> ConcatTypes(IEnumerable<Type> src1, IEnumerable<Type> src2)
        {
            return src1.Concat(src2);
        }

       public MappingProviderTypeProvider(
            [ImportMany]IEnumerable<IMappingProvider> mappingProviders
            , [ImportMany]IEnumerable<IIndeterminateSubclassMappingProvider> subclassMappingProviders
            , [ImportMany]IEnumerable<IConvention> conventions)
            : base( mappingProviders
                                .Select(mp=>mp.GetType())
                                .Concat(subclassMappingProviders.Select(mp => mp.GetType())), conventions.Select(mp => mp.GetType()))
        {
        }
    }


}
