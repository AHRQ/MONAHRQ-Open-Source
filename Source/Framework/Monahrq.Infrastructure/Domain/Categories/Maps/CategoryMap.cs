using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Domain.Categories.Maps
{
    [Data.MappingProviderExport]
    public class CategoryMap : EntityMap<Category, int, IdentityGeneratedKeyStrategy>
    {
        public CategoryMap()
        {
            var indexName = "IDX_" + EntityTableName;
            Map(x => x.CategoryID).Index(indexName);
            Map(x => x.Version).Index(indexName);
            Map(x => x.IsSourcedFromBaseData)
                      .CustomSqlType("bit")
                      .Index(indexName)
                      .Not.Nullable()
                      .Default("0");

            DiscriminateSubClassesOnColumn("CategoryType")
                .Length(50)
                .AlwaysSelectWithValue()
                .Index(indexName + "_CategoryTypes");

            Cache.NonStrictReadWrite().Region(Inflector.Pluralize(typeof(Category).Name));
        }

        public override string EntityTableName
        {
            get
            {
                return Inflector.Pluralize(typeof(Category).Name);
            }
        }
    }
}
