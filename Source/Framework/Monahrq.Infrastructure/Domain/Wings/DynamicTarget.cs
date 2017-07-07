using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Domain.Common;

namespace Monahrq.Infrastructure.Domain.Wings
{
    #region Old Code to be removed after successful merge
    //[Serializable, 
    // XmlRoot(ElementName = "Wing", Namespace = "", IsNullable = false)]
    //public class DynamicWing
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="DynamicWing"/> class.
    //    /// </summary>
    //    public DynamicWing()
    //    {
    //        DisplayOrder = 999;
    //    }

    //    /// <summary>
    //    /// Gets or sets the target.
    //    /// </summary>
    //    /// <value>
    //    /// The target.
    //    /// </value>
    //    public WingTarget Target { get; set; }

    //    /// <summary>
    //    /// Gets or sets the unique identifier.
    //    /// </summary>
    //    /// <value>
    //    /// The unique identifier.
    //    /// </value>
    //    //[XmlAttribute]
    //    public Guid Id { get; set; }

    //    /// <summary>
    //    /// Gets or sets the name.
    //    /// </summary>
    //    /// <value>
    //    /// The name.
    //    /// </value>
    //    [XmlAttribute]
    //    public string Name { get; set; }

    //    /// <summary>
    //    /// Gets or sets the description.
    //    /// </summary>
    //    /// <value>
    //    /// The description.
    //    /// </value>
    //    [XmlAttribute]
    //    public string Description { get; set; }

    //    /// <summary>
    //    /// Gets or sets the depends on module names.
    //    /// </summary>
    //    /// <value>
    //    /// The depends on module names.
    //    /// </value>
    //                                                [XmlAttribute]
    //                                                public string DependsOnModuleNames { get; set; }

    //                                                /// <summary>
    //                                                /// Gets or sets the display order.
    //                                                /// </summary>
    //                                                /// <value>
    //                                                /// The display order.
    //                                                /// </value>
    //                                                [XmlAttribute, DefaultValue(999)]
    //                                                public int DisplayOrder { get; set; }


    //                                            }
    #endregion

    [Serializable,
     XmlRoot(ElementName = "Target", Namespace = "", IsNullable = false)]
    public class DynamicTarget
    {
        private string _dbSchemaName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTarget"/> class.
        /// </summary>
        public DynamicTarget()
        {
            IsReferenceTarget = false;
            DisplayOrder = 999;
            //ImportSteps = new List<WingTargetStep>();
            Columns = new List<DynamicTargetColumn>();
        }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        [XmlArrayItem("Column", IsNullable = false)]
        public List<DynamicTargetColumn> Columns { get; set; }

        /// <summary>
        /// Gets or sets the reports.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        [XmlArrayItem("ReportManifest", IsNullable = false)]
        public List<ReportManifest> Reports { get; set; }


        [XmlArray("Measures"), 
         XmlArrayItem("Measure", IsNullable = false)]
        public List<DynamicTargetMeasure> Measures { get; set; }

            /// <summary>
        /// Gets or sets the import steps.
        /// </summary>
        /// <value>
        /// The import steps.
        /// </value>
        //[XmlArrayItem("Step", IsNullable = false)]
        [XmlElement("ImportSteps", Type = typeof(DynamicTargetStep), IsNullable = true)]
        public DynamicTargetStep ImportSteps { get; set; }

        /// <summary>
        /// Gets or sets the create table script.
        /// </summary>
        /// <value>
        /// The create table script.
        /// </value>
        [XmlElement(IsNullable = true)]
        public string CreateTableScript { get; set; }

        /// <summary>
        /// Gets or sets the import SQL script.
        /// </summary>
        /// <value>
        /// The import SQL script.
        /// </value>
        [XmlElement(IsNullable = true)]
        public string ImportSQLScript { get; set; }

        /// <summary>
        /// Gets or sets the add measures script.
        /// </summary>
        /// <value>
        /// The add measures script.
        /// </value>
        [XmlElement(IsNullable = true)]
        public string AddMeasuresScript { get; set; }

