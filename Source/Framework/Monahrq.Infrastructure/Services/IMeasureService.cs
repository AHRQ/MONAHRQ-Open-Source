using System;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Services;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
    /// <summary>
    /// The Monahrq measure domain service interface/contract.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.IDataServiceBase" />
    public interface IMeasureService : IDataServiceBase
    {
        /// <summary>
        /// Gets the measure.
        /// </summary>
        /// <param name="measureCode">The measure code.</param>
        /// <returns></returns>
        Measure GetMeasure(string measureCode);
        /// <summary>
        /// Adds the topic.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="longTitle">The long title.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        Topic AddTopic(TopicCategory category, string name, string longTitle, string description);
        /// <summary>
        /// Gets the topic.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        Topic GetTopic(string category, string name);
        /// <summary>
        /// Adds the topic category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="longTitle">The long title.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        TopicCategory AddTopicCategory(string category, string longTitle, string description);
        /// <summary>
        /// Gets the topic category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        TopicCategory GetTopicCategory(string category);
        /// <summary>
        /// Imports the measures.
        /// </summary>
        /// <param name="wingName">Name of the wing.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="measureType">Type of the measure.</param>
        /// <returns></returns>
        bool ImportMeasures(string wingName, string fileName, Type measureType);
        /// <summary>
        /// Imports the measure topic file.
        /// </summary>
        /// <param name="wingName">Name of the wing.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="topicType">Type of the topic.</param>
        void ImportMeasureTopicFile(string wingName, string fileName, TopicTypeEnum topicType);
    }
}
