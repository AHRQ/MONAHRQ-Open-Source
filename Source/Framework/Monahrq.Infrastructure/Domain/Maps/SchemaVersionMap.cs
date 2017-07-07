using System.Security.Cryptography.X509Certificates;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData.Maps;

namespace Monahrq.Infrastructure.Domain.Maps
{
    [MappingProviderExport]
    public class SchemaVersionMap : GeneratedKeyLookupMap<SchemaVersion>
    {
        public SchemaVersionMap()
        {
            Map(x => x.Version).CustomSqlType("nvarchar(50)");
            Map(x => x.Month).Nullable();
            Map(x => x.Year).Nullable();
            Map(x => x.ActiveDate).Not.Nullable().Default("getDate()");
            Map(x => x.VersionType).CustomType<VersionType>().Nullable();
            Map(x => x.FileName).Length(255).Nullable();
            Map(x => x.Major).Access.ReadOnly().Not.Insert().Not.Update();
            Map(x => x.Minor).Access.ReadOnly().Not.Insert().Not.Update();
            Map(x => x.Milestone).Access.ReadOnly().Not.Insert().Not.Update();
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return Map(i => i.Name)
                .Length(100).Unique().UniqueKey(string.Format("UI_{0}_NAME", EntityTableName.ToUpper()))
                .Not.Nullable().Index(NameIndexName);
        }
    }
}
