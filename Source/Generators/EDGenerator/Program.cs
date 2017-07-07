using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace EDGenerator
{
    class Program
    {
        const int
ID = 0,
CAT_ID = 1,
CAT_VAL = 2,
NUM_ED_VISITS = 3,
NUM_ADMIT_HOSP = 4,
DIED_ED=5,
DIED_HOSP=6,
QUARTERS = 13;

        private const string PROGRESS_BAR_REMAINING = "                                                  ";
        private const string PROGRESS_BAR_DONE = "##################################################";


        static Stopwatch _stopWatch;
        static Dictionary<int, string[]> _hospitalRegion;
        static Dictionary<int, List<int>> _hospitalCategory;
        static int _flushCount;

        static Dictionary<int, string> _edNationalTotals;
        static int _logMod;
        static string _tempPath;
        static string _rootDir;
        static string _mainConnectionString;
        static string _hospitalIdList;
        static string _ipContentItemRecored;
        static string _edContentItemRecored;

        static int _edVisitsSuppression;
        static int _admitHospitalSuppression;
        static int _diedEDSuppression;
        static int _diedHospitalSuppression;
        static bool _applyOptimization;
        static List<int> _reportQuarters;

        static string _regionType;
        static int _timeout;
        static void Main(string[] args)
        {
            try
            { 

            _rootDir = ConfigurationManager.AppSettings["reportDir"];
            _hospitalIdList = ConfigurationManager.AppSettings["hospitalIDList"];
            _ipContentItemRecored = ConfigurationManager.AppSettings["IPContentItemRecord"];
            _edContentItemRecored = ConfigurationManager.AppSettings["EDContentItemRecord"];

            _edVisitsSuppression = int.Parse(ConfigurationManager.AppSettings["edVisitsSuppression"]);
            _admitHospitalSuppression = int.Parse(ConfigurationManager.AppSettings["admitHospitalSuppression"]);
            _diedEDSuppression = int.Parse(ConfigurationManager.AppSettings["diedEdSuppression"]);
            _diedHospitalSuppression = int.Parse(ConfigurationManager.AppSettings["diedHospitalSuppression"]);
            
            _timeout = int.Parse(ConfigurationManager.AppSettings["Timeout"]);
            _regionType = ConfigurationManager.AppSettings["RegionType"];
            _reportQuarters = ConfigurationManager.AppSettings["ReportQuarter"] != null
                   ? new List<int>(ConfigurationManager.AppSettings["ReportQuarter"].Split('|').Select(q => int.Parse(q)).ToList())
                   : new List<int> { 1, 2, 3, 4 }; //int.Parse(ConfigurationManager.AppSettings["ReportQuarter"] ?? "0");
                _applyOptimization = ConfigurationManager.AppSettings["ApplyOptimization"] == "1";
                _mainConnectionString = ConfigurationManager.ConnectionStrings["MAIN"].ToString();


            //--------------- args ----------------//
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-d":
                        _rootDir = args[i + 1];
                        break;
                    case "-c":
                        _mainConnectionString = args[i + 1];
                        break;
                    case "-h":
                        _hospitalIdList = args[i + 1];
                        break;
                    case "-i":
                        _ipContentItemRecored = args[i + 1];
                        break;
                    case "-e":
                        _edContentItemRecored = args[i + 1];
                        break;
                    case "-ED_VISITS_SUPPRESSION":
                        _edVisitsSuppression = int.Parse(args[i + 1]);
                        break;
                    case "-ADMIT_HOSPITAL_SUPPRESSION":
                        _admitHospitalSuppression = int.Parse(args[i + 1]);
                        break;
                    case "-DIED_ED_SUPPRESSION":
                        _diedEDSuppression = int.Parse(args[i + 1]);
                        break;
                    case "-DIED_HOSPITAL_SUPPRESSION":
                        _diedHospitalSuppression = int.Parse(args[i + 1]);
                        break;

                    case "-r":
                        _regionType = args[i + 1];
                        break;
                    case "-l":
                        _logMod = int.Parse(args[i + 1]);
                        break;
                    case "-t":
                        _timeout = int.Parse(args[i + 1]);
                        break;
                    case "-o":
                        _applyOptimization = bool.Parse(args[i + 1]);
                        break;
                    case "-rq":
                        _reportQuarters = args[i + 1] != null
                                                    ? new List<int>(args[i + 1].Split('|').Select(int.Parse).ToList())
                                                    : new List<int> { 1, 2, 3, 4 };
                        break;
                }
            }
            //-------------------------------------//

            Directory.CreateDirectory(Path.GetTempPath() + "Monahrq\\");
            Directory.CreateDirectory(Path.GetTempPath() + "Monahrq\\Generators\\");
            Directory.CreateDirectory(Path.GetTempPath() + "Monahrq\\Generators\\EDGenerator\\");
            _tempPath = Path.GetTempPath() + "Monahrq\\Generators\\EDGenerator\\" + Guid.NewGuid().ToString().Substring(0, 8);
            
            string rootDir = _rootDir;
            if (Directory.Exists(rootDir))
            {
                if (_logMod == 0)
                    Console.WriteLine("deleteing old data ...");
                
                Directory.Delete(rootDir, true);
            }
            _stopWatch = new Stopwatch();
            Stopwatch totalWatch = new Stopwatch();
            totalWatch.Start();

            InitializeHospitalRegion();
            InitializeHospitalCategory();
            //------ compile partial files -----//
            InitEDNationalTotals();

            string ccsSQL = ConfigurationManager.AppSettings["CCS_SQL"].Replace("[hospitalIDList]", _hospitalIdList).Replace("[IPContentItemRecord]", _ipContentItemRecored).Replace("[EDContentItemRecord]", _edContentItemRecored);
            GenerateDimension("CCS", ccsSQL);
            GC.Collect();
            CompilePartialsPerDimension(_tempPath, rootDir);
            GC.Collect();

            //var tempdirs = Directory.EnumerateDirectories(_tempPath);
            //if (_logMod == 0)
            //    Console.WriteLine();

            //Console.WriteLine("Compiling");
            //foreach (var dir in tempdirs)
            //{
            //    foreach (var l1 in Directory.EnumerateDirectories(dir))
            //    {
            //        Console.WriteLine("Compiling :" + dir.Substring(dir.LastIndexOf("\\", StringComparison.InvariantCulture)) + l1.Substring(l1.LastIndexOf("\\", StringComparison.InvariantCulture)));

            //        foreach (var l2 in Directory.EnumerateDirectories(l1))
            //        {
            //            CompilePartials(l2, l2.Replace(_tempPath, rootDir));
            //        }
            //    }
            //}

            var ts = totalWatch.Elapsed;

            // Format and display the TimeSpan value. 
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds);

            Console.WriteLine("Emergency Department Treat-and-Release (ED) Data Total RunTime: " + elapsedTime);

            //---- clear temp data -----------------//
            //GC.Collect();
            //System.IO.Directory.Delete(tempPath, true);
            }
            catch (Exception e)
            {
                var exc = e.GetBaseException();
                Console.WriteLine("Exception :" + exc.Message);
                Console.WriteLine(exc.StackTrace);
            } 
        }

        private static void InitializeHospitalRegion()
        {
            string hospitalRegionSQL = ConfigurationManager.AppSettings["HospitalRegion_SQL"].Replace("[hospitalIDList]", _hospitalIdList).Replace("[RegionType]", _regionType);
            _hospitalRegion = new Dictionary<int, string[]>();
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = hospitalRegionSQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();
            // int regionid;

            while (dataRead.Read())
            {
                var hospid = dataRead.GetInt32(0);
                var hospData = new string[3];
                hospData[0] = dataRead.GetInt32(1).ToString();
                hospData[1] = dataRead.IsDBNull(2)?"-1": dataRead.GetInt32(2).ToString();
                hospData[2] = dataRead.GetString(3);
                _hospitalRegion[hospid] = hospData;
            }
        }

        private static void InitializeHospitalCategory()
        {
            var hospitalCategorySQL = ConfigurationManager.AppSettings["HospitalCategory_SQL"].Replace("[hospitalIDList]", _hospitalIdList);
            _hospitalCategory = new Dictionary<int, List<int>>();
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = hospitalCategorySQL;
            command.CommandTimeout = _timeout;
            var dataRead = command.ExecuteReader();
            while (dataRead.Read())
            {
                var hospid = dataRead.GetInt32(0);
                var categoryid = dataRead.GetInt32(1);

                if (!_hospitalCategory.Keys.Contains(hospid))
                    _hospitalCategory[hospid] = new List<int>();

                _hospitalCategory[hospid].Add(categoryid);
            }
        }

        private static void GenerateDimension(string dimention, string dimentionSQL)
        {
            _stopWatch.Start();

            var acii = new ASCIIEncoding();
            Console.WriteLine("starting " + dimention);
            //string rootDir = _rootDir + "\\" + dimention;
            string tempDir = _tempPath + "\\" + dimention;
#if DEBUG
            Console.WriteLine("initialize datareader ... ");
#endif
            var conn = new SqlConnection(_mainConnectionString);

            var command = conn.CreateCommand();
            conn.Open();

            command.CommandText = dimentionSQL;
            command.CommandTimeout = _timeout;
            var dataRead = command.ExecuteReader();
            //var columns = new List<string>();

            //for (int i = 0; i < dataRead.FieldCount; i++)
            //{
            //    columns.Add(dataRead.GetName(i));
            //    Console.Write("{0}\t| ", dataRead.GetName(i));
            //}
            //Console.WriteLine("\n--------------------------------");

            var clinicalClinicalDetails = new Dictionary<int, object>();

            var clinicalHospitalDetails = new Dictionary<int, object>();
            Dictionary<int, object> clinicalHospitalData;

            var clinicalCountyDetails = new Dictionary<int, object>();
            Dictionary<int, object> clinicalCountyData;

            var clinicalRegionDetails = new Dictionary<int, object>();
            Dictionary<int, object> clinicalRegionData;

            var clinicalCategoryDetails = new Dictionary<int, object>();
            Dictionary<int, object> clinicalCategoryData;

            int clinicalId;
            Dictionary<int, object> clinicalData;

            int hospitalid;
            Dictionary<int, object> hospitalData;

            int ageid;
            Dictionary<int, object> ageData;
            Dictionary<int, object> ageGroupData;

            int raceid;
            Dictionary<int, object> raceData;
            Dictionary<int, object> raceGroupData;

            int sexid;
            Dictionary<int, object> sexData;
            Dictionary<int, object> sexGroupData;

            int primaryPayerid;
            Dictionary<int, object> primaryPayerData;
            Dictionary<int, object> primaryPayerGroupData;

            int dataSourceId;
            bool died;

            int dischargeQuarter;

            //Dictionary<int, object> hospitalMetaData = new Dictionary<int, object>();
            if (_logMod == 0)
                LogRuntime();
#if DEBUG
            Console.WriteLine("memory manipulation");
#endif
            var clinical0 = GetAndInitIfNeeded(clinicalClinicalDetails, 0);
            var hospital0 = GetAndInitIfNeeded(clinicalHospitalDetails, 0);
            var county0 = GetAndInitIfNeeded(clinicalCountyDetails, 0);
            var region0 = GetAndInitIfNeeded(clinicalRegionDetails, 0);
            var category0 = GetAndInitIfNeeded(clinicalCategoryDetails, 0);

            dataRead.Read();
            var rowCount = dataRead.GetInt32(0);
            int processed = 0;

            const int memoryFlushPoint = 500;
            const int rowsPerMemoryCheck = 100;
            const int maxPatchSize = 1000;
            _flushCount = 0;

            dataRead.NextResult();
            do
            {
                while (dataRead.Read())
                {
                    hospitalid = dataRead.GetInt32(0);
                    clinicalId = dataRead.GetInt32(1);
                    dataSourceId = dataRead.GetInt32(2);
                    died = dataRead.GetInt32(3) != 0;
                    ageid = dataRead.GetInt32(4);
                    raceid = dataRead.GetInt32(5);
                    sexid = dataRead.GetInt32(6);
                    primaryPayerid = dataRead.GetInt32(7);
                    dischargeQuarter = dataRead.IsDBNull(8) ? 0 : dataRead.GetInt32(8);

                    clinicalData = GetAndInitIfNeeded(clinicalClinicalDetails, clinicalId);
                    ArrangeData(clinicalData, hospitalid, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                    clinical0 = GetAndInitIfNeeded(clinicalClinicalDetails, 0);
                    ArrangeData(clinical0, hospitalid, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                    clinicalHospitalData = GetAndInitIfNeeded(clinicalHospitalDetails, hospitalid);
                    ArrangeData(clinicalHospitalData, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                    hospital0 = GetAndInitIfNeeded(clinicalHospitalDetails, 0);
                    ArrangeData(hospital0, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                    clinicalCountyData = GetAndInitIfNeeded(clinicalCountyDetails, int.Parse(_hospitalRegion[hospitalid][1]));
                    ArrangeData(clinicalCountyData, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                    county0 = GetAndInitIfNeeded(clinicalCountyDetails, 0);
                    ArrangeData(county0, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                    clinicalRegionData = GetAndInitIfNeeded(clinicalRegionDetails, int.Parse(_hospitalRegion[hospitalid][0]));
                    ArrangeData(clinicalRegionData, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                    region0 = GetAndInitIfNeeded(clinicalRegionDetails, 0);
                    ArrangeData(region0, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                    foreach (var catid in _hospitalCategory[hospitalid])
                    {
                        clinicalCategoryData = GetAndInitIfNeeded(clinicalCategoryDetails, catid);
                        ArrangeData(clinicalCategoryData, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                        category0 = GetAndInitIfNeeded(clinicalCategoryDetails, 0);
                        ArrangeData(category0, clinicalId, ageid, raceid, sexid, primaryPayerid, dataSourceId, died, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                    }

                    processed++;
                    var progress = processed * 100 / rowCount;
                    if (_logMod == 0)
                        Console.Write("\r {0}/{1} [{2}%] [{3}] ", processed, rowCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2));

                    //------------ MemoryCheck -------------//
                    if (processed % rowsPerMemoryCheck == 0)
                    {
                        if (processed % maxPatchSize == 0)
                        {
                            flushMemory(dimention, acii, tempDir, clinicalClinicalDetails, clinicalHospitalDetails, clinicalCountyDetails, clinicalRegionDetails, clinicalCategoryDetails, clinical0, hospital0, county0, region0, category0);
                        }
                        else
                        {

                            try
                            {
                                MemoryFailPoint memoryFailPoint = new MemoryFailPoint(memoryFlushPoint);
                                memoryFailPoint.Dispose();
                            }
                            catch (InsufficientMemoryException)
                            {
                                flushMemory(dimention, acii, tempDir, clinicalClinicalDetails, clinicalHospitalDetails, clinicalCountyDetails, clinicalRegionDetails, clinicalCategoryDetails, clinical0, hospital0, county0, region0, category0);
                            }
                        }
                    }
                }
            } while (dataRead.NextResult());
            //-------- flush ---------//
            if (_logMod == 0)
                LogRuntime();
            if (_logMod == 0)
            {
#if DEBUG
            Console.WriteLine("\t..........flushing memory............");
#endif
            }

            GC.Collect();
            CreatePartialFiles(tempDir, dimention, dimention, clinicalClinicalDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalClinicalDetails.Clear();
            clinical0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Hospital", "Hospital", clinicalHospitalDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalHospitalDetails.Clear();
            hospital0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "County", "County", clinicalCountyDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalCountyDetails.Clear();
            county0.Clear();
            GC.Collect();


            CreatePartialFiles(tempDir, "Region", "Region", clinicalRegionDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalRegionDetails.Clear();
            region0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "HospitalType", "HospitalType", clinicalCategoryDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalCategoryDetails.Clear();
            category0.Clear();
            GC.Collect();

            conn.Close();
        }

        private static void flushMemory(string dimention, ASCIIEncoding acii, string tempDir, Dictionary<int, object> clinicalClinicalDetails, Dictionary<int, object> clinicalHospitalDetails, Dictionary<int, object> clinicalCountyDetails, Dictionary<int, object> clinicalRegionDetails, Dictionary<int, object> clinicalCategoryDetails, Dictionary<int, object> clinical0, Dictionary<int, object> hospital0, Dictionary<int, object> county0, Dictionary<int, object> region0, Dictionary<int, object> category0)
        {
            //-------- flush ---------//
            if (_logMod == 0)
                LogRuntime();
            if (_logMod == 0)
            {
#if DEBUG
            Console.WriteLine("\t..........flushing memory............");
#endif
            }

            GC.Collect();
            CreatePartialFiles(tempDir, dimention, dimention, clinicalClinicalDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalClinicalDetails.Clear();
            clinical0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Hospital", "Hospital", clinicalHospitalDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalHospitalDetails.Clear();
            hospital0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "County", "County", clinicalCountyDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalCountyDetails.Clear();
            county0.Clear();
            GC.Collect();


            CreatePartialFiles(tempDir, "Region", "Region", clinicalRegionDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalRegionDetails.Clear();
            region0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "HospitalType", "HospitalType", clinicalCategoryDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalCategoryDetails.Clear();
            category0.Clear();
            GC.Collect();

            _flushCount++;
        }

        private static void LogRuntime()
        {
            _stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = _stopWatch.Elapsed;

            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds);
            Console.WriteLine("RunTime " + elapsedTime);
            _stopWatch.Restart();
        }

        private static Dictionary<int, object> GetAndInitIfNeeded(Dictionary<int, object> dic, int id)
        {
            Dictionary<int, object> child;
            if (dic.Keys.Contains(id))
            {
                child = dic[id] as Dictionary<int, object>;
            }
            else
            {
                child = new Dictionary<int, object>();
                dic.Add(id, child);
            }
            return child;
        }

        private static void ArrangeData(Dictionary<int, object> clinicalData, int hospitalid, int ageid, int raceid, int sexid, int primaryPayerid, int dataSourceId, bool died, int quarter, 
                                        out Dictionary<int, object> hospitalData, out Dictionary<int, object> ageData, out Dictionary<int, object> ageGroupData, out Dictionary<int, object> raceData, 
                                        out Dictionary<int, object> raceGroupData, out Dictionary<int, object> sexData, out Dictionary<int, object> sexGroupData, out Dictionary<int, object> primaryPayerData, 
                                        out Dictionary<int, object> primaryPayerGroupData)
        {
            hospitalData = GetAndInitIfNeeded(clinicalData, hospitalid);
            hospitalData = AddQuarter(hospitalData, quarter);

            ageData = GetAndInitIfNeeded(hospitalData, 1);
            ageGroupData = GetAndInitIfNeeded(ageData, ageid);

            sexData = GetAndInitIfNeeded(hospitalData, 2);
            sexGroupData = GetAndInitIfNeeded(sexData, sexid);

            primaryPayerData = GetAndInitIfNeeded(hospitalData, 3);
            primaryPayerGroupData = GetAndInitIfNeeded(primaryPayerData, primaryPayerid);

            raceData = GetAndInitIfNeeded(hospitalData, 4);
            raceGroupData = GetAndInitIfNeeded(raceData, raceid);

            AddEDVisits2(ageGroupData, quarter);
            AddEDVisits2(sexGroupData, quarter);
            AddEDVisits2(primaryPayerGroupData, quarter);
            AddEDVisits2(raceGroupData, quarter);

            if(dataSourceId==1)
            {
                AddNumAdmitHosp2(ageGroupData, quarter);
                AddNumAdmitHosp2(sexGroupData, quarter);
                AddNumAdmitHosp2(primaryPayerGroupData, quarter);
                AddNumAdmitHosp2(raceGroupData, quarter);
            }
            if ( died)
            {
                if(dataSourceId == 2 )
                {
                    AddDiedEd2(ageGroupData, quarter);
                    AddDiedEd2(sexGroupData, quarter);
                    AddDiedEd2(primaryPayerGroupData, quarter);
                    AddDiedEd2(raceGroupData, quarter);
                }
                else
                {
                    AddDiedHosp2(ageGroupData, quarter);
                    AddDiedHosp2(sexGroupData, quarter);
                    AddDiedHosp2(primaryPayerGroupData, quarter);
                    AddDiedHosp2(raceGroupData, quarter);
                }
            } 
        }

        private static void CreatePartialFiles(string rootDir, string subDir1, string subDir2, Dictionary<int, object> clinicalClinicalDetails,
                                               string progressBarRemaining, string progressBarDone, ASCIIEncoding acii)
        {
            var location = rootDir.Substring(rootDir.LastIndexOf("\\", StringComparison.InvariantCulture) + 1) + "\\" + subDir1;

            var fileCount = clinicalClinicalDetails.Count;

            var processed = 0;
            var clinicalDetails = clinicalClinicalDetails.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var clinical in clinicalDetails)
            {
                Directory.CreateDirectory(rootDir + "\\" + subDir1 + "\\" + subDir2 + "_" + clinical.Key);
                using (var fs = new FileStream(
                    rootDir + "\\" + subDir1 + "\\" + subDir2 + "_" + clinical.Key + string.Format(@"\PartialBuffer_{0}.pb", _flushCount),
                    FileMode.CreateNew,
                    FileAccess.ReadWrite))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(fs, clinical.Value);

                    processed++;
                }

                var progress = processed * 100 / fileCount;
                if (_logMod == 0)
                    Console.Write("\r\t{4,-20}{0,4}/{1,-4} [{2,3}%] [{3}] \t", processed, fileCount, progress, progressBarDone.Substring(50 - (progress / 2)) + progressBarRemaining.Substring(progress / 2), location);
            }
            clinicalClinicalDetails.Clear();
            GC.Collect();

            if (_logMod == 0)
                LogRuntime();
        }

        private static Dictionary<int, object> AddQuarter(Dictionary<int, object> groupData, int quarter)
        {
            if (!groupData.ContainsKey(QUARTERS))
            {
                groupData.Add(QUARTERS, 0);
            }

            if ((int)groupData[QUARTERS] != quarter)
                groupData[QUARTERS] = quarter;

            return groupData;
        }

        private static void AddEDVisits(Dictionary<int, object> groupData, int quarter, int numberOfEDVisits = 1)
        {
            if (!groupData.Keys.Contains(NUM_ED_VISITS))
            {
                groupData.Add(NUM_ED_VISITS, new List<RptValue<int>>());
            }
            ((List<RptValue<int>>)groupData[NUM_ED_VISITS]).Add(new RptValue<int> { Quarter = quarter, Value = numberOfEDVisits });

            //if (!groupData.Keys.Contains(NUM_ED_VISITS))
            //{
            //    groupData.Add(NUM_ED_VISITS, 0);
            //}
            //groupData[NUM_ED_VISITS] = (int)groupData[NUM_ED_VISITS] + numberOfEDVisits;
        }

        private static void AddEDVisits2(Dictionary<int, object> groupData, int quarter, int numberOfEDVisits = 1)
        {
            if (!groupData.Keys.Contains(NUM_ED_VISITS))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(NUM_ED_VISITS, quarterly);

                // groupData.Add(NUM_ED_VISITS, new List<RptValue<int>>());
            }
            ((Dictionary<int, List<int>>)groupData[NUM_ED_VISITS])[quarter].Add(numberOfEDVisits);
        }

        private static void AddEDVisits(Dictionary<int, object> groupData, List<int> numberOfEDVisits, int quarter)
        {
            if (!groupData.ContainsKey(NUM_ED_VISITS))
            {
                groupData.Add(NUM_ED_VISITS, new List<RptValue<int>>());
            }

            //if (numberOfEDVisits.Any(d => !d.HasValue))
            //{
            //    List<int?> tempDischarges = new List<int?>();

            //    numberOfEDVisits.ForEach(d => { tempDischarges.Add(!d.HasValue ? 1 : d); });

            //    numberOfEDVisits = tempDischarges;
            //}

            ((List<RptValue<int>>)groupData[NUM_ED_VISITS]).AddRange(numberOfEDVisits.Select(d => new RptValue<int> { Quarter = quarter, Value = d }).ToList());
      
        }

        private static void AddEDVisits(Dictionary<int, object> groupData, Dictionary<int, List<int>> numberOfEDVisits)
        {
            if (!groupData.ContainsKey(NUM_ED_VISITS))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(NUM_ED_VISITS, quarterly);
            }

            foreach (var item in numberOfEDVisits)
                ((Dictionary<int, List<int>>)groupData[NUM_ED_VISITS])[item.Key].AddRange(item.Value);
        }

        private static void AddNumAdmitHosp(Dictionary<int, object> groupData, int quarter, int numberOfNumAdmitHosp = 1)
        {
            if (!groupData.Keys.Contains(NUM_ADMIT_HOSP))
            {
                groupData.Add(NUM_ADMIT_HOSP, new List<RptValue<int>>());
            }
            ((List<RptValue<int>>)groupData[NUM_ADMIT_HOSP]).Add(new RptValue<int> { Quarter = quarter, Value = numberOfNumAdmitHosp });

            //if (!groupData.Keys.Contains(NUM_ADMIT_HOSP))
            //{
            //    groupData.Add(NUM_ADMIT_HOSP, 0);
            //}
            //groupData[NUM_ADMIT_HOSP] = (int)groupData[NUM_ADMIT_HOSP] + numberOfNumAdmitHosp;
        }

        private static void AddNumAdmitHosp2(Dictionary<int, object> groupData, int quarter, int numberOfNumAdmitHosp = 1)
        {
            if (!groupData.Keys.Contains(NUM_ADMIT_HOSP))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(NUM_ADMIT_HOSP, quarterly);
            }
           ((Dictionary<int, List<int>>)groupData[NUM_ADMIT_HOSP])[quarter].Add(numberOfNumAdmitHosp);
        }

        private static void AddNumAdmitHosp(Dictionary<int, object> groupData, List<int> numberOfNumAdmitHosps, int quarter)
        {
            if (!groupData.ContainsKey(NUM_ADMIT_HOSP))
            {
                groupData.Add(NUM_ADMIT_HOSP, new List<RptValue<int>>());
            }

            //if (numberOfEDVisits.Any(d => !d.HasValue))
            //{
            //    List<int?> tempDischarges = new List<int?>();

            //    numberOfEDVisits.ForEach(d => { tempDischarges.Add(!d.HasValue ? 1 : d); });

            //    numberOfEDVisits = tempDischarges;
            //}

            ((List<RptValue<int>>)groupData[NUM_ADMIT_HOSP]).AddRange(numberOfNumAdmitHosps.Select(d => new RptValue<int> { Quarter = quarter, Value = d }).ToList());

        }

        private static void AddNumAdmitHosps(Dictionary<int, object> groupData, Dictionary<int, List<int>> numberOfNumAdmitHosps)
        {
            if (!groupData.ContainsKey(NUM_ADMIT_HOSP))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(NUM_ADMIT_HOSP, quarterly);
            }

            foreach (var item in numberOfNumAdmitHosps)
                ((Dictionary<int, List<int>>)groupData[NUM_ADMIT_HOSP])[item.Key].AddRange(item.Value);
        }

        private static void AddNumAdmitHosps(Dictionary<int, object> groupData, List<RptValue<int>> numberOfNumAdmitHosps)
        {
            if (!groupData.ContainsKey(NUM_ADMIT_HOSP))
            {
                groupData.Add(NUM_ADMIT_HOSP, new List<RptValue<int>>());
            }

           ((List<RptValue<int>>)groupData[NUM_ADMIT_HOSP]).AddRange(numberOfNumAdmitHosps);
        }

        private static void AddDiedEd(Dictionary<int, object> groupData, int quarter, int numberOfDiedEd = 1)
        {
            if (!groupData.Keys.Contains(DIED_ED))
            {
                groupData.Add(DIED_ED, new List<RptValue<int>>());
            }
            ((List<RptValue<int>>)groupData[DIED_ED]).Add(new RptValue<int> { Quarter = quarter, Value = numberOfDiedEd });

            //if (!groupData.Keys.Contains(DIED_ED))
            //{
            //    groupData.Add(DIED_ED, 0);
            //}
            //groupData[DIED_ED] = (int)groupData[DIED_ED] + numberOfDiedEd;
        }

        private static void AddDiedEd2(Dictionary<int, object> groupData, int quarter, int numberOfDiedEd = 1)
        {
            if (!groupData.Keys.Contains(DIED_ED))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(DIED_ED, quarterly);
            }

           ((Dictionary<int, List<int>>)groupData[DIED_ED])[quarter].Add(numberOfDiedEd);
        }

        //private static void AddDiedEd(Dictionary<int, object> groupData, List<int> numberOfDiedEds, int quarter)
        //{
        //    if (!groupData.ContainsKey(DIED_ED))
        //    {
        //        groupData.Add(DIED_ED, new List<RptValue<int>>());
        //    }

        //    //if (numberOfEDVisits.Any(d => !d.HasValue))
        //    //{
        //    //    List<int?> tempDischarges = new List<int?>();

        //    //    numberOfEDVisits.ForEach(d => { tempDischarges.Add(!d.HasValue ? 1 : d); });

        //    //    numberOfEDVisits = tempDischarges;
        //    //}

        //    ((List<RptValue<int>>)groupData[DIED_ED]).AddRange(numberOfDiedEds.Select(d => new RptValue<int> { Quarter = quarter, Value = d }).ToList());

        //}

        private static void AddDiedEds(Dictionary<int, object> groupData, Dictionary<int, List<int>> numberOfDiedEds)
        {
            if (!groupData.ContainsKey(DIED_ED))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                    quarterly.Add(i, new List<int>());

                groupData.Add(DIED_ED, quarterly);
            }

            foreach (var item in numberOfDiedEds)
                ((Dictionary<int, List<int>>)groupData[DIED_ED])[item.Key].AddRange(item.Value);
        }

        private static void AddDiedEds(Dictionary<int, object> groupData, List<RptValue<int>> numberOfDiedEds)
        {
            if (!groupData.ContainsKey(DIED_ED))
            {
                groupData.Add(DIED_ED, new List<RptValue<int>>());
            }

           ((List<RptValue<int>>)groupData[DIED_ED]).AddRange(numberOfDiedEds);
        }

        //private static void AddDiedHosp(Dictionary<int, object> groupData, int quarter, int numberOfDiedHosp = 1)
        //{
        //    if (!groupData.Keys.Contains(DIED_HOSP))
        //    {
        //        groupData.Add(DIED_HOSP, new List<RptValue<int>>());
        //    }
        //    ((List<RptValue<int>>)groupData[DIED_HOSP]).Add(new RptValue<int> { Quarter = quarter, Value = numberOfDiedHosp });

        //    //if (!groupData.Keys.Contains(DIED_HOSP))
        //    //{
        //    //    groupData.Add(DIED_HOSP, 0);
        //    //}
        //    //groupData[DIED_HOSP] = (int)groupData[DIED_HOSP] + numberOfDiedHosp;
        //}

        private static void AddDiedHosp2(Dictionary<int, object> groupData, int quarter, int numberOfDiedHosp = 1)
        {
            if (!groupData.Keys.Contains(DIED_HOSP))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(DIED_HOSP, quarterly);
            }
           ((Dictionary<int, List<int>>)groupData[DIED_HOSP])[quarter].Add(numberOfDiedHosp);
        }

        private static void AddDiedHosp(Dictionary<int, object> groupData, List<int> numberOfDiedHosps, int quarter)
        {
            if (!groupData.ContainsKey(DIED_HOSP))
            {
                groupData.Add(DIED_HOSP, new List<RptValue<int>>());
            }

            ((List<RptValue<int>>)groupData[DIED_HOSP]).AddRange(numberOfDiedHosps.Select(d => new RptValue<int> { Quarter = quarter, Value = d }).ToList());
        }

        private static void AddDiedHosps(Dictionary<int, object> groupData, Dictionary<int, List<int>> numberOfDiedHosps)
        {
            if (!groupData.ContainsKey(DIED_HOSP))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(DIED_HOSP, quarterly);
                //groupData.Add(DIED_HOSP, new List<RptValue<int>>());
            }

            foreach (var item in numberOfDiedHosps)
                ((Dictionary<int, List<int>>)groupData[DIED_HOSP])[item.Key].AddRange(item.Value);
        }

        private static void AddDiedHosps(Dictionary<int, object> groupData, List<RptValue<int>> numberOfDiedHosps)
        {
            if (!groupData.ContainsKey(DIED_HOSP))
            {
                groupData.Add(DIED_HOSP, new List<RptValue<int>>());
            }

            ((List<RptValue<int>>)groupData[DIED_HOSP]).AddRange(numberOfDiedHosps);
        }

        private static void InitEDNationalTotals()
        {
            string edNationalTotalsSQL = ConfigurationManager.AppSettings["EDNationalTotals_SQL"];
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = edNationalTotalsSQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();
            _edNationalTotals = new Dictionary<int, string>();
                        
            _edNationalTotals.Add(-1, "\"NumEdVisits\":-1,\"NumAdmitHosp\":-1,\"DiedEd\":-1,\"DiedHosp\":-1");
            while (dataRead.Read())
            {
                int id = dataRead.GetInt32(0);
                string value = string.Format("\"NumEdVisits\":{0},\"NumAdmitHosp\":{1},\"DiedEd\":{2},\"DiedHosp\":{3}",
                    dataRead.GetInt32(1),
                    dataRead.GetInt32(2),
                    dataRead.GetInt32(3),
                    dataRead.GetInt32(4)
                    );

                _edNationalTotals.Add(id, value);
            }
        }

        private static void CompilePartialsPerDimension(string tempPath, string newPath)
        {
            var tempdirs = Directory.EnumerateDirectories(tempPath);
            if (_logMod == 0)
                Console.WriteLine();
#if DEBUG
                Console.WriteLine("Compiling");
#endif
            foreach (var dir in tempdirs)
            {
                foreach (var l1 in Directory.EnumerateDirectories(dir))
                {
                    Console.WriteLine("Compiling: " + dir.Substring(dir.LastIndexOf("\\", StringComparison.InvariantCulture)) + l1.Substring(l1.LastIndexOf("\\", StringComparison.InvariantCulture)));

                    foreach (var l2 in Directory.EnumerateDirectories(l1))
                    {
                        if (!_applyOptimization)
                            CompilePartials(l2, l2.Replace(tempPath, newPath));
                        else
                            CompileOptmizedPartials(l2, l2.Replace(tempPath, newPath));
                    }
                }
            }

            GC.Collect();

            Thread.Sleep(1000);

            try
            {
                DeletedTempFiles(_tempPath);
            }
            catch
            {
                // Eat exception since files get deleted anyways.

                //while (Directory.GetFiles(_tempPath, "*.pdb", SearchOption.AllDirectories).ToList().Count > 0)
                //{
                //    DeletedTempFiles(_tempPath);
                //}
            }
            GC.Collect();
        }

        private static void DeletedTempFiles(string tempFilePath)
        {
            if (Directory.Exists(tempFilePath))
            {
                Directory.Delete(tempFilePath, true);
            }
        }

        private static void CompilePartials(string sourceDir, string targetDir)
        {
            var locationparts = sourceDir.Split(new [] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            var location = locationparts[locationparts.Length - 2] + "\\" + locationparts[locationparts.Length - 1];
            var folderName = locationparts[locationparts.Length - 1];
            var utilName = locationparts[locationparts.Length - 3];
            int utilID = 0;
            switch (utilName)
            {
                case "DRG":
                    utilID = 0;
                    break;
                case "MDC":
                    utilID = 1;
                    break;
                case "CCS":
                    utilID = 2;
                    break;
                case "PRCCS":
                    utilID = 3;
                    break;
            }

            int utilValue = 0;

            switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
            {
                case "DRG":
                case "MDC":
                case "CCS":
                case "PRCCS":
                    utilValue = int.Parse(folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1));
                    if (!_edNationalTotals.Keys.Contains(utilValue))
                    {
                        utilValue = -1;
                    }
                    break;
            }

#if DEBUG
            if (_logMod == 0)
                Console.Write("{0} ................loading Partial Buffer ......................", location);
#endif

            BinaryFormatter formatter = new BinaryFormatter();
            var dicId = sourceDir.Substring(sourceDir.LastIndexOf("_", StringComparison.InvariantCulture) + 1);
            Dictionary<int, object> dic = null;
            foreach (var pbf in Directory.EnumerateFiles(sourceDir, "PartialBuffer*").ToList())
            {
                FileStream pbfs = null;
                try
                {
                    while (pbfs == null)
                    {
                        try
                        {
                            pbfs = new FileStream(pbf, FileMode.Open);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("waiting for :{0}", pbf);
                            Thread.Sleep(100);
                        }
                    }
                    if (dic == null)
                    {
                        //dic = pbfs.Length > 0 ? (Dictionary<int, object>)formatter.Deserialize(pbfs) : new Dictionary<int, object>();
                        dic = (Dictionary<int, object>)formatter.Deserialize(pbfs);
                    }
                    else
                    {
                        var tempdic = (Dictionary<int, object>)formatter.Deserialize(pbfs);

                        //int quarter2 = 0;
                        foreach (var hosp in tempdic)
                        {
                            if (dic.Keys.Contains(hosp.Key))
                            {
                                var dichosp = (Dictionary<int, object>)dic[hosp.Key];
                                foreach (var cat in (Dictionary<int, object>)hosp.Value)
                                {
                                    if (cat.Key == QUARTERS)
                                    {
                                        //quarter2 = (int)cat.Value;
                                        continue;
                                    }

                                    if (dichosp.Keys.Contains(cat.Key))
                                    {
                                        var diccat = (Dictionary<int, object>)dichosp[cat.Key];
                                        foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                                        {
                                            if (diccat.Keys.Contains(catGroup.Key))
                                            {
                                                var diccatGroup = (Dictionary<int, object>)diccat[catGroup.Key];

                                                var edVists = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]);
                                                AddEDVisits(diccatGroup, edVists);

                                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                                    AddNumAdmitHosps(
                                                        diccatGroup,
                                                        ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]));
                                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED))
                                                    AddDiedEds(
                                                        diccatGroup,
                                                        ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]));
                                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP))
                                                    AddDiedHosps(
                                                        diccatGroup,
                                                        ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]));
                                            }
                                            else
                                            {
                                                diccat.Add(catGroup.Key, catGroup.Value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dichosp.Add(cat.Key, cat.Value);
                                    }
                                }
                            }
                            else
                            {
                                dic.Add(hosp.Key, hosp.Value);
                            }
                        }
                    }
                }
                finally
                {
                    if (pbfs != null)
                        pbfs.Dispose();
                }
            }

            GC.Collect();

           // var c = dic.Count;

            //float? averageCost;

            var fileCount = dic.Count + 2;
            ASCIIEncoding acii = new ASCIIEncoding();

            //------   --------//
            int processed = 0;
            int progress;
            //Console.WriteLine(drg.Key);

            Dictionary<int, object> combinedDetails = new Dictionary<int, object>();

            Dictionary<int, object> summary = new Dictionary<int, object>();
            Directory.CreateDirectory(targetDir);
            foreach (var hosp in dic)
            {
                using (FileStream fs = File.Create(targetDir + @"\details_" + hosp.Key + @".js"))
                {
                    string fileContent = "$.monahrq.emergencydischarge = {\"NationalData\" : [{" + _edNationalTotals[utilValue] + "}],"
                                         + "\"TableData\": [";
                    string catGroupData = "";
                    Dictionary<int, object> curentHospitalData = GetAndInitIfNeeded(summary, hosp.Key);

                    int quarter = 0;
                    foreach (var cat in (Dictionary<int, object>)hosp.Value)
                    {
                        if (cat.Key == QUARTERS)
                        {
                            quarter = (int)cat.Value;
                        }
                        else
                        {
                            Dictionary<int, object> combinedDetailsCat;
                            if (combinedDetails.Keys.Contains(cat.Key))
                            {
                                combinedDetailsCat = (Dictionary<int, object>)combinedDetails[cat.Key];
                            }
                            else
                            {
                                combinedDetailsCat = new Dictionary<int, object>();
                                combinedDetails.Add(cat.Key, combinedDetailsCat);
                            }

                            foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                            {
                                Dictionary<int, object> combinedDetailsCatGroup;
                                if (combinedDetailsCat.Keys.Contains(catGroup.Key))
                                {
                                    combinedDetailsCatGroup = (Dictionary<int, object>)combinedDetailsCat[catGroup.Key];
                                }
                                else
                                {
                                    combinedDetailsCatGroup = new Dictionary<int, object>();
                                    combinedDetailsCat.Add(catGroup.Key, combinedDetailsCatGroup);
                                }

                                if (cat.Key == 1)
                                {
                                    AddEDVisits(
                                        curentHospitalData,
                                        ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]));
                                    if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                        AddNumAdmitHosps(
                                            curentHospitalData,
                                            (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]);
                                    if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED))
                                        AddDiedEds(
                                            curentHospitalData,
                                            (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]);
                                    if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP))
                                        AddDiedHosps(
                                            curentHospitalData,
                                            (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]);
                                }

                                AddEDVisits(
                                    combinedDetailsCatGroup,
                                    (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]);
                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                    AddNumAdmitHosps(
                                        combinedDetailsCatGroup,
                                        (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]);
                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED))
                                    AddDiedEds(
                                        combinedDetailsCatGroup,
                                        (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]);
                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP))
                                    AddDiedHosps(
                                        combinedDetailsCatGroup,
                                        (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]);

                                if (!string.IsNullOrEmpty(catGroupData))
                                    catGroupData += ",";

                                catGroupData += "{\"ID\": " + hosp.Key + ",\"CatID\": " + cat.Key + ",\"CatVal\": " + catGroup.Key;

                                var numEdVisitsVal = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS])
                                        .SelectMany(d => d.Value)
                                        .Sum();
                                var numAdmitHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                                    ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP])
                                    .SelectMany(d => d.Value)
                                    .Sum()
                                    : 0;
                                var diedEdVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                                    ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]).SelectMany(d => d.Value).Sum()
                                    : 0;
                                var diedHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                                    ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP])
                                    .SelectMany(d => d.Value)
                                    .Sum()
                                    : 0;

                                if (numEdVisitsVal > 0 && numEdVisitsVal < _edVisitsSuppression)
                                    numEdVisitsVal = -2;

                                if (numAdmitHospVal > 0 && numAdmitHospVal < _admitHospitalSuppression)
                                    numAdmitHospVal = -2;

                                if (diedEdVal > 0 && diedEdVal < _diedEDSuppression)
                                    diedEdVal = -2;

                                if (diedHospVal > 0 && diedHospVal < _diedHospitalSuppression)
                                    diedHospVal = -2;

                                catGroupData += ",\"NumEdVisits\": " + numEdVisitsVal +
                                                ",\"NumAdmitHosp\" :" + numAdmitHospVal +
                                                ",\"DiedEd\" :" + diedEdVal +
                                                ",\"DiedHosp\" :" + diedHospVal;

                                for (var q = 1; q <= 4; q++)
                                {

                                    var numEdVisitsVal2 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS])[q]
                                            .Select(d => d)
                                            .Sum();
                                    var numAdmitHospVal2 = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP])[q]
                                        .Select(d => d)
                                        .Sum()
                                        : 0;
                                    var diedEdVal2 = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED])[q].Select(d => d).Sum()
                                        : 0;
                                    var diedHospVal2 = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP])[q].Select(d => d).Sum()
                                        : 0;

                                    if (numEdVisitsVal2 > 0 && numEdVisitsVal2 < _edVisitsSuppression)
                                        numEdVisitsVal2 = -2;

                                    if (numAdmitHospVal2 > 0 && numAdmitHospVal < _admitHospitalSuppression)
                                        numAdmitHospVal2 = -2;

                                    if (diedEdVal2 > 0 && diedEdVal2 < _diedEDSuppression)
                                        diedEdVal2 = -2;

                                    if (diedHospVal2 > 0 && diedHospVal2 < _diedHospitalSuppression)
                                        diedHospVal2 = -2;

                                    catGroupData += ",\"Q" + q + "_NumEdVisits\": " + numEdVisitsVal2 +
                                                    ",\"Q" + q + "_NumAdmitHosp\" :" + numAdmitHospVal2 +
                                                    ",\"Q" + q + "_DiedEd\" :" + diedEdVal2 +
                                                    ",\"Q" + q + "_DiedHosp\" :" + diedHospVal2;

                                }

                                catGroupData += "}";
                            }
                        }
                    }
                    fileContent += catGroupData + "]};";
                    byte[] result = acii.GetBytes(fileContent);
                    fs.Write(result, 0, result.Length);
                    processed++;
                    progress = processed * 100 / fileCount;
                }
                if (_logMod == 0)
                    Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

                ((Dictionary<int, object>)hosp.Value).Clear();

                GC.Collect();
            }
            dic.Clear();
            GC.Collect();

            using (var fsAll = File.Create(targetDir + @"\details.js"))
            {

                string fsAllString = "$.monahrq.emergencydischarge = {\"NationalData\" : [{" + _edNationalTotals[utilValue] + "}],"
                                     + "\"TableData\" : [";
                foreach (var cat in combinedDetails)
                {
                    foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                    {
                        fsAllString += "{\"ID\":" + dicId + ",\"CatID\":" + cat.Key + ",\"CatVal\":" + catGroup.Key;

                        var numEdVisitsVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ED_VISITS)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]).SelectMany(d => d.Value).Sum()
                            : 0;
                        var numAdmitHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]).SelectMany(d => d.Value).Sum()
                            : 0;
                        var diedEdVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]).SelectMany(d => d.Value).Sum()
                            : 0;
                        var diedHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]).SelectMany(d => d.Value).Sum()
                            : 0;

                        if (numEdVisitsVal > 0 && numEdVisitsVal < _edVisitsSuppression)
                            numEdVisitsVal = -2;

                        if (numAdmitHospVal > 0 && numAdmitHospVal < _admitHospitalSuppression)
                            numAdmitHospVal = -2;

                        if (diedEdVal > 0 && diedEdVal < _diedEDSuppression)
                            diedEdVal = -2;

                        if (diedHospVal > 0 && diedHospVal < _diedHospitalSuppression)
                            diedHospVal = -2;

                        fsAllString += ",\"NumEdVisits\": " + numEdVisitsVal + ",\"NumAdmitHosp\" :" + numAdmitHospVal + ",\"DiedEd\" :" + diedEdVal
                                       + ",\"DiedHosp\" :" + diedHospVal;

                        for (var q = 1; q <= 4; q++)
                        {
                            var numEdVisitsVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ED_VISITS)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum()
                                : 0);
                            var numAdmitHospVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum()
                                : 0);
                            var diedEdVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED])[q].Select(d => d).Sum()
                                : 0);
                            var diedHospVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP])[q].Select(d => d).Sum()
                                : 0);

                            if (numEdVisitsVal2 > 0 && numEdVisitsVal2 < _edVisitsSuppression)
                                numEdVisitsVal2 = -2;

                            if (numAdmitHospVal2 > 0 && numAdmitHospVal2 < _admitHospitalSuppression)
                                numAdmitHospVal2 = -2;

                            if (diedEdVal2 > 0 && diedEdVal2 < _diedEDSuppression)
                                diedEdVal2 = -2;

                            if (diedHospVal2 > 0 && diedHospVal2 < _diedHospitalSuppression)
                                diedHospVal2 = -2;

                            fsAllString += ",\"Q" + q + "_NumEdVisits\": " + numEdVisitsVal2 +
                                           ",\"Q" + q + "_NumAdmitHosp\" :" + numAdmitHospVal2 +
                                           ",\"Q" + q + "_DiedEd\" :" + diedEdVal2 +
                                           ",\"Q" + q + "_DiedHosp\" :" + diedHospVal2;
                        }

                        fsAllString += "},";
                    }
                }
                combinedDetails.Clear();
                GC.Collect();
                fsAllString += "]};";
                byte[] allresult = acii.GetBytes(fsAllString);
                fsAll.Write(allresult, 0, allresult.Length);
                processed++;
            }

            progress = processed * 100 / fileCount;
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            using (FileStream fsSummary = File.Create(targetDir + @"\summary.js"))
            {
                string nationalData = "$.monahrq.emergencydischarge = {\"NationalData\":[{" + _edNationalTotals[utilValue] + "}],";
                const string totalData = "\"TotalData\":[{{\"NumEdVisits\":{0},\"NumAdmitHosp\":{1},\"DiedEd\":{2},\"DiedHosp\":{3}," +
                                         "\"Q1_NumEdVisits\":{4},\"Q1_NumAdmitHosp\":{5},\"Q1_DiedEd\":{6},\"Q1_DiedHosp\":{7}," +
                                         "\"Q2_NumEdVisits\":{8},\"Q2_NumAdmitHosp\":{9},\"Q2_DiedEd\":{10},\"Q2_DiedHosp\":{11}," +
                                         "\"Q3_NumEdVisits\":{12},\"Q3_NumAdmitHosp\":{13},\"Q3_DiedEd\":{14},\"Q3_DiedHosp\":{15}," +
                                         "\"Q4_NumEdVisits\":{16},\"Q4_NumAdmitHosp\":{17},\"Q4_DiedEd\":{18},\"Q4_DiedHosp\":{19}}}],";
                string tableData = "\"TableData\":[";
                //float? MeanCosts;

                int totalNumEDVisits = 0;
                int totalNumAdmitHosp = 0;
                int totalDiedEd = 0;
                int totalDiedHosp = 0;

                int totalNumEDVisitsQ1 = 0;
                int totalNumAdmitHospQ1 = 0;
                int totalDiedEdQ1 = 0;
                int totalDiedHospQ1 = 0;

                int totalNumEDVisitsQ2 = 0;
                int totalNumAdmitHospQ2 = 0;
                int totalDiedEdQ2 = 0;
                int totalDiedHospQ2 = 0;

                int totalNumEDVisitsQ3 = 0;
                int totalNumAdmitHospQ3 = 0;
                int totalDiedEdQ3 = 0;
                int totalDiedHospQ3 = 0;

                int totalNumEDVisitsQ4 = 0;
                int totalNumAdmitHospQ4 = 0;
                int totalDiedEdQ4 = 0;
                int totalDiedHospQ4 = 0;

                foreach (var hosp in summary)
                {
                    tableData += "{";

                    switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
                    {
                        case "DRG":
                        case "MDC":
                        case "CCS":
                        case "PRCCS":
                            tableData += "\"HospitalID\":" + hosp.Key + ",\"RegionID\":" + _hospitalRegion[hosp.Key][0] + ",\"CountyID\":"
                                         + _hospitalRegion[hosp.Key][1] +
                                         ",\"Zip\":" + _hospitalRegion[hosp.Key][2] + ",\"HospitalType\":\"" + _hospitalCategory[hosp.Key][0] + "\"";
                            break;
                        default:
                            tableData += "\"CCSID\":" + hosp.Key;
                            break;
                    }

                    var numEdVisitsVal = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])
                            .SelectMany(d => d.Value)
                            .Sum();
                    var numAdmitHospVal = ((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP)
                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP]).SelectMany(d => d.Value).Sum()
                        : 0;
                    var diedEdVal = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED)
                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED]).SelectMany(d => d.Value).Sum()
                        : 0);
                    var diedHospVal = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP)
                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP]).SelectMany(d => d.Value).Sum()
                        : 0);

                    if (numEdVisitsVal > 0 && numEdVisitsVal < _edVisitsSuppression)
                        numEdVisitsVal = -2;

                    if (numAdmitHospVal > 0 && numAdmitHospVal < _admitHospitalSuppression)
                        numAdmitHospVal = -2;

                    if (diedEdVal > 0 && diedEdVal < _diedEDSuppression)
                        diedEdVal = -2;

                    if (diedHospVal > 0 && diedHospVal < _diedHospitalSuppression)
                        diedHospVal = -2;

                    tableData += ",\"NumEdVisits\":" + numEdVisitsVal +
                                 ",\"NumAdmitHosp\":" + numAdmitHospVal +
                                 ",\"DiedEd\":" + diedEdVal +
                                 ",\"DiedHosp\":" + diedHospVal;

                    for (var q = 1; q <= 4; q++)
                    {
                        var numEdVisitsVal2 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q]
                                .Select(d => d)
                                .Sum();
                        var numAdmitHospVal2 = ((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum()
                            : 0;
                        var diedEdVal2 = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum()
                            : 0);
                        var diedHospVal2 = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q].Select(d => d).Sum()
                            : 0);

                        if (numEdVisitsVal2 > 0 && numEdVisitsVal2 < _edVisitsSuppression)
                            numEdVisitsVal2 = -2;

                        if (numAdmitHospVal2 > 0 && numAdmitHospVal2 < _admitHospitalSuppression)
                            numAdmitHospVal2 = -2;

                        if (diedEdVal2 > 0 && diedEdVal2 < _diedEDSuppression)
                            diedEdVal2 = -2;

                        if (diedHospVal2 > 0 && diedHospVal2 < _diedHospitalSuppression)
                            diedHospVal2 = -2;

                        tableData += ",\"Q" + q + "_NumEdVisits\":" + numEdVisitsVal2 +
                                     ",\"Q" + q + "_NumAdmitHosp\":" + numAdmitHospVal2 +
                                     ",\"Q" + q + "_DiedEd\":" + diedEdVal2 +
                                     ",\"Q" + q + "_DiedHosp\":" + diedHospVal2;

                        if (q == 1)
                        {
                            totalNumEDVisitsQ1 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q]
                                    .Select(d => d)
                                    .Sum();

                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ1 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ1 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ1 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                        }

                        if (q == 2)
                        {
                            totalNumEDVisitsQ2 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q]
                                    .Select(d => d)
                                    .Sum();

                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ2 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ2 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ2 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                        }

                        if (q == 3)
                        {
                            totalNumEDVisitsQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q]
                                    .Select(d => d)
                                    .Sum();

                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                        }

                        if (q == 4)
                        {
                            totalNumEDVisitsQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q]
                                    .Select(d => d)
                                    .Sum();

                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q]
                                        .Select(d => d)
                                        .Sum();
                        }
                    }

                    tableData += "},";

                    totalNumEDVisits += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])
                            .SelectMany(d => d.Value)
                            .Sum();

                    if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                        totalNumAdmitHosp += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])
                                .SelectMany(d => d.Value)
                                .Sum();

                    if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                        totalDiedEd += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED]).SelectMany(d => d.Value).Sum();

                    if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                        totalDiedHosp += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])
                                .SelectMany(d => d.Value)
                                .Sum();
                }
                tableData += "]};";

                byte[] summaryResult = acii.GetBytes(
                    nationalData +
                    string.Format(
                        totalData,
                        totalNumEDVisits,
                        totalNumAdmitHosp,
                        totalDiedEd,
                        totalDiedHosp,

                        totalNumEDVisitsQ1,
                        totalNumAdmitHospQ1,
                        totalDiedEdQ1,
                        totalDiedHospQ1,

                        totalNumEDVisitsQ2,
                        totalNumAdmitHospQ2,
                        totalDiedEdQ2,
                        totalDiedHospQ2,

                        totalNumEDVisitsQ3,
                        totalNumAdmitHospQ3,
                        totalDiedEdQ3,
                        totalDiedHospQ3,

                        totalNumEDVisitsQ4,
                        totalNumAdmitHospQ4,
                        totalDiedEdQ4,
                        totalDiedHospQ4) + tableData);

                fsSummary.Write(summaryResult, 0, summaryResult.Length);
                processed++;
            }
            progress = processed * 100 / fileCount;
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            if (_logMod == 0)
                LogRuntime();
            //-----------------//
        }

        private static void CompileOptmizedPartials(string sourceDir, string targetDir)
        {
            var locationparts = sourceDir.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            var location = locationparts[locationparts.Length - 2] + "\\" + locationparts[locationparts.Length - 1];
            var folderName = locationparts[locationparts.Length - 1];
            var utilName = locationparts[locationparts.Length - 3];
            int utilID = 0;
            switch (utilName)
            {
                case "DRG":
                    utilID = 0;
                    break;
                case "MDC":
                    utilID = 1;
                    break;
                case "CCS":
                    utilID = 2;
                    break;
                case "PRCCS":
                    utilID = 3;
                    break;
            }

            int utilValue = 0;

            switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
            {
                case "DRG":
                case "MDC":
                case "CCS":
                case "PRCCS":
                    utilValue = int.Parse(folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1));
                    if (!_edNationalTotals.Keys.Contains(utilValue))
                    {
                        utilValue = -1;
                    }
                    break;
            }

