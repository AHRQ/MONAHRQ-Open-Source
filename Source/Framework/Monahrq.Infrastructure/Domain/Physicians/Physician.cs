using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Domain.Audits;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Validation;
using Monahrq.Sdk.Extensions;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Physicians
{
    [Serializable, Auditable(typeof(PhysicianAuditLog)),
     ImplementPropertyChanged, EntityTableName("Physicians")]
    public class Physician : Entity<int>, ISoftDeletableOnly
    {
        #region Error Message

        private const string CHARS_LIMIT25 = "Please use fewer than 25 characters.";
        private const string CHARS_LIMIT35 = "Please use fewer than 35 characters.";
        private const string CHARS_LIMIT3 = "Please use fewer than 3 characters.";
        private const string CHARS_LIMIT100 = "Please use fewer than 100 characters.";
        private const string CHARS_LIMIT50 = "Please use fewer than 50 characters.";
        private const string CharsLimit200 = "Please use fewer than 200 characters.";
        private const string CHARS_LIMIT70 = "Please use fewer than 70 characters.";
        private const string CHARS_LIMIT10 = "Please use fewer than 10 characters.";
        private const string CHARS_LIMIT4 = "Please use fewer than four characters.";
        private const string CHARS_LIMIT6 = "Please use fewer than six characters.";
        private const string FIRST_NAME_REQUIRED = "Please enter the first name of the physician.";
        private const string LAST_NAME_REQUIRED = "Please enter the last name of the physician.";

        #endregion

        protected override void Initialize()
        {
            base.Initialize();

            ForeignLanguages = new List<string>();
            Addresses = new List<PhysicianAddress>();
			PhysicianMedicalPractices = new List<PhysicianMedicalPractice>();
            States = new List<string>();
            AffiliatedHospitals = new List<PhysicianAffiliatedHospital>();
        }

        #region Properties

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public IList<string> States { get; set; }

        /// <summary>
        /// Gets or sets the npi.
        /// </summary>
        /// <value>
        /// The npi.
        /// </value>
        //[CustomValidation(typeof(Physician), "NpiIsUnique")]
        [UniqueConstraintCheck(DbSchemaName = "Physicians", DbColumnName = "Npi", ValidationMessage = "NPI must be unique.")]
        [Required(ErrorMessage = @"The NPI field is required")]
        [RegularExpression(@"[0-9]*", ErrorMessage = @"NPI must be number.")]
        public virtual long? Npi { get; set; }
        /// <summary>
        /// Gets or sets the pac identifier.
        /// </summary>
        /// <value>
        /// The pac identifier.
        /// </value>
        public virtual string PacId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name
        {
            get
            {
                return string.Format("{0}{1} {2}", FirstName,
                                     !string.IsNullOrEmpty(MiddleName) ? " " + MiddleName : null,
                                     LastName);
            }
            set { }
        }
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
        [Required(ErrorMessage = FIRST_NAME_REQUIRED)]
        [StringLength(25, ErrorMessage = CHARS_LIMIT25)]
        public virtual string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the middle.
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        [StringLength(25, ErrorMessage = CHARS_LIMIT25)]
        public virtual string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [Required(ErrorMessage = LAST_NAME_REQUIRED)]
        [StringLength(35, ErrorMessage = CHARS_LIMIT35)]
        public virtual string LastName { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>
        /// The suffix.
        /// </value>
        [StringLength(3, ErrorMessage = CHARS_LIMIT3)]
        public virtual string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public virtual GenderEnum? Gender { get; set; }

        /// <summary>
        /// Gets or sets the foreign languages.
        /// </summary>
        /// <value>
        /// The foreign languages.
        /// </value>
        public virtual IList<string> ForeignLanguages { get; set; }

        /// <summary>
        /// Gets or sets the addresses.
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public virtual IList<PhysicianAddress> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        /// <value>
        /// The credentials.
        /// </value>
        public virtual string Credentials { get; set; }

        /// <summary>
        /// Gets or sets the name of the medical school.
        /// </summary>
        /// <value>
        /// The name of the medical school.
        /// </value>
        [StringLength(100, ErrorMessage = CHARS_LIMIT100)]
        public virtual string MedicalSchoolName { get; set; }
        /// <summary>
        /// Gets or sets the graduation year.
        /// </summary>
        /// <value>
        /// The graduation year.
        /// </value>

        [Range(0, 9999, ErrorMessage = CHARS_LIMIT4)]
        public virtual int? GraduationYear { get; set; }

        /// <summary>
        /// Gets or sets the primary specialty.
        /// </summary>
        /// <value>
        /// The primary specialty.
        /// </value>
        [StringLength(50, ErrorMessage = CHARS_LIMIT50)]
        public virtual string PrimarySpecialty { get; set; }

        /// <summary>sta
        /// 
        /// Gets or sets the secondary specialty1.
        /// </summary>
        /// <value>
        /// The secondary specialty1.
        /// </value>
        [StringLength(50, ErrorMessage = CHARS_LIMIT50)]
        public virtual string SecondarySpecialty1 { get; set; }

        /// <summary>
        /// Gets or sets the secondary specialty2.
        /// </summary>
        /// <value>
        /// The secondary specialty2.
        /// </value>
        [StringLength(50, ErrorMessage = CHARS_LIMIT50)]
        public virtual string SecondarySpecialty2 { get; set; }

        /// <summary>
        /// Gets or sets the secondary specialty3.
        /// </summary>
        /// <value>
        /// The secondary specialty3.
        /// </value>
        [StringLength(50, ErrorMessage = CHARS_LIMIT50)]
        public virtual string SecondarySpecialty3 { get; set; }

        /// <summary>
        /// Gets or sets the secondary specialty4.
        /// </summary>
        /// <value>
        /// The secondary specialty4.
        /// </value>
        [StringLength(50, ErrorMessage = CHARS_LIMIT50)]
        public virtual string SecondarySpecialty4 { get; set; }

        /// <summary>
        /// Gets or sets all seconardspecialties.
        /// </summary>
        /// <value>
        /// All seconardspecialties.
        /// </value>
        [StringLength(200, ErrorMessage = CharsLimit200)]
        public virtual string AllSeconardspecialties
        {
            get { return string.Join(SecondarySpecialty1, SecondarySpecialty2, SecondarySpecialty3, SecondarySpecialty4); }
            //set { }
        }

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

        /// <summary>
        /// Gets or sets the practices.
        /// </summary>
        /// <value>
        /// The practices.
        /// </value>
		public virtual IList<PhysicianMedicalPractice> PhysicianMedicalPractices { get; set; }

        /// <summary>
        /// Gets or sets the physician affiliated hospitals.
        /// </summary>
        /// <value>
        /// The physician affiliated hospitals.
        /// </value>
        public virtual IList<PhysicianAffiliatedHospital> AffiliatedHospitals { get; set; }

        /// <summary>
        /// Gets or sets the city for display.
        /// </summary>
        /// <value>
        /// The city for display.
        /// </value>
        public string CityForDisplay { get; set; }

        /// <summary>
        /// Gets or sets the state for display.
        /// </summary>
        /// <value>
        /// The state for display.
        /// </value>
        public string StateForDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edited.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is edited; otherwise, <c>false</c>.
        /// </value>
        public bool IsEdited { get; set; }

        #endregion

        #region Override

        /// <summary>
        /// Cleans the before save.
        /// </summary>
        public override void CleanBeforeSave()
        {
            base.CleanBeforeSave();

            if (Addresses != null)
            {
                Addresses.ForEach(x =>
                {
                    if (x.Physician == null)
                        x.Physician = this;
                });
            }

            if (this.States == null)
                this.States = new List<string>();
                

            if (HasAddresses() && !this.States.Contains(Addresses[0].State))
                this.States.Add(Addresses[0].State);

            if (!this.States.Any() && HasMedicalPractices())
            {
				foreach (var pmp in PhysicianMedicalPractices)
                {
                    if (pmp.MedicalPractice.HasAddresses() && !this.States.Contains(pmp.MedicalPractice.Addresses[0].State))
                    {
                        this.States.Add(pmp.MedicalPractice.Addresses[0].State);
                        break;
                    }
                }
            }

            this.States = this.States.RemoveDuplicates().ToList();
        }

        public bool HasAddresses()
        {
            return Addresses != null && Addresses.Any();
        }

        public bool HasMedicalPractices()
        {
			return PhysicianMedicalPractices != null && PhysicianMedicalPractices.Any();
        }

        #endregion

        #region Custom Validation Methods
        //public static ValidationResult NpiIsUnique(int? npiValue, ValidationContext validationContext)
        //{
        //    var ownerToTest = validationContext.ObjectInstance as Entity<int>;
        //    if (ownerToTest == null || ownerToTest.IsPersisted) return ValidationResult.Success;
        //    if (!npiValue.HasValue || npiValue.Value == 0) return ValidationResult.Success;

        //    var entityType = validationContext.ObjectType;
        //    var dbSchemaName = entityType.EntityTableName();
        //    var dbColumnName = validationContext.MemberName;
        //    var validationMessage = string.Format("Property {0} must be unique across all \"{1}\" entities.", validationContext.MemberName, Inflector.Pluralize(entityType.Name));

        //    var provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
        //    int npiCount;
        //    using (var session = provider.SessionFactory.OpenStatelessSession())
        //    {
        //        var query = string.Format("select Count({1}) from [dbo].[{0}] where [{1}]={2}", dbSchemaName, dbColumnName, npiValue);
        //        npiCount = session.CreateSQLQuery(query).UniqueResult<int>();
        //    }

        //        return (npiCount > 0) ? new ValidationResult(validationMessage, new string[] { validationContext.MemberName }) : null;
        //}
        #endregion
    }

    [Serializable, Auditable(typeof(PhysicianAuditLog)),
    ImplementPropertyChanged, EntityTableName("Physicians_AffiliatedHospitals")]
    public class PhysicianAffiliatedHospital : Entity<int>, ISoftDeletableOnly
    {
        #region Properties
        /// <summary>
        /// Gets or sets the medical practice.
        /// </summary>
        /// <value>
        /// The medical practice.
        /// </value>
        public virtual string HospitalCmsProviderId { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public virtual int Index { get; set; }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable, EntityTableName("Physicians_Audits")]
    public class PhysicianAuditLog : AuditChangeLog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianAuditLog"/> class.
        /// </summary>
        public PhysicianAuditLog()
        { }

        public PhysicianAuditLog(AuditType auditType, Physician physician)
            : base(auditType, physician)
        { }

        public PhysicianAuditLog(AuditType auditType, PhysicianAddress address)
            : base(auditType, address)
        { }

        public PhysicianAuditLog(AuditType auditType, PhysicianMedicalPractice practice)
            : base(auditType, practice)
        { }

        public PhysicianAuditLog(AuditType auditType, PhysicianAffiliatedHospital affiliatedHospital)
            : base(auditType, affiliatedHospital)
        { }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }
    }

    [Serializable]
    public enum MedicalAssignmentEnum
    {
        [Description("Physician(s) accepts Medicare approved amount as payment in full")]
        Y = 0,
        [Description("Physician(s) may accept Medicare Assignment")]
        M = 1
    }
}


