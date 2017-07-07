using Monahrq.Sdk.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Wing.NursingHomeCompare.NHCAHPS;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Domain.Websites;
using System.Runtime.Serialization;
using System.IO;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Utility;
using Monahrq.Infrastructure;
using System.Data;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Domain.BaseData;
using System.Collections.Concurrent;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Wing.NursingHomeCompare.Reports
{
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
	[ReportGenerator(new[] { "87E04110-46B0-4CAE-9592-022C3111FAC7", "BA52B7B2-F4C8-4831-B910-1D036B94AE75", "F2F2B7FE-8653-488B-8ED8-FD4417CD0F9E" },
				 new[] { "NH-CAHPS Survey Results Data" },
				 new[] { typeof(NHCAHPSSurveyTarget) },
				 3, null,
				 "NHCAHPS Survey")]
	public class NHCAHPSReportGenerator : BaseReportGenerator
	{
		#region Fields and Constants

		private string _measuresDir;
		private string _nursingHomesDir;
		private const string _overallMeasureId = "NH_COMP_OVERALL";
		private const string _measuresNS = "$.monahrq.NursingHomes.Report.Measures";
		private const string _nursingHomeNS = "$.monahrq.NursingHomes.Report.NursingHomes";
		private List<Measure> _includedNHCAHPSMeasures { get; set; }
		private List<NursingHome> _includedNursingHomes { get; set; }
		private string _clearWorkingTableScriptFile;

		#endregion

		public override void InitGenerator()
		{
			EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Nursing Home CAHPS reports" });

			// scripts init
			string[] scriptFiles = new string[] { "Sproc-spNHCompFinal.sql", "Sproc-spNHUpdate.sql" };
			RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\NHCAHPS\\"), scriptFiles);

			_clearWorkingTableScriptFile = Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\NHCAHPS\\", "Script-ClearNHWorkingTables.sql");

		}

		protected override bool LoadReportData()
		{
			_measuresDir = Path.Combine(BaseDataDirectoryPath, "NursingHomes", "Measures");
			_nursingHomesDir = Path.Combine(BaseDataDirectoryPath, "NursingHomes", "NursingHomes");
			return true;
		}


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

		protected override bool OutputDataFiles()
		{
			var _nhCahpsReportMeasures = new List<NHCahpsReportMeasure>();
			var nhCahpsResponses = new Dictionary<string, List<string>>();
			var questionTypes = new List<NHCAHPSMeasureLookup>();

			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				nhCahpsResponses.Add("YNB/YNA", GetDBResponses<YesNo>(session, CurrentWebsite.YesNoExcludeClause));
				nhCahpsResponses.Add("OFTEN", GetDBResponses<HowOften>(session, CurrentWebsite.HowOftenExcludeClause));
				nhCahpsResponses.Add("TIMES", GetDBResponses<NumberOfTimes2>(session, CurrentWebsite.Times2ExcludeClause));
				nhCahpsResponses.Add("DEFINITE", GetDBResponses<Definite>(session, CurrentWebsite.DefiniteExcludeClause));
				nhCahpsResponses.Add("RATING", GetDBResponses<Ratings>(session, CurrentWebsite.RatingExcludeClause));
				nhCahpsResponses.Add("Avg", new List<string> { });
				questionTypes = session.QueryOver<NHCAHPSMeasureLookup>().List().ToList();
			}

			//	Generate
			RunSqlScriptFile(_clearWorkingTableScriptFile);
			var websiteIdParam = new KeyValuePair<string, object>("@websiteId", CurrentWebsite.Id);
			RunSproc("spNHCompFinal", "Init NH Comp", new[] { websiteIdParam });
			var result = RunSprocReturnDataTable("spNHUpdate", new[] { websiteIdParam }) as DataTable;

			foreach (var row in result.AsEnumerable())
			{
				var data = new NHCahpsReportMeasure()
				{
					NursingHomeID = row.SafeField<int>("NursingHomeId"),
					MeasureID = row.SafeField<int>("MonMeasureId"),
					Rate = string.Format("{0:#0.00%}", row.SafeField<decimal>("rating")),
					PeerRating = row.SafeField<string>("peer_rating"),
					PeerRate = string.Format("{0:#0.00%}", row.SafeField<decimal>("peer_rate")),
					ResponseValues = NHCahpsReportMeasure.GetCAHPSResponseValues(row, row.SafeField<string>("MeasureId"), nhCahpsResponses, questionTypes),
				};

				_nhCahpsReportMeasures.Add(data);
			}


			// RunSqlScriptFile(_clearWorkingTableScriptFile);

			//Generate Measure.js files
			_nhCahpsReportMeasures.GroupBy(x => x.MeasureID).ToList().ForEach(m =>
			{
				JsonHelper.GenerateJsonFile(m.ToList(), Path.Combine(_measuresDir, string.Format("Measure_{0}.js", m.Key)), string.Format("{0}['{1}']=", _measuresNS, m.Key));
			});

			//Generate NursingHome.js files
			_nhCahpsReportMeasures.GroupBy(x => x.NursingHomeID).ToList().ForEach(nh =>
			{
				var fileName = Path.Combine(_nursingHomesDir, string.Format("NursingHome_{0}.js", nh.Key));
				var nameSpace = string.Format("{0}['{1}']=", _nursingHomeNS, nh.Key);
				if (File.Exists(fileName))
				{
					var content = File.ReadAllText(fileName).Replace(nameSpace, "").Replace(";", "");
					var items = JsonHelper.Deserialize<List<BaseNursingHomeReportGenerator.NursingHomeReportMeasure>>(content);
					if (items != null)
					{
						items.AddRange(nh.ToList());
						JsonHelper.GenerateJsonFile(items, fileName, nameSpace);
					}
				}
				else
					JsonHelper.GenerateJsonFile(nh.ToList(), fileName, nameSpace);
			});

			return true;
		}

		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
		{
			if (base.ValidateDependencies(website, validationResults))
			{
				var isDatasetIncluded = CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.ToLower().Contains("NH-CAHPS Survey Results Data".ToLower()));
				if (!isDatasetIncluded) return false;

				_includedNHCAHPSMeasures = CurrentWebsite.Measures
					.Where(m => (m.OverrideMeasure == null && m.OriginalMeasure.Source != null && m.OriginalMeasure.Source.Contains("NH-CAHPS survey")) ||
					((m.OverrideMeasure != null && m.OverrideMeasure.Source != null && m.OverrideMeasure.Source.Contains("NH-CAHPS survey"))))
					.Select(x => x.OverrideMeasure == null ? x.OriginalMeasure : x.OverrideMeasure)
					.ToList();
				_includedNursingHomes = CurrentWebsite.NursingHomes.Select(x => x.NursingHome).ToList();

				if (!_includedNHCAHPSMeasures.Any()) validationResults.Add(new ValidationResult("You have not selected NH-CAHPS survey measures. NH-CAHPS survey report could not be generated."));

				if (!_includedNursingHomes.Any()) validationResults.Add(new ValidationResult("You have not selected Nursing Homes. NH-CAHPS survey report could not be generated."));

			}

			return validationResults == null || validationResults.Count == 0;
		}

		internal class NHCahpsReportMeasure : BaseNursingHomeReportGenerator.NursingHomeReportMeasure
		{
			[DataMember(Name = "CAHPSResponseValues", Order = 10)]
			public List<string> ResponseValues { get; set; }

			public NHCahpsReportMeasure()
			{
			}

			public NHCahpsReportMeasure(Measure measure)
			{
				MeasureID = measure.Id;
				ResponseValues = new List<string> { "30%", "75%" };
			}

			public NHCahpsReportMeasure(NursingHome nursingHome, Measure measure) : this(measure)
			{
				NursingHomeID = nursingHome.Id;
				Rate = "-";
				NatRating = "-";
				NatRate = "-";
				PeerRating = "-";
				PeerRate = "-";
				CountyRating = "-";
			}

			internal static List<string> GetCAHPSResponseValues(DataRow row, string measureCode, Dictionary<string, List<string>> cgCahpsResponses, List<NHCAHPSMeasureLookup> questionTypes)
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

			private static List<string> ProcessResponseValues(DataRow row, List<string> columnNames)
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


		}
	}
}
