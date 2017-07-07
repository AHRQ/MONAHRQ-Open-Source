using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Domain.Flutters.Maps
{
    [MappingProviderExport]
    public class FlutterMap : EntityMap<Flutter, int, IdentityGeneratedKeyStrategy>
    {
        private readonly string _indexName = null;
        public FlutterMap()
        {
            _indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.ConfigurationId).Length(256);
            Map(x => x.AssociatedReportsTypes).Length(500);
            Map(x => x.InstallPath).Length(255);
            Map(x => x.OutputPath).Length(255);
            Cache.NonStrictReadWrite().Region(Inflector.Pluralize(typeof(Flutter).Name));
        }

        protected override PropertyPart NameMap()
        {
            return Map(i => i.Name)
                       .Length(255).Not.Nullable()
                       .Index(_indexName);
        }
    }
}
