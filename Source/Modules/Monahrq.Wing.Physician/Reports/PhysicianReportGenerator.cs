using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Generators;
using dbPhysician = Monahrq.Infrastructure.Domain.Physicians.Physician;
using NHibernate.Linq;
using Monahrq.Infrastructure.Domain.Physicians;
using Monahrq.Wing.Physician.Physicians;
using NHibernate;
using NHibernate.Criterion;
using Monahrq.Infrastructure.Data.Transformers;
using NHibernate.Transform;

namespace Monahrq.Wing.Physician.Reports
{
	/// <summary>
	/// Generates the report data/.json files for Physician reports.
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new[] { "4C5727B4-0E85-4F80-ADE9-418B49A1373E", "E007BB9C-E539-41D6-9D06-FF52F8A15BF6" },
                     new[] { "Physician Data" },
                     new[] { typeof(PhysicianTarget) },
                     8)]
    public class PhysicianReportGenerator : BaseReportGenerator
    {
		//private readonly IList<DataTable> _datatables;
		/// <summary>
		/// The dataset states
		/// </summary>
		private readonly List<string> _datasetStates = new List<string>();
		//private const string JSON_DOMAIN = "$.monahrq.medicareprovidercharge=";
		/// <summary>
		/// The use realtime data
		/// </summary>
		private bool _useRealtimeData;

		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicianReportGenerator"/> class.
		/// </summary>
		public PhysicianReportGenerator()
        {
            // _datatables = new List<DataTable>();
        }

		/// <summary>
		/// Loads the report data.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Something seriously went wrong. The reporting website can not be null.</exception>
		protected override bool LoadReportData()
        {
            try
            {
                if (CurrentWebsite == null)
                    throw new InvalidOperationException("Something seriously went wrong. The reporting website can not be null.");

                // TODO: Add Ramesh's code here...
                var tempStates = CurrentWebsite.Datasets.Where(wds => wds.Dataset.ContentType.Name.EqualsIgnoreCase("Physician Data") && !string.IsNullOrEmpty(wds.Dataset.ProviderStates))
                                                        .Select(wd => wd.Dataset.ProviderStates)
                                                        .ToList();

                tempStates.ForEach(temp => _datasetStates.AddRange(temp.Split(',').ToArray()));


                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.GetBaseException());
                return false;
            }
        }

		/// <summary>
		/// Outputs the data files.
		/// </summary>
		/// <returns></returns>
		protected override bool OutputDataFiles()
        {
            try
            {
                // This is the base data needed for the physician reports. Please do not remove. - Jason
                ExecuteMethod(GeneratePhysicianSpecialtiesJson);

                if (_useRealtimeData) return true;

                ExecuteMethod(GenerateMedicalPracticeJson);
                ExecuteMethod(GeneratePhysicianHospitalAffilBaseJson);

                // TODO: Add Ramesh's code here...
                ExecuteMethod(GeneratePhysicianIndexBase);
                ExecuteMethod(GeneratePhysicianCityIndexBase);
                ExecuteMethod(GeneratePhysicianProfileBase);
                ExecuteMethod(GeneratePhysicianSpecialtiesReport);
                ExecuteMethod(GeneratePhysicianZipReport);
                // ExecuteMethod(GeneratePhysicianZipReport);
                ExecuteMethod(GeneratePhysicianPracticeReport);
            }
            catch (Exception ex)
            {
                Logger.Write(ex.GetBaseException());
                return false;
            }
            return true;
        }

		/// <summary>
		/// Executes the method.
		/// </summary>
		/// <param name="action">The action.</param>
		private void ExecuteMethod(Action action)
        {
            if (action == null) return;
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Write(ex.GetBaseException());
            }
        }

		/// <summary>
		/// Generates the physician city index base.
		/// </summary>
		private void GeneratePhysicianCityIndexBase()
        {
            var outputPath = Path.Combine(BaseDataDirectoryPath, "Base", "PhysicianCityIndex.js");
            if (File.Exists(outputPath)) return;

            var states = string.Join(",", _datasetStates.ToArray());
            var zipCodes = GetDistinctFiveDigitZipcodes(_datasetStates);
            var temp = new ConcurrentQueue<PhysicianCityIndex>();

            Parallel.ForEach(zipCodes, zip =>
            {
                var dt = RunSprocReturnDataTable("spExportPhysician",
                new KeyValuePair<string, object>("@selection", 3),
                new KeyValuePair<string, object>("@csvStates", states),
                new KeyValuePair<string, object>("@zipCode", zip));

                foreach (DataRow row in dt.Rows)
                {
                    temp.Enqueue(new PhysicianCityIndex() { Npi = Convert.ToInt64(row["npi"]), Cty = row["cty"].ToString() });
                }

                dt.Dispose();
            });

            GenerateJsonFile(temp, outputPath, "$.monahrq.Physicians.Base.PhysicianCityIndex=");
        }

		/// <summary>
		/// Generates the physician practice report.  Data/Physicians
		/// </summary>
		private void GeneratePhysicianPracticeReport()
        {
            var outputPath = Path.Combine(BaseDataDirectoryPath, "Physicians", "Practice");
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            string states = string.Join(",", _datasetStates.ToArray());
            //string directoryPath = Path.Combine(BaseDataDirectoryPath, "..\\", "Physicians", "Practice");
            DataTable dt = RunSprocReturnDataTable("spExportPhysician",
                new KeyValuePair<string, object>("@selection", 4),
                new KeyValuePair<string, object>("@csvStates", states));
            IterateData("org_pac_id", typeof(string), outputPath, "Practice_{0}.js", "$.monahrq.Physicians.Report.Practice['{0}']=", ref dt);
        }

		/// <summary>
		/// Generates the physician zip report. // Data/Physicians
		/// </summary>
		private void GeneratePhysicianZipReport()
        {
            var outputPath = Path.Combine(BaseDataDirectoryPath, "Physicians", "Zip");
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            var states = string.Join(",", _datasetStates.ToArray());
            var zipcodes = GetDistinctFiveDigitZipcodes(_datasetStates);

            Parallel.ForEach(zipcodes, zip => ProcessPhysicianZipCodeReport(outputPath, states, zip));
        }

		/// <summary>
		/// Gets the distinct five digit zipcodes.
		/// </summary>
		/// <param name="states">The states.</param>
		/// <returns></returns>
		public List<string> GetDistinctFiveDigitZipcodes(List<string> states)
        {
            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                return session.Query<Address>()
                    .Where(x => states.Contains(x.State) && x.ZipCode != null)
                    .Select(x => x.ZipCode.Substring(0, x.ZipCode.Length > 5 ? 5 : x.ZipCode.Length))
                    .DistinctBy(x => x)
                    .OrderBy(x => x).ToList();
            }
        }

		/// <summary>
		/// Processes the physician zip code report.
		/// </summary>
		/// <param name="outputPath">The output path.</param>
		/// <param name="states">The states.</param>
		/// <param name="zipcode">The zipcode.</param>
		private void ProcessPhysicianZipCodeReport(string outputPath, string states, string zipcode)
        {
            var dt = RunSprocReturnDataTable("spExportPhysician",
                new KeyValuePair<string, object>("@selection", 3),
                new KeyValuePair<string, object>("@csvStates", states),
                new KeyValuePair<string, object>("@zipCode", zipcode));
            IterateData("zip5", typeof(string), outputPath, "Zip_{0}.js", "$.monahrq.Physicians.Report.Zip['{0}']=", ref dt);

        }

		/// <summary>
		/// Generates the physician specialties report. Data/Physicians
		/// </summary>
		private void GeneratePhysicianSpecialtiesReport()
        {
            var outputPath = Path.Combine(BaseDataDirectoryPath, "Physicians", "Specialty");
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            var states = string.Join(",", _datasetStates.ToArray());
            DataTable dt = RunSprocReturnDataTable("spExportPhysician",
                new KeyValuePair<string, object>("@selection", 2),
                new KeyValuePair<string, object>("@csvStates", states));
            IterateData("pri_spec_id", typeof(int), outputPath, "Specialty_{0}.js", "$.monahrq.Physicians.Report.Specialty['{0}']=", ref dt);
        }

		/// <summary>
		/// Generates the physician profile base. /base/PhysicianProfiles/...
		/// </summary>
		private void GeneratePhysicianProfileBase()
        {
            string states = string.Join(",", _datasetStates.ToArray());
            var dicHedisData = new ConcurrentDictionary<long, IList<HEDISRateStruct>>();
            var profiles = new ConcurrentDictionary<long, IList<PhysicianProfileStruct>>();

            var isDatasetIncluded = CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.ToLower().Contains("Medical Practice HEDIS Measures Data".ToLower()));
            var reportAttributeSelected = CurrentWebsite.Reports.Any(x => x.Report.Filters.Any(f => f.Values.Any(v => v.Value && v.Name == "HEDIS Measures")));
            if (reportAttributeSelected && isDatasetIncluded && CurrentWebsite.Measures.Any(x => x.ReportMeasure.Source != null && x.ReportMeasure.Source.Contains("Physician HEDIS")))
            {
                //TODO: Execute Hedis report first
                DataTable hedisDt = RunSprocReturnDataTable("spGenerateHedisReport", new KeyValuePair<string, object>("@csvStates", states));
                foreach (var row in hedisDt.AsEnumerable())
                {
                    var data = new HEDISRateStruct
                    {
                        Npi = Convert.ToInt64(row["Npi"]),
                        PracticeId = row.SafeField<string>("PracticeId"),
                        MeasureId = !row.IsNull("MeasureId") ? (int?)Convert.ToInt32(row["MeasureId"]) : null,
                        PhysicianRate = string.Format("{0:#0.00}", row.SafeField<double>("PhysicianRate")),
                        PeerRate = string.Format("{0:#0.00}", row.SafeField<double>("PeerRate"))
                    };

                    if (dicHedisData.ContainsKey(data.Npi))
                    {
                        var val = dicHedisData[data.Npi] as List<HEDISRateStruct>;
                        val.Add(data);
                    }
                    else
                    {
                        dicHedisData.TryAdd(data.Npi, new List<HEDISRateStruct> { data });
                    }
                }
                hedisDt.Dispose();
            }

            var outputPath = Path.Combine(BaseDataDirectoryPath, "Base", "PhysicianProfiles");
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            using (DataTable dt = RunSprocReturnDataTable("spExportPhysician", new KeyValuePair<string, object>("@selection", -1), new KeyValuePair<string, object>("@csvStates", states)))
            {
                foreach (var row in dt.AsEnumerable())
                {
                    var practiceId = row.SafeField<string>("org_pac_id");
                    var npi = row.SafeField<long>("npi");
                    var phyProfdata = new PhysicianProfileStruct
                    {
                        npi = row.SafeField<long>("npi"),
                        ind_pac_id = Convert.ToString(row["ind_pac_id"]),
                        ind_enrl_id = Convert.ToString(row["ind_enrl_id"]),
                        frst_nm = Convert.ToString(row["frst_nm"]),
                        mid_nm = Convert.ToString(row["mid_nm"]),
                        lst_nm = Convert.ToString(row["lst_nm"]),
                        suff = Convert.ToString(row["suff"]),
                        gndr = Convert.ToString(row["gndr"]),
                        cred = Convert.ToString(row["cred"]),
                        med_sch = Convert.ToString(row["med_sch"]),
                        grd_yr = !row.IsNull("grd_yr") ? (int?)Convert.ToInt32(row["grd_yr"]) : null,
                        pri_spec = Convert.ToString(row["pri_spec"]),
                        sec_spec_1 = Convert.ToString(row["sec_spec_1"]),
                        sec_spec_2 = Convert.ToString(row["sec_spec_2"]),
                        sec_spec_3 = Convert.ToString(row["sec_spec_3"]),
                        sec_spec_4 = Convert.ToString(row["sec_spec_4"]),
                        sec_spec_all = Convert.ToString(row["sec_spec_all"]),
                        assgn = Convert.ToString(row["assgn"]),
                        erx = Convert.ToString(row["erx"]),
                        pqrs = Convert.ToString(row["pqrs"]),
                        ehr = Convert.ToString(row["ehr"]),
                        org_lgl_nm = Convert.ToString(row["org_lgl_nm"]),
                        org_pac_id = Convert.ToString(row["org_pac_id"]),
                        org_dba_nm = Convert.ToString(row["org_dba_nm"]),
                        num_org_mem = !row.IsNull("num_org_mem") ? (int?)Convert.ToInt32(row["num_org_mem"]) : null,
                        adr_ln_1 = Convert.ToString(row["adr_ln_1"]),
                        adr_lin_2 = Convert.ToString(row["adr_lin_2"]),
                        cty = Convert.ToString(row["cty"]),
                        st = Convert.ToString(row["st"]),
                        zip = Convert.ToString(row["zip"]),
                        ln_2_sprs = Convert.ToString(row["ln_2_sprs"]),
                        hosp_afl_1 = Convert.ToString(row["hosp_afl_1"]),
                        hosp_afl_2 = Convert.ToString(row["hosp_afl_2"]),
                        hosp_afl_3 = Convert.ToString(row["hosp_afl_3"]),
                        hosp_afl_4 = Convert.ToString(row["hosp_afl_4"]),
                        hosp_afl_5 = Convert.ToString(row["hosp_afl_5"]),
                        hosp_afl_lbn_1 = Convert.ToString(row["hosp_afl_lbn_1"]),
                        hosp_afl_lbn_2 = Convert.ToString(row["hosp_afl_lbn_2"]),
                        hosp_afl_lbn_3 = Convert.ToString(row["hosp_afl_lbn_3"]),
                        hosp_afl_lbn_4 = Convert.ToString(row["hosp_afl_lbn_4"]),
                        hosp_afl_lbn_5 = Convert.ToString(row["hosp_afl_lbn_5"]),
                        MOC = Convert.ToString(row["MOC"]),
                        MHI = Convert.ToString(row["MHI"]),
						HEDISRates = dicHedisData.ContainsKey(npi) ? (dicHedisData[npi] as List<HEDISRateStruct>).Where(x => x.Npi == npi && x.PracticeId == practiceId).ToList() : new List<HEDISRateStruct>(),
					//	HEDISRates = dicHedisData.ContainsKey(npi) ? (dicHedisData[npi] as List<HEDISRateStruct>).Where(x => x.Npi == npi && x.PracticeId == practiceId).ToList() : null,	//new List<HEDISRateStruct>(),
                    };

					if (phyProfdata.HEDISRates != null && phyProfdata.HEDISRates.Count() == 0)
						phyProfdata.HEDISRates = null;

					if (profiles.ContainsKey(phyProfdata.npi))
                    {
                        var val = profiles[phyProfdata.npi] as List<PhysicianProfileStruct>;
                        val.Add(phyProfdata);
                    }
                    else
                    {
                        profiles.TryAdd(phyProfdata.npi, new List<PhysicianProfileStruct> { phyProfdata });
                    }
                };
            }

            foreach (var profile in profiles.OrderBy(x => x.Key))
            {
                string fileName = Path.Combine(outputPath, string.Format("Profile_{0}.js", profile.Key));
                var ns = string.Format("$.monahrq.Physicians.Base.Profiles['{0}']=", profile.Key);
                GenerateJsonFile(profile.Value as List<PhysicianProfileStruct>, fileName, ns);
            }
            //IterateData("npi", typeof(long), outputPath, "Profile_{0}.js", "$.monahrq.Physicians.Base.Profiles['{0}']=", ref dt);
        }

		/// <summary>
		/// Generates the physician index base.  //Directly under base
		/// </summary>
		private void GeneratePhysicianIndexBase()
        {
            var outputPath = Path.Combine(BaseDataDirectoryPath, "Base", "PhysiciansIndex.js");
            if (File.Exists(outputPath)) return;

            string states = string.Join(",", _datasetStates.ToArray());
            DataTable dt = RunSprocReturnDataTable("spExportPhysician",
                new KeyValuePair<string, object>("@selection", 0),
                new KeyValuePair<string, object>("@csvStates", states));
            GenerateJsonFile(dt, outputPath, "$.monahrq.Physicians.Base.PhysicianIndex=");

            dt.Dispose();
        }

		/// <summary>
		/// Iterates the data. Assume resultset is ordered by colName
		/// This logic such that we dont need to delete rows while iteration
		/// </summary>
		/// <param name="colName">Name of the col.</param>
		/// <param name="colDataType">Type of the col data.</param>
		/// <param name="directoryPath">The directory path.</param>
		/// <param name="fileNamePattern">The file name pattern.</param>
		/// <param name="varNameFormat">The variable name format.</param>
		/// <param name="dtSrc">The dt source.</param>
		private void IterateData(string colName, Type colDataType, string directoryPath, string fileNamePattern, string varNameFormat, ref DataTable dtSrc)
        {
            if (dtSrc.Rows.Count == 0)
                return;

            var rowCtr = 0;
            var totalRowCount = dtSrc.Rows.Count;
            var dtBuff = new DataTable();
            var fileNames = new List<string>();

            do
            {
                string currValue = dtSrc.Rows[rowCtr][colName].ToString();
                string filterExpr = string.Empty;
                if (colDataType == typeof(string))
                {
                    filterExpr = colName + "='" + currValue.Replace("'", "''") + "'";
                }
                else if (colDataType == typeof(int) || colDataType == typeof(long))
                {
                    filterExpr = colName + "=" + currValue;
                }

                DataRow[] selRows = dtSrc.Select(filterExpr);
                string fileName = Path.Combine(directoryPath, string.Format(fileNamePattern, currValue));
                string varName = string.Format(varNameFormat, currValue);

                if (fileNames.Any(f => f.EqualsIgnoreCase(fileName)) /*File.Exists(fileName)*/) continue;

                if (selRows.Length > 0)
                {
                    dtBuff.Clear();
                    dtBuff = selRows.CopyToDataTable();
                    rowCtr += dtBuff.Rows.Count;
                    GenerateJsonFile(dtBuff, fileName, varName);
                    fileNames.Add(fileName);
                }

            } while (rowCtr < totalRowCount);

            dtBuff.Dispose();
        }

		/// <summary>
		/// Generates the physician specialties json.
		/// </summary>
		private void GeneratePhysicianSpecialtiesJson()
        {
            var jsonNamespace = "$.monahrq.Physicians.Base.PhysicianSpecialty=";
            var jsonFileName = Path.Combine(BaseDataDirectoryPath, "Base", "PhysicianSpecialty.js");

            if (File.Exists(jsonFileName)) return;

            List<ProviderSpecialityDto> physiciansSpecialties;

            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                physiciansSpecialties =
                    session.Query<ProviderSpeciality>()
                        .Select(
                            ps =>
                            new ProviderSpecialityDto
                                {
                                    Id = ps.Id,
                                    Name = ps.Name,
                                    ProviderTaxonomy = ps.ProviderTaxonomy
                                }).ToList();
            }

            physiciansSpecialties.Add(AddUnknownToPhysicianSpecialtiesList());

            GenerateJsonFile(physiciansSpecialties, jsonFileName, jsonNamespace);
        }

		/// <summary>
		/// Creates a ProviderSpecialityDto for physicians with unknosn primary specialties
		/// </summary>
		/// <returns>
		/// A ProviderSpecialityDto for Unknown
		/// </returns>
		private static ProviderSpecialityDto AddUnknownToPhysicianSpecialtiesList()
        {        
            return new ProviderSpecialityDto
            {
                Id = -1,
                Name = "UNKNOWN",
                ProviderTaxonomy = null
            };       
        }

		/// <summary>
		/// Generates the medical practice json.
		/// </summary>
		private void GenerateMedicalPracticeJson()
        {
            var jsonNamespace = "$.monahrq.Physicians.Base.MedicalPractices=";
            var jsonFileName = Path.Combine(base.BaseDataDirectoryPath, "Base", "MedicalPractices.js");

            if (File.Exists(jsonFileName)) return;

            var medicalPractices = new List<MedicalPracticeDto>();

            using (var session = this.DataProvider.SessionFactory.OpenStatelessSession())
            {
                medicalPractices = session.Query<MedicalPractice>().Where(mp => _datasetStates.Contains(mp.State))
                                          .Select(mp => new MedicalPracticeDto
                                          {
                                              PracticeId = mp.Id,
                                              GroupPracticePacId = mp.GroupPracticePacId,
                                              PracticeName = mp.Name,
                                              PracticeDbName = mp.DBAName,
                                              NumberofGroupPracticeMembers = mp.NumberofGroupPracticeMembers,
                                              State = mp.State
                                          }).ToList();
            }

            GenerateJsonFile(medicalPractices, jsonFileName, jsonNamespace);

            medicalPractices.Clear();
        }

		/// <summary>
		/// Generates the physician hospital affil base json.
		/// </summary>
		private void GeneratePhysicianHospitalAffilBaseJson()
        {
            var jsonNamespace = "$.monahrq.Physicians.Base.PhysicianHospitalAffiliation=";
            var jsonFileName = Path.Combine(base.BaseDataDirectoryPath, "Base", "PhysicianHospitalAffiliations.js");

            bool showAsArrays = false;
            if (File.Exists(jsonFileName)) return;

            var pahList = new List<PhysicianHospitalAffilDto>();

            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                foreach (var state in _datasetStates.ToList())
                {
                    // Used if return array of hospital Provider Ids
                    string phsyiciansQuery;
                    if (showAsArrays)
                    {
                        phsyiciansQuery = string.Format(@"SELECT DISTINCT p.[Npi] 'NPI', 
                    	    STUFF((SELECT ',' + pha.[Hospital_CmsProviderId] 
                                   FROM [dbo].[Physicians_AffiliatedHospitals] pha WITH(NOLOCK) 
                                   WHERE pha.[Physician_Id] = p.id
                                   FOR XML PATH (''))
                                  , 1, 1, '')  'HospitalProviderID'
                        FROM [dbo].[Physicians] p WITH(NOLOCK)
                        WHERE p.[States] like '%{0}%'
                        ORDER BY p.[Npi];", state);
                    }
                    else
                    {
                        // return not distinct rows
                        phsyiciansQuery = string.Format(@"select distinct p.[Npi] 'NPI', pha.[Hospital_CmsProviderId] 'HospitalProviderID'
                        FROM [dbo].[Physicians] p WITH(NOLOCK) 
                        	JOIN [dbo].[Physicians_AffiliatedHospitals] pha WITH(NOLOCK) ON pha.[Physician_Id] = p.[Id]
                        WHERE p.[States] like '%{0}%'
                        ORDER BY p.[Npi];", state);
                    }

                    pahList.AddRange(session.CreateSQLQuery(phsyiciansQuery)
                                            .AddScalar("Npi", NHibernateUtil.Int64)
                                            .AddScalar("HospitalProviderID", NHibernateUtil.String)
                                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(PhysicianHospitalAffilDto)))
                                            .List<PhysicianHospitalAffilDto>());
                }
            }

            if (showAsArrays)
            {
                var pahFinalList = new List<PhysicianHospitalAffilDto>();

                foreach (var pah in pahList)
                {
                    var physicianNpi = pah.Npi;
                    var providerIds = pah.HospitalProviderID;
                    var phaDto = new PhysicianHospitalAffilDto()
                    {
                        Npi = physicianNpi,
                        HospitalProviderIDs = !string.IsNullOrEmpty(providerIds)
                                                             ? new List<string>(providerIds.Split(new[] { ',' }).ToArray())
                                                             : new List<string>()
                    };

                    pahFinalList.Add(phaDto);
                }
                base.GenerateJsonFile(pahFinalList, jsonFileName, jsonNamespace);
            }
            else
            {
                // return no distinct rows 
                base.GenerateJsonFile(pahList, jsonFileName, jsonNamespace);
            }

        }

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
        {
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Physicians reports" });

            // scripts init
            string[] scriptFiles = new string[] { "List2Table.sql", "ExportPhysician.sql", "spGenerateHedisReport.sql" };
            RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\Physician\\"), scriptFiles);
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
                //var profileRpt = website.Reports.FirstOrDefault(r => r.Report.SourceTemplate.Name.EqualsIgnoreCase("Physician Profile") || 
                //                                                     r.Report.SourceTemplate.Name.EqualsIgnoreCase("Physician Listing Report"));

                _useRealtimeData = (CurrentWebsite.Datasets.Any(wds => wds.Dataset.ContentType.Name.EqualsIgnoreCase("Physician Data") && wds.Dataset.UseRealtimeData));

                if (ActiveReport != null && (ActiveReport.SourceTemplate.Name.EqualsIgnoreCase("Physician Profile") ||
                                             ActiveReport.SourceTemplate.Name.EqualsIgnoreCase("Physician Listing Report")))
                {
                    // TODO add logic for correct validation.
                    using (var session = this.DataProvider.SessionFactory.OpenStatelessSession())
                    {
                        if (!_useRealtimeData && !session.Query<dbPhysician>().Any())
                        {
                            validationResults.Add(new ValidationResult("The Physican Profile or Listing report was not generated due to no Physicians imported into the Monahrq."));
                        }
                    }
                }
                else
                    validationResults.Add(new ValidationResult("The Physician Profile or Listing report was not selected. Please make sure to select the correct report and try again."));
            }

            return validationResults == null || validationResults.Count == 0;
        }

		/// <summary>
		/// 
		/// </summary>
		[DataContract(Name = "")]
        internal class ProviderSpecialityDto
        {
			/// <summary>
			/// Gets or sets the identifier.
			/// </summary>
			/// <value>
			/// The identifier.
			/// </value>
			[DataMember]
            public int Id { get; set; }
			/// <summary>
			/// Gets or sets the name.
			/// </summary>
			/// <value>
			/// The name.
			/// </value>
			[DataMember]
            public string Name { get; set; }
			/// <summary>
			/// Gets or sets the provider taxonomy.
			/// </summary>
			/// <value>
			/// The provider taxonomy.
			/// </value>
			[DataMember]
            public string ProviderTaxonomy { get; set; }
        }

		/// <summary>
		/// 
		/// </summary>
		[DataContract(Name = "")]
        internal class MedicalPracticeDto
        {
			/// <summary>
			/// Gets or sets the practice identifier.
			/// </summary>
			/// <value>
			/// The practice identifier.
			/// </value>
			[DataMember]
            public int PracticeId { get; set; }
			/// <summary>
			/// Gets or sets the name of the practice.
			/// </summary>
			/// <value>
			/// The name of the practice.
			/// </value>
			[DataMember]
            public string PracticeName { get; set; }
			/// <summary>
			/// Gets or sets the name of the practice database.
			/// </summary>
			/// <value>
			/// The name of the practice database.
			/// </value>
			[DataMember]
            public string PracticeDbName { get; set; }
			/// <summary>
			/// Gets or sets the group practice pac identifier.
			/// </summary>
			/// <value>
			/// The group practice pac identifier.
			/// </value>
			[DataMember]
            public string GroupPracticePacId { get; set; }
			/// <summary>
			/// Gets or sets the numberof group practice members.
			/// </summary>
			/// <value>
			/// The numberof group practice members.
			/// </value>
			[DataMember]
            public int? NumberofGroupPracticeMembers { get; set; }
			/// <summary>
			/// Gets or sets the state.
			/// </summary>
			/// <value>
			/// The state.
			/// </value>
			[DataMember]
            public string State { get; set; }
        }

		/// <summary>
		/// 
		/// </summary>
		[DataContract(Name = "")]
        internal class PhysicianHospitalAffilDto
        {
			/// <summary>
			/// Gets or sets the npi.
			/// </summary>
			/// <value>
			/// The npi.
			/// </value>
			[DataMember]
            public long Npi { get; set; }
			/// <summary>
			/// Gets or sets the hospital provider identifier.
			/// </summary>
			/// <value>
			/// The hospital provider identifier.
			/// </value>
			[DataMember]
            public string HospitalProviderID { get; set; }
			//[DataMember]
			/// <summary>
			/// Gets or sets the hospital provider i ds.
			/// </summary>
			/// <value>
			/// The hospital provider i ds.
			/// </value>
			public List<string> HospitalProviderIDs { get; set; }
        }

		/// <summary>
		/// 
		/// </summary>
		[DataContract(Name = "")]
        internal class PhysicianCityIndex
        {
			/// <summary>
			/// Gets or sets the npi.
			/// </summary>
			/// <value>
			/// The npi.
			/// </value>
			[DataMember(Name = "npi")]
            public long Npi { get; set; }
			/// <summary>
			/// Gets or sets the cty.
			/// </summary>
			/// <value>
			/// The cty.
			/// </value>
			[DataMember(Name = "cty")]
            public string Cty { get; set; }
        }

		/// <summary>
		/// 
		/// </summary>
		[DataContract]
        internal struct HEDISRateStruct
        {
			/// <summary>
			/// Gets or sets the npi.
			/// </summary>
			/// <value>
			/// The npi.
			/// </value>
			[IgnoreDataMember]
            public long Npi { get; set; }
			/// <summary>
			/// Gets or sets the practice identifier.
			/// </summary>
			/// <value>
			/// The practice identifier.
			/// </value>
			[DataMember(Name = "PracticeId")]
            public string PracticeId { get; set; }
			/// <summary>
			/// Gets or sets the measure identifier.
			/// </summary>
			/// <value>
			/// The measure identifier.
			/// </value>
			[DataMember(Name = "MeasureID")]
            public int? MeasureId { get; set; }
			/// <summary>
			/// Gets or sets the physician rate.
			/// </summary>
			/// <value>
			/// The physician rate.
			/// </value>
			[DataMember(Name = "PhysicianRate")]
            public string PhysicianRate { get; set; }
			/// <summary>
			/// Gets or sets the peer rate.
			/// </summary>
			/// <value>
			/// The peer rate.
			/// </value>
			[DataMember(Name = "PeerRate")]
            public string PeerRate { get; set; }
        }

		/// <summary>
		/// 
		/// </summary>
		[DataContract]
        internal struct PhysicianProfileStruct
        {
			/// <summary>
			/// Gets or sets the npi.
			/// </summary>
			/// <value>
			/// The npi.
			/// </value>
			[DataMember(Name = "npi")]
            public long npi { get; set; }
			/// <summary>
			/// Gets or sets the ind pac identifier.
			/// </summary>
			/// <value>
			/// The ind pac identifier.
			/// </value>
			[DataMember(Name = "ind_pac_id")]
            public string ind_pac_id { get; set; }
			/// <summary>
			/// Gets or sets the ind enrl identifier.
			/// </summary>
			/// <value>
			/// The ind enrl identifier.
			/// </value>
			[DataMember(Name = "ind_enrl_id")]
            public string ind_enrl_id { get; set; }
			/// <summary>
			/// Gets or sets the FRST nm.
			/// </summary>
			/// <value>
			/// The FRST nm.
			/// </value>
			[DataMember(Name = "frst_nm")]
            public string frst_nm { get; set; }
			/// <summary>
			/// Gets or sets the mid nm.
			/// </summary>
			/// <value>
			/// The mid nm.
			/// </value>
			[DataMember(Name = "mid_nm")]
            public string mid_nm { get; set; }
			/// <summary>
			/// Gets or sets the LST nm.
			/// </summary>
			/// <value>
			/// The LST nm.
			/// </value>
			[DataMember(Name = "lst_nm")]
            public string lst_nm { get; set; }
			/// <summary>
			/// Gets or sets the suff.
			/// </summary>
			/// <value>
			/// The suff.
			/// </value>
			[DataMember(Name = "suff")]
            public string suff { get; set; }
			/// <summary>
			/// Gets or sets the GNDR.
			/// </summary>
			/// <value>
			/// The GNDR.
			/// </value>
			[DataMember(Name = "gndr")]
            public string gndr { get; set; }
			/// <summary>
			/// Gets or sets the cred.
			/// </summary>
			/// <value>
			/// The cred.
			/// </value>
			[DataMember(Name = "cred")]
            public string cred { get; set; }
			/// <summary>
			/// Gets or sets the med SCH.
			/// </summary>
			/// <value>
			/// The med SCH.
			/// </value>
			[DataMember(Name = "med_sch")]
            public string med_sch { get; set; }
			/// <summary>
			/// Gets or sets the GRD yr.
			/// </summary>
			/// <value>
			/// The GRD yr.
			/// </value>
			[DataMember(Name = "grd_yr")]
            public int? grd_yr { get; set; }
			/// <summary>
			/// Gets or sets the pri spec.
			/// </summary>
			/// <value>
			/// The pri spec.
			/// </value>
			[DataMember(Name = "pri_spec")]
            public string pri_spec { get; set; }
			/// <summary>
			/// Gets or sets the sec spec 1.
			/// </summary>
			/// <value>
			/// The sec spec 1.
			/// </value>
			[DataMember(Name = "sec_spec_1")]
            public string sec_spec_1 { get; set; }
			/// <summary>
			/// Gets or sets the sec spec 2.
			/// </summary>
			/// <value>
			/// The sec spec 2.
			/// </value>
			[DataMember(Name = "sec_spec_2")]
            public string sec_spec_2 { get; set; }
			/// <summary>
			/// Gets or sets the sec spec 3.
			/// </summary>
			/// <value>
			/// The sec spec 3.
			/// </value>
			[DataMember(Name = "sec_spec_3")]
            public string sec_spec_3 { get; set; }
			/// <summary>
			/// Gets or sets the sec spec 4.
			/// </summary>
			/// <value>
			/// The sec spec 4.
			/// </value>
			[DataMember(Name = "sec_spec_4")]
            public string sec_spec_4 { get; set; }
			/// <summary>
			/// Gets or sets the sec spec all.
			/// </summary>
			/// <value>
			/// The sec spec all.
			/// </value>
			[DataMember(Name = "sec_spec_all")]
            public string sec_spec_all { get; set; }
			/// <summary>
			/// Gets or sets the assgn.
			/// </summary>
			/// <value>
			/// The assgn.
			/// </value>
			[DataMember(Name = "assgn")]
            public string assgn { get; set; }
			/// <summary>
			/// Gets or sets the erx.
			/// </summary>
			/// <value>
			/// The erx.
			/// </value>
			[DataMember(Name = "erx")]
            public string erx { get; set; }
			/// <summary>
			/// Gets or sets the PQRS.
			/// </summary>
			/// <value>
			/// The PQRS.
			/// </value>
			[DataMember(Name = "pqrs")]
            public string pqrs { get; set; }
			/// <summary>
			/// Gets or sets the ehr.
			/// </summary>
			/// <value>
			/// The ehr.
			/// </value>
			[DataMember(Name = "ehr")]
            public string ehr { get; set; }
			/// <summary>
			/// Gets or sets the org LGL nm.
			/// </summary>
			/// <value>
			/// The org LGL nm.
			/// </value>
			[DataMember(Name = "org_lgl_nm")]
            public string org_lgl_nm { get; set; }
			/// <summary>
			/// Gets or sets the org pac identifier.
			/// </summary>
			/// <value>
			/// The org pac identifier.
			/// </value>
			[DataMember(Name = "org_pac_id")]
            public string org_pac_id { get; set; }
			/// <summary>
			/// Gets or sets the org dba nm.
			/// </summary>
			/// <value>
			/// The org dba nm.
			/// </value>
			[DataMember(Name = "org_dba_nm")]
            public string org_dba_nm { get; set; }
			/// <summary>
			/// Gets or sets the number org memory.
			/// </summary>
			/// <value>
			/// The number org memory.
			/// </value>
			[DataMember(Name = "num_org_mem")]
            public int? num_org_mem { get; set; }
			/// <summary>
			/// Gets or sets the adr ln 1.
			/// </summary>
			/// <value>
			/// The adr ln 1.
			/// </value>
			[DataMember(Name = "adr_ln_1")]
            public string adr_ln_1 { get; set; }
			/// <summary>
			/// Gets or sets the adr lin 2.
			/// </summary>
			/// <value>
			/// The adr lin 2.
			/// </value>
			[DataMember(Name = "adr_lin_2")]
            public string adr_lin_2 { get; set; }
			/// <summary>
			/// Gets or sets the cty.
			/// </summary>
			/// <value>
			/// The cty.
			/// </value>
			[DataMember(Name = "cty")]
            public string cty { get; set; }
			/// <summary>
			/// Gets or sets the st.
			/// </summary>
			/// <value>
			/// The st.
			/// </value>
			[DataMember(Name = "st")]
            public string st { get; set; }
			/// <summary>
			/// Gets or sets the zip.
			/// </summary>
			/// <value>
			/// The zip.
			/// </value>
			[DataMember(Name = "zip")]
            public string zip { get; set; }
			/// <summary>
			/// Gets or sets the ln 2 SPRS.
			/// </summary>
			/// <value>
			/// The ln 2 SPRS.
			/// </value>
			[DataMember(Name = "ln_2_sprs")]
            public string ln_2_sprs { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl 1.
			/// </summary>
			/// <value>
			/// The hosp afl 1.
			/// </value>
			[DataMember(Name = "hosp_afl_1")]
            public string hosp_afl_1 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl 2.
			/// </summary>
			/// <value>
			/// The hosp afl 2.
			/// </value>
			[DataMember(Name = "hosp_afl_2")]
            public string hosp_afl_2 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl 3.
			/// </summary>
			/// <value>
			/// The hosp afl 3.
			/// </value>
			[DataMember(Name = "hosp_afl_3")]
            public string hosp_afl_3 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl 4.
			/// </summary>
			/// <value>
			/// The hosp afl 4.
			/// </value>
			[DataMember(Name = "hosp_afl_4")]
            public string hosp_afl_4 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl 5.
			/// </summary>
			/// <value>
			/// The hosp afl 5.
			/// </value>
			[DataMember(Name = "hosp_afl_5")]
            public string hosp_afl_5 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl LBN 1.
			/// </summary>
			/// <value>
			/// The hosp afl LBN 1.
			/// </value>
			[DataMember(Name = "hosp_afl_lbn_1")]
            public string hosp_afl_lbn_1 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl LBN 2.
			/// </summary>
			/// <value>
			/// The hosp afl LBN 2.
			/// </value>
			[DataMember(Name = "hosp_afl_lbn_2")]
            public string hosp_afl_lbn_2 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl LBN 3.
			/// </summary>
			/// <value>
			/// The hosp afl LBN 3.
			/// </value>
			[DataMember(Name = "hosp_afl_lbn_3")]
            public string hosp_afl_lbn_3 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl LBN 4.
			/// </summary>
			/// <value>
			/// The hosp afl LBN 4.
			/// </value>
			[DataMember(Name = "hosp_afl_lbn_4")]
            public string hosp_afl_lbn_4 { get; set; }
			/// <summary>
			/// Gets or sets the hosp afl LBN 5.
			/// </summary>
			/// <value>
			/// The hosp afl LBN 5.
			/// </value>
			[DataMember(Name = "hosp_afl_lbn_5")]
            public string hosp_afl_lbn_5 { get; set; }
			/// <summary>
			/// Gets or sets the moc.
			/// </summary>
			/// <value>
			/// The moc.
			/// </value>
			[DataMember(Name = "MOC")]
            public string MOC { get; set; }
			/// <summary>
			/// Gets or sets the mhi.
			/// </summary>
			/// <value>
			/// The mhi.
			/// </value>
			[DataMember(Name = "MHI")]
            public string MHI { get; set; }
			/// <summary>
			/// Gets or sets the hedis rates.
			/// </summary>
			/// <value>
			/// The hedis rates.
			/// </value>
			[DataMember(Name = "HEDISRates", EmitDefaultValue =false)]
            public List<HEDISRateStruct> HEDISRates { get; set; }

        }

    }
}
