using Monahrq.Infrastructure.Entities.Domain.Measures;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The data transfer object used in the Json data serialization process for measure topics.
    /// </summary>
    [DataContract]
    public class MeasureTopicJs
    {
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public static string FileName { get { return "MeasureTopics"; } }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        public static string FileExtension { get { return ".js"; } }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember(Name = "TopicID", Order = 1)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the topic category identifier.
        /// </summary>
        /// <value>
        /// The topic category identifier.
        /// </value>
        [DataMember(Name = "TopicCategoryID", Order = 2)]
        public int TopicCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the measure topic name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "Name", Order = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the long title.
        /// </summary>
        /// <value>
        /// The long title.
        /// </value>
        [DataMember(Name = "LongTitle", Order = 4)]
        public string LongTitle { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember(Name = "Description", Order = 5)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [subset in score].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [subset in score]; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "SubsetInScore", Order = 6)]
        public bool SubsetInScore { get; set; }

        /// <summary>
        /// Gets or sets the overall measure.
        /// </summary>
        /// <value>
        /// The overall measure.
        /// </value>
        [DataMember(Name = "OverallMeasure", Order = 7)]
        public int OverallMeasure { get; set; }

        /// <summary>
        /// Gets or sets the measure ids.
        /// </summary>
        /// <value>
        /// The measure ids.
        /// </value>
        [DataMember(Name = "MeasureIDs", Order = 8)]
        public List<int> MeasureIds { get; set; }

        /// <summary>
        /// Gets or sets the topic identifier.
        /// </summary>
        /// <value>
        /// The topic identifier.
        /// </value>
        [IgnoreDataMember]
        public int TopicId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureTopicJs"/> class.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <param name="measureIds">The measure ids.</param>
        /// <param name="categoryId">The category identifier.</param>
        public MeasureTopicJs(Topic topic, List<int> measureIds, int? categoryId = null)
        {
            if (topic == null) return;

            var catergory = topic.Owner;
            TopicCategoryId = catergory != null && !categoryId.HasValue ? catergory.Id : categoryId.Value;   //topic Category Id
            TopicId = topic.Id; //topic id
            Name = topic.Name;
            LongTitle = topic.LongTitle;
            Description = topic.Description;
            MeasureIds = measureIds;
        }
    }
}
