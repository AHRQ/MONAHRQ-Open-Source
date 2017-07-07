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
using System.Runtime.Serialization;
using NHibernate.Transform;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Services.Generators;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.Wing.NursingHomeCompare.Reports
{
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
	[ReportGenerator(new string[] { "64B545BF-D183-4F0D-8ABC-6643FF39F5DF" },
					 new string[] { },
					 new[] { typeof(NHC.NursingHomeTarget) },
					 3)] // Old modules "AHRQ-QI Area Data", "Hospital Compare Data"
	public class NursingHomeInfographicReportGenerator : BaseReportGenerator, IReportGenerator
	{
		#region Variables.
		private string FilePath { get; set; }
		private string BaseDataDir { get; set; }
		#region SQL Query Variable.
		private const string sqlGetData = @"

			select			m.Name as 'MeasureName'
						,	coalesce(wm.OverrideMeasure_Id,wm.OriginalMeasure_Id) as 'ReportMeasureId'
						,	case m.Name
								when 'NH-HI-01' then 4
			--					when 'NH-HI-04'	then 4
			--					when 'NH-HI-05'	then 4
								when 'NH-QM-01'	then 4
								else 0
							end as 'RatingThreshhold'
			from			Websites_WebsiteMeasures wm
				inner join	Measures m on m.Id = wm.OriginalMeasure_Id	
			where			wm.Website_Id = :websiteId
				and	((		m.Name = 'NH-HI-01' and exists
							(
								select			*
								from			Websites_WebsiteReports wr
									inner join	Reports r on r.Id = wr.Report_Id		
									inner join	Reports_Filters rf on rf.Report_id = r.Id
								where			wr.Website_id = :websiteId
									and			r.ReportType = 'Nursing Home Infographic Report'
									and			rf.Name = 'Health Inspection Section'
									and			rf.Value = 1
							))
			--		or		m.Name = 'NH-HI-04'
			--		or		m.Name = 'NH-HI-05'
					or (	m.Name = 'NH-QM-01' and exists
							(
								select			*
								from			Websites_WebsiteReports wr
									inner join	Reports r on r.Id = wr.Report_Id		
									inner join	Reports_Filters rf on rf.Report_id = r.Id
								where			wr.Website_id = :websiteId
									and			r.ReportType = 'Nursing Home Infographic Report'
									and			rf.Name = 'Quality Section'
									and			rf.Value = 1
							)))";

		private const string sqlGetSafetyData = @"
			with
				DeficiencyAverages(Total,AvgHealthDeficiencies,AvgFireSafetyDeficiences) as
				(
					select			cast(count(*) as decimal) as Total
								,	avg(tht.TotalHealthDeficiencies) as AvgHealthDeficiencies
								,	avg(tht.TotalFireSafetyDeficiencies) as AvgFireSafetyDeficiences
					from			Targets_NursingHomeTargets tht
					where			tht.ProviderId in ({0})
				)
				select			da.AvgHealthDeficiencies as NH_HI_04_PeerRating
							,	da.AvgFireSafetyDeficiences as NH_HI_05_PeerRating
				from			DeficiencyAverages da";
		//		select			sum(	case when (tht.TotalHealthDeficiencies < da.AvgHealthDeficiencies)
		//									then 1
		//									else 0
		//								end
		//							) / da.Total as NH_HI_04_PeerRating
		//					,	sum(	case when tht.TotalFireSafetyDeficiencies < da.AvgFireSafetyDeficiences
		//									then 1
		//									else 0
		//								end
		//							) / da.Total as NH_HI_05_PeerRating
		//		from			Targets_NursingHomeTargets tht
		//			cross join	DeficiencyAverages da
		//		where			tht.ProviderId in ({0})
		//		group by		da.Total";
		#endregion
		#endregion

		#region Methods.
		public override void InitGenerator()
		{ }
		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
		{
			if (base.ValidateDependencies(website, validationResults))
			{
				var rpt = CurrentWebsite.Reports.FirstOrDefault(wr => wr.Report.SourceTemplate.RptId == ActiveReport.SourceTemplate.RptId);
				if (rpt == null)
				{
					validationResults.Add(new ValidationResult(base.ActiveReport.Name + " could not be generated due to the \"Nursing Home Infographics Report\" not being selected when configuring website."));
				}


				var baseNHRG = ServiceLocator.Current.GetInstance<IReportGenerator>("BaseNursingHomeReportGenerator") as BaseNursingHomeReportGenerator;
				if (!CurrentWebsite.Reports.Any(r => baseNHRG.ReportIds.Any(id => id.EqualsIgnoreCase(r.Report.SourceTemplate.RptId))))
				{
					validationResults.Add(new ValidationResult("The Nursing Home Comparison report is not included in this website.  Skipping Generation."));
				}
			}

			return validationResults == null || validationResults.Count == 0;
		}

		protected override bool LoadReportData()
		{
			try
			{

				// Make sure the base directories are created.
				BaseDataDir = Path.Combine(base.BaseDataDirectoryPath, "Reports");
				if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);

				FilePath = Path.Combine(BaseDataDir, "NursingHomeInfographic.js");
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

		protected override bool OutputDataFiles()
		{
			try
			{

				var data = GetNHInfograhicData();

				if (data != null && data.Measures != null && data.Measures.Any())
				{
					GenerateJsonFile(data, FilePath, "$.monahrq.nursinghome_infographic=");
				}
				return true;
			}
			catch (Exception exc)
			{
				Logger.Write(exc.GetBaseException());
				return false;
			}
		}

		private NHInfographicData GetNHInfograhicData()
		{

			var baseNHRG = ServiceLocator.Current.GetInstance<IReportGenerator>("BaseNursingHomeReportGenerator") as BaseNursingHomeReportGenerator;
			var infographicData = new NHInfographicData();
			var activeSections = GetActiveSections(ActiveReport, CurrentWebsite.Datasets);
			NHibernate.IQuery query = null;

			//	Validate.
			if (baseNHRG == null ||
				baseNHRG.TempNursingHomeReportMeasuresByMeasure == null ||
				baseNHRG.TempNursingHomeReportMeasuresByMeasure.Count == 0)
			{
				var message = "No data available from Base Nursing Home Report Generation.  Skipping NursingHomeInfographic Report Generation.";

				LogMessage(message);
				return null;
			}

			//	Populate generic data.
			infographicData.RegionName = CurrentWebsite.GeographicDescription;
			infographicData.SiteName = CurrentWebsite.HeaderTitle ?? String.Empty;
			infographicData.SiteUrl = CurrentWebsite.HeaderTitle ?? String.Empty;
			infographicData.ActiveSections = activeSections;

			//	Populate Overall Infographic measure data.
			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				query = session
					.CreateSQLQuery(sqlGetData)
					//	.SetResultTransformer(Transformers.AliasToBean(typeof(InfographicTopicDTO)))
					.AddScalar("MeasureName", NHibernate.NHibernateUtil.String)
					.AddScalar("ReportMeasureId", NHibernate.NHibernateUtil.Int32)
					.AddScalar("RatingThreshhold", NHibernate.NHibernateUtil.Double)
					.SetParameter("websiteId", CurrentWebsite.Id);
				var nhInfographicReportDetailSet = query
					.List<Object[]>()
					.Select(row => new
					{
						MeasureName = (String)row[0],
						ReportMeasureId = (int)row[1],
						RatingThreshhold = (double)row[2],
					})
					.ToList();

				//	Build JSON object.
				foreach (var nhInfographicReportDetail in nhInfographicReportDetailSet)
				{
					var numBelowAvg = 0;
					var curMeasureSet = baseNHRG.TempNursingHomeReportMeasuresByMeasure[(int)nhInfographicReportDetail.ReportMeasureId];
					foreach (var nhMeasure in curMeasureSet)
					{
						var rating = MathExtensions.ParseNullableDouble(nhMeasure.PeerRating);
						if (rating.HasValue && rating < (int)nhInfographicReportDetail.RatingThreshhold)
							++numBelowAvg;
					}

					infographicData.Measures.Add(new NHInfographicMeasure
						{
							Name = nhInfographicReportDetail.MeasureName,
							Values = new List<String>() { string.Format("{0} out of {1}", numBelowAvg, curMeasureSet.Count.AsDouble() ) },
						//	Values = new List<String>() { (numBelowAvg / curMeasureSet.Count.AsDouble() * 100.0).ToString("F0") },
					});
				}
			}
			//	Populate deficiency Infographic measure data.
			using (var session = DataProvider.SessionFactory.OpenSession())
			{
				var providerIds = CurrentWebsite.NursingHomes
					.Where(nh => nh.NursingHome.ProviderId != null)
					.Select(nh => String.Format("'{0}'", nh.NursingHome.ProviderId));

				var sqlGetSafetyDatax = String.Format(sqlGetSafetyData, string.Join(",",providerIds));
				query = session
					.CreateSQLQuery(sqlGetSafetyDatax)
					.AddScalar("NH_HI_04_PeerRating", NHibernate.NHibernateUtil.Double)
					.AddScalar("NH_HI_05_PeerRating", NHibernate.NHibernateUtil.Double);

				var nhDeficiencyData = query
					.List<Object[]>()
					.Select(row => new
					{
						NH_HI_04_PeerRating = row[0] != null ? (double?)row[0] : null,
						NH_HI_05_PeerRating = row[1] != null ? (double?)row[1] : null,
					})
					.ToList();			

				infographicData.Measures.Add(new NHInfographicMeasure
				{
					Name = "NH-HI-04",
					Values = nhDeficiencyData[0].NH_HI_04_PeerRating != null? 
                             new List<String>() { ((double)nhDeficiencyData[0].NH_HI_04_PeerRating).ToString("F0") }:
                             new List<String>(new String[] { "-" }),
				});
				infographicData.Measures.Add(new NHInfographicMeasure
				{
					Name = "NH-HI-05",
					Values = nhDeficiencyData[0].NH_HI_05_PeerRating != null? 
                             new List<String>() { ((double)nhDeficiencyData[0].NH_HI_05_PeerRating).ToString("F0") }:
                             new List<String>(new String[] { "-" }),
				});
			}

			return infographicData;
		}



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
	internal class InfographicTopicDTO
	{
		#region Properties.
		public String RegionName { get; set; }
		public String SiteName { get; set; }
		public String SiteUrl { get; set; }
		public IList<int> ActiveSections { get; set; }
		public int TopicCategory { get; set; }

		public String MeasureName { get; set; }
		public Double MeasureValue { get; set; }
		#endregion
	}

	[DataContract(Name = "")]
	internal class NHInfographicData
	{
		#region Properties.
		[DataMember(Name = "regionName")]
		public String RegionName { get; set; }

		[DataMember(Name = "siteName")]
		public String SiteName { get; set; }

		[DataMember(Name = "siteUrl")]
		public String SiteUrl { get; set; }

		[DataMember(Name = "activeSections")]
		public IList<int> ActiveSections { get; set; }


		[DataMember(Name = "measures")]
		public IList<NHInfographicMeasure> Measures { get; set; }
		#endregion

		#region Methods.
		public NHInfographicData()
		{
			Measures = new List<NHInfographicMeasure>();
		}
		#endregion
	}

	[DataContract(Name = "")]
	internal class NHInfographicMeasure
	{
		#region Properties.
		[DataMember(Name = "name")]
		public String Name { get; set; }

		[DataMember(Name = "values")]
		public IList<String> Values { get; set; }
		#endregion
	}
#endregion
}
