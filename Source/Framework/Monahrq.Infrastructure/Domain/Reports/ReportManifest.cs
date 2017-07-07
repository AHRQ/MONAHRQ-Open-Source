using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

// This file was AUTO-GENERATED, but it is manually updated now. Any changes in ReportManifest.xsd MUST BE MANUALLY APPLIED HERE.
namespace Monahrq.Infrastructure.Entities.Domain.Reports
{
    [Flags, Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Description("Target audience for reports generated from the template")]
    public enum Audience
    {
        [Description("Select Audience")]
        None = 0,
        [Description("Consumers")]
        Consumers = 1,
        [Description("Healthcare Professionals")]
        Professionals = 2
        //,
        //[Description("All Audiences")]
        //AllAudiences = 3
    }

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class RptManifestAudience
    {
        [XmlAttribute]
        public Audience AudienceType { get; set; }
    }

    /// <remarks/>
    [Serializable, DebuggerStepThrough]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class RptManifestColumn : Entity<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportColumn"/> class.
        /// </summary>
        public RptManifestColumn()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportColumn"/> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="name">The name.</param>
        public RptManifestColumn(ReportManifest report, string name)
        {
            Name = name;
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
        //[XmlElement(Type = typeof(ReportManifest))]
        [XmlIgnore]
        public Entity<int> Report
        {
            get;
            set;
        }
    }

