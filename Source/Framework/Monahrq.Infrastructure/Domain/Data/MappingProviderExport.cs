using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Conventions;

namespace Monahrq.Infrastructure.Domain.Data
{
    public class MappingProviderExportAttribute : ExportAttribute
    {
        public MappingProviderExportAttribute() : base(typeof(FluentNHibernate.IMappingProvider)) 
        { 
        }
    }

     public class SubclassMappingProviderExportAttribute : ExportAttribute
    {
        public SubclassMappingProviderExportAttribute() 
            : base(typeof(FluentNHibernate.Mapping.Providers.IIndeterminateSubclassMappingProvider)) 
        { 
        }
    }

    

    public class ConventionExportAttribute : ExportAttribute
    {
        public ConventionExportAttribute()
            : base(typeof(IConvention))
        {
        }
    }
}
