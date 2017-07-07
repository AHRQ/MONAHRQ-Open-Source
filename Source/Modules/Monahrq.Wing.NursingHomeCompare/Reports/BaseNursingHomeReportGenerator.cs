using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Services.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NHibernate;
using System.Data.SqlClient;
using Monahrq.Infrastructure.Utility;
using Monahrq.Wing.NursingHomeCompare.NHC;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using NHibernate.Criterion;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Wing.NursingHomeCompare.Reports
{
    [Export("BaseNursingHomeReportGenerator", typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new[] { "87E04110-46B0-4CAE-9592-022C3111FAC7", "BA52B7B2-F4C8-4831-B910-1D036B94AE75", "F2F2B7FE-8653-488B-8ED8-FD4417CD0F9E" },
                     new[] { "Nursing Home Compare Data" },
                     new[] { typeof(NursingHomeTarget) },
                     2, null, "Nursing Home")]
    public class BaseNursingHomeReportGenerator : BaseReportGenerator, IReportGenerator
    {

        #region Fields and Constants

        private List<NursingHome> _websiteNursingHomes = new List<NursingHome>();
        private List<NHCAHPSMeasureLookup> _questionTypes;
        private List<string> compositeMeasureIds = new List<string> { "NH_COMP_OVERALL", "NH_COMP_01", "NH_COMP_02", "NH_COMP_03", "NH_COMP_04", "NH_COMP_05" };

        #endregion

        #region Properties

        //[Import]
        //public IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// A temporary in memory repository of the output of a ReportGenerator run.
        /// Key = NursingHomeID
        /// Value = List of ReportMeasure Data.
        /// </summary>
        internal Dictionary<String, List<NursingHomeReportMeasure>> TempNursingHomeReportMeasuresByNursingHome { get; set; }
        /// <summary>
        /// A temporary in memory repository of the output of a ReportGenerator run.
        /// Key = MeasureID
        /// Value = List of ReportMeasure Data.
        /// </summary>
        internal Dictionary<int, List<NursingHomeReportMeasure>> TempNursingHomeReportMeasuresByMeasure { get; set; }

        #endregion

        public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            if (base.ValidateDependencies(website, validationResults))
            {
                var dataset = CurrentWebsite.Datasets.FirstOrDefault(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Nursing Home Compare Data"));
                if (dataset == null)
                {
                    validationResults.Add(new ValidationResult(base.ActiveReport.Name + " could not be generated due to \"Nursing Home Compare Data\" not selected."));
                }

                if (base.CurrentWebsite.NursingHomes == null || !base.CurrentWebsite.NursingHomes.Any())
                {
                    validationResults.Add(new ValidationResult("Nursing home reports could not be generated due to no Nursing homes being selected when configuring website."));
                }
            }

            return validationResults == null || validationResults.Count == 0;
        }

        protected override bool LoadReportData()
        {
            _websiteNursingHomes = CurrentWebsite.NursingHomes.Any()
                                                 ? CurrentWebsite.NursingHomes.Select(nh => nh.NursingHome).ToList()
                                                 : new List<NursingHome>();

            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                _questionTypes = session.QueryOver<NHCAHPSMeasureLookup>().List().ToList();
            }
            return true;
        }

        protected override bool OutputDataFiles()
        {
            // Creating the Nursing home provider id, address, geo base files
            //GenerateNursingHomeLatLogLookup();

            DataTable tbNursingHomeList = new DataTable("stringIdentifiers");
            tbNursingHomeList.Columns.Add("Id", typeof(string));
            _websiteNursingHomes.DistinctBy(nh => nh.ProviderId).ToList().ForEach(nh => tbNursingHomeList.Rows.Add(nh.ProviderId));


            var nhWebsiteDataset = base.CurrentWebsite.Datasets.First(d => d.Dataset.ContentType.Name == "Nursing Home Compare Data");
            String reportProcessVersion = nhWebsiteDataset.Dataset.ReportProcessVersion;
            if (reportProcessVersion == null) reportProcessVersion = "spInitNursingHomeReport_V1";
            else if (reportProcessVersion == "Version.1") reportProcessVersion = "spInitNursingHomeReport_V1";
            else if (reportProcessVersion == "Version.2") reportProcessVersion = "spInitNursingHomeReport_V2";
			else if (reportProcessVersion == "Version.3") reportProcessVersion = "spInitNursingHomeReport_V3";
			else reportProcessVersion = "spInitNursingHomeReport_V1";

            RunSproc(reportProcessVersion, "InitNursingHomeReport",
                new KeyValuePair<string, object>("@NursingHomes", tbNursingHomeList),
                new KeyValuePair<string, object>("@DatasetID", nhWebsiteDataset.Dataset.Id)
                );

            var baseNursingHomeProfilesDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base", "NursingHomeProfiles");
            Directory.CreateDirectory(baseNursingHomeProfilesDir);

            var baseNursingHomeMeasuresDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base", "NursingHomeMeasures");
            Directory.CreateDirectory(baseNursingHomeMeasuresDir);

            GenerateBaseNursingHomeProfile(CurrentWebsite, baseNursingHomeProfilesDir);
            GenerateBaseNursingHomeMeasureDescriptions(CurrentWebsite, baseNursingHomeMeasuresDir);
            GenerateNursingHomeMeasureTopics(CurrentWebsite, baseNursingHomeMeasuresDir);
            GenerateNursingHomeCounties(CurrentWebsite);
            GenerateNursingHomeIndex(CurrentWebsite);
            GenerateNursingHomeTypes(CurrentWebsite);

            Directory.CreateDirectory(Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "NursingHomes"));

            var nursingHomesDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "NursingHomes", "NursingHomes");
            Directory.CreateDirectory(nursingHomesDir);
            GenerateNursingHomes(CurrentWebsite, nursingHomesDir);

            var measuresDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "NursingHomes", "Measures");
            Directory.CreateDirectory(measuresDir);
            GenerateMeasures(CurrentWebsite, measuresDir);

            return true;
        }

        //private void GenerateNursingHomeLatLogLookup()
        //{
        //    var baseDir = Path.Combine(base.CurrentWebsite.OutPutDirectory, "Data", "Base");

        //    if (!Directory.Exists(baseDir))
        //        Directory.CreateDirectory(baseDir);

        //    IList<ProviderLatLogLookUp> list = new List<ProviderLatLogLookUp>();
        //    using(var session = base.DataProvider.SessionFactory.OpenStatelessSession())
        //    {
        //        list = session.CreateCriteria<NHProviderToLatLong>()
        //                      .Add(Restrictions.In(Projections.Property("ProviderId"),
        //                                           this._websiteNursingHomes.Select(nh => nh.ProviderId).ToList()))
        //                                          .SetProjection(Projections.ProjectionList()
        //                                                               .Add(Projections.Alias(Projections.Property("ProviderId"), "ProviderId"))
        //                                                               .Add(Projections.Alias(Projections.Property("Latitude"), "Latitude"))
        //                                                               .Add(Projections.Alias(Projections.Property("Longitude"), "Longitude")))
        //                      .SetResultTransformer(new AliasToBeanResultTransformer(typeof(ProviderLatLogLookUp)))
        //                      .List<ProviderLatLogLookUp>();
        //    }

        //    base.GenerateJsonFile(list.ToList(), Path.Combine(baseDir, "NuringsHomeProviderLatLong.js"), "$.monahrq.NursingHome.Base.ProviderLatLong=");            
        //}

        public override void InitGenerator()
        {
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Nursing home reports" });

            OnInstalled();

        }

        public void OnInstalled()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            //base.OnWingAdded();
            string[] scriptFiles ={
                                      "NursingHomeMeasureCodeMapping.sql",
                                      "spInitNursingHomeReport.Version.1.sql",
                                      "spInitNursingHomeReport.Version.2.sql",
									  "spInitNursingHomeReport.Version.3.sql",
									  "spGetNursingHomeReport.sql",
                                      "fnFindCountyID.sql"
                                  };

            RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\NursingHome"), scriptFiles);
        }

        private void GenerateNursingHomes(Website website, string nursingHomesDir)
        {
            //var selectedMeasuresCSV = string.Join(",", website.Measures.Where(m => m.IsSelected).Select(m => m.ReportMeasure.Id).ToArray());
            var measureCodeQuery = @"
select distinct m.id ,m.name,nhmm.AccessCode from
[dbo].[NursingHomeMeasureCodeMapping] nhmm
left join [dbo].[Measures] m on m.Name=nhmm.MonahrqCode
";
            //where m.id in (" + selectedMeasuresCSV + ")";
            var measureCodeQueryResults = RunSqlReturnDataTable(measureCodeQuery, null);

            var measureCodeMap = new Dictionary<string, int>();
            foreach (DataRow item in measureCodeQueryResults.Rows)
            {
                if (measureCodeMap.ContainsKey(item["AccessCode"].ToString())) continue;

                measureCodeMap.Add(item["AccessCode"].ToString(), int.Parse(item["id"].ToString()));
            }

            var targetQuery = string.Format(@"
SELECT distinct nh.id as nhid,tnh.*
  FROM 
  [Targets_NursingHomeTargets] as tnh 
  left join [NursingHomes] as nh on nh.ProviderId=tnh.ProviderId
inner join [Websites_WebsiteNursingHomes] snh on snh.NursingHome_ProviderId=nh.ProviderId
  where
  nh.[State] in ('{0}')
and tnh.[Dataset_Id]={1}
and snh.Website_Id={2}
",
 string.Join("','", website.SelectedReportingStates)
 , website.Datasets.First(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Nursing Home Compare Data")).Dataset.Id
 , website.Id);

            var rs = RunSqlReturnDataTable(targetQuery, null);

            //var nursingHomes = new Dictionary<String, List<NursingHomeReportMeasure>>();
            var nursingHomes = (TempNursingHomeReportMeasuresByNursingHome == null) ?
                TempNursingHomeReportMeasuresByNursingHome = new Dictionary<String, List<NursingHomeReportMeasure>>() :
                TempNursingHomeReportMeasuresByNursingHome;
            nursingHomes.Clear();

            foreach (DataRow item in rs.Rows)
            {
                string id = item["nhid"].ToString();
                List<NursingHomeReportMeasure> nh = null;
                if (nursingHomes.ContainsKey(id))
                {
                    nh = nursingHomes[id];
                }
                else
                {
                    nh = new List<NursingHomeReportMeasure>();
                    nursingHomes[id] = nh;
                }

                foreach (DataColumn c in rs.Columns)
                {
                    if (c.ColumnName.StartsWith("QM") && c.ColumnName.EndsWith("Score"))
                    {
                        int mid = measureCodeMap[c.ColumnName.Substring(0, 5)];
                        nh.Add(new NursingHomeReportMeasure()
                        {
                            NursingHomeID = int.Parse(id),
                            MeasureID = mid,
                            Rate = item[c.ColumnName].ToString(),
                            NatRating = "-",
                            NatRate = item[c.ColumnName.Replace("Score", "Nation")].ToString(),

                            PeerRating = "-",
                            PeerRate = item[c.ColumnName.Replace("Score", "State")].ToString(),
                            CountyRating = "-"
                        });
                    }
                    else
                    {
                        if (c.ColumnName == "ReportedCNAStaffingHoursperResidentperDay"
                           || c.ColumnName == "ReportedLPNStaffingHoursperResidentperDay"
                           || c.ColumnName == "ReportedRNStaffingHoursperResidentperDay"
                           || c.ColumnName == "LicensedStaffingHoursperResidentperDay"
                           || c.ColumnName == "TotalNurseStaffingHoursperResidentperDay"
                           || c.ColumnName == "PhysicalTherapistStaffingHoursperResidentPerDay"
                           || c.ColumnName == "HealthSurveyDate"
                           || c.ColumnName == "FireSafetySurveyDate"
                           || c.ColumnName == "TotalHealthDeficiencies"
                           || c.ColumnName == "TotalFireSafetyDeficiencies"
                           )
                        {

                            int mid = measureCodeMap[c.ColumnName];
                            nh.Add(new NursingHomeReportMeasure()
                            {
                                NursingHomeID = int.Parse(id),
                                MeasureID = mid,
                                Rate = item[c.ColumnName].ToString(),
                                NatRating = "-",
                                NatRate = item[c.ColumnName + "Nation"].ToString(),
                                PeerRating = "-",
                                PeerRate = item[c.ColumnName + "State"].ToString(),
                                CountyRating = "-"
                            });

                        }

                    }
                }
            }

            DataTable tbCountiesList = new DataTable("stringIdentifiers");
            tbCountiesList.Columns.Add("Id", typeof(string));
            website.NursingHomes.Select(nh => nh.NursingHome.CountyName).Distinct().ToList().ForEach(c => tbCountiesList.Rows.Add(c));

            DataTable tbStatesList = new DataTable("stringIdentifiers");
            tbStatesList.Columns.Add("Id", typeof(string));
            website.SelectedReportingStates.ForEach(s => tbStatesList.Rows.Add(s));

            DataTable tbNursingHomeList = new DataTable("stringIdentifiers");
            tbNursingHomeList.Columns.Add("Id", typeof(string));
            website.NursingHomes.ToList().ForEach(nh => tbNursingHomeList.Rows.Add(nh.NursingHome.ProviderId));


            var overallrs = RunSprocReturnDataTable("spGetNursingHomeReport",
                new KeyValuePair<string, object>("@States", tbStatesList),
                new KeyValuePair<string, object>("@Counties", tbCountiesList),
                new KeyValuePair<string, object>("@NursingHomes", tbNursingHomeList),
                new KeyValuePair<string, object>("@DatasetID", website.Datasets.First(d => d.Dataset.ContentType.Name == "Nursing Home Compare Data").Dataset.Id)
                );

            var overallMesureNames = new string[] { "OverallRating", "qualityrating", "staffingrating", "surveyrating" };
            foreach (DataRow ovaitem in overallrs.Rows)
            {
                string id = ovaitem["nhid"].ToString();
                List<NursingHomeReportMeasure> nh;
                if (nursingHomes.ContainsKey(id))
                {
                    nh = nursingHomes[id];
                }
                else
                {
                    nh = new List<NursingHomeReportMeasure>();
                    nursingHomes[id] = nh;
                }

                foreach (var mn in overallMesureNames)
                {
                    nh.Add(new NursingHomeReportMeasure()
                    {
                        NursingHomeID = int.Parse(id),
                        MeasureID = measureCodeMap[mn],
                        Rate = "-",
                        NatRating = ovaitem[mn].ToString(),
                        NatRate = "-",
                        PeerRating = overallrs.Columns.Contains(mn + " Comparison 5pt") ? ovaitem[mn + " Comparison 5pt"].ToString() : "-",
                        PeerRate = "-",
                        CountyRating = "-"
                    });
                }
            }

            foreach (var countyName in website.NursingHomes.Select(nh => nh.NursingHome.CountyName).Distinct())
            {
                tbCountiesList = new DataTable("stringIdentifiers");
                tbCountiesList.Columns.Add("Id", typeof(string));
                tbCountiesList.Rows.Add(countyName);

                var countyOverallrs = RunSprocReturnDataTable("spGetNursingHomeReport",
                    new KeyValuePair<string, object>("@States", tbStatesList),
                    new KeyValuePair<string, object>("@Counties", tbCountiesList),
                    new KeyValuePair<string, object>("@NursingHomes", tbNursingHomeList),
                    new KeyValuePair<string, object>("@DatasetID", website.Datasets.First(d => d.Dataset.ContentType.Name == "Nursing Home Compare Data").Dataset.Id)
                    );


                var countyOverallMesureNames = new[] { "qualityrating", "staffingrating", "surveyrating" };
                foreach (DataRow countyOvaitem in countyOverallrs.Rows)
                {
                    string id = countyOvaitem["nhid"].ToString();
                    List<NursingHomeReportMeasure> nh;
                    if (nursingHomes.ContainsKey(id))
                    {
                        nh = nursingHomes[id];
                    }
                    else
                    {
                        nh = new List<NursingHomeReportMeasure>();
                        nursingHomes[id] = nh;
                    }

                    foreach (var mn in countyOverallMesureNames)
                    {
                        var countyOverallMeasure = nh.Find(m => m.NursingHomeID == int.Parse(id) && m.MeasureID == measureCodeMap[mn]);
                        countyOverallMeasure.CountyRating = overallrs.Columns.Contains(mn + " Comparison 5pt") ? countyOvaitem[mn + " Comparison 5pt"].ToString() : "-";
                    }
                }
            }

            foreach (var item in nursingHomes.Keys)
            {
                //Remove all Measures that have been excluded 
                var nhrm = nursingHomes[item];
                var excludedWebsiteMeasures = website.Measures.Where(m => !m.IsSelected).Select(m => m);
                foreach (var excWebMeasure in excludedWebsiteMeasures)
                {
                    int excludedMeasureId;
                    if (excWebMeasure.OverrideMeasure != null)
                        excludedMeasureId = excWebMeasure.OverrideMeasure.Id;
                    else
                        excludedMeasureId = excWebMeasure.OriginalMeasure.Id;

                    var nhExcMeas = nhrm.FirstOrDefault(m => m.MeasureID == excludedMeasureId);
                    if (nhExcMeas != null) nhrm.Remove(nhExcMeas);

                }
                //Dump the website's measure files
                JsonHelper.GenerateJsonFile(nursingHomes[item], nursingHomesDir + "\\NursingHome_" + item + ".js", "$.monahrq.NursingHomes.Report.NursingHomes['" + item + "']=");
            }
        }

        private void GenerateMeasures(Website website, string measureDir)
        {
            //            var measureCodeQuery = @"
            //select distinct m.id ,m.name,nhmm.AccessCode from
            //[dbo].[NursingHomeMeasureCodeMapping] nhmm
            //left join [dbo].[Measures] m on m.Name=nhmm.MonahrqCode
            //";
            var measureCodeQuery = @"
SELECT DISTINCT wm_m.Id, wm_m.Name, nhmm.AccessCode, nhmm.MonahrqCode
FROM    NursingHomeMeasureCodeMapping AS nhmm LEFT OUTER JOIN
    (SELECT  wm.IsSelected,  m.Id, m.ClassType, m.Name, m.MeasureType, m.Source, m.Description, m.MoreInformation, m.Footnotes, m.NationalBenchmark, m.HigherScoresAreBetter, m.ScaleBy, m.ScaleTarget, 
        m.RiskAdjustedMethod, m.RateLabel, m.NQFEndorsed, m.NQFID, m.SuppressionDenominator, m.SuppressionNumerator, m.PerformMarginSuppression, m.UpperBound, m.LowerBound, m.Url, 
        m.UrlTitle, m.IsOverride, m.IsLibEdit, m.UsedInCalculations, m.Target_id, m.ClinicalTitle, m.PlainTitle, m.PolicyTitle, m.SelectedTitle, m.ProvidedBenchmark, m.CalculationMethod
    FROM    (SELECT Website_Id, IsSelected, CASE WHEN OverrideMeasure_Id IS NULL THEN OriginalMeasure_Id ELSE OverrideMeasure_Id END AS Measure_Id
        FROM            Websites_WebsiteMeasures
        WHERE        (Website_Id = {0}) ) AS wm INNER JOIN
        Measures AS m ON wm.Measure_Id = m.Id) AS wm_m ON wm_m.Name = nhmm.MonahrqCode
WHERE        (wm_m.Id IS NOT NULL) 
AND (IsSelected = 1 or nhmm.MonahrqCode = 'NH-OA-01')
";
            measureCodeQuery = string.Format(measureCodeQuery, website.Id);
            var measureCodeQueryResults = RunSqlReturnDataTable(measureCodeQuery, null);

            var measureCodeMap = new Dictionary<string, int>();
            foreach (DataRow item in measureCodeQueryResults.Rows)
            {
                if (measureCodeMap.ContainsKey(item["AccessCode"].ToString())) continue;

                measureCodeMap.Add(item["AccessCode"].ToString(), int.Parse(item["id"].ToString()));
            }

            var targetQuery = string.Format(@"
SELECT distinct nh.id as nhid, tnh.*
  FROM 
  [Targets_NursingHomeTargets] as tnh 
  left join [NursingHomes] as nh on nh.ProviderId=tnh.ProviderId
  where
  nh.[State] in ('{0}')
and tnh.[Dataset_Id]={1}
", string.Join("','", website.SelectedReportingStates)
  , website.Datasets.First(d => d.Dataset.ContentType.Name == "Nursing Home Compare Data").Dataset.Id);

            var rs = RunSqlReturnDataTable(targetQuery, null);

            var measurs = (TempNursingHomeReportMeasuresByMeasure == null)
                ? TempNursingHomeReportMeasuresByMeasure = new Dictionary<int, List<NursingHomeReportMeasure>>()
                : TempNursingHomeReportMeasuresByMeasure;
            measurs.Clear();

            foreach (DataRow item in rs.Rows)
            {
                foreach (DataColumn c in rs.Columns)
                {
                    if (c.ColumnName.StartsWith("QM") && c.ColumnName.EndsWith("Score"))
                    {
                        int id = measureCodeMap[c.ColumnName.Substring(0, 5)];
                        List<NursingHomeReportMeasure> m;
                        if (measurs.ContainsKey(id))
                        {
                            m = measurs[id];
                        }
                        else
                        {
                            m = new List<NursingHomeReportMeasure>();
                            measurs[id] = m;
                        }

                        m.Add(new NursingHomeReportMeasure
                        {
                            NursingHomeID = int.Parse(item["nhid"].ToString()),
                            MeasureID = id,
                            Rate = item[c.ColumnName].ToString(),
                            NatRating = "-",
                            NatRate = item[c.ColumnName.Replace("Score", "Nation")].ToString(),

                            PeerRating = "-",
                            PeerRate = item[c.ColumnName.Replace("Score", "State")].ToString(),
                            CountyRating = "-"
                        });
                    }
                    else
                    {
                        if (c.ColumnName == "ReportedCNAStaffingHoursperResidentperDay"
                            || c.ColumnName == "ReportedLPNStaffingHoursperResidentperDay"
                            || c.ColumnName == "ReportedRNStaffingHoursperResidentperDay"
                            || c.ColumnName == "LicensedStaffingHoursperResidentperDay"
                            || c.ColumnName == "TotalNurseStaffingHoursperResidentperDay"
                            || c.ColumnName == "PhysicalTherapistStaffingHoursperResidentPerDay"
                            || c.ColumnName == "HealthSurveyDate"
                            || c.ColumnName == "FireSafetySurveyDate"
                            || c.ColumnName == "TotalHealthDeficiencies"
                            || c.ColumnName == "TotalFireSafetyDeficiencies"
                            )
                        {

                            int id = measureCodeMap[c.ColumnName];
                            List<NursingHomeReportMeasure> m;
                            if (measurs.Keys.Contains(id))
                            {
                                m = measurs[id];
                            }
                            else
                            {
                                m = new List<NursingHomeReportMeasure>();
                                measurs[id] = m;
                            }

                            m.Add(new NursingHomeReportMeasure
                            {
                                NursingHomeID = int.Parse(item["nhid"].ToString()),
                                MeasureID = id,
                                Rate = item[c.ColumnName].ToString(),
                                NatRating = "-",
                                NatRate = item[c.ColumnName + "Nation"].ToString(),
                                PeerRating = "-",
                                PeerRate = item[c.ColumnName + "State"].ToString(),
                                CountyRating = "-"
                            });
                        }
                    }
                }
            }


            DataTable tbCountiesList = new DataTable("stringIdentifiers");
            tbCountiesList.Columns.Add("Id", typeof(string));
            website.NursingHomes.Select(nh => nh.NursingHome.CountyName).Distinct().ToList().ForEach(c => tbCountiesList.Rows.Add(c));

            DataTable tbStatesList = new DataTable("stringIdentifiers");
            tbStatesList.Columns.Add("Id", typeof(string));
            website.SelectedReportingStates.ForEach(s => tbStatesList.Rows.Add(s));


            DataTable tbNursingHomeList = new DataTable("stringIdentifiers");
            tbNursingHomeList.Columns.Add("Id", typeof(string));
            website.NursingHomes.ToList().ForEach(nh => tbNursingHomeList.Rows.Add(nh.NursingHome.ProviderId));


            var overallrs = RunSprocReturnDataTable("spGetNursingHomeReport",
                new KeyValuePair<string, object>("@States", tbStatesList),
                new KeyValuePair<string, object>("@Counties", tbCountiesList),
                new KeyValuePair<string, object>("@NursingHomes", tbNursingHomeList),
                new KeyValuePair<string, object>("@DatasetID", website.Datasets.First(d => d.Dataset.ContentType.Name == "Nursing Home Compare Data").Dataset.Id)
                );

            var overallMesureNames = new[] { "OverallRating", "qualityrating", "staffingrating", "surveyrating" };
            foreach (DataRow ovaitem in overallrs.Rows)
            {
                foreach (var mn in overallMesureNames)
                {
                    //int id = measureCodeMap[mn];
                    int id;
                    //Continue if MeasureName not found (most likely because it was excluded by operator
                    if (!measureCodeMap.TryGetValue(mn, out id))
                        continue;

                    List<NursingHomeReportMeasure> m;
                    if (measurs.ContainsKey(id))
                    {
                        m = measurs[id];
                    }
                    else
                    {
                        m = new List<NursingHomeReportMeasure>();
                        measurs[id] = m;
                    }

                    m.Add(new NursingHomeReportMeasure
                    {
                        NursingHomeID = int.Parse(ovaitem["nhid"].ToString()),
                        MeasureID = id,
                        Rate = "-",
                        NatRating = ovaitem[mn].ToString(),
                        NatRate = "-",
                        PeerRating = overallrs.Columns.Contains(mn + " Comparison 5pt") ? ovaitem[mn + " Comparison 5pt"].ToString() : "-",
                        PeerRate = "-",
                        CountyRating = "-"
                    });
                }
            }

            foreach (var countyName in website.NursingHomes.Select(nh => nh.NursingHome.CountyName).Distinct())
            {
                tbCountiesList = new DataTable("stringIdentifiers");
                tbCountiesList.Columns.Add("Id", typeof(string));
                tbCountiesList.Rows.Add(countyName);

                var countyOverallrs = RunSprocReturnDataTable("spGetNursingHomeReport",
                    new KeyValuePair<string, object>("@States", tbStatesList),
                    new KeyValuePair<string, object>("@Counties", tbCountiesList),
                    new KeyValuePair<string, object>("@NursingHomes", tbNursingHomeList),
                    new KeyValuePair<string, object>("@DatasetID", website.Datasets.First(d => d.Dataset.ContentType.Name == "Nursing Home Compare Data").Dataset.Id)
                    );

                var countyOverallMesureNames = new[] { "qualityrating", "staffingrating", "surveyrating" };
                foreach (DataRow countyOvaitem in countyOverallrs.Rows)
                {
                    foreach (var mn in countyOverallMesureNames)
                    {
                        int mid;
                        if (!measureCodeMap.TryGetValue(mn, out mid))
                            continue;

                        List<NursingHomeReportMeasure> mdata;
                        if (measurs.ContainsKey(mid))
                        {
                            mdata = measurs[mid];
                        }
                        else
                        {
                            mdata = new List<NursingHomeReportMeasure>();
                            measurs[mid] = mdata;
                        }

                        var countyOverallMeasure = mdata.Find(m => m.NursingHomeID == int.Parse(countyOvaitem["nhid"].ToString()) && m.MeasureID == mid);
                        countyOverallMeasure.CountyRating = overallrs.Columns.Contains(mn + " Comparison 5pt") ? countyOvaitem[mn + " Comparison 5pt"].ToString() : "-";

                    }
                }
            }
            foreach (var k in measurs.Keys)
            {
                JsonHelper.GenerateJsonFile(measurs[k], measureDir + "\\Measure_" + k + ".js", "$.monahrq.NursingHomes.Report.Measures['" + k + "']=");
            }
        }

        private void GenerateNursingHomeTypes(Website website)
        {

            var nhtl = new List<object>();
            var d = new Dictionary<string, object> { { "TypeID", null }, { "Name", "" } };
            nhtl.Add(d);

            d = new Dictionary<string, object> { { "TypeID", 1 }, { "Name", "Medicare" } };
            nhtl.Add(d);

            d = new Dictionary<string, object> { { "TypeID", 2 }, { "Name", "Medicaid" } };
            nhtl.Add(d);

            d = new Dictionary<string, object> { { "TypeID", 3 }, { "Name", "Medicare and Medicaid" } };
            nhtl.Add(d);

            JsonHelper.GenerateJsonFile(nhtl, Path.Combine(website.OutPutDirectory, "Data", "Base") + "\\NursingHomeTypes.js", "$.monahrq.NursingHomes.Base.NursingHomeTypes=");
        }

        private void GenerateNursingHomeIndex(Website website)
        {
            var nursingHomeIndexQuery = string.Format(

                @"	SELECT DISTINCT	nh.Id AS ID"
                + "				,	nh.Name"
                + "				,	nh.Zip"
                + "				,	nh.CountyName"
                + "				,	nh.City"
                + "				,	nh.State"
                + "				,	dbo.fnFindCountyID('',nh.CountySSA,nh.CountyName,nh.State) AS CountyID"
                + "				,	CASE "
                + "						WHEN nh.[Certification] = 'Medicare' THEN 1"
                + "						WHEN nh.[Certification] = 'Medicaid' THEN 2"
                + "						WHEN nh.[Certification] = 'Medicare and Medicaid' THEN 3"
                + "						ELSE - 1 END AS TypeID"
                + "				,	nh.InHospital"
                + "				,	ISNULL(nhll.[Latitude],0) as Latitude"
                + "				,	ISNULL(nhll.[Longitude],0) as Longitude"
                + " FROM			NursingHomes AS nh"
                + "		left join	Base_NHProviderToLatLongs as nhll on nhll.[ProviderId] = nh.[ProviderId]"
                + " where			nh.[state] in ('{0}')"

                , string.Join("','", website.SelectedReportingStates));

            var rs = RunSqlReturnDataTable(nursingHomeIndexQuery, null);
            var nl = new List<object>();
            foreach (DataRow item in rs.Rows)
            {
                var l = new Dictionary<string, object>();
                double latitude = 0;
                double longitude = 0;
                foreach (var c in rs.Columns)
                {
                    if (c.ToString() == "Latitude") { latitude = item[c.ToString()].AsDouble(0); continue; }
                    if (c.ToString() == "Longitude") { longitude = item[c.ToString()].AsDouble(0); continue; }

                    l.Add(c.ToString(), item[c.ToString()]);
                }

                l.Add("LatLng", new[] { latitude, longitude });
                nl.Add(l);
            }

            JsonHelper.GenerateJsonFile(nl, Path.Combine(website.OutPutDirectory, "Data", "Base") + "\\NursingHomeIndex.js", "$.monahrq.NursingHomes.Base.NursingHomeIndex=");
        }

        private void GenerateNursingHomeCounties(Website website)
        {
            var nursingHomeCountiesQuery = string.Format(@"
SELECT  distinct
    bc.id as  CountyID,
    bc.Name as  CountyName,
    bc.CountyFIPS as FIPS
FROM 
    [NursingHomes] as nh
    left join Base_Counties as bc on bc.[CountySSA]=nh.[CountySSA] and bc.[State]=nh.[State]
where bc.id is not null and
    nh.[state] in ('{0}')", string.Join("','", website.SelectedReportingStates));

            var rs = RunSqlReturnDataTable(nursingHomeCountiesQuery, null);
            var cl = new List<object>();
            foreach (DataRow item in rs.Rows)
            {
                var l = new Dictionary<string, object>();
                foreach (var c in rs.Columns)
                {
                    l.Add(c.ToString(), item[c.ToString()]);
                }
                cl.Add(l);
            }

            JsonHelper.GenerateJsonFile(cl, Path.Combine(website.OutPutDirectory, "Data", "Base") + "\\NursingHomeCounties.js", "$.monahrq.NursingHomes.Base.NursingHomeCounties=");
        }

        private void GenerateNursingHomeMeasureTopics(Website website, string nursingHomeMeasuresDir)
        {
            var healthInspectionMeasure = new NursingHomeMeasureStruct(website, ConfigurationService.ConnectionSettings.ConnectionString, "(Name = 'NH-HI-01' OR MeasureType = 'Health Inspection') AND MeasureType <> 'Survey Summary'", "NH-HI-01");
            var qualityMeasure = new NursingHomeMeasureStruct(website, ConfigurationService.ConnectionSettings.ConnectionString, "(Name = 'NH-QM-01' OR MeasureType IN ('Quality Measures Short Stay', 'Quality Measures Long Stay'))", "NH-QM-01");
            var staffingMeasure = new NursingHomeMeasureStruct(website, ConfigurationService.ConnectionSettings.ConnectionString, "(Name = 'NH-SD-01' OR MeasureType = 'Staffing')", "NH-SD-01");


            List<string> overallMeasures = GetNursingHomeCAHPSMeasures(website);
            if (!string.IsNullOrEmpty(healthInspectionMeasure.OverallMeasureId))
            {
                overallMeasures.Add(CreateMeasureTopic(int.Parse(healthInspectionMeasure.TopicId), "Health Inspection", "Health Inspection", healthInspectionMeasure.Description,
                   int.Parse(healthInspectionMeasure.OverallMeasureId), healthInspectionMeasure.RelatedMeasures, false));
            }

            if (!string.IsNullOrEmpty(qualityMeasure.OverallMeasureId))
            {
                var measureTopic = CreateMeasureTopic(int.Parse(qualityMeasure.TopicId), "Quality Measures", "Quality Measures", qualityMeasure.Description,
                   int.Parse(qualityMeasure.OverallMeasureId), qualityMeasure.RelatedMeasures, true, false);
                measureTopic += ","
                    + string.Format("\"OverallMeasure\" : {0},", qualityMeasure.OverallMeasureId)
                    + "\"GroupingModalsContent\" : {"
                    + "\"InScore\": \"These measures were used to calculate the “summary score” for (insert as appropriate) Inspections, Quality, Staffing OR These measures were used to calculate the Overall Summary Score\","
                    + "\"NotInScore\" : \"These measures were NOT used to calculate the “summary score” for (insert as appropriate) Inspections, Quality, Staffing OR These measures were NOT used to calculate the Overall Summary Score Note:  The wireframe is a bit confusing here, since it appears there is only one spot where one can click for either definition.  Will this be a definition for each and every measure?  If not, do we need to actually identify, in the definition, the measures included and not included?\","
                    + "\"LongStay\" : \"Many nursing home residents stay for quite a long time, more than 100 days.  Long stay residents typically enter a nursing home with several health problems.  They also need help with some common daily tasks, like bathing, getting dressed, eating, walking, or moving from a bed to a chair.  Quality for these residents means making sure that they have their daily needs met, don’t develop new problems, and don’t have problems that get worse.  The measures for long stay residents focus on these issues.  \","
                    + "\"ShortStay\" : \"Some residents stay in the nursing home for 100 or fewer days. Short stay residents typically come to a nursing home to recover from an injury, illness or surgery.  Often, they were just in the hospital and expect to return home to live on their own.  Quality for these residents means they make progress toward independence, don’t develop new problems, and don’t have problems that get worse.  The measures for short stay residents focus on these issues.   \""
                    + "}}";
                overallMeasures.Add(measureTopic);
            }

            if (!string.IsNullOrEmpty(staffingMeasure.OverallMeasureId))
            {
                overallMeasures.Add(CreateMeasureTopic(int.Parse(staffingMeasure.TopicId), "Staffing", "Staffing", staffingMeasure.Description,
                   int.Parse(staffingMeasure.OverallMeasureId), staffingMeasure.RelatedMeasures, false));
            }

            var overMeasuresCSV = String.Join(", ", overallMeasures.ToArray());
            var measureTopics = "$.monahrq.NursingHomes.Base.MeasureTopics = [" + overMeasuresCSV + "];";
            var measureTopicsConsumer = "$.monahrq.NursingHomes.Base.MeasureTopicsConsumer = [" + overMeasuresCSV + "];";

            WriteToFile(nursingHomeMeasuresDir + "\\MeasureTopics.js", measureTopics);
            if (website.HasConsumersAudience) WriteToFile(nursingHomeMeasuresDir + "\\MeasureTopicsConsumer.js", measureTopicsConsumer);

        }

        private static void WriteToFile(string filename, string content)
        {
            using (var sw = new StreamWriter(filename))
            {
                sw.Write(content);
                sw.Close();
            }
        }

        private void GenerateBaseNursingHomeMeasureDescriptions(Website website, string nursingHomeMeasuresDir)
        {
            var nursingHomeMeasuresQuery = @"
	select			m.id as MeasureID
					,	case 
						when m.Name in ('NH_COMP_01', 'NH_COMP_02', 'NH_COMP_03', 'NH_COMP_04', 'NH_COMP_05') then replace(m.Name, 'NH_COMP_','') 
						when m.Source  = 'NH-CAHPS survey' and m.Name != 'NH_COMP_OVERALL' then (select top 1 replace(mx.Name, 'NH_COMP_','')  from Measures mx where mx.MeasureType = m.MeasureType and mx.Name like 'NH_COMP_%')
						else  mt.Topic_Id end as TopicID
				,	m.name as MeasuresName
				,	m.Source as MeasureSource
				,	MeasureType
				,	HigherScoresAreBetter
				,	'' as HigherScoresAreBetterDescription
				,	m.[UsedInCalculations] as InScore
				,	'Nationwide Mean' as NatLabel
				,	m.NationalBenchmark as NatRate
				,	'State Mean' as PeerLabel
				,	m.ProvidedBenchmark as PeerRate
				,	'County Mean' as CountyLabel
				,	null as  CountyRate
				,	m.Footnotes as Footnote
				,	PlainTitle as SelectedTitle
				,	PlainTitle
				,	ClinicalTitle
				,	m.[Description] as  MeasureDescription

				,	ConsumerPlainTitle as SelectedTitleConsumer
				,	ConsumerPlainTitle as PlainTitleConsumer
				,	[ConsumerDescription] AS MeasureDescriptionConsumer
				
				,	null as Bullets
				,	null as StatisticsAvailable
				,	MoreInformation
				,	URL
				,	URLTitle
				,	m.Url as DataSourceURL
				,	m.UrlTitle as DataSourceURLTitle
	FROM			Measures AS m
		INNER JOIN	(
						select			IsSelected
									,	case 
											when OverrideMeasure_Id is null then OriginalMeasure_Id
											else OverrideMeasure_Id end
										as Measure_Id 
						from			Websites_WebsiteMeasures
						WHERE			Website_Id ={0}
					) as W ON m.Id = W.Measure_Id 
		LEFT JOIN	Measures_MeasureTopics AS mt ON mt.Measure_Id = m.Id
		where		m.ClassType='NURSINGHOME'
			AND		(W.IsSelected = 1 OR m.Name = 'NH-OA-01')
";
            nursingHomeMeasuresQuery = string.Format(nursingHomeMeasuresQuery, website.Id);
            var rs = RunSqlReturnDataTable(nursingHomeMeasuresQuery, null);
            foreach (DataRow item in rs.Rows)
            {
                var l = new Dictionary<string, object>();
                var questionType = _questionTypes.FirstOrDefault(x => x.MeasureId.Replace("_Rate", "").Contains(item["MeasuresName"].ToString()));

                foreach (var c in rs.Columns)
                {
                    l.Add(c.ToString(), item[c.ToString()]);
                }

                l.Add("CAHPSQuestionType", questionType != null ? questionType.CAHPSQuestionType : null);

                JsonHelper.GenerateJsonFile(l, nursingHomeMeasuresDir + "\\MeasureDescription_" + item["MeasureID"] + ".js", "$.monahrq.NursingHomes.Base.MeasureDescriptions['" + item["MeasureID"] + "']=");

            }
        }

        private void GenerateBaseNursingHomeProfile(Website website, string nursingHomeProfilesDir)
        {
            var nursingHomeProfilesQuery = string.Format(@"
	SELECT			nh.ID as ID
				,	nh.Name
				,	[Address]
				,	City
				,	nh.[State]
				,	Zip
				,	dbo.fnFindCountyID('',nh.CountySSA, nh.CountyName, nh.[State]) as  CountyID
				,	[Address] + '' + City + ', ' + nh.[State] + ' ' + Zip as DisplayAddress
				,	[Description]
				,	[Phone] as PhoneNumber
				,	case 
						when [Certification]='Medicare' then 1
						when [Certification]='Medicaid' then 2
						when [Certification]='Medicare and Medicaid' then 3
						else -1 end as TypeID
				,	[Ownership] as OwnershipType
				,	[NumberCertBeds] as CertifiedBeds
				,	[NumberResidCertBeds] as ResidentsInCertifiedBeds
				,	nh.[ProviderId] as ProviderType
				,	InHospital
				,	nh.HasSpecialFocus as SpecialFocus
				,	InRetirementCommunity
				,	[ParticipationDate] as DateApprovedMedicareMedicaid
				,	[ChangedOwnership_12Mos] as LastYearOwnershipChange
				,	[ResFamCouncil] as HasCouncil
				,	[SprinklerStatus] as HasSprinkler
				,	Accreditation
				,	ISNULL(nhll.[Latitude],0) as Latitude
				,	ISNULL(nhll.[Longitude],0) as Longitude
	FROM			[NursingHomes] as nh
		left join	Base_Counties as bc on bc.[CountySSA]=nh.[CountySSA] and bc.[State]=nh.[State]
		left join	Base_NHProviderToLatLongs as nhll on nhll.[ProviderId] = nh.[ProviderId]
	where			nh.[state] in ('{0}')",
                                          string.Join("','", website.SelectedReportingStates));


            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                // Get NursingHome data from DB and serialize it to 'dynamic' ExpandoObject.
                var nursingHomeItems = session.CreateSQLQuery(nursingHomeProfilesQuery)
                    .AddScalar("ID", NHibernateUtil.Int32)
                    .AddScalar("Name", NHibernateUtil.String)
                    .AddScalar("Address", NHibernateUtil.String)
                    .AddScalar("City", NHibernateUtil.String)
                    .AddScalar("State", NHibernateUtil.String)
                    .AddScalar("Zip", NHibernateUtil.String)
                    .AddScalar("CountyID", NHibernateUtil.Int32)
                    .AddScalar("DisplayAddress", NHibernateUtil.String)
                    .AddScalar("Description", NHibernateUtil.String)
                    .AddScalar("PhoneNumber", NHibernateUtil.String)
                    .AddScalar("TypeID", NHibernateUtil.Int32)
                    .AddScalar("OwnershipType", NHibernateUtil.String)
                    .AddScalar("CertifiedBeds", NHibernateUtil.Int32)
                    .AddScalar("ResidentsInCertifiedBeds", NHibernateUtil.Int32)
                    .AddScalar("ProviderType", NHibernateUtil.String)
                    .AddScalar("InHospital", NHibernateUtil.Boolean)
                    .AddScalar("SpecialFocus", NHibernateUtil.Boolean)
                    .AddScalar("InRetirementCommunity", NHibernateUtil.Boolean)
                    .AddScalar("DateApprovedMedicareMedicaid", NHibernateUtil.Date)
                    .AddScalar("LastYearOwnershipChange", NHibernateUtil.Boolean)
                    .AddScalar("HasCouncil", NHibernateUtil.String)
                    .AddScalar("HasSprinkler", NHibernateUtil.String)
                    .AddScalar("Accreditation", NHibernateUtil.String)
                    .AddScalar("Latitude", NHibernateUtil.Double)
                    .AddScalar("Longitude", NHibernateUtil.Double)
                    .DynamicList();

                foreach (var item in nursingHomeItems)
                {
                    // Property LatLng added in post process since adding double[] via SQL is not possible.
                    // Remove properties from object not needed to be serialized.
                    // Then serialize object to JSON file.
                    item.LatLng = new Double[] { item.Latitude, item.Longitude };
                    IDictionary<string, object> itemMap = item;
                    itemMap.Remove("Latitude");
                    itemMap.Remove("Longitude");
                    JsonHelper.GenerateJsonFile(itemMap, nursingHomeProfilesDir + "\\Profile_" + item.ID + ".js", "$.monahrq.NursingHomes.Base.Profiles['" + item.ID + "']=");
                }
            }
        }

        private List<string> GetNursingHomeCAHPSMeasures(Website website)
        {
            var result = new List<string>();

            var nursingCAHPSMeasures = website.Measures
                     .Where(m => (m.OverrideMeasure == null && m.OriginalMeasure.Source != null && m.OriginalMeasure.Source.Contains("NH-CAHPS survey")) ||
                     ((m.OverrideMeasure != null && m.OverrideMeasure.Source != null && m.OverrideMeasure.Source.Contains("NH-CAHPS survey"))))
                     .Select(x => x.OverrideMeasure == null ? x.OriginalMeasure : x.OverrideMeasure)
                     .ToList();

            if (!nursingCAHPSMeasures.Any()) return result;

            var topic = nursingCAHPSMeasures.FirstOrDefault(x => x.MeasureTopics.Any()).MeasureTopics.Select(x => x.Topic).FirstOrDefault() ?? new Topic();
            var overallMeasure = nursingCAHPSMeasures.FirstOrDefault(x => x.MeasureCode.Contains("NH_COMP_OVERALL"));
            if (overallMeasure != null)
            {
                //    result.Add(CreateMeasureTopic(topic.Id, topic.Name, topic.LongTitle, topic.Description, overallMeasure.Id, string.Join(",", nursingCAHPSMeasures.Select(x => x.Id).ToList())));
            }

            foreach (var measure in nursingCAHPSMeasures.GroupBy(x => x.MeasureType))
            {
                var compositeMeasure = measure.ToList().FirstOrDefault(x => compositeMeasureIds.Contains(x.Name));
                if (compositeMeasure == overallMeasure) continue;

                var measures = measure.ToList().Where(x => !compositeMeasureIds.Contains(x.Name)).ToList();
                var topicId = Convert.ToInt32(compositeMeasure.MeasureCode.Replace("NH_COMP_", ""));
                result.Add(CreateMeasureTopic(topicId, compositeMeasure.MeasureType, compositeMeasure.MeasureType, topic.Description, compositeMeasure.Id, string.Join(",", measures.Select(x => x.Id).ToList()), false));
            }

            return result;
        }

        private string CreateMeasureTopic(int topicId, string topicName, string topicLongTitle, string description, int overallMeasureId, string measureIds, bool subsetInScore, bool closeTag = true)
        {
			var result = "{"
					+ string.Format("\"TopicID\": {0},", topicId)
					+ string.Format("\"Name\":\"{0}\",", topicName)
					+ string.Format("\"LongTitle\":\"{0}\",", topicLongTitle)
					+ string.Format("\"Description\":\"{0}\",", description)
					+ string.Format("\"SubsetInScore\":{0},", subsetInScore.ToString().ToLower())
                    + string.Format("\"OverallMeasure\": {0},", overallMeasureId)
                    + string.Format("\"MeasureIDs\": [{0}{1}{0}]", Environment.NewLine, measureIds);

            result += closeTag ? "}" : "";
            return result;
        }

        #region Helper Class and Struct

        [DataContract(Name = "")]
        internal class NursingHomeReportMeasure
        {

            [DataMember(Name = "NursingHomeID", Order = 1)]
            public int NursingHomeID { get; set; }

            [DataMember(Name = "MeasureID", Order = 2)]
            public int MeasureID { get; set; }

            [DataMember(Name = "Rate", Order = 3)]
            public string Rate { get; set; }

            [DataMember(Name = "NatRating", Order = 4)]
            public string NatRating { get; set; }

            [DataMember(Name = "NatRate", Order = 5)]
            public string NatRate { get; set; }

            [DataMember(Name = "PeerRating", Order = 6)]
            public string PeerRating { get; set; }

            [DataMember(Name = "PeerRate", Order = 7)]
            public string PeerRate { get; set; }

            [DataMember(Name = "CountyRating", Order = 8)]
            public string CountyRating { get; set; }

        }

        struct NursingHomeMeasureStruct
        {
            private string _query;

            private IList<string> _relatedMeasures;

            public string OverallMeasureId;

            public string RelatedMeasures;

            public string TopicId;

            public string Description;

            public NursingHomeMeasureStruct(Website website, string connectionstring, string filter, string overallMeasureCode)
            {
                _query = @"SELECT m.Id, m.MeasureType, m.Name as Code, m.PlainTitle, mt.Topic_Id, m.[Description]
                                      FROM (select Website_Id, IsSelected, case when OverrideMeasure_Id is null 
												then OriginalMeasure_Id else OverrideMeasure_Id end as Measure_Id 
												from Websites_WebsiteMeasures) wm Join
									  Measures m on wm.Measure_Id = m.Id  
                                        LEFT JOIN Measures_MeasureTopics mt on m.Id = mt.Measure_Id
                                      WHERE m.ClassType = 'NursingHome' and wm.Website_Id = {1} and wm.IsSelected = 1 AND {0}
                                      ORDER BY mt.Topic_Id";
                _relatedMeasures = new List<string>();
                OverallMeasureId = string.Empty;
                TopicId = string.Empty;
                Description = string.Empty;

                using (var con = new SqlConnection(connectionstring))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = string.Format(_query, filter, website.Id);
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var code = reader["Code"].ToString();
                            var plainTitle = reader["PlainTitle"].ToString();
                            var Id = reader["Id"].ToString();
                            TopicId = reader["Topic_Id"].ToString();

                            if (code.ToUpper().Equals(overallMeasureCode))
                            {
                                OverallMeasureId = Id;
                                Description = reader["Description"].ToString();
                            }
                            else
                            {
                                if (!plainTitle.ToUpper().Equals("Overall Quality"))
                                    _relatedMeasures.Add(Id);
                            }
                        }
                    }
                }
                RelatedMeasures = string.Join(string.Format(",{0}", Environment.NewLine), _relatedMeasures);
            }
        }


        #endregion
    }

    [DataContract(Name = "")]
    public class ProviderLatLogLookUp
    {
        [DataMember(Name = "ProviderId")]
        public string ProviderId { get; set; }
        [DataMember(Name = "Latitude")]
        public double Latitude { get; set; }
        [DataMember(Name = "Longitude")]
        public double Longitude { get; set; }
    }
}
