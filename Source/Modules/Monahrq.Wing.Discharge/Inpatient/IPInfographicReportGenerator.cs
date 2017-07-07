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

namespace Monahrq.Wing.Discharge.Inpatient
{
	/// <summary>
	/// Generates the report data/.json files for Inpatient Infographic Discharge reports
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	/// <seealso cref="Monahrq.Sdk.Generators.IReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new string[] { "64B545BF-D183-4F0D-8ABC-6643FF39F5DD" },
                     new string[] { },
                     new[] { typeof(InpatientTarget) },
                     3,
					 "Surgical Patient Safety")] // Old modules "AHRQ -QI Area Data", "Hospital Compare Data"
    public class IPInfographicReportGenerator : BaseReportGenerator, IReportGenerator
    {
		/// <summary>
		/// The query
		/// </summary>
		private const string Query = @"WITH tempTbl (name, stateTotal)
                                    AS
                                    (
                                     SELECT m.name, sum(cast(case [Col1] 
						                            	when '-' then replace ([Col1], '-', 0) 
						                            	when 'c' then replace([Col1], 'c', 0) 
					                                end as decimal)) [Col1]  
		                            FROM Temp_Quality tq
		                            	INNER JOIN Measures m on tq.MeasureID = m.Id
		                            WHERE m.Name in ('PSI 05','PSI 16')
		                            group by m.name
                                    )
                                   
                                    SELECT m.name as Name, CASE m.name
                                         WHEN 'PSI 05' THEN (select case when stateTotal <= m.SuppressionNumerator then -1 else stateTotal end from tempTbl where name ='PSI 05')
                                         WHEN 'PSI 16' THEN (select case when stateTotal <= m.SuppressionNumerator then -1 else stateTotal end from tempTbl where name ='PSI 16')
                                         WHEN 'PSI 06' THEN CAST(tm.PeerRateAndCI AS float)/10
                                         WHEN 'PSI 14' THEN CAST(tm.PeerRateAndCI AS float)/10
                                         WHEN 'PSI 15' THEN CAST(tm.PeerRateAndCI AS float)/10
                                         ELSE tm.PeerRateAndCI END [Values]
                                    FROM Temp_Quality_Measures tm ,Measures m
                                    WHERE tm.measureID = m.id and (m.name in ('SCIP-INF-10', 'SCIP-INF-1', 'SCIP-INF-2', 'SCIP-INF-9', 'SCIP-VTE-2')--Process
                                      OR m.name in ('PSI 05','PSI 16')--Volume
                                      OR m.name in ('PSI 06', 'PSI 14','PSI 15'))--QIProvider
                                    ORDER BY m.Name";

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
                //var dataset = CurrentWebsite.Datasets.FirstOrDefault(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"));
                //if (dataset == null)
                //{
                //    validationResults.Add(new ValidationResult(base.ActiveReport.Name + " could not be generated due to \"Inpatient Discharge\" not selected."));
                //}

                var rpt = CurrentWebsite.Reports.FirstOrDefault(wr => wr.Report.SourceTemplate.RptId == ActiveReport.SourceTemplate.RptId);
                if (rpt == null)
                {
                    validationResults.Add(new ValidationResult(base.ActiveReport.Name + " could not be generated due to the \"Infographics Report - Surgical Safety\" not being selected when configuring website."));
                }
            }

            return validationResults == null || validationResults.Count == 0;
        }

		/// <summary>
		/// Loads the report data.
		/// </summary>
		/// <returns></returns>
		protected override bool LoadReportData()
        {
            return true;
        }

		/// <summary>
		/// Outputs the data files.
		/// </summary>
		/// <returns></returns>
		protected override bool OutputDataFiles()
        {
            try
            {
                var directoryPath = Path.Combine(base.BaseDataDirectoryPath, "Reports");
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                var filePath = Path.Combine(directoryPath, "Infographic.js");

                if (File.Exists(filePath)) File.Delete(filePath);

                var results = RunSqlReturnDataTable(Query, null);

                var builder = new StringBuilder();
                builder.AppendLine("$.monahrq.infographic = {");
                builder.AppendLine(string.Format("regionName: \"{0}\",", CurrentWebsite.GeographicDescription));  //Geographic Desciption
                builder.AppendLine(string.Format("siteName: \"{0}\",", string.IsNullOrEmpty(CurrentWebsite.HeaderTitle) ? string.IsNullOrEmpty(CurrentWebsite.BrowserTitle) ? "Undefined" : CurrentWebsite.BrowserTitle : CurrentWebsite.HeaderTitle)); // Website Header Title 
                builder.AppendLine(string.Format("siteURL: \"{0}\",", CurrentWebsite.HeaderTitle)); //Browser title 
                builder.AppendLine(string.Format("activeSections: [{0}],", GetActiveSections(ActiveReport, CurrentWebsite.Datasets)));  //ActiveReport.Filters  
                builder.AppendLine("measures: [");

                var tempVal = string.Empty;
                var infographics = new List<Infographic>();
                results.Rows.Cast<DataRow>().ForEach(row => infographics.Add(new Infographic { Name = row["Name"].ToString(), Value = row["Values"] }));
                tempVal += string.Join(",", infographics.DistinctBy(x => x.Name)) + ",";
				tempVal += "  { name: \"INF-GRPH-NAT-01\", values: [\"85,000\"] },";
				tempVal += "  { name: \"INF-GRPH-NAT-02\", values: [\"70,000\"] },";
				//	tempVal += "  { name: \"INF-GRPH-NAT-01\", values: [\"400,000\"] },";
				//	tempVal += "  { name: \"INF-GRPH-NAT-02\", values: [\"4 to 8 million\"] },";
				//	tempVal += "  { name: \"INF-GRPH-NAT-03\", values: [\"$4.4 billion\"] },";
				tempVal += "  { name: \"INF-GRPH-NAT-04\", values: [\"4\"] }";
				builder.AppendLine(tempVal);
                builder.AppendLine("]};");

                File.WriteAllText(filePath, builder.ToString());

                return true;
            }
            catch (Exception exc)
            {
                Logger.Write(exc.GetBaseException());
                return false;
            }
        }

		/// <summary>
		/// Gets the active sections.
		/// </summary>
		/// <param name="activeReport">The active report.</param>
		/// <param name="datasets">The datasets.</param>
		/// <returns></returns>
		private string GetActiveSections(Report activeReport, IList<WebsiteDataset> datasets)
        {
            var sections = new List<int>();
            if (activeReport == null || activeReport.Filters == null) return string.Empty;

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

            return string.Join(",", sections.Distinct().OrderByDescending(x => x));
        }

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
        { }
    }

	/// <summary>
	/// 
	/// </summary>
	public class Infographic
    {
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public object Value { get; set; }

		/// <summary>
		/// Gets the converted value.
		/// </summary>
		/// <value>
		/// The converted value.
		/// </value>
		public string ConvertedValue
        {
            get
            {
                var temp = !string.IsNullOrEmpty(Value.ToString()) ? Value.ToString() == "-1" ? "c" : string.Format("{0:0.####}", Convert.ToDouble(Value)) : "0.0";
                return temp;
            }
        }

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
        {
            return "  { " + string.Format("name: \"{0}\", values: [\"{1}\"]", Name, string.IsNullOrEmpty(Value.ToString()) ? "-" : ConvertedValue) + " }";
        }
    }
}