#if DEBUG
            if (_logMod == 0)
                Console.Write("{0} ................loading Partial Buffer ......................", location);
#endif

            BinaryFormatter formatter = new BinaryFormatter();
            var dicId = sourceDir.Substring(sourceDir.LastIndexOf("_", StringComparison.InvariantCulture) + 1);
            Dictionary<int, object> dic = null;
            foreach (var pbf in Directory.EnumerateFiles(sourceDir, "PartialBuffer*").ToList())
            {
                FileStream pbfs = null;
                try
                {
                    while (pbfs == null)
                    {
                        try
                        {
                            pbfs = new FileStream(pbf, FileMode.Open);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("waiting for :{0}", pbf);
                            Thread.Sleep(100);
                        }
                    }
                    if (dic == null)
                    {
                        //dic = pbfs.Length > 0 ? (Dictionary<int, object>)formatter.Deserialize(pbfs) : new Dictionary<int, object>();
                        dic = (Dictionary<int, object>)formatter.Deserialize(pbfs);
                    }
                    else
                    {
                        var tempdic = (Dictionary<int, object>)formatter.Deserialize(pbfs);

                        int quarter2 = 0;
                        foreach (var hosp in tempdic)
                        {
                            if (dic.Keys.Contains(hosp.Key))
                            {
                                var dichosp = (Dictionary<int, object>)dic[hosp.Key];
                                foreach (var cat in (Dictionary<int, object>)hosp.Value)
                                {
                                    if (cat.Key == QUARTERS)
                                    {
                                        quarter2 = (int)cat.Value;
                                        continue;
                                    }

                                    if (dichosp.Keys.Contains(cat.Key))
                                    {
                                        var diccat = (Dictionary<int, object>)dichosp[cat.Key];
                                        foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                                        {
                                            if (diccat.Keys.Contains(catGroup.Key))
                                            {
                                                var diccatGroup = (Dictionary<int, object>)diccat[catGroup.Key];

                                                var edVists = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]);
                                                AddEDVisits(diccatGroup, edVists);
                                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                                    AddNumAdmitHosps(
                                                        diccatGroup,
                                                        (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]);
                                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED))
                                                    AddDiedEds(
                                                        diccatGroup,
                                                        (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]);
                                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP))
                                                    AddDiedHosps(
                                                        diccatGroup,
                                                        (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]);
                                            }
                                            else
                                            {
                                                diccat.Add(catGroup.Key, catGroup.Value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dichosp.Add(cat.Key, cat.Value);
                                    }
                                }
                            }
                            else
                            {
                                dic.Add(hosp.Key, hosp.Value);
                            }
                        }
                    }
                }
                finally
                {
                    if (pbfs != null)
                        pbfs.Dispose();
                }
            }

            GC.Collect();

            // var c = dic.Count;

            //float? averageCost;

            var fileCount = dic.Count + 2;
            ASCIIEncoding acii = new ASCIIEncoding();

            //------   --------//
            int processed = 0;
            int progress;
            //Console.WriteLine(drg.Key);

            Dictionary<int, object> combinedDetails = new Dictionary<int, object>();

            Dictionary<int, object> summary = new Dictionary<int, object>();
            Directory.CreateDirectory(targetDir);

            var levelIdGroupData = new StringBuilder();
            var idGroupData = new StringBuilder();
            var catIdGroupData = new StringBuilder();
            var catValueGroupData = new StringBuilder();
            var numEdVisitsGroupData = new StringBuilder();
            var numAdmitHospsGroupData = new StringBuilder();
            var diedEdGroupData = new StringBuilder();
            var diedHospGroupData = new StringBuilder();

            // Quarterly values
            var q1numEdVisitsGroupData = new StringBuilder();
            var q1numAdmitHospsGroupData = new StringBuilder();
            var q1diedEdGroupData = new StringBuilder();
            var q1diedHospGroupData = new StringBuilder();
           
            var q2numEdVisitsGroupData = new StringBuilder();
            var q2numAdmitHospsGroupData = new StringBuilder();
            var q2diedEdGroupData = new StringBuilder();
            var q2diedHospGroupData = new StringBuilder();
            
            var q3numEdVisitsGroupData = new StringBuilder();
            var q3numAdmitHospsGroupData = new StringBuilder();
            var q3diedEdGroupData = new StringBuilder();
            var q3diedHospGroupData = new StringBuilder();
            
            var q4numEdVisitsGroupData = new StringBuilder();
            var q4numAdmitHospsGroupData = new StringBuilder();
            var q4diedEdGroupData = new StringBuilder();
            var q4diedHospGroupData = new StringBuilder();
           
            using (var fs = File.Create(targetDir + @"\details.js"))
            {
                string fileContent = "$.monahrq.emergencydischarge = {\"NationalData\" : [{" + _edNationalTotals[utilValue] + "}]," + "\"TableData\": [{";

                foreach (var hosp in dic)
                {

                    Dictionary<int, object> curentHospitalData = GetAndInitIfNeeded(summary, hosp.Key);

                    levelIdGroupData.Append(hosp.Key + ",");

                    idGroupData.Append("[");
                    catIdGroupData.Append("[");

                    catValueGroupData.Append("[");
                    numEdVisitsGroupData.Append("[");
                    numAdmitHospsGroupData.Append("[");
                    diedEdGroupData.Append("[");
                    diedHospGroupData.Append("[");

                    q1numEdVisitsGroupData.Append("[");
                    q1numAdmitHospsGroupData.Append("[");
                    q1diedEdGroupData.Append("[");
                    q1diedHospGroupData.Append("[");

                    q2numEdVisitsGroupData.Append("[");
                    q2numAdmitHospsGroupData.Append("[");
                    q2diedEdGroupData.Append("[");
                    q2diedHospGroupData.Append("[");

                    q3numEdVisitsGroupData.Append("[");
                    q3numAdmitHospsGroupData.Append("[");
                    q3diedEdGroupData.Append("[");
                    q3diedHospGroupData.Append("[");

                    q4numEdVisitsGroupData.Append("[");
                    q4numAdmitHospsGroupData.Append("[");
                    q4diedEdGroupData.Append("[");
                    q4diedHospGroupData.Append("[");

                    int quarter = 0;
                    foreach (var cat in (Dictionary<int, object>)hosp.Value)
                    {
                        if (cat.Key == QUARTERS)
                        {
                            quarter = (int)cat.Value;
                        }
                        else
                        {

                            Dictionary<int, object> combinedDetailsCat;
                            if (combinedDetails.Keys.Contains(cat.Key))
                            {
                                combinedDetailsCat = (Dictionary<int, object>)combinedDetails[cat.Key];
                            }
                            else
                            {
                                combinedDetailsCat = new Dictionary<int, object>();
                                combinedDetails.Add(cat.Key, combinedDetailsCat);
                            }

                        

                            foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                            {
                                idGroupData.Append(targetDir.SubStrAfterLast("_").Replace("\\", null) + ",");
                                catIdGroupData.Append(cat.Key + ",");
                                catValueGroupData.Append(catGroup.Key + ",");

                                Dictionary<int, object> combinedDetailsCatGroup;
                                if (combinedDetailsCat.Keys.Contains(catGroup.Key))
                                {
                                    combinedDetailsCatGroup = (Dictionary<int, object>)combinedDetailsCat[catGroup.Key];
                                }
                                else
                                {
                                    combinedDetailsCatGroup = new Dictionary<int, object>();
                                    combinedDetailsCat.Add(catGroup.Key, combinedDetailsCatGroup);
                                }

                                if (cat.Key == 1)
                                {
                                    AddEDVisits(curentHospitalData, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]);
                                    if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                        AddNumAdmitHosps(curentHospitalData, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]);
                                    if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED))
                                        AddDiedEds(curentHospitalData, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]);
                                    if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP))
                                        AddDiedHosps(curentHospitalData, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]);
                                }

                                AddEDVisits(combinedDetailsCatGroup, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]);

                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                    AddNumAdmitHosps(combinedDetailsCatGroup, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]);
                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED))
                                    AddDiedEds(combinedDetailsCatGroup, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]);
                                if (((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP))
                                    AddDiedHosps(combinedDetailsCatGroup, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]);
                           


                                var numEdVisitsVal = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]).SelectMany(d => d.Value).Sum();
                                var numAdmitHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                                                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]).SelectMany(d => d.Value).Sum()
                                                        : 0;
                                var diedEdVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                                                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]).SelectMany(d => d.Value).Sum()
                                                        : 0;
                                var diedHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                                                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]).SelectMany(d => d.Value).Sum()
                                                        : 0;

                                if (numEdVisitsVal > 0 && numEdVisitsVal < _edVisitsSuppression)
                                    numEdVisitsVal = -2;

                                if (numAdmitHospVal > 0 && numAdmitHospVal < _admitHospitalSuppression)
                                    numAdmitHospVal = -2;

                                if (diedEdVal > 0 && diedEdVal < _diedEDSuppression)
                                    diedEdVal = -2;

                                if (diedHospVal > 0 && diedHospVal < _diedHospitalSuppression)
                                    diedHospVal = -2;

                                numEdVisitsGroupData.Append(numEdVisitsVal + ",");
                                numAdmitHospsGroupData.Append(numAdmitHospVal + ",");
                                diedEdGroupData.Append(diedEdVal + ",");
                                diedHospGroupData.Append(diedHospVal + ",");



                                for (var q = 1; q <= 4; q++)
                                {
                                        var numEdVisitsVal2 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum();
                                        var numAdmitHospVal2 = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                                                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum()
                                                                : 0;
                                        var diedEdVal2 = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                                                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED])[q].Select(d => d).Sum()
                                                                : 0;
                                        var diedHospVal2 = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                                                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP])[q].Select(d => d).Sum()
                                                                : 0;

                                        if (numEdVisitsVal2 > 0 && numEdVisitsVal2 < _edVisitsSuppression)
                                            numEdVisitsVal2 = -2;

                                        if (numAdmitHospVal2 > 0 && numAdmitHospVal < _admitHospitalSuppression)
                                            numAdmitHospVal2 = -2;

                                        if (diedEdVal2 > 0 && diedEdVal2 < _diedEDSuppression)
                                            diedEdVal2 = -2;

                                        if (diedHospVal2 > 0 && diedHospVal2 < _diedHospitalSuppression)
                                            diedHospVal2 = -2;

                                        if (q == 1)
                                        {
                                            q1numEdVisitsGroupData.Append(numEdVisitsVal + ",");
                                            q1numAdmitHospsGroupData.Append(numAdmitHospVal + ",");
                                            q1diedEdGroupData.Append(diedEdVal + ",");
                                            q1diedHospGroupData.Append(diedHospVal + ",");
                                        }

                                        if (q == 2)
                                        {
                                            q2numEdVisitsGroupData.Append(numEdVisitsVal + ",");
                                            q2numAdmitHospsGroupData.Append(numAdmitHospVal + ",");
                                            q2diedEdGroupData.Append(diedEdVal + ",");
                                            q2diedHospGroupData.Append(diedHospVal + ",");
                                        }

                                        if (q == 3)
                                        {
                                            q3numEdVisitsGroupData.Append(numEdVisitsVal + ",");
                                            q3numAdmitHospsGroupData.Append(numAdmitHospVal + ",");
                                            q3diedEdGroupData.Append(diedEdVal + ",");
                                            q3diedHospGroupData.Append(diedHospVal + ",");
                                        }

                                        if (q == 4)
                                        {
                                            q4numEdVisitsGroupData.Append(numEdVisitsVal + ",");
                                            q4numAdmitHospsGroupData.Append(numAdmitHospVal + ",");
                                            q4diedEdGroupData.Append(diedEdVal + ",");
                                            q4diedHospGroupData.Append(diedHospVal + ",");
                                        }


                                }


                                GC.Collect();
                            }

                        }

                    }

                    idGroupData = new StringBuilder(idGroupData.ToString().SubStrBeforeLast(",") + "],");
                    catIdGroupData = new StringBuilder(catIdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    catValueGroupData = new StringBuilder(catValueGroupData.ToString().SubStrBeforeLast(",") + "],");
                    numEdVisitsGroupData = new StringBuilder(numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    numAdmitHospsGroupData = new StringBuilder(numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    diedEdGroupData = new StringBuilder(diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    diedHospGroupData = new StringBuilder(diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q1numEdVisitsGroupData = new StringBuilder(q1numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1numAdmitHospsGroupData = new StringBuilder(q1numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1diedEdGroupData = new StringBuilder(q1diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1diedHospGroupData = new StringBuilder(q1diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q2numEdVisitsGroupData = new StringBuilder(q2numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2numAdmitHospsGroupData = new StringBuilder(q2numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2diedEdGroupData = new StringBuilder(q2diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2diedHospGroupData = new StringBuilder(q2diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q3numEdVisitsGroupData = new StringBuilder(q3numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3numAdmitHospsGroupData = new StringBuilder(q3numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3diedEdGroupData = new StringBuilder(q3diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3diedHospGroupData = new StringBuilder(q3diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q4numEdVisitsGroupData = new StringBuilder(q4numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4numAdmitHospsGroupData = new StringBuilder(q4numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4diedEdGroupData = new StringBuilder(q4diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4diedHospGroupData = new StringBuilder(q4diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                    ((Dictionary<int, object>)hosp.Value).Clear();

                    GC.Collect();
                }

                #region Codes for LevelID=0 (Summary)

                idGroupData.Append("[");
                catIdGroupData.Append("[");

                catValueGroupData.Append("[");
                numEdVisitsGroupData.Append("[");
                numAdmitHospsGroupData.Append("[");
                diedEdGroupData.Append("[");
                diedHospGroupData.Append("[");

                q1numEdVisitsGroupData.Append("[");
                q1numAdmitHospsGroupData.Append("[");
                q1diedEdGroupData.Append("[");
                q1diedHospGroupData.Append("[");

                q2numEdVisitsGroupData.Append("[");
                q2numAdmitHospsGroupData.Append("[");
                q2diedEdGroupData.Append("[");
                q2diedHospGroupData.Append("[");

                q3numEdVisitsGroupData.Append("[");
                q3numAdmitHospsGroupData.Append("[");
                q3diedEdGroupData.Append("[");
                q3diedHospGroupData.Append("[");

                q4numEdVisitsGroupData.Append("[");
                q4numAdmitHospsGroupData.Append("[");
                q4diedEdGroupData.Append("[");
                q4diedHospGroupData.Append("[");

                foreach (var cat in combinedDetails)
                {
                    foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                    {
                        idGroupData.Append(targetDir.SubStrAfterLast("_").Replace("\\", null) + ",");
                        catIdGroupData.Append(cat.Key + ",");
                        catValueGroupData.Append(catGroup.Key + ",");

                        var numEdVisitsVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ED_VISITS)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS]).SelectMany(d => d.Value).Sum()
                            : 0;
                        var numAdmitHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP]).SelectMany(d => d.Value).Sum()
                            : 0;
                        var diedEdVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED]).SelectMany(d => d.Value).Sum()
                            : 0;
                        var diedHospVal = ((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP]).SelectMany(d => d.Value).Sum()
                            : 0;

                        if (numEdVisitsVal > 0 && numEdVisitsVal < _edVisitsSuppression)
                            numEdVisitsVal = -2;

                        if (numAdmitHospVal > 0 && numAdmitHospVal < _admitHospitalSuppression)
                            numAdmitHospVal = -2;

                        if (diedEdVal > 0 && diedEdVal < _diedEDSuppression)
                            diedEdVal = -2;

                        if (diedHospVal > 0 && diedHospVal < _diedHospitalSuppression)
                            diedHospVal = -2;

                        numEdVisitsGroupData.Append(numEdVisitsVal + ",");
                        numAdmitHospsGroupData.Append(numAdmitHospVal + ",");
                        diedEdGroupData.Append(diedEdVal + ",");
                        diedHospGroupData.Append(diedHospVal + ",");

                        for (var q = 1; q <= 4; q++)
                        {
                            var numEdVisitsVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ED_VISITS)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum()
                                : 0);
                            var numAdmitHospVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(NUM_ADMIT_HOSP)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum()
                                : 0);
                            var diedEdVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_ED)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_ED])[q].Select(d => d).Sum()
                                : 0);
                            var diedHospVal2 = (int)(((Dictionary<int, object>)catGroup.Value).Keys.Contains(DIED_HOSP)
                                ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[DIED_HOSP])[q].Select(d => d).Sum()
                                : 0);

                            if (numEdVisitsVal2 > 0 && numEdVisitsVal2 < _edVisitsSuppression)
                                numEdVisitsVal2 = -2;

                            if (numAdmitHospVal2 > 0 && numAdmitHospVal2 < _admitHospitalSuppression)
                                numAdmitHospVal2 = -2;

                            if (diedEdVal2 > 0 && diedEdVal2 < _diedEDSuppression)
                                diedEdVal2 = -2;

                            if (diedHospVal2 > 0 && diedHospVal2 < _diedHospitalSuppression)
                                diedHospVal2 = -2;

                            switch (q)
                            {
                                case 1:
                                    q1numEdVisitsGroupData.Append(numEdVisitsVal2 + ",");
                                    q1numAdmitHospsGroupData.Append(numAdmitHospVal2 + ",");
                                    q1diedEdGroupData.Append(diedEdVal2 + ",");
                                    q1diedHospGroupData.Append(diedHospVal2 + ",");
                                    break;
                                case 2:
                                    q2numEdVisitsGroupData.Append(numEdVisitsVal2 + ",");
                                    q2numAdmitHospsGroupData.Append(numAdmitHospVal2 + ",");
                                    q2diedEdGroupData.Append(diedEdVal2 + ",");
                                    q2diedHospGroupData.Append(diedHospVal2 + ",");
                                    break;
                                case 3:
                                    q3numEdVisitsGroupData.Append(numEdVisitsVal2 + ",");
                                    q3numAdmitHospsGroupData.Append(numAdmitHospVal2 + ",");
                                    q3diedEdGroupData.Append(diedEdVal2 + ",");
                                    q3diedHospGroupData.Append(diedHospVal2 + ",");
                                    break;
                                case 4:
                                    q4numEdVisitsGroupData.Append(numEdVisitsVal2 + ",");
                                    q4numAdmitHospsGroupData.Append(numAdmitHospVal2 + ",");
                                    q4diedEdGroupData.Append(diedEdVal2 + ",");
                                    q4diedHospGroupData.Append(diedHospVal2 + ",");
                                    break;
                            }
                        }
                    }
                }



                idGroupData = new StringBuilder(idGroupData.ToString().SubStrBeforeLast(",") + "],");
                catIdGroupData = new StringBuilder(catIdGroupData.ToString().SubStrBeforeLast(",") + "],");
                catValueGroupData = new StringBuilder(catValueGroupData.ToString().SubStrBeforeLast(",") + "],");
                numEdVisitsGroupData = new StringBuilder(numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                numAdmitHospsGroupData = new StringBuilder(numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                diedEdGroupData = new StringBuilder(diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                diedHospGroupData = new StringBuilder(diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                q1numEdVisitsGroupData = new StringBuilder(q1numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1numAdmitHospsGroupData = new StringBuilder(q1numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1diedEdGroupData = new StringBuilder(q1diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1diedHospGroupData = new StringBuilder(q1diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                q2numEdVisitsGroupData = new StringBuilder(q2numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2numAdmitHospsGroupData = new StringBuilder(q2numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2diedEdGroupData = new StringBuilder(q2diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2diedHospGroupData = new StringBuilder(q2diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                q3numEdVisitsGroupData = new StringBuilder(q3numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3numAdmitHospsGroupData = new StringBuilder(q3numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3diedEdGroupData = new StringBuilder(q3diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3diedHospGroupData = new StringBuilder(q3diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                q4numEdVisitsGroupData = new StringBuilder(q4numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4numAdmitHospsGroupData = new StringBuilder(q4numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4diedEdGroupData = new StringBuilder(q4diedEdGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4diedHospGroupData = new StringBuilder(q4diedHospGroupData.ToString().SubStrBeforeLast(",") + "],");

                combinedDetails.Clear();
                GC.Collect();
                #endregion

                fileContent += "\"LevelID\":[" + levelIdGroupData.ToString().SubStrBeforeLast(",") + ", 0]," +
                               "\"ID\":[" + idGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"CatID\":[" + catIdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"CatVal\":[" + catValueGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"NumEdVisits\":[" + numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"NumAdmitHosp\":[" + numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"DiedEd\":[" + diedEdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"DiedHosp\":[" + diedHospGroupData.ToString().SubStrBeforeLast(",") + "]," +

                               "\"Q1_NumEdVisits\":[" + q1numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_NumAdmitHosp\":[" + q1numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_DiedEd\":[" + q1diedEdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_DiedHosp\":[" + q1diedHospGroupData.ToString().SubStrBeforeLast(",") + "]," +

                               "\"Q2_NumEdVisits\":[" + q2numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_NumAdmitHosp\":[" + q2numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_DiedEd\":[" + q2diedEdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_DiedHosp\":[" + q2diedHospGroupData.ToString().SubStrBeforeLast(",") + "]," +

                               "\"Q3_NumEdVisits\":[" + q3numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_NumAdmitHosp\":[" + q3numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_DiedEd\":[" + q3diedEdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_DiedHosp\":[" + q3diedHospGroupData.ToString().SubStrBeforeLast(",") + "]," +

                               "\"Q4_NumEdVisits\":[" + q4numEdVisitsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_NumAdmitHosp\":[" + q4numAdmitHospsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_DiedEd\":[" + q4diedEdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_DiedHosp\":[" + q4diedHospGroupData.ToString().SubStrBeforeLast(",") + "]" +
                               "}]};";

                byte[] result = acii.GetBytes(fileContent);
                fs.Write(result, 0, result.Length);
                processed++;
                progress = processed * 100 / fileCount;

                if (_logMod == 0)
                    Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);
            
                GC.Collect();
            }
            dic.Clear();
            GC.Collect();

            processed++;

            progress = processed * 100 / fileCount;
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            using (var fsSummary = File.Create(targetDir + @"\summary.js"))
            {
                string nationalData = "$.monahrq.emergencydischarge = {\"NationalData\":[{" + _edNationalTotals[utilValue] + "}],";

                string tableData = "\"TableData\":[";
                //float? MeanCosts;

                int totalNumEDVisits = 0;
                int totalNumAdmitHosp = 0;
                int totalDiedEd = 0;
                int totalDiedHosp = 0;

                int totalNumEDVisitsQ1 = 0;
                int totalNumAdmitHospQ1 = 0;
                int totalDiedEdQ1 = 0;
                int totalDiedHospQ1 = 0;

                int totalNumEDVisitsQ2 = 0;
                int totalNumAdmitHospQ2 = 0;
                int totalDiedEdQ2 = 0;
                int totalDiedHospQ2 = 0;

                int totalNumEDVisitsQ3 = 0;
                int totalNumAdmitHospQ3 = 0;
                int totalDiedEdQ3 = 0;
                int totalDiedHospQ3 = 0;

                int totalNumEDVisitsQ4 = 0;
                int totalNumAdmitHospQ4 = 0;
                int totalDiedEdQ4 = 0;
                int totalDiedHospQ4 = 0;

                foreach (var hosp in summary)
                {
                    tableData += "{";

                    switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
                    {
                        case "DRG":
                        case "MDC":
                        case "CCS":
                        case "PRCCS":
                            tableData += "\"HospitalID\":" + hosp.Key + ",\"RegionID\":" + _hospitalRegion[hosp.Key][0] + ",\"CountyID\":" + _hospitalRegion[hosp.Key][1] +
                                         ",\"Zip\":" + _hospitalRegion[hosp.Key][2] + ",\"HospitalType\":\"" + _hospitalCategory[hosp.Key][0] + "\"";
                            break;
                        default:
                            tableData += "\"CCSID\":" + hosp.Key;
                            break;
                    }

                    var numEdVisitsVal = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS]).SelectMany(d => d.Value).Sum();
                    var numAdmitHospVal = ((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP)
                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP]).SelectMany(d => d.Value).Sum()
                        : 0;
                    var diedEdVal = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED)
                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED]).SelectMany(d => d.Value).Sum()
                        : 0);
                    var diedHospVal = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP)
                        ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP]).SelectMany(d => d.Value).Sum()
                        : 0);

                    if (numEdVisitsVal > 0 && numEdVisitsVal < _edVisitsSuppression)
                        numEdVisitsVal = -2;

                    if (numAdmitHospVal > 0 && numAdmitHospVal < _admitHospitalSuppression)
                        numAdmitHospVal = -2;

                    if (diedEdVal > 0 && diedEdVal < _diedEDSuppression)
                        diedEdVal = -2;

                    if (diedHospVal > 0 && diedHospVal < _diedHospitalSuppression)
                        diedHospVal = -2;

                    tableData += ",\"NumEdVisits\":" + numEdVisitsVal +
                                 ",\"NumAdmitHosp\":" + numAdmitHospVal +
                                 ",\"DiedEd\":" + diedEdVal +
                                 ",\"DiedHosp\":" + diedHospVal;

                    for (var q = 1; q <= 4; q++)
                    {
                        var numEdVisitsVal2 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum();
                        var numAdmitHospVal2 = ((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum()
                            : 0;
                        var diedEdVal2 = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum()
                            : 0);
                        var diedHospVal2 = (int)(((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP)
                            ? ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q].Select(d => d).Sum()
                            : 0);

                        if (numEdVisitsVal2 > 0 && numEdVisitsVal2 < _edVisitsSuppression)
                            numEdVisitsVal2 = -2;

                        if (numAdmitHospVal2 > 0 && numAdmitHospVal2 < _admitHospitalSuppression)
                            numAdmitHospVal2 = -2;

                        if (diedEdVal2 > 0 && diedEdVal2 < _diedEDSuppression)
                            diedEdVal2 = -2;

                        if (diedHospVal2 > 0 && diedHospVal2 < _diedHospitalSuppression)
                            diedHospVal2 = -2;

                        tableData += ",\"Q" + q + "_NumEdVisits\":" + numEdVisitsVal2 +
                                        ",\"Q" + q + "_NumAdmitHosp\":" + numAdmitHospVal2 +
                                        ",\"Q" + q + "_DiedEd\":" + diedEdVal2 +
                                        ",\"Q" + q + "_DiedHosp\":" + diedHospVal2;
                    
                        if (q == 1)
                        {
                            totalNumEDVisitsQ1 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ1 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>) hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ1 += ((Dictionary<int, List<int>>) ((Dictionary<int, object>) hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ1 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q].Select(d => d).Sum();
                        }

                        if (q == 2)
                        {
                            totalNumEDVisitsQ2 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ2 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ2 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>) hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ2 += ((Dictionary<int, List<int>>) ((Dictionary<int, object>) hosp.Value)[DIED_HOSP])[q].Select(d => d).Sum();
                        }

                        if (q == 3)
                        {
                            totalNumEDVisitsQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ3 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q].Select(d => d).Sum();
                        }

                        if (q == 4)
                        {
                            totalNumEDVisitsQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                                totalNumAdmitHospQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                                totalDiedEdQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED])[q].Select(d => d).Sum();
                            if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                                totalDiedHospQ4 += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP])[q].Select(d => d).Sum();
                        }
                    }

                    tableData += "},";

                    totalNumEDVisits += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ED_VISITS]).SelectMany(d => d.Value).Sum();

                    if (((Dictionary<int, object>)hosp.Value).Keys.Contains(NUM_ADMIT_HOSP))
                        totalNumAdmitHosp += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[NUM_ADMIT_HOSP]).SelectMany(d => d.Value).Sum();

                    if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_ED))
                        totalDiedEd += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_ED]).SelectMany(d => d.Value).Sum();

                    if (((Dictionary<int, object>)hosp.Value).Keys.Contains(DIED_HOSP))
                        totalDiedHosp += ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[DIED_HOSP]).SelectMany(d => d.Value).Sum();
                }
                tableData += "]};";

                const string totalData = "\"TotalData\":[{{\"NumEdVisits\":{0},\"NumAdmitHosp\":{1},\"DiedEd\":{2},\"DiedHosp\":{3}," +
                             "\"Q1_NumEdVisits\":{4},\"Q1_NumAdmitHosp\":{5},\"Q1_DiedEd\":{6},\"Q1_DiedHosp\":{7}," +
                             "\"Q2_NumEdVisits\":{8},\"Q2_NumAdmitHosp\":{9},\"Q2_DiedEd\":{10},\"Q2_DiedHosp\":{11}," +
                             "\"Q3_NumEdVisits\":{12},\"Q3_NumAdmitHosp\":{13},\"Q3_DiedEd\":{14},\"Q3_DiedHosp\":{15}," +
                             "\"Q4_NumEdVisits\":{16},\"Q4_NumAdmitHosp\":{17},\"Q4_DiedEd\":{18},\"Q4_DiedHosp\":{19}}}],";

                byte[] summaryResult = acii.GetBytes(nationalData +
                                                     string.Format(totalData,
                                                     totalNumEDVisits,
                                                     totalNumAdmitHosp,
                                                     totalDiedEd,
                                                     totalDiedHosp,

                                                     totalNumEDVisitsQ1,
                                                     totalNumAdmitHospQ1,
                                                     totalDiedEdQ1,
                                                     totalDiedHospQ1,

                                                     totalNumEDVisitsQ2,
                                                     totalNumAdmitHospQ2,
                                                     totalDiedEdQ2,
                                                     totalDiedHospQ2,

                                                     totalNumEDVisitsQ3,
                                                     totalNumAdmitHospQ3,
                                                     totalDiedEdQ3,
                                                     totalDiedHospQ3,

                                                     totalNumEDVisitsQ4,
                                                     totalNumAdmitHospQ4,
                                                     totalDiedEdQ4,
                                                     totalDiedHospQ4) + tableData);

                fsSummary.Write(summaryResult, 0, summaryResult.Length);
                processed++;
            }

            progress = processed * 100 / fileCount;
            if (progress > 100)
                progress = 100;

            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            if (_logMod == 0)
                LogRuntime();
            //-----------------//
        }
    }

    [Serializable]
    internal class RptValue<T>
    {
        public T Value { get; set; }
        public int Quarter { get; set; }
    }

    internal static class Helper
    {
        public static bool In(this object obj, IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Contains(obj);
        }

        /// <summary>
        /// Returns the given potion of the string after the last occurrence of the [search] string. 
        /// an empty string [s] is returned if the [search] string is not found
        /// </summary>
        /// <param name="s">string to search</param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static string SubStrAfterLast(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(s.LastIndexOf(search) + search.Length) : string.Empty;
        }

        public static string SubStrBeforeLast(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(0, s.LastIndexOf(search)) : s;
        }
    }
}
