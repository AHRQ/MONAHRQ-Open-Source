using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Modules.Wings;
using NHibernate.Linq;

namespace Monahrq.Wing.Discharge
{
    public abstract class DischargeTargetedModule<T> : TargetedModuleWithMeasuresAndTopics<T>
           where T : DatasetRecord
    {
		#region 
		public override string MeasureFilePath { get { return null; } }
		public override string MeasureTopicFilePath { get { return null; } }
		public override bool RefreshDb() { return false; }
		#endregion


		/// <summary>
		/// Gets or sets the code n.
		/// </summary>
		/// <value>
		/// The code n.
		/// </value>
		protected virtual int CodeN { get; set; }

        /// <summary>
        /// Gets the measures XML.
        /// </summary>
        /// <value>
        /// The measures XML.
        /// </value>
        protected virtual XDocument MeasuresXml
        {
            get
            {
                using (var str = GetType().Assembly.GetManifestResourceStream(GetType(), "Measures.xml"))
                {
                    return XDocument.Load(str);
                }
            }
        }

        /// <summary>
        /// Imports the measures.
        /// </summary>
        protected override void ImportMeasures()
        {
            using (var session = SessionFactoryProvider.SessionFactory.OpenSession())
            {
                var target = session.Query<Target>().FirstOrDefault(t => t.Name == TargetAttribute.Name);
                var xElement = MeasuresXml.Element("topics");
                if (xElement != null)
                {
                    foreach (var categoryXml in xElement.Elements("category"))
                    {
                        var temp = categoryXml.ToCategory();
                        var category = session.Query<TopicCategory>().SingleOrDefault(cat => cat.Name == temp.Name);

                        if (category == null)
                        {
                            category = temp;
                        }
                        else
                        {
                            category.Description = temp.Description;
                            category.LongTitle = temp.LongTitle;
                        }

                        foreach (var topicXml in categoryXml.Elements("topic"))
                        {
                            var tempTopic = topicXml.ToTopic(category);
                            var topic = session.Query<Topic>().FirstOrDefault(t => t.Name == tempTopic.Name);

                            if (topic == null)
                            {
                                topic = tempTopic;
                            }
                            else
                            {
                                topic.Description = tempTopic.Description;
                                topic.LongTitle = tempTopic.LongTitle;
                            }

                            var codeN = 1;
                            foreach (var measureXml in topicXml.Elements("measure"))
                            {
                                using (var tx = session.BeginTransaction())
                                {
                                    var code = CreateMeasureCode(codeN++);
                                    var measure = measureXml.ToMeasure(target, code);
                                    measure.AddTopic(topic);
                                    topic.Measures.Add(measure);
                                    session.SaveOrUpdate(measure);
                                    session.SaveOrUpdate(topic);
                                    session.SaveOrUpdate(category);
                                    tx.Commit();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the measure prefix.
        /// </summary>
        /// <value>
        /// The measure prefix.
        /// </value>
        protected abstract string MeasurePrefix { get; }

        /// <summary>
        /// Creates the measure code.
        /// </summary>
        /// <param name="codeN">The code n.</param>
        /// <returns></returns>
        protected string CreateMeasureCode(int codeN)
        {
            var formatedNumber = (codeN < 10) 
                    ? "0"+ codeN.ToString(CultureInfo.InvariantCulture) 
                    : codeN.ToString(CultureInfo.InvariantCulture);

            return string.Format("{0}-{1}", MeasurePrefix, formatedNumber);
        }

        /// <summary>
        /// Imports the measure topics.
        /// </summary>
        protected override void ImportMeasureTopics()
        {
            //var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", MeasureFileName);
            //ImportMeasureTopicFile(temp);
        }
    }

    static class XHelpers
    {
        /// <summary>
        /// To the category.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static TopicCategory ToCategory(this XElement element)
        {
            var result = new TopicCategory(element.Attribute("name").Value)
                {
                    LongTitle = element.Attribute("longTitle").Value,
                    Description = element.Attribute("description").Value
                };
            return result;
        }

        /// <summary>
        /// To the topic.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public static Topic ToTopic(this XElement element, TopicCategory category)
        {
            var result = new Topic(category, element.Attribute("name").Value)
                {
                    LongTitle = element.Attribute("longTitle").Value,
                    Description = element.Attribute("description").Value
                };
            return result;
        }

        /// <summary>
        /// To the measure.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="target">The target.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public static Measure ToMeasure(this XElement element, Target target, string code)
        {
            var measureName = element.Attribute("name").Value;

            var measreType = typeof (HospitalMeasure);

            if (measureName.ContainsIgnoreCase("Region"))
                measreType = typeof (RegionMeasure);
            else if(measureName.ContainsIgnoreCase("County"))
                measreType = typeof(CountyMeasure);

            var result = Measure.CreateMeasure(measreType, target, code); //new Measure(target, code);

            if (result.MeasureTitle == null) result.MeasureTitle = new MeasureTitle();

            result.MeasureTitle.Plain = element.Attribute("name").Value;
            result.MeasureTitle.Clinical = element.Attribute("name").Value;
            result.MeasureTitle.Policy = element.Attribute("name").Value;

            result.NationalBenchmark = null;
            result.UpperBound = null;
            result.LowerBound = null;

            if (result.StatePeerBenchmark != null)
            {
                result.StatePeerBenchmark.ProvidedBenchmark = null;
            }
            result.SuppressionDenominator = null;
            result.SuppressionNumerator = null;

            result.Description = element.Attribute("name").Value;

            return result;

            #region Do not delete. Will be needed when extending Measures functionality
            // TODO: Add functionality for edit/validation rules in a correct manner. Jason
            //result.MeasureTitle.EditRule.Editability = Editability.EditWithWarning;
            //result.HigherScoresAreBetter.EditRule.Editability = Editability.Closed;
            //result.Description.EditRule.Editability = Editability.Open;
            //result.Footnotes.EditRule.Editability = Editability.Closed;
            //result.MoreInformation.EditRule.Editability = Editability.Closed;
            //result.UrlTitle.EditRule.Editability = Editability.Closed;
            //result.NationalBenchmark.EditRule.Editability = Editability.Closed;
            //result.UpperBound.EditRule.Editability = Editability.Closed;
            //result.LowerBound.EditRule.Editability = Editability.Closed;
            //result.SuppressionNumerator.EditRule.Editability = Editability.Closed;
            //result.SuppressionDenominator.EditRule.Editability = Editability.Closed;
            //result.ScaleBy.EditRule.Editability = Editability.Closed;
            //result.Url.EditRule.Editability = Editability.Closed;
            //result.StatePeerBenchmark.EditRule.Editability = Editability.Closed;
            #endregion
        }
    }
}
