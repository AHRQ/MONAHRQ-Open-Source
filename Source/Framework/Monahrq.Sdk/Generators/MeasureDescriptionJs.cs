using Monahrq.Infrastructure.Entities.Domain.Measures;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The data transfer object used in the Json data serialization process for measure descriptions.
    /// </summary>
    [DataContract]
    public class MeasureDescriptionJs
    {
        #region Fields and Constants

        private string _higerScoresAreBetterDescription;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [IgnoreDataMember]
        [JsonIgnore]
        [XmlIgnore]
        public string FileName { get { return string.Format("{0}_{1}.js", GetType().Name.Replace("Js", ""), MeasureId); } }

        /// <summary>
        /// Gets or sets the measure identifier.
        /// </summary>
        /// <value>
        /// The measure identifier.
        /// </value>
        [DataMember(Name = "MeasureID")]
        public int MeasureId { get; set; }

        /// <summary>
        /// Gets or sets the topic identifier.
        /// </summary>
        /// <value>
        /// The topic identifier.
        /// </value>
        [DataMember(Name = "TopicID")]
        public int? TopicId { get; set; }

        /// <summary>
        /// Gets or sets the name of the measures.
        /// </summary>
        /// <value>
        /// The name of the measures.
        /// </value>
        [DataMember(Name = "MeasuresName")]
        public string MeasuresName { get; set; }

        /// <summary>
        /// Gets or sets the measure source.
        /// </summary>
        /// <value>
        /// The measure source.
        /// </value>
        [DataMember(Name = "MeasureSource")]
        public string MeasureSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the measure.
        /// </summary>
        /// <value>
        /// The type of the measure.
        /// </value>
        [DataMember(Name = "MeasureType")]
        public string MeasureType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [higher scores are better].
        /// </summary>
        /// <value>
        /// <c>true</c> if [higher scores are better]; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "HigherScoresAreBetter")]
        public bool HigherScoresAreBetter { get; set; }

        /// <summary>
        /// Gets or sets the higer scores are better description.
        /// </summary>
        /// <value>
        /// The higer scores are better description.
        /// </value>
        [DataMember(Name = "HigherScoresAreBetterDescription")]
        public string HigerScoresAreBetterDescription { get { return _higerScoresAreBetterDescription ?? string.Empty; } set { _higerScoresAreBetterDescription = value; } }

        /// <summary>
        /// Gets or sets a value indicating whether [in score].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in score]; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "InScore")]
        public bool InScore { get; set; }

        /// <summary>
        /// Gets or sets the nat label.
        /// </summary>
        /// <value>
        /// The nat label.
        /// </value>
        [DataMember(Name = "NatLabel")]
        public string NatLabel { get; set; }

        /// <summary>
        /// Gets or sets the nat rate.
        /// </summary>
        /// <value>
        /// The nat rate.
        /// </value>
        [DataMember(Name = "NatRate")]
        public decimal? NatRate { get; set; }

        /// <summary>
        /// Gets or sets the peer label.
        /// </summary>
        /// <value>
        /// The peer label.
        /// </value>
        [DataMember(Name = "PeerLabel")]
        public string PeerLabel { get; set; }

        /// <summary>
        /// Gets or sets the peer rate.
        /// </summary>
        /// <value>
        /// The peer rate.
        /// </value>
        [DataMember(Name = "PeerRate")]
        public decimal? PeerRate { get; set; }

        /// <summary>
        /// Gets or sets the county label.
        /// </summary>
        /// <value>
        /// The county label.
        /// </value>
        [DataMember(Name = "CountyLabel")]
        public string CountyLabel { get; set; }

        /// <summary>
        /// Gets or sets the county rate.
        /// </summary>
        /// <value>
        /// The county rate.
        /// </value>
        [DataMember(Name = "CountyRate")]
        public string CountyRate { get; set; }

        /// <summary>
        /// Gets or sets the footnote.
        /// </summary>
        /// <value>
        /// The footnote.
        /// </value>
        [DataMember(Name = "Footnote")]
        public string Footnote { get; set; }

        /// <summary>
        /// Gets or sets the selected title.
        /// </summary>
        /// <value>
        /// The selected title.
        /// </value>
        [DataMember(Name = "SelectedTitle")]
        public string SelectedTitle { get; set; }

        /// <summary>
        /// Gets or sets the plain title.
        /// </summary>
        /// <value>
        /// The plain title.
        /// </value>
        [DataMember(Name = "PlainTitle")]
        public string PlainTitle { get; set; }

        /// <summary>
        /// Gets or sets the clinical title.
        /// </summary>
        /// <value>
        /// The clinical title.
        /// </value>
        [DataMember(Name = "ClinicalTitle")]
        public string ClinicalTitle { get; set; }

        /// <summary>
        /// Gets or sets the measure description.
        /// </summary>
        /// <value>
        /// The measure description.
        /// </value>
        [DataMember(Name = "MeasureDescription")]
        public string MeasureDescription { get; set; }

        /// <summary>
        /// Gets or sets the selected title consumer.
        /// </summary>
        /// <value>
        /// The selected title consumer.
        /// </value>
        [DataMember(Name = "SelectedTitleConsumer")]
        public string SelectedTitleConsumer { get; set; }

        /// <summary>
        /// Gets or sets the plain title consumer.
        /// </summary>
        /// <value>
        /// The plain title consumer.
        /// </value>
        [DataMember(Name = "PlainTitleConsumer")]
        public string PlainTitleConsumer { get; set; }

        /// <summary>
        /// Gets or sets the measure description consumer.
        /// </summary>
        /// <value>
        /// The measure description consumer.
        /// </value>
        [DataMember(Name = "MeasureDescriptionConsumer")]
        public string MeasureDescriptionConsumer { get; set; }

        /// <summary>
        /// Gets or sets the bullets.
        /// </summary>
        /// <value>
        /// The bullets.
        /// </value>
        [DataMember(Name = "Bullets")]
        public string Bullets { get; set; }

        /// <summary>
        /// Gets or sets the statistics available.
        /// </summary>
        /// <value>
        /// The statistics available.
        /// </value>
        [DataMember(Name = "StatisticsAvailable")]
        public string StatisticsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the more information.
        /// </summary>
        /// <value>
        /// The more information.
        /// </value>
        [DataMember(Name = "MoreInformation")]
        public string MoreInformation { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember(Name = "URL")]
        public string URL { get; set; }

        /// <summary>
        /// Gets or sets the URL title.
        /// </summary>
        /// <value>
        /// The URL title.
        /// </value>
        [DataMember(Name = "URLTitle")]
        public string URLTitle { get; set; }

        /// <summary>
        /// Gets or sets the data source URL.
        /// </summary>
        /// <value>
        /// The data source URL.
        /// </value>
        [DataMember(Name = "DataSourceURL")]
        public string DataSourceURL { get; set; }

        /// <summary>
        /// Gets or sets the data source URL title.
        /// </summary>
        /// <value>
        /// The data source URL title.
        /// </value>
        [DataMember(Name = "DataSourceURLTitle")]
        public string DataSourceURLTitle { get; set; }

        //[DataMember(Name = "Heading")]
        //public string Heading { get; set; }

        /// <summary>
        /// Gets or sets the type of the question.
        /// </summary>
        /// <value>
        /// The type of the question.
        /// </value>
        [DataMember(Name = "CAHPSQuestionType")]
        public string QuestionType { get; set; }

        /// <summary>
        /// Gets or sets the topic ids.
        /// </summary>
        /// <value>
        /// The topic ids.
        /// </value>
        [IgnoreDataMember]
        [JsonIgnore]
        [XmlIgnore]
        public List<int> TopicIds { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureDescriptionJs"/> class.
        /// </summary>
        /// <param name="measure">The measure.</param>
        public MeasureDescriptionJs(Measure measure)
        {
            MeasureId = measure.Id;
            MeasuresName = measure.MeasureCode;
            MeasureSource = measure.Source;
            MeasureType = measure.MeasureType;
            SelectedTitle = measure.MeasureTitle.Selected == SelectedMeasuretitleEnum.Plain ? measure.MeasureTitle.Plain : measure.MeasureTitle.Clinical;
            PlainTitle = measure.MeasureTitle.Plain;
            ClinicalTitle = measure.MeasureTitle.Clinical;
            MeasureDescription = measure.Description;
            MoreInformation = measure.MoreInformation;
            URL = measure.Url;
            URLTitle = measure.UrlTitle;
            DataSourceURL = "http://www.qualityindicators.ahrq.gov";
            DataSourceURLTitle = "AHRQ Quality Indicator";
            SelectedTitleConsumer = measure.ConsumerPlainTitle;
            PlainTitleConsumer = measure.ConsumerPlainTitle;
            MeasureDescriptionConsumer = measure.ConsumerDescription;

            TopicIds = measure.Topics.Select(x => x.Id).Distinct().ToList();

            HigherScoresAreBetter = measure.HigherScoresAreBetter;
            InScore = measure.UsedInCalculations;

            NatLabel = "Nationwide Mean";
            NatRate = measure.NationalBenchmark;
            PeerLabel = "State Mean";
            PeerRate = measure.StatePeerBenchmark.ProvidedBenchmark;
            CountyLabel = "County Mean";
            CountyRate = null;
            Footnote = measure.Footnotes;
        }

        #endregion

    }
}
