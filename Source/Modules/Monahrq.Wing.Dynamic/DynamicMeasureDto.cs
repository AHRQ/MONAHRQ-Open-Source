using System;
using System.Linq;
using System.Runtime.Serialization;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Wing.Dynamic
{
    /// <summary>
    /// Data transfer object class for Dynamic module
    /// </summary>
    [Serializable, DataContract]
    public class DynamicMeasureDto
    {
        #region Properties
        /// <summary>
        /// Gets or sets the measure identifier.
        /// </summary>
        /// <value>
        /// The measure identifier.
        /// </value>
        [DataMember]
        public int MeasureID { get; set; }
        /// <summary>
        /// Gets or sets the name of the measures.
        /// </summary>
        /// <value>
        /// The name of the measures.
        /// </value>
        [DataMember]
        public string MeasuresName { get; set; }
        /// <summary>
        /// Gets or sets the measure source.
        /// </summary>
        /// <value>
        /// The measure source.
        /// </value>
        [DataMember]
        public string MeasureSource { get; set; }
        /// <summary>
        /// Gets or sets the type of the measure.
        /// </summary>
        /// <value>
        /// The type of the measure.
        /// </value>
        [DataMember]
        public string MeasureType{ get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether higher scores are better.
        /// </summary>
        /// <value>
        /// <c>true</c> if higher scores are better; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HigherScoresAreBetter { get; set; }
        /// <summary>
        /// Gets or sets the higher scores are better description.
        /// </summary>
        /// <value>
        /// The higher scores are better description.
        /// </value>
        [DataMember]
        public string HigherScoresAreBetterDescription { get; set; }
        /// <summary>
        /// Gets or sets the topics identifier.
        /// </summary>
        /// <value>
        /// The topics identifier.
        /// </value>
        [DataMember]
        public string TopicsID { get; set; }
        /// <summary>
        /// Gets or sets the nat label.
        /// </summary>
        /// <value>
        /// The nat label.
        /// </value>
        [DataMember]
        public string NatLabel { get; set; }
        /// <summary>
        /// Gets or sets the nat rate and CI.
        /// </summary>
        /// <value>
        /// The nat rate and CI.
        /// </value>
        [DataMember]
        public string NatRateAndCI { get; set; }
        /// <summary>
        /// Gets or sets the nat top10 label.
        /// </summary>
        /// <value>
        /// The nat top10 label.
        /// </value>
        [DataMember]
        public string NatTop10Label { get; set; }
        /// <summary>
        /// Gets or sets the nat top10.
        /// </summary>
        /// <value>
        /// The nat top10.
        /// </value>
        [DataMember]
        public string NatTop10 { get; set; }
        /// <summary>
        /// Gets or sets the peer label.
        /// </summary>
        /// <value>
        /// The peer label.
        /// </value>
        [DataMember]
        public string PeerLabel { get; set; }
        /// <summary>
        /// Gets or sets the peer rate and CI.
        /// </summary>
        /// <value>
        /// The peer rate and CI.
        /// </value>
        [DataMember]
        public string PeerRateAndCI { get; set; }
        /// <summary>
        /// Gets or sets the peer top10 label.
        /// </summary>
        /// <value>
        /// The peer top10 label.
        /// </value>
        [DataMember]
        public string PeerTop10Label { get; set; }
        /// <summary>
        /// Gets or sets the peer top10.
        /// </summary>
        /// <value>
        /// The peer top10.
        /// </value>
        [DataMember]
        public string PeerTop10 { get; set; }
        /// <summary>
        /// Gets or sets the footnote.
        /// </summary>
        /// <value>
        /// The footnote.
        /// </value>
        [DataMember]
        public string Footnote { get; set; }
        /// <summary>
        /// Gets or sets the bar header.
        /// </summary>
        /// <value>
        /// The bar header.
        /// </value>
        [DataMember]
        public string BarHeader { get; set; }
        /// <summary>
        /// Gets or sets the bar footer.
        /// </summary>
        /// <value>
        /// The bar footer.
        /// </value>
        [DataMember]
        public string BarFooter { get; set; }
        /// <summary>
        /// Gets or sets the col description1.
        /// </summary>
        /// <value>
        /// The col description1.
        /// </value>
        [DataMember]
        public string ColDesc1 { get; set; }
        /// <summary>
        /// Gets or sets the col description2.
        /// </summary>
        /// <value>
        /// The col description2.
        /// </value>
        [DataMember]
        public string ColDesc2 { get; set; }
        /// <summary>
        /// Gets or sets the col description3.
        /// </summary>
        /// <value>
        /// The col description3.
        /// </value>
        [DataMember]
        public string ColDesc3 { get; set; }
        /// <summary>
        /// Gets or sets the col description4.
        /// </summary>
        /// <value>
        /// The col desc4.
        /// </value>
        [DataMember]
        public string ColDesc4 { get; set; }
        /// <summary>
        /// Gets or sets the col desc5.
        /// </summary>
        /// <value>
        /// The col desc5.
        /// </value>
        [DataMember]
        public string ColDesc5 { get; set; }
        /// <summary>
        /// Gets or sets the col description6.
        /// </summary>
        /// <value>
        /// The col desc6.
        /// </value>
        [DataMember]
        public string ColDesc6 { get; set; }
        /// <summary>
        /// Gets or sets the col description7.
        /// </summary>
        /// <value>
        /// The col desc7.
        /// </value>
        [DataMember]
        public string ColDesc7 { get; set; }
        /// <summary>
        /// Gets or sets the col description8.
        /// </summary>
        /// <value>
        /// The col desc8.
        /// </value>
        [DataMember]
        public string ColDesc8 { get; set; }
        /// <summary>
        /// Gets or sets the col description9.
        /// </summary>
        /// <value>
        /// The col desc9.
        /// </value>
        [DataMember]
        public string ColDesc9 { get; set; }
        /// <summary>
        /// Gets or sets the col description10.
        /// </summary>
        /// <value>
        /// The col desc10.
        /// </value>
        [DataMember]
        public string ColDesc10 { get; set; }
        /// <summary>
        /// Gets or sets the nat col1.
        /// </summary>
        /// <value>
        /// The nat col1.
        /// </value>
        [DataMember]
        public string NatCol1 { get; set; }
        /// <summary>
        /// Gets or sets the nat col2.
        /// </summary>
        /// <value>
        /// The nat col2.
        /// </value>
        [DataMember]
        public string NatCol2 { get; set; }
        /// <summary>
        /// Gets or sets the nat col3.
        /// </summary>
        /// <value>
        /// The nat col3.
        /// </value>
        [DataMember]
        public string NatCol3 { get; set; }
        /// <summary>
        /// Gets or sets the nat col4.
        /// </summary>
        /// <value>
        /// The nat col4.
        /// </value>
        [DataMember]
        public string NatCol4 { get; set; }
        /// <summary>
        /// Gets or sets the nat col5.
        /// </summary>
        /// <value>
        /// The nat col5.
        /// </value>
        [DataMember]
        public string NatCol5 { get; set; }
        /// <summary>
        /// Gets or sets the nat col6.
        /// </summary>
        /// <value>
        /// The nat col6.
        /// </value>
        [DataMember]
        public string NatCol6 { get; set; }
        /// <summary>
        /// Gets or sets the nat col7.
        /// </summary>
        /// <value>
        /// The nat col7.
        /// </value>
        [DataMember]
        public string NatCol7 { get; set; }
        /// <summary>
        /// Gets or sets the nat col8.
        /// </summary>
        /// <value>
        /// The nat col8.
        /// </value>
        [DataMember]
        public string NatCol8 { get; set; }
        /// <summary>
        /// Gets or sets the nat col9.
        /// </summary>
        /// <value>
        /// The nat col9.
        /// </value>
        [DataMember]
        public string NatCol9 { get; set; }
        /// <summary>
        /// Gets or sets the nat col10.
        /// </summary>
        /// <value>
        /// The nat col10.
        /// </value>
        [DataMember]
        public string NatCol10 { get; set; }
        /// <summary>
        /// Gets or sets the peer col1.
        /// </summary>
        /// <value>
        /// The peer col1.
        /// </value>
        [DataMember]
        public string PeerCol1 { get; set; }
        /// <summary>
        /// Gets or sets the peer col2.
        /// </summary>
        /// <value>
        /// The peer col2.
        /// </value>
        [DataMember]
        public string PeerCol2 { get; set; }
        /// <summary>
        /// Gets or sets the peer col3.
        /// </summary>
        /// <value>
        /// The peer col3.
        /// </value>
        [DataMember]
        public string PeerCol3 { get; set; }
        /// <summary>
        /// Gets or sets the peer col4.
        /// </summary>
        /// <value>
        /// The peer col4.
        /// </value>
        [DataMember]
        public string PeerCol4 { get; set; }
        /// <summary>
        /// Gets or sets the peer col5.
        /// </summary>
        /// <value>
        /// The peer col5.
        /// </value>
        [DataMember]
        public string PeerCol5 { get; set; }
        /// <summary>
        /// Gets or sets the peer col6.
        /// </summary>
        /// <value>
        /// The peer col6.
        /// </value>
        [DataMember]
        public string PeerCol6 { get; set; }
        /// <summary>
        /// Gets or sets the peer col7.
        /// </summary>
        /// <value>
        /// The peer col7.
        /// </value>
        [DataMember]
        public string PeerCol7 { get; set; }
        /// <summary>
        /// Gets or sets the peer col8.
        /// </summary>
        /// <value>
        /// The peer col8.
        /// </value>
        [DataMember]
        public string PeerCol8 { get; set; }
        /// <summary>
        /// Gets or sets the peer col9.
        /// </summary>
        /// <value>
        /// The peer col9.
        /// </value>
        [DataMember]
        public string PeerCol9 { get; set; }
        /// <summary>
        /// Gets or sets the peer col10.
        /// </summary>
        /// <value>
        /// The peer col10.
        /// </value>
        [DataMember]
        public string PeerCol10 { get; set; }
        /// <summary>
        /// Gets or sets the selected title.
        /// </summary>
        /// <value>
        /// The selected title.
        /// </value>
        [DataMember]
        public string SelectedTitle { get; set; }
        /// <summary>
        /// Gets or sets the plain title.
        /// </summary>
        /// <value>
        /// The plain title.
        /// </value>
        [DataMember]
        public string PlainTitle { get; set; }
        /// <summary>
        /// Gets or sets the clinical title.
        /// </summary>
        /// <value>
        /// The clinical title.
        /// </value>
        [DataMember]
        public string ClinicalTitle { get; set; }
        /// <summary>
        /// Gets or sets the measure description.
        /// </summary>
        /// <value>
        /// The measure description.
        /// </value>
        [DataMember]
        public string MeasureDescription { get; set; }
        /// <summary>
        /// Gets or sets the bullets.
        /// </summary>
        /// <value>
        /// The bullets.
        /// </value>
        [DataMember]
        public string Bullets { get; set; }
        /// <summary>
        /// Gets or sets the statistics available.
        /// </summary>
        /// <value>
        /// The statistics available.
        /// </value>
        [DataMember]
        public string StatisticsAvailable { get; set; }
        /// <summary>
        /// Gets or sets the more information.
        /// </summary>
        /// <value>
        /// The more information.
        /// </value>
        [DataMember]
        public string MoreInformation { get; set; }
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public string URL { get; set; }
        /// <summary>
        /// Gets or sets the URL title.
        /// </summary>
        /// <value>
        /// The URL title.
        /// </value>
        [DataMember]
        public string URLTitle { get; set; }
        /// <summary>
        /// Gets or sets the data source URL.
        /// </summary>
        /// <value>
        /// The data source URL.
        /// </value>
        [DataMember]
        public string DataSourceURL { get; set; }
        /// <summary>
        /// Gets or sets the data source URL title.
        /// </summary>
        /// <value>
        /// The data source URL title.
        /// </value>
        [DataMember]
        public string DataSourceURLTitle { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Creates the specified measure.
        /// </summary>
        /// <param name="measure">The measure.</param>
        /// <returns></returns>
        public static DynamicMeasureDto Create(Measure measure)
        {
            var dto = new DynamicMeasureDto();

            dto.MeasureID = measure.Id;
            dto.MeasuresName = measure.Name;
            dto.MeasureSource = measure.Source;
            dto.MeasureType = measure.MeasureType;
            dto.HigherScoresAreBetter = measure.HigherScoresAreBetter;
            dto.HigherScoresAreBetterDescription = string.Empty;
            dto.TopicsID = string.Join(",", measure.Topics.Select(t => t.Id).ToArray());
            dto.NatLabel = string.Empty;
            dto.NatRateAndCI = string.Empty;
            dto.NatTop10Label = string.Empty;
            dto.NatTop10 = string.Empty;
            dto.PeerLabel = string.Empty;
            dto.PeerRateAndCI = string.Empty;
            dto.PeerTop10Label = string.Empty;
            dto.PeerTop10 = string.Empty;

            dto.Footnote = measure.Footnotes;
            dto.BarHeader = string.Empty;
            dto.BarFooter = string.Empty;
            dto.ColDesc1 = string.Empty;
            dto.ColDesc2 = string.Empty;
            dto.ColDesc3 = string.Empty;
            dto.ColDesc4 = string.Empty;
            dto.ColDesc5 = string.Empty;
            dto.ColDesc6 = string.Empty;
            dto.ColDesc7 = string.Empty;
            dto.ColDesc8 = string.Empty;
            dto.ColDesc9 = string.Empty;
            dto.ColDesc10 = string.Empty;
            dto.NatCol1 = string.Empty;
            dto.NatCol2 = string.Empty;
            dto.NatCol3 = string.Empty;
            dto.NatCol4 = string.Empty;
            dto.NatCol5 = string.Empty;
            dto.NatCol6 = string.Empty;
            dto.NatCol7 = string.Empty;
            dto.NatCol8 = string.Empty;
            dto.NatCol9 = string.Empty;
            dto.NatCol10 = string.Empty;
            dto.PeerCol1 = string.Empty;
            dto.PeerCol2 = string.Empty;
            dto.PeerCol3 = string.Empty;
            dto.PeerCol4 = string.Empty;
            dto.PeerCol5 = string.Empty;
            dto.PeerCol6 = string.Empty;
            dto.PeerCol7 = string.Empty;
            dto.PeerCol8 = string.Empty;
            dto.PeerCol9 = string.Empty;
            dto.PeerCol10 = string.Empty;
            dto.SelectedTitle = measure.MeasureTitle.Selected == SelectedMeasuretitleEnum.Plain ? measure.MeasureTitle.Plain : measure.MeasureTitle.Clinical;
            dto.PlainTitle = measure.MeasureTitle.Plain;
            dto.ClinicalTitle = measure.MeasureTitle.Clinical;
            dto.MeasureDescription = measure.Description;
            dto.Bullets = string.Empty;
            dto.StatisticsAvailable = string.Empty;
            dto.MoreInformation = measure.MoreInformation;
            dto.URL = measure.Url;
            dto.URLTitle = measure.UrlTitle;
            dto.DataSourceURL = measure.Owner.PublisherWebsite;
            dto.DataSourceURLTitle = measure.Owner.Publisher;

            return dto;
        }
        #endregion
    }
}