        /// <summary>
        /// Gets or sets the add reports script.
        /// </summary>
        /// <value>
        /// The add reports script.
        /// </value>
        [XmlElement(IsNullable = true)]
        public string AddReportsScript { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [XmlAttribute]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the database schema.
        /// </summary>
        /// <value>
        /// The name of the database schema.
        /// </value>
        [XmlAttribute]
        public string DbSchemaName
        {
            get
            {
                if (!string.IsNullOrEmpty(_dbSchemaName) && !_dbSchemaName.StartsWith("Targets_"))
                    _dbSchemaName = string.Format("Targets_{0}", _dbSchemaName);

                return _dbSchemaName;
            }
            set { _dbSchemaName = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reference target.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is reference target; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute,
         DefaultValue(false)]
        public bool IsReferenceTarget { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple imports].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow multiple imports]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute,
         DefaultValue(true)]
        public virtual bool AllowMultipleImports { get; set; }

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        /// <value>
        /// The display order.
        /// </value>
        [XmlAttribute, DefaultValue(999)]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the publisher.
        /// </summary>
        /// <value>
        /// The publisher.
        /// </value>
        [XmlAttribute]
        public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the publisher email.
        /// </summary>
        /// <value>
        /// The publisher email.
        /// </value>
        [XmlAttribute]
        public string PublisherEmail { get; set; }

        /// <summary>
        /// Gets or sets the publisher website.
        /// </summary>
        /// <value>
        /// The publisher website.
        /// </value>
        [XmlAttribute]
        public string PublisherWebsite { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [XmlAttribute]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute, DefaultValue(false)]
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets the wing target XML file path.
        /// </summary>
        /// <value>
        /// The wing target XML path.
        /// </value>
        [XmlIgnore]
        public string WingTargetXmlFilePath { get; set; }

        /// <summary>
        /// Gets or sets the wing target dataset tempate (.csv) file path.
        /// </summary>
        /// <value>
        /// The wing target tempate file path.
        /// </value>
        [XmlAttribute(AttributeName = "TempateFileName")]
        public string TempateFileName { get; set; } 
    }

    /// <remarks/>
    [Serializable, 
     DebuggerStepThrough]
    public class DynamicTargetColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTargetColumn"/> class.
        /// </summary>
        public DynamicTargetColumn()
        {
            Scope = DynamicScopeEnum.None;
            IsRequired = false;
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
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        [XmlAttribute]
        public DataTypeEnum DataType { get; set; }

        /// <summary>
        /// Gets the type of the database.
        /// </summary>
        /// <value>
        /// The type of the database.
        /// </value>
        [XmlIgnore]
        public DbType DbType
        {
            get { return ConvertToDbType(DataType); }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        [XmlAttribute, DefaultValue(DynamicScopeEnum.None)]
        public DynamicScopeEnum Scope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute, DefaultValue(false)]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unique.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is unique; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute, DefaultValue(false)]
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        [XmlAttribute, DefaultValue(-1)]
        public int Scale { get; set; }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>
        /// The percision.
        /// </value>
        [XmlAttribute, DefaultValue(-1)]
        public int Precision { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        [XmlAttribute, DefaultValue(-1)]
        public int Length { get; set; }

        /// <summary>
        /// Converts the values.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <returns></returns>
        private DbType ConvertToDbType(DataTypeEnum dataType)
        {
            return (DbType)Enum.Parse(typeof(DbType), dataType.ToString(), true);
        }
    }

    [Serializable, DebuggerStepThrough]
    //[XmlRoot(Namespace = "", IsNullable = false)]
    public class DynamicTargetMeasure  //: NullValueSerializableEntity
    {
        /// <summary>
        /// Gets or sets the measure title.
        /// </summary>
        /// <value>
        /// The measure title.
        /// </value>
        [XmlElement]
        public DynamicMeasureTitle MeasureTitle { get; set; }

