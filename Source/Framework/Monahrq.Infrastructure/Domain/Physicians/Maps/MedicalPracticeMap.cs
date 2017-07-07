using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Domain.Physicians.Maps
{
    [MappingProviderExport]
    public class MedicalPracticeMap : EntityMap<MedicalPractice, int, IdentityGeneratedKeyStrategy>
    {
        public MedicalPracticeMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.GroupPracticePacId).Length(15)
                .Unique()
                .UniqueKey(string.Format("UX_{0}_GroupPracticePacId", EntityTableName))
                .Not.Nullable()
                .Index(indexName);

            Map(x => x.DBAName).Length(150).Nullable();

            Map(x => x.NumberofGroupPracticeMembers)
                .Nullable().Index(indexName);

            HasMany(X => X.Addresses)
                .KeyColumn(typeof(MedicalPractice).Name + "_Id")
                .Not.Inverse()
                .AsList(x => x.Column("[Index]"))
                .Cascade.AllDeleteOrphan()
                .Not.LazyLoad().NotFound.Ignore()
                .ForeignKeyConstraintName(string.Format("FK_Addresses_{0}", typeof(MedicalPractice).Name))
                .Cache.Region("MedicalPractice_Addresses").NonStrictReadWrite();

            Map(x => x.State).Length(3)
                             .Index(indexName);

            Map(x => x.IsEdited).Not.Nullable().Default("0").Index(indexName);

            Map(x => x.Version).Index(base.IndexName("Version"));
        }

        protected override PropertyPart NameMap()
        {
            return Map(i => i.Name)
                .Length(150).Not.Nullable()
                .Index(NameIndexName);

        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SubclassMappingProviderExport]
    public class MedicalPracticeAddressMap : SubclassMap<MedicalPracticeAddress>
    {
        public MedicalPracticeAddressMap()
        {
            DiscriminatorValue(typeof(MedicalPractice).Name);

            Map(x => x.Version);
        }
    }
}