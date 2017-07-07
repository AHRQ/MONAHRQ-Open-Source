using System;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Domain.Audits;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.NursingHomes
{
    [Serializable,
     Auditable(typeof(NursingHomeAuditLog)),
     ImplementPropertyChanged,
     EntityTableName("NursingHomes")]
    public class NursingHome : Entity<int>, ISoftDeletableOnly, ISelectable
    {
        private bool _isSelected;
        public const string MAX_STRING_LENGTH = "Please use fewer than 200 characters.";

        #region Properties

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnValueChnaged();
            }
        }

        /// <summary>
        /// Gets or sets the Name .
        /// </summary>
        /// <value>
        /// The Name .
        /// </value>
        [Required(ErrorMessage = @"Please enter the name of the nursing home.")]
        [StringLength(200, ErrorMessage = MAX_STRING_LENGTH)]
        public override string Name { get; set; }

        /// <summary>
        /// Gets or sets Description
        /// </summary>
        /// <value>
        /// The Description.
        /// </value>
        [StringLength(1000, ErrorMessage = @"Please use fewer than 1000 characters. ")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the provider id.
        /// </summary>
        /// <value>
        /// The provider number.
        /// </value>
        [Required(ErrorMessage = @"Please select or enter a valid CMS Provider ID.")]
        [StringLength(6, ErrorMessage = @"Please use fewer than 6 characters.")]
        public virtual string ProviderId { get; set; }
        /// <summary>
        /// Gets or sets the type of the facility.
        /// </summary>
        /// <value>
        /// The type of the facility.
        /// </value>
        public virtual FacilityTypeEnum? FacilityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is custom.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCustom { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [StringLength(200, ErrorMessage = MAX_STRING_LENGTH)]
        public virtual string Address { get; set; }
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [StringLength(100, ErrorMessage = @"Please use fewer than 100 characters.")]
        public virtual string City { get; set; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [Required(ErrorMessage = @"Please select a valid state.")]
        public virtual string State { get; set; }
        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>
        /// The zip.
        /// </value>
        [RegularExpression(@"(^\d{5}){1,1}", ErrorMessage = @"Cannot exceed 5 numbers.")]
        public virtual string Zip { get; set; }
        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        [RegularExpression(@"(^\d{10}){1,1}", ErrorMessage = @"Cannot exceed ten numbers.")]
        [Phone]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:##########}")]
        public virtual string Phone { get; set; }
        /// <summary>
        /// Gets or sets the county ssa.
        /// </summary>
        /// <value>
        /// The county ssa.
        /// </value>
        public virtual string CountySSA { get; set; }
        /// <summary>
        /// Gets or sets the name of the county.
        /// </summary>
        /// <value>
        /// The name of the county.
        /// </value>
        public virtual string CountyName { get; set; }
        /// <summary>
        /// Gets or sets the ownership.
        /// </summary>
        /// <value>
        /// The ownership.
        /// </value>
        [StringLength(200, ErrorMessage = MAX_STRING_LENGTH)]
        public virtual string Ownership { get; set; }
        /// <summary>
        /// Gets or sets the number cert beds. 
        /// Maps to the BEDCERT from NursingHomeCompare database
        /// </summary>
        /// <value>
        /// The number cert beds.
        /// </value>
        [Range(0, 99999, ErrorMessage = @"Please use fewer than five characters.")]
        public virtual int? NumberCertBeds { get; set; }
        /// <summary>
        /// Gets or sets the number resid cert beds.
        /// Maps to the RESTOT from NursingHomeCompare database
        /// </summary>
        /// <value>
        /// The number resid cert beds.
        /// </value>
        [Range(0, 99999, ErrorMessage = @"Please use fewer than five characters.")]
        public virtual int? NumberResidCertBeds { get; set; }
        /// <summary>
        /// Gets or sets the certification.
        /// </summary>
        /// <value>
        /// The certification.
        /// </value>
        public virtual string Certification { get; set; }

        /// <summary>
        /// Gets or sets the name of the legal business.
        /// </summary>
        /// <value>
        /// The name of the legal business.
        /// </value>
        [StringLength(200, ErrorMessage = MAX_STRING_LENGTH)]
        public virtual string LegalBusinessName { get; set; }
        /// <summary>
        /// Gets or sets the participation date.
        /// </summary>
        /// <value>
        /// The participation date.
        /// </value>
        public virtual DateTime? ParticipationDate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [in hospital].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in hospital]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool? InHospital { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [in retirement community].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in retirement community]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool? InRetirementCommunity { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [has special focus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [has special focus]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool? HasSpecialFocus { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [has special focus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [has special focus]; otherwise, <c>false</c>.
        /// </value>
        public virtual string Accreditation { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is CCRC facility.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is CCRC facility; otherwise, <c>false</c>.
        /// </value>S
        public virtual bool IsCCRCFacility { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is sf facility.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is sf facility; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSFFacility { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [changed ownership_12 mos].
        /// </summary>
        /// <value>
        /// <c>true</c> if [changed ownership_12 mos]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool ChangedOwnership_12Mos { get; set; }
        /// <summary>
        /// Gets or sets the resource fam council.
        /// </summary>
        /// <value>
        /// The resource fam council.
        /// </value>
        public virtual ResFamCouncilEnum? ResFamCouncil { get; set; }
        /// <summary>
        /// Gets or sets the sprinkler status.
        /// </summary>
        /// <value>
        /// The sprinkler status.
        /// </value>
        public virtual SprinklerStatusEnum? SprinklerStatus { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public virtual NursingHomeCategory Type { get; set; }

        /// <summary>
        /// Gets or sets the file date version.
        /// </summary>
        /// <value>
        /// The file version.
        /// </value>
        public DateTime? FileDate { get; set; }

        #endregion

        /// <summary>
        /// Cleans the before save.
        /// </summary>
        public override void CleanBeforeSave()
        {
            base.CleanBeforeSave();

            if (FileDate.HasValue && (FileDate == DateTime.MinValue || FileDate == DateTime.MaxValue))
                FileDate = null;

            if (ParticipationDate.HasValue && (ParticipationDate == DateTime.MinValue || ParticipationDate == DateTime.MaxValue))
                ParticipationDate = null;
        }

        public event EventHandler IsValueChanged;

        private void OnValueChnaged()
        {
            if (IsValueChanged != null) IsValueChanged(this, new EventArgs());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable, EntityTableName("NursingHomes_Audits")]
    public class NursingHomeAuditLog : AuditChangeLog
    {
        public string ProviderId { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="NursingHomeAuditLog"/> class.
        /// </summary>
        public NursingHomeAuditLog()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NursingHomeAuditLog"/> class.
        /// </summary>
        /// <param name="auditType">Type of the audit.</param>
        /// <param name="nursingHome">The nursing home.</param>
        public NursingHomeAuditLog(AuditType auditType, NursingHome nursingHome)
            : base(auditType, nursingHome)
        { }

        public override void InitLog(Entity<int> entity)
        {
            this.ProviderId = ((NursingHome)entity).ProviderId;
        }

    }
}