    [Serializable, DebuggerStepThrough]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class ReportDataset
    {
        [XmlAttribute("Name")]
        public string Name
        {
            get;
            set;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? Name : base.ToString();
        }
    }

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Description("Determines where the report appears in a MONAHRQ-generated website")]
    public enum ReportCategory
    {
        [Description("Quality")]
        Quality = 0,
        [Description("Utilization")]
        Utilization = 1
    }

    [Flags, Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Description("Available report attributes")]
    public enum ReportAttributeOption : int
    {
        [Description("No attributes")]
        None = 0x0,
        [Description("Hospital Filters: Hospital Name, Category, Zip, Region")] // , All Hospitals
        HospitalFilters = 0x01,
        [Description("DRGs and Discharges Filters: MDC, DRG, Condition, Procedure")] //, All Discharges Combined
        DRGsDischargesFilters = 0x02,
        [Description("Conditions and Diagnosis: Condition")] //, All Diagnoses Combined
        ConditionsAndDiagnosisFilters = 0x04,
        [Description("Keys for Ratings")]
        KeysForRatings = 0x08,
        [Description("Included Hospitals")]
        IncludedHospitals = 0x10,
        [Description("Report Columns")]
        ReportColumns = 0x20,
        [Description("Display (Hospital Profile report)")]
        Display = 0x40,
        [Description("County Filters: County")]
        CountyFilters = 0x80
    }

    [Description(Constants.REPORT_FILTER_DESCRIPTION)]
    [Flags]
    public enum ReportFilter : uint
    {
        None = 0x0,
        [Description(Constants.HOSPITAL_FILTER_DESCRIPTION)]
        HospitalFilters = 0x10000,
        [Description(Constants.HOSPITAL_NAME_DESCRIPTION)]
        HospitalName = HospitalFilters | 0x1,
        [Description(Constants.CATEGORY_DESCRIPTION)]
        Category = HospitalFilters | 0x2,
        //   [Description(Constants.ZipCodeDescription)]
        //   ZipCode = HospitalFilters | 0x4,
        [Description(Constants.REGION_DESCRIPTION)]
        Region = HospitalFilters | 0x8,
        //[Description(Constants.AllHospitalsDescription)]
        //AllHospitals = HospitalFilters | 0x10,
        [Description(Constants.COUNTY_DESCRIPTION)]
        County = HospitalFilters | 0x20,
        [Description(Constants.DR_GS_DISCHARGES_FILTER_DESCRIPTION)]
        DRGsDischargesFilters = 0x20000,
        [Description(Constants.MDC_DESCRIPTION)]
        MDC = DRGsDischargesFilters | 0x20,
        [Description(Constants.DRG_DESCRIPTION)]
        DRG = DRGsDischargesFilters | 0x40,
        [Description(Constants.DRG_CONDITION_DESCRIPTION)]
        DRGCondition = DRGsDischargesFilters | 0x80,
        [Description(Constants.PROCEDURE_DESCRIPTION)]
        Procedure = DRGsDischargesFilters | 0x100,
        //[Description(Constants.AllDischargesCombinedDescription)]
        //AllDischargesCombined = DRGsDischargesFilters | 0x200,
        [Description(Constants.CONDITIONS_AND_DIAGNOSIS_FILTER_DESCRIPTION)]
        ConditionsAndDiagnosisFilters = 0x40000,
        [Description(Constants.CONDITIONS_DESCRIPTION)]
        Conditions = ConditionsAndDiagnosisFilters | 0x400,
        //[Description(Constants.AllDiagnosesCombinedDescription)]
        //AllDiagnosesCombined = ConditionsAndDiagnosisFilters | 0x800,
        [Description(Constants.COUNTY_FILTER_DESCRIPTION)]
        CountyFilters = 0x80000,
        [Description(Constants.COUNTY_NAME_DESCRIPTION)]
        CountyName = CountyFilters | 0x800,
    }

    /// <remarks/>
    [Serializable, DebuggerStepThrough]
    public class RptManifestFilterValue
    {
        public RptManifestFilterValue()
        {
            Value = true;
        }

        /// <remarks/>
        [XmlAttribute]
        public string Name { get; set; }

        /// <remarks/>
        [XmlAttribute, DefaultValue(true)]
        public bool Value { get; set; }

        [XmlAttribute, DefaultValue(false)]
        public bool IsRadioButton { get; set; }

        [XmlAttribute]
        public string RadioGroupName { get; set; }
    }

    /// <remarks/>
    [Serializable, DebuggerStepThrough]
    public class RptManifestFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        public RptManifestFilter()
        { }

        /// <remarks/>
        [XmlArray("Values"),
         XmlArrayItem("FilterValue", Type = typeof(RptManifestFilterValue))]
        public List<RptManifestFilterValue> Values { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public ReportFilterTypeEnum Type { get; set; }
    }


    /// <remarks/>
    [Serializable]
    public enum ReportFilterTypeEnum
    {

        /// <remarks/>
        [Description("Conditions and Diagnosis")]
        ConditionsAndDiagnosis,

        /// <remarks/>
        [Description("County")]
        County,

        /// <remarks/>
        [Description("Display")]
        Display,

        /// <remarks/>
        [Description("DRGs Discharges")]
        DRGsDischarges,

        /// <remarks/>
        [Description("Hospital")]
        Hospital,

        [Description("Physician Filters")]
        PhysicianFilters,

        [Description("Nursing Home Filters")]
        NursingHomeFilters,


        [Description("Geo Location")]
        GeoLocation,

        [Description("Active Sections")]
        ActiveSections,

        ///// <remarks/>
        //KeysForRatings,

        ///// <remarks/>
        //ReportColumns
    }

    public static class Constants
    {
        public const string REPORT_FILTER_DESCRIPTION = "Filter values";
        public const string HOSPITAL_FILTER_DESCRIPTION = "Hospital Filters";
        public const string COUNTY_FILTER_DESCRIPTION = "County Filters";
        public const string COUNTY_NAME_DESCRIPTION = "County";

        public const string HOSPITAL_NAME_DESCRIPTION = "Hospital Name";
        public const string CATEGORY_DESCRIPTION = "Hospital Type";
        public const string ZIP_CODE_DESCRIPTION = "ZIP Code";
        public const string REGION_DESCRIPTION = "Region";
        public const string COUNTY_DESCRIPTION = "County";
        public const string ALL_HOSPITALS_DESCRIPTION = "All Hospitals";
        public const string DR_GS_DISCHARGES_FILTER_DESCRIPTION = "DRGs and Discharges Filters";
        public const string MDC_DESCRIPTION = "Major Diagnosis Category";
        public const string DRG_DESCRIPTION = "Diagnosis Related Group";
        public const string DRG_CONDITION_DESCRIPTION = "Health Condition or Topic";
        public const string PROCEDURE_DESCRIPTION = "Procedure";
        public const string ALL_DISCHARGES_COMBINED_DESCRIPTION = "All Discharges Combined";
        public const string CONDITIONS_AND_DIAGNOSIS_FILTER_DESCRIPTION = "Conditions and Diagnosis";
        public const string CONDITIONS_DESCRIPTION = "Health Condition or Topic";
        public const string ALL_DIAGNOSES_COMBINED_DESCRIPTION = "All Diagnoses Combined";
        public const string REPORT_PROFILE_MAP = "Map";
        public const string REPORT_PROFILE_PATIENT_EXPERIENCE = "Overall Patient Experience Ratings";
        public const string REPORT_PROFILE_PAYER_COST = "All Payer Cost";
        public const string REPORT_PROFILE_COST_TO_CHARGE_MEDICARE = "Cost Charge Data (Medicare)";
        public const string REPORT_PROFILE_COST_TO_CHARGE_ALL_PATIENTS = "Cost Charge Data (All Patients)";
        public const string REPORT_PROFILE_BASIC = "Basic Descriptive Data";
    }

    [Flags, Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Description("Hospital Profile Display Items")]
    public enum ReportProfileDisplayItem : uint
    {
        None = 0x00,
        [Description(Constants.REPORT_PROFILE_MAP)]
        Map = 0x01,
        [Description(Constants.REPORT_PROFILE_PATIENT_EXPERIENCE)]
        PatientExperience = 0x02,
        [Description(Constants.REPORT_PROFILE_PAYER_COST)]
        PayerCost = 0x04,
        [Description(Constants.REPORT_PROFILE_COST_TO_CHARGE_MEDICARE)]
        CostToChargeMedicare = 0x08,
        [Description(Constants.REPORT_PROFILE_BASIC)]
        Basic = 0x10,
        [Description(Constants.REPORT_PROFILE_COST_TO_CHARGE_ALL_PATIENTS)]
        CostToChargeAllPatients = 0x20,
    }


    [Serializable]
    [DebuggerStepThrough]
    [XmlRoot(Namespace = "", IsNullable = true)]
    public class ReportManifest : Entity<int>
    {
        public ReportManifest()
        { }

        public ReportManifest(string name)
        {
            Name = name;
        }

        public void AddColumn(RptManifestColumn column)
        {
            Columns.Add(column);
            column.Report = this;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Columns = new List<RptManifestColumn>();
            Datasets = new List<ReportDataset>();
            Filters = new List<RptManifestFilter>();
        }

        private string _interpretationText;
        private XmlDocument _interpretationTextXml;

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [XmlAttribute("RptId")]
        public string RptId { get; set; }

        /// <summary>
        /// Gets or sets the audiences.
        /// </summary>
        /// <value>
        /// The audiences.
        /// </value>
        [XmlArray("Audiences")]
        [XmlArrayItem("Audience")]
        public virtual List<RptManifestAudience> Audiences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show interpretation text].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show interpretation text]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool ShowInterpretationText { get; set; }

        /// <summary>
        /// Gets or sets the interpretation text.
        /// </summary>
        /// <value>
        /// The interpretation text.
        /// </value>
        public string InterpretationText { get; set; }

        /// <summary>
        /// Gets or sets the report attributes.
        /// </summary>
        /// <value>
        /// The report attributes.
        /// </value>
        public virtual ReportAttributeOption ReportAttributes { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [XmlAttribute]
        public virtual ReportCategory Category { get; set; }


        /// <summary>
        /// Gets or sets the isTrending.
        /// </summary>
        /// <value>
        /// Report IsTrending flag.
        /// </value>
        [XmlAttribute]
        public virtual bool IsTrending { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [XmlElement]
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon sets.
        /// </summary>
        /// <value>
        /// The icon sets.
        /// </value>
        [XmlArray("IconSets"),
         XmlArrayItem("IconSet")]
        public List<ReportIconSet> IconSets { get; set; }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        [XmlArray("Filters")]
        [XmlArrayItem("Filter", Type = typeof(RptManifestFilter))]
        public List<RptManifestFilter> Filters { get; set; }

        /// <summary>
        /// Gets or sets the datasets.
        /// </summary>
        /// <value>
        /// The datasets.
        /// </value>
        [XmlArrayItem("Dataset")]
        public List<ReportDataset> Datasets { get; set; }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        [XmlArray("Columns")]
        [XmlArrayItem("Column", Type = typeof(RptManifestColumn))]
        public virtual List<RptManifestColumn> Columns { get; set; }

        /// <summary>
        /// Gets or sets the ReportWebsitePages.
        /// </summary>
        [XmlArray("WebsitePages")]
        [XmlArrayItem("WebsitePage", Type = typeof(ReportWebsitePage))]
        public virtual List<ReportWebsitePage> WebsitePages { get; set; }

        /// <summary>
        /// Gets or sets the preview image.
        /// </summary>
        /// <value>
        /// The preview image.
        /// </value>
        [XmlAttribute]
        public string PreviewImage { get; set; }


        /// <summary>
        /// Gets or sets the consumer preview image.
        /// </summary>
        /// <value>
        /// The consumer preview image.
        /// </value>
        [XmlAttribute]
        public string ConsumerPreviewImage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires CMS provider identifier].
        /// </summary>
        /// <value>
        /// <c>true</c> if [requires CMS provider identifier]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool RequiresCmsProviderId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires cost to charge ratio].
        /// </summary>
        /// <value>
        /// <c>true</c> if [requires cost to charge ratio]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool RequiresCostToChargeRatio { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        [XmlElement]
        public string Footer { get; set; }

        /// <summary>
        /// Gets or sets the report output SQL.
        /// </summary>
        /// <value>
        /// The report output SQL.
        /// </value>
        [XmlElement("ReportOutputSQL")]
        public string ReportOutputSql { get; set; }

        /// <summary>
        /// Gets or sets the name of the report output file.
        /// </summary>
        /// <value>
        /// The name of the output file.
        /// </value>
        [XmlAttribute("OutputFileName")]
        public string OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the report output js namespace.
        /// </summary>
        /// <value>
        /// The output js namespace.
        /// </value>
        [XmlAttribute("OutputJsNamespace")]
        public string OutputJsNamespace { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            //return XmlHelper.Serialize(this, typeof (ReportManifest)).OuterXml;
            //using (var str = new MemoryStream())
            //{
            //    var ser = new XmlSerializer(typeof(ReportManifest));
            //    ser.Serialize(str, this);
            //    str.Flush();
            //    str.Position = 0;
            //    return UTF8Encoding.UTF8.GetString(str.ToArray());
            //}

            return string.Format("Report Manifest \"{0}\" - Report Id: {1}", Name, RptId);
        }

        [DebuggerStepThrough]
        public static ReportManifest Deserialize(string p)
        {
            if (p.Contains("AudienceType=\"AllAudiences\""))
                p = p.Replace("AudienceType=\"AllAudiences\"", "AudienceType=\"Professionals\"");

            ReportManifest reportManifest;
            using (var rdr = new StringReader(p))
            {
                var ser = new XmlSerializer(typeof(ReportManifest));
                reportManifest = ser.Deserialize(rdr) as ReportManifest;
            }

            return reportManifest;
        }

        [DebuggerStepThrough]
        public static XmlDocument Serialize(ReportManifest manifest)
        {
            var manifestXml = new XmlDocument();
            using (var memstream = new MemoryStream())
            {
                //Create a XmlWriter and point it at the stringbuilder for the location to write the serialized Xml
                using (var xw = XmlWriter.Create(memstream))
                {
                    var ser = new XmlSerializer(typeof(ReportManifest));
                    //Serialize the object into Xml and write the Xml to the in memory stringbuilder
                    ser.Serialize(xw, manifest);

                    memstream.Position = 0;

                    //Load the Xml from memory to the XmlDocument and return
                    manifestXml.Load(memstream);
                    return manifestXml;
                }
            }
        }

        public IEnumerable<ComparisonKeyIconSet> ToComparisonKeyIconSet(IEnumerable<ReportIconSet> iconsets, Report report)
        {
            foreach (var iconset in iconsets.ToList())
            {
                var iconSetPath = Path.Combine(MonahrqContext.BinFolderPath, "Resources", iconset.IconType.ToString());
                yield return new ComparisonKeyIconSet(report, iconset.IconType.ToString())
                {
                    AverageImage = new Uri(Path.Combine(iconSetPath, "average.gif"), UriKind.RelativeOrAbsolute),
                    BelowImage = new Uri(Path.Combine(iconSetPath, "below.gif"), UriKind.RelativeOrAbsolute),
                    BestImage = new Uri(Path.Combine(iconSetPath, "best.gif"), UriKind.RelativeOrAbsolute),
                    BetterImage = new Uri(Path.Combine(iconSetPath, "better.gif"), UriKind.RelativeOrAbsolute),
                    NotEnoughDataImage = new Uri(Path.Combine(iconSetPath, "notenoughdata.gif"), UriKind.RelativeOrAbsolute)
                };
            }
        }

        public DateTime FileLastModifiedDate { get; set; }
    }

    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public enum IconSetEnum
    {
        [Description("Icon Set 1")]
        IconSet1 = 0,
        [Description("Icon Set 2")]
        IconSet2 = 1
    }

    [Serializable]
    public class ReportIconSet
    {
        /// <summary>
        /// Gets or sets the type of the icon.
        /// </summary>
        /// <value>
        /// The type of the icon.
        /// </value>
        [XmlAttribute]
        public IconSetEnum IconType { get; set; }
    }
}
