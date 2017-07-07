using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using PropertyChanged;
using System.ComponentModel;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Domain.BaseData;

namespace Monahrq.Infrastructure.Domain.Websites
{
    [ImplementPropertyChanged]
    public class Website : Entity<int> //, INotifyDataErrorInfo
    {
        #region Fields & Constants
        public static int[] ApplicableZipCodeRadii = { 1, 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 80, 90, 100, 150, 200 };
        private List<string> _selectedReportingStates;
        private List<int> _selectedZipCodeRadii;
        private List<string> _feedbackTopics;
        private string _geographicDescription;

        private XmlDocument _selectedReportingStatesXml;
        private XmlDocument _selectedZipCodeRadiiXml;
        private XmlDocument _feedbackTopicsXml;
        private IList<string> _stateContext;
        private const string WEBSITE_THEME_NAME_FORMAT = "Theme for {0}";
        public Expression<Func<YesNo, bool>> YesNoExcludeClause = o => o.Value != 0 && o.Value < 90;
        public Expression<Func<HowOften, bool>> HowOftenExcludeClause = o => o.Value != 0 && o.Value < 90;
        public Expression<Func<Definite, bool>> DefiniteExcludeClause = o => o.Value != 0 && o.Value < 90;
        public Expression<Func<Ratings, bool>> RatingExcludeClause = o => o.Value < 90;
        public Expression<Func<NumberOfTimes2, bool>> Times2ExcludeClause = o => o.Value != 0 && o.Value < 90;


        // private const string WEBSITE_METADATA_NAME_FORMAT = "Metadata for {0}";

        public static string[] ApplicableSiteFonts = {
                                            "'Droid Sans', Arial, sans-serif",
                                            "Courier New, monospace",
                                            "Arial, Helvetica, sans-serif",
                                            "Impact, Charcoal, sans-serif",
                                            "Lucida Console, Monaco, monospace",
                                            "Lucida Sans Unicode, Lucida Grande, sans-serif",
                                            "Palatino Linotype, Book Antiqua, Palatino, serif",
                                            "Tahoma, Geneva, sans-serif",
                                            "Times New Roman, Times, serif",
                                            "Trebuchet MS, sans-serif",
                                            "Verdana, Geneva, sans-serif"

                                                     };
        //public enum WebsiteDefaultAudience
        //{
        //    //NotSelected,
        //    [Description("Consumers")]
        //    Consumers,
        //    [Description("Healthcare Professionals")]
        //    Professionals

        //}
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Website"/> class.
        /// </summary>
        public Website()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Website"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="year">The year.</param>
        /// <param name="quarter">The quarter.</param>
        /// <param name="audience">The audience.</param>
        /// <param name="status">The status.</param>
        public Website(string name, string description, string year, int? quarter, WebsiteState status)
            : this()
        {
            Name = name;
            Description = description;
            ReportedYear = year;
            ReportedQuarter = quarter;
            //Audience = audience;
            if (Audiences == null) Audiences = new List<Audience>();
            CurrentStatus = status;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            ActivityLog = new List<ActivityLogEntry>();
            Datasets = new List<WebsiteDataset>();
            Measures = new List<WebsiteMeasure>();
            Reports = new List<WebsiteReport>();
            Hospitals = new List<WebsiteHospital>();
            NursingHomes = new List<WebsiteNursingHome>();
            WebsitePages = new List<WebsitePage>();
            StateContext = new List<string>();
            Themes = new List<WebsiteTheme>();
            Menus = new List<WebsiteMenu>();
        }

        private void ResetDefaultAudience()
        {
            //DefaultAudience is only required when multiple Audiences are targeted
            if (Audiences.Count < 2) DefaultAudience = null;
            //Set DefaultAudience to Consumers by default when multiple audiences selected
            if (Audiences.Count > 1 && DefaultAudience == null) DefaultAudience = Audience.Consumers;
        }


