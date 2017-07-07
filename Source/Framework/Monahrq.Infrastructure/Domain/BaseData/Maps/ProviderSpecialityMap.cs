using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.BaseData.Maps;

namespace Monahrq.Infrastructure.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ProviderSpecialityMap : GeneratedKeyLookupMap<ProviderSpeciality>
    {
        private readonly string _indexName = "IDX_" + typeof(ProviderSpeciality).Name;
        
        public ProviderSpecialityMap()
        {
            Map(x => x.ProviderTaxonomy).CustomSqlType("nvarchar(2000)")
                .Nullable();
                //.Index(_indexName);
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            var unqueIndexName = _indexName.Replace("IDX_", "UDX_");
            return Map(i => i.Name)
                 .Length(256).Unique().UniqueKey(unqueIndexName)
                 .Not.Nullable().Index(NameIndexName);
        }

        protected override string NameIndexName
        {
            get
            {
                return _indexName;
            }
        }
    }
}
