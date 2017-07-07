using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Domain.Common.Maps
{
    [MappingProviderExport]
    public class AddressMap : EntityMap<Address, int, IdentityGeneratedKeyStrategy>
    {
        public AddressMap()
        {
            var indexName = "IDX_" + EntityTableName;

            Map(x => x.Line1).Length(255).Not.Nullable();
            Map(x => x.Line2).Length(255).Nullable();
            Map(x => x.Line3).Length(255).Nullable();

            Map(x => x.City).Length(150).Nullable().Index(indexName);
            Map(x => x.State).Length(3).Not.Nullable().Index(indexName);
            Map(x => x.ZipCode).Length(12).Nullable().Index(indexName);

            Map(x => x.Index, "[Index]").Default("0").Nullable();

            DiscriminateSubClassesOnColumn("AddressType")
                .AlwaysSelectWithValue()
                .Length(30)
                .Index(indexName + "_AddressTypes");

            Cache.NonStrictReadWrite().Region(Inflector.Pluralize(typeof (Address).Name));

        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }

        //public override string EntityTableName
        //{
        //    get { return Inflector.Pluralize(typeof (Address).Name); }
        //}
    }
}