        public override void CleanBeforeSave()
        {
            base.CleanBeforeSave();

            if ((SelectedReportingStates == null || !SelectedReportingStates.Any()) && StateContext.Any())
            {
                if (SelectedReportingStates == null) SelectedReportingStates = new List<string>();


                StateContext.ForEach(s =>
                {
                    if (SelectedReportingStates.All(rs => !rs.EqualsIgnoreCase(s)))
                        SelectedReportingStates.Add(s);
                });
            }

            // Handle Reports null values to force index reordering
            //CleanUpCollectionItems();

            // ****** Status processing for website ******

            // Check status is greater than CompletedDependencyCheck return.
            if ((int?)CurrentStatus >= (int?)WebsiteState.CompletedDependencyCheck)
                return;

            if ((Reports != null && Reports.Any()) && (Measures != null && Measures.Any()) && Datasets.Any())
                CurrentStatus = WebsiteState.Initialized;

            // Check status should be set to HasReports.
            if (CurrentStatus == WebsiteState.HasReports || (Reports != null && Reports.Any()))
            {
                CurrentStatus = WebsiteState.HasReports;
                return;
            }

            // Check status should be set to HasMeasures.
            if (CurrentStatus == WebsiteState.HasMeasures || (Measures != null && Measures.Any()))
            {
                CurrentStatus = WebsiteState.HasMeasures;
                return;
            }

            // Check status should be set to HasDatasources.
            if (CurrentStatus == WebsiteState.HasDatasources || !Datasets.Any()) return;
            CurrentStatus = WebsiteState.HasDatasources;
        }

        public static ValidationResult IsAudienceSelected(object value, ValidationContext context)
        {
            var model = context.ObjectInstance as Website ?? new Website();
            if (!model.HasConsumersAudience && !model.HasProfessionalsAudience)
            {
                var result = new ValidationResult("Please select a Target Audience.", new List<string> { "HasConsumersAudience" });
                return result;
            }

            return ValidationResult.Success;
        }

        public static ValidationResult IsDefaultAudienceSelected(object value, ValidationContext context)
        {
            var model = context.ObjectInstance as Website ?? new Website();
            if (model.Audiences.Count > 1 && model.DefaultAudience == null)
            {
                var result = new ValidationResult("Please select a Default Target Audience.", new List<string> { "DefaultAudience" });
                return result;
            }

            return ValidationResult.Success;
        }

        #endregion

        #region Properties

        #region Overview 

        /// <summary>
        /// Gets or sets the state context.
        /// </summary>
        /// <value>
        /// The state context.
        /// </value>
        public virtual IList<string> StateContext
        {
            get { return _stateContext; }
            set
            {
                _stateContext = value;

                if (!SelectedReportingStates.Any())
                    _stateContext.ForEach(s =>
                    {
                        if (SelectedReportingStates.All(rs => !rs.EqualsIgnoreCase(s)))
                            SelectedReportingStates.Add(s);
                    });
            }
        }

