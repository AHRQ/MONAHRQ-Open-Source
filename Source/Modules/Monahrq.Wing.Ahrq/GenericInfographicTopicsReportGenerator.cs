using System.Data;
using System.Data.SqlClient;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.Services.Generators;
using NHibernate.Mapping;
using System.Runtime.Serialization;
using NHibernate.Transform;
using Microsoft.Practices.ServiceLocation;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    /// class for ReportGenerator.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    /// <seealso cref="Monahrq.Sdk.Generators.IReportGenerator" />
    [Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
	[ReportGenerator(new string[] { "64B545BF-D183-4F0D-8ABC-6643FF39F5DE" },
					 new string[] { },
					 new[] { typeof(Area.AreaTarget) },
					 3)]
	public class GenericInfographicTopicsReportGenerator : BaseReportGenerator, IReportGenerator
	{
        #region Variables.
        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        private string FilePath { get; set; }
        /// <summary>
        /// Gets or sets the base data dir.
        /// </summary>
        /// <value>
        /// The base data dir.
        /// </value>
        private string BaseDataDir { get; set; }
        #region SQL Query Variable.
        /// <summary>
        /// The SQL get data
        /// </summary>
        private const string sqlGetData = @"
											with 
											IGTopics (TopicId) as
											(
												select distinct	mt.Topic_Id
												from			Measures_MeasureTopics mt
												where			mt.UsedForInfographic = 1
											),
											VolumeMeasuresStateTotals (MeasureName, StateAvg) as
											(
												select			m.Name
															,	avg(cast([Col1] as decimal)) [Col1]  
												from			Temp_Quality tq
													inner join	Measures m on tq.MeasureID = m.Id
												where			m.MeasureType = 'QIVolume'
													and		(	[Col1] <> '-' and [Col1] <> 'c')
												group by		m.Name
											)

											select			t.TopicCategory_id as TopicCategory
														,	m.Name as MeasureName
														,	isnull(case m.MeasureType
																when 'QIVolume' then (select case when StateAvg <= m.SuppressionNumerator then '-1' else cast(cast(StateAvg as decimal(8,1)) as varchar) end from VolumeMeasuresStateTotals where MeasureName = m.Name)
																else
																	case when abs(m.ScaleBy) = 1 
																	--	then	tqm.PeerRateAndCI
																		then	case when isnumeric(tqm.PeerRateAndCI) = 1
																					then cast( cast(tqm.PeerRateAndCI as decimal(20,2)) as varchar )
																					else tqm.PeerRateAndCI
																				end
																		else	cast( cast( cast(tqm.PeerRateAndCI AS float) * (100 / isnull(m.ScaleBy,1)) as decimal(8,1) ) as varchar) + '%'
																	--	else	cast(tqm.PeerRateAndCI as varchar)
																	end
															end,'-') as MeasureValue
											from			Temp_Quality_Measures tqm
												inner join	Measures m on m.Id = tqm.MeasureID
												inner join	Measures_MeasureTopics mt
																on	mt.Measure_Id = m.Id
																and	mt.UsedForInfographic = 1
												inner join	Topics t on t.Id = mt.Topic_Id
											where			m.ClassType = 'HOSPITAL'
												and			tqm.ReportID in (select ReportID from Temp_Quality)";
        #endregion
        #endregion

        #region Methods.
        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator()
		{ }
		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
		{
			if (base.ValidateDependencies(website, validationResults))
			{
				var rpt = CurrentWebsite.Reports.FirstOrDefault(wr => wr.Report.SourceTemplate.RptId == ActiveReport.SourceTemplate.RptId);
				if (rpt == null)
				{
					validationResults.Add(new ValidationResult(base.ActiveReport.Name + " could not be generated due to the \"Generic Infographics Report for Topics\" not being selected when configuring website."));
				}

				var qualityRG = ServiceLocator.Current.GetInstance<IReportGenerator>("QualityReportGenerator") as QualityReportGenerator;
				if (!CurrentWebsite.Reports.Any(r => qualityRG.ReportIds.Any(id => id.EqualsIgnoreCase(r.Report.SourceTemplate.RptId))))
				{
					validationResults.Add(new ValidationResult("The Quality report is not included in this website.  Skipping Generation."));
				}
			}

			return validationResults == null || validationResults.Count == 0;
		}

        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Something seriously went wrong. The reporting website can not be null.</exception>
        protected override bool LoadReportData()
		{
			try
			{

				// Make sure the base directories are created.
				BaseDataDir = Path.Combine(base.BaseDataDirectoryPath, "Reports");
				if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);

				//FilePath = Path.Combine(BaseDataDir, "GenericInfographicTopics.js");
				FilePath = Path.Combine(BaseDataDir, "GenericInfographic.js");
				if (File.Exists(FilePath)) File.Delete(FilePath);

				if (CurrentWebsite == null)
					throw new InvalidOperationException("Something seriously went wrong. The reporting website can not be null.");

				return true;
			}
			catch (Exception ex)
			{
				Logger.Write(ex.GetBaseException());
				return false;
			}
		}

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
		{
			try
			{

				var topics = GetInfograhicTopics();

				if (topics.Any())
				{
					//GenerateJsonFile(topics, FilePath, "$.monahrq.genericinfographic=");
					GenerateJsonFile(topics, FilePath, "$.monahrq.generic_infographic=");
				}
				return true;
			}
			catch (Exception exc)
			{
				Logger.Write(exc.GetBaseException());
				return false;
			}
		}

        /// <summary>
        /// Gets the infograhic topics.
        /// </summary>
        /// <returns></returns>
        private IList<InfographicTopic> GetInfograhicTopics()
		{
			IList<InfographicTopic> topics = new List<InfographicTopic>();
			NHibernate.IQuery query = null;
			var activeSections = GetActiveSections(ActiveReport, CurrentWebsite.Datasets);

			//	Retrieve CMS Zone data from DB.
			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				var infectionTopicCategoryId = 
					session.Query<TopicCategory>().SingleOrDefault(tc => tc.Name.Contains("Infections")).Id;

				query = session
					.CreateSQLQuery(sqlGetData)
					.SetResultTransformer(Transformers.AliasToBean(typeof(InfographicTopicDTO)));

				var topicDTOs = query.List<InfographicTopicDTO>();

				foreach (var topicDTO in topicDTOs)
				{
					var topic = topics.FirstOrDefault(t => t.TopicCategory == topicDTO.TopicCategory);
					if (topic == null)
					{
						topic = new InfographicTopic()
						{
							RegionName = CurrentWebsite.GeographicDescription,
							SiteName = CurrentWebsite.HeaderTitle,
							SiteUrl = CurrentWebsite.HeaderTitle,
							ActiveSections = activeSections,
							TopicCategory = topicDTO.TopicCategory,
						};
						topics.Add(topic);
					}

					if (topic.Measures.Count >= 2) continue;

					if (topicDTO.TopicCategory == infectionTopicCategoryId)
					{
						var num = topicDTO.MeasureValue.AsNullableDouble();
						if (num != null && num.HasValue)
							topicDTO.MeasureValue = num.Value.ToString("F1");
					}

					topic.Measures.Add(new InfographicMeasure()
					{
						Name = topicDTO.MeasureName,
						Values = new List<String>() { String.Format("{0}", QualityReportPostProcessLogic.FormatMeasureValue(topicDTO.MeasureName, topicDTO.MeasureValue.ToString()) ) },
					//	Values = new List<String>() { String.Format("{0}",topicDTO.MeasureValue.ToString()) }
					});
				}
			}

			return topics;
		}



        /// <summary>
        /// Gets the active sections.
        /// </summary>
        /// <param name="activeReport">The active report.</param>
        /// <param name="datasets">The datasets.</param>
        /// <returns></returns>
        private IList<int> GetActiveSections(Report activeReport, IList<WebsiteDataset> datasets)
		{
			var sections = new List<int>();
			if (activeReport == null || activeReport.Filters == null) return sections;

			var filters = ActiveReport.Filters.Where(f => f.Type == ReportFilterTypeEnum.ActiveSections).Select(x => x.Values).ToList();
			foreach (var filter in filters)
			{
				foreach (var filterValue in filter.Where(x => x.Value))
				{
					if (filterValue.Name.Contains("CMS Measures") && !sections.Contains(2) && datasets.Any(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Hospital Compare Data")))
						sections.Add(2);
					else if (filterValue.Name.Contains("AHRQ QI Measures") && !sections.Contains(1) && datasets.Any(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("AHRQ-QI Provider Data")))
						sections.Add(1);
				}
			}

			return sections.Distinct().OrderByDescending(x => x).ToList();
		}
		#endregion

	}

    #region Internal Classes.
    /// <summary>
    /// Class for InfographicTopicDTO
    /// </summary>
    internal class InfographicTopicDTO
	{
        #region Properties.
        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>
        /// The name of the region.
        /// </value>
        public String RegionName { get; set; }
        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>
        /// The name of the site.
        /// </value>
        public String SiteName { get; set; }
        /// <summary>
        /// Gets or sets the site URL.
        /// </summary>
        /// <value>
        /// The site URL.
        /// </value>
        public String SiteUrl { get; set; }
        /// <summary>
        /// Gets or sets the active sections.
        /// </summary>
        /// <value>
        /// The active sections.
        /// </value>
        public IList<int> ActiveSections { get; set; }
        /// <summary>
        /// Gets or sets the topic category.
        /// </summary>
        /// <value>
        /// The topic category.
        /// </value>
        public int TopicCategory { get; set; }

        /// <summary>
        /// Gets or sets the name of the measure.
        /// </summary>
        /// <value>
        /// The name of the measure.
        /// </value>
        public String MeasureName { get; set; }
        /// <summary>
        /// Gets or sets the measure value.
        /// </summary>
        /// <value>
        /// The measure value.
        /// </value>
        public String MeasureValue { get; set; }
		#endregion
	}

    /// <summary>
    /// Class for InfographicTopic.
    /// </summary>
    [DataContract(Name = "")]
	internal class InfographicTopic
	{
        #region Properties.
        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>
        /// The name of the region.
        /// </value>
        [DataMember(Name = "regionName")]
		public String RegionName { get; set; }

        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>
        /// The name of the site.
        /// </value>
        [DataMember(Name = "siteName")]
		public String SiteName { get; set; }

        /// <summary>
        /// Gets or sets the site URL.
        /// </summary>
        /// <value>
        /// The site URL.
        /// </value>
        [DataMember(Name = "siteUrl")]
		public String SiteUrl { get; set; }

        /// <summary>
        /// Gets or sets the active sections.
        /// </summary>
        /// <value>
        /// The active sections.
        /// </value>
        [DataMember(Name = "activeSections")]
		public IList<int> ActiveSections { get; set; }

        /// <summary>
        /// Gets or sets the topic category.
        /// </summary>
        /// <value>
        /// The topic category.
        /// </value>
        [DataMember(Name = "TopicCategoryID")]
		public int TopicCategory { get; set; }

        /// <summary>
        /// Gets or sets the measures.
        /// </summary>
        /// <value>
        /// The measures.
        /// </value>
        [DataMember(Name = "measures")]
		public IList<InfographicMeasure> Measures { get; set; }
        #endregion

        #region Methods.
        /// <summary>
        /// Initializes a new instance of the <see cref="InfographicTopic"/> class.
        /// </summary>
        public InfographicTopic()
		{
			Measures = new List<InfographicMeasure>();
		}
		#endregion
	}

    /// <summary>
    /// Claa for InfographicMeasure.
    /// </summary>
    [DataContract(Name = "")]
	internal class InfographicMeasure
	{
        #region Properties.
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "name")]
		public String Name { get; set; }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        [DataMember(Name = "values")]
		public IList<String> Values { get; set; }
		#endregion
	}
#endregion
}
