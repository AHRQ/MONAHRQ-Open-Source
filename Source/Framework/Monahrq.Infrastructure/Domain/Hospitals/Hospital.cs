using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Monahrq.Infrastructure.Domain.Categories;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;
using Monahrq.Infrastructure.Generators;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals
{
    [ImplementPropertyChanged,
     Serializable]
    public class Hospital : HospitalRegistryItem, ISelectable //, IDataErrorInfo
    {
        //private Region _selectedRegion;

        /// <summary>
        /// Initializes a new instance of the <see cref="Hospital"/> class.
        /// </summary>
        public Hospital()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hospital"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public Hospital(HospitalRegistry registry)
            : base(registry)
        {
            registry.Hospitals.Add(this);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            Categories = new List<HospitalCategory>();
            RegionsListForDisplay = new ObservableCollection<Region>();
        }

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                if (ValueChanged != null) ValueChanged(this, new EventArgs());
            }
        }

        public ObservableCollection<Region> RegionsListForDisplay { get; set; }

        public List<string> CmsCollection { get; set; }

        public Region SelectedRegion { get; set; }

        // Name and Id fields are inherited
        [CSVMap(Ordinal = 3, Type = FieldType.Alphanumeric)]
        public override string Name { get; set; }


        // these fields are in the left column in DetailsView
        // LocalHospitalId is user-editable, it's not the Monahrq internal Id
        [CSVMap(Name = "DSHOSPID", Ordinal = 1)]
        [StringLength(15, ErrorMessage = @"Please use fewer than 15 characters.")]
        public virtual string LocalHospitalId { get; set; }

        // TODO: setter should save this to SQL when user edits the row in Hospital listing view

        /// <summary>
        /// Gets or sets the CMS provider identifier.
        /// </summary>
        /// <value>
        /// The CMS provider identifier.
        /// </value>
        [CSVMap(Ordinal = 7, Type = FieldType.Alphanumeric)]
        public virtual string CmsProviderID { get; set; }

        /// <summary>
        /// Gets or sets the CCR.
        /// </summary>
        /// <value>
        /// The CCR.
        /// </value>
        [CSVMap(Ordinal = 5)]
        public virtual decimal? CCR { get; set; }

        // Not imported
        /// <summary>
        /// Gets or sets the hospital ownership.
        /// </summary>
        /// <value>
        /// The hospital ownership.
        /// </value>
        [CSVMap(Ordinal = 13)]
        public virtual string HospitalOwnership { get; set; }           // this is the parent Org

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [CSVMap(Name = "FIPS", Ordinal = 2, Type = FieldType.Alphanumeric)]
        public virtual string County { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [CSVMap(Ordinal = 8, Type = FieldType.Alphanumeric)]
        public virtual string Address { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [CSVMap(Ordinal = 9)]
        public virtual string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public virtual string State { get; set; }

        //public bool IsValid
        //{
        //    get
        //    {
        //        var isValid = !string.IsNullOrEmpty(Name) &&
        //                      !string.IsNullOrEmpty(Zip) &&
        //                      State != null && 
        //                      County != null &&
        //                      LocalHospitalId <= 0;


        //        return isValid;
        //    }
        //}

        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>
        /// The zip.
        /// </value>
        [CSVMap(Ordinal = 4)]
        public virtual string Zip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [CSVMap(Ordinal = 25)]
        public virtual double? Latitude { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [CSVMap(Ordinal = 26)]
        public virtual double? Longitude { get; set; }


        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [CSVMap(Ordinal = 14)]
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the fax number.
        /// </summary>
        /// <value>
        /// The fax number.
        /// </value>
        [CSVMap(Ordinal = 15)]
        public virtual string FaxNumber { get; set; }

        // these fields are in the center column in DetailsView
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [CSVMap(Ordinal = 10)]
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the employees.
        /// </summary>
        /// <value>
        /// The employees.
        /// </value>
        [CSVMap(Ordinal = 11)]
        public virtual int? Employees { get; set; }

        /// <summary>
        /// Gets or sets the total beds.
        /// </summary>
        /// <value>
        /// The total beds.
        /// </value>
        [CSVMap(Ordinal = 12)]
        public virtual int? TotalBeds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [medicare medicaid provider].
        /// </summary>
        /// <value>
        /// <c>true</c> if [medicare medicaid provider]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 16)]
        public virtual bool MedicareMedicaidProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [emergency service].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [emergency service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 17)]
        public virtual bool EmergencyService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [trauma service].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [trauma service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 18)]
        public virtual bool TraumaService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [urgent care service].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [urgent care service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 19)]
        public virtual bool UrgentCareService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [pediatric service].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pediatric service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 20)]
        public virtual bool PediatricService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [pediatric icu service].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pediatric icu service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 21)]
        public virtual bool PediatricICUService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [cardiac catherization service].
        /// </summary>
        /// <value>
        /// <c>true</c> if [cardiac catherization service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 22)]
        public virtual bool CardiacCatherizationService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [pharmacy service].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pharmacy service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 23)]
        public virtual bool PharmacyService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [diagnostic x ray service].
        /// </summary>
        /// <value>
        /// <c>true</c> if [diagnostic x ray service]; otherwise, <c>false</c>.
        /// </value>
        [CSVMap(Ordinal = 24)]
        public virtual bool DiagnosticXRayService { get; set; }

        ///// <summary>
        ///// Gets or sets the number times edited.
        ///// </summary>
        ///// <value>
        ///// The number times edited.
        ///// </value>
        //public virtual int NumberTimesEdited { get; set; }

        ///// <summary>
        ///// Gets or sets the zip code lookup.
        ///// </summary>
        ///// <value>
        ///// The zip code lookup.
        ///// </value>
        //public virtual ZipCodeToHRRAndHSA ZipCodeToHrRandHsaLookup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is deleted].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is deleted]; otherwise, <c>false</c>.
        /// </value>
        public new bool IsDeleted { get; set; }

        #region Archive Properties and Methods
        /// <summary>
        /// Gets or sets a value indicating whether [is archived].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is archived]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the linked hospital identifier.
        /// </summary>
        /// <value>
        /// The linked hospital identifier.
        /// </value>
        public virtual int? LinkedHospitalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force local hospital identifier validation].
        /// </summary>
        /// <value>
        /// <c>true</c> if [force local hospital identifier validation]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool ForceLocalHospitalIdValidation { get; set; }

        /// <summary>
        /// Gets or sets the archive date.
        /// </summary>
        /// <value>
        /// The archive date.
        /// </value>
        public DateTime? ArchiveDate { get; set; }

        public Hospital Archive()
        {
            //int originalId = Id;
            Hospital archivedHospital = this.Clone<Hospital, int>(true);
            //archivedHospital.OriginalHospitalId = originalId;
            archivedHospital.IsArchived = true;
            archivedHospital.ArchiveDate = DateTime.Now;

            return archivedHospital;
        }
        #endregion

        // these fields are in the right-side column in DetailsView
        public virtual IList<HospitalCategory> Categories { get; set; }

        public virtual string CategoriesForDisplay
        {
            get { return Categories != null && Categories.Any() ? string.Join(",", Categories.Where(x => x != null).Select(c => c.Name)) : null; }
        }

        /// <summary>
        /// Gets or sets the hospital service area.
        /// </summary>
        /// <value>
        /// The hospital service area.
        /// </value>
        public virtual HospitalServiceArea HospitalServiceArea { get; set; }

        /// <summary>
        /// Gets or sets the custom region.
        /// </summary>
        /// <value>
        /// The custom region.
        /// </value>
        public virtual CustomRegion CustomRegion { get; set; }

        /// <summary>
        /// Gets the import region identifier.
        /// </summary>
        /// <value>
        /// The import region identifier.
        /// </value>
        [CSVMap(Name = "Region", Ordinal = 6)]
        public virtual int? ImportRegionId
        {
            get
            {
                return CustomRegion != null ? CustomRegion.ImportRegionId : null;
            }
        }

        /// <summary>
        /// Gets or sets the health referral region.
        /// </summary>
        /// <value>
        /// The health referral region.
        /// </value>
        public virtual HealthReferralRegion HealthReferralRegion { get; set; }

        /// <summary>
        /// Gets or sets the selected region.
        /// </summary>
        /// <value>
        /// The selected region.
        /// </value>
        public virtual Region GetSelectedRegion(Type regionalContextType)
        {
            if (CustomRegion != null || regionalContextType == typeof(CustomRegion))
            {
                return CustomRegion;
            }

            return regionalContextType == typeof(HospitalServiceArea)
                       ? HospitalServiceArea as Region
                       : HealthReferralRegion as Region;
        }

        /// <summary>
        /// Gets the region.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">UNSUPPORTED REGION TYPE:  + typeof(T).FullName</exception>
        public virtual T GetRegion<T>() where T : Region
        {
            if (typeof(T) == typeof(CustomRegion))
            {
                return CustomRegion as T;
            }
            if (typeof(T) == typeof(HospitalServiceArea))
            {
                return HospitalServiceArea as T;
            }
            if (typeof(T) == typeof(HealthReferralRegion))
            {
                return HealthReferralRegion as T;
            }

            throw new ArgumentException("UNSUPPORTED REGION TYPE: " + typeof(T).FullName);
        }

        //protected new string OnValidate(string columnName)
        //{
        //    //var context = new ValidationContext(this) { MemberName = columnName };

        //    //var results = new Collection<ValidationResult>();
        //    //bool isValid = Validator.TryValidateObject(this, context, results, true);

        //    //return !isValid ? results[0].ErrorMessage : null;
        //}

        public void SetHospitalRegion(Type selectedRegionType)
        {
            if (CustomRegion != null)
            {
                SelectedRegion = CustomRegion;
                return;
            }
            //var selectedRegionType = HospitalRegion.Default.SelectedRegionType;
            if (selectedRegionType == typeof(HealthReferralRegion))
                SelectedRegion = HealthReferralRegion;
            else if (selectedRegionType == typeof(HospitalServiceArea))
                SelectedRegion = HospitalServiceArea;

            if (SelectedRegion == null)
            {
                SelectedRegion = null;
            }
        }

        public override void CleanBeforeSave()
        {
            base.CleanBeforeSave();

            if (!string.IsNullOrEmpty(CmsProviderID) && CmsProviderID.Length < 6)
                CmsProviderID = CmsProviderID.PadLeft(6, '0');
        }
    }

    public class HospitalExport
    {
        public HospitalExport()
        { }

        // ReSharper disable once FunctionComplexityOverflow
        public HospitalExport(Hospital hospital) : this()
        {
            Name = !string.IsNullOrEmpty(hospital.Name)
                ? hospital.Name.Contains(",") ? string.Format("\"{0}\"", hospital.Name) : hospital.Name
                : null;
            CmsProviderID = hospital.CmsProviderID;
            LocalHospitalId = hospital.LocalHospitalId;
            CCR = hospital.CCR;
            HospitalOwnership = !string.IsNullOrEmpty(hospital.HospitalOwnership)
                ? hospital.HospitalOwnership.Contains(",") ? string.Format("\"{0}\"", hospital.HospitalOwnership) : hospital.HospitalOwnership
                : null;
            County = hospital.County;
            Address = !string.IsNullOrEmpty(hospital.Address)
                ? hospital.Address.Contains(",") ? string.Format("\"{0}\"", hospital.Address) : hospital.Address
                : null;
            City = hospital.City;
            ImportRegionId = hospital.CustomRegion != null
                ? hospital.CustomRegion.ImportRegionId
                : null;
            Zip = hospital.Zip;
            Latitude = hospital.Latitude;
            Longitude = hospital.Longitude;
            PhoneNumber = hospital.PhoneNumber;
            FaxNumber = hospital.FaxNumber;
            Description = !string.IsNullOrEmpty(hospital.Description)
                ? hospital.Description.Contains(",") ? string.Format("\"{0}\"", hospital.Description) : hospital.Description
                : null;
            Employees = hospital.Employees;
            TotalBeds = hospital.TotalBeds;
            MedicareMedicaidProvider = hospital.MedicareMedicaidProvider;
            EmergencyService = hospital.EmergencyService;
            TraumaService = hospital.TraumaService;
            UrgentCareService = hospital.UrgentCareService;
            PediatricService = hospital.PediatricService;
            PediatricIcuService = hospital.PediatricICUService;
            CardiacCatherizationService = hospital.CardiacCatherizationService;
            PharmacyService = hospital.PharmacyService;
            DiagnosticXRayService = hospital.DiagnosticXRayService;
        }

        // Name and Id fields are inherited
        [CSVMap(Ordinal = 3)]
        public string Name { get; set; }
        [CSVMap(Name = "DSHOSPID", Ordinal = 1)]
        public string LocalHospitalId { get; set; }
        [CSVMap(Ordinal = 7)]
        public string CmsProviderID { get; set; }
        [CSVMap(Ordinal = 5)]
        public decimal? CCR { get; set; }
        [CSVMap(Ordinal = 13)]
        public string HospitalOwnership { get; set; }
        [CSVMap(Name = "FIPS", Ordinal = 2)]
        public string County { get; set; }
        [CSVMap(Ordinal = 8)]
        public string Address { get; set; }
        [CSVMap(Ordinal = 9)]
        public string City { get; set; }
        [CSVMap(Name = "CustomRegionID", Ordinal = 6)]
        public int? ImportRegionId { get; set; }
        [CSVMap(Ordinal = 4)]
        public string Zip { get; set; }
        [CSVMap(Ordinal = 25)]
        public double? Latitude { get; set; }
        [CSVMap(Ordinal = 26)]
        public double? Longitude { get; set; }
        [CSVMap(Ordinal = 14)]
        public string PhoneNumber { get; set; }
        [CSVMap(Ordinal = 15)]
        public string FaxNumber { get; set; }
        [CSVMap(Ordinal = 10)]
        public string Description { get; set; }
        [CSVMap(Ordinal = 11)]
        public int? Employees { get; set; }
        [CSVMap(Ordinal = 12)]
        public int? TotalBeds { get; set; }
        [CSVMap(Ordinal = 16)]
        public bool MedicareMedicaidProvider { get; set; }
        [CSVMap(Ordinal = 17)]
        public bool EmergencyService { get; set; }
        [CSVMap(Ordinal = 18)]
        public bool TraumaService { get; set; }
        [CSVMap(Ordinal = 19)]
        public bool UrgentCareService { get; set; }
        [CSVMap(Ordinal = 20)]
        public bool PediatricService { get; set; }
        [CSVMap(Ordinal = 21)]
        public bool PediatricIcuService { get; set; }
        [CSVMap(Ordinal = 22)]
        public bool CardiacCatherizationService { get; set; }
        [CSVMap(Ordinal = 23)]
        public bool PharmacyService { get; set; }
        [CSVMap(Ordinal = 24)]
        public bool DiagnosticXRayService { get; set; }
    }
}
