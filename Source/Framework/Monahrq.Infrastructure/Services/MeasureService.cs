using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using NHibernate;
using NHibernate.Linq;
using System;
using System.ComponentModel.Composition;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Data;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
    /// <summary>
    /// The Monahrq measure domain service.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.DataServiceBase" />
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.Measures.IMeasureService" />
    [Export(typeof(IMeasureService))]
	public class MeasureService : DataServiceBase, IMeasureService
	{
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        ILogWriter Logger { get; set; }
        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        IDomainSessionFactoryProvider Provider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureService"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
		public MeasureService(IDomainSessionFactoryProvider provider, [Import(LogNames.Session)] ILogWriter logger)
		{
			Logger = logger ?? NullLogger.Instance;
			Provider = provider;
		}

        /// <summary>
        /// Adds the topic.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="longTitle">The long title.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Topic AddTopic(TopicCategory category, string name, string longTitle, string description)
		{
			try
			{
				Topic topic = new Topic(category, name.Trim());
				topic.LongTitle = longTitle.Trim();
				topic.Description = description.Trim();
				using (var session = Provider.SessionFactory.OpenSession())
				{
					using (var trans = session.BeginTransaction())
					{
						if (!topic.IsPersisted)
							session.SaveOrUpdate(topic);
						else
							session.Merge(topic);

						trans.Commit();
					}
				}
				return topic;
			}
			catch (Exception ex)
			{
				Logger.Write(ex, "Error adding topic {0} to category {1}", name, category);
				return null;
			}
		}

        /// <summary>
        /// Gets the topic.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Topic GetTopic(string category, string name)
		{
			using (ISession session = Provider.SessionFactory.OpenSession())
			{
				try
				{
					//TopicCategory topicCategory = GetTopicCategory(category);
					return session.Query<Topic>()
								  .SingleOrDefault(topics => topics.Name == name && topics.Owner.Name == category);
				}
				catch (Exception ex)
				{
				    Logger.Write(ex, "Error getting topic {0} from category {1}", name, category);
					return null;
				}
			}
		}

        /// <summary>
        /// Adds the topic category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="longTitle">The long title.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public TopicCategory AddTopicCategory(string category, string longTitle, string description)
		{
			try
			{
				var topicCategory = new TopicCategory(category.Trim())
				{
					LongTitle = longTitle.Trim(),
					Description = description.Trim()
				};

				using (var session = Provider.SessionFactory.OpenSession())
				{
					using (var trans = session.BeginTransaction())
					{
						if (!topicCategory.IsPersisted)
							session.SaveOrUpdate(topicCategory);
						else
							session.Merge(topicCategory);

						trans.Commit();
					}
				}

				return topicCategory;
			}
			catch (Exception ex)
			{
				Logger.Write(ex, "Error adding topic category {0}", category);
				return null;
			}
		}

        /// <summary>
        /// Gets the topic category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public TopicCategory GetTopicCategory(string category)
		{
			using (ISession session = Provider.SessionFactory.OpenSession())
			{
				try
				{
					TopicCategory topicCategory = (from categories in session.Query<TopicCategory>()
												   where categories.Name == category.Trim()
												   select categories).SingleOrDefault();
					return topicCategory;
				}
				catch (Exception ex)
				{
					Logger.Write(ex, "Error getting topic category with name {0}", category);
					return null;
				}
			}
		}

        /// <summary>
        /// Gets the measure.
        /// </summary>
        /// <param name="measureCode">The measure code.</param>
        /// <returns></returns>
        public Measure GetMeasure(string measureCode)
		{
			using (ISession session = Provider.SessionFactory.OpenSession())
			{
				try
				{
					return session.Query<Measure>().SingleOrDefault(tst => tst.Name == measureCode);
				}
				catch (Exception ex)
				{
					Logger.Write(ex, "Error getting measure with code {0}", measureCode);
					return null;
				}
			}
		}

        /// <summary>
        /// Imports the measures.
        /// </summary>
        /// <param name="wingName">Name of the wing.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="measureType">Type of the measure.</param>
        /// <returns></returns>
        public bool ImportMeasures(string wingName, string fileName, Type measureType)
		{
			using (var session = Provider.SessionFactory.OpenSession())
			{
				var selectStatement = string.Format("SELECT * FROM [{0}]", Path.GetFileName(fileName));

				var builder = new OleDbConnectionStringBuilder
				{
					Provider = "Microsoft.ACE.OLEDB.12.0",
					DataSource = Path.GetDirectoryName(fileName)
				};
				builder["Extended Properties"] = "text;HDR=YES;FMT=Delimited";
				using (var conn = new OleDbConnection(builder.ConnectionString))
				{
					conn.Open();
					var query = session.Query<Target>().Where(t => t.Name == wingName).ToList();
					using (var tx = session.BeginTransaction())
					{
						try
						{
							foreach (var target in query)
							{
								using (var cmd = new OleDbCommand(selectStatement, conn))
								{
									using (var rdr = cmd.ExecuteReader())
									{
										while (rdr.Read())
										{
											ImportMeasuresFactory(rdr, target, measureType);
										}
									}
								}

								if (!target.IsPersisted)
									session.SaveOrUpdate(target);
								else
									session.Merge(target);
								//session.Flush();
							}
							tx.Commit();
							return true;
						}
						catch (Exception ex)
						{
							tx.Rollback();
						    Logger.Write(ex, "Error importing measures for wing \"{0}\" from file {1}", wingName, fileName);
							return false;
						}
					}
				}
			}
		}

        /// <summary>
        /// Imports the measures target specific read.
        /// </summary>
        /// <param name="rdr">The RDR.</param>
        /// <param name="measure">The measure.</param>
        protected virtual void ImportMeasuresTargetSpecificRead(OleDbDataReader rdr, Measure measure)
		{ }

        /// <summary>
        /// Imports the measures factory.
        /// </summary>
        /// <param name="rdr">The RDR.</param>
        /// <param name="target">The target.</param>
        /// <param name="measureType">Type of the measure.</param>
        private void ImportMeasuresFactory(OleDbDataReader rdr, Target target, Type measureType)
		{
			var measureCode = rdr["MeasureCode"].ToString();

			var measure = target.Measures.FirstOrDefault(m => m.Name == measureCode);
			if (measure == null)
				measure = Measure.CreateMeasure(measureType, target, measureCode);
			measure.MeasureType = rdr["MeasureType"].ToString();

			// measures.UsedInCalculations
			string usedInCalculations = rdr["IsIncludedInDomainScoring"].ToString();
			if (usedInCalculations == "1")
			{
				measure.UsedInCalculations = true;
			}
			else if (usedInCalculations == "0")
			{
				measure.UsedInCalculations = false;
			}

			measure.MeasureTitle = new MeasureTitle
			{
				Clinical = rdr["ClinicalTitle"].ToString(),
				Plain = rdr["WebName"].ToString()
			};
			//measure.Title.Policy.Value = rdr[""].ToString();
			measure.Source = rdr["Source"].ToString();
			measure.Description = rdr["Description"].ToString();
			// TODO: BetterHighLow
			string betterHighLow = rdr["BetterHighLow"].ToString();
			if (betterHighLow == "H")
			{
				measure.HigherScoresAreBetter = true;
			}
			else if (betterHighLow == "L")
			{
				measure.HigherScoresAreBetter = false;
			}

			measure.Footnotes = rdr["Footnote"].ToString();

			decimal tempDecimal;
			if (rdr["ScaleBy"] == null || !decimal.TryParse(rdr["ScaleBy"].ToString(), out tempDecimal))
			{
				// TODO: Is this correct?
				measure.ScaleBy = -1;
			}
			else
			{
				measure.ScaleBy = tempDecimal;
			}

			measure.ScaleTarget = rdr["ScaleTarget"].ToString();
			measure.RiskAdjustedMethod = rdr["RaMethod"].ToString();
			measure.RateLabel = rdr["RateLabel"].ToString();
			if (rdr["nqf"].ToString() == "Y")
			{
				measure.NQFEndorsed = true;
			}
			else
			{
				measure.NQFEndorsed = false;
			}
			string nqfID = rdr["NqfId"].ToString();
			if (nqfID == "N/A")
			{
				measure.NQFID = "";
			}
			else
			{
				measure.NQFID = nqfID;
			}

			if (rdr["NatBenchmark"] == null || !decimal.TryParse(rdr["NatBenchmark"].ToString(), out tempDecimal))
			{
				measure.NationalBenchmark = null;
			}
			else
			{
				if (tempDecimal != 0)
					measure.NationalBenchmark = tempDecimal;
				else
					measure.NationalBenchmark = null;
			}


			measure.StatePeerBenchmark = new StatePeerBenchmark();
			if (rdr["PeerBenchmark"] == null || !decimal.TryParse(rdr["PeerBenchmark"].ToString(), out tempDecimal))
				measure.StatePeerBenchmark.ProvidedBenchmark = (decimal?)null;
			else
			{
				if (tempDecimal != 0)
					measure.StatePeerBenchmark.ProvidedBenchmark = tempDecimal;
				else
					measure.StatePeerBenchmark.ProvidedBenchmark = null;
			}

			// TODO: why aren't these assigned???
			//measure.UpperBound.CurrentValue = tempDouble;
			//measure.LowerBound.CurrentValue = tempDouble;

			// NOTE: We're not importing Numerator/Denominator into SupressionXXX, they should derfault to 0 to be overwritten by user in app.
			//measure.SuppressionNumerator = rdr["Numerator"] == null || !decimal.TryParse(rdr["Numerator"].ToString(), out tempDecimal)
			//                                   ? (decimal?) null
			//                                   : tempDecimal;

			//measure.SuppressionDenominator = rdr["Denominator"] == null || !decimal.TryParse(rdr["Denominator"].ToString(), out tempDecimal)
			//                                     ? (decimal?) null
			//                                     : tempDecimal;
			measure.SuppressionNumerator = 0;
			measure.SuppressionDenominator = 0;


			measure.SupportsCost = !string.IsNullOrEmpty(rdr["SupportsCost"].ToString()) && Convert.ToBoolean(Convert.ToInt32(rdr["SupportsCost"].ToString()));

			measure.ConsumerDescription = rdr["ConsumerDescription"].ToString();
			measure.ConsumerPlainTitle = rdr["ConsumerPlainTitle"].ToString();

			ImportMeasuresTargetSpecificRead(rdr, measure);

			// Add any target specific properties to the measure.
		}

        /// <summary>
        /// Imports the measure topic file.
        /// </summary>
        /// <param name="wingName">Name of the wing.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="topicType">Type of the topic.</param>
        public void ImportMeasureTopicFile(string wingName, string fileName, TopicTypeEnum topicType)
		{
			using (var session = Provider.SessionFactory.OpenSession())
			{
				string selectStatement = string.Format("SELECT * FROM [{0}]", Path.GetFileName(fileName));

				var builder = new OleDbConnectionStringBuilder()
				{
					Provider = "Microsoft.ACE.OLEDB.12.0",
					DataSource = Path.GetDirectoryName(fileName),
				};

				builder["Extended Properties"] = "text;HDR=YES;FMT=Delimited";

				using (var conn = new OleDbConnection(builder.ConnectionString))
				{
					conn.Open();
					using (var tx = session.BeginTransaction())
					{
						try
						{
							using (var cmd = new OleDbCommand(selectStatement, conn))
							{
								using (var rdr = cmd.ExecuteReader())
								{
									while (rdr != null && rdr.Read())
									{
										var measureCode = rdr["MeasureCode"].ToString().Trim();
										var topicCategoryName = rdr["TopicCategory"].ToString().Trim();
										var topicName = rdr["TopicName"].ToString().Trim();
										var usedForInfographics = rdr["UsedForInfographic"].ToString().Trim().ToLower() == "true";
										var audiencesText = rdr.ColumnExists("Audiences") ? rdr.Guard<string>("Audiences") : null;

										if (string.IsNullOrEmpty(measureCode) ||
											string.IsNullOrEmpty(topicCategoryName) ||
											string.IsNullOrEmpty(topicName))
											continue;

										var measure =
											session.Query<Measure>().FirstOrDefault(tst => tst.Name == measureCode);

										if (measure == null) continue;

										//	Get/Update topicCategory.
										var topicCategory = session.Query<TopicCategory>().FirstOrDefault(tc => tc.Name.ToLower() == topicCategoryName.ToLower()) ?? new TopicCategory(topicCategoryName);
										topicCategory.TopicType = topicType;

										//	Get topic; create if needed.
										var newTopic = topicCategory.Topics.SingleOrDefault(t => t.Name.EqualsIgnoreCase(topicName) && t.Owner.Name.EqualsIgnoreCase(topicCategoryName));
										if (newTopic == null) newTopic = new Topic(topicCategory, topicName);

										//	Create/Update measureTopic.
										var measureTopic = measure.MeasureTopics.FirstOrDefault(mt => mt.Topic.Name.EqualsIgnoreCase(topicName) && mt.Topic.Owner.Name.EqualsIgnoreCase(topicCategoryName));
										if (measureTopic == null) measure.AddTopic(newTopic, usedForInfographics);
										else measureTopic.UsedForInfographic = usedForInfographics;

										session.SaveOrUpdate(measure);
										session.SaveOrUpdate(newTopic);
										session.SaveOrUpdate(topicCategory);
									}
									tx.Commit();
								}
							}
						}
						catch (Exception ex)
						{
							//tx.Rollback();
							Logger.Write(ex, "Error importing measure topics for Wing \"{0}\" from file {1}", wingName, fileName);
						}
					}
				}
			}
		}
	}
}
