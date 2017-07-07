using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Entities.Domain.Wings.Maps
{
    [MappingProviderExport]
    public class ScopeMap : TargetOwnedWingItemMap<Scope, int, IdentityGeneratedKeyStrategy>
    {
        public ScopeMap()
        {
            var idxName = string.Format("IDX_{0}", typeof(Scope).EntityTableName());
            Map(x => x.ClrType)
                .Nullable().Index(idxName);
            Map(x => x.IsCustom)
                .Not.Nullable().Default("0").Index(idxName);

            References(x => x.Owner).Column("Target_Id").Not.Nullable()
                                    .Not.LazyLoad()
                                    .Cascade.All();

            HasMany(x => x.Values)
                .Not.LazyLoad()
                .Not.Inverse()
                .Cascade.All();

            Cache.ReadWrite().Region("Scopes");
        }
    }

    [MappingProviderExport]
    public class ScopeValueMap : ScopedOwnedWingItemMap<ScopeValue, int, IdentityGeneratedKeyStrategy>
    {
        public ScopeValueMap()
        {
            References(x => x.Owner).Column("Scope_Id").Not.Nullable()
                .Not.LazyLoad()
                .Cascade.All();

            Map(x => x.Value).CustomType<SqlVariant>().CustomSqlType("SQL_VARIANT")
                .Not.LazyLoad().Not.Nullable();

            Cache.ReadWrite().Region("ScopeValues");
        }
    }
}