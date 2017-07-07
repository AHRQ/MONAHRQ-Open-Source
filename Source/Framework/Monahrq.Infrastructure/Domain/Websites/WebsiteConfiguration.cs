using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Monahrq.Infrastructure.Domain.Websites
{
    [DataContract(Name = "Configuration")]
    public class WebsiteConfiguration
    {
        #region Constructor

        public WebsiteConfiguration()
        {
            Products = new Dictionary<string, WebsiteAudience> { { "consumer", new WebsiteAudience() }, { "professional", new WebsiteAudience() } };
        }

        #endregion

        #region Ignored DataMember

        /// <summary>
        /// Gets or sets the out put directory.
        /// </summary>
        /// <value>
        /// The out put directory.
        /// </value>
        [IgnoreDataMember]
        public virtual string OutPutDirectory { get; set; }

        //[DataMember(Name = "website_BrandColor")]
        public virtual string BrandColor { get; set; }
        //[DataMember(Name = "website_AccentColor")]
        public virtual string AccentColor { get; set; }

        #endregion

        [DataMember(Name = "active_products", Order = 0)]
        public List<string> ActiveAudiences { get; set; }

        [DataMember(Name = "default_product", Order = 1)]
        public string DefaultAudience { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [DataMember(Name = "website_BrowserTitle", Order = 2)]
        public virtual string BrowserTitle { get; set; }

        [DataMember(Name = "website_HeaderTitle", Order = 3)]
        public virtual string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>
        /// The keywords.
        /// </value>
        [DataMember(Name = "website_Keywords", Order = 4)]
        public virtual string Keywords { get; set; }

        /// <summary>
        /// Gets or sets the google analytics key.
        /// </summary>
        /// <value>
        /// The google analytics key.
        /// </value>
        [DataMember(Name = "website_GoogleAnalyticsKey", Order = 5)]
        public virtual string GoogleAnalyticsKey { get; set; }

        /// <summary>
        /// Gets or sets the google maps API key.
        /// </summary>
        /// <value>
        /// The google maps API key.
        /// </value>
        [DataMember(Name = "website_GoogleMapsApiKey", Order = 6)]
        public virtual string GoogleMapsApiKey { get; set; }

        [DataMember(Name = "website_MapquestApiKey", Order = 7)]
        public virtual string MapquestApiKey { get; set; }

        [DataMember(Name = "PHYSICIAN_API_TOKEN", Order = 8)]
        public virtual string PhysicianApiToken { get; set; }

        [DataMember(Name = "USED_REAL_TIME", Order = 9)]
        public virtual int UseRealTimeApi { get; set; }

        /// <summary>
        /// Gets or sets the geographic description.
        /// </summary>
        /// <value>
        /// The geographic description.
        /// </value>
        [DataMember(Name = "website_GeographicDescription", Order = 10)]
        public virtual string GeographicDescription { get; set; }

        /// <summary>
        /// Gets or sets the selected reporting states.
        /// </summary>
        /// <value>
        /// The selected reporting states.
        /// </value>
        [DataMember(Name = "website_States", Order = 11)]
        public virtual List<string> States { get; set; }

        /// <summary>
        /// Gets or sets the selected zip code radii.
        /// </summary>
        /// <value>
        /// The selected zip code radii.
        /// </value>
        [DataMember(Name = "website_ZipCodeRadii", Order = 12)]
        public virtual List<int> ZipCodeRadii { get; set; }

        [DataMember(Name = "HOSPITAL_OVERALL_ID", Order = 13)]
        public virtual string HospitalOverAllId { get; set; }

        [DataMember(Name = "PATIENT_EXPERIENCE_ID", Order = 14)]
        public virtual string PatientEXxperienceIDd { get; set; }

        [DataMember(Name = "NURSING_OVERALL_ID", Order = 15)]
        public virtual int? NursingOverAllId { get; set; }

        [DataMember(Name = "NURSING_OVERALL_QUALITY_ID", Order = 16)]
        public virtual int? NursingOverAllQualityId { get; set; }

        [DataMember(Name = "NURSING_OVERALL_STAFFING_ID", Order = 17)]
        public virtual int? NursingOverAllStaffingId { get; set; }

        [DataMember(Name = "NURSING_OVERALL_HEALTH_ID", Order = 18)]
        public virtual int? NursingOverAllHealthId { get; set; }

        [DataMember(Name = "SURGICALSAFETY_MEASURES", Order = 19)]
        public virtual string[] SurgicalSafetyMeasures { get; set; }

        [DataMember(Name = "REGIONAL_CONTEXT", Order = 20)]
        public virtual string RegionContext { get; set; }

        [DataMember(Name = "DE_IDENTIFICATION", Order = 21)]
        public int Deidentification { get; set; }

        [DataMember(Name = "products", Order = 22)]
        public Dictionary<string, WebsiteAudience> Products { get; set; }

        [DataMember(Name = "website-version", Order = 23)]
        public string WebsiteVersion { get; set; }

        [DataMember(Name = "iFrameVersion", Order = 24)]
        public int PublishIFrameVersion { get; set; }

        [DataMember(Name = "CompressedAndOptimized", Order = 25)]
        public int CompressedAndOptimized { get; set; }

        [DataMember(Name = "NURSING_OVERALL_FMLYRATE_ID", Order = 26)]
        public int? NuringHomeFmlyRateId { get; set; }

        [DataMember(Name = "NURSING_OVERALL_CAHPS_MEASURES", Order = 27)]
        public List<int> NHCaphsOverallMeasures { get; set; }

        [DataMember(Name = "NURSING_CAHPS_QUESTION_TYPES", Order = 28)]
        public NHCAHPSQuestionType NHCaphsQustionsType { get; set; }

        [DataMember(Name = "MEDICALPRACTICE_OVERALL_QUALITY_ID", Order = 29)]
        public int? MedicalPracticeOverallMeasureId { get; set; }

        [DataMember(Name = "MEDICALPRACTICE_OVERALL_CAHPS_MEASURES", Order = 29)]
        public List<int> MedicalPracticeOverallMeasures { get; set; }

        [DataMember(Name = "MEDICALPRACTICE_CAHPS_QUESTION_TYPES", Order = 30)]
        public CGCAHPSQuestionType MedicalPracticeQuestionTypes { get; set; }

        [DataMember(Name = "CMS_OVERALL_ID", Order = 31)]
        public int CmsOverallId { get; set; }

        [DataMember(Name = "USE_CMS_OVERALL_ID", Order = 32)]
        public int UseCmsOverallId { get; set; }
    }

    [DataContract]
    public struct NHCAHPSQuestionType
    {
        [DataMember(Name = "YNB/YNA")]
        public List<string> YesNo { get; set; }
        [DataMember(Name = "OFTEN")]
        public List<string> Often { get; set; }
        [DataMember(Name = "TIMES")]
        public List<string> Times { get; set; }
        [DataMember(Name = "RATING")]
        public List<string> Rating { get; set; }

    }

    [DataContract]
    public struct CGCAHPSQuestionType
    {
        [DataMember(Name = "YESNO")]
        public List<string> YesNo { get; set; }
        [DataMember(Name = "HOWOFN4F")]
        public List<string> Often { get; set; }
        [DataMember(Name = "DEFINITE")]
        public List<string> Times { get; set; }
        [DataMember(Name = "RATEDOCT")]
        public List<string> Rating { get; set; }
        [DataMember(Name = "AVG")]
        public List<string> Avg { get; set; }
    }
}


