using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions.Maps
{

    public class ClinicalDimensionMap<T> : EntityMap<T, int, IdentityGeneratedKeyStrategy>
        where T : ClinicalDimension
    {
        public ClinicalDimensionMap()
        {
            Map(x => x.Description);
            Map(x => x.FirstYear);
            Map(x => x.LastYear);
            Map(x => x.Version);
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }

        //public override string EntityTableName
        //{
        //    get
        //    {
        //        return "Base_" + Inflector.Pluralize(typeof(T).Name);
        //    }
        //}
    }
}
