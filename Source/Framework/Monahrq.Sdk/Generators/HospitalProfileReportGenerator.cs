using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The report generator to generate the hospital profile report Json data files.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    /// <seealso cref="Monahrq.Sdk.Generators.IReportGenerator" />
    [Export("HospitalProfileReportGenerator", typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(
		new[] { "2AAF7FBA-7102-4C66-8598-A70597E2F82B" },
		new string[] {},
		new[] { typeof(object) },
		1)]
    public class HospitalProfileReportGenerator : BaseReportGenerator, IReportGenerator
    {
        private string _baseDataDir;
        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Something seriously went wrong. The reporting website can not be null.</exception>
        protected override bool LoadReportData()
        {
            try
            {
                if (CurrentWebsite == null)
                    throw new InvalidOperationException("Something seriously went wrong. The reporting website can not be null.");

                // Make sure the base directories are created.
                _baseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
                if (!Directory.Exists(_baseDataDir)) Directory.CreateDirectory(_baseDataDir);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error loading data for report {0}", this.ActiveReport.Name);
                return false;
            }
        }

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
        {
            try
            {
                // Output CMS-OVERALL-STAR Measure
                var measureDirectoryPath = Path.Combine(_baseDataDir, "QualityMeasures");

                if(CurrentWebsite.Measures.Any(wr => wr.ReportMeasure.Name.EqualsIgnoreCase("CMS-­OVERALL-STAR")))
                {
                    var cmsOverallMeasure = CurrentWebsite.Measures.FirstOrDefault(wr => wr.ReportMeasure.Name.EqualsIgnoreCase("CMS-­OVERALL-STAR"));

                    if(cmsOverallMeasure != null)
                    {
                        var measureDto = new
                        {
                            MeasureID = cmsOverallMeasure.ReportMeasure.Id,
                            MeasuresName = cmsOverallMeasure.ReportMeasure.Name.Replace("--","-"),
                            MeasureSource = cmsOverallMeasure.ReportMeasure.Source,
                            MeasureType = cmsOverallMeasure.ReportMeasure.MeasureType,
                            HigherScoresAreBetter = cmsOverallMeasure.ReportMeasure.HigherScoresAreBetter,
                            HigherScoresAreBetterDescription = string.Empty,
                            TopicsID = string.Empty,
                            NatLabel = string.Empty,
                            NatRateAndCI = string.Empty,
                            NatTop10Label = string.Empty,
                            NatTop10 = string.Empty,
                            PeerLabel = string.Empty,
                            PeerRateAndCI = string.Empty,
                            PeerTop10Label = string.Empty,
                            PeerTop10 = string.Empty,
                            Footnote = string.Empty,
                            BarHeader = string.Empty,
                            BarFooter = string.Empty,
                            ColDesc1 = string.Empty,
                            ColDesc2 = string.Empty,
                            ColDesc3 = string.Empty,
                            ColDesc4 = string.Empty,
                            ColDesc5 = string.Empty,
                            ColDesc6 = string.Empty,
                            ColDesc7 = string.Empty,
                            ColDesc8 = string.Empty,
                            ColDesc9 = string.Empty,
                            ColDesc10 = string.Empty,
                            NatCol1 = string.Empty,
                            NatCol2 = string.Empty,
                            NatCol3 = string.Empty,
                            NatCol4 = string.Empty,
                            NatCol5 = string.Empty,
                            NatCol6 = string.Empty,
                            NatCol7 = string.Empty,
                            NatCol8 = string.Empty,
                            NatCol9 = string.Empty,
                            NatCol10 = string.Empty,
                            PeerCol1 = string.Empty,
                            PeerCol2 = string.Empty,
                            PeerCol3 = string.Empty,
                            PeerCol4 = string.Empty,
                            PeerCol5 = string.Empty,
                            PeerCol6 = string.Empty,
                            PeerCol7 = string.Empty,
                            PeerCol8 = string.Empty,
                            PeerCol9 = string.Empty,
                            PeerCol10 = string.Empty,
                            SelectedTitle = cmsOverallMeasure.ReportMeasure.MeasureTitle.Selected == Infrastructure.Entities.Domain.Measures.SelectedMeasuretitleEnum.Plain ? cmsOverallMeasure.ReportMeasure.MeasureTitle.Plain : cmsOverallMeasure.ReportMeasure.MeasureTitle.Clinical,
                            PlainTitle = cmsOverallMeasure.ReportMeasure.MeasureTitle.Plain,
                            ClinicalTitle = cmsOverallMeasure.ReportMeasure.MeasureTitle.Clinical,
                            MeasureDescription = cmsOverallMeasure.ReportMeasure.Description,
                            PlainTitleConsumer = cmsOverallMeasure.ReportMeasure.ConsumerPlainTitle,
                            SelectedTitleConsumer = cmsOverallMeasure.ReportMeasure.ConsumerPlainTitle,
                            MeasureDescriptionConsumer = cmsOverallMeasure.ReportMeasure.ConsumerDescription,
                            Bullets = string.Empty,
                            StatisticsAvailable = string.Empty,
                            MoreInformation = string.Empty,
                            URL = string.Empty,
                            URLTitle = string.Empty,
                            DataSourceURL = "http://www.hospitalcompare.hhs.gov/",
                            DataSourceURLTitle = "CMS Hospital Compare",
                            SupportsCost = false

                        };
                        var fileName = Path.Combine(_baseDataDir, "QualityMeasures", string.Format("Measure_{0}.js", measureDto.MeasureID));

                        if (File.Exists(fileName))
                            File.Delete(fileName);

                        GenerateJsonFile(measureDto, fileName, string.Format("$.monahrq.qualitydata.measuredescription_{0}=", measureDto.MeasureID));
                    }
                }

                // Output the hospital profiles files
				var hospitalProfiles = GetHospitalProfiles();

                if (hospitalProfiles.Any())
				{
					foreach (var hospital in hospitalProfiles.Distinct().ToList())
					{
						var fileName = Path.Combine(_baseDataDir, "hospitals", string.Format("Hospital_{0}.js", hospital.Id));

						if (File.Exists(fileName))
							File.Delete(fileName);

						GenerateJsonFile(hospital, fileName, "$.monahrq.hospitalProfile=");
					}
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error generating data file for report {0}", this.ActiveReport.Name);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the hospital profiles.
        /// </summary>
        /// <returns></returns>
        private IList<HospitalProfile> GetHospitalProfiles()
        {
			IList<HospitalProfile> hospitalProfiles = new List<HospitalProfile>();
			List<int> applicableHospitals = HospitalIds.Rows.Cast<DataRow>()
                                                       .Select(row => int.Parse(row[0].ToString())).ToList();
			List<int> applicableDatasets = CurrentWebsite.Datasets
				.Where(ds => ds.Dataset.ContentType.Name.EqualsIgnoreCase("Hospital Compare Data"))
				.Select(ds => ds.Dataset.Id).ToList();
			applicableDatasets.Add(-1);

			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				var sqlText = @"	
						select			h.*
									,	h.State as '[State]'		--	because State is used as the Column (and Property) name and is also an SQL reserved name,
																	--	it seems NHibernate looks for State via text '[State]'
									,	hctOP25.Sample as SampleOP25
									,	hctSMSS.Sample as SampleSMSS
									,	hctSTAR.Sample as SampleSTAR
						from			Hospitals h
							left join	Targets_HospitalCompareTargets hctOP25
											on	hctOP25.CMSProviderID = h.CmsProviderID
											and	hctOP25.MeasureCode = 'OP-25'
											and	hctOP25.Dataset_Id in (:applicableDatasets)
							left join	Targets_HospitalCompareTargets hctSMSS
											on	hctSMSS.CMSProviderID = h.CmsProviderID
											and	hctSMSS.MeasureCode = 'SM-SS-CHECK'
											and	hctSMSS.Dataset_Id in (:applicableDatasets)
							left join	Targets_HospitalCompareTargets hctSTAR
											on	hctSTAR.CMSProviderID = h.CmsProviderID
											and	hctSTAR.MeasureCode = 'CMS-OVERALL-STAR'
											and	hctSTAR.Dataset_Id in (:applicableDatasets)
						where			h.Id in (:applicableHospitalIds)";

				var query = session.CreateSQLQuery(sqlText);
				query.AddEntity("h", typeof(Hospital));
				query.AddScalar("SampleOP25", NHibernate.NHibernateUtil.Int32);
				query.AddScalar("SampleSMSS", NHibernate.NHibernateUtil.Int32);
				query.AddScalar("SampleSTAR", NHibernate.NHibernateUtil.Int32);
                query.SetParameterList("applicableHospitalIds", applicableHospitals);
				query.SetParameterList("applicableDatasets", applicableDatasets);

				var hospitalDTOs = query
					.List<object[]>()
					.Select(row => new
					{
						Hospital = (Hospital) row[0],
						HospitalUsesASafeSurgeryChecklist = row[1] == null ? (bool?)null : (bool?) row[1].Equals(1),
						HospitalUsesASafeSurgeryChecklistInpatient = row[2] == null ? (bool?)null : (bool?) row[2].Equals(1),
						HospitalCmsOverAllStarRating = row[3] == null ? 0 : Convert.ToInt32(row[3])
					}).ToList();

				if (hospitalDTOs != null && hospitalDTOs.Any())
				{
					foreach (var hospitalDTO in hospitalDTOs)
					{ 
						var profile = new HospitalProfile(hospitalDTO.Hospital);
						profile.HospitalUsesASafeSurgeryChecklist = hospitalDTO.HospitalUsesASafeSurgeryChecklist;
						profile.HospitalUsesASafeSurgeryChecklistInpatient = hospitalDTO.HospitalUsesASafeSurgeryChecklistInpatient;
						profile.HospitalCmsOverAllStarRating = hospitalDTO.HospitalCmsOverAllStarRating;
                        hospitalProfiles.Add(profile);
					};
				}
			}

            return hospitalProfiles;
        }

        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator()
        {
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Hospital Profile reports" });

        }

        /// <summary>
        /// Validates the report(S) dependencies needed to generate the report.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns></returns>
        public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            if (base.ValidateDependencies(website, validationResults))
            {
                //var profileRpt = website.Reports.FirstOrDefault(r => r.Report.SourceTemplate.Name.EqualsIgnoreCase("Hospital Profile Report"));

                if (website.Hospitals == null || !website.Hospitals.Any())
                {
                    validationResults.Add(new ValidationResult("The hospital profile base data was not generated due to no hospitals selected for website \"" + CurrentWebsite.Name + "\""));
                }
            }

            return validationResults == null || validationResults.Count == 0;
        }
    }

    [DataContract(Name = "")]
    internal class HospitalProfile
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalProfile"/> class.
        /// </summary>
        internal HospitalProfile() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalProfile"/> class.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        internal HospitalProfile(Hospital hospital)
			: this()
		{
			Id = hospital.Id;
			ProviderId = hospital.CmsProviderID;
			Name = !string.IsNullOrEmpty(hospital.Name) ? hospital.Name : "N/A";
			Address = hospital.Address;
			City = hospital.City;
			State = hospital.State;
			Zip = hospital.Zip;
			TotalBeds = hospital.TotalBeds.HasValue ? hospital.TotalBeds.ToString() : "N/A";
			TotalEmployees = hospital.Employees.HasValue && hospital.Employees.Value != 0 ? hospital.Employees.ToString() : "N/A";
			Description = hospital.Description;
			PhoneNumber = !string.IsNullOrEmpty(hospital.PhoneNumber) ? hospital.PhoneNumber : "N/A";
			FaxNumber = !string.IsNullOrEmpty(hospital.FaxNumber) ? hospital.FaxNumber : "N/A";
			LatLng = new Double[] { hospital.Latitude.AsDouble(0), hospital.Longitude.AsDouble(0) };

			if (hospital.Categories != null && hospital.Categories.Any())
				CategoryTypes = hospital.Categories.Select(hc => new HospitalCategoryType
				{
					Id = hc.Id,
					Name = hc.Name
				}).ToList();

			ParentOrganizationName = hospital.HospitalOwnership;
			MedicareMedicaidProvider = hospital.MedicareMedicaidProvider;
			EmergencyService = hospital.EmergencyService;
			TraumaService = hospital.TraumaService;
			UrgentCareService = hospital.UrgentCareService;
			PediatricService = hospital.PediatricService;
			PediatricICUService = hospital.PediatricICUService;
			CardiacCatherizationService = hospital.CardiacCatherizationService;
			PharmacyService = hospital.PharmacyService;
			DiagnosticXRayService = hospital.DiagnosticXRayService;
		}

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the provider identifier.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        [DataMember(Name = "HospitalProviderID")]
        public string ProviderId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [DataMember(Name = "address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [DataMember(Name = "city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [DataMember(Name = "state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>
        /// The zip.
        /// </value>
        [DataMember(Name = "zip")]
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the lat LNG.
        /// </summary>
        /// <value>
        /// The lat LNG.
        /// </value>
        [DataMember(Name = "LatLng")]
		public double[] LatLng { get; set; }

        /// <summary>
        /// Gets the display address.
        /// </summary>
        /// <value>
        /// The display address.
        /// </value>
        [DataMember(Name = "diaplayAddress")]
        public string DisplayAddress
        {
            get
            {
                var address = string.Empty;
                if (string.IsNullOrEmpty(Address) && string.IsNullOrEmpty(City) && string.IsNullOrEmpty(State) &&
                    string.IsNullOrEmpty(Zip))
                {
                    address = "N/A";
                }
                else if (!string.IsNullOrEmpty(Address) && !string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(State) &&
                    !string.IsNullOrEmpty(Zip))
                {
                    address = Address + System.Environment.NewLine;
                    address += City + ", " + State + " " + Zip;
                }
                else if (!string.IsNullOrEmpty(City) && string.IsNullOrEmpty(State) &&
                    string.IsNullOrEmpty(Zip))
                {
                    address += City + ", " + State + " " + Zip;
                }
                else
                {
                    address = "N/A";
                }

                return address;
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the total beds.
        /// </summary>
        /// <value>
        /// The total beds.
        /// </value>
        [DataMember(Name = "totalBeds")]
        public string TotalBeds { get; set; }

        /// <summary>
        /// Gets or sets the total employees.
        /// </summary>
        /// <value>
        /// The total employees.
        /// </value>
        [DataMember(Name = "totalEmployees")]
        public string TotalEmployees { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [DataMember(Name = "phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the fax number.
        /// </summary>
        /// <value>
        /// The fax number.
        /// </value>
        [DataMember(Name = "faxNumber")]
        public string FaxNumber { get; set; }

        /// <summary>
        /// Gets or sets the category types.
        /// </summary>
        /// <value>
        /// The category types.
        /// </value>
        [DataMember(Name = "types")]
        public List<HospitalCategoryType> CategoryTypes { get; set; }

        /// <summary>
        /// Gets or sets the name of the parent organization.
        /// </summary>
        /// <value>
        /// The name of the parent organization.
        /// </value>
        [DataMember(Name = "parentOrganizationName")]
        public string ParentOrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the medicare medicaid provider.
        /// </summary>
        /// <value>
        /// The medicare medicaid provider.
        /// </value>
        [DataMember(Name = "medicareMedicaidProvider")]
        public bool? MedicareMedicaidProvider { get; set; }

        /// <summary>
        /// Gets or sets the emergency service.
        /// </summary>
        /// <value>
        /// The emergency service.
        /// </value>
        [DataMember(Name = "emergencyService")]
        public bool? EmergencyService { get; set; }

        /// <summary>
        /// Gets or sets the trauma service.
        /// </summary>
        /// <value>
        /// The trauma service.
        /// </value>
        [DataMember(Name = "traumaService")]
        public bool? TraumaService { get; set; }

        /// <summary>
        /// Gets or sets the urgent care service.
        /// </summary>
        /// <value>
        /// The urgent care service.
        /// </value>
        [DataMember(Name = "urgentCareService")]
        public bool? UrgentCareService { get; set; }

        /// <summary>
        /// Gets or sets the pediatric service.
        /// </summary>
        /// <value>
        /// The pediatric service.
        /// </value>
        [DataMember(Name = "pediatricService")]
        public bool? PediatricService { get; set; }

        /// <summary>
        /// Gets or sets the pediatric icu service.
        /// </summary>
        /// <value>
        /// The pediatric icu service.
        /// </value>
        [DataMember(Name = "pediatricICUService")]
        public bool? PediatricICUService { get; set; }

        /// <summary>
        /// Gets or sets the cardiac catherization service.
        /// </summary>
        /// <value>
        /// The cardiac catherization service.
        /// </value>
        [DataMember(Name = "cardiacCatherizationService")]
        public bool? CardiacCatherizationService { get; set; }

        /// <summary>
        /// Gets or sets the pharmacy service.
        /// </summary>
        /// <value>
        /// The pharmacy service.
        /// </value>
        [DataMember(Name = "pharmacyService")]
        public bool? PharmacyService { get; set; }

        /// <summary>
        /// Gets or sets the diagnostic x ray service.
        /// </summary>
        /// <value>
        /// The diagnostic x ray service.
        /// </value>
        [DataMember(Name = "diagnosticXRayService")]
        public bool? DiagnosticXRayService { get; set; }

        /// <summary>
        /// Gets or sets the gmap embed URL.
        /// </summary>
        /// <value>
        /// The gmap embed URL.
        /// </value>
        [DataMember(Name = "gmapEmbedUrl")]
        public string GmapEmbedUrl { get; set; }

        /// <summary>
        /// Gets or sets the gmap link URL.
        /// </summary>
        /// <value>
        /// The gmap link URL.
        /// </value>
        [DataMember(Name = "gmapLinkUrl")]
		public string GmapLinkUrl { get; set; }

        /// <summary>
        /// Gets or sets the hospital uses a safe surgery checklist.
        /// </summary>
        /// <value>
        /// The hospital uses a safe surgery checklist.
        /// </value>
        [DataMember(Name = "HospitalUsesASafeSurgeryChecklist")]
		public bool? HospitalUsesASafeSurgeryChecklist { get; set; }

        /// <summary>
        /// Gets or sets the hospital uses a safe surgery checklist inpatient.
        /// </summary>
        /// <value>
        /// The hospital uses a safe surgery checklist inpatient.
        /// </value>
        [DataMember(Name = "HospitalUsesASafeSurgeryChecklistInpatient")]
		public bool? HospitalUsesASafeSurgeryChecklistInpatient { get; set; }


        /// <summary>
        /// Gets or sets the hospital CMS over all star rating.
        /// </summary>
        /// <value>
        /// The hospital CMS over all star rating.
        /// </value>
        [DataMember(Name = "CMSOverallNationalRating")]
        public int HospitalCmsOverAllStarRating { get; set; }
    }
}