        /// <summary>
        /// Gets or sets the state peer benchmark.
        /// </summary>
        /// <value>
        /// The state peer benchmark.
        /// </value>
        [XmlElement(typeof(DynamicStatePeerBenchmark))]
        public DynamicStatePeerBenchmark StatePeerBenchmark { get; set; }

        /// <summary>
        /// Gets or sets the national benchmark.
        /// </summary>
        /// <value>
        /// The national benchmark.
        /// </value>
        [XmlAttribute]
        public string NationalBenchmark { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [XmlElement]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the footnotes.
        /// </summary>
        /// <value>
        /// The footnotes.
        /// </value>
        [XmlElement]
        public string Footnotes { get; set; }

        /// <summary>
        /// Gets or sets the topics.
        /// </summary>
        /// <value>
        /// The topics.
        /// </value>
        [XmlArray("Topics"), 
         XmlArrayItem("Topic", IsNullable = false)]
        public DynamicTopic[] Topics { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the measure.
        /// </summary>
        /// <value>
        /// The type of the measure.
        /// </value>
        [XmlAttribute]
        public string MeasureType { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        [XmlAttribute]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [higher scores are better].
        /// </summary>
        /// <value>
        /// <c>true</c> if [higher scores are better]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool HigherScoresAreBetter { get; set; }

        /// <summary>
        /// Gets or sets the NQF identifier.
        /// </summary>
        /// <value>
        /// The NQF identifier.
        /// </value>
        [XmlAttribute("NQFID")]
        public string NqfId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [NQF endorsed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [NQF endorsed]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("NQFEndorsed")]
        public bool NqfEndorsed { get; set; }

        /// <summary>
        /// Gets or sets the risk adjusted method.
        /// </summary>
        /// <value>
        /// The risk adjusted method.
        /// </value>
        [XmlAttribute]
        public string RiskAdjustedMethod { get; set; }

        /// <summary>
        /// Gets or sets the scale target.
        /// </summary>
        /// <value>
        /// The scale target.
        /// </value>
        [XmlAttribute]
        public string ScaleTarget { get; set; }

        /// <summary>
        /// Gets or sets the scale by.
        /// </summary>
        /// <value>
        /// The scale by.
        /// </value>
        [XmlAttribute]
        public string ScaleBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [perform margin suppression].
        /// </summary>
        /// <value>
        /// <c>true</c> if [perform margin suppression]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool PerformMarginSuppression { get; set; }

        /// <summary>
        /// Gets or sets the suppression denominator.
        /// </summary>
        /// <value>
        /// The suppression denominator.
        /// </value>
        [XmlAttribute]
        public string SuppressionDenominator { get; set; }

        /// <summary>
        /// Gets or sets the suppression numerator.
        /// </summary>
        /// <value>
        /// The suppression numerator.
        /// </value>
        [XmlAttribute]
        public string SuppressionNumerator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is existing measure.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is existing measure; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsExistingMeasure { get; set; }

        /// <summary>
        /// Gets or sets the measure code.
        /// </summary>
        /// <value>
        /// The measure code.
        /// </value>
        [XmlAttribute]
        public string MeasureCode { get; set; }

        /// <summary>
        /// Gets or sets the upper bound.
        /// </summary>
        /// <value>
        /// The upper bound.
        /// </value>
        [XmlAttribute]
        public string UpperBound { get; set; }

        /// <summary>
        /// Gets or sets the lower bound.
        /// </summary>
        /// <value>
        /// The lower bound.
        /// </value>
        [XmlAttribute]
        public string LowerBound { get; set; }

        [XmlAttribute]
        public string Url { get; set; }

        [XmlAttribute]
        public string UrlTitle { get; set; }

        [XmlElement]
        public virtual string MoreInformation { get; set; }

        [XmlAttribute]
        public virtual bool SupportsCost { get; set; }
        [XmlElement]
        public virtual string ConsumerDescription { get; set; }

        
    }

    [Serializable, DebuggerStepThrough]
    //[XmlType(AnonymousType = true)]
    public class DynamicMeasureTitle
    {
        /// <summary>
        /// Gets or sets the plain title.
        /// </summary>
        /// <value>
        /// The plain.
        /// </value>
        public string Plain { get; set; }

        /// <summary>
        /// Gets or sets the consumer plain title.
        /// </summary>
        /// <value>
        /// The consumer plain title.
        /// </value>
        public string ConsumerPlain { get; set; }

        /// <summary>
        /// Gets or sets the clinical title.
        /// </summary>
        /// <value>
        /// The clinical.
        /// </value>
        public string Clinical { get; set; }

        /// <summary>
        /// Gets or sets the selected title type.
        /// </summary>
        /// <value>
        /// The selected.
        /// </value>
        [XmlAttribute]
        public SelectedMeasuretitleEnum Selected { get; set; }
    }

    [Serializable, DebuggerStepThrough]
    //[XmlType(AnonymousType = true)]
    public class DynamicStatePeerBenchmark //: NullValueSerializableEntity
    {
        /// <summary>
        /// Gets or sets the provided benchmark.
        /// </summary>
        /// <value>
        /// The provided benchmark.
        /// </value>
        [XmlAttribute]
        public string ProvidedBenchmark { get; set; }

        /// <summary>
        /// Gets or sets the calculation method.
        /// </summary>
        /// <value>
        /// The calculation method.
        /// </value>
        [XmlAttribute]
        public StatePeerBenchmarkCalculationMethod CalculationMethod { get; set; }

        //public override void ReadXml(XmlReader reader)
        //{
        //    var providedBenchmark = reader.GetAttribute("ProvidedBenchmark");

        //    reader.Read();

        //    ProvidedBenchmark = ConvertToNullable<decimal>(providedBenchmark);
        //}
    }

    [Serializable, DebuggerStepThrough]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class DynamicTopic
    {
        /// <summary>
        /// Gets or sets the sub topics.
        /// </summary>
        /// <value>
        /// The sub topics.
        /// </value>
        [XmlArray("SubTopics"),
         XmlArrayItem("SubTopic", IsNullable = false)]
        public DynamicSubTopic[] SubTopics { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement]
        public string LongTitle { get; set; }

        [XmlElement]
        public string Description { get; set; }

        [XmlAttribute]
        public TopicTypeEnum Type { get; set; }

        [XmlAttribute]
        public string WingTargetName { get; set; }

        [XmlAttribute]
        public TopicCategoryTypeEnum CategoryType { get; set; }

        [XmlAttribute]
        public string ConsumerDescription { get; set; }

        [XmlIgnore]
        public DateTime? DateCreated { get; set; }

        [XmlArray("Facts")]
        [XmlArrayItem("Fact", typeof(DynamicTopicFact), IsNullable = true)]
        public List<DynamicTopicFact> Facts { get; set; }

        //public virtual string TopicFacts1 { get; set; }
        //public virtual string TopicFacts2 { get; set; }
        //public virtual string TopicFacts3 { get; set; }
        //public virtual string TipsChecklist { get; set; }
        //public virtual string TopicIcon { get; set; }
    }

    [Serializable]
    public class DynamicTopicFact
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ImagePath { get; set; }
        [XmlElement]
        public string Text { get; set; }
        [XmlElement]
        public string CitationText { get; set; }
    }

    //[Serializable]
    //public enum DaynamicTopicType
    //{
    //    Hospital = 0,
    //    NursingHome = 1,
    //    Physician
    //}

    [Serializable, DebuggerStepThrough]
    //[XmlType(AnonymousType = true)]
    public class DynamicSubTopic
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }
    }

    public abstract class NullValueSerializableEntity : IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public abstract void ReadXml(XmlReader reader);

        public void WriteXml(XmlWriter writer)
        {}

        protected T? ConvertToNullable<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return null;

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFrom(value);
            }
            catch
            {
                // Conversion failed so return null value
                return null;
            }
        }
    }
}
