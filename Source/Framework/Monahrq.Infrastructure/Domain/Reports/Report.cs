using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain.Reports.Attributes;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;
using Version = Monahrq.Infrastructure.Domain.Wings.Version;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Entities.Domain.Reports.Validators;

namespace Monahrq.Infrastructure.Entities.Domain.Reports
{
    [Serializable]
    [ImplementPropertyChanged]
    public class Report : Entity<int>
    {
        //private string _audiences;
        XmlDocument _filterItemsXml;
        XmlDocument _reportManifestXml;
        IList<FilterItem> _filterItems;
        private ReportManifest _sourceTemplate;

        private IList<RptFilter> _rptfilters;
        private IList<Filter> _filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Report"/> class.
        /// </summary>
        public Report()
        {
            WebsitePages = new List<ReportWebsitePage>();
            //NHibernate.Type.GenericBagType
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Report"/> class.
        /// </summary>
        /// <param name="manifest">The manifest.</param>
        public Report(ReportManifest manifest)
            : this()
        {
            AssignFrom(manifest);
        }
        public void AssignFrom(ReportManifest manifest)
        {
            //Filters = new List<Filter>();
            Columns = Columns ?? new List<ReportColumn>();
            WebsitePages = WebsitePages ?? new List<ReportWebsitePage>();
            ComparisonKeyIcons = ComparisonKeyIcons ?? new List<ComparisonKeyIconSet>();
            _rptfilters = _rptfilters ?? new List<RptFilter>();
            Audiences = Audiences ?? new List<Audience>();

            Columns.Clear();
            WebsitePages.Clear();
            ComparisonKeyIcons.Clear();
            _rptfilters.Clear();
            Audiences.Clear();

            ComparisonKeyIcons.AddRange(manifest.ToComparisonKeyIconSet(manifest.IconSets, this).ToList());
            Audiences.AddRange(manifest.Audiences.Select(a => a.AudienceType).ToList());
            Category = manifest.Category;
            IsTrending = manifest.IsTrending;
            Datasets = manifest.Datasets.Select(d => d.Name).ToList();
            manifest.Columns.ForEach(col => new ReportColumn(this, col.Name) { IsMeasure = col.IsMeasure, MeasureCode = col.MeasureCode, IsIncluded = true });
            manifest.WebsitePages.ForEach(wp => new ReportWebsitePage(this, wp));

            Name = manifest.Name;
            ReportAttributes = manifest.ReportAttributes;
            SourceTemplate = manifest;
            Description = !string.IsNullOrWhiteSpace(manifest.Description) ? manifest.Description.TrimStart().TrimEnd() : null;
            Filter = CandidateFilters.ToReportFilter();
            RequiresCmsProviderId = manifest.RequiresCmsProviderId;
            RequiresCostToChargeRatio = manifest.RequiresCostToChargeRatio;
            ReportType = manifest.Name;
            ShowInterpretationText = manifest.ShowInterpretationText;
            InterpretationText = !string.IsNullOrWhiteSpace(manifest.InterpretationText) ? manifest.InterpretationText.TrimStart().TrimEnd() : null;
            Footnote = !string.IsNullOrWhiteSpace(manifest.Footer) ? manifest.Footer.TrimStart().TrimEnd() : null;
            ReportOutputSql = !string.IsNullOrWhiteSpace(manifest.ReportOutputSql) ? manifest.ReportOutputSql.TrimStart().TrimEnd() : null;
            OutputFileName = !string.IsNullOrWhiteSpace(manifest.OutputFileName) ? manifest.OutputFileName.TrimStart().TrimEnd() : null;
            OutputJsNamespace = !string.IsNullOrWhiteSpace(manifest.OutputJsNamespace) ? manifest.OutputJsNamespace.TrimStart().TrimEnd() : null;

            _rptfilters.AddRange(RptFilter.FromFilterList(manifest.Filters, this).ToList());

            LastReportManifestUpdate = manifest.FileLastModifiedDate;

        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="column">The column.</param>
        public void AddColumn(ReportColumn column)
        {
            Columns.Add(column);
            column.Report = this;
        }

        public void AddWebsitePage(ReportWebsitePage rwp)
        {
            WebsitePages.Add(rwp);
            rwp.Report = this;
        }

        #region Properties
        /// <summary>
        /// Gets or sets the report SQL.
        /// </summary>
        /// <value>
        /// The report SQL.
        /// </value>
        public string ReportOutputSql { get; set; }
        /// <summary>
        /// Gets or sets the filter items.
        /// </summary>
        /// <value>
        /// The filter items.
        /// </value>
        public IList<FilterItem> FilterItems
        {
            get
            {
                if (_filterItemsXml != null && _filterItems == null)
                {
                    _filterItems = _filterItemsXml.ConvertFromXml<List<FilterItem>>();
                }

                return _filterItems ?? new List<FilterItem>();
            }
            set
            {
                _filterItems = value;

                if (_filterItems != null)
                {
                    if (_filterItemsXml == null)
                    {
                        _filterItemsXml = new XmlDocument();
                    }

                    _filterItemsXml = _filterItems.ConvertToXml(true);
                }
                else
                {
                    _filterItemsXml = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        public IList<Filter> Filters
        {
            get
            {
                if (_filters == null && _rptfilters != null)
                {
                    _filters = RptFilter.ToFilterList(new List<RptFilter>(_rptfilters)).ToList();
                }

                //if (_filters != null && _filters.Any())
                //    _filters = _filters.ToList().DistinctBy<Filter>(x => x.);

                return _filters ?? new List<Filter>();
            }
            set
            {
                _filters = value;

                _rptfilters = _filters == null
                                        ? new List<RptFilter>()
                                        : RptFilter.FromFilterList(_filters, this).ToList();
            }
        }

        /// <summary>
        /// Cleans the before save.
        /// </summary>
        public override void CleanBeforeSave()
        {
            _rptfilters = Filters == null
                                        ? new List<RptFilter>()
                                        : RptFilter.FromFilterList(Filters, this).ToList();

            //if (Filters != null && Filters.Any())
            //    Filters = Filters.RemoveNullValues().Distinct().ToList();

            //if (Columns != null && Columns.Any())
            //    Columns = Columns.RemoveNullValues().Distinct().ToList();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [requires CMS provider identifier].
        /// </summary>
        /// <value>
        /// <c>true</c> if [requires CMS provider identifier]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresCmsProviderId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires cost to charge ratio].
        /// </summary>
        /// <value>
        /// <c>true</c> if [requires cost to charge ratio]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresCostToChargeRatio { get; set; }

        /// <summary>
        /// Gets or sets the report type for display.
        /// </summary>
        /// <value>
        /// The report type for display.
        /// </value>
        public string ReportType { get; set; }

        /// <summary>
        /// Gets the candidate filters.
        /// </summary>
        /// <value>
        /// The candidate filters.
        /// </value>
        public HashSet<ReportFilter> CandidateFilters
        {
            get
            {
                var filter = (ReportAttributes & ReportAttributeOption.HospitalFilters) == ReportAttributeOption.HospitalFilters
                    ? ReportFilter.HospitalFilters : ReportFilter.None;
                filter |= (ReportAttributes & ReportAttributeOption.DRGsDischargesFilters) == ReportAttributeOption.DRGsDischargesFilters
                    ? ReportFilter.DRGsDischargesFilters : ReportFilter.None;
                filter |= (ReportAttributes & ReportAttributeOption.ConditionsAndDiagnosisFilters) == ReportAttributeOption.ConditionsAndDiagnosisFilters
                   ? ReportFilter.ConditionsAndDiagnosisFilters : ReportFilter.None;
                filter |= (ReportAttributes & ReportAttributeOption.CountyFilters) == ReportAttributeOption.CountyFilters
                    ? ReportFilter.CountyFilters : ReportFilter.None;

                return new HashSet<ReportFilter>(filter.GetValuesForGroup());
            }
        }

        [NonEmptyList]
        public IList<Audience> Audiences { get; set; }

        //public IList<Audience> Audiences
        //{
        //    get;
        //    //{
        //    //    if (string.IsNullOrEmpty(_audiences) || string.IsNullOrWhiteSpace(_audiences))
        //    //        return new List<Audience>();

        //    //    return _audiences.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList()
        //    //                    .Select(a => (Audience)Enum.Parse(typeof(Audience), a))
        //    //                    .ToList();
        //    //}
        //    set; 
        //    //{
        //    //    _audiences = value != null && value.Any()
        //    //                ? string.Join(",", value.Select(x => x.ToString()).ToList())
        //    //                : null;
        //    //}
        //}

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
            }
        }

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
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show interpretation text].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show interpretation text]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInterpretationText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the interpretation text.
        /// </summary>
        /// <value>
        /// The interpretation text.
        /// </value>
        public string InterpretationText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the report attributes.
        /// </summary>
        /// <value>
        /// The report attributes.
        /// </value>
        public ReportAttributeOption ReportAttributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public ReportCategory Category
        {
            get;
            set;
        }

        /// <summary>
        /// Depicts if report is a Trending Report
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public bool IsTrending { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public ReportFilter Filter { get; set; }

        /// <summary>
        /// Gets the comparison key icons.
        /// </summary>
        /// <value>
        /// The comparison key icons.
        /// </value>
        public IList<ComparisonKeyIconSet> ComparisonKeyIcons
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the datasets.
        /// </summary>
        /// <value>
        /// The datasets.
        /// </value>
        public IList<string> Datasets
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public virtual IList<ReportColumn> Columns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is default report].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is default report]; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefaultReport { get; set; }

        /// <summary>
        /// Gets or sets the report profile.
        /// </summary>
        /// <value>
        /// The report profile.
        /// </value>
        public ReportProfileDisplayItem ReportProfile { get; set; }

        /// <summary>
        /// Gets or sets the source template.
        /// </summary>
        /// <value>
        /// The source template.
        /// </value>
        public ReportManifest SourceTemplate
        {
            get
            {
                if (_reportManifestXml != null && _sourceTemplate == null)
                {
                    _sourceTemplate = _reportManifestXml.ConvertFromXml<ReportManifest>();
                }

                return _sourceTemplate;
            }
            set
            {
                _sourceTemplate = value;

                if (_sourceTemplate != null)
                {
                    if (_reportManifestXml == null)
                    {
                        _reportManifestXml = new XmlDocument();
                    }

                    _reportManifestXml = _sourceTemplate.ConvertToXml(true);
                }
                else
                {
                    _reportManifestXml = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the footnote.
        /// </summary>
        /// <value>
        /// The footnote.
        /// </value>
        public string Footnote { get; set; }
        /// <summary>
        /// Gets or sets the name of the comparison key icon set.
        /// </summary>
        /// <value>
        /// The name of the comparison key icon set.
        /// </value>
        public string ComparisonKeyIconSetName { get; set; }

        /// <summary>
        /// Gets or sets the websites for report display.
        /// </summary>
        /// <value>
        /// The websites for report display.
        /// </value>
        public IList<string> WebsitesForReportDisplay
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this report is a 3rd party custom wing target report.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom; otherwise, <c>false</c>.
        /// </value>
        public bool IsCustom { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Version Version { get; set; }
        /// <summary>
        /// Gets or sets the publisher.
        /// </summary>
        /// <value>
        /// The publisher.
        /// </value>
        public string Publisher { get; set; }
        /// <summary>
        /// Gets or sets the publisher email.
        /// </summary>
        /// <value>
        /// The publisher email.
        /// </value>
        public string PublisherEmail { get; set; }
        /// <summary>
        /// Gets or sets the publisher website.
        /// </summary>
        /// <value>
        /// The publisher website.
        /// </value>
        public string PublisherWebsite { get; set; }

        /// <summary>
        /// Gets or sets the name of the report output file.
        /// </summary>
        /// <value>
        /// The name of the report output file.
        /// </value>
        public string OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the ReportWebsitePageZones.
        /// </summary>
        public virtual IList<ReportWebsitePage> WebsitePages { get; set; }
        #endregion

        /// <summary>
        /// Gets or sets the report output JavaScript/Json namespace.
        /// </summary>
        /// <value>
        /// The report output JavaScript/Json namespace.
        /// </value>
        public string OutputJsNamespace { get; set; }

        public BitmapImage ProfessionalPreviewImage { get; set; }

        public BitmapImage ConsumerPreviewImage { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? LastReportManifestUpdate { get; set; }
    }

    [Serializable, DebuggerStepThrough]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [EntityTableName("Reports_ReportColumns")]
    [ImplementPropertyChanged]
    public class ReportColumn : Entity<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportColumn"/> class.
        /// </summary>
        public ReportColumn()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportColumn"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="name">The name.</param>
        public ReportColumn(Report report, string name)
        {
            Name = name;
            Report = report;
            report.AddColumn(this);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is measure.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is measure; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsMeasure { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [measure code].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [measure code]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public string MeasureCode { get; set; }

        /// <summary>
        /// Gets or sets the report.
        /// </summary>
        /// <value>
        /// The report.
        /// </value>
        [XmlIgnore]
        public Entity<int> Report { get; set; }

        /// <summary>
        /// Gets or sets the Comparison is included.
        /// </summary>
        /// <value>
        /// The is included.
        /// </value>
        public virtual bool IsIncluded { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        [XmlIgnore]
        public virtual int Index { get; set; }
    }

    [Serializable]
    [DebuggerStepThrough]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [EntityTableName("Reports_WebsitePages")]
    [ImplementPropertyChanged]
    public class ReportWebsitePage : Entity<int>
    {
        #region Properties.

        [XmlAttribute]
        public virtual Reports.Audience Audience { get; set; }
        [XmlAttribute]
        public virtual String Path { get; set; }
        [XmlAttribute]
        public virtual String Url { get; set; }
        [XmlAttribute]
        public virtual bool IsEditable { get; set; }

        #region WebsitePageZones Property.
        private IList<ReportWebsitePageZone> _websitePageZones;
        [XmlArray("WebsitePageZones")]
        [XmlArrayItem("WebsitePageZone", Type = typeof(ReportWebsitePageZone))]
        public virtual List<ReportWebsitePageZone> XmlWebsitePageZone
        {
            get
            {
                if (_websitePageZones == null) return null;
                else if (_websitePageZones is List<ReportWebsitePageZone>) return _websitePageZones as List<ReportWebsitePageZone>;
                else return _websitePageZones.ToList();
                //return (List<ReportWebsitePageZone>) _websitePageZones;
            }
            set
            {
                _websitePageZones = value;
            }
        }
        [XmlIgnore]
        public virtual IList<ReportWebsitePageZone> WebsitePageZones
        {
            get
            {
                return _websitePageZones;
            }
            set
            {
                _websitePageZones = value;
            }
        }
        #endregion

        [XmlIgnore]
        public Entity<int> Report { get; set; }
        #endregion


        #region Methods.
        #region Constructors.
        public ReportWebsitePage()
        {
            WebsitePageZones = new List<ReportWebsitePageZone>();
        }
        public ReportWebsitePage(ReportWebsitePage other)
        {
            WebsitePageZones = new List<ReportWebsitePageZone>();

            Name = other.Name;
            Audience = other.Audience;
            Path = other.Path;
            Url = other.Url;
            IsEditable = other.IsEditable;
            other.WebsitePageZones.ForEach(owpz => AddWebsitePageZone(owpz));
        }
        public ReportWebsitePage(Report report, ReportWebsitePage other) : this(other)
        {
            report.AddWebsitePage(this);
        }
        #endregion

        #region Add Methods.
        public void AddWebsitePageZone(ReportWebsitePageZone rwpz)
        {
            WebsitePageZones.Add(rwpz);
            rwpz.ReportWebsitePage = this;
        }
        #endregion
        #endregion
    }

    [Serializable]
    [DebuggerStepThrough]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [EntityTableName("Reports_WebsitePageZones")]
    [ImplementPropertyChanged]
    public class ReportWebsitePageZone : Entity<int>
    {
        #region Properties.
        [XmlAttribute]
        public virtual String CodePath { get; set; }

        [XmlIgnore]
        public Entity<int> ReportWebsitePage { get; set; }


        [XmlIgnore]
        public virtual int Index { get; set; }
        #endregion

        #region Methods.
        #region Constructors.
        public ReportWebsitePageZone()
        {
        }
        public ReportWebsitePageZone(ReportWebsitePageZone other)
        {
            Name = other.Name;
            CodePath = other.CodePath;
            ReportWebsitePage = other.ReportWebsitePage;
        }
        public ReportWebsitePageZone(ReportWebsitePage reportWebsitePage, ReportWebsitePageZone other) : this(other)
        {
            reportWebsitePage.AddWebsitePageZone(this);
        }
        #endregion
        #endregion

    }

    [Serializable, ImplementPropertyChanged]
    [EntityTableName("Reports_ComparisonKeyIconSets")]
    public class ComparisonKeyIconSet : OwnedEntity<Report, int, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonKeyIconSet"/> class.
        /// </summary>
        protected ComparisonKeyIconSet()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonKeyIconSet"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="name">The name.</param>
        public ComparisonKeyIconSet(Report report, string name)
            : base(report)
        {
            Name = name;
            Owner = report;
            report.ComparisonKeyIcons.Add(this);
        }

        /// <summary>
        /// Gets or sets the best image URI.
        /// </summary>
        /// <value>
        /// The best image URI.
        /// </value>
        public virtual Uri BestImage { get; set; }

        /// <summary>
        /// Gets or sets the better image URI.
        /// </summary>
        /// <value>
        /// The better image URI.
        /// </value>
        public virtual Uri BetterImage { get; set; }

        /// <summary>
        /// Gets or sets the below image URI URI.
        /// </summary>
        /// <value>
        /// The below image.
        /// </value>
        public virtual Uri BelowImage { get; set; }
        /// <summary>
        /// Gets or sets the average image URI.
        /// </summary>
        /// <value>
        /// The average image.
        /// </value>
        public virtual Uri AverageImage { get; set; }
        /// <summary>
        /// Gets or sets the not enough data image URI.
        /// </summary>
        /// <value>
        /// The not enough data image.
        /// </value>
        public virtual Uri NotEnoughDataImage { get; set; }

        /// <summary>
        /// Gets or sets the Comparison is included.
        /// </summary>
        /// <value>
        /// The is included.
        /// </value>
        public virtual bool IsIncluded { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        [XmlIgnore]
        public virtual int Index { get; set; }
    }

    [Serializable, EntityTableName("Reports_Filters")]
    [ImplementPropertyChanged]
    public class RptFilter : OwnedEntity<Report, int, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RptFilter"/> class.
        /// </summary>
        public RptFilter()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedEntity{TOwner, TOwnerId, TId}" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public RptFilter(Report owner)
            : base(owner)
        { }

        /// <summary>
        /// Gets or sets the type of the filter.
        /// </summary>
        /// <value>
        /// The type of the filter.
        /// </value>
        public virtual ReportFilterTypeEnum FilterType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RptFilter"/> is value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if value; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is RadioButton.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is RadioButton; otherwise, <c>false</c>.
        /// </value>
        public bool IsRadioButton { get; set; }

        /// <summary>
        /// Gets or sets the name of the radio group.
        /// </summary>
        /// <value>
        /// The name of the radio group.
        /// </value>
        public string RadioGroupName { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        [XmlIgnore]
        public virtual int Index { get; set; }

        /// <summary>
        /// To the filter list.
        /// </summary>
        /// <param name="rptsFilters">The RPTS filters.</param>
        /// <returns></returns>
        public static IList<Filter> ToFilterList(IList<RptFilter> rptsFilters)
        {
            var filters = new List<Filter>();
            rptsFilters = rptsFilters.DistinctBy(f => f.Name).ToList();
            foreach (var rptFilter in rptsFilters.Select(f => new { FilterType = f.FilterType, Owner = f.Owner }).ToList())
            {
                var cols =
                    rptsFilters.Where(rf => rf.FilterType == rptFilter.FilterType)
                               .Distinct()
                               .Select(rf => new FilterValue
                               {
                                   Name = rf.Name,
                                   Value = rf.Value,
                                   IsRadioButton = rf.IsRadioButton,
                                   RadioGroupName = rf.RadioGroupName
                               })
                               .ToList();


                if (!cols.Any())
                {
                    continue;
                }

                var filter = new Filter(rptFilter.Owner);
                filter.Type = rptFilter.FilterType;

                cols.ForEach(kvp =>
                {
                    if (!filter.Values.Any(f => f.Name.EqualsIgnoreCase(kvp.Name)))
                        filter.Values.Add(kvp);
                });

                if (filters.All(x => x.Type != filter.Type))
                    filters.Add(filter);
            }

            //if (filters.Any())
            //filters = filters.Distinct().ToList();

            return filters;
        }

        /// <summary>
        /// Froms the filter list.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="report">The report.</param>
        /// <returns></returns>
        public static IList<RptFilter> FromFilterList(IList<Filter> filters, Report report)
        {
            var rptsFilters = new List<RptFilter>();

            foreach (var filter in filters.ToList())
            {
                if (!filter.Values.ToList().Any()) continue;

                foreach (var fv in filter.Values.ToList())
                {
                    var rptFilter = new RptFilter(report);

                    if (string.IsNullOrEmpty(fv.Name)) continue;

                    rptFilter.FilterType = filter.Type;
                    rptFilter.Name = fv.Name;
                    rptFilter.Value = fv.Value;
                    rptFilter.IsRadioButton = fv.IsRadioButton;
                    rptFilter.RadioGroupName = fv.RadioGroupName;

                    if (!rptsFilters.Any(f => f.FilterType == rptFilter.FilterType && f.Name.EqualsIgnoreCase(rptFilter.Name)))
                        rptsFilters.Add(rptFilter);
                }
            }

            return rptsFilters.DistinctBy(f => f.Name).ToList();
            //return rptsFilters.Distinct().ToList();
        }

        /// <summary>
        /// Froms the filter list.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="report">The report.</param>
        /// <returns></returns>
        public static IList<RptFilter> FromFilterList(IList<RptManifestFilter> filters, Report report)
        {
            var rptsFilters = new List<RptFilter>();
            foreach (var filter in filters.ToList())
            {
                foreach (var fv in filter.Values.ToList())
                {
                    var rptFilter = new RptFilter(report);

                    if (string.IsNullOrEmpty(fv.Name)) continue;

                    rptFilter.FilterType = filter.Type;
                    rptFilter.Name = fv.Name;
                    rptFilter.Value = fv.Value;
                    rptFilter.IsRadioButton = fv.IsRadioButton;
                    rptFilter.RadioGroupName = fv.RadioGroupName;

                    if (!rptsFilters.Any(f => f.FilterType == rptFilter.FilterType && f.Name.EqualsIgnoreCase(rptFilter.Name)))
                        rptsFilters.Add(rptFilter);
                }
            }

            return rptsFilters.DistinctBy(fv => fv.Name).ToList();
            //return rptsFilters.ToList();
        }
    }

    [Serializable, DebuggerStepThrough]
    [ImplementPropertyChanged]
    public class FilterValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterValue"/> class.
        /// </summary>
        public FilterValue()
        {
            Value = true;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FilterValue"/> is value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if value; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute, DefaultValue(true)]
        public bool Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is RadioButton.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is RadioButton; otherwise, <c>false</c>.
        /// </value>
        public bool IsRadioButton { get; set; }

        /// <summary>
        /// Gets or sets the name of the radio group.
        /// </summary>
        /// <value>
        /// The name of the radio group.
        /// </value>
        public string RadioGroupName { get; set; }
    }

    /// <remarks/>
    [Serializable, DebuggerStepThrough]
    [ImplementPropertyChanged]
    public class Filter : OwnedEntity<Report, int, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Monahrq.Infrastructure.Entities.Domain.Reports.Filter"/> class.
        /// </summary>
        public Filter()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedEntity{TOwner, TOwnerId, TId}"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="name">The name.</param>
        public Filter(Report owner)
            : base(owner)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            Values = new List<FilterValue>();
        }

        /// <remarks/>
        [XmlArrayItem("FilterValue", IsNullable = true)]
        public List<FilterValue> Values { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public ReportFilterTypeEnum Type { get; set; }
    }
}
