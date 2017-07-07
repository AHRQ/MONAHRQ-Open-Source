using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Services.Generators;
using Monahrq.Wing.Ahrq.Area;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    /// Class for Quality report
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    [Export("QualityReportGenerator", typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new []
                        {
                            "1BD85413-734B-4C7A-9AAF-C442D8C2FACE", "7AAC8244-0F39-424A-85BE-943B465ED61A", "7AAC8244-0F39-424A-85BE-943B465ED61B",
                            "12546E5A-CA82-4D7E-AAB5-C1DB88A4FD33", "2AAF7FBA-7102-4C66-8598-A70597E2F82B", "2AAF7FBA-7102-4C66-8598-A70597E2F821",
                            "2AAF7FBA-7102-4C66-8598-A70597E2F823"
                        },
                        new string[] { },
                        new[] { typeof(AreaTarget) },
                        2)]
    public class QualityReportGenerator : BaseReportGenerator
    {

        #region Fields and Constants

        // NOTE: Domain will be expanded in export.
        /// <summary>
        /// The json domain
        /// </summary>
        private const string jsonDomain = "$.monahrq.qualitydata";
        /// <summary>
        /// The publish task
        /// </summary>
        private PublishTask _publishTask;
        /// <summary>
        /// The report identifier
        /// </summary>
        private string _reportID;
        /// <summary>
        /// The measures
        /// </summary>
        private DataTable _measures;
        /// <summary>
        /// The hospital compare dataset i ds
        /// </summary>
        private DataTable _hospitalCompareDatasetIDs;
        /// <summary>
        /// The q i dataset i ds
        /// </summary>
        private DataTable _qIDatasetIDs;
        /// <summary>
        /// The base data dir
        /// </summary>
        private string _baseDataDir;
        /// <summary>
        /// The base measures dir
        /// </summary>
        private string _baseMeasuresDir;
        /// <summary>
        /// The quality data dir
        /// </summary>
        private string _qualityDataDir;
        /// <summary>
        /// The quality hospital data dir
        /// </summary>
        private string _qualityHospitalDataDir;
        /// <summary>
        /// The quality measure data dir
        /// </summary>
        private string _qualityMeasureDataDir;
        /// <summary>
        /// The facts image path
        /// </summary>
        private string _factsImagePath;

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the dataservice provider.
        /// </summary>
        /// <value>
        /// The dataservice provider.
        /// </value>
        [Import]
        protected IDomainSessionFactoryProvider DataserviceProvider { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Once installed runs all the quality related sql scripts in the database.
        /// </summary>
        public void OnInstalled()
        {
            string[] scriptFiles ={
                                        //"Table-Temp_Quality.sql",
                                        "Table-Temp_Quality_Measures.sql",

                                        "UDF-fnQualityBarChartPercentage.sql",
                                        "UDF-fnQualityBullets.sql",
                                        "UDF-fnQualityScaleByFootnote.sql",
                                        "UDF-fnQualitySelectedMeasureTitle.sql",

                                        "Sproc-spQualityGetHospitalCompareMeasures.sql",
                                        "Sproc-spQualityGetQIMeasures.sql",

                                        "Sproc-spQualityInitializeHCBinary.sql",
                                        "Sproc-spQualityInitializeHCCategorical.sql",
                                        "Sproc-spQualityInitializeHCOutcome.sql",
                                        "Sproc-spQualityInitializeHCProcess.sql",
                                        "Sproc-spQualityInitializeHCRatio.sql",
                                        "Sproc-spQualityInitializeHCScale.sql",
                                        "Sproc-spQualityInitializeHCStructural.sql",
                                        "Sproc-spQualityInitializeHCYNM.sql",

                                        "Sproc-spQualityInitializeQIComposite.sql",
                                        "Sproc-spQualityInitializeQIProvider.sql",
                                        "Sproc-spQualityInitializeQIProviderMCMC.sql",
                                        "Sproc-spQualityInitializeQIVolume.sql",

                                        "Sproc-spQualityGetMeasure.sql",
                                        "Sproc-spQualityGetMeasureTopics.sql",
                                        "Sproc-spQualityGetMeasureTopicCategories.sql",
                                        "Sproc-spQualityGetMeasureTopicCategoriesConsumer.sql",

                                        "Sproc-spQualityGetDataByHospital.sql",
                                        "Sproc-spQualityGetDataByMeasure.sql"
                                  };

            RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\HospitalCompare"), scriptFiles);
        }

        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator()
        {
            // Following should only run once, but this procedure is running every time on application startup.
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Quality Ratings reports" });

            OnInstalled();
        }

        /// <summary>
        /// Main function for all report generators that handles the worfkow for each report generator that derives from the BaseReportGenerator class.
        /// </summary>
        /// <param name="website">The website object used to generate reporting website.</param>
        /// <param name="publishTask">The publish task. This parameter is optional with the default value being PublishTask.Full</param>
        public override void GenerateReport(Website website, PublishTask publishTask = PublishTask.Full)
        {

            _publishTask = publishTask;

            // Call the base generator report first
            base.GenerateReport(website);
        }

        /// <summary>
        /// Initializes the report data.
        /// </summary>
        private void InitializeReportData()
        {
            try
            {
                // Make sure the base directories are created.
                CreateBaseDirectories();

#if DEBUG
				LogMessage("Initializing report data");
#endif
                #region Get base information about the website - hospitals, measures, datasets, etc.

                _reportID = Guid.NewGuid().ToString();

                _measures = new DataTable();
                _measures.Columns.Add("ID", typeof(int));
                foreach (WebsiteMeasure measure in CurrentWebsite.Measures.Where(m => m.IsSelected).ToList())
                {
                    //if (measure.IsSelected)
                    //{
                    _measures.Rows.Add(measure.ReportMeasure.Id);
                    //}
                }

                //// Get the needed DataSets
                _hospitalCompareDatasetIDs = new DataTable();
                _hospitalCompareDatasetIDs.Columns.Add("ID", typeof(int));
                _qIDatasetIDs = new DataTable();
                _qIDatasetIDs.Columns.Add("ID", typeof(int));
                foreach (WebsiteDataset dataSet in CurrentWebsite.Datasets)
                {
                    switch (dataSet.Dataset.ContentType.Name)
                    {
                        case "Hospital Compare Data":
                            // Add a new Hospital Compare dataset
                            _hospitalCompareDatasetIDs.Rows.Add(dataSet.Dataset.Id);
                            break;
                        case "AHRQ-QI Area Data":
                        case "AHRQ-QI Composite Data":
                        case "AHRQ-QI Provider Data":
                            // Add a new AHRQ QI dataset
                            _qIDatasetIDs.Rows.Add(dataSet.Dataset.Id);
                            break;
                    }
                }

                var hcParams = new[]
                {
                        new KeyValuePair<string, object>("@ReportID", _reportID),
                        new KeyValuePair<string, object>("@HospitalCompareDataset", _hospitalCompareDatasetIDs),
                        new KeyValuePair<string, object>("@Hospitals", HospitalIds),
                        new KeyValuePair<string, object>("@Measures", _measures),
                        new KeyValuePair<string, object>("@RegionType", CurrentWebsite.RegionTypeContext)
                };

                //Clean Temp_Quality Table 
                ExecuteNonQuery("IF (OBJECT_ID(N'Temp_Quality')) IS NOT NULL TRUNCATE Table Temp_Quality");

                RunSproc("spQualityInitializeHCBinary", "", hcParams);
                RunSproc("spQualityInitializeHCCategorical", "", hcParams);
                RunSproc("spQualityInitializeHCOutcome", "", hcParams);
                RunSproc("spQualityInitializeHCProcess", "", hcParams);
                RunSproc("spQualityInitializeHCRatio", "", hcParams);
                RunSproc("spQualityInitializeHCScale", "", hcParams);
                RunSproc("spQualityInitializeHCStructural", "", hcParams);
                RunSproc("spQualityInitializeHCYNM", "", hcParams);

                var qiParams = new[]
                {
                    new KeyValuePair<string, object>("@ReportID", _reportID),
                    new KeyValuePair<string, object>("@QIDataset", _qIDatasetIDs),
                    new KeyValuePair<string, object>("@Hospitals", HospitalIds),
                    new KeyValuePair<string, object>("@Measures", _measures),
                    new KeyValuePair<string, object>("@RegionType", CurrentWebsite.RegionTypeContext)
                };

                RunSproc("spQualityInitializeQIComposite", "", qiParams);
                RunSproc("spQualityInitializeQIProvider", "", qiParams);
                RunSproc("spQualityInitializeQIProviderMCMC", "", qiParams);
                RunSproc("spQualityInitializeQIVolume", "", qiParams);

                QualityReportPostProcessLogic.PostProcessQualityReport(_reportID, CurrentWebsite, DataserviceProvider);

                #endregion

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        /// <summary>
        /// Creates the base directories.
        /// </summary>
        private void CreateBaseDirectories()
        {
            try
            {
                _baseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
                if (!Directory.Exists(_baseDataDir)) Directory.CreateDirectory(_baseDataDir);

                _baseMeasuresDir = Path.Combine(_baseDataDir, "QualityMeasures");
                if (!Directory.Exists(_baseMeasuresDir)) Directory.CreateDirectory(_baseMeasuresDir);

                _qualityDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "QualityRatings");
                if (!Directory.Exists(_qualityDataDir)) Directory.CreateDirectory(_qualityDataDir);

                _qualityHospitalDataDir = Path.Combine(_qualityDataDir, "Hospital");
                if (!Directory.Exists(_qualityHospitalDataDir)) Directory.CreateDirectory(_qualityHospitalDataDir);

                _qualityMeasureDataDir = Path.Combine(_qualityDataDir, "Measure");
                if (!Directory.Exists(_qualityMeasureDataDir)) Directory.CreateDirectory(_qualityMeasureDataDir);


                _factsImagePath = Path.Combine(CurrentWebsite.OutPutDirectory, "themes", "consumer", "assets", "infographic", "facts-tips", "custom");
                if (!Directory.Exists(_factsImagePath)) Directory.CreateDirectory(_factsImagePath);

            }

            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        /// <summary>
        /// Generates the json files.
        /// </summary>
        /// <param name="publishTask">The publish task.</param>
        public void GenerateJsonFiles(PublishTask publishTask)
        {
            try
            {
                // Generate any report specific json files

                // Get the measure topics, topic categories and detailed information about the measure (national and peer data, popup help, measure details, etc.)
                if (!File.Exists(Path.Combine(_baseMeasuresDir, "Topics.js")))
                    GenerateJsonFile(RunSprocReturnDataTable("spQualityGetMeasureTopics", new KeyValuePair<string, object>("@ReportID", _reportID)),
                        Path.Combine(_baseMeasuresDir, "Topics.js"), jsonDomain + ".measuretopics=");

                if (!File.Exists(Path.Combine(_baseMeasuresDir, "TopicCategories.js")))
                    GenerateJsonFile(RunSprocReturnDataTable("spQualityGetMeasureTopicCategories", new KeyValuePair<string, object>("@ReportID", _reportID)),
                        Path.Combine(_baseMeasuresDir, "TopicCategories.js"), jsonDomain + ".measuretopiccategories=");


                if (!File.Exists(Path.Combine(_baseMeasuresDir, "TopicsConsumer.js")))
                    GenerateJsonFile(RunSprocReturnDataTable("spQualityGetMeasureTopics",
                        new[] {
                            new KeyValuePair<string, object>("@ReportID", _reportID),
                            new KeyValuePair<string, object>("@AudienceType","consumer")
                        }), Path.Combine(_baseMeasuresDir, "TopicsConsumer.js"), jsonDomain + ".measuretopics_consumer=");

                if (!File.Exists(Path.Combine(_baseMeasuresDir, "TopicCategoriesConsumer.js")))
                {
                    var topicCategories = GetMeasureTopicCategories(new KeyValuePair<string, object>("@ReportID", _reportID),
                          new KeyValuePair<string, object>("@IsCostQualityIncluded", CurrentWebsite.Reports.Any(x => x.Report.Name.EqualsIgnoreCase("Cost and Quality Report – Side By Side Display")) && CurrentWebsite.Measures.Any(m => m.ReportMeasure.SupportsCost)));

                    GenerateJsonFile(topicCategories, Path.Combine(_baseMeasuresDir, "TopicCategoriesConsumer.js"), jsonDomain + ".measuretopiccategories_consumer=");

                }

                if (publishTask == PublishTask.PreviewOnly)
                {
                    return;
                }

                foreach (DataRow row in _measures.Rows)
                {
                    if (File.Exists(Path.Combine(_baseMeasuresDir, "Measure_" + row["ID"] + ".js"))) continue;

                    var result = RunSprocReturnDataTable("spQualityGetMeasure", new[]
                    {
                        new KeyValuePair<string, object>("@ReportID", _reportID),
                        new KeyValuePair<string, object>("@MeasureID", row["ID"])
                    });
                    if (result.Rows.Count < 1) continue;

                    GenerateJsonFile(result, Path.Combine(_baseMeasuresDir, "Measure_" + row["ID"] + ".js"), jsonDomain + ".measuredescription_" + row["ID"] + "=");
                }

                // Get the quality data by hospital.
                foreach (DataRow row in HospitalIds.Rows)
                {
                    var hospitalDataTable = RunSprocReturnDataTable("spQualityGetDataByHospital", new[] {
                                            new KeyValuePair<string, object>("@ReportID", _reportID),
                                            new KeyValuePair<string, object>("@HospitalID", row["ID"])
                                        });
                    GenerateJsonFile(hospitalDataTable, Path.Combine(_qualityHospitalDataDir, "hospital_" + row["ID"] + ".js"), jsonDomain + ".hospital_" + row["ID"] + "=");
                }

                // Get the quality data by measure.
                foreach (DataRow row in _measures.Rows)
                {

                    DataTable measureDataTable = RunSprocReturnDataTable("spQualityGetDataByMeasure", new[] {
                            new KeyValuePair<string, object>("@ReportID", _reportID),
                            new KeyValuePair<string, object>("@MeasureID", row["ID"])
                        });
                    GenerateJsonFile(measureDataTable, Path.Combine(_qualityMeasureDataDir, "measure_" + row["ID"] + ".js"), jsonDomain + ".measure_" + row["ID"] + "=");

                    //if (measureDataTable.Rows.Count > 0)
                    //{
                    //	var currentWebsiteMeasure = CurrentWebsite.Measures.Where(m => m.ReportMeasure.Id > 0 && m.ReportMeasure.Id == (int)row["ID"]).Select(m => m).FirstOrDefault();
                    //	QualityReportSuppressionLogic.Suppress(measureDataTable, currentWebsiteMeasure);
                    //	GenerateJsonFile(measureDataTable, Path.Combine(QualityMeasureDataDir, "measure_" + row["ID"] + ".js"), jsonDomain + ".measure_" + row["ID"] + "=");
                    //}
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        /// <summary>
        /// To get the measure topic categories from the database.
        /// </summary>
        /// <param name="keyValuePair1">The key value pair1.</param>
        /// <param name="keyValuePair2">The key value pair2.</param>
        /// <returns></returns>
        private List<TopicCategoryStruct> GetMeasureTopicCategories(KeyValuePair<string, object> keyValuePair1, KeyValuePair<string, object> keyValuePair2)
        {
            var defaultFactsImagePath = Path.Combine(Environment.CurrentDirectory, "Resources", "Templates", "Site", "themes", "consumer", "assets", "infographic", "facts-tips");
            var data = RunSprocReturnDataTable("spQualityGetMeasureTopicCategoriesConsumer", new[] { keyValuePair1, keyValuePair2 });
            var result = new List<TopicCategoryStruct>();
            //DataContractSerializer serializer = new DataContractSerializer(typeof(FactStruct));

            foreach (DataRow row in data.AsEnumerable())
            {
                result.Add(new TopicCategoryStruct(_factsImagePath, defaultFactsImagePath, CurrentWebsite.OutPutDirectory)
                {
                    TopicCategoryID = row.IsNull("TopicCategoryID") ? null : (int?)Convert.ToInt32(row["TopicCategoryID"]),
                    Name = row.IsNull("Name") ? null : Convert.ToString(row["Name"]),
                    ConsumerDescription = row.IsNull("ConsumerDescription") ? null : Convert.ToString(row["ConsumerDescription"]),
                    LongTitle = row.IsNull("LongTitle") ? null : Convert.ToString(row["LongTitle"]),
                    TipsChecklist = row.IsNull("TipsChecklist") ? null : Convert.ToString(row["TipsChecklist"]),
                    TopicIcon = row.IsNull("TopicIcon") ? null : Convert.ToString(row["TopicIcon"]),
                    TopicFacts = row.IsNull("Facts") ? null : JsonHelper.Deserialize<List<TopicFacts>>(Convert.ToString(row["Facts"])) as List<TopicFacts>,
                });
            }


            return result;
        }

        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        protected override bool LoadReportData()
        {
            // Initialize the data for this report.
            InitializeReportData();
            return true;
        }

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
        {
            // Generate the json files for the report.
            GenerateJsonFiles(_publishTask);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Struct for topic category.
    /// </summary>
    [DataContract]
    internal class TopicCategoryStruct
    {
        private string _factsImagePath;
        private string _defaultFactsImagePath;
        private string _outputDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicCategoryStruct"/> class.
        /// </summary>
        /// <param name="factImagePath">The fact image path.</param>
        /// <param name="defaultFactsImagePath">The default facts image path.</param>
        /// <param name="outputDir">The output dir.</param>
        public TopicCategoryStruct(string factImagePath, string defaultFactsImagePath, string outputDir)
        {
            _factsImagePath = factImagePath;
            _defaultFactsImagePath = defaultFactsImagePath;
            _outputDir = outputDir;
        }

        /// <summary>
        /// Gets or sets the topic category identifier.
        /// </summary>
        /// <value>
        /// The topic category identifier.
        /// </value>
        [DataMember(Name = "TopicCategoryID")]
        public int? TopicCategoryID { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "Name")]
        public string Name { get; set; }
        [DataMember(Name = "LongTitle")]
        public string LongTitle { get; set; }
        /// <summary>
        /// Gets or sets the consumer description.
        /// </summary>
        /// <value>
        /// The consumer description.
        /// </value>
        [DataMember(Name = "ConsumerDescription")]
        public string ConsumerDescription { get; set; }
        /// <summary>
        /// Gets or sets the tips checklist.
        /// </summary>
        /// <value>
        /// The tips checklist.
        /// </value>
        [DataMember(Name = "TipsChecklist")]
        public string TipsChecklist { get; set; }
        /// <summary>
        /// Gets or sets the topic icon.
        /// </summary>
        /// <value>
        /// The topic icon.
        /// </value>
        [DataMember(Name = "TopicIcon")]
        public string TopicIcon { get; set; }

        [DataMember(Name = "TopicFacts")]
        public List<FactStruct> Facts
        {
            get
            {
                var newList = new List<FactStruct>();
                if (TopicFacts != null)
                {
                    TopicFacts.ForEach(x =>
                    {
                        var fact = new FactStruct(x);
                        if (!string.IsNullOrEmpty(fact.ImagePath))
                        {
                            var fileName = Path.GetFileName(fact.ImagePath);
                            var dstFile = Path.Combine(_factsImagePath, fileName);
                            var sourceFile = File.Exists(fact.ImagePath) ? fact.ImagePath : Path.Combine(_defaultFactsImagePath, fileName);

                            if (File.Exists(sourceFile))
                            {
                                File.Copy(sourceFile, dstFile, true);
                                fact.ImagePath = dstFile.Replace(_outputDir + @"\", "");
                            }
                        }
                        newList.Add(fact);
                    });
                }
                return newList;
            }
        }

        /// <summary>
        /// Gets or sets the topic facts.
        /// </summary>
        /// <value>
        /// The topic facts.
        /// </value>
        [IgnoreDataMember]
        public List<TopicFacts> TopicFacts { get; set; }
    }

    /// <summary>
    /// Facts struct
    /// </summary>
    [DataContract]
    internal class FactStruct
    {
        /// <summary>
        /// Gets or sets the image path.
        /// </summary>
        /// <value>
        /// The image path.
        /// </value>
        [DataMember(Name = "Image")]
        public string ImagePath { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        [DataMember(Name = "Text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the citation text.
        /// </summary>
        /// <value>
        /// The citation text.
        /// </value>
        [DataMember(Name = "Citation")]
        public string CitationText { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactStruct"/> class.
        /// </summary>
        /// <param name="fact">The fact.</param>
        public FactStruct(TopicFacts fact)
        {
            ImagePath = fact.ImagePath;
            Text = fact.Text;
            CitationText = fact.CitationText;
        }

    }

}


