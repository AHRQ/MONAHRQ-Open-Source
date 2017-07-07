using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Domain.Physicians;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Monahrq.Wing.Physician.Reports
{
	/// <summary>
	/// Generates the report data/.json files for CGCAHPS reports, a sub report of the Physician Report.
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
	[ReportGenerator(new[] { "4C5727B4-0E85-4F80-ADE9-418B49A1373E", "E007BB9C-E539-41D6-9D06-FF52F8A15BF6" },
		new string[] { },
		new[] { typeof(CGCAHPS.CGCAHPSSurveyTarget) },
		5, null,
		"CGCAHPS Survey")]
	public class CGCAHPSReportGenerator : BaseReportGenerator
	{
		#region Fields and Constants

		/// <summary>
		/// The mp measures folder
		/// </summary>
		private string _mpMeasuresFolder;
		/// <summary>
		/// The medical practice report folder
		/// </summary>
		private string _medicalPracticeReportFolder;
		/// <summary>
		/// The overall measure identifier
		/// </summary>
		private const string _overallMeasureId = "CG_ALL";
		/// <summary>
		/// The composite meause ids
		/// </summary>
		private static List<string> _compositeMeauseIds = new List<string> { "AV_COMP_01", "AV_COMP_02", "AV_COMP_03", "AV_COMP_04", "AV_COMP_05", "AV_COMP_06", "CD_COMP_01", "CD_COMP_02" };
		/// <summary>
		/// The topic names
		/// </summary>
		private static List<string> _topicNames = new List<string> { "Adult Surveys", "Children Surveys" };
		/// <summary>
		/// The measure description ns
		/// </summary>
		private const string _measureDescriptionNS = "$.monahrq.MedicalPractices.Base.MeasureDescriptions";
		/// <summary>
		/// The measure topic ns
		/// </summary>
		private const string _measureTopicNS = "$.monahrq.MedicalPractices.Base.MeasureTopics";
		/// <summary>
		/// The topic category ns
		/// </summary>
		private const string _topicCategoryNS = "$.monahrq.MedicalPractices.Base.MeasureTopicCategories";
		/// <summary>
		/// The medical practice ns
		/// </summary>
		private const string _medicalPracticeNS = "$.monahrq.MedicalPractices.Report.CGCAHPS.MedicalPractice";
		/// <summary>
		/// The measure descriptions
		/// </summary>
		private List<MeasureDescriptionJs> _measureDescriptions;
		/// <summary>
		/// The measure topics
		/// </summary>
		private List<MeasureTopicJs> _measureTopics;
		/// <summary>
		/// The included cgcahps measures
		/// </summary>
		private List<Measure> _includedCGCAHPSMeasures;
		/// <summary>
		/// The topics categories
		/// </summary>
		private IEnumerable<TopicCategory> _topicsCategories;
		/// <summary>
		/// The topics
		/// </summary>
		private IList<Topic> _topics;
		/// <summary>
		/// The question types
		/// </summary>
		private IEnumerable<CGCAHPSMeasureLookup> _questionTypes;
		/// <summary>
		/// The medical practices
		/// </summary>
		private ConcurrentDictionary<string, IEnumerable<MedicalPracticeStruct>> _medicalPractices;
		/// <summary>
		/// The clear working table script file
		/// </summary>
		private string _clearWorkingTableScriptFile;
		/// <summary>
		/// The topic category identifier lookup
		/// </summary>
		public static Dictionary<string, int> TopicCategoryIdLookup = new Dictionary<string, int> { { "Adult Surveys", 10 }, { "Child Surveys", 11 } };
		/// <summary>
		/// The composite measures
		/// </summary>
		public IEnumerable<Measure> _compositeMeasures;


		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CGCAHPSReportGenerator"/> class.
		/// </summary>
		public CGCAHPSReportGenerator()
		{
			_measureDescriptions = new List<MeasureDescriptionJs>();
			_measureTopics = new List<MeasureTopicJs>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
		{
			EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Physicians CGCAHPS reports" });

			string[] scriptFiles = new string[]
			{
				"Sproc-spCGAdultCompFinal.sql","Sproc-spCGChildCompFinal.sql","Sproc-spCGAdultChildUpdate.sql"
			};

			RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\Physician\\"), scriptFiles);

			_clearWorkingTableScriptFile = Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\Physician\\", "Script-ClearCGWorkingTables.sql");
		}

		/// <summary>
		/// Loads the report data.
		/// </summary>
		/// <returns></returns>
		protected override bool LoadReportData()
		{
			_mpMeasuresFolder = Path.Combine(BaseDataDirectoryPath, "Base", "MedicalPracticeMeasures");
			_medicalPracticeReportFolder = Path.Combine(BaseDataDirectoryPath, "MedicalPractices");
			_measureTopics = new List<MeasureTopicJs>();

			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				_topicsCategories = session.CreateCriteria<TopicCategory>()
					.CreateCriteria("Topics")
					.Add(Restrictions.In("Name", _topicNames))
					.SetMaxResults(1)
					.Future<TopicCategory>();

				_compositeMeasures = session.CreateCriteria<Measure>()
					.Add(Restrictions.Or(Restrictions.In("Name", _compositeMeauseIds), Restrictions.Eq("Name", _overallMeasureId)))
					.Future<Measure>();

				_questionTypes = session.CreateCriteria<CGCAHPSMeasureLookup>()
					.Future<CGCAHPSMeasureLookup>();

				var count = 0;

				var compMeasures = _compositeMeasures.Where(x => x.Name != _overallMeasureId).ToList();

				//for every composite measure create an MeasureTopic entity
				foreach (var compositeMeasure in compMeasures)
				{
					var category = _topicsCategories.FirstOrDefault() ?? new TopicCategory("");
					_topics = category.Topics;
					foreach (var topic in _topics)
					{
						if (topic.Name != compositeMeasure.MeasureType) continue;

						var lowerBound = compMeasures[count].Id;
						var upperBound = compMeasures.Count() != count + 1 ? compMeasures[count + 1].Id : lowerBound;

						var subMeasures = _measureDescriptions.Where(x => x.TopicIds.Contains(topic.Id))
							.Where(x => !compMeasures.Select(m => m.Id).Contains(x.MeasureId))
							.Where(x => lowerBound != upperBound ? x.MeasureId >= lowerBound && x.MeasureId <= upperBound : x.MeasureId > upperBound);
						var measureTopic = new MeasureTopicJs(topic, subMeasures.Select(x => x.MeasureId).ToList(), TopicCategoryIdLookup[topic.Name]);
						var compMeasure = compMeasures.FirstOrDefault(x => x.Id <= upperBound && x.Id >= lowerBound);
						measureTopic.OverallMeasure = compMeasure != null ? compMeasure.Id : -1;
						measureTopic.Id = count + 1;
						measureTopic.Name = compMeasure != null ? compMeasure.ConsumerPlainTitle : "";
						measureTopic.LongTitle = compMeasure != null ? compMeasure.ConsumerPlainTitle : "";
						_measureTopics.Add(measureTopic);
					}
					count++;
				}
			}


			return true;
		}

		/// <summary>
		/// Gets the database responses.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="session">The session.</param>
		/// <param name="clause">The clause.</param>
		/// <returns></returns>
		private static List<string> GetDBResponses<T>(NHibernate.ISession session, Expression<Func<T, bool>> clause)
			where T : EnumLookupEntity<int>
		{
			return
				session.QueryOver<T>().OrderBy(x => x.Value).Asc.List().Select((x, index) =>
					String.Format("{0},{1},{2},{3}",
						x.Id,
						Inflector.Titleize(x.Name),
						index + 1,
						clause.Compile()(x) ? "Include" : "Exclude")).ToList();
		}

		/// <summary>
		/// Outputs the data files.
		/// </summary>
		/// <returns></returns>
		protected override bool OutputDataFiles()
		{
			_medicalPractices = new ConcurrentDictionary<string, IEnumerable<MedicalPracticeStruct>>();

			//Generate MeasureDescription files
			_measureDescriptions.ForEach(md =>
			{
				var question = _questionTypes.FirstOrDefault(x => x.MeasureId.Replace("_Rate", "").Contains(md.MeasuresName));
				md.QuestionType = question != null ? question.CAHPSQuestionType : null;
				var topic = _measureTopics.Where(x => md.TopicIds.Contains(x.TopicId)).FirstOrDefault(x => x.OverallMeasure.Equals(md.MeasureId) || x.MeasureIds.Contains(md.MeasureId));
				if (topic != null) md.TopicId = topic.Id;

				GenerateJsonFile(md, Path.Combine(_mpMeasuresFolder, md.FileName), string.Format("{0}['{1}'] =", _measureDescriptionNS, md.MeasureId));
			});

			//Generate MeasureTopic file
			GenerateJsonFile(_measureTopics, Path.Combine(_mpMeasuresFolder, string.Format("{0}{1}", MeasureTopicJs.FileName, MeasureTopicJs.FileExtension)), string.Format("{0} =", _measureTopicNS));

			//Generate MeasureCategories file
			GenerateJsonFile(_topics.Select(x => new TopicCategoryStruct(x)).ToList(), Path.Combine(_mpMeasuresFolder, "MeasureTopicCategories.js"), string.Format("{0} =", _topicCategoryNS));

			var cgCahpsResponses = new Dictionary<string, List<string>>();

			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				cgCahpsResponses.Add("YESNO", GetDBResponses<YesNo>(session, CurrentWebsite.YesNoExcludeClause));
				cgCahpsResponses.Add("HOWOFN4F", GetDBResponses<HowOften>(session, CurrentWebsite.HowOftenExcludeClause));
				cgCahpsResponses.Add("DEFINITE", GetDBResponses<Definite>(session, CurrentWebsite.DefiniteExcludeClause));
				cgCahpsResponses.Add("RATEDOCT", GetDBResponses<Ratings>(session, CurrentWebsite.RatingExcludeClause));
				cgCahpsResponses.Add("Avg", new List<string> { });
			}

			var websiteIdParam = new KeyValuePair<string, object>("@WebsiteId", CurrentWebsite.Id);
			RunSqlScriptFile(_clearWorkingTableScriptFile);
			RunSproc("spCGAdultCompFinal", "Init CG-CAHPS Adult", new[] { websiteIdParam });
			RunSproc("spCGChildCompFinal", "Init CG-CAHPS Child", new[] { websiteIdParam });
			var result = RunSprocReturnDataTable("spCGAdultChildUpdate", new[] { websiteIdParam }) as DataTable;

			//Parallel.ForEach(result.AsEnumerable(), (row) =>
			foreach (var row in result.AsEnumerable())
			{
				try
				{
					var data = new MedicalPracticeStruct
					{
						MedicalPracticeId = !row.IsNull("practice_id") ? Convert.ToString(row["practice_id"]) : "",
						MeasusreId = !row.IsNull("MonMeasureId") ? (int?)Convert.ToInt32(row["MonMeasureId"]) : null,
						Rating = !row.IsNull("rating") ? string.Format("{0:#0.00%}", Convert.ToDecimal(row["rating"])) : null,      // Should be "Rate" not "Rating"
						PeerRating = !row.IsNull("peer_rating") ? Convert.ToString(row["peer_rating"]) : null,
						PeerRate = !row.IsNull("peer_Rate") ? string.Format("{0:#0.00%}", Convert.ToDecimal(row["peer_Rate"])) : null,
						CAHPSResponseValues = GetCAHPSResponseValues(row, row["MeasureId"].ToString(), cgCahpsResponses, _questionTypes)
					};

					if (_medicalPractices.ContainsKey(data.MedicalPracticeId))
					{
						//  I believe this line below to be the cause of exceptions with this code being in a Parallel.Foreach
						//	- Fix would be to have a temp list of Parallel friendly that run, and then consolidate the temp
						//		with the actual after the Parallel.
						//	- But there is not really enough 'work' happening in this loop to warrant the effort.
						var val = _medicalPractices[data.MedicalPracticeId] as List<MedicalPracticeStruct>;
						val.Add(data);
					}
					else
					{
						_medicalPractices.TryAdd(data.MedicalPracticeId, new List<MedicalPracticeStruct> { data });
					}
				}
				catch (Exception ex)
				{
					//var excToUse = ex.GetBaseException();
					var eecMessage = string.Format("{0} : {1}", "CGCAHPSReportGenerator", ex.Message);
					//Logger.Log(eecMessage, Category.Exception, Priority.High);

					//PublishEvent(eecMessage, PubishMessageTypeEnum.Error, WebsiteGenerationStatus.Error, DateTime.Now);
					throw;
				}
			}//);
			//RunSqlScriptFile(_clearWorkingTableScriptFile);

			//Generate MedicalPractice file 
			_medicalPractices.ToList().ForEach(mp =>
			{
				GenerateJsonFile(mp.Value, Path.Combine(_medicalPracticeReportFolder, string.Format("MedicalPractice_{0}.js", mp.Key)), string.Format("{0}['{1}'] =", _medicalPracticeNS, mp.Key));

			});

			return true;
		}

		/// <summary>
		/// Gets the cahps response values.
		/// </summary>
		/// <param name="row">The row.</param>
		/// <param name="measureCode">The measure code.</param>
		/// <param name="cgCahpsResponses">The cg cahps responses.</param>
		/// <param name="questionTypes">The question types.</param>
		/// <returns></returns>
		private List<string> GetCAHPSResponseValues(DataRow row, string measureCode, Dictionary<string, List<string>> cgCahpsResponses, IEnumerable<CGCAHPSMeasureLookup> questionTypes)
		{
			var columnNames = new List<string>();

			var defaultResponsevals = 11;
			var questionType = questionTypes.GroupBy(x => x.CAHPSQuestionType).Where(x => x.ToList().Any(val => val.MeasureId.Equals(measureCode))).Select(x => x.Key).FirstOrDefault() ?? "";
			if (!string.IsNullOrEmpty(questionType) && cgCahpsResponses[questionType] != null)
			{
				foreach (var item in cgCahpsResponses[questionType])
				{
					var itemParts = item.Split(new char[] { ',' });
					if (itemParts[3] == "Exclude") continue;
					columnNames.Add("response" + itemParts[2]);
				}
			}
			else
			{
				for (var i = 0; i < defaultResponsevals; i++)
				{
					columnNames.Add("response" + (i + 1).ToString());
				}
			}

			return ProcessResponseValues(row, columnNames);
		}

		/// <summary>
		/// Processes the response values.
		/// </summary>
		/// <param name="row">The row.</param>
		/// <param name="columnNames">The column names.</param>
		/// <returns></returns>
		private List<string> ProcessResponseValues(DataRow row, List<string> columnNames)
		{
			var responses = new List<string>();

			foreach (var columnName in columnNames)
			{
				if (row.IsNull(columnName))
				{
					responses.Add(null);
					continue;
				}

				var val = Convert.ToDecimal(row[columnName]);
				responses.Add(string.Format("{0:#0.00%}", val));
			}

			return responses;
		}

		/// <summary>
		/// Validates the dependencies.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <param name="validationResults">The validation results.</param>
		/// <returns></returns>
		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
		{
			if (base.ValidateDependencies(website, validationResults))
			{

				var isDatasetIncluded = CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.ToLower().Contains("CG-CAHPS Survey Results Data".ToLower()));
				if (!isDatasetIncluded) return false;

				var reportAttributeSelected = CurrentWebsite.Reports.Any(x => x.Report.Filters.Any(f => f.Values.Any(v => v.Value && v.Name == "Medical Practice")));
				if (!reportAttributeSelected) return false;

				_includedCGCAHPSMeasures = CurrentWebsite.Measures
				.Where(m => (m.OverrideMeasure == null && m.OriginalMeasure.Source != null && m.OriginalMeasure.Source.Contains("CG-CAHPS survey")) ||
				((m.OverrideMeasure != null && m.OverrideMeasure.Source != null && m.OverrideMeasure.Source.Contains("CG-CAHPS survey"))))
				.Select(x => x.OverrideMeasure == null ? x.OriginalMeasure : x.OverrideMeasure)
				.ToList();

				_measureDescriptions = _includedCGCAHPSMeasures.Select(m => new MeasureDescriptionJs(m)).ToList();

				if (!_measureDescriptions.Any()) validationResults.Add(new ValidationResult("You have not selected CG-CAHPS survey measures. CG-CAHPS survey report could not be generated."));

			}

			return validationResults == null || validationResults.Count == 0;
		}

		/// <summary>
		/// 
		/// </summary>
		private struct TopicCategoryStruct
		{
			/// <summary>
			/// Gets or sets the topic category identifier.
			/// </summary>
			/// <value>
			/// The topic category identifier.
			/// </value>
			public int TopicCategoryID { get; set; }
			/// <summary>
			/// Gets or sets the name.
			/// </summary>
			/// <value>
			/// The name.
			/// </value>
			public string Name { get; set; }
			/// <summary>
			/// Gets or sets the long title.
			/// </summary>
			/// <value>
			/// The long title.
			/// </value>
			public string LongTitle { get; set; }
			/// <summary>
			/// Gets or sets the description.
			/// </summary>
			/// <value>
			/// The description.
			/// </value>
			public string Description { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="TopicCategoryStruct"/> struct.
			/// </summary>
			/// <param name="topic">The topic.</param>
			public TopicCategoryStruct(Topic topic) : this()
			{
				TopicCategoryID = TopicCategoryIdLookup[topic.Name];
				Name = topic.Name;
				LongTitle = topic.LongTitle ?? string.Empty;
				Description = topic.Description ?? string.Empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[DataContract]
		private struct MedicalPracticeStruct
		{
			#region Fields and Constants

			/// <summary>
			/// The rating
			/// </summary>
			private string _rating;
			/// <summary>
			/// The peer rating
			/// </summary>
			private string _peerRating;
			/// <summary>
			/// The peer rate
			/// </summary>
			private string _peerRate;

			#endregion

			/// <summary>
			/// Gets or sets the medical practice identifier.
			/// </summary>
			/// <value>
			/// The medical practice identifier.
			/// </value>
			[IgnoreDataMember]
			public string MedicalPracticeId { get; set; }

			/// <summary>
			/// Gets the group practice pac identifier.
			/// </summary>
			/// <value>
			/// The group practice pac identifier.
			/// </value>
			[DataMember(Name = "MedicalPracticeID", Order = 1)]
			//	public long? GroupPracticePacId { get { return !string.IsNullOrEmpty(MedicalPracticeId) ? (long?)Convert.ToInt64(MedicalPracticeId) : null; } }
			public string GroupPracticePacId { get { return !string.IsNullOrEmpty(MedicalPracticeId) ? MedicalPracticeId : null; } }

			/// <summary>
			/// Gets or sets the name.
			/// </summary>
			/// <value>
			/// The name.
			/// </value>
			[IgnoreDataMember]
			public string Name { get; set; }
			/// <summary>
			/// Gets or sets the measusre identifier.
			/// </summary>
			/// <value>
			/// The measusre identifier.
			/// </value>
			[DataMember(Name = "MeasureID", Order = 2)]
			//public string MeasusreId { get; set; }
			public int? MeasusreId { get; set; }
			/// <summary>
			/// Gets or sets the rating.
			/// </summary>
			/// <value>
			/// The rating.
			/// </value>
			[DataMember(Name = "Rate", Order = 3)]
			public string Rating { get { return _rating ?? "-"; } set { _rating = value; } }
			/// <summary>
			/// Gets or sets the peer rating.
			/// </summary>
			/// <value>
			/// The peer rating.
			/// </value>
			[DataMember(Name = "PeerRating", Order = 4)]
			public string PeerRating { get { return _peerRating ?? "-"; } set { _peerRating = value; } }
			/// <summary>
			/// Gets or sets the peer rate.
			/// </summary>
			/// <value>
			/// The peer rate.
			/// </value>
			[DataMember(Name = "PeerRate", Order = 5)]
			public string PeerRate { get { return _peerRate ?? "-"; } set { _peerRate = value; } }
			/// <summary>
			/// Gets or sets the cahps response values.
			/// </summary>
			/// <value>
			/// The cahps response values.
			/// </value>
			[DataMember(Name = "CAHPSResponseValues", Order = 6)]
			public List<string> CAHPSResponseValues { get; set; }

		}

		#endregion
	}
}
