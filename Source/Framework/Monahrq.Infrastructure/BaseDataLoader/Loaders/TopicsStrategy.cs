using System;
using System.ComponentModel.Composition;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Extensions;
using NHibernate;
using NHibernate.Linq;
using System.Collections.Generic;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Topic"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataCustomImporter{Monahrq.Infrastructure.Entities.Domain.Measures.Topic, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class TopicsStrategy : BaseDataCustomImporter<Topic, int>
    {
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 1; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(Topic));
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        public override void LoadData()
        {
            try
            {
                // Get list of files matching mask
                // TODO: Throw an error if the path doesn't exist?
                if (Directory.Exists(baseDataDir))
                {
                    var files = Directory.GetFiles(baseDataDir, "MeasureTopics*.csv");
                    foreach (var file in files)
                    {
                        VersionStrategy.Filename = file;
                        // Appending measures to the topics tables, not replacing with the newest file.
                        if (!VersionStrategy.IsLoaded())
                        {
                            // Verify data file exists.
                            if (!File.Exists(Path.Combine(baseDataDir, file)))
                            {
                                Logger.Log(string.Format("Import file \"{0}\" missing from the base data resources directory.", file), Category.Warn, Priority.Medium);
                                return;
                            }

                            using (var session = DataProvider.SessionFactory.OpenSession())
                            {
                                using (var trans = session.BeginTransaction())
                                {
                                    var builder = new OleDbConnectionStringBuilder()
                                    {
                                        Provider = "Microsoft.ACE.OLEDB.12.0",
                                        DataSource = baseDataDir,
                                    };

                                    builder["Extended Properties"] = "text;HDR=YES;FMT=Delimited";

                                    using (var conn = new OleDbConnection(builder.ConnectionString))
                                    {
                                        conn.Open();
                                        var sql = string.Format("SELECT * FROM [{0}]", Path.GetFileName(file));

                                        using (var cmd = new OleDbCommand(sql, conn))
                                        {
                                            // NOTE: Not using the bulk importer here because we have two types of data being imported at the same time.
                                            var reader = cmd.ExecuteReader();
                                            while (reader != null && reader.Read())
                                            {
                                                var parentName = reader.Guard<string>("ParentName") ?? "";
                                                var name = reader.Guard<string>("Name") ?? "";
                                                var longTitle = reader.Guard<string>("Longtitle") ?? "";
                                                var description = reader.Guard<string>("description") ?? "";
                                                var consumerLongTitle = reader.Guard<string>("ConsumerLongTitle") ?? "";
                                                var consumerDescription = reader.Guard<string>("ConsumerDescription") ?? "";
                                                var categoryType = reader.Guard<string>("CategoryType");

												var topicFacts1Text = reader.Guard<string>("TopicFact1Text") ?? "";
												var topicFacts1Citation = reader.Guard<string>("TopicFact1Citation") ?? "";
												var topicFacts1Image = reader.Guard<string>("TopicFact1Image") ?? "";
												var topicFacts2Text = reader.Guard<string>("TopicFact2Text") ?? "";
												var topicFacts2Citation = reader.Guard<string>("TopicFact2Citation") ?? "";
												var topicFacts2Image = reader.Guard<string>("TopicFact2Image") ?? "";
												var topicFacts3Text = reader.Guard<string>("TopicFact3Text") ?? "";
												var topicFacts3Citation = reader.Guard<string>("TopicFact3Citation") ?? "";
												var topicFacts3Image = reader.Guard<string>("TopicFact3Image") ?? "";

												var tipsChecklist = reader.Guard<string>("TipsChecklist") ?? "";
												var topicIcon = reader.Guard<string>("TopicIcon") ?? "";


                                                if (!parentName.Equals("SKIP_ROW"))
                                                {
                                                    AddOrUpdateToDB(
														session, parentName, name, longTitle, description, consumerLongTitle, consumerDescription, categoryType,
														topicFacts1Text,	topicFacts1Citation,	topicFacts1Image,
														topicFacts2Text,	topicFacts2Citation,	topicFacts2Image,
														topicFacts3Text,	topicFacts3Citation,	topicFacts3Image,
														tipsChecklist,topicIcon);
                                                }
                                            }
                                        }
                                    }
                                    trans.Commit();
                                }
                            }

                            using (var session = DataProvider.SessionFactory.OpenSession())
                            {
                                var version = VersionStrategy.GetVersion(session);

                                session.SaveOrUpdate(version);
                                session.Flush();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), Category.Warn, Priority.Medium);
            }
        }


        /// <summary>
        /// Adds the or update to database.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="parentName">Name of the parent.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="longTitle">The long title.</param>
        /// <param name="description">The description.</param>
        /// <param name="consumerLongTitle">The consumer long title.</param>
        /// <param name="consumerDescription">The consumer description.</param>
        /// <param name="categoryType">Type of the category.</param>
        /// <param name="topicFacts1Text">The topic facts1 text.</param>
        /// <param name="topicFacts1Citation">The topic facts1 citation.</param>
        /// <param name="topicFacts1Image">The topic facts1 image.</param>
        /// <param name="topicFacts2Text">The topic facts2 text.</param>
        /// <param name="topicFacts2Citation">The topic facts2 citation.</param>
        /// <param name="topicFacts2Image">The topic facts2 image.</param>
        /// <param name="topicFacts3Text">The topic facts3 text.</param>
        /// <param name="topicFacts3Citation">The topic facts3 citation.</param>
        /// <param name="topicFacts3Image">The topic facts3 image.</param>
        /// <param name="tipsChecklist">The tips checklist.</param>
        /// <param name="topicIcon">The topic icon.</param>
        private void AddOrUpdateToDB(ISession session, string parentName, string topicName, string longTitle, string description, string consumerLongTitle, string consumerDescription, string categoryType,
			string topicFacts1Text, string topicFacts1Citation, string topicFacts1Image,
			string topicFacts2Text,	string topicFacts2Citation,	string topicFacts2Image,
			string topicFacts3Text,	string topicFacts3Citation,	string topicFacts3Image,
			string tipsChecklist, string topicIcon)
        {
            try
            {
                //  Get the TopicCategory object if exist; otherwise create it.
                var cat = session.Query<TopicCategory>().FirstOrDefault(tc => tc.Name == parentName) ?? new TopicCategory(parentName);

                //  If topicName is empty we are only creating/updating a TopicCategory.
                if (string.IsNullOrWhiteSpace(topicName))
                {
                    cat.Description = description;
                    cat.LongTitle = longTitle;
                    cat.ConsumerDescription = consumerDescription;
					cat.Facts = new List<TopicFacts>();

					if (StringExtensions.AnyPopulated(topicFacts1Text, topicFacts1Citation, topicFacts1Image))
						cat.Facts.Add(new TopicFacts() { Name = "Fact1", Text = topicFacts1Text, CitationText = topicFacts1Citation, ImagePath = topicFacts1Image });
					if (StringExtensions.AnyPopulated(topicFacts2Text, topicFacts2Citation, topicFacts2Image))
						cat.Facts.Add(new TopicFacts() { Name = "Fact2", Text = topicFacts2Text, CitationText = topicFacts2Citation, ImagePath = topicFacts2Image });
					if (StringExtensions.AnyPopulated(topicFacts3Text, topicFacts3Citation, topicFacts3Image))
						cat.Facts.Add(new TopicFacts() { Name = "Fact3", Text = topicFacts3Text, CitationText = topicFacts3Citation, ImagePath = topicFacts3Image });

					cat.TipsChecklist = tipsChecklist;
					cat.TopicIcon = topicIcon;

					if (!string.IsNullOrEmpty(categoryType))
                        cat.CategoryType = EnumExtensions.GetEnumValueFromString<TopicCategoryTypeEnum>(categoryType);
                }
                //  Create/Update the topic
                else
                {
                    var topic = session.Query<Topic>().FirstOrDefault(t => t.Name.ToLower().Equals(topicName.ToLower()) && 
                                                                      t.Owner.Name.ToLower().Equals(parentName.ToLower())) ?? new Topic(cat, topicName);

                    topic.Description = description;
                    topic.LongTitle = longTitle;
                    topic.ConsumerLongTitle = consumerLongTitle;
                    
                    session.SaveOrUpdate(topic);
                }
                session.SaveOrUpdate(cat);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), Category.Warn, Priority.Medium);
            }
        }
    }
}
