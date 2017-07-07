using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Domain.Audits;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Domain.NursingHomes.Maps
{
    public class NursingHomeMap : EntityMap<NursingHome, int, IdentityGeneratedKeyStrategy>
    {
        public NursingHomeMap()
        {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            var indexName = string.Format("IDX_{0}", EntityTableName);
// ReSharper restore DoNotCallOverridableMethodsInConstructor

            Map(x => x.LegalBusinessName).Length(150)
                .Index(indexName);

            Map(x => x.ProviderId).Length(10)
                .Nullable()
                .Index(indexName);

            Map(x => x.Address).Length(250)
                .Nullable()
                .Index(indexName);

            Map(x => x.City).Length(100)
                .Nullable()
                .Index(indexName);

            Map(x => x.State).Length(3)
                .Not.Nullable()
                .Index(indexName);

            Map(x => x.Zip).Length(12)
                .Nullable()
                .Index(indexName);

            Map(x => x.CountySSA).Length(5)
                .Nullable()
                .Index(indexName);

            Map(x => x.CountyName).Length(150)
                .Nullable();

            Map(x => x.Phone).Length(15)
               .Nullable()
               .Index(indexName);

            Map(x => x.NumberCertBeds)
                .Nullable();

            Map(x => x.NumberResidCertBeds)
               .Nullable();

            Map(x => x.ParticipationDate).CustomType<NullableDateTimeType>()
              .Nullable();

            Map(x => x.Certification).Length(255)
               .Nullable();

            Map(x => x.ResFamCouncil).Length(50)
               .Nullable();
            //.Index(indexName);

            Map(x => x.SprinklerStatus).Length(50)
                                       .Nullable();
            //.Index(indexName);

            Map(x => x.InHospital)
                .Nullable();
            //.Index(indexName);

            Map(x => x.IsCCRCFacility).Default("0")
                                      .Not.Nullable();
            //.Index(indexName);

            Map(x => x.IsSFFacility).Default("0")
                                    .Not.Nullable();
            //.Index(indexName);

            Map(x => x.Description)
                .Length(1000).Nullable();

            Map(x => x.Accreditation).Length(200)
                .Nullable();

            Map(x => x.Ownership).Length(200)
                .Nullable();
                //.Index(indexName);

            Map(x => x.InRetirementCommunity)
                .Nullable();
            //.Index(indexName);

            Map(x => x.HasSpecialFocus)
                .Nullable();
            //.Index(indexName);

            Map(x => x.ChangedOwnership_12Mos).Default("0")
                                              .Not.Nullable();
            //.Index(indexName);

            References(x => x.Type, "CategoryType_Id")
                // ReSharper disable DoNotCallOverridableMethodsInConstructor
                .ForeignKey("FK_" + EntityTableName + "_Cateorigies")
                // ReSharper restore DoNotCallOverridableMethodsInConstructor
                .Cascade.None()
                .Not.LazyLoad()
                .Nullable();

            Map(x => x.IsDeleted).Default("0")
                                 .Not.Nullable()
                                 .Index(indexName);

            Map(x => x.FileDate).CustomType<NullableDateTimeType>()
                                .Nullable()
                                .Index(indexName);



            // HasMany(X => X.Addresses)
            //     .KeyColumn(typeof(NursingHome).Name + "_Id")
            //     .Not.Inverse()
            //    .AsList(x => x.Column("[Index]"))
            //    .Cascade.AllDeleteOrphan()
            //    .Not.LazyLoad()
            //    .ForeignKeyConstraintName(string.Format("FK_Addresses_{0}", typeof(NursingHome).Name))
            //    .Cache.Region("NursingHome_Addresses").NonStrictReadWrite();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Data.MappingProviderExport]
    public class NursingHomeAuditMap : EntityMap<NursingHomeAuditLog, int, IdentityGeneratedKeyStrategy>
    {
        public NursingHomeAuditMap()
        {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            var indexName = string.Format("IDX_{0}", EntityTableName);
// ReSharper restore DoNotCallOverridableMethodsInConstructor

            Map(x => x.Action)
               .CustomType<EnumStringType<AuditType>>()
               .Length(20).Not.Nullable();

            Map(x => x.OwnerType, "EntityTypeName")
                .Length(50)
                .Not.Nullable()
                .Index(indexName);

            Map(x => x.OwnerId, "Owner_Id")
                .Not.Nullable()
                .Index(indexName);

            Map(x => x.PropertyName)
                .Length(150)
                .Index(indexName);

            Map(x => x.OldPropertyValue).Length(500);

            Map(x => x.NewPropertyValue).Length(500);

            Map(x => x.CreateDate).Not.Nullable();

            Map(x => x.ProviderId).Index(indexName);

            Cache.NonStrictReadWrite().Region(Inflector.Pluralize(typeof(NursingHomeAuditLog).Name));
            
        }

        protected override PropertyPart NameMap()
        {
            return null;
        }
    }
}
