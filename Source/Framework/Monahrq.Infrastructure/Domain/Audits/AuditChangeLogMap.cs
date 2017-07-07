/*
 * Commented out this code unless we need a more advanced way of mapping audit logs.
 * Jason L. Duffus 11/20/2014
 */

#region Commented out code
    //using Monahrq.Infrastructure.Entities.Data.Strategies;
    //using Monahrq.Infrastructure.Entities.Domain.Maps;
    //using Monahrq.Sdk.Utilities;
    //using NHibernate.Type;

    //namespace Monahrq.Infrastructure.Domain.Audits
    //{
    //    [Data.MappingProviderExport]
    //    public class AuditChangeLogMap : EntityMap<AuditChangeLog, int, GuidGeneratedKeyStrategy>
    //        //where T : AuditChangeLog<TEntity>
    //        //where TEntity : Entity<int>
    //    {
    //        protected AuditChangeLogMap()
    //        {
    //            var indexName = string.Format("IDX_{0}", EntityTableName);

    //            UseUnionSubclassForInheritanceMapping();

    //            //DiscriminateSubClassesOnColumn("AuditChangeType")
    //            //     .Length(50)
    //            //     .AlwaysSelectWithValue()
    //            //     .Index(indexName + "_AuditTypes");

    //            Map(x => x.Action)
    //                .CustomType<EnumType<AuditType>>()
    //                .Length(20).Not.Nullable();

    //            Map(x => x.OwnerType, "EntityTypeName")
    //                .Length(50)
    //                .Not.Nullable()
    //                .Index(indexName);

    //            Map(x => x.Owner.Id, "Owner_Id")
    //                .Not.Nullable()
    //                .Index(indexName);

    //            Map(x => x.PropertyName)
    //                .Length(50)
    //                .Index(indexName);

    //            Map(x => x.OldPropertyValue)
    //                .CustomSqlType("nvarchar(max)");

    //            Map(x => x.NewPropertyValue)
    //                .CustomSqlType("nvarchar(max)");

    //            Map(x => x.CreateDate).Not.Nullable();

    //            Cache.NonStrictReadWrite().Region(Inflector.Pluralize(GetType().Name));
    //        }

    //        public override string EntityTableName
    //        {
    //            get
    //            {
    //                return null;
    //            }
    //        }

    //        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
    //        {
    //            return null;
    //        }
    //    }
    //}
#endregion