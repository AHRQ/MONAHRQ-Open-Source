using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Utility;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Entities.Domain.Maps
{
    public abstract class EntityMap<TEntity, TId, TKeyStrategy> : ClassMap<TEntity>
        where TEntity : Entity<TId>
        where TKeyStrategy : IKeyStrategy, new()
    {
        protected EntityMap()
        {
            Schema("dbo");
            //Not.LazyLoad();

            if (!string.IsNullOrEmpty(EntityTableName ?? string.Empty))
            {
                Table(EntityTableName);
            }

            var id = Id(o => o.Id);

            KeyStrategy.Apply(id);

            NameMap();
            
            DynamicInsert();
            DynamicUpdate();
            SelectBeforeUpdate();
            OptimisticLock.Version();

            Cache.NonStrictReadWrite().Region(Inflector.Pluralize(typeof(TEntity).Name));
        }

        protected IKeyStrategy KeyStrategy
        {
            get
            {
                return new TKeyStrategy();
            }
        }

        protected string IndexName(string field)
        {
            return string.Format("IDX_{0}_{1}", Inflector.Pluralize(GetType().Name).ToUpper(), field.ToUpper());
        }

        public virtual string EntityTableName
        {
            get
            {
                return typeof(TEntity).EntityTableName();
            }
        }

        protected virtual string NameIndexName
        {
            get { return string.Format("IDX_{0}_NAME", EntityTableName.ToUpper()); }
        }

        protected virtual string PrimaryIndexName
        {
            get { return string.Format("PK_{0}", EntityTableName.ToUpper()); }
        }

        protected virtual PropertyPart NameMap()
        {
            return Map(i => i.Name)
                .Length(255).Not.Nullable()
                .Index(NameIndexName);
        }
    }

    //public abstract class NamedEntityMap<TEntity, TKey, TKeyStrategy> : EntityMap<TEntity, TKey, TKeyStrategy>
    //    where TKeyStrategy : IKeyStrategy, new()
    //    where TEntity : Entity<TKey>
    //{
    //    protected NamedEntityMap()
    //        : base()
    //    {
    //        NameMap();
    //    }

    //    protected new virtual string NameIndexName
    //    {
    //        get { return string.Format("IDX_{0}_Name", EntityTableName); }
    //    }

    //    protected new virtual PropertyPart NameMap()
    //    {
    //        return Map(i => i.Name)
    //            .Length(255)
    //            .Not.Nullable().Index(NameIndexName);
    //    }
    //}

    public abstract class ExtendedEntityMap<TEntity, TId, TKeyStrategy> : EntityMap<TEntity, TId, TKeyStrategy>
        where TKeyStrategy : IKeyStrategy, new()
        where TEntity : Entity<TId>
    {
        protected abstract void ApplyExtensionMappings();

        protected ExtendedEntityMap()
            : base()
        {
            ApplyExtensionMappings();
        }
    }

    public abstract class EntityExtensionMap<TEntity, TExtended, TId, TKeyStrategy> : EntityMap<TEntity, TId, TKeyStrategy>
        where TKeyStrategy : IKeyStrategy, new()
        where TExtended : Entity<TId>
        where TEntity : OwnedEntity<TExtended, TId, TId>
    {

        protected OneToOnePart<TExtended> OwnerReference { get; set; }

        protected EntityExtensionMap()
        {
            Id(extension => extension.Id)
                .Column(typeof(TExtended).Name + "_id")
                .GeneratedBy.Foreign("Owner");
            OwnerReference = HasOne(e => e.Owner)
               .Constrained()
               .Not.LazyLoad()
               .ForeignKey(string.Format("{0}_{1}_Extension_FK", typeof(TExtended).Name, typeof(TEntity).Name));

        }

    }

    public abstract class OwnedEntityMap<TEntity, TId, TOwner, TOwnerId, TKeyStrategy> : EntityMap<TEntity, TId, TKeyStrategy>
        where TKeyStrategy : IKeyStrategy, new()
        where TOwner : Entity<TOwnerId>
        where TEntity : OwnedEntity<TOwner, TOwnerId, TId>
    {
        //protected override void ApplyExtensionMappings()
        //{

        //}

        protected OwnedEntityMap()
            : base()
        {
            OwnerReference = References(o => o.Owner)
                .Column(typeof(TOwner).Name + "_id")
                .ForeignKey(string.Format("{0}_owns_{1}_FK", typeof(TOwner).Name, typeof(TEntity).Name))
                .Not.LazyLoad();

            OwnerReference = typeof (TOwner) == typeof (Wings.Wing)
                ? OwnerReference.Cascade.None()
                : OwnerReference.Cascade.SaveUpdate();
        }

        protected ManyToOnePart<TOwner> OwnerReference { get; set; }

    }

    public abstract class OwnedEntitySubclassMap<TEntity, TKey, TOwner, TOwnerKey> : SubclassMap<TEntity>
        where TEntity : OwnedEntity<TOwner, TOwnerKey, TKey>
        where TOwner : Entity<TOwnerKey>
    {
        protected OwnedEntitySubclassMap()
            : base()
        {
            Table(EntityTableName);
            References(o => o.Owner).Cascade.None()
                      .Cascade.SaveUpdate()
                .Column(typeof(TOwner).Name + "_id").Not.LazyLoad()
                      .ForeignKey(string.Format("{0}_owns_{1}_FK", typeof(TOwner).Name, typeof(TEntity).Name));
        }

        protected virtual string OwnerName
        {
            get
            {
                return "Owner_Id";
            }
        }

        protected virtual string NameIndexName
        {
            get { return string.Format("IDX_{0}_Name", EntityTableName); }
        }

        public virtual string EntityTableName
        {
            get
            {
                return typeof(TEntity).EntityTableName();
            }
        }
    }

}