        /// <summary>
        /// Gets or sets the region type context.
        /// </summary>
        /// <value>
        /// The region type context.
        /// </value>
        public virtual string RegionTypeContext { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the reported year.
        /// </summary>
        /// <value>
        /// The reported year.
        /// </value>
        [Required(ErrorMessage = @"Please select a reporting year.")]
        public virtual string ReportedYear { get; set; }

        /// <summary>
        /// Gets or sets the reported quarter.
        /// </summary>
        /// <value>
        /// The reported quarter.
        /// </value>
        public virtual int? ReportedQuarter { get; set; }

        public virtual List<Audience> Audiences { get; set; }

        public virtual bool DefaultAudienceIsRequired
        {
            get { return HasProfessionalsAudience && HasConsumersAudience; }
        }

        [CustomValidation(typeof(Website), "IsDefaultAudienceSelected")]
        public virtual Audience? DefaultAudience { get; set; }

        #endregion

        #region Settings

        #region Content

        /// <summary>
        /// Gets or sets the about us section summary.
        /// </summary>
        /// <value>
        /// The about us section summary.
        /// </value>
        public virtual string AboutUsSectionSummary { get; set; }

        /// <summary>
        /// Gets or sets the about us section text.
        /// </summary>
        /// <value>
        /// The about us section text.
        /// </value>
        public virtual string AboutUsSectionText { get; set; }

        /// <summary>
        /// Gets or sets the selected reporting states.
        /// </summary>
        /// <value>
        /// The selected reporting states.
        /// </value>
        public virtual List<string> SelectedReportingStates
        {
            get
            {
                if (_selectedReportingStatesXml != null && _selectedReportingStates == null)
                {
                    _selectedReportingStates = _selectedReportingStatesXml.ConvertFromXml<List<string>>();
                }

                return _selectedReportingStates ?? new List<string>();
            }
            set
            {
                _selectedReportingStates = value;

                if (_selectedReportingStates != null)
                {
                    if (_selectedReportingStatesXml == null)
                    {
                        _selectedReportingStatesXml = new XmlDocument();
                    }

                    _selectedReportingStatesXml = _selectedReportingStates.ConvertToXml(true);
                }
                else
                {
                    _selectedReportingStatesXml = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected zip code radii.
        /// </summary>
        /// <value>
        /// The selected zip code radii.
        /// </value>
        public virtual List<int> SelectedZipCodeRadii
        {
            get
            {
                if (_selectedZipCodeRadiiXml != null && (_selectedZipCodeRadii == null || !_selectedZipCodeRadii.Any()))
                {
                    _selectedZipCodeRadii = _selectedZipCodeRadiiXml.ConvertFromXml<List<int>>();
                }

                return _selectedZipCodeRadii ?? new List<int>();
            }
            set
            {
                _selectedZipCodeRadii = value;

                if (_selectedZipCodeRadii != null && _selectedZipCodeRadii.Any())
                {
                    if (_selectedZipCodeRadiiXml == null)
                        _selectedZipCodeRadiiXml = new XmlDocument();

                    _selectedZipCodeRadiiXml = _selectedZipCodeRadii.ConvertToXml(true);
                }
                else
                {
                    _selectedZipCodeRadiiXml = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the geographic description.
        /// </summary>
        /// <value>
        /// The geographic description.
        /// </value>
        [Required(ErrorMessage = @"Please provide a geographic description.", AllowEmptyStrings = false)]
        public virtual string GeographicDescription
        {
            get { return _geographicDescription; }
            set
            {
                _geographicDescription = value;
                //Validate();
            }
        }

        /// <summary>
        /// Gets or sets the feedback topics.
        /// </summary>
        /// <value>
        /// The feedback topics.
        /// </value>
        public virtual List<string> FeedbackTopics
        {
            get
            {
                if (_feedbackTopicsXml != null && _feedbackTopics == null)
                {
                    _feedbackTopics = _feedbackTopicsXml.ConvertFromXml<List<string>>();
                }

                return _feedbackTopics ?? new List<string>();
            }
            set
            {
                _feedbackTopics = value;

                if (_feedbackTopics != null)
                {
                    if (_feedbackTopicsXml == null)
                    {
                        _feedbackTopicsXml = new XmlDocument();
                    }

                    _feedbackTopicsXml = _feedbackTopics.ConvertToXml(true);
                }
                else
                {
                    _feedbackTopicsXml = null;
                }
            }
        }

        public virtual bool IsStandardFeedbackForm { get; set; }

        public virtual string CustomFeedbackFormUrl { get; set; }

        public virtual bool IncludeFeedbackFormInYourWebsite { get; set; }

        public virtual bool IncludeGuideToolInYourWebsite { get; set; }

        /// <summary>
        /// Gets or sets the Feedback email address .
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        public virtual string FeedBackEmail { get; set; }

        #region SEO, Analytics and Mapping
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Required(ErrorMessage = @"Please Provide a browser title")]
        [StringLength(100)]
        public virtual string BrowserTitle { get; set; }
        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>
        /// The keywords.
        /// </value>
        [StringLength(1000)]
        public virtual string Keywords { get; set; }
        /// <summary>
        /// Gets or sets the google analytics key.
        /// </summary>
        /// <value>
        /// The google analytics key.
        /// </value>
        [StringLength(100)]
        public virtual string GoogleAnalyticsKey { get; set; }
        /// <summary>
        /// Gets or sets the google maps API key.
        /// </summary>
        /// <value>
        /// The google maps API key.
        /// </value>
        [StringLength(100)]
        public virtual string GoogleMapsApiKey { get; set; }
        #endregion

        /// <summary>
        /// Gets or sets the out put directory.
        /// </summary>
        /// <value>
        /// The out put directory.
        /// </value>
        [Required]
        public virtual string OutPutDirectory { get; set; }

        #endregion

        #region Theme

        [StringLength(100)]
        public virtual string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the logo image.
        /// </summary>
        /// <value>
        /// The logo image.
        /// </value>
        public virtual WebsiteImage LogoImage { get; set; }

        /// <summary>
        /// Gets or sets the banner image.
        /// </summary>
        /// <value>
        /// The banner image.
        /// </value>
        public virtual WebsiteImage BannerImage { get; set; }

        /// <summary>
        /// Gets or sets the homepage content image.
        /// </summary>
        /// <value>
        /// The homepage content image.
        /// </value>
        public virtual WebsiteImage HomepageContentImage { get; set; }

        #endregion

        #endregion

        /// <summary>
        /// Gets the activity log.
        /// </summary>
        /// <value>
        /// The activity log.
        /// </value>
        public virtual IList<ActivityLogEntry> ActivityLog { get; set; }

        /// <summary>
        /// Gets the datasets.
        /// </summary>
        /// <value>
        /// The datasets.
        /// </value>
        public virtual IList<WebsiteDataset> Datasets { get; set; }

        /// <summary>
        /// Gets the reports.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        public virtual IList<WebsiteReport> Reports { get; set; }

        /// <summary>
        /// Gets or sets the measures.
        /// </summary>
        /// <value>
        /// The measures.
        /// </value>
        public virtual IList<WebsiteMeasure> Measures { get; set; }


        /// <summary>
        /// Gets or sets the WebsitePages.
        /// </summary>
        /// <value>
        /// The WebsitePages.
        /// </value>
        public virtual IList<WebsitePage> WebsitePages { get; set; }

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        public virtual WebsiteState? CurrentStatus { get; set; }

        [CustomValidation(typeof(Website), "IsAudienceSelected")]
        public bool HasProfessionalsAudience
        {
            get
            {
                if (Audiences == null) return false;
                return Audiences.Any(a => a == Audience.Professionals);
            }
            set
            {
                if (value) Audiences.Add(Audience.Professionals);
                else Audiences.Remove(Audience.Professionals);
                ResetDefaultAudience();
            }
        }

        [CustomValidation(typeof(Website), "IsAudienceSelected")]
        public bool HasConsumersAudience
        {
            get
            {
                if (Audiences == null) return false;
                return Audiences.Any(a => a == Audience.Consumers);
            }
            set
            {
                if (value) Audiences.Add(Audience.Consumers);
                else Audiences.Remove(Audience.Consumers);
                ResetDefaultAudience();

            }
        }

        public List<Audience> AllAudiences = new List<Audience> { Audience.Consumers, Audience.Professionals };

        public bool HasAllAudiences
        {
            get { return HasProfessionalsAudience && HasConsumersAudience; }
        }

        /// <summary>
        /// Gets or sets the hospitals.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        public virtual IList<WebsiteHospital> Hospitals { get; set; }

        /// <summary>
        /// Gets or sets the Nursing Homes.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        public virtual IList<WebsiteNursingHome> NursingHomes { get; set; }

        public bool? HospitalsChangedWarning { get; set; }

        public bool? UtilizationReportCompression { get; set; }

        public bool? PublishIframeVersion { get; set; }

        public IList<WebsiteTheme> Themes { get; set; }

        public IList<WebsiteMenu> Menus { get; set; }

        #endregion
    }

    #region Webite Measures

    [ImplementPropertyChanged]
    public class WebsiteMeasure : Entity<int>, ISelectable
    {
        public event EventHandler SelectedChanged;
        private void RaiseValueChanged()
        {
            if (SelectedChanged != null)
            {
                SelectedChanged(this, new EventArgs());
            }
        }
        ///// <summary>
        ///// Gets or sets the website.
        ///// </summary>
        ///// <value>
        ///// The website.
        ///// </value>
        //public virtual Website Website { get; set; }

        /// <summary>
        /// Gets or sets the original measure.
        /// </summary>
        /// <value>
        /// The original measure.
        /// </value>
        public virtual Measure OriginalMeasure { get; set; }
        /// <summary>
        /// Gets or sets the override measure.
        /// </summary>
        /// <value>
        /// The override measure.
        /// </value>
        public virtual Measure OverrideMeasure { get; set; }

        /// <summary>
        /// Gets the report measure.
        /// </summary>
        /// <value>
        /// The report measure.
        /// </value>
        public Measure ReportMeasure { get { return OverrideMeasure ?? OriginalMeasure; } }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public virtual int Index { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [is selected].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is selected]; otherwise, <c>false</c>.
        /// </value>
        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                RaiseValueChanged();
            }
        }
    }

    #endregion

    #region Webite Reports

    [ImplementPropertyChanged]
    public class WebsiteReport : Entity<int>
    {
        /// <summary>
        /// Gets or sets the associated report.
        /// </summary>
        /// <value>
        /// The associated report.
        /// </value>
        public virtual Report Report { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public virtual int Index { get; set; }

        /// <summary>
        /// Gets or sets the IsQuarterlyTrendingEnabled.
        /// </summary>
        /// <value>
        /// The IsQuarterlyTrendingEnabled.
        /// </summary>
        public bool IsQuarterlyTrendingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the SelectedYears.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        public List<TrendingYear> SelectedYears { get; set; }

        /// <summary>
        /// Gets or sets the DefaultSelectedYear.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        public string DefaultSelectedYear { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            IsQuarterlyTrendingEnabled = true;
        }

        public virtual string AssociatedWebsites { get; set; }
    }

    #endregion

    #region Webite Datasets

    [ImplementPropertyChanged]
    public class WebsiteDataset : Entity<int>
    {
        /// <summary>
        /// Gets or sets the dataset.
        /// </summary>
        /// <value>
        /// The dataset.
        /// </value>
        public virtual Dataset Dataset { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public virtual int Index { get; set; }
    }

    #endregion

    #region Website Hospitals

    [ImplementPropertyChanged]
    public class WebsiteHospital : Entity<int>
    {
        /// <summary>
        /// Gets or sets the dataset.
        /// </summary>
        /// <value>
        /// The dataset.
        /// </value>
        public virtual Hospital Hospital { get; set; }

        /// <summary>
        /// Gets or sets the CCR.
        /// </summary>
        /// <value>
        /// The CCR.
        /// </value>
        public virtual string CCR { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public virtual int Index { get; set; }
    }

    #endregion

    #region Webite Images

    [DataContract]
    [ImplementPropertyChanged]
    public class WebsiteImage
    {
        /// <summary>
        /// Gets or sets the image path.
        /// </summary>
        /// <value>
        /// The image path.
        /// </value>
        [DataMember]
        public string ImagePath { get; set; }
        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        [DataMember]
        public Byte[] Image { get; set; }
        /// <summary>
        /// Gets or sets the type of the meme.
        /// </summary>
        /// <value>
        /// The type of the meme.
        /// </value>
        [DataMember]
        public string MemeType { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    #endregion

    #region Webite Audience

    [DataContract]
    public class WebsiteAudience
    {
        /// <summary>
        /// Gets or sets the selected theme.
        /// </summary>
        /// <value>
        /// The selected theme.
        /// </value>
        [DataMember(Name = "website_Theme", Order = 7)]
        public virtual string SelectedTheme { get; set; }

        /// <summary>
        /// Gets or sets the selected font.
        /// </summary>
        /// <value>
        /// The selected font.
        /// </value>
        [DataMember(Name = "website_Font", Order = 8)]
        public virtual string SelectedFont { get; set; }

        /// <summary>
        /// Gets or sets the logo image path.
        /// </summary>
        /// <value>
        /// The logo image path.
        /// </value>
        [DataMember(Name = "website_LogoImagePath", Order = 9)]
        public virtual string LogoImagePath { get; set; }

        /// <summary>
        /// Gets or sets the banner image path.
        /// </summary>
        /// <value>
        /// The banner image path.
        /// </value>
        [DataMember(Name = "website_BannerImagePath", Order = 10)]
        public virtual string BannerImagePath { get; set; }

        /// <summary>
        /// Gets or sets the homepage content image path.
        /// </summary>
        /// <value>
        /// The homepage content image path.
        /// </value>
        [DataMember(Name = "website_HomepageContentImagePath", Order = 11)]
        public virtual string HomepageContentImagePath { get; set; }

        /// <summary>
        /// Gets or sets the feedback topics.
        /// </summary>
        /// <value>
        /// The feedback topics.
        /// </value>
        [DataMember(Name = "website_FeedbackTopics", Order = 12)]
        public virtual List<string> FeedbackTopics { get; set; }

        /// <summary>
        /// Gets or sets the Feedback email address .
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        [DataMember(Name = "website_FeedBackEmail", Order = 13)]
        public virtual string FeedBackEmail { get; set; }

        /// <summary>
        /// Gets or sets the feeback URL.
        /// </summary>
        /// <value>
        /// The feeback URL.
        /// </value>
        [DataMember(Name = "website_FeebackUrl", Order = 16)]
        public virtual string FeebackUrl { get; set; }

        /// <summary>
        /// Gets or sets the homepage video.
        /// </summary>
        /// <value>
        /// The homepage video.
        /// </value>
        [DataMember(Name = "HOMEPAGE_VIDEO", Order = 26)]
        public int HomepageVideo { get; set; }

        /// <summary>
        /// Gets or sets the homepage video URL.
        /// </summary>
        /// <value>
        /// The homepage video URL.
        /// </value>
        [DataMember(Name = "HOMEPAGE_VIDEO_URL", Order = 27)]
        public string HomepageVideoUrl { get; set; }

        /// <summary>
        /// Gets or sets the help.
        /// </summary>
        /// <value>
        /// The help.
        /// </value>
        [DataMember(Name = "patch_maine_iphelp", Order = 28)]
        public WebsiteAudienceHelp Help { get; set; }

        /// <summary>
        /// Gets or sets the guide tool.
        /// </summary>
        /// <value>
        /// The guide tool.
        /// </value>
        [DataMember(Name = "website_guidetool", Order = 33)]
        public bool guidetool { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        public struct WebsiteAudienceHelp
        {
            [DataMember(Name = "InterpretationHTMLDescription", Order = 1)]
            public string InterpretationHtmlDescription { get; set; }
        }
    }

    #endregion

    #region Website Themes

    [ImplementPropertyChanged]
    public class WebsiteTheme : Entity<int>
    {
        public Audience AudienceType { get; set; }

        public virtual string SelectedTheme { get; set; }

        public virtual string BrandColor { get; set; }

        public virtual string Brand2Color { get; set; }

        public virtual string AccentColor { get; set; }

        public virtual string SelectedFont { get; set; }

        public string BackgroundColor { get; set; }

        public string BodyTextColor { get; set; }

        public string LinkTextColor { get; set; }

    }

    #endregion

    #region Webite Menu

    [DataContract]
    [ImplementPropertyChanged]
    public class WebsiteMenu : Entity<int>, ISelectable
    {
        [DataMember]
        public bool IsSelected { get; set; }

        [DataMember]
        public virtual Menu Menu { get; set; }
    }

    #endregion

    #region Helper Classes

    [Serializable]
    [ImplementPropertyChanged]
    public class TrendingYear : ISelectable
    {
        private bool _isSelected;
        private List<Period> _quarters;

        [XmlElement]
        public string Year { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public bool IsExpanded { get; set; }

        [XmlElement]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (Quarters != null)
                {
                    foreach (var quarter in Quarters)
                    {
                        quarter.ValueChanged -= ValidateQuarters;
                        quarter.ValueChanged += ValidateQuarters;
                    }
                }
                Validate();
            }
        }

        [XmlElement]
        public bool IsDefault { get; set; }

        [XmlElement]
        public List<Period> Quarters
        {
            get { return _quarters; }
            set
            {
                _quarters = value;
                if (_quarters != null)
                {
                    foreach (var quarter in Quarters)
                    {
                        quarter.ValueChanged -= ValidateQuarters;
                        quarter.ValueChanged += ValidateQuarters;
                    }
                }
            }
        }

        public bool IsValid { get; set; }

        public void Validate()
        {
            IsValid = Quarters != null && Quarters.All(x => !x.IsSelected) && IsSelected;
        }

        private void ValidateQuarters(object sender, EventArgs args)
        {
            Validate();
        }
    }

    [Serializable]
    public class Period : ISelectable
    {
        private bool _isSelected;
        public event EventHandler ValueChanged;

        [XmlElement]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnValueChanged();
            }
        }

        [XmlElement]
        public string Text { get; set; }

        private void OnValueChanged()
        {
            var evt = ValueChanged;
            if (evt != null)
            {
                evt(this, new EventArgs());
            }
        }
    }

    #region Serializing Helper and WebsiteMenu

    public class CustomContractResolver : DefaultContractResolver
    {
        private readonly List<string> _excludedProperties;

        public CustomContractResolver(List<string> excludedProperties)
        {
            _excludedProperties = excludedProperties;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            return properties.Where(x => !_excludedProperties.Contains(x.PropertyName)).ToList();
        }
    }

    #endregion
    #endregion
}
