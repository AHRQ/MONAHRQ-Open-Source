using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Wing.Ahrq.Area;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Sdk.Services.Generators;
using System.Data.SqlClient;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    /// Cost Quality Report Generator class
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    [Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new string[] { "7D841284-5179-44E5-A00E-BDD042B0A7BD" },
                        new string[] { },
                        new[] { typeof(AreaTarget) },
                        3)]
    public class CostQualityReportGenerator : BaseReportGenerator
    {
        #region Fields and Constants

        /// <summary>
        /// The cost quality topic dir
        /// </summary>
        private string _costQualityTopicDir;
        /// <summary>
        /// The cost quality data dir
        /// </summary>
        private string _costQualityDataDir;
        /// <summary>
        /// The cost quality hospital data dir
        /// </summary>
        private string _costQualityHospitalDataDir;
        /// <summary>
        /// The json topic domain
        /// </summary>
        private const string JSON_TOPIC_DOMAIN = "$.monahrq.costqualityTopics = ";
        /// <summary>
        /// The json hospital domain
        /// </summary>
        private const string JSON_HOSPITAL_DOMAIN = "$.monahrq.costdata.hospital_";
        /// <summary>
        /// The json measure domain
        /// </summary>
        private const string JSON_MEASURE_DOMAIN = "$.monahrq.costdata.measuredescription_";
        /// <summary>
        /// The excluded icd codes
        /// </summary>
        private static readonly List<string> _excludedICDCodes = new List<string>()
        {
            "'82019'","'82001'","'82020'","'82002'","'82021'","'82003'","'82009'","'82022'",
            "'82009'","'82030'","'82010'","'82031'","'82011'","'82032'","'82012'","'8208'",
            "'82013'","'8209 '","'71500'","'71595'","'71509'","'71598'","'71510'","'71650'",
            "'71515'","'71655'","'71518'","'71658'","'71520'","'71659'","'71525'","'71660'",
            "'71528'","'71665'","'71530'","'71668'","'71535'","'71690'","'71538'","'71695'",
            "'71580'","'71698'","'71589'","'71699'","'71590'"
        };

        /// <summary>
        /// The cost quality report name
        /// </summary>
        public static string CostQualityReportName = "Cost and Quality Report – Side By Side Comparison Report";
        //"Cost and Quality Report – Side By Side Comparison Report"
        /// <summary>
        /// Gets or sets the cost quality calculator.
        /// </summary>
        /// <value>
        /// The cost quality calculator.
        /// </value>
		public CostQualityCalculator CostQualityCalculator { get; set; }
		#endregion

		#region Imports

        /// <summary>
        /// Gets or sets the data service provider.
        /// </summary>
        /// <value>
        /// The data service provider.
        /// </value>
		[Import]
        public IDomainSessionFactoryProvider DataServiceProvider { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        protected override bool LoadReportData()
        {
            return true;
        }

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
        {
            try
            {
                _costQualityDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base", "CostQualityMeasures");
                CreateDir(_costQualityDataDir);

                _costQualityHospitalDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "CostQualityRatings", "Hospital");
                CreateDir(_costQualityHospitalDataDir);


				//	Calculate Data in WinQI.
				try
				{
					if (TestWinQISettings())
					{
						CostQualityCalculator =
								new CostQualityCalculator(
									ConfigurationService.WinQiConnectionSettings.ConnectionString,
									ConfigurationService.ConnectionSettings.ConnectionString,
									CurrentWebsite);
						CostQualityCalculator.Calculate();

						LogMessage("Finished cost quality calculations");
					}
				}
				catch (Exception ex)
				{
					Logger.Write(ex);

					LogMessage(
						"A error occurred while preforming cost calculation. Skipping the rest of cost quality calculation.",
						PubishMessageTypeEnum.Error);
					return false;
				}

				//	Determine which measures to report on.
				//	- Currently we have to family of measures where each family contains 3 Measures each:
				//		- IQI 12, IQI 12_QNTY, IQI 12_COST &
				//		- IQI 14, IQI 14_QNTY, IQI 14_COST
				//	- If the ENTIRE family isn't selected then none in the family should be reported on.
				var cqrMeasures = CurrentWebsite.Measures
					.Where(wm =>
					{
						return
							wm.IsSelected &&
							wm.ReportMeasure.Name.EqualsAny(
								"IQI 12", "IQI 12_QNTY", "IQI 12_COST",
								"IQI 14", "IQI 14_QNTY", "IQI 14_COST");
					}).Select(wm => wm.ReportMeasure).ToList();
				var cqrMeasureGroups = cqrMeasures.GroupBy(m => m.Name.Substring(0,6));
				cqrMeasureGroups.ForEach(group =>
				{
					if (group.Count() != 3)
						group.ForEach(gm => cqrMeasures.Remove(gm));
				});

				//	Get data.
				var costQualityMeasureDescriptions = CostQualityMeasureDescription.GetCostQualityMeasureDescriptions(this, CurrentWebsite.HasAllAudiences, cqrMeasures);
                var hospitalIds = CurrentWebsite.Hospitals.Select(x => x.Hospital.Id).ToList();
                var costQualityhospitals = CostQualityHospital.GetCostQualityHospitals(this, string.Join(",", hospitalIds), string.Join(",", costQualityMeasureDescriptions.Select(m => m.MeasureId)));
				var costqualityTopics = new List<CostQualityTopic>();
				CostQualityTopic.FindAddCostQualityTopic(costqualityTopics,DataServiceProvider, "Health Care Cost and Quality", "Hip replacement surgery", cqrMeasures);
				CostQualityTopic.FindAddCostQualityTopic(costqualityTopics,DataServiceProvider, "Health Care Cost and Quality", "Heart Surgeries and Procedures", cqrMeasures);

                CostQualityTopic.FindAddCostQualityTopic(costqualityTopics, DataServiceProvider, "Hip or knee replacement surgery", "Results of care", cqrMeasures);
                CostQualityTopic.FindAddCostQualityTopic(costqualityTopics, DataServiceProvider, "Heart surgeries and procedures", "Results of Care", cqrMeasures); //"Results of Care – Deaths


                var ipDatasets = CurrentWebsite.Datasets.Where(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")).ToList();

                WebsiteDataset datasetToUse;
                if (ipDatasets.Count == 1)
                {
                    datasetToUse = ipDatasets.First();
                }
                else
                {
                    datasetToUse = ipDatasets.FirstOrDefault(d => d.Dataset.ReportingYear == CurrentWebsite.ReportedYear) ??
                                   ipDatasets.First();
                }

                var values = GetCostAndQuantityValues(datasetToUse, CurrentWebsite.Id);

                GenerateJsonFile(costqualityTopics.OrderBy(x => x.Name), Path.Combine(_costQualityDataDir, "CostQualityTopic.js"), JSON_TOPIC_DOMAIN);
				costQualityhospitals.GroupBy(cqh => cqh.HospitalId).ForEach(cqhs =>
			//	costQualityhospitals.ForEach(h =>
                {
                    var hospitalDetail = CostQualityHospital.GetCostQualityHospitalDetail(values, cqhs.ToList()[0],
                        CurrentWebsite.Measures.Where(m =>
                            (m.OriginalMeasure != null && m.OriginalMeasure.Source == "Calculated") ||
                            (m.OverrideMeasure != null && m.OverrideMeasure.Source == "Calculated")).ToList());

                    hospitalDetail.AddRange(cqhs.ToList());
                    GenerateJsonFile(hospitalDetail, Path.Combine(_costQualityHospitalDataDir, String.Format("Hospital_{0}.js", cqhs.Key)), JSON_HOSPITAL_DOMAIN + cqhs.Key + "=");
                });

                costQualityMeasureDescriptions.ForEach(measureDesc => GenerateJsonFile(new List<CostQualityMeasureDescription>() { measureDesc }, Path.Combine(_costQualityDataDir, String.Format("Measure_{0}.js", measureDesc.MeasureId)), JSON_MEASURE_DOMAIN + measureDesc.MeasureId + "="));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return false;
            }

            return true;
        }

		#region WinQI Test Methods.
        /// <summary>
        /// Tests the win qi settings.
        /// </summary>
        /// <returns></returns>
		private bool TestWinQISettings()
		{
			if (ConfigurationService.WinQiConnectionSettings == null ||
				string.IsNullOrEmpty(ConfigurationService.WinQiConnectionSettings.ConnectionString))
			{
				const string message =
					"There was an error while trying to perform cost quality calculation. Please ensure that you have saved the connection string to \n" +
					"the cost Quality Indicators database. Skipping the rest of cost quality calculation.";

				LogMessage(
					message,
					PubishMessageTypeEnum.Error);
				return false;
			}
			else
			{
				if (!TestWinQIConnection(ConfigurationService.WinQiConnectionSettings.ConnectionString))
				{
					const string message =
						"There was an error while trying to perform cost calculation. Please ensure that you have saved the connection string to \n" +
						"the cost Quality Indicators database, the Quality indicators database exists or you do not have permissions to access the database. Skipping the rest of cost quality calculation.";

					LogMessage(
						message,
						PubishMessageTypeEnum.Error);
					return false;
				}
			}
			return true;
		}
        /// <summary>
        /// Tests the win qi connection.
        /// </summary>
        /// <param name="winQiConnectionStr">The win qi connection string.</param>
        /// <returns></returns>
		private static bool TestWinQIConnection(string winQiConnectionStr)
		{
			try
			{
				using (var winQiConnection = new SqlConnection(winQiConnectionStr))
				{
					winQiConnection.Open();
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
		#endregion

        /// <summary>
        /// Gets the cost and quantity values.
        /// </summary>
        /// <param name="dataset">The dataset.</param>
        /// <param name="websiteId">The website identifier.</param>
        /// <returns></returns>
		private List<Tuple<int, int, double>> GetCostAndQuantityValues(WebsiteDataset dataset, int websiteId)
		{
			var query = string.Format(@"SELECT h.Id as HospitalId, COUNT(h.id) AS TotalCount, AVG(TotalCharge * CAST(ISNULL(wh.ccr, dbo.fnGetCostToChargeRatio('{2}',h.CmsProviderID)) as float)) AS AvgCost
                                        FROM Websites_WebsiteHospitals wh
                                        	INNER JOIN Hospitals h on wh.Hospital_Id = h.Id
                                        	INNER JOIN Targets_InpatientTargets ip on h.LocalHospitalId = ip.LocalHospitalID
                                        	INNER JOIN [Base_ICD9toPRCCSCrosswalks] bicd on bicd.ICD9ID = ip.PrincipalProcedure
                                        	INNER JOIN Base_CostToCharges ccr on ccr.ProviderID = h.CmsProviderID 
                                        WHERE PRCCSID = 153 and ip.PrincipalDiagnosis NOT IN ({0}) 
                                        AND Dataset_Id = {1} and wh.Website_Id = {3}
                                        AND 1 = (CASE  WHEN wh.CCR IS NULL AND ccr.Ratio IS NULL THEN 0 ELSE 1 END)
                                        GROUP BY h.id", string.Join(",", _excludedICDCodes), dataset.Dataset.Id, dataset.Dataset.ReportingYear, websiteId);

            List<Tuple<int, int, double>> result;
            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                result = session.CreateSQLQuery(query)
                    .AddScalar("HospitalId", NHibernateUtil.Int32)
                    .AddScalar("TotalCount", NHibernateUtil.Int32)
                    .AddScalar("AvgCost", NHibernateUtil.Double)
                    .List<object[]>().Select(r => Tuple.Create((int)r[0], (int)r[1], (double)r[2]))
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator() { }

        /// <summary>
        /// Validates the report(S) dependencies needed to generate the report.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns></returns>
        public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            var isvalid = true;
            if (!website.Measures.Any(m => (m.OriginalMeasure != null && m.OriginalMeasure.SupportsCost)
                                            || (m.OverrideMeasure != null && m.OverrideMeasure.SupportsCost)))
            {
                validationResults.Add(new ValidationResult("AHRQ QI Provider file is not imported. The “Cost and Quality Report – Side By Side Comparison” report will not be generated."));
                isvalid = false;
            }

            //if (website.Reports.All(r => r.Report.Name != CostQualityReportName))
            //{
            //    validationResults.Add(new ValidationResult(" The “Cost and Quality Report – Side By Side Comparison” report is not selected. The “Cost and Quality Report – Side By Side Comparison” will not be generated."));
            //    isvalid = false;
            //}

            if (website.Datasets.All(d => !d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")))
            {
                validationResults.Add(new ValidationResult("Inpatient Hospital Discharge (IP) file is not imported. The “Cost and Quality Report – Side By Side Comparison” report will not be generated."));
                isvalid = false;
            }

            return isvalid && base.ValidateDependencies(website, validationResults);
        }



        #endregion

        /// <summary>
        /// Cost Quality Topic class
        /// </summary>
        [Serializable]
        private struct CostQualityTopic
        {
            public int Id;
            public string Name;
            public IList<CostQualityMeasure> Measures;

			public CostQualityTopic(
				int id,
				string name,
				IList<CostQualityMeasure> measures)
			{
				Id = id;
				Name = name;
				Measures = measures;
			}

            /// <summary>
            /// Finds the add cost quality topic.
            /// </summary>
            /// <param name="list">The list.</param>
            /// <param name="dataserviceProvider">The dataservice provider.</param>
            /// <param name="topicCategoryName">Name of the topic category.</param>
            /// <param name="topicName">Name of the topic.</param>
            /// <param name="selectedMeasures">The selected measures.</param>
            public static void FindAddCostQualityTopic(
				List<CostQualityTopic> list,
				IDomainSessionFactoryProvider dataserviceProvider,
				string topicCategoryName,
				string topicName,
				IList<Measure> selectedMeasures)
            {

                using (var session = dataserviceProvider.SessionFactory.OpenSession())
                {
					var measureTopics =
						session.Query<MeasureTopic>()
							.Where(mt => mt.Topic.Name == topicName && mt.Topic.Owner.Name == topicCategoryName)
							.ToList()
							.Where(mt => selectedMeasures.Select(sm => sm.Id).Contains(mt.Measure.Id))
							.ToList();

					if (measureTopics == null || measureTopics.Count == 0)
						return;
					
					var id = measureTopics[0].Topic != null ? measureTopics[0].Topic.Id : 0;
					var name = measureTopics[0].Topic != null ? measureTopics[0].Topic.Name : String.Empty;
					var measures = new List<CostQualityMeasure>();
					measureTopics.ForEach(mt => measures.Add(new CostQualityMeasure(mt.Measure)) );

					list.Add(new CostQualityTopic(id, name, measures));

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public struct CostQualityMeasure
        {
            public string Id;
            public int MeasureId;
            public string Name;
            public string DisplayGroup;

            public CostQualityMeasure(Measure measure)
            {
                Id = measure.MeasureCode;
                MeasureId = measure.Id;
                Name = measure.ConsumerPlainTitle;
                DisplayGroup = measure.MeasureCode.Contains("QNTY") ? "Quantity" : measure.MeasureCode.Contains("COST") ? "Cost" : "Quality";
            }
        }

        /// <summary>
        /// Cost Quality Measure class.
        /// </summary>
        [Serializable]
        public struct CostQualityMeasureDescription
        {
            public int MeasureId;
            public string MeasuresName;
            public string MeasureSource;
            public string SelectedTitle;
            public string PlainTitle;
            public string ClinicalTitle;
            public string MeasureDescription;
            public string MoreInformation;
            public string URL;
            public string URLTitle;
            public string DataSourceURL;
            public string DataSourceURLTitle;
            public string SelectedTitleConsumer;
            public string PlainTitleConsumer;
            public string MeasureDescriptionConsumer;
            public string Heading;
			public string PeerRates;
			static private CostQualityReportGenerator CostQualityReportGenerator;
			static private ISession session;



            /// <summary>
            /// Gets the cost quality measure descriptions.
            /// </summary>
            /// <param name="cqReportGen">The cq report gen.</param>
            /// <param name="hasAllAudiences">if set to <c>true</c> [has all audiences].</param>
            /// <param name="selectedMeasures">The selected measures.</param>
            /// <returns></returns>
            public static IEnumerable<CostQualityMeasureDescription> GetCostQualityMeasureDescriptions(CostQualityReportGenerator cqReportGen, bool hasAllAudiences, IList<Measure> selectedMeasures)
            {
				CostQualityReportGenerator = cqReportGen;
				using (session = CostQualityReportGenerator.DataServiceProvider.SessionFactory.OpenSession())
                {
					var measures = 
						session.Query<Measure>()
							.Where(m => (m.SupportsCost || m.MeasureType == "CostQuality"))
							.DistinctBy(m => m.MeasureCode)
					//		.Select(x => GeneratetCostQualityMeasureDescription(x, hasAllAudiences))
							.ToList();
					return measures
							.Where(m =>
							{
								return
									selectedMeasures.Select(sm => sm.Id).Contains(m.Id) &&
									IsCostQualityMeasureDataAvailable(m);
							})
							.Select(m => GeneratetCostQualityMeasureDescription(m, hasAllAudiences))
							.ToList();
				}
			}
            /// <summary>
            /// Determines whether [is cost quality measure data available] [the specified measure].
            /// </summary>
            /// <param name="measure">The measure.</param>
            /// <returns>
            ///   <c>true</c> if [is cost quality measure data available] [the specified measure]; otherwise, <c>false</c>.
            /// </returns>
			public static bool IsCostQualityMeasureDataAvailable(Measure measure)
			{
				if (measure.MeasureCode.EqualsAnyIgnoreCase("IQI 12_QNTY", "IQI 12_COST", "IQI 14_QNTY", "IQI 14_COST"))
				{
					if (CostQualityReportGenerator.CostQualityCalculator == null ||
						CostQualityReportGenerator.CostQualityCalculator.CQCalculations == null) return false;

					var cqType =
						measure.MeasureCode.EqualsAnyIgnoreCase("IQI 12_QNTY", "IQI 12_COST")
							? CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_12
							: CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_14;

					return 
						CostQualityReportGenerator.CostQualityCalculator.CQCalculations.ContainsKey(cqType) &&
						CostQualityReportGenerator.CostQualityCalculator.CQCalculations[cqType].ContainsKey(0);
				}
				return true;
			}
            /// <summary>
            /// Generatets the cost quality measure description.
            /// </summary>
            /// <param name="measure">The measure.</param>
            /// <param name="hasAllAudiences">if set to <c>true</c> [has all audiences].</param>
            /// <returns></returns>
			public static CostQualityMeasureDescription GeneratetCostQualityMeasureDescription(Measure measure, bool hasAllAudiences = true)
			{


				var cqmd = new CostQualityMeasureDescription()
				{
					MeasureId = measure.Id,
					MeasuresName = measure.MeasureCode,
					MeasureSource = measure.Source,
					SelectedTitle = measure.MeasureTitle.Selected == SelectedMeasuretitleEnum.Plain ? measure.MeasureTitle.Plain : measure.MeasureTitle.Clinical,
					PlainTitle = measure.MeasureTitle.Plain,
					ClinicalTitle = measure.MeasureTitle.Clinical,
					MeasureDescription = measure.Description,
					MoreInformation = measure.MoreInformation,
					URL = measure.Url,
					URLTitle = measure.UrlTitle,
					DataSourceURL = "http://www.qualityindicators.ahrq.gov",
					DataSourceURLTitle = "AHRQ Quality Indicator",
					SelectedTitleConsumer = measure.ConsumerPlainTitle,
					PlainTitleConsumer = measure.ConsumerPlainTitle,
					MeasureDescriptionConsumer = measure.ConsumerDescription,
					Heading = GetCqdHeading(measure),
					PeerRates = GetCqdPeerRate(measure)
				};

				return cqmd;
			}



            /// <summary>
            /// Gets the CQD heading.
            /// </summary>
            /// <param name="measure">The measure.</param>
            /// <returns></returns>
			private static string GetCqdHeading(Measure measure)
			{
				return 
					measure.MeasureCode.EqualsAnyIgnoreCase("IQI 12_COST", "IQI 14_COST")
						? "Average Surgery Cost"
						: measure.MeasureCode.EqualsAnyIgnoreCase("IQI 12_QNTY", "IQI 14_QNTY")
							? "Number of Surgeries"
							: "Quality Rating";
			}
            /// <summary>
            /// Gets the CQD peer rate.
            /// </summary>
            /// <param name="measure">The measure.</param>
            /// <returns></returns>
			private static String GetCqdPeerRate(Measure measure)
			{
				if (measure.MeasureCode.EqualsAny("IQI 12", "IQI 14"))
				{
					var peerRateSql = String.Format(@"
						select			tqm.PeerRateAndCI
								--	,	case when (isnumeric(tqm.PeerRateAndCI) = 0)
								--			then	'0'
								--			else	tqm.PeerRateAndCI
								--		end as PeerRateAndCI
						from			Temp_Quality_Measures tqm
						where			tqm.ReportId = (select top 1 tq.ReportId from Temp_Quality tq)
							and			tqm.MeasureName = '{0}'"
						, measure.MeasureCode);

					return 
						String.Format(
							"({0})",
							session
							.CreateSQLQuery(peerRateSql)
							.AddScalar("PeerRateAndCI", NHibernateUtil.String)
							.List<string>()
						//	.Select(x => Convert.ToDouble(x))
							.FirstOrDefault());

			//		var peerRatings = session.Query<TempQuality>().Where(tq => tq.MeasureId == measure.Id).Select(tqx => tqx.PeerRating).ToList();
			//		return String.Join(",", peerRatings);
				}
				else if (measure.MeasureCode.EqualsIgnoreCase("IQI 12_QNTY")) { return CostQualityReportGenerator.CostQualityCalculator.CQCalculations[CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_12][0].MeasureQuantityValue.ToString(); }
				else if (measure.MeasureCode.EqualsIgnoreCase("IQI 12_COST")) { return CostQualityReportGenerator.CostQualityCalculator.CQCalculations[CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_12][0].MeasureAverageCostValue.ToString(); }
				else if (measure.MeasureCode.EqualsIgnoreCase("IQI 14_QNTY")) { return CostQualityReportGenerator.CostQualityCalculator.CQCalculations[CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_14][0].MeasureQuantityValue.ToString();  }
				else if (measure.MeasureCode.EqualsIgnoreCase("IQI 14_COST")) { return CostQualityReportGenerator.CostQualityCalculator.CQCalculations[CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_14][0].MeasureAverageCostValue.ToString(); }

				return null;
			}

		}

        /// <summary>
        /// Cost Quality Hospital class.
        /// </summary>
        [Serializable]
        public struct CostQualityHospital
        {
            public int? MeasureId { get; set; }
            public int HospitalId { get; set; }
            public int CountyId { get; set; }
            public int RegionId { get; set; }
            public string ZipCode { get; set; }
            public string HospitalType { get; set; }
            public string Value { get; set; }
            public string NatRating { get; set; }
            public string PeerRating { get; set; }
			public string Rate { get; set; }
			static private CostQualityReportGenerator CostQualityReportGenerator;

            /// <summary>
            /// Initializes a new instance of the <see cref="CostQualityHospital"/> struct.
            /// </summary>
            /// <param name="tq">The tq.</param>
            /// <param name="m">The m.</param>
			public CostQualityHospital(TempQuality tq,Measure m) : this()
			{

				MeasureId = tq.MeasureId;
				HospitalId = tq.HospitalId.GetValueOrDefault();
				CountyId = tq.CountyId.GetValueOrDefault();
				RegionId = tq.RegionId.GetValueOrDefault();
				ZipCode = tq.ZipCode;
				HospitalType = tq.HospitalType;
				Value = "";
				NatRating = tq.NatRating;
				PeerRating = tq.PeerRating;
				Rate = GetRate(tq, m, HospitalId);
			}

            /// <summary>
            /// Gets the cost quality hospital detail.
            /// </summary>
            /// <param name="values">The values.</param>
            /// <param name="hospital">The hospital.</param>
            /// <param name="websiteMeasures">The website measures.</param>
            /// <returns></returns>
			public static List<CostQualityHospital> GetCostQualityHospitalDetail(List<Tuple<int, int, double>> values, CostQualityHospital hospital, List<WebsiteMeasure> websiteMeasures)
            {
                if (values == null) return Enumerable.Empty<CostQualityHospital>().ToList();

                return websiteMeasures.Select(measure => new CostQualityHospital
                {
                    MeasureId = measure.OriginalMeasure != null ? measure.OriginalMeasure.Id : measure.OverrideMeasure != null ? measure.OverrideMeasure.Id : 0,
                    HospitalId = hospital.HospitalId,
                    CountyId = hospital.CountyId,
                    RegionId = hospital.RegionId,
                    ZipCode = hospital.ZipCode,
                    HospitalType = hospital.HospitalType,
				//	Value = GetValue(values.FirstOrDefault(x => x.Item1 == hospital.HospitalId), measure),
					Value = GetRate(null, measure.ReportMeasure, hospital.HospitalId),
					Rate = GetRate(null, measure.ReportMeasure, hospital.HospitalId),
                }).ToList();
            }

            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <param name="values">The values.</param>
            /// <param name="measure">The measure.</param>
            /// <returns></returns>
            private static string GetValue(Tuple<int, int, double> values, WebsiteMeasure measure)
            {
                var code =
                    measure.OriginalMeasure != null
                        ? measure.OriginalMeasure.MeasureCode
                        : measure.OverrideMeasure != null ? measure.OverrideMeasure.MeasureCode : string.Empty;

                if (string.IsNullOrEmpty(code) || values == null) return string.Empty;

                return code.Contains("COST") ? string.Format("{0}", values.Item3) : values.Item2.ToString(CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Gets the cost quality hospitals.
            /// </summary>
            /// <param name="cqReportGen">The cq report gen.</param>
            /// <param name="hospitalIds">The hospital ids.</param>
            /// <param name="measureids">The measureids.</param>
            /// <returns></returns>
            public static IEnumerable<CostQualityHospital> GetCostQualityHospitals(CostQualityReportGenerator cqReportGen, string hospitalIds, string measureids)
            {
				CostQualityReportGenerator = cqReportGen;

				using (var session = CostQualityReportGenerator.DataServiceProvider.SessionFactory.OpenSession())
                {
					//	var query = String.Format("SELECT MeasureId, HospitalId, CountyId, RegionId, ZipCode, HospitalType, '' as Value, NatRating, PeerRating FROM Temp_Quality WHERE HospitalId in ({0}) and MeasureId in ({1})", hospitalIds, measureids);
					//	var result = session.CreateSQLQuery(query)
					//	    .AddScalar("MeasureId", NHibernateUtil.Int32)
					//	    .AddScalar("HospitalId", NHibernateUtil.Int32)
					//	    .AddScalar("CountyId", NHibernateUtil.Int32)
					//	    .AddScalar("RegionId", NHibernateUtil.Int32)
					//	    .AddScalar("ZipCode", NHibernateUtil.String)
					//	    .AddScalar("HospitalType", NHibernateUtil.String)
					//	    .AddScalar("Value", NHibernateUtil.String)
					//	    .AddScalar("NatRating", NHibernateUtil.String)
					//	    .AddScalar("PeerRating", NHibernateUtil.String)
					//	    .SetResultTransformer(new AliasToBeanResultTransformer(typeof(CostQualityHospital)))
					//	    .List<CostQualityHospital>();


					var result =
						session.Query<TempQuality>()
							.Where(tq =>
								hospitalIds.Split(',').ToList().Contains(tq.HospitalId.Value.ToString()) &&
								measureids.Split(',').ToList().Contains(tq.MeasureId.ToString()))
							.Join(
								session.Query<Measure>(),
								tq => tq.MeasureId,
								m => m.Id,
								(tq, m) =>
									new CostQualityHospital(tq,m))
							//		{
							//			MeasureId = tq.MeasureId,
							//			HospitalId = tq.HospitalId.GetValueOrDefault(),
							//			CountyId = tq.CountyId.GetValueOrDefault(),
							//			RegionId = tq.RegionId.GetValueOrDefault(),
							//			ZipCode = tq.ZipCode,
							//			HospitalType = tq.HospitalType,
							//			Value = "",
							//			NatRating = tq.NatRating,
							//			PeerRating = tq.PeerRating,
							//			Rate = (m.MeasureCode.IsContainedIn("IQI 12", "IQI 14")) ? tq.RateAndCI : ""
							//		})
							.ToList();

					return result;
                }
            }

            /// <summary>
            /// Gets the rate.
            /// </summary>
            /// <param name="tq">The tq.</param>
            /// <param name="m">The m.</param>
            /// <param name="hospitalId">The hospital identifier.</param>
            /// <returns></returns>
			private static string GetRate(TempQuality tq, Measure m, int hospitalId)
			{
				if (m.MeasureCode.EqualsAny("IQI 12", "IQI 14"))
				{
					return String.Format("({0})", tq.Col3);		// Col3 = ObservedRate for QI Provider Measures.
				}

				var rate = "";
				var cqCalculation =
					(m.MeasureCode.EqualsAnyIgnoreCase("IQI 12_QNTY", "IQI 12_COST")) ? CostQualityReportGenerator.CostQualityCalculator.CQCalculations[CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_12] :
					(m.MeasureCode.EqualsAnyIgnoreCase("IQI 14_QNTY", "IQI 14_COST")) ? CostQualityReportGenerator.CostQualityCalculator.CQCalculations[CostQualityCalculator.CostQualityCalculatorMeasureType.IQI_14] :
					null;

				if (cqCalculation != null && cqCalculation.ContainsKey(hospitalId) && cqCalculation[hospitalId] != null)
				{
					rate =
						m.MeasureCode.EndsWith("_QNTY") ? cqCalculation[hospitalId].MeasureQuantityValue :
						m.MeasureCode.EndsWith("_COST") ? cqCalculation[hospitalId].MeasureAverageCostValue :
						"";
				}

				return String.Format("{0}",rate);
			}
        }
    }
}
