using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using System;

namespace Monahrq.Infrastructure.Entities.Domain.Wings.Maps
{

    [MappingProviderExport]
    public class WingMap : WingItemMap<Wing, int, IdentityGeneratedKeyStrategy>
    {
        public WingMap()
        {
            Map(x => x.WingGUID).Unique().Not.Nullable();
            HasMany(x => x.Targets)
               .Inverse()
               .Not.LazyLoad()
               .Cascade.All();
			Map(x => x.LastWingUpdate).CustomType<NullableDateTimeType>().Default(DateTime.MinValue.ToString("MM/dd/yy"));

			Cache.ReadWrite().Region("Wings");
        }

        //protected override string NameIndexName
        //{
        //    get { return string.Format("UDX_{0}_Name", EntityTableName); }
        //}

        protected override PropertyPart NameMap()
        {
            return base.NameMap().Unique()
                  .Length(255);
        }
    }

    public abstract class WingOwnedWingItemMap<TItemType, TKey, TKeyStrategy> : OwnedWingItemMap<Wing, int, TItemType, TKey, TKeyStrategy>
             where TKeyStrategy : IKeyStrategy, new()
                where TItemType : WingOwnedWingItem<TKey>
    {}
}
