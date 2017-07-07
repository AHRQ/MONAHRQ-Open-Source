using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    public class AssignedKeyLookupMap<TEntity> : EntityMap<TEntity, int, AssignedValueKeyStrategy>
         where TEntity : Entity<int>
    {
        protected override PropertyPart NameMap()
        {
            return Map(i => i.Name)
                .Length(255).Unique()
                .Not.Nullable().Index(NameIndexName);
        }
    }

    public class GeneratedKeyLookupMap<TEntity> : EntityMap<TEntity, int, IdentityGeneratedKeyStrategy>
         where TEntity : Entity<int>
    {
        protected override PropertyPart NameMap()
        {
            return Map(i => i.Name)
                .Length(255).Unique()
                .Not.Nullable().Index(NameIndexName);
        }
    }

    public class EnumLookupEntityMap<TEntity> : EntityMap<TEntity, int, AssignedValueKeyStrategy>
         where TEntity : EnumLookupEntity<int>
    {
        public EnumLookupEntityMap()
        {
            Map(x => x.Value)
                .Not.Nullable()
                .UniqueKey(string.Format("UQ_{0}_Value", EntityTableName))
                .Unique();
        }
    }
}
