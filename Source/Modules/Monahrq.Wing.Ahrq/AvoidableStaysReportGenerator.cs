using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Services.Generators;
using Monahrq.Wing.Ahrq.Area;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    /// class for Avoidable stays report generator.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    [Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(
        new[] { "2AAF7FBA-7102-4C66-8598-A70597E2F833" },
        null,
        new[] { typeof(AreaTarget) },
        3)]
    public class AvoidableStaysReportGenerator : BaseReportGenerator
    {
        /// <summary>
        /// The json ahs domain
        /// </summary>
        private const string JSON_AHS_DOMAIN = " $.monahrq.ahs = ";
        /// <summary>
        /// The ahs county json file
        /// </summary>
        private const string AHS_COUNTY_JSON_FILE = "ahs.js";
        // private const string AhsConditionJasonFile = "ahsCondition.js";
        /// <summary>
        /// The ahs topics json file
        /// </summary>
        private const string AHS_TOPICS_JSON_FILE = "ahsTopics.js";
        /// <summary>
        /// The json topics domain
        /// </summary>
        private const string JSON_TOPICS_DOMAIN = " $.monahrq.ahsTopics = ";

        /// <summary>
        /// The qiarea measures json file
        /// </summary>
        private const string QIAREA_MEASURES_JSON_FILE = "Measure_{0}.js";
        /// <summary>
        /// The qiarea measures domain
        /// </summary>
        private const string QIAREA_MEASURES_DOMAIN = "$.monahrq.qualitydata.measuredescription_{0}=";

        // private List<AreaTarget> _applicableAreaTargets;
        /// <summary>
        /// The applicable counties
        /// </summary>
        private List<County> _applicableCounties;
        /// <summary>
        /// The applicable measures areas
        /// </summary>
        private IList<Measure> _applicableMeasuresAreas;
        //  private IList<Measure> _userSelectedMeasures;
        /// <summary>
        /// The area target identifier
        /// </summary>
        private int _areaTargetId;

        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator()
        {
            ////TODO delete for testing only 
            //Task.Run(() =>
            //{
            //    if (LoadReportData())
            //    {
            //        OutputDataFiles();
            //    }
            //}).Wait();
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Avoidable Stays reports" });
        }

        /// <summary>
        /// Validates the report(S) dependencies needed to generate the report.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns></returns>
        public override bool ValidateDependencies(Infrastructure.Domain.Websites.Website website, IList<ValidationResult> validationResults)
        {
            var result = base.ValidateDependencies(website, validationResults);
            if (result && CurrentWebsite.Datasets.All(ds => ds.Dataset.ContentType.Name.ToUpper() != "AHRQ-QI AREA DATA"))
            {
                validationResults.Add(
                    new ValidationResult(
                        "Avoidable Stays reports can not be generated due to no AHRQ-QI Area dataset attached to website."));
            }
            return result;
        }

        /// <summary>
        ///     Loads the report data.
        /// </summary>
        /// <returns></returns>
        protected override bool LoadReportData()
        {
            var wd = CurrentWebsite.Datasets.FirstOrDefault(ds => ds.Dataset.ContentType.Name.ToUpper() == "AHRQ-QI AREA DATA");
            if (wd == null) return false;
            _areaTargetId = wd.Dataset.Id;

            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                var states = CurrentWebsite.SelectedReportingStates.ToList();
                //MonahrqContext.ReportingStatesContext.Where(s => CurrentWebsite.SelectedReportingStates.Contains(s.Abbreviation)).Select(s => s.Abbreviation).ToList();

                _applicableCounties =
                   (List<County>)session.QueryOver<County>() // this bring a full county object ??? we don't n
                       .WhereRestrictionOn(c => c.State).IsIn(states)
                       .List<County>() ?? new List<County>();

                //per  discussion with jon use the website measures vs Area measures from DB 
       

                _applicableMeasuresAreas = CurrentWebsite.Measures != null
                                                            ? CurrentWebsite.Measures.Where(m=>m.IsSelected)
                                                                                     .Select(m => m.OverrideMeasure ?? m.OriginalMeasure)
                                                                                     .Where(m => m.MeasureType == "QIarea" )
                                                                                     .ToList()
                                                            : new List<Measure>();

                //TODO check dependecy if WinQi Connection is is available 

                try
                {
                    var dictionary = new Dictionary<string, decimal>();

                    var hospitalIds = CurrentWebsite.Hospitals.Select(h => h.Hospital.Id).ToList();

                    foreach (var selectedHospital in CurrentWebsite.Hospitals.ToList())
                    {
                        var localHospId = selectedHospital.Hospital.CmsProviderID;

                        var ccr = new decimal(0.0);

                        if (!string.IsNullOrEmpty(selectedHospital.CCR))
                            ccr = decimal.Parse(selectedHospital.CCR);

                        if (localHospId != null && !dictionary.ContainsKey(localHospId))
                            dictionary.Add(localHospId, ccr);
                    }

                    var hospToRefernceList = hospitalIds.Aggregate(string.Empty, (current, hospid) => current + string.Format("{0},", hospid));
                    if (hospToRefernceList.EndsWith(","))
                        hospToRefernceList = hospToRefernceList.SubStrBeforeLast(",");

                    var statesToReferenceList = (CurrentWebsite.SelectedReportingStates ?? Enumerable.Empty<string>()).ToList().Aggregate(string.Empty, (current, state) => current + string.Format("'{0}',", state));
                    if (statesToReferenceList.EndsWith(","))
                        statesToReferenceList = statesToReferenceList.SubStrBeforeLast(",");

                    var hqlQuery = string.Format("select o from Hospital o, State s where o.State = s.Abbreviation and s.Abbreviation in ({0}) ", statesToReferenceList);
                    hqlQuery += string.Format(" and o.Id not in ({0}) and o.IsArchived=0 and o.IsDeleted=0 and o.CmsProviderID is not null", hospToRefernceList);

                    var hospital2 = session.CreateQuery(hqlQuery).List<Hospital>().ToList().ToDictionary(h => h.CmsProviderID, h => h.CCR ?? 0M);

                    foreach (var hospital in hospital2)
                    {
                        var kvpToAdd = hospital;
                        if (hospital.Value == 0)
                        {
                            var query = string.Format("select top 1 [dbo].fnGetCostToChargeRatio('{0}','{1}') from Hospitals ", CurrentWebsite.ReportedYear, kvpToAdd.Key);
                            var ccr = session.CreateSQLQuery(query).UniqueResult<float>();

                            kvpToAdd = new KeyValuePair<string, decimal>(kvpToAdd.Key, decimal.Parse(ccr.ToString(CultureInfo.InvariantCulture)));
                        }

                        if (!dictionary.ContainsKey(kvpToAdd.Key))
                            dictionary.Add(kvpToAdd.Key, kvpToAdd.Value);
                    }

                    //if (CheckIfCostAlreadyCalculated(_areaTargetId))
                    //{
                    //    Logger.Write(
                    //        "Total Cost for Area QI data has been calulated already. Skipping cost calculation.");
                    //    EventAggregator.GetEvent<WebsitePublishEvent>()
                    //                   .Publish(
                    //                       new ExtendedEventArgs<WebsitePublishEventArgs>(
                    //                           new WebsitePublishEventArgs(
                    //                               "Total Cost for Area QI data has been calulated already. Skipping cost calculation.",
                    //                               PubishMessageTypeEnum.Information,
                    //                               WebsiteGenerationStatus.ReportsGenerationInProgress, DateTime.Now,
                    //                               PublishTask.Full)
                    //                           ));
                    //}
                    //else
                    //{
                    if (ConfigurationService.WinQiConnectionSettings == null ||
                        string.IsNullOrEmpty(ConfigurationService.WinQiConnectionSettings.ConnectionString))
                    {
                        const string message =
                            "There was an error while trying to perform cost calculation. Please ensure that you have saved the connection string to \n" +
							"the cost Quality Indicators database. Skipping the rest of cost calculation.";

						LogMessage(message, PubishMessageTypeEnum.Error);
                    }
                    else
                    {
                        if (!TestWinQiConnection(ConfigurationService.WinQiConnectionSettings.ConnectionString))
                        {
                            const string message =
                                "There was an error while trying to perform cost calculation. Please ensure that you have saved the connection string to \n" +
								"the cost Quality Indicators database, the Quality indicators database exists or you do not have permissions to access the database. Skipping the rest of cost calculation.";

							LogMessage(message, PubishMessageTypeEnum.Error);
						}
                        else
						{
							LogMessage("Executing cost calculations");

                            try
                            {
                                new CostCalculator(ConfigurationService.WinQiConnectionSettings.ConnectionString,
                                                   ConfigurationService.ConnectionSettings.ConnectionString, dictionary,
                                                   _areaTargetId)
                                    .LoadMonAhrqData()
                                    .CalculateCost()
                                    .UpdateCost();
                            }
                            catch (Exception ex)
							{
								LogMessage(
									string.Format("A error occurred while preforming cost calculation. Skipping the rest of cost calculation.{0}{1}", Environment.NewLine, ex.GetBaseException().Message), 
									PubishMessageTypeEnum.Error);
							}

							LogMessage("Finished cost calculations");

                            //}
                            //else
                            //{
                            //    Logger.Write(
                            //        "WinQI is not installed or there is an issue with connecting to WinQI database. Skipping cost calculation.");
                            //    EventAggregator.GetEvent<WebsitePublishEvent>()
                            //                   .Publish(
                            //                       new ExtendedEventArgs<WebsitePublishEventArgs>(
                            //                           new WebsitePublishEventArgs(
                            //                               "WinQI is not installed or there is an issue with connecting to WinQI database. Skipping cost calculation.",
                            //                               PubishMessageTypeEnum.Information,
                            //                               WebsiteGenerationStatus.ReportsGenerationInProgress,
                            //                               DateTime.Now,
                            //                               PublishTask.Full)
                            //                           ));
                            //}
                        }
                    }
                }
                catch (Exception ex)
				{
					LogMessage(
						"WinQI is not installed or there is an issue with connecting to WinQI database. Skipping cost calculation.",
						PubishMessageTypeEnum.Error);
					
                    Logger.Write((ex.InnerException ?? ex).Message + System.Environment.NewLine + (ex.InnerException ?? ex).StackTrace);

                }


                //ConfigurationManager.ConnectionStrings["Monharq"].ConnectionString);
                //ConfigurationService.WinQiConnectionSettings = new ConnectionStringSettings("winQi", @"Data Source=(local);Initial Catalog=qualityindicators;Integrated Security=True");
                //ConfigurationService.ConnectionSettings = new ConnectionStringSettings("Monahrq", @"Data Source=(local);Initial Catalog=as;Integrated Security=True");
            }

            return true;
        }

        /// <summary>
        /// Checks if cost already calculated.
        /// </summary>
        /// <param name="areaTargetId">The area target identifier.</param>
        /// <returns></returns>
        private bool CheckIfCostAlreadyCalculated(int areaTargetId)
        {
            bool hasCostTotalsAlready;
            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                hasCostTotalsAlready = session.Query<AreaTarget>()
                                              .Any(at => at.Dataset.Id == areaTargetId && at.TotalCost.HasValue);
            }
            return hasCostTotalsAlready;
        }

        /// <summary>
        /// Tests the win qi connection.
        /// </summary>
        /// <param name="winQiConnectionStr">The win qi connection string.</param>
        /// <returns></returns>
        private static bool TestWinQiConnection(string winQiConnectionStr)
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

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
        {
            try
            {
                var reportsDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Reports");
                if (!Directory.Exists(reportsDataDir)) Directory.CreateDirectory(reportsDataDir);

                var baseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
                if (!Directory.Exists(baseDataDir)) Directory.CreateDirectory(baseDataDir);

                var ahsCounties = GetAhsCounties().ToList();

                var countiesByMeasures = new Dictionary<string, IList<ahsCounty>>();

                foreach (var county in ahsCounties)
                {
                    var measureCode = county.measureId;
                    var measureTopic = county.topicId;
                    if (countiesByMeasures.ContainsKey(measureCode)) continue;


                    var tempCounties = ahsCounties.Where(c => c.measureId.EqualsIgnoreCase(measureCode))
                                                  .DistinctBy(x => x.countyId).ToList();

                    tempCounties.AddRange(_applicableCounties.Where(c => tempCounties.All(c2 => c2.countyId != c.Id))
                                                             .DistinctBy(x => x.Id)
                                                             .Select(x => new ahsCounty
                                                             {
                                                                 topicId = measureTopic,
                                                                 //stratification = null,
                                                                 measureId = measureCode,
                                                                 state = x.State.ToLower(),
                                                                 countyId = x.Id,
                                                                 numerator = "-1",
                                                                 denominator = "-1",
                                                                 observedRate = "-1",
                                                                 riskAdjRate = "-1",
                                                                 mapRate = -1,
                                                                 pct10 = -1,
                                                                 pct20 = -1,
                                                                 pct30 = -1,
                                                                 pct40 = -1,
                                                                 pct50 = -1,
                                                                 band = -1
                                                             })
                                                             .ToList());

                    countiesByMeasures.Add(measureCode, tempCounties.DistinctBy(x => x.countyId)
                                                                    .OrderByDescending(x => x.mapRate)
                                                                    .ToList()); //mapRate
                }
                //.Where(c => c.measureId.EqualsIgnoreCase(measureCode))

                ahsCounties.Clear();
                foreach (var countiesByMeasure in countiesByMeasures)
                {
                    var countiesCount = countiesByMeasure.Value.Count();
                    int divisor;

                    if (countiesCount <= 8) //10
                    {
                        divisor = 2;
                    }
                    else if (countiesCount <= 12) //15
                    {
                        divisor = 3;
                    }
                    else if (countiesCount <= 16) //15
                    {
                        divisor = 4;
                    }
                    else
                    {
                        divisor = 5;
                    }

                    var countiesGroupSize = (countiesCount / divisor) > 4 ? (countiesCount / 4) : divisor; //5

                    for (var i = 0; i < countiesCount; i++)
                    {
                        //countiesByMeasure.Value[i].band = countiesByMeasure.Value[i].mapRate == 0 || countiesByMeasure.Value[i].mapRate == -1 || countiesByMeasure.Value[i].mapRate == -2 
                        //                        ? -1
                        //                        : i/countiesGroupSize;
                        countiesByMeasure.Value[i].band = countiesByMeasure.Value[i].mapRate <= 0
                                                ? -1
                                                : i / countiesGroupSize;
                    }

                    ahsCounties.AddRange(countiesByMeasure.Value.OrderByDescending(x => x.band).ToList());
                }

                CalculateTotals(ref ahsCounties);

                GenerateJsonFile(ahsCounties, Path.Combine(reportsDataDir, AHS_COUNTY_JSON_FILE), JSON_AHS_DOMAIN);
                GenerateJsonFile(GetAhsTopics(), Path.Combine(baseDataDir, AHS_TOPICS_JSON_FILE), JSON_TOPICS_DOMAIN);

                var ahsMeasureDirectoryPath = Path.Combine(baseDataDir, "QualityMeasures");
                if (!Directory.Exists(ahsMeasureDirectoryPath))
                    Directory.CreateDirectory(ahsMeasureDirectoryPath);

                foreach (var measure in GetQIAreaMeasures().ToArray())
                {
                    GenerateJsonFile(measure,
                                     Path.Combine(ahsMeasureDirectoryPath, string.Format(QIAREA_MEASURES_JSON_FILE, measure.MeasureID)),
                                     string.Format(QIAREA_MEASURES_DOMAIN, measure.MeasureID));
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return false;
            }
        }

        private void CalculateTotals(ref List<ahsCounty> ahsCounties)
        {
            var result = new List<ahsCounty>();

            using (var session = base.DataProvider.SessionFactory.OpenStatelessSession())
            {
                //
                var measures = ahsCounties.Where(m => !m.measureId.EqualsIgnoreCase("*****"))
                    .OrderBy(m => m.topicId)
                    .ThenBy(m => m.measureId)
                    .Select(m => m.measureId)
                    .Distinct()
                    .ToList();

                foreach (var measureId in measures)
                {
                    //foreach (var measreId in  ahsCounties.Where(m => m.topicId == topicId).Select(m => m.measureId).Distinct().ToList())
                    //{
                    // string totalState;
                    decimal denominator = 0;
                    decimal numerator = 0;
                    decimal obRate = 0M;
                    decimal rAdjRate = 0M;
                    decimal seRAdjRate = 0M;

                    decimal totalPct10 = 0M;
                    decimal totalPct20 = 0M;
                    decimal totalPct30 = 0M;
                    decimal totalPct40 = 0M;
                    decimal totalPct50 = 0M;

                    var measureItemResults = ahsCounties.Where(ahsC => ahsC.measureId == measureId).OrderBy(ahsC => ahsC.measureId).ToList();
                    var itemCount = 0;
                    foreach (var item in measureItemResults)
                    {
                        denominator = denominator +
                                      ((item.denominator != "-1" && item.denominator != "-2")
                                          ? decimal.Parse(item.denominator)
                                          : 0);
                        numerator = numerator +
                                    ((item.numerator != "-1" && item.numerator != "-2")
                                        ? decimal.Parse(item.numerator)
                                        : 0);

                        obRate = obRate +
                                 ((item.observedRate != "-1" && item.observedRate != "-2")
                                     ? decimal.Parse(item.observedRate)
                                     : 0);
                        rAdjRate = rAdjRate +
                                   ((item.riskAdjRate != "-1" && item.riskAdjRate != "-2")
                                       ? decimal.Parse(item.riskAdjRate)
                                       : 0);

                        totalPct10 = totalPct10 + (item.pct10 >= 0M ? item.pct10 : 0M);
                        totalPct20 = totalPct20 + (item.pct20 >= 0M ? item.pct20 : 0M);
                        totalPct30 = totalPct30 + (item.pct30 >= 0M ? item.pct30 : 0M);
                        totalPct40 = totalPct40 + (item.pct40 >= 0M ? item.pct40 : 0M);
                        totalPct50 = totalPct50 + (item.pct50 >= 0M ? item.pct50 : 0M);

                        result.Add(item);

                        if (itemCount == measureItemResults.Count - 1)
                        {
                            var totalItems = session.Query<AreaTarget>()
                                                   .Where(at => at.Stratification.ToUpper() == "AVG" &&
                                                                 at.MeasureCode.ToUpper() == item.measureId.ToUpper())
                                                   .Select(at => new
                                                   {
                                                       Measure = at.MeasureCode,
                                                       CountyFIPS = -1,
                                                       Numerator = at.ObservedNumerator,
                                                       Denominator = at.ObservedDenominator,
                                                       OR = at.ObservedRate,
                                                       RAR = at.RiskAdjustedRate
                                                   })
                                                   .ToList();

                            if (totalItems.Any())
                            {
                                result.Add(new ahsCounty
                                {
                                    topicId = item.topicId,
                                    measureId = item.measureId,
                                    state = item.state.ToLower(),
                                    countyId = -1,
                                    numerator = totalItems[0].Numerator.ToString(),
                                    denominator = totalItems[0].Denominator.ToString(),
                                    observedRate = totalItems[0].OR.ToString(),
                                    riskAdjRate = totalItems[0].RAR.ToString(),
                                    mapRate = 0M,
                                    band = 0,
                                    pct10 = totalPct10,
                                    pct20 = totalPct20,
                                    pct30 = totalPct30,
                                    pct40 = totalPct40,
                                    pct50 = totalPct50
                                });
                            }
                            else
                            {
                                result.Add(new ahsCounty
                                {
                                    topicId = item.topicId,
                                    measureId = item.measureId,
                                    state = item.state.ToLower(),
                                    countyId = -1,
                                    numerator = numerator.ToString(),
                                    denominator = denominator.ToString(),
                                    //observedRate = Math.Round((obRate / measureItemResults.Count), 2).ToString(),
                                    //riskAdjRate = Math.Round((rAdjRate / measureItemResults.Count), 2).ToString(),
                                    observedRate = (obRate / measureItemResults.Count).ToString(),
                                    riskAdjRate = (rAdjRate / measureItemResults.Count).ToString(),
                                    mapRate = 0M,
                                    pct10 = totalPct10,
                                    pct20 = totalPct20,
                                    pct30 = totalPct30,
                                    pct40 = totalPct40,
                                    pct50 = totalPct50
                                });
                            }
                        }
                        itemCount++;
                    }
                    //  }
                }
            }

            if (result.Any())
            {
                ahsCounties = result.OrderBy(ahs => ahs.topicId)
                                    .ThenBy(ahs => ahs.measureId)
                                    //.Where(ahs => ahs.countyId != -1).Union(result.)
                                    .ToList();
            }
        }

        /// <summary>
        /// Gets the qi area measures.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MeasureDto> GetQIAreaMeasures()
        {
            var qiAreaMeasures = CurrentWebsite.Measures
                                               .Where(m => m.ReportMeasure.MeasureType.EqualsIgnoreCase("QIAREA"))
                                               .Select(m => new MeasureDto
                                               {
                                                   MeasureID = m.ReportMeasure.Id,
                                                   MeasuresName = m.ReportMeasure.Name,
                                                   MeasureSource = "AHRQ",
                                                   MeasureType = m.ReportMeasure.MeasureType,
                                                   HigherScoresAreBetter = m.ReportMeasure.HigherScoresAreBetter,
                                                   HigherScoresAreBetterDescription = string.Empty,
                                                   TopicsID = string.Join(",", m.ReportMeasure.Topics.Select(t => t.Id).ToArray()),
                                                   NatLabel = "Nationwide Mean",
                                                   NatRateAndCI = m.ReportMeasure.NationalBenchmark != null ? m.ReportMeasure.NationalBenchmark.ToString() : string.Empty,
                                                   NatTop10Label = "Nationwide best 10%", // TODO: Look into this
                                                   NatTop10 = "87.0", // TODO: Look into this
                                                   PeerLabel = "State Mean", // TODO: Look into this
                                                   PeerRateAndCI = m.ReportMeasure.StatePeerBenchmark != null && m.ReportMeasure.StatePeerBenchmark.ProvidedBenchmark.HasValue ? m.ReportMeasure.StatePeerBenchmark.ProvidedBenchmark.Value.ToString() : string.Empty,
                                                   PeerTop10Label = "Peer best 10%",
                                                   PeerTop10 = "", // TODO: Look into this
                                                   Footnote = m.ReportMeasure.Footnotes,
                                                   BarHeader = "",
                                                   BarFooter = "per 100 cases.",// TODO: Look into this
                                                   ColDesc1 = "Patients responding &quot;Always&quot;", // TODO: Look into this
                                                   ColDesc2 = "Patients responding &quot;Usually&quot;", // TODO: Look into this
                                                   ColDesc3 = "Patients responding &quot;Never&quot;", // TODO: Look into this
                                                   ColDesc4 = "Surveys completed", // TODO: Look into this
                                                   ColDesc5 = "Response rate", // TODO: Look into this
                                                   ColDesc6 = "",
                                                   ColDesc7 = "",
                                                   ColDesc8 = "",
                                                   ColDesc9 = "",
                                                   ColDesc10 = "",
                                                   NatCol1 = m.ReportMeasure.NationalBenchmark.HasValue ? m.ReportMeasure.NationalBenchmark.Value.ToString() : string.Empty,
                                                   NatCol2 = "",
                                                   NatCol3 = "",
                                                   NatCol4 = "",
                                                   NatCol5 = "",
                                                   NatCol6 = "",
                                                   NatCol7 = "",
                                                   NatCol8 = "",
                                                   NatCol9 = "",
                                                   NatCol10 = "",
                                                   PeerCol1 = m.ReportMeasure.StatePeerBenchmark != null && m.ReportMeasure.StatePeerBenchmark.ProvidedBenchmark.HasValue ? m.ReportMeasure.StatePeerBenchmark.ProvidedBenchmark.Value.ToString() : string.Empty,
                                                   PeerCol2 = "",
                                                   PeerCol3 = "",
                                                   PeerCol4 = "",
                                                   PeerCol5 = "",
                                                   PeerCol6 = "",
                                                   PeerCol7 = "",
                                                   PeerCol8 = "",
                                                   PeerCol9 = "",
                                                   PeerCol10 = "",
                                                   SelectedTitle = m.ReportMeasure.MeasureTitle.Selected == SelectedMeasuretitleEnum.Plain ? m.ReportMeasure.MeasureTitle.Plain : m.ReportMeasure.MeasureTitle.Clinical,
                                                   PlainTitle = m.ReportMeasure.MeasureTitle.Plain,
                                                   ClinicalTitle = m.ReportMeasure.MeasureTitle.Clinical,
                                                   MeasureDescription = m.ReportMeasure.Description,
                                                   Bullets = GetMeasureBullets(m.ReportMeasure),
                                                   StatisticsAvailable = string.Empty,
                                                   MoreInformation = m.ReportMeasure.MoreInformation,
                                                   URL = m.ReportMeasure.Url,
                                                   URLTitle = m.ReportMeasure.UrlTitle,
                                                   DataSourceURL = @"http://www.ahrq.gov/",
                                                   DataSourceURLTitle = "AHRQ Quality Indicator"
                                               })
                                               .ToArray();

            return qiAreaMeasures;
        }

        /// <summary>
        /// Gets the measure bullets.
        /// TODO: This is a huge refactor candidate.
        /// </summary>
        /// <param name="measure">The measure.</param>
        /// <returns></returns>
        private string GetMeasureBullets(Measure measure)
        {
            var bullets = new StringBuilder();

            // first bullet points
            bullets.Append(!measure.HigherScoresAreBetter
                               ? "<li>A lower score is better.</li>"
                               : "<li>A higher score is better.</li>");

            // 2nd bullet points
            if (measure.Name.StartsWith("PQI"))
                bullets.Append(
                    "<li>This is not a measure of hospital quality. Evidence shows these hospital stays are potentially avoidable when patients have access to high quality outpatient care.</li>");
            else if (measure.Name.StartsWith("PSI"))
                bullets.Append(
                    "<li>This is a measure of patient safety.  It captures hospital stays caused by potentially avoidable complications.  It also captures hospital stays in which potentially avoidable complications occur.</li>");
            else if (measure.Name.StartsWith("IQI"))
                bullets.Append("<li>This is not a measure of hospital quality. It is a measure of practice patterns in an area. There can be wide variation in practice patterns that might suggest overuse or underuse.</li>");

            // 3rd bullet points

            const string iqIcontent1 = "deaths due to these conditions";
            const string iqIcontent2 = "patients died";
            const string psIcontent1 = "selected patient safety events";
            const string psIcontent2 = "discharges involved complications";

            const string bulletText = "<li>Hospital scores presented are risk-standardized ratios summed across all conditions. The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer CONT1 than other hospitals nationwide with a similar case mix. For example, an overall score of 0.5 means that half as many CONT2 as expected.</li><li>A score of 1 means that the hospital had the same number of CONT1 as other hospitals nationwide with a similar case mix.</li><li>A score of more than 1 means the hospital had more CONT1 than other hospitals nationwide with a similar case mix. For example, an overall score of 2.0 means that twice as many CONT2 as expected.</li></ul><li>Each individual component of this score takes into account how sick patients were before they went to the hospital (the components are risk-adjusted).</li><li>Ratings include a significance test.</li>";
            if (measure.Name.StartsWith("IQI"))
            {
                bullets.Append(bulletText.Replace("CONT1", iqIcontent1).Replace("CONT2", iqIcontent2));
            }
            else if (measure.Name.StartsWith("PSI"))
                bullets.Append(bulletText.Replace("CONT1", psIcontent1).Replace("CONT2", psIcontent2));

            if (measure.NQFEndorsed)
                bullets.Append("<li>This measure is endorsed by the National Quality Forum, an independent organization that sets standards for health care quality measurement.</li>");

            // bullet 6
            if (measure.ScaleBy.HasValue)
            {
                if (measure.ScaleBy == -2)
                    bullets.Append("<li>Figures presented are ratios of observed to expected.</li>");
                else if (measure.ScaleBy == -1)
                    bullets.Append("<li>Figures presented are in minutes.</li>");
                else if (measure.ScaleBy == 1)
                    bullets.Append("<li>Figures presented are counts.</li>");
                else if (measure.ScaleBy > 1) // Area Measures.
                {
                    if (string.IsNullOrEmpty(measure.RateLabel) || measure.RateLabel.Length <= 3) // TBD is default
                        bullets.Append("<<li>The number of hospital stays is provided for every " + measure.ScaleBy + " people who reside in that area (i.e., the population).</li>");
                    else
                        bullets.Append("<li>The number of hospital stays is provided for every " + measure.ScaleBy + " " + measure.RateLabel + ".</li>");
                }
            }

            return string.Format("<ul>{0}</ul>", bullets);
        }

        /// <summary>
        /// Gets the ahs topics.
        /// </summary>
        /// <returns></returns>
        private IEnumerable GetAhsTopics()
        {
            //var topics = _applicableMeasuresAreas
            //        .SelectMany(m => m.Topics).Distinct();
            //foreach (var topic in topics)
            //{
            //    // TODO: Check correct topic.
            //    yield return new
            //    {
            //        id = topic.Id,
            //        name = topic.Name,
            //        measures = topic.Measures.Select(m=>new
            //        {
            //           id=m.Name,
            //           name=m.MeasureTitle.Plain,
            //            scale= new {
            //                scaleBy=m.ScaleBy,
            //                scaleTarget = m.ScaleTarget
            //            }
            //        })
            //    };
            //}

            var topics = _applicableMeasuresAreas
                    .SelectMany(m => m.Topics).DistinctBy(x => x.Id);

            foreach (var topic in topics)
            {
                // TODO: Check correct topic.
                yield return new
                {
                    id = topic.Id,
                    name = topic.Name,
                    measures = _applicableMeasuresAreas.Where(m => m.Topics.Any(t => t.Id == topic.Id)).Select(m => new
                    {
                        id = m.Name,
                        MeasureId = m.Id,
                        name = m.MeasureTitle.Plain,
                        scale = new
                        {
                            scaleBy = m.ScaleBy,
                            scaleTarget = m.ScaleTarget,
                            scaleSource = m.RiskAdjustedMethod == "no"
                                                    ? "Observed rate"
                                                    : "Risk adjusted rate"
                        }
                    })
                };
            }
        }

        //private IEnumerable GetAhsCondition()
        //{
        //    foreach (var applicableCountiesFip in _applicableCountiesFips)
        //    {
        //        var targets =
        //            _applicableAreaTargets.Where(t => t.StratID == applicableCountiesFip)
        //                .ToList();

        //        if (!targets.Any())
        //        {
        //            yield return new
        //            {
        //                topicId = "******",
        //                measureId = "*****",
        //                state = ,
        //                countyId = applicableCountiesFip,
        //                numerator = 0,
        //                denominator = 0,
        //                observedRate = 0,
        //                riskAdjRate = 0,
        //                pct10 = 0,
        //                pct20 = 0,
        //                pct30 = 0,
        //                pct40 = 0,
        //                pct50 = 0,
        //                band=0
        //            };
        //        }
        //        else
        //        {
        //            foreach (var areaTarget in targets)
        //            {
        //                var topics =
        //                    _applicableMeasuresAreas.Where(m => m.Name == areaTarget.MeasureCode)
        //                        .SelectMany(m => m.Topics)
        //                        .ToList();
        //                if (!topics.Any())
        //                {
        //                    yield return new
        //                    {
        //                        topicId = "******",
        //                        measureId = areaTarget.MeasureCode,
        //                        state = ,
        //                        countyId = applicableCountiesFip,
        //                        numerator = areaTarget.ObservedNumerator,
        //                        denominator = areaTarget.ObservedDenominator,
        //                        observedRate = areaTarget.ObservedRate,
        //                        riskAdjRate = areaTarget.RiskAdjustedRate,
        //                        pct10 = 0,
        //                        pct20 = 0,
        //                        pct30 = 0,
        //                        pct40 = 0,
        //                        pct50 = 0,
        //                        band=0
        //                    };
        //                }
        //                else
        //                {
        //                    foreach (var topic in topics)
        //                    {
        //                        yield return new
        //                        {
        //                            topicId = topic.Id,
        //                            measureId = areaTarget.MeasureCode,
        //                            state = ,
        //                            countyId = applicableCountiesFip,
        //                            numerator = areaTarget.ObservedNumerator,
        //                            denominator = areaTarget.ObservedDenominator,
        //                            observedRate = areaTarget.ObservedRate,
        //                            riskAdjRate = areaTarget.RiskAdjustedRate,
        //                            pct10 = 0,
        //                            pct20 = 0,
        //                            pct30 = 0,
        //                            pct40 = 0,
        //                            pct50 = 0
        //                        };
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Gets the ahs counties.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ahsCounty> GetAhsCounties()
        {
            List<ahsCounty> result = new List<ahsCounty>();
            List<ahsCounty> resultTemp = new List<ahsCounty>();
            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                foreach (var applicableCounty in _applicableCounties)
                {
                    var county = applicableCounty;
                    var targets = session.QueryOver<AreaTarget>()
                                         .Where(a => a.Dataset.Id == _areaTargetId && a.CountyFIPS == county.CountyFIPS)
                                         .List<AreaTarget>() ?? new List<AreaTarget>();

                    if (!targets.Any())
                    {

                        resultTemp.Add(new ahsCounty
                        {
                            topicId = "******",
                            //stratification = null,
                            measureId = "*****",
                            state = county.State.ToLower(),
                            countyId = county.Id,
                            numerator = "0",
                            denominator = "0",
                            observedRate = "0",
                            riskAdjRate = "0",
                            mapRate = 0M,
                            pct10 = 0M,
                            pct20 = 0M,
                            pct30 = 0M,
                            pct40 = 0M,
                            pct50 = 0M,
                            band = -1
                        });
                    }
                    else
                    {

                        foreach (var areaTarget in targets)
                        {
                            //var measure = _applicableMeasuresAreas.FirstOrDefault(m => m.Name.ToUpper() == areaTarget.MeasureCode.ToUpper());
                            var measure = _applicableMeasuresAreas.FirstOrDefault(m => m.Name.ToUpper() == ((m.Name.Contains(" "))
                                                                                                                    ? areaTarget.MeasureCode.ToUpper()
                                                                                                                    : areaTarget.MeasureCode.Replace(" ", null).ToUpper()));
                            var raMethod = measure != null && measure.RiskAdjustedMethod == "no";
                            var measurescaleBy = measure == null ? 0 : (measure.ScaleBy ?? 0);

                            if (measure == null || measure.Topics == null || !measure.Topics.Any())
                            {
                                resultTemp.Add(new ahsCounty
                                {
                                    topicId = "******",
                                    //stratification = areaTarget.Stratification,
                                    measureId = areaTarget.MeasureCode,
                                    state = county.State.ToLower(),
                                    countyId = county.Id,
                                    numerator = areaTarget.ObservedNumerator.HasValue ? areaTarget.ObservedNumerator.Value.ToString(CultureInfo.InvariantCulture) : "-",
                                    denominator = areaTarget.ObservedDenominator.HasValue ? areaTarget.ObservedDenominator.Value.ToString(CultureInfo.InvariantCulture) : "-",
                                    //observedRate = (Math.Round((areaTarget.ObservedRate ?? 0M) * measurescaleBy, 2)).ToString(CultureInfo.InvariantCulture),
                                    //riskAdjRate = (Math.Round((areaTarget.RiskAdjustedRate ?? 0M) * measurescaleBy, 2)).ToString(CultureInfo.InvariantCulture),
                                    //mapRate = Math.Round((areaTarget.RiskAdjustedRate ?? 0M)*measurescaleBy, 2),
                                    //pct10 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.10, 2),
                                    //pct20 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.20, 2),
                                    //pct30 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.30, 2),
                                    //pct40 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.40, 2),
                                    //pct50 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.50, 2),
                                    observedRate = ((areaTarget.ObservedRate ?? 0M) * measurescaleBy).ToString(CultureInfo.InvariantCulture),
                                    riskAdjRate = ((areaTarget.RiskAdjustedRate ?? 0M) * measurescaleBy).ToString(CultureInfo.InvariantCulture),
                                    mapRate = Math.Round((areaTarget.RiskAdjustedRate ?? 0M) * measurescaleBy, 2),
                                    pct10 = (areaTarget.TotalCost ?? 0M) * (decimal)0.10,
                                    pct20 = (areaTarget.TotalCost ?? 0M) * (decimal)0.20,
                                    pct30 = (areaTarget.TotalCost ?? 0M) * (decimal)0.30,
                                    pct40 = (areaTarget.TotalCost ?? 0M) * (decimal)0.40,
                                    pct50 = (areaTarget.TotalCost ?? 0M) * (decimal)0.50,
                                    band = -1
                                });
                            }
                            else
                            {
                                bool suppressNumerator = false;

                                bool suppressDenominator = ShouldSuppressDenominator(areaTarget, measure);

                                if (!suppressDenominator)
                                    suppressNumerator = ShouldSuppressNumerator(areaTarget, measure);

                                foreach (var topic in measure.Topics)
                                {
                                    if (suppressDenominator)
                                    {
                                        const string suppressDenominatorValue = "-1";
                                        resultTemp.Add(new ahsCounty
                                        {
                                            topicId = topic.Id.ToString(CultureInfo.InvariantCulture),
                                            measureId = measure.Name,
                                            state = county.State.ToLower(),
                                            countyId = county.Id,
                                            numerator = suppressDenominatorValue,
                                            denominator = suppressDenominatorValue,
                                            observedRate = suppressDenominatorValue,
                                            riskAdjRate = suppressDenominatorValue,
                                            mapRate = -1,
                                            pct10 = -1,//Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.10, 2),
                                            pct20 = -1,//Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.20, 2),
                                            pct30 = -1,//Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.30, 2),
                                            pct40 = -1,//Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.40, 2),
                                            pct50 = -1,//Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.50, 2),
                                            band = -1
                                        });
                                    }
                                    else if (suppressNumerator)
                                    {
                                        const string suppressNumeratorValue = "-2";
                                        resultTemp.Add(new ahsCounty
                                        {
                                            topicId = topic.Id.ToString(CultureInfo.InvariantCulture),
                                            measureId = measure.Name,
                                            state = county.State.ToLower(CultureInfo.InvariantCulture),
                                            countyId = county.Id,
                                            numerator = suppressNumeratorValue,
                                            denominator = areaTarget.ObservedDenominator.HasValue ? Math.Round((decimal)areaTarget.ObservedDenominator.Value, 2, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture) : "-",
                                            observedRate = suppressNumeratorValue,
                                            riskAdjRate = suppressNumeratorValue,
                                            mapRate = -2,
                                            pct10 = -2,
                                            pct20 = -2,
                                            pct30 = -2,
                                            pct40 = -2,
                                            pct50 = -2,
                                            band = -1
                                        });
                                    }
                                    else
                                    {
                                        resultTemp.Add(new ahsCounty
                                        {
                                            topicId = topic.Id.ToString(CultureInfo.InvariantCulture),
                                            measureId = measure.Name,
                                            state = county.State.ToLower(),
                                            countyId = county.Id,
                                            numerator = (areaTarget.ObservedNumerator.HasValue ? areaTarget.ObservedNumerator.ToString() : "-"),
                                            denominator = (areaTarget.ObservedDenominator.HasValue ? areaTarget.ObservedDenominator.ToString() : "-"),
                                            //observedRate = (Math.Round(areaTarget.ObservedRate * measurescaleBy ?? 0, 2)).ToString(CultureInfo.InvariantCulture),
                                            //riskAdjRate = (Math.Round(areaTarget.RiskAdjustedRate * measurescaleBy ?? 0, 2)).ToString(CultureInfo.InvariantCulture),
                                            observedRate = (areaTarget.ObservedRate * measurescaleBy ?? 0).ToString(CultureInfo.InvariantCulture),
                                            riskAdjRate = (areaTarget.RiskAdjustedRate * measurescaleBy ?? 0).ToString(CultureInfo.InvariantCulture),
                                            mapRate = Math.Round(((raMethod
                                                              ? (areaTarget.ObservedRate ?? 0M)
                                                              : (areaTarget.RiskAdjustedRate ?? 0M)) * measurescaleBy), 2),
                                            //pct10 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.10, 2),
                                            //pct20 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.20, 2),
                                            //pct30 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.30, 2),
                                            //pct40 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.40, 2),
                                            //pct50 = Math.Round((areaTarget.TotalCost ?? 0M)*(decimal) 0.50, 2),
                                            pct10 = (areaTarget.TotalCost ?? 0M) * (decimal)0.10,
                                            pct20 = (areaTarget.TotalCost ?? 0M) * (decimal)0.20,
                                            pct30 = (areaTarget.TotalCost ?? 0M) * (decimal)0.30,
                                            pct40 = (areaTarget.TotalCost ?? 0M) * (decimal)0.40,
                                            pct50 = (areaTarget.TotalCost ?? 0M) * (decimal)0.50,
                                            band = -1
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return resultTemp.ToList();
        }

        /// <summary>
        /// Shoulds the suppress denominator.
        /// </summary>
        /// <param name="areaTarget">The area target.</param>
        /// <param name="measure">The measure.</param>
        /// <returns></returns>
        private bool ShouldSuppressDenominator(AreaTarget areaTarget, Measure measure)
        {
            if (!areaTarget.ObservedDenominator.HasValue) return true;

            if (areaTarget.ObservedDenominator.Value > 0 &&
                (measure.SuppressionDenominator.HasValue && measure.SuppressionDenominator.Value > 0) &&
                areaTarget.ObservedDenominator.Value <= measure.SuppressionDenominator.Value)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shoulds the suppress numerator.
        /// </summary>
        /// <param name="areaTarget">The area target.</param>
        /// <param name="measure">The measure.</param>
        /// <returns></returns>
        private bool ShouldSuppressNumerator(AreaTarget areaTarget, Measure measure)
        {
            if (!areaTarget.ObservedNumerator.HasValue) return true;

            if (areaTarget.ObservedNumerator.Value > 0 &&
                (measure.SuppressionNumerator.HasValue && measure.SuppressionNumerator.Value > 0) &&
                areaTarget.ObservedNumerator.Value <= measure.SuppressionNumerator.Value)
            {
                return true;
            }

            if (measure.PerformMarginSuppression ||
               (areaTarget.ObservedNumerator.Value > 0 &&
                measure.SuppressionNumerator.HasValue && measure.SuppressionNumerator.Value > 0) &&
                (areaTarget.ObservedDenominator.HasValue && areaTarget.ObservedDenominator.Value > 0) &&
                (areaTarget.ObservedDenominator.Value - areaTarget.ObservedNumerator.Value) < measure.SuppressionNumerator.Value)
            {
                return true;
            }

            if (areaTarget.ObservedNumerator.Value <= 10)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Data Transfer Object class for Measure.
    /// </summary>
    public class MeasureDto
    {
        public int MeasureID { get; set; } // 47,
        public string MeasuresName { get; set; } //"IQI 30",
        public string MeasureSource { get; set; } // "AHRQ",
        public string MeasureType { get; set; } // "QIarea",
        public bool HigherScoresAreBetter { get; set; } // false
        public string HigherScoresAreBetterDescription { get; set; } // "",
        public string TopicsID { get; set; } // "11,17",
        public string NatLabel { get; set; } // "Nationwide Mean",
        public string NatRateAndCI { get; set; } // "1.2000",
        public string NatTop10Label { get; set; } // "",
        public string NatTop10 { get; set; } // "",
        public string PeerLabel { get; set; } //"State Mean",
        public string PeerRateAndCI { get; set; } // "0.0000",
        public string PeerTop10Label { get; set; } // "",
        public string PeerTop10 { get; set; } // "",
        public string Footnote { get; set; } // "1",
        public string BarHeader { get; set; } // "",
        public string BarFooter { get; set; } // "per 100 cases.",
        public string ColDesc1 { get; set; } // "Numerator",
        public string ColDesc2 { get; set; } // "Denominator",
        public string ColDesc3 { get; set; } // "Observed Rate",
        public string ColDesc4 { get; set; } // "Observed Lower-bound CI",
        public string ColDesc5 { get; set; } // "Observed Upper-bound CI",
        public string ColDesc6 { get; set; } // "Expected Rate",
        public string ColDesc7 { get; set; } // "Risk-Adjusted Rate",
        public string ColDesc8 { get; set; } // "Risk-Adjusted Lower-bound CI",
        public string ColDesc9 { get; set; } // "Risk-Adjusted Upper-bound CI",
        public string ColDesc10 { get; set; } // "",
        public string NatCol1 { get; set; } // "",
        public string NatCol2 { get; set; } // "",
        public string NatCol3 { get; set; } // "1.2000",
        public string NatCol4 { get; set; } // "",
        public string NatCol5 { get; set; } // "",
        public string NatCol6 { get; set; } // "",
        public string NatCol7 { get; set; } // "",
        public string NatCol8 { get; set; } // "",
        public string NatCol9 { get; set; } // "",
        public string NatCol10 { get; set; } // "",
        public string PeerCol1 { get; set; } // "",
        public string PeerCol2 { get; set; } // "",
        public string PeerCol3 { get; set; } // "0.0000",
        public string PeerCol4 { get; set; } // "",
        public string PeerCol5 { get; set; } // "",
        public string PeerCol6 { get; set; } // "",
        public string PeerCol7 { get; set; } // "",
        public string PeerCol8 { get; set; } // "",
        public string PeerCol9 { get; set; } // "",
        public string PeerCol10 { get; set; } // "",
        public string SelectedTitle { get; set; } // "Dying in the hospital during or after a procedure to open up blocked vessels in the heart (angioplasty)",
        public string PlainTitle { get; set; } // "Dying in the hospital during or after a procedure to open up blocked vessels in the heart (angioplasty)",
        public string ClinicalTitle { get; set; } // "Percutaneous Transluminal Coronary Angioplasty (PTCA) Mortality Rate",
        public string MeasureDescription { get; set; } // "Deaths in the hospital following a percutaneous transluminal coronary angioplasty, or PTCA, a surgery in which clogged arteries of the heart are opened up and then kept open using wire mesh tubes or <i>stents</i>.",
        public string Bullets { get; set; } // "<ul><li>A lower score is better.</li><li>The rate takes into account how sick patients were before they went to the hospital (it is risk-adjusted).</li><li>Ratings include a significance test that makes us more confident the hospital rating is accurate.</li><li>Figures presented are events per 100 cases.</li></ul>",
        public string StatisticsAvailable { get; set; } // "Numerator, Denominator, Observed Rate and CI, Expected Rate, Risk-Adjusted Rate and CI",
        public string MoreInformation { get; set; } // null,
        public string URL { get; set; } // null,
        public string URLTitle { get; set; } // "",
        public string DataSourceURL { get; set; } // "http://www.qualityindicators.ahrq.gov/",
        public string DataSourceURLTitle { get; set; } // "AHRQ Quality Indicator"
    }
}