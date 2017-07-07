using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Utility;
using System.Xml;
using Newtonsoft.Json;

namespace Monahrq.Infrastructure.Domain.Websites
{
    [XmlRoot(ElementName = "MonahrqWebsite")]
    public class WebsiteExport
    {
        #region Fields and Constants

        private string _menus;
        private JsonSerializerSettings _menuSettings = new JsonSerializerSettings
        {
            ContractResolver = new CustomContractResolver(new List<string> { "$id", "DataSets", "target" }),
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteExport"/> class.
        /// </summary>
        public WebsiteExport()
        {
            Audiences = new List<Audience>();
            Datasets = new List<WebsiteExportDataset>();
            Measures = new List<WebsiteExportMeasure>();
            Reports = new List<WebsiteExportReport>();
            Hospitals = new List<WebsiteExportHospital>();
            NursingHomes = new List<WebsiteExportNursingHome>();
            StateContext = new List<string>();
        }

        // ReSharper disable once FunctionComplexityOverflow
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteExport"/> class.
        /// </summary>
        /// <param name="website">The website.</param>
        public WebsiteExport(Website website)
            : this()
        {
            Id = website.Id;
            Name = website.Name;
            StateContext = website.StateContext.ToList();
            ReportingStates = website.SelectedReportingStates.ToList();
            RegionTypeContext = website.RegionTypeContext;
            CurrentStatus = website.CurrentStatus ?? WebsiteState.Initialized;
            Description = website.Description;
            ReportedYear = website.ReportedYear;
            ReportedQuarterNullable = website.ReportedQuarter;
            Audiences = website.Audiences.ToList();
            DefaultAudienceNullable = website.DefaultAudience;
            OutPutDirectory = website.OutPutDirectory;
            _menus = JsonHelper.Serialize(website.Menus.Select(x => x.Menu), _menuSettings);

            Datasets = website.Datasets.ToList().Select(ds => new WebsiteExportDataset
            {
                Id = ds.Dataset.Id,
                Name = ds.Dataset.Name,
                Type = ds.Dataset.ContentType.Name
            }).ToList();

            Measures = website.Measures.ToList().Select(m => new WebsiteExportMeasure
            {
                OrginalId = m.OriginalMeasure.Id,
                OrginalCode = m.OriginalMeasure.Name,
                OverrideId = m.OverrideMeasure != null ? m.OverrideMeasure.Id.ToString() : null,
                OverrideCode = m.OverrideMeasure != null ? m.OverrideMeasure.Name : null,
                IsSelected = m.IsSelected
            }).ToList();

            Reports = website.Reports.ToList().Select(m => new WebsiteExportReport
            {
                Id = m.Report.Id,
                RptGuid = m.Report.SourceTemplate.RptId,
                Name = m.Report.Name,
                ReportType = m.Report.ReportType,
                SelectedYears = m.SelectedYears,
                DefaultSelectedYear = m.DefaultSelectedYear
            }).ToList();


            // Settings Tab
            Hospitals = website.Hospitals.ToList().Select(h => new WebsiteExportHospital
            {
                Id = h.Hospital.Id,
                Name = h.Hospital.Name,
                ProviderId = h.Hospital.CmsProviderID,
                LocalHospitalId = h.Hospital.LocalHospitalId,
                Ccr = h.CCR
            }).ToList();

            NursingHomes = website.NursingHomes.ToList().Select(h => new WebsiteExportNursingHome
            {
                Id = h.NursingHome.Id,
                Name = h.NursingHome.Name,
                ProviderId = h.NursingHome.ProviderId
            }).ToList();

            SelectedZipCodeRadii = website.SelectedZipCodeRadii.ToList();
            GeographicDescription = website.GeographicDescription;

            AboutUsSectionSummary = website.AboutUsSectionSummary;
            AboutUsSectionText = website.AboutUsSectionText;

            CustomFeedbackFormUrl = website.CustomFeedbackFormUrl;
            FeedBackEmail = website.FeedBackEmail;
            FeedbackTopics = website.FeedbackTopics.ToList();
            IncludeFeedbackFormInYourWebsite = website.IncludeFeedbackFormInYourWebsite;

            IncludeGuideToolInYourWebsite = website.IncludeGuideToolInYourWebsite;

            GoogleAnalyticsKey = website.GoogleAnalyticsKey;
            GoogleMapsApiKey = website.GoogleMapsApiKey;

            BrowserTitle = website.BrowserTitle;
            HeaderTitle = website.HeaderTitle;
            Keywords = website.Keywords;

            var professionalTheme = website.Themes.FirstOrDefault(x => x.AudienceType == Audience.Professionals);

            if (professionalTheme != null)
            {
                SelectedTheme = professionalTheme.SelectedTheme;
                AccentColor = professionalTheme.AccentColor;
                SelectedFont = professionalTheme.SelectedFont;
                BrandColor = professionalTheme.BrandColor;
            }

            var consumerTheme = website.Themes.FirstOrDefault(x => x.AudienceType == Audience.Consumers);
            if (consumerTheme != null)
            {
                ConsumerBrandColor = consumerTheme.BrandColor;
                ConsumerSelectedTheme = professionalTheme.SelectedTheme;
                ConsumerAccentColor = professionalTheme.AccentColor;
                ConsumerSelectedFont = professionalTheme.SelectedFont;
            }

            if (website.BannerImage != null)
            {
                //Encoding.UTF8.GetString(website.BannerImage.Image),
                BannerImage = new WebsiteExportImage
                {
                    Image = website.BannerImage.Image,
                    MemeType = website.BannerImage.MemeType,
                    Path = website.BannerImage.ImagePath
                };
            }

            //if (website.HomepageContentImage != null)
            //{
            //    HomepageContentImage = new WebsiteExportImage
            //    {
            //        Image = Encoding.UTF8.GetString(website.HomepageContentImage.Image),
            //        MemeType = website.HomepageContentImage.MemeType,
            //        Path = website.HomepageContentImage.ImagePath
            //    };
            //}

            // Encoding.UTF8.GetString(website.LogoImage.Image),
            if (website.LogoImage != null)
            {
                LogoImage = new WebsiteExportImage
                {
                    Image = website.LogoImage.Image,
                    MemeType = website.LogoImage.MemeType,
                    Path = website.LogoImage.ImagePath
                };
            }
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [XmlAttribute(AttributeName = "Id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        [XmlElement("Status", Type = typeof(WebsiteState), Order = 0, Namespace = "")]
        public WebsiteState CurrentStatus { get; set; }

        /// <summary>
        /// Gets or sets the reported year.
        /// </summary>
        /// <value>
        /// The reported year.
        /// </value>
        [XmlElement("Year", Order = 1)]
        public string ReportedYear { get; set; }

        /// <summary>
        /// Gets or sets the reported quarter.
        /// </summary>
        /// <value>
        /// The reported quarter.
        /// </value>
        //[XmlIgnore]
        [XmlElement("Quarter", Order = 2, IsNullable = true)]
        public int? ReportedQuarterNullable { get; set; }

        ///// <summary>
        ///// Score db record
        ///// </summary>        
        //[XmlElement("Quarter", Order = 2,
        //    Namespace = "")]
        //public object ReportedQuarter
        //{
        //    get
        //    {
        //        return ReportedQuarterNullable;
        //    }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            ReportedQuarterNullable = null;
        //        }
        //        else if (value is int)
        //        {
        //            ReportedQuarterNullable = (int)value;
        //        }
        //        else
        //        {
        //            ReportedQuarterNullable = int.Parse(value.ToString());
        //        }
        //    }
        //}

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [XmlElement(Order = 3)]
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets the state context.
        /// </summary>
        /// <value>
        /// The state context.
        /// </value>
        [XmlArray(Order = 4)]
        [XmlArrayItem]
        public List<string> StateContext { get; set; }

        /// <summary>
        /// Gets or sets the region type context.
        /// </summary>
        /// <value>
        /// The region type context.
        /// </value>+
        [XmlElement(Order = 5)]
        public string RegionTypeContext { get; set; }

        /// <summary>
        /// Gets or sets the reporting states.
        /// </summary>
        /// <value>
        /// The reporting states.
        /// </value>
        [XmlArray(Order = 6)]
        [XmlArrayItem]
        public List<string> ReportingStates { get; set; }

        /// <summary>
        /// Gets or sets the audiences.
        /// </summary>
        /// <value>
        /// The audiences.
        /// </value>
        [XmlArray(Order = 7)]
        [XmlArrayItem(typeof(Audience))]
        public List<Audience> Audiences { get; set; }

        /// <summary>
        /// Gets or sets the default audience.
        /// </summary>
        /// <value>
        /// The default audience.
        /// </value>
        // [XmlIgnore]
        [XmlElement("DefaultAudience", Order = 8, IsNullable = true,
         Namespace = "")]
        public Audience? DefaultAudienceNullable { get; set; }

        /// <summary>
        /// Gets or sets the default audience.
        /// </summary>
        /// <value>
        /// The default audience.
        /// </value>
        //[XmlElement(Order = 8,
        //    Namespace = "")]
        //public object DefaultAudience
        //{
        //    get
        //    {
        //        return DefaultAudienceNullable;
        //    }
        //    set
        //    {
        //        if (value == null)
        //        {
        //            DefaultAudienceNullable = null;
        //        }
        //        else if (value is int)
        //        {
        //            DefaultAudienceNullable = (Audience?)value;
        //        }
        //        else
        //        {
        //            DefaultAudienceNullable = (Audience?)Enum.Parse(typeof(Audience), value.ToString());
        //        }
        //    }
        //}

        /// <summary>
        /// Gets or sets the out put directory.
        /// </summary>
        /// <value>
        /// The out put directory.
        /// </value>
        [XmlElement(Order = 9)]
        public string OutPutDirectory { get; set; }

        /// <summary>
        /// Gets or sets the selected zip code radii.
        /// </summary>
        /// <value>
        /// The selected zip code radii.
        /// </value>
        [XmlArray("ZipCodeRadii", Order = 10)]
        [XmlArrayItem(Type = typeof(int))]
        public List<int> SelectedZipCodeRadii { get; set; }

        /// <summary>
        /// Gets or sets the geographic description.
        /// </summary>
        /// <value>
        /// The geographic description.
        /// </value>
        [XmlElement(Order = 11)]
        public string GeographicDescription { get; set; }

        [XmlElement("WebsiteMenus", Order = 40)]
        public XmlCDataSection Menus
        {
            get
            {
                var doc = new XmlDocument();
                return  doc.CreateCDataSection(_menus);
            }
            set
            {
                _menus = value.Value;
            }
        }

        #region Metadata Related Properties
        /// <summary>
        /// Gets or sets the about us section summary.
        /// </summary>
        /// <value>
        /// The about us section summary.
        /// </value>
        [XmlElement(Order = 12)]
        public string AboutUsSectionSummary { get; set; }

        /// <summary>
        /// Gets or sets the about us section text.
        /// </summary>
        /// <value>
        /// The about us section text.
        /// </value>
        [XmlElement(Order = 13)]
        public string AboutUsSectionText { get; set; }

        /// <summary>
        /// Gets or sets the Feedback email address .
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        [XmlElement(Order = 14)]
        public string FeedBackEmail { get; set; }

        /// <summary>
        /// Gets or sets the custom feedback form URL.
        /// </summary>
        /// <value>
        /// The custom feedback form URL.
        /// </value>
        [XmlElement(Order = 15)]
        public string CustomFeedbackFormUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is standard feedback form.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is standard feedback form; otherwise, <c>false</c>.
        /// </value>
        [XmlElement(Order = 16)]
        public bool IsStandardFeedbackForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include feedback form in your website].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include feedback form in your website]; otherwise, <c>false</c>.
        /// </value>
        [XmlElement(Order = 17)]
        public bool IncludeFeedbackFormInYourWebsite { get; set; }

        /// <summary>
        /// Gets or sets the feedback topics.
        /// </summary>
        /// <value>
        /// The feedback topics.
        /// </value>
        [XmlArray("FeedbackTopics", Order = 18)]
        [XmlArrayItem]
        public List<string> FeedbackTopics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [Include Guide Tool In Your Website].
        /// </summary>
        /// <value>
        /// <c>true</c> if [Include Guide Tool In Your Website]; otherwise, <c>false</c>.
        /// </value>
        [XmlElement(Order = 41)]
        public bool IncludeGuideToolInYourWebsite { get; set; }

        #region SEO, Analytics and Mapping

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [XmlElement(Order = 19)]
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>
        /// The keywords.
        /// </value>
        [XmlElement(Order = 20)]
        public string Keywords { get; set; }

        /// <summary>
        /// Gets or sets the google analytics key.
        /// </summary>
        /// <value>
        /// The google analytics key.
        /// </value>
        [XmlElement(Order = 21)]
        public string GoogleAnalyticsKey { get; set; }

        /// <summary>
        /// Gets or sets the google maps API key.
        /// </summary>
        /// <value>
        /// The google maps API key.
        /// </value>
        [XmlElement(Order = 22)]
        public string GoogleMapsApiKey { get; set; }

        #endregion

        #region Site Theme Properties

        /// <summary>
        /// Gets or sets the header title.
        /// </summary>
        /// <value>
        /// The header title.
        /// </value>
        [XmlElement(Order = 23)]
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the selected theme.
        /// </summary>
        /// <value>
        /// The selected theme.
        /// </value>
        [XmlElement(Order = 24)]
        public string SelectedTheme { get; set; }

        #endregion

        #region Advanced Color Options Properties

        /// <summary>
        /// Gets or sets the color of the brand.
        /// </summary>
        /// <value>
        /// The color of the brand.
        /// </value>
        [XmlElement(Order = 25)]
        public string BrandColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the accent.
        /// </summary>
        /// <value>
        /// The color of the accent.
        /// </value>
        [XmlElement(Order = 26)]
        public string AccentColor { get; set; }

        [XmlElement(Order = 36)]
        public string ConsumerBrandColor { get; set; }

        [XmlElement(Order = 37)]
        public string ConsumerSelectedTheme { get; set; }

        [XmlElement(Order = 38)]
        public string ConsumerAccentColor { get; set; }

        [XmlElement(Order = 39)]
        public string ConsumerSelectedFont { get; set; }

        /// <summary>
        /// Gets or sets the selected font.
        /// </summary>
        /// <value>
        /// The selected font.
        /// </value>
        [XmlElement(Order = 30)]
        public string SelectedFont { get; set; }

        #endregion

        #region Logo and Images Properties

        /// <summary>
        /// Gets or sets the logo image.
        /// </summary>
        /// <value>
        /// The logo image.
        /// </value>
        [XmlElement(Type = typeof(WebsiteExportImage), Order = 27)]
        public WebsiteExportImage LogoImage { get; set; }

        /// <summary>
        /// Gets or sets the banner image.
        /// </summary>
        /// <value>
        /// The banner image.
        /// </value>
        [XmlElement(Type = typeof(WebsiteExportImage), Order = 28)]
        public WebsiteExportImage BannerImage { get; set; }

        ///// <summary>
        ///// Gets or sets the homepage content image.
        ///// </summary>
        ///// <value>
        ///// The homepage content image.
        ///// </value>
        //[XmlElement(Type = typeof(WebsiteExportImage), Order = 29)]
        //public WebsiteExportImage HomepageContentImage { get; set; }

        #endregion

        #endregion

        #region Collections

        /// <summary>
        /// Gets the datasets.
        /// </summary>
        /// <value>
        /// The datasets.
        /// </value>
        [XmlArray("Datasets", Order = 31)]
        [XmlArrayItem(Type = typeof(WebsiteExportDataset))]
        public List<WebsiteExportDataset> Datasets { get; set; }

        /// <summary>
        /// Gets the reports.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        [XmlArray("Reports", Order = 32)]
        [XmlArrayItem(Type = typeof(WebsiteExportReport))]
        public List<WebsiteExportReport> Reports { get; set; }

        /// <summary>
        /// Gets or sets the measures.
        /// </summary>
        /// <value>
        /// The measures.
        /// </value>
        [XmlArray("Measures", Order = 33)]
        [XmlArrayItem(Type = typeof(WebsiteExportMeasure))]
        public List<WebsiteExportMeasure> Measures { get; set; }

        /// <summary>
        /// Gets or sets the hospitals.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        [XmlArray("Hospitals", Order = 34)]
        [XmlArrayItem(Type = typeof(WebsiteExportHospital))]
        public List<WebsiteExportHospital> Hospitals { get; set; }

        /// <summary>
        /// Gets or sets the Nursing Homes.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        [XmlArray("NursingHomes", Order = 35)]
        [XmlArrayItem(Type = typeof(WebsiteExportNursingHome))]
        public List<WebsiteExportNursingHome> NursingHomes { get; set; }
        #endregion
    }

    [Serializable]
    public class WebsiteExportDataset
    {
        [XmlAttribute]
        public int Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
    }

    [Serializable]
    public class WebsiteExportMeasure
    {
        [XmlAttribute]
        public int OrginalId { get; set; }
        [XmlAttribute]
        public string OrginalCode { get; set; }
        [XmlAttribute]
        public string OverrideId { get; set; }
        [XmlAttribute]
        public string OverrideCode { get; set; }
        [XmlAttribute]
        public bool IsSelected { get; set; }

    }

    [Serializable]
    public class WebsiteExportReport
    {
        [XmlAttribute]
        public int Id { get; set; }
        [XmlAttribute]
        public string RptGuid { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ReportType { get; set; }
        [XmlAttribute]
        public bool IsCustom { get; set; }
        [XmlArray(Order = 0)]
        [XmlArrayItem(Type = typeof(TrendingYear))]
        public virtual List<TrendingYear> SelectedYears { get; set; }
        [XmlAttribute]
        public string DefaultSelectedYear { get; set; }
    }

    [Serializable]
    public class WebsiteExportHospital
    {
        [XmlAttribute]
        public int Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ProviderId { get; set; }
        [XmlAttribute]
        public string LocalHospitalId { get; set; }
        [XmlAttribute]
        public string Ccr { get; set; }
    }

    [Serializable]
    public class WebsiteExportNursingHome
    {
        [XmlAttribute]
        public int Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ProviderId { get; set; }
    }

    [Serializable]
    public class WebsiteExportImage
    {
        [XmlElement(Order = 0)]
        public byte[] Image { get; set; }
        [XmlAttribute]
        public string MemeType { get; set; }
        [XmlAttribute]
        public string Path { get; set; }
    }
}
