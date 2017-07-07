using System;
using System.Xml.Linq;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Physicians;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Wing.Physician.Physicians
{
	/// <summary>
	/// The Physician Data Model.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
	[Serializable]
    [WingTarget("Physician Data", PhysicianConstants.WING_TARGET_GUID, "Mapping Physician Data", false, 6,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class PhysicianTarget : DatasetRecord
    {
		#region Physician Related Properties
		/// <summary>
		/// Gets or sets the npi.
		/// </summary>
		/// <value>
		/// The npi.
		/// </value>
		public virtual long? Npi { get; set; }
		/// <summary>
		/// Gets or sets the pac identifier.
		/// </summary>
		/// <value>
		/// The pac identifier.
		/// </value>
		public virtual string PacId { get; set; }

		/// <summary>
		/// Gets or sets the prof enroll identifier.
		/// </summary>
		/// <value>
		/// The prof enroll identifier.
		/// </value>
		public virtual string ProfEnrollId { get; set; }

		/// <summary>
		/// Gets or sets the first name.
		/// </summary>
		/// <value>
		/// The first name.
		/// </value>
		public virtual string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the name of the middle.
		/// </summary>
		/// <value>
		/// The name of the middle.
		/// </value>
		public virtual string MiddleName { get; set; }

		/// <summary>
		/// Gets or sets the last name.
		/// </summary>
		/// <value>
		/// The last name.
		/// </value>
		public virtual string LastName { get; set; }

		/// <summary>
		/// Gets or sets the suffix.
		/// </summary>
		/// <value>
		/// The suffix.
		/// </value>
		public virtual string Suffix { get; set; }

		/// <summary>
		/// Gets or sets the gender.
		/// </summary>
		/// <value>
		/// The gender.
		/// </value>
		public virtual GenderEnum? Gender { get; set; }

		///// <summary>
		///// Gets or sets the foreign languages.
		///// </summary>
		///// <value>
		///// The foreign languages.
		///// </value>
		//public virtual IList<string> ForeignLanguages { get; set; }

		/// <summary>
		/// Gets or sets the credentials.
		/// </summary>
		/// <value>
		/// The credentials.
		/// </value>
		public virtual string Credential { get; set; }

		/// <summary>
		/// Gets or sets the name of the medical school.
		/// </summary>
		/// <value>
		/// The name of the medical school.
		/// </value>
		public virtual string MedicalSchoolName { get; set; }
		/// <summary>
		/// Gets or sets the graduation year.
		/// </summary>
		/// <value>
		/// The graduation year.
		/// </value>

		public virtual int? GraduationYear { get; set; }

		/// <summary>
		/// Gets or sets the primary specialty.
		/// </summary>
		/// <value>
		/// The primary specialty.
		/// </value>
		public virtual string PrimarySpecialty { get; set; }

		/// <summary>
		/// Gets or sets the secondary specialty1.
		/// </summary>
		/// <value>
		/// The secondary specialty1.
		/// </value>
		public virtual string SecondarySpecialty1 { get; set; }

		/// <summary>
		/// Gets or sets the secondary specialty2.
		/// </summary>
		/// <value>
		/// The secondary specialty2.
		/// </value>
		public virtual string SecondarySpecialty2 { get; set; }

		/// <summary>
		/// Gets or sets the secondary specialty3.
		/// </summary>
		/// <value>
		/// The secondary specialty3.
		/// </value>
		public virtual string SecondarySpecialty3 { get; set; }

		/// <summary>
		/// Gets or sets the secondary specialty4.
		/// </summary>
		/// <value>
		/// The secondary specialty4.
		/// </value>
		public virtual string SecondarySpecialty4 { get; set; }

		/// <summary>
		/// Gets or sets all secondary specialties.
		/// </summary>
		/// <value>
		/// All secondary specialties.
		/// </value>
		public virtual string AllSecondarySpecialties { get; set; }

		/// <summary>
		/// Gets or sets the council board certification.
		/// </summary>
		/// <value>
		/// The council board certification.
		/// </value>
		public virtual bool? CouncilBoardCertification { get; set; }

		/// <summary>
		/// Gets or sets the accepts medicare assignment.
		/// </summary>
		/// <value>
		/// The accepts medicare assignment.
		/// </value>
		public virtual MedicalAssignmentEnum? AcceptsMedicareAssignment { get; set; }

		/// <summary>
		/// Gets or sets the participates in erx.
		/// </summary>
		/// <value>
		/// The participates in erx.
		/// </value>
		public virtual bool? ParticipatesInERX { get; set; }

		/// <summary>
		/// Gets or sets the participates in PQRS.
		/// </summary>
		/// <value>
		/// The participates in PQRS.
		/// </value>
		public virtual bool? ParticipatesInPQRS { get; set; }

		/// <summary>
		/// Gets or sets the participates in ehr.
		/// </summary>
		/// <value>
		/// The participates in ehr.
		/// </value>
		public virtual bool? ParticipatesInEHR { get; set; }
		#endregion

		#region Medical Practice Related Properties
		/// <summary>
		/// Gets or sets the group practice pac identifier.
		/// </summary>
		/// <value>
		/// The group practice pac identifier.
		/// </value>
		public virtual string GroupPracticePacId { get; set; }

		/// <summary>
		/// Gets or sets the name of the org legal.
		/// </summary>
		/// <value>
		/// The name of the org legal.
		/// </value>
		public virtual string OrgLegalName { get; set; }

		/// <summary>
		/// Gets or sets the name of the dba.
		/// </summary>
		/// <value>
		/// The name of the dba.
		/// </value>
		public virtual string DBAName { get; set; }

		/// <summary>
		/// Gets or sets the numberof group practice members.
		/// </summary>
		/// <value>
		/// The numberof group practice members.
		/// </value>
		public virtual int? NumberofGroupPracticeMembers { get; set; }
		#endregion

		#region Address Related Properties
		/// <summary>
		/// Gets or sets the line1.
		/// </summary>
		/// <value>
		/// The line1.
		/// </value>
		public virtual string Line1 { get; set; }

		/// <summary>
		/// Gets or sets the line2.
		/// </summary>
		/// <value>
		/// The line2.
		/// </value>
		public virtual string Line2 { get; set; }

		/// <summary>
		/// Gets or sets the markerof adress line2 suppression.
		/// </summary>
		/// <value>
		/// The markerof adress line2 suppression.
		/// </value>
		public virtual bool? MarkerofAdressLine2Suppression { get; set; }

		/// <summary>
		/// Gets or sets the city.
		/// </summary>
		/// <value>
		/// The city.
		/// </value>
		public virtual string City { get; set; }

		/// <summary>
		/// Gets or sets the state.
		/// </summary>
		/// <value>
		/// The state.
		/// </value>
		public virtual string State { get; set; }

		/// <summary>
		/// Gets or sets the zip code.
		/// </summary>
		/// <value>
		/// The zip code.
		/// </value>
		public virtual string ZipCode { get; set; }
		#endregion

		#region Hospital Affiliation Properties
		/// <summary>
		/// Gets or sets the hospital affiliation cc n1.
		/// </summary>
		/// <value>
		/// The hospital affiliation cc n1.
		/// </value>
		public virtual string HospitalAffiliationCCN1 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation lb n1.
		/// </summary>
		/// <value>
		/// The hospital affiliation lb n1.
		/// </value>
		public virtual string HospitalAffiliationLBN1 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation cc n2.
		/// </summary>
		/// <value>
		/// The hospital affiliation cc n2.
		/// </value>
		public virtual string HospitalAffiliationCCN2 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation lb n2.
		/// </summary>
		/// <value>
		/// The hospital affiliation lb n2.
		/// </value>
		public virtual string HospitalAffiliationLBN2 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation cc n3.
		/// </summary>
		/// <value>
		/// The hospital affiliation cc n3.
		/// </value>
		public virtual string HospitalAffiliationCCN3 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation lb n3.
		/// </summary>
		/// <value>
		/// The hospital affiliation lb n3.
		/// </value>
		public virtual string HospitalAffiliationLBN3 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation cc n4.
		/// </summary>
		/// <value>
		/// The hospital affiliation cc n4.
		/// </value>
		public virtual string HospitalAffiliationCCN4 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation lb n4.
		/// </summary>
		/// <value>
		/// The hospital affiliation lb n4.
		/// </value>
		public virtual string HospitalAffiliationLBN4 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation cc n5.
		/// </summary>
		/// <value>
		/// The hospital affiliation cc n5.
		/// </value>
		public virtual string HospitalAffiliationCCN5 { get; set; }
		/// <summary>
		/// Gets or sets the hospital affiliation lb n5.
		/// </summary>
		/// <value>
		/// The hospital affiliation lb n5.
		/// </value>
		public virtual string HospitalAffiliationLBN5 { get; set; }
		#endregion

		/// <summary>
		/// Set the flag if user edited. In this case it would only be true of user importing from custom file system, otherwise false.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is edited; otherwise, <c>false</c>.
		/// </value>
		public bool IsEdited { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		public long? Version { get; set; }
    }

	/// <summary>
	/// </summary>
	static class XHelpers
    {
		/// <summary>
		/// To the measure.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="target">The target.</param>
		/// <param name="code">The code.</param>
		/// <returns></returns>
		public static Measure ToMeasure(this XElement element, Target target, string code)
        {
            var result = new Measure(target, code);

            if (result.MeasureTitle == null) result.MeasureTitle = new MeasureTitle();

            result.MeasureTitle.Plain = element.Attribute("name").Value;
            result.MeasureTitle.Clinical = element.Attribute("name").Value;
            result.MeasureTitle.Policy = element.Attribute("name").Value;
            result.Description = element.Attribute("name").Value;

            return result;
        }
    }

	/// <summary>
	/// The NHibernate mapping for the Physician Data Model.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.Physician.Physicians.PhysicianTarget}" />
	public class PhysicianTargetMap : DatasetRecordMap<PhysicianTarget>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicianTargetMap"/> class.
		/// </summary>
		public PhysicianTargetMap()
        {
            var indexName = string.Format("IDX_{0}", typeof (PhysicianTarget).EntityTableName());
            Map(x => x.Npi).Index(indexName).Not.Nullable();
            Map(x => x.PacId).Length(17).Index(indexName);
            Map(x => x.ProfEnrollId).Length(20).Index(indexName);

            Map(x => x.FirstName).Length(50);
            Map(x => x.MiddleName).Length(50);
            Map(x => x.LastName).Length(50);
            Map(x => x.Suffix).Length(20);

            Map(x => x.Gender).Length(10);

            Map(x => x.Credential).Length(255);

            Map(x => x.MedicalSchoolName).Length(255);
            Map(x => x.GraduationYear).Nullable();

            Map(x => x.CouncilBoardCertification)
                         .CustomSqlType("bit").Default("0");

            Map(x => x.PrimarySpecialty).Length(255).Nullable();
            Map(x => x.SecondarySpecialty1).Length(255);
            Map(x => x.SecondarySpecialty2).Length(255);
            Map(x => x.SecondarySpecialty3).Length(255);
            Map(x => x.SecondarySpecialty4).Length(255);
            Map(x => x.AllSecondarySpecialties).Length(255);
            Map(x => x.AcceptsMedicareAssignment).Length(2).Nullable();
            Map(x => x.ParticipatesInERX).Nullable();
            Map(x => x.ParticipatesInPQRS).Nullable();
            Map(x => x.ParticipatesInEHR).Nullable();

            // Hospital Affiliation Related
            Map(x => x.HospitalAffiliationCCN1).Length(7).Index(indexName);
            Map(x => x.HospitalAffiliationLBN1).Length(255);
            Map(x => x.HospitalAffiliationCCN2).Length(7).Index(indexName);
            Map(x => x.HospitalAffiliationLBN2).Length(255);
            Map(x => x.HospitalAffiliationCCN3).Length(7).Index(indexName);
            Map(x => x.HospitalAffiliationLBN3).Length(255);
            Map(x => x.HospitalAffiliationCCN4).Length(7).Index(indexName);
            Map(x => x.HospitalAffiliationLBN4).Length(255);
            Map(x => x.HospitalAffiliationCCN5).Length(7).Index(indexName);
            Map(x => x.HospitalAffiliationLBN5).Length(255);

            // Medical Practice Related
            Map(x => x.GroupPracticePacId).Length(15).Nullable().Index(indexName);
            Map(x => x.OrgLegalName).Length(150).Nullable().Index(indexName + "_OLN");
            Map(x => x.DBAName).Length(150).Nullable();
            Map(x => x.NumberofGroupPracticeMembers).Nullable();

            // Addresses Related
            Map(x => x.Line1).Length(255).Not.Nullable();
            Map(x => x.Line2).Length(255).Nullable();
            Map(x => x.MarkerofAdressLine2Suppression).Nullable();

            Map(x => x.City).Length(150).Nullable();
            Map(x => x.State).Length(3).Not.Nullable().Index(indexName);
            Map(x => x.ZipCode).Length(12).Nullable();

            Map(x => x.IsEdited).CustomSqlType("bit").Default("0");
            Map(x => x.Version).Nullable();
        }

		/// <summary>
		/// Names the map.
		/// </summary>
		/// <returns></returns>
		protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}