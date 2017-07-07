using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Entities.Domain.Wings.Maps
{
    [MappingProviderExport]
    public class ElementMap : TargetOwnedWingItemMap<Element, int, IdentityGeneratedKeyStrategy>
    {
        public ElementMap()
        {
            var idxName = string.Format("IDX_{0}", typeof(Element).EntityTableName());
            Map(x => x.IsRequired).Not.Nullable().Default("0").Index(idxName);
            Map(x => x.Hints).Default(string.Empty);
            Map(x => x.LongDescription).Default(string.Empty);
            Map(x => x.Ordinal).Not.Nullable().Default(int.MaxValue.ToString());
            Map(x => x.Type).CustomType<NHibernate.Type.EnumStringType<DataTypeEnum>>()
                            .Length(50).Nullable().Index(idxName);
            References(x => x.Scope).Not.LazyLoad();
            References(x => x.DependsOn).Not.LazyLoad();
            References(e => e.Owner).Column("Target_Id").Not.LazyLoad().Cascade.All();

            Cache.ReadWrite().Region("Elements");
        }
    }
}