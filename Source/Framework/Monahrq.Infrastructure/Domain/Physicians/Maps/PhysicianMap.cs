using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Domain.Audits;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Domain.Physicians.Maps
{
	[MappingProviderExport]
	public class PhysicianMap : EntityMap<Physician, int, IdentityGeneratedKeyStrategy>
	{
		public PhysicianMap()
		{
			var indexName = string.Format("IDX_{0}", EntityTableName);

			Map(x => x.Npi)
						.Unique()
						.UniqueKey(string.Format("UX_{0}_Npi", EntityTableName))
						//.Index(indexName)
						.Not.Nullable();

			Map(x => x.PacId).Length(17)
						.Index(indexName);
			Map(x => x.ProfEnrollId).Length(20)
						.Index(indexName);

			Map(x => x.FirstName).Length(50)
						.Index(indexName);
		    Map(x => x.MiddleName).Length(50);
						//.Index(indexName);
			Map(x => x.LastName).Length(50)
						.Index(indexName);
		    Map(x => x.Suffix).Length(20);
						//.Index(indexName);

			Map(x => x.Gender).Length(10);

			Map(x => x.Credentials).Length(50);

			Map(x => x.ForeignLanguages).CustomType<StringListToStringType>()
										.Length(255).Nullable();//.Index(indexName);

			Map(x => x.MedicalSchoolName).Length(255);
			Map(x => x.GraduationYear);

			Map(x => x.CouncilBoardCertification)
						 .CustomSqlType("bit").Default("0");

			Map(x => x.PrimarySpecialty).Length(255).Nullable();
			Map(x => x.SecondarySpecialty1).Length(255).Nullable();
			Map(x => x.SecondarySpecialty2).Length(255).Nullable();
			Map(x => x.SecondarySpecialty3).Length(255).Nullable();
			Map(x => x.SecondarySpecialty4).Length(255).Nullable();
			Map(x => x.AcceptsMedicareAssignment).Index(indexName).Length(2).Nullable();
			Map(x => x.ParticipatesInERX).Nullable().Index(indexName);
			Map(x => x.ParticipatesInPQRS).Nullable().Index(indexName);
			Map(x => x.ParticipatesInEHR).Nullable().Index(indexName);

			Map(x => x.States).CustomType<StringListToStringType>().Length(30).Nullable().Index(indexName);

			Map(x => x.Version).Index(indexName);
			Map(x => x.IsDeleted).Not.Nullable().Default("0").Index(indexName);
			Map(x => x.IsEdited).Not.Nullable().Default("0").Index(indexName);

			HasMany(x => x.Addresses)
				.KeyColumn(typeof(Physician).Name + "_Id")
				.Not.Inverse()
				.AsList(x => x.Column("[Index]"))
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.ForeignKeyConstraintName(string.Format("FK_Addresses_{0}", typeof(Physician).Name))
				.Cache.Region(typeof(Physician).Name + "_Addresses").NonStrictReadWrite();

			HasMany(x => x.PhysicianMedicalPractices)
				.KeyColumn(typeof(Physician).Name + "_Id")
				.AsBag()
				.Not.Inverse()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Physicians_MedicalPractices").NonStrictReadWrite();
		

			HasMany(x => x.AffiliatedHospitals)
				.KeyColumn(typeof(Physician).Name + "_Id")
				.AsList(x => x.Column("[Index]"))
				.Not.Inverse()
				.NotFound.Ignore()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region(typeof(Physician).Name + "_AffiliatedHospitals").NonStrictReadWrite();

		}

		protected override PropertyPart NameMap()
		{
			return null;
		}
	}

	[MappingProviderExport]
	public class PhysicianAffialitedHospitalMap : EntityMap<PhysicianAffiliatedHospital, int, IdentityGeneratedKeyStrategy>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicianAffialitedHospitalMap"/> class.
		/// </summary>
		public PhysicianAffialitedHospitalMap()
		{
			var indexName = string.Format("IDX_{0}", EntityTableName);

			Map(x => x.HospitalCmsProviderId, "Hospital_CmsProviderId").Length(7)
				.Not.Nullable()
				.Index(indexName);

			Map(x => x.Version).Index(indexName);
		}

		/// <summary>
		/// Names the map.
		/// </summary>
		/// <returns></returns>
		protected override PropertyPart NameMap()
		{
			return null;
		}
	}

	[SubclassMappingProviderExport]
	public class PhysicianAddressMap : SubclassMap<PhysicianAddress>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicianAddressMap"/> class.
		/// </summary>
		public PhysicianAddressMap()
		{
			DiscriminatorValue(typeof(Physician).Name);

			Map(x => x.Version);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[MappingProviderExport]
	public class PhysicianAuditMap : EntityMap<PhysicianAuditLog, int, IdentityGeneratedKeyStrategy> //SubclassMap<PhysicianAuditLog>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicianAuditMap"/> class.
		/// </summary>
		public PhysicianAuditMap()
		{
			// the name of the table corresponding to the type
			//Table(string.Format("{0}_Audits", typeof(Physician).EntityTableName()));
			// indicates that the base class is abstract

			var indexName = string.Format("IDX_{0}", EntityTableName);

			Map(x => x.Action)
					  .CustomType<EnumType<AuditType>>()
					  .Length(20).Not.Nullable();

			Map(x => x.OwnerType, "EntityTypeName")
					  .Length(50)
					  .Not.Nullable()
					  .Index(indexName);

			Map(x => x.OwnerId, "Owner_Id")
					  .Not.Nullable()
					  .Index(indexName);

			//Map(x => x.Owner.Id, "Owner_Id")
			//    .Not.Nullable()
			//    .Index(indexName);

			Map(x => x.PropertyName)
					  .Length(50)
					  .Index(indexName);

			Map(x => x.OldPropertyValue).Length(500);

			Map(x => x.NewPropertyValue).Length(500);

			Map(x => x.Version).Index(indexName);

			Map(x => x.CreateDate).Not.Nullable();

			Cache.NonStrictReadWrite().Region(Inflector.Pluralize(typeof(PhysicianAuditLog).Name));
		}

		protected override PropertyPart NameMap()
		{
			return null;
		}
	}

}