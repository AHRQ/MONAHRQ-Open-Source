using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using NHibernate.Linq;
using System.Xml.Linq;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using NHibernate;

namespace Monahrq.Wing.HospitalSpendingByClaimType
{
    /// <summary>
    /// Describes measures related to <see cref="HospitalSpendingByClaimTypeTarget"/> and provides column name hints to use
    /// when importing records.
    /// </summary>
    [WingModule(typeof(HospitalSpendingModule),
        "{7D39A125-315D-47B0-BE69-E7D6DFB64BB3}",
        "Hospital Spending",
        "Hospital Spending Sample Module")]
    public class HospitalSpendingModule : TargetedModuleWithMeasuresAndTopics<HospitalSpendingByClaimTypeTarget>
    {
        /// <inheritdoc/>
        public override string MeasureFilePath { get { return null; } }

        /// <inheritdoc/>
        public override string MeasureTopicFilePath { get { return null; } }

        /// <inheritdoc/>
        protected override void ImportMeasures()
        {
            // note: use of an embedded XML file is not required
            XDocument measures;
            using (var str = GetType().Assembly.GetManifestResourceStream(GetType(), "Measures.xml"))
            {
                if (str == null)
                    throw new NotSupportedException("The embedded resource 'Measures.xml' is missing");
                measures = XDocument.Load(str);
            }

            var moduleMetadata = base.TargetAttribute;
            using (var session = base.SessionFactoryProvider.SessionFactory.OpenSession())
            {
                var moduleTarget = session.Query<Target>().FirstOrDefault(t => t.Name == moduleMetadata.Name);

                var topics = measures.Element("topics");
                if (topics == null)
                    return;
                foreach (var xCategory in topics.Elements("category"))
                {
                    var category = this.GetCategory(session, xCategory);
                    foreach (var xTopic in xCategory.Elements("topic"))
                    {
                        var topic = this.GetTopic(session, category, xTopic);
                        foreach (var xMeasure in xTopic.Elements("measure"))
                        {
                            this.AddMeasure(session, moduleTarget, category, topic, xMeasure);
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Obtains a <see cref="TopicCategory"/> instance that reflects the information in the given <paramref name="element"/>. If no instance exists in the database,
        /// a new instance is created and returned. The new instance is not saved to the database by this method.
        /// </summary>
        /// <param name="session">NHibernate <see cref="ISession"/></param>
        /// <param name="element">The <see cref="XElement"/> that contains data about this <see cref="TopicCategory"/></param>
        /// <returns>A <see cref="TopicCategory"/> instance</returns>
        private TopicCategory GetCategory(ISession session, XElement element)
        {
            var name = element.Attribute("name").Value;
            var result = session.Query<TopicCategory>().FirstOrDefault(c => c.Name == name);
            if (result != null)
                return result;

            result = new TopicCategory(name)
            {
                LongTitle = element.Attribute("longTitle").Value,
                Description = element.Attribute("description").Value
            };
            return result;
        }

        /// <summary>
        /// Obtains a <see cref="Topic"/> instance that reflects the information in the given <paramref name="element"/>. If no instance exists in the database,
        /// a new instance is created and returned. The new instance is not saved to the database by this method.
        /// </summary>
        /// <param name="session">NHibernate <see cref="ISession"/></param>
        /// <param name="category">The <see cref="TopicCategory"/> that the <see cref="Topic"/> should be associated with</param>
        /// <param name="element">The <see cref="XElement"/> that contains data about this <see cref="Topic"/></param>
        /// <returns>A <see cref="Topic"/> instance</returns>
        private Topic GetTopic(ISession session, TopicCategory category, XElement element)
        {
            var name = element.Attribute("name").Value;
            var result = session.Query<Topic>().FirstOrDefault(t => t.Name == name);
            if (result != null)
                return result;

            result = new Topic(category, name)
            {
                LongTitle = element.Attribute("longTitle").Value,
                Description = element.Attribute("description").Value
            };
            return result;
        }

        private const string MeasurePrefix = "HS-";
        private int measurePrefixCount = 1;
        /// <summary>
        /// Creates a new <see cref="Measure"/> that reflects the information in the given <paramref name="element"/>, associates it
        /// with the given <paramref name="target"/>, <paramref name="category"/> and <paramref name="topic"/>, and commits
        /// all changes to the database.
        /// </summary>
        /// <param name="session">NHibernate <see cref="ISession"/></param>
        /// <param name="target">The <see cref="Target"/> that the <see cref="Measure"/> should be associated with</param>
        /// <param name="category">The <see cref="TopicCategory"/> that the <see cref="Measure"/> should be associated with</param>
        /// <param name="topic">The <see cref="Topic"/> that the <see cref="Measure"/> should be associated with</param>
        /// <param name="element">The <see cref="XElement"/> that contains data about this <see cref="Topic"/></param>
        private void AddMeasure(ISession session, Target target, TopicCategory category, Topic topic, XElement element)
        {
            var measureId = string.Format("{0}{1:00}", MeasurePrefix, measurePrefixCount++);
            var measureName = element.Attribute("name").Value;
            var measure = Measure.CreateMeasure(typeof(HospitalMeasure), target, measureId);
            if (measure.MeasureTitle == null) measure.MeasureTitle = new MeasureTitle();

            measure.MeasureTitle.Plain = measureName;
            measure.MeasureTitle.Clinical = measureName;
            measure.MeasureTitle.Policy = measureName;

            measure.NationalBenchmark = null;
            measure.UpperBound = null;
            measure.LowerBound = null;

            if (measure.StatePeerBenchmark != null)
            {
                measure.StatePeerBenchmark.ProvidedBenchmark = null;
            }
            measure.SuppressionDenominator = null;
            measure.SuppressionNumerator = null;

            measure.Description = measureName;
            
            using (var tx = session.BeginTransaction())
            {
                measure.AddTopic(topic);
                topic.Measures.Add(measure);
                session.SaveOrUpdate(measure);
                session.SaveOrUpdate(category);
                session.SaveOrUpdate(topic);
                tx.Commit();
            }
        }

        /// <inheritdoc/>
        protected override void OnApplyDatasetHints()
        {
            Target<HospitalSpendingByClaimTypeTarget>(t => t.CmsProviderId).ApplyMappingHints("Provider_ID");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.ClaimType).ApplyMappingHints("Claim_Type");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.Period).ApplyMappingHints("Period");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.AverageSpendingPerEpisodeHospital).ApplyMappingHints("Avg_Spending_Per_Episode_Hospital");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.AverageSpendingPerEpisodeState).ApplyMappingHints("Avg_Spending_Per_Episode_State");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.AverageSpendingPerEpisodeNational).ApplyMappingHints("Avg_Spending_Per_Episode_Nation");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.PercentSpendingPerEpisodeHospital).ApplyMappingHints("Percent_of_Spending_Hospital");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.PercentSpendingPerEpisodeState).ApplyMappingHints("Percent_of_Spending_State");
            Target<HospitalSpendingByClaimTypeTarget>(t => t.PercentSpendingPerEpisodeNational).ApplyMappingHints("Percent_of_Spending_Nation");
        }
    }
}
