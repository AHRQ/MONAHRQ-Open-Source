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
using System.Threading.Tasks;

namespace RegionGenerator
{
    static class Helper
    {
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

        public static bool In(this object obj, IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Contains(obj);
        }

        public static float? Median(this List<float?> source, float? avarage)
        {
            if (avarage.HasValue)
            {
                for (var i = 0; i < source.Count; i++)
                {
                    if (!source[i].HasValue)
                        source[i] = avarage;
                }
            }

            source.Sort();
            if (source.Count == 0)
            {
                return -1;
            }
            if (source.Count % 2 == 1)
            {
                return source[source.Count / 2];

            }
            if (source[source.Count / 2] != null && source[source.Count / 2 - 1] != null)
                return (source[source.Count / 2].Value + source[source.Count / 2 - 1].Value) / 2;
            return -1;
        }
        public static float? Median(this List<int?> source)
        {
            source.Sort();
            if (source.Count == 0)
            {
                return -1;
            }
            if (source.Count % 2 == 1)
            {
                return source[source.Count / 2];

            }
            if (source[source.Count / 2] != null && source[source.Count / 2 - 1] != null)
                return (source[source.Count / 2].Value + source[source.Count / 2 - 1].Value) / 2;
            return -1;
        }
        public static double Median(this List<int> source)
        {
            source.Sort();
            if (source.Count == 0)
            {
                return -1;
            }
            if (source.Count % 2 == 1)
            {
                return source[source.Count / 2];

            }

            return (source[source.Count / 2] + source[source.Count / 2 - 1]) / 2d;

        }
        public static string ToPartialArrayString(this List<int?> source)
        {
            string result = "[";
            if (source.Count > 0)
                result += source[0];

            for (int i = 1; i < source.Count; i++)
            {
                result += "," + source[i];

            }
            result += "]";
            return result;
        }
        public static string ToPartialArrayString(this List<int> source)
        {
            string result = "[";
            if (source.Count > 0)
                result += source[0];

            for (int i = 1; i < source.Count; i++)
            {
                result += "," + source[i];

            }
            result += "]";
            return result;
        }
        public static string ToPartialArrayString(this List<float?> source)
        {
            string result = "[";
            if (source.Count > 0)
                result += source[0];

            for (int i = 1; i < source.Count; i++)
            {
                result += "," + source[i];

            }
            result += "]";
            return result;
        }


    }

    class Program
    {
        const int   ID = 0,
                    CAT_ID = 1,
                    CAT_VAL = 2,
                    DISCHARGES = 3,
                    MEAN_COSTS = 5,
                    MEDIAN_COSTS = 8,
                    COSTS = 11,
                    QUARTERS = 13;

        private const string PROGRESS_BAR_REMAINING = "                                                  ";
        private const string PROGRESS_BAR_DONE = "##################################################";
        
        static Stopwatch _stopWatch;

        static int _flushCount = 0;

        static List<Dictionary<int, string>> _ipNationalTotals = null;
        static Dictionary<string, int> _areaPopulationStrats = null;
        static int _logMod = 0;
        static string _tempPath;
        static string _rootDir;
        static string _mainConnectionString;
        static string _contentItemRecored;
        static int _suppression;
        static string _websiteId;
        static string _regionSource;
        static int _scale;
        static int _timeout;
        static string _year;
        static string _datasetyear;
        static List<int> _reportQuarters;
        static bool _applyOptimization;

        static void Main(string[] args)
        {
            try
            {

                _rootDir = ConfigurationManager.AppSettings["reportDir"];
                _websiteId = ConfigurationManager.AppSettings["websiteID"];
                _contentItemRecored = ConfigurationManager.AppSettings["ContentItemRecord"];
                _suppression = int.Parse(ConfigurationManager.AppSettings["Suppression"]);
                _scale = int.Parse(ConfigurationManager.AppSettings["Scale"]);
                _regionSource = ConfigurationManager.AppSettings["RegionSource"];
                _timeout = int.Parse(ConfigurationManager.AppSettings["Timeout"]);
                _year = ConfigurationManager.AppSettings["WebsiteYear"];
                _datasetyear = ConfigurationManager.AppSettings["DatasetYear"];
                _mainConnectionString = ConfigurationManager.ConnectionStrings["MAIN"].ToString();

                _applyOptimization = ConfigurationManager.AppSettings["ApplyOptimization"] == "1";
                _reportQuarters = ConfigurationManager.AppSettings["ReportQuarter"] != null
                   ? new List<int>(ConfigurationManager.AppSettings["ReportQuarter"].Split('|').Select(int.Parse).ToList())
                   : new List<int> { 1, 2, 3, 4 };

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
                        case "-i":
                            _contentItemRecored = args[i + 1];
                            break;
                        case "-s":
                            _suppression = int.Parse(args[i + 1]);
                            break;
                        case "-r":
                            _regionSource = args[i + 1];
                            break;
                        case "-l":
                            _logMod = int.Parse(args[i + 1]);
                            break;
                        case "-scale":
                            _scale = int.Parse(args[i + 1]);
                            break;
                        case "-t":
                            _timeout = int.Parse(args[i + 1]);
                            break;
                        case "-y":
                            _year = args[i + 1];
                            break;
                        case "-dy":
                            _datasetyear = args[i + 1];
                            break;
                        case "-wid":
                            _websiteId = args[i + 1];
                            break;
                        case "-o":
                            _applyOptimization = bool.Parse(args[i + 1]);
                            break;
                    }
                }
                //InitAreaPopulationStrats();

                var tempDirectoryPath = Path.GetTempPath();
               // while (!Directory.Exists(tempDirectoryPath + "Monahrq\\Generators\\RegionGenerator\\"))
                {
                    if (!Directory.Exists(tempDirectoryPath + "Monahrq\\"))
                        Directory.CreateDirectory(tempDirectoryPath + "Monahrq\\");
                    if (!Directory.Exists(tempDirectoryPath + "Monahrq\\"))
                        Directory.CreateDirectory(tempDirectoryPath + "Monahrq\\Generators\\");
                    if (!Directory.Exists(tempDirectoryPath + "Monahrq\\"))
                        Directory.CreateDirectory(tempDirectoryPath + "Monahrq\\Generators\\RegionGenerator\\");
                }
               
                _tempPath = tempDirectoryPath + "Monahrq\\Generators\\RegionGenerator\\" + Guid.NewGuid().ToString().Substring(0, 8);

                if (!Directory.Exists(_tempPath))
                    Directory.CreateDirectory(_tempPath);
                else
                    Directory.Delete(_tempPath, true);


                string rootDir = _rootDir;

                //---- delete old data -----//
                if (Directory.Exists(rootDir))
                {
                    if (_logMod == 0)
                        Console.WriteLine("deleteing old data ...");
                    Directory.Delete(rootDir, true);
                }

                _stopWatch = new Stopwatch();
                Stopwatch totalWatch = new Stopwatch();
                totalWatch.Start();

                //------ compile partial files -----//
                InitIPNationalTotals();
                InitAreaPopulationStrats();

                string drgSQL = ConfigurationManager.AppSettings["DRG_SQL"].Replace("[ContentItemRecord]", _contentItemRecored).Replace("[RegionSource]", _regionSource).Replace("[websiteID]", _websiteId);
                string mdcSQL = ConfigurationManager.AppSettings["MDC_SQL"].Replace("[ContentItemRecord]", _contentItemRecored).Replace("[RegionSource]", _regionSource).Replace("[websiteID]", _websiteId);
                string ccsSQL = ConfigurationManager.AppSettings["CCS_SQL"].Replace("[ContentItemRecord]", _contentItemRecored).Replace("[RegionSource]", _regionSource).Replace("[websiteID]", _websiteId);
                string prccsSQL = ConfigurationManager.AppSettings["PRCCS_SQL"].Replace("[ContentItemRecord]", _contentItemRecored).Replace("[RegionSource]", _regionSource).Replace("[websiteID]", _websiteId);

                GenerateDimension("DRG", drgSQL);
                GC.Collect();
                CompilePartialsPerDimension(_tempPath, rootDir);
                GC.Collect();
                GenerateDimension("MDC", mdcSQL);
                GC.Collect();
                CompilePartialsPerDimension(_tempPath, rootDir);
                GC.Collect();
                GenerateDimension("CCS", ccsSQL);
                GC.Collect();
                CompilePartialsPerDimension(_tempPath, rootDir);
                GC.Collect();
                GenerateDimension("PRCCS", prccsSQL);
                GC.Collect();
                CompilePartialsPerDimension(_tempPath, rootDir);
                GC.Collect();

                TimeSpan ts = totalWatch.Elapsed;
                // Format and display the TimeSpan value. 
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                Console.WriteLine("Region Rates Data Total RunTime: " + elapsedTime);

                //---- clear temp data -----------------//
                //GC.Collect();
                //Directory.Delete(tempPath, true);

                //Console.WriteLine("Press any key to exit.");
                //Console.ReadKey();
            }
            catch (Exception e)
            {
                var exc = e.GetBaseException();
                Console.WriteLine("Exception :" + exc.Message);
                Console.WriteLine(exc.StackTrace);
            }
        }

        private static void InitAreaPopulationStrats()
        {
            int regionSourceId = 0;
            switch (_regionSource)
            {
                case "CustomRegionID":
                    regionSourceId = 0;
                    break;
                case "HRRRegionID":
                    regionSourceId = 1;
                    break;
                case "HSARegionID":
                    regionSourceId = 2;
                    break;
            }

            _areaPopulationStrats = new Dictionary<string, int>();
            var areaPopulationStratsSQL = ConfigurationManager.AppSettings["AreaPopulationStrats_sql"].Replace("[RegionSourceID]", regionSourceId.ToString()).Replace("[WebsiteYear]", _year).Replace("[DatasetYear]",_datasetyear);
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();
            command.CommandText = areaPopulationStratsSQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();
            while (dataRead.Read())
            {
                if (!dataRead.IsDBNull(0))
                    _areaPopulationStrats.Add(dataRead.GetString(0), dataRead.GetInt32(1));
            }
        }

        private static void GenerateDimension(string dimention, string dimentionSQL)
        {
            _stopWatch.Start();
            ASCIIEncoding acii = new ASCIIEncoding();
            Console.WriteLine("starting " + dimention);
            string rootDir = _rootDir + "\\" + dimention;
            string tempDir = _tempPath + "\\" + dimention;

#if DEBUG
            Console.WriteLine("initialize datareader ... ");
#endif

            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = dimentionSQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();

            //var columns = new List<string>();

            //for (int i = 0; i < dataRead.FieldCount; i++)
            //{
            //    columns.Add(dataRead.GetName(i));
            //    Console.Write("{0}\t| ", dataRead.GetName(i));
            //}
            //Console.WriteLine("\n--------------------------------");

            var clinicalClinicalDetails = new Dictionary<int, object>();
            var clinicalRegionDetails = new Dictionary<int, object>();
            int dischargeQuarter;
            //Dictionary<int, object> primaryPayerData;
            //Dictionary<int, object> primaryPayerGroupData;
            //Dictionary<int, object> hospitalMetaData = new Dictionary<int, object>();

            if (_logMod == 0)
                LogRuntime();
#if DEBUG
            Console.WriteLine("memory manipulation");
#endif
            var clinical0 = GetAndInitIfNeeded(clinicalClinicalDetails, 0);
            var region0 = GetAndInitIfNeeded(clinicalRegionDetails, 0);

            dataRead.Read();
            var rowCount = dataRead.GetInt32(0);
            int processed = 0;

            const int memoryFlushPoint = 500;
            const int rowsPerMemoryCheck = 100;
            const int maxPatchSize = 1000;
            _flushCount = 0;

            dataRead.NextResult();
            while (dataRead.Read())
            {
                var regionId = dataRead.IsDBNull(0) ? -1 : dataRead.GetInt32(0);
                var clinicalId = dataRead.GetInt32(1);

                var ageid = dataRead.GetInt32(2);
                var raceid = dataRead.GetInt32(3);
                var sexid = dataRead.GetInt32(4);
                var primaryPayerid = dataRead.GetInt32(5);
                dischargeQuarter = dataRead.IsDBNull(7) ? 0 : dataRead.GetInt32(7);

                float? totalCost = null;
                try
                {
                    if (dataRead.IsDBNull(6))
                    {
                        totalCost = null;
                    }
                    else
                    {
                        if (dataRead.GetValue(6).GetType() == typeof (double))
                        {
                            totalCost = (float) (dataRead.GetDouble(6));
                        }
                        else
                        {
                            totalCost = dataRead.GetFloat(6);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                Dictionary<int, object> regionData;
                Dictionary<int, object> ageData;
                Dictionary<int, object> ageGroupData;
                Dictionary<int, object> raceData;
                Dictionary<int, object> raceGroupData;
                Dictionary<int, object> sexData;
                Dictionary<int, object> sexGroupData;

                var clinicalData = GetAndInitIfNeeded(clinicalClinicalDetails, clinicalId);
                ArrangeData(clinicalData, regionId, ageid, raceid, sexid, primaryPayerid, totalCost, dischargeQuarter, out regionData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData);
                clinical0 = GetAndInitIfNeeded(clinicalClinicalDetails, 0);
                ArrangeData(clinical0, regionId, ageid, raceid, sexid, primaryPayerid, totalCost, dischargeQuarter, out regionData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData);
                clinical0 = GetAndInitIfNeeded(clinicalClinicalDetails, -1);
                ArrangeData(clinical0, regionId, ageid, raceid, sexid, primaryPayerid, totalCost, dischargeQuarter, out regionData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData);
                
                var clinicalRegionData = GetAndInitIfNeeded(clinicalRegionDetails, regionId);
                ArrangeData(clinicalRegionData, clinicalId, ageid, raceid, sexid, primaryPayerid, totalCost, dischargeQuarter, out regionData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData);
                region0 = GetAndInitIfNeeded(clinicalRegionDetails, 0);
                ArrangeData(region0, clinicalId, ageid, raceid, sexid, primaryPayerid, totalCost, dischargeQuarter, out regionData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData);
                region0 = GetAndInitIfNeeded(clinicalRegionDetails, -1);
                ArrangeData(region0, clinicalId, ageid, raceid, sexid, primaryPayerid, totalCost, dischargeQuarter, out regionData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData);
                
                processed++;

//#if DEBUG
                //processed++;
                var progress = processed * 100 / rowCount;

                if (progress > 50)
                    progress = 50;

                if (_logMod == 0)
                    Console.Write("\r {0}/{1} [{2}%] [{3}] ", processed, rowCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2));
//#endif
                //if (_logMod == 0)
                //{
                //    var index1 = 50 - progress/2;
                //    if (index1 < 0) index1 = 0;

                //    var index2 = progress/2;
                //    if (index2 < 0) index2 = 0;

                //    Console.Write("\r {0}/{1} [{2}%] [{3}] ", processed, rowCount, progress, PROGRESS_BAR_DONE.Substring(index1) + PROGRESS_BAR_REMAINING.Substring(index2));
                //}


                //------------ MemoryCheck -------------//
                if (processed % rowsPerMemoryCheck == 0)
                {
                    if (processed % maxPatchSize == 0)
                    {
                        flushMemory(dimention, acii, tempDir, clinicalClinicalDetails, clinicalRegionDetails, clinical0, region0);
                    }
                    else
                    {

                        try
                        {
                            MemoryFailPoint memoryFailPoint = new MemoryFailPoint(memoryFlushPoint);
                            memoryFailPoint.Dispose();
                        }
                        catch (InsufficientMemoryException imex)
                        {
                            flushMemory(dimention, acii, tempDir, clinicalClinicalDetails, clinicalRegionDetails, clinical0, region0);
                        }
                    }
                }
            }

            //-------- flush ---------//
            if (_logMod == 0)
                LogRuntime();

#if DEBUG
            if (_logMod == 0)
                Console.WriteLine("\t..........flushing memory............");
#endif

            GC.Collect();
            CreatePartialFiles(tempDir, dimention, dimention, clinicalClinicalDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalClinicalDetails.Clear();
            clinical0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Region", "Region", clinicalRegionDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalRegionDetails.Clear();
            region0.Clear();
            GC.Collect();

            conn.Close();
        }

        private static void flushMemory(string dimention, ASCIIEncoding acii, string tempDir, Dictionary<int, object> clinicalClinicalDetails, Dictionary<int, object> clinicalRegionDetails, Dictionary<int, object> clinical0, Dictionary<int, object> region0)
        {
            //-------- flush ---------//
            if (_logMod == 0)
                LogRuntime();

#if DEBUG
            if (_logMod == 0)
                Console.WriteLine("\t..........flushing memory............");
#endif

            GC.Collect();
            CreatePartialFiles(tempDir, dimention, dimention, clinicalClinicalDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalClinicalDetails.Clear();
            clinical0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Region", "Region", clinicalRegionDetails, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            clinicalRegionDetails.Clear();
            region0.Clear();
            GC.Collect();

            _flushCount++;
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

        private static void CreatePartialFiles(string rootDir, string subDir1, string subDir2, Dictionary<int, object> drgDRGDetails, string progressBarRemaining, string progressBarDone, ASCIIEncoding acii)
        {
            var location = rootDir.Substring(rootDir.LastIndexOf("\\", StringComparison.InvariantCulture) + 1) + "\\" + subDir1;

            int fileCount = drgDRGDetails.Count;

            int processed = 0;
            int progress = 0;
            foreach (var drg in drgDRGDetails)
            {
                Directory.CreateDirectory(rootDir + "\\" + subDir1 + "\\" + subDir2 + "_" + drg.Key);
                using (var fs = new FileStream(
                    rootDir + "\\" + subDir1 + "\\" + subDir2 + "_" + drg.Key + string.Format(@"\PartialBuffer_{0}.pb", _flushCount),
                    FileMode.CreateNew))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(fs, drg.Value);
                }
                processed++;

                progress = processed * 100 / fileCount;
                if (_logMod == 0)
                    Console.Write("\r\t{4,-20}{0,4}/{1,-4} [{2,3}%] [{3}] \t", processed, fileCount, progress, progressBarDone.Substring(50 - (progress / 2)) + progressBarRemaining.Substring(progress / 2), location);
            }
            drgDRGDetails.Clear();
            GC.Collect();
            if (_logMod == 0)
                LogRuntime();
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
        private static void InitIPNationalTotals()
        {
            string ipNationalTotalsSQL = ConfigurationManager.AppSettings["IPNationalTotals_SQL"];
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = ipNationalTotalsSQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();

            _ipNationalTotals = new List<Dictionary<int, string>>();
            do
            {
                var nationalTotal = new Dictionary<int, string>();
                _ipNationalTotals.Add(nationalTotal);
                nationalTotal.Add(-1, "\"Discharges\":-1,\"MeanCharges\":-1,\"MeanCost\":-1,\"MeanLOS\":-1,\"MedianCharges\":-1,\"MedianCost\":-1,\"MedianLOS\":-1");
                while (dataRead.Read())
                {
                    int id = dataRead.GetInt32(0);
                    string value = string.Format("\"Discharges\":{0},\"MeanCharges\":{1},\"MeanCost\":{2},\"MeanLOS\":{3},\"MedianCharges\":{4},\"MedianCost\":{5},\"MedianLOS\":{6}",
                        dataRead.GetInt32(1),
                        dataRead.GetDouble(2),
                        dataRead.GetDouble(3),
                        dataRead.GetFloat(4),
                        dataRead.GetFloat(5),
                        dataRead.GetFloat(6),
                        dataRead.GetFloat(7));

                    nationalTotal.Add(id, value);
                }


            } while (dataRead.NextResult());
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
                    Console.WriteLine("Compiling :" + dir.Substring(dir.LastIndexOf("\\", StringComparison.InvariantCulture)) + l1.Substring(l1.LastIndexOf("\\", StringComparison.InvariantCulture)));

                    foreach (var l2 in Directory.EnumerateDirectories(l1))
                    {
                        if (!_applyOptimization)
                            CompilePartials(l2, l2.Replace(tempPath, newPath));
                        else
                            CompileOptimizedPartials(l2, l2.Replace(tempPath, newPath));
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
            int utilId = 0;
            switch (utilName)
            {
                case "DRG":
                    utilId = 0;
                    break;
                case "MDC":
                    utilId = 1;
                    break;
                case "CCS":
                    utilId = 2;
                    break;
                case "PRCCS":
                    utilId = 3;
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
                    if (!_ipNationalTotals[utilId].Keys.Contains(utilValue))
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
                        dic = (Dictionary<int, object>)formatter.Deserialize(pbfs);
                    }
                    else
                    {
                        var tempdic = (Dictionary<int, object>)formatter.Deserialize(pbfs);

                        //int quarter = 0;
                        foreach (var hosp in tempdic)
                        {
                            if (dic.Keys.Contains(hosp.Key))
                            {
                                var dichosp = (Dictionary<int, object>)dic[hosp.Key];
                                foreach (var cat in (Dictionary<int, object>)hosp.Value)
                                {
                                    if (cat.Key == QUARTERS)
                                    {
                                        //quarter = (int)cat.Value;
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

                                                AddDischarges(
                                                    diccatGroup,
                                                    (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                                AddCosts(
                                                    diccatGroup,
                                                    (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
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

            var c = dic.Count;

            int fileCount = 0;
            float? averageCost;

            fileCount = dic.Count + 2;
            ASCIIEncoding acii = new ASCIIEncoding();

            //------   --------//
            int processed = 0;
            int progress = 0;
            //Console.WriteLine(drg.Key);

            Dictionary<int, object> combinedDetails = new Dictionary<int, object>();

            Dictionary<int, object> summary = new Dictionary<int, object>();
            Directory.CreateDirectory(targetDir);
            foreach (var hosp in (Dictionary<int, object>)dic)
            {
                using (var fs = File.Create(targetDir + @"\details_" + hosp.Key + @".js"))
                {
                    string fileContent = "$.monahrq.Region={\"NationalData\":[{" + _ipNationalTotals[utilId][utilValue] + "}]," + "\"TableData\":[";
                    string catGroupData = "";
                    Dictionary<int, object> curentHospitalData = GetAndInitIfNeeded(summary, hosp.Key);

                    //int quarter = 0;
                    foreach (var cat in (Dictionary<int, object>)hosp.Value)
                    {
                        if (cat.Key == QUARTERS)
                        {
                            // quarter = (int) cat.Value;
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
                                    AddDischarges(
                                        curentHospitalData,
                                        (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                    AddCosts(curentHospitalData, (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
                                }

                                AddDischarges(
                                    combinedDetailsCatGroup,
                                    (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                AddCosts(combinedDetailsCatGroup, (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);

                                if (!string.IsNullOrEmpty(catGroupData))
                                    catGroupData += ",";

                                catGroupData += "{\"ID\":" + hosp.Key + ",\"CatID\":" + cat.Key + ",\"CatVal\":" +
                                                catGroup.Key;

                                if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])
                                    .SelectMany(d => d.Value)
                                    .Sum() > _suppression)
                                {
                                    var costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])
                                            .SelectMany(d => d.Value)
                                            .ToList();
                                    averageCost = costs.Average();
                                    var medianCosts = costs.Median(averageCost);

                                    averageCost = averageCost ?? -1;
                                    medianCosts = medianCosts ?? -1;

                                    string populationKey = string.Format("{0}-{1}-{2}", hosp.Key, cat.Key, catGroup.Key);
                                    if (
                                        folderName.Substring(
                                            0,
                                            folderName.LastIndexOf("_", StringComparison.InvariantCulture)) == "Region")
                                    {
                                        populationKey = string.Format(
                                            "{0}-{1}-{2}",
                                            folderName.Substring(
                                                folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1),
                                            cat.Key,
                                            catGroup.Key);
                                    }

                                    var discharges = ((Dictionary<int, object>)catGroup.Value)[DISCHARGES] != null
                                        ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])
                                        .SelectMany(d => d.Value)
                                        .Sum()
                                        : 0;

                                    discharges = discharges ?? 0;

                                    float? rate = -1;

                                    if (_areaPopulationStrats.Keys.Contains(populationKey))
                                        rate = ((discharges / (float)_areaPopulationStrats[populationKey]) * _scale);

                                    rate = rate ?? -1;

                                    catGroupData += ",\"Discharges\": " + discharges +
                                                    ",\"MeanCosts\" :" + averageCost +
                                                    ",\"MedianCosts\" :" + medianCosts +
                                                    ",\"RateDischarges\":" + rate;

                                    for (var q = 1; q <= 4; q++)
                                    {
                                        var costs2 = ((Dictionary<int, List<float?>>)(((Dictionary<int, object>)catGroup.Value)[COSTS]))[q]
                                                .Select(d => d)
                                                .ToList();
                                        var averageCost2 = costs2.Average();
                                        var medianCosts2 = costs2.Median(averageCost2);

                                        averageCost2 = averageCost2 ?? -1;
                                        medianCosts2 = medianCosts2 ?? -1;

                                        //string populationKey2 = string.Format("{0}-{1}-{2}", hosp.Key, cat.Key, catGroup.Key);
                                        //if (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)) == "Region")
                                        //{
                                        //    populationKey2 = string.Format("{0}-{1}-{2}",
                                        //                           folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1), cat.Key, catGroup.Key);
                                        //}

                                        var discharges2 = ((Dictionary<int, object>)catGroup.Value)[DISCHARGES] != null
                                            ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])[q]
                                            .Select(d => d)
                                            .Sum()
                                            : 0;

                                        float? rate2 = -1;

                                        if (_areaPopulationStrats.Keys.Contains(populationKey))
                                            rate2 = ((discharges2 / (float)_areaPopulationStrats[populationKey]) * _scale);

                                        rate2 = rate2 ?? -1;

                                        catGroupData += ",\"Q" + q + "_Discharges\":" + discharges2 +
                                                        ",\"Q" + q + "_MeanCosts\":" + averageCost2 +
                                                        ",\"Q" + q + "_MedianCosts\":" + medianCosts2 +
                                                        ",\"Q" + q + "_RateDischarges\":" + rate2;
                                    }
                                }
                                else
                                {
                                    catGroupData +=
                                            ",\"Discharges\":-2,\"MeanCosts\":-2,\"MedianCosts\":-2,\"RateDischarges\":-2" +
                                            ",\"Q1_Discharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MedianCosts\":-2,\"Q1_RateDischarges\":-2" +
                                            ",\"Q2_Discharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MedianCosts\":-2,\"Q2_RateDischarges\":-2" +
                                            ",\"Q3_Discharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MedianCosts\":-2,\"Q3_RateDischarges\":-2" +
                                            ",\"Q4_Discharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MedianCosts\":-2,\"Q4_RateDischarges\":-2";
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
                    if (_logMod == 0)
                        Console.Write(
                            "\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ",
                            processed,
                            fileCount,
                            progress,
                            PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2),
                            location);

                    ((Dictionary<int, object>)hosp.Value).Clear();
                }
                GC.Collect();
            }
            dic.Clear();
            GC.Collect();

            using (FileStream fsAll = File.Create(targetDir + @"\details.js"))
            {
                string fsAllString = "$.monahrq.Region={\"NationalData\":[{" + _ipNationalTotals[utilId][utilValue] + "}]," + "\"TableData\":[";
                foreach (var cat in combinedDetails)
                {
                    foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                    {
                        fsAllString += "{\"ID\":" + dicId + ",\"CatID\":" + cat.Key + ",\"CatVal\":" + catGroup.Key;

                        if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                            > _suppression)
                        {
                            var costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])
                                    .SelectMany(d => d.Value)
                                    .ToList();
                            var averageCost2 = costs.Average();
                            var medianCosts2 = costs.Median(averageCost2);

                            averageCost2 = averageCost2 ?? -1;
                            medianCosts2 = medianCosts2 ?? -1;

                            string populationKey = string.Format("{0}-{1}-{2}", cat.Key, cat.Key, catGroup.Key);
                            if (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)) == "Region")
                            {
                                populationKey = string.Format(
                                    "{0}-{1}-{2}",
                                    folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1),
                                    cat.Key,
                                    catGroup.Key);
                            }

                            var discharges2 = ((Dictionary<int, object>)catGroup.Value)[DISCHARGES] != null
                                ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                                : 0;

                            discharges2 = discharges2 ?? 0;

                            float? rate2 = -1;
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                                rate2 = ((discharges2 / (float)_areaPopulationStrats[populationKey]) * _scale) ?? -1;

                            fsAllString += ",\"Discharges\":" + discharges2 +
                                           ",\"MeanCosts\":" + averageCost2 +
                                           ",\"MedianCosts\":" + medianCosts2 +
                                           ",\"RateDischarges\":" + rate2;

                            for (var q = 1; q <= 4; q++)
                            {
                                var costs2 = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])[q]
                                        .Select(d => d)
                                        .ToList();
                                var averageCost3 = costs2.Average();
                                var medianCosts3 = costs2.Median(averageCost3);

                                averageCost3 = averageCost3 ?? -1;
                                medianCosts3 = medianCosts3 ?? -1;

                                var discharges3 = ((Dictionary<int, object>)catGroup.Value)[DISCHARGES] != null
                                    ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])[q].Select(d => d).Sum()
                                    : 0;
                                discharges3 = discharges3 ?? 0;

                                float? rate3 = -1;
                                if (_areaPopulationStrats.Keys.Contains(populationKey))
                                    rate3 = ((discharges3 / (float)_areaPopulationStrats[populationKey]) * _scale) ?? -1;

                                fsAllString += ",\"Q" + q + "_Discharges\":" + discharges3 +
                                               ",\"Q" + q + "_MeanCosts\":" + averageCost3 +
                                               ",\"Q" + q + "_MedianCosts\":" + medianCosts3 +
                                               ",\"Q" + q + "_RateDischarges\":" + rate3;
                            }
                        }
                        else
                        {
                            fsAllString += ",\"Discharges\":-2,\"MeanCosts\":-2,\"MedianCosts\":-2,\"RateDischarges\":-2" +
                                           ",\"Q1_Discharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MedianCosts\":-2,\"Q1_RateDischarges\":-2" +
                                           ",\"Q2_Discharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MedianCosts\":-2,\"Q2_RateDischarges\":-2" +
                                           ",\"Q3_Discharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MedianCosts\":-2,\"Q3_RateDischarges\":-2" +
                                           ",\"Q4_Discharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MedianCosts\":-2,\"Q4_RateDischarges\":-2";
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

            // Summary files now
            using (var fsSummary = File.Create(targetDir + @"\summary.js"))
            {
                var nationalData = "$.monahrq.Region = {\"NationalData\" : [{" + _ipNationalTotals[utilId][utilValue] + "}],";
                const string totalData = "\"TotalData\" :[{{\"Discharges\":{0},\"MeanCosts\":{1},\"MedianCosts\":{2},\"RateDischarges\":{3}," +
                                         "\"Q1_Discharges\":{4},\"Q1_MeanCosts\":{5},\"Q1_MedianCosts\":{6},\"Q1_RateDischarges\":{7}," +
                                         "\"Q2_Discharges\":{8},\"Q2_MeanCosts\":{9},\"Q2_MedianCosts\":{10},\"Q2_RateDischarges\":{11}," +
                                         "\"Q3_Discharges\":{12},\"Q3_MeanCosts\":{13},\"Q3_MedianCosts\":{14},\"Q3_RateDischarges\":{15}," +
                                         "\"Q4_Discharges\":{16},\"Q4_MeanCosts\":{17},\"Q4_MedianCosts\":{18},\"Q4_RateDischarges\":{19}}}],";
                var tableData = "\"TableData\" : [";
                //float? MeanCosts;

                int? totalDischarges = 0;
                var totalPopulation = 0;
                var totalCosts = new List<float?>();

                var totalDischargesQ1 = new List<int?>();
                var totalRatesQ1 = new List<float>();
                var totalCostsQ1 = new List<float?>();
                var totalDischargesQ2 = new List<int?>();
                var totalRatesQ2 = new List<float>();
                var totalCostsQ2 = new List<float?>();
                var totalDischargesQ3 = new List<int?>();
                var totalRatesQ3 = new List<float>();
                var totalCostsQ3 = new List<float?>();

                var totalDischargesQ4 = new List<int?>();
                var totalRatesQ4 = new List<float>();
                var totalCostsQ4 = new List<float?>();

                foreach (var hosp in summary)
                {
                    tableData += "{";

                    switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
                    {
                        case "DRG":
                        case "MDC":
                        case "CCS":
                        case "PRCCS":
                            tableData += "\"RegionID\":" + hosp.Key;
                            break;
                        default:
                            tableData += "\"ID\":" + hosp.Key;
                            break;
                    }

                    if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                        > _suppression)
                    {
                        var costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]).SelectMany(d => d.Value).ToList();
                        var averageCost2 = costs.Average();
                        var medianCosts2 = costs.Median(averageCost2);

                        averageCost2 = averageCost2 ?? -1;
                        medianCosts2 = medianCosts2 ?? -1;

                        string populationKey = string.Format("{0}-0-0", hosp.Key);
                        if (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)) == "Region")
                        {
                            populationKey = string.Format(
                                "{0}-0-0",
                                folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1));
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                            {
                                totalPopulation = _areaPopulationStrats[populationKey];
                            }
                        }
                        else
                        {
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                            {
                                totalPopulation += _areaPopulationStrats[populationKey];
                            }
                        }

                        var discharges2 = ((Dictionary<int, object>)hosp.Value)[DISCHARGES] != null
                            ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                            : -1;

                        discharges2 = discharges2 ?? -1;

                        float? rate2 = -1;
                        if (_areaPopulationStrats.Keys.Contains(populationKey))
                            rate2 = ((discharges2 / (float)_areaPopulationStrats[populationKey]) * _scale) ?? -1;

                        tableData += ",\"Discharges\":" + discharges2 +
                                     ",\"MeanCosts\":" + averageCost2 +
                                     ",\"MedianCosts\":" + medianCosts2 +
                                     ",\"RateDischarges\":" + rate2;

                        for (var q = 1; q <= 4; q++)
                        {
                            var costs3 = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].Select(d => d).ToList();
                            var averageCost3 = costs3.Average();
                            var medianCosts3 = costs3.Median(averageCost2);

                            averageCost3 = averageCost3 ?? -1;
                            medianCosts3 = medianCosts3 ?? -1;

                            var discharges3 = ((Dictionary<int, object>)hosp.Value)[DISCHARGES] != null
                                ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].Select(d => d).Sum()
                                : -1;

                            discharges3 = discharges3 ?? -1;

                            float? rate3 = -1;
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                                rate3 = (discharges3 / (float)(_areaPopulationStrats[populationKey] / 4M) * _scale) ?? -1;

                            if (rate3 == 0) rate3 = -1;

                            tableData += ",\"Q" + q + "_Discharges\":" + discharges3 +
                                         ",\"Q" + q + "_MeanCosts\":" + averageCost3 +
                                         ",\"Q" + q + "_MedianCosts\":" + medianCosts3 +
                                         ",\"Q" + q + "_RateDischarges\":" + rate3;

                            if (q == 1)
                            {
                                totalDischargesQ1.AddRange(
                                    ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ1.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ1.Add(rate3.Value);
                            }

                            if (q == 2)
                            {
                                totalDischargesQ2.AddRange(
                                    ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ2.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ2.Add(rate3.Value);
                            }

                            if (q == 3)
                            {
                                totalDischargesQ3.AddRange(
                                    ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ3.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ3.Add(rate3.Value);
                            }

                            if (q == 4)
                            {
                                totalDischargesQ4.AddRange(
                                    ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ4.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ4.Add(rate3.Value);
                            }
                        }
                    }
                    else
                    {
                        tableData += ",\"Discharges\":-2,\"MeanCosts\":-2,\"MedianCosts\":-2,\"RateDischarges\":-2" +
                                     ",\"Q1_Discharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MedianCosts\":-2,\"Q1_RateDischarges\":-2" +
                                     ",\"Q2_Discharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MedianCosts\":-2,\"Q2_RateDischarges\":-2" +
                                     ",\"Q3_Discharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MedianCosts\":-2,\"Q3_RateDischarges\":-2" +
                                     ",\"Q4_Discharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MedianCosts\":-2,\"Q4_RateDischarges\":-2";

                    }
                    tableData += "},";

                    totalDischarges += ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])
                            .SelectMany(d => d.Value)
                            .Sum(); //(int)((Dictionary<int, object>)hosp.Value)[DISCHARGES];
                    totalCosts.AddRange(
                        ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]).SelectMany(d => d.Value).ToList());
                }
                tableData += "]};";

                float? totalRate = -1;
                if (totalPopulation > 0)
                    totalRate = (totalDischarges / (float)totalPopulation * _scale) ?? -1;

                var totalMeanCosts = totalCosts.Count > 0 ? totalCosts.Average() : -1;
                var totalMedianCosts = totalCosts.Count > 0 ? totalCosts.Median(totalCosts.Average()) : -1;

                totalMeanCosts = totalMeanCosts ?? -1;
                totalMedianCosts = totalMedianCosts ?? -1;

                byte[] summaryResult = acii.GetBytes(
                    nationalData +
                    string.Format(
                        totalData,
                        totalDischarges,
                        totalMeanCosts,
                        totalMedianCosts,
                        totalRate,

                        totalDischargesQ1.Select(d => d ?? 1).Sum(),
                        totalCostsQ1.Count > 0 ? totalCostsQ1.Select(d => d).Average() ?? -1 : -1,
                        totalCostsQ1.Count > 0 ? totalCostsQ1.Select(d => d).ToList().Median(totalCostsQ1.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ1.Any() ? totalRatesQ1.Average() : -1,

                        totalDischargesQ2.Select(d => d ?? 1).Sum(),
                        totalCostsQ2.Count > 0 ? totalCostsQ2.Select(d => d).Average() ?? -1 : -1,
                        totalCostsQ2.Count > 0 ? totalCostsQ2.Select(d => d).ToList().Median(totalCostsQ2.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ2.Any() ? totalRatesQ2.Average() : -1,

                        totalDischargesQ3.Select(d => d ?? 1).Sum(),
                        totalCostsQ3.Count > 0 ? totalCostsQ3.Select(d => d).Average() ?? -1 : -1,
                        totalCostsQ3.Count > 0 ? totalCostsQ3.Select(d => d).ToList().Median(totalCostsQ3.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ3.Any() ? totalRatesQ3.Average() : -1,

                        totalDischargesQ4.Select(d => d ?? 1).Sum(),
                        totalCostsQ4.Count > 0 ? totalCostsQ4.Select(d => d).Average() ?? -1 : -1,
                        totalCostsQ4.Count > 0 ? totalCostsQ4.Select(d => d).ToList().Median(totalCostsQ4.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ4.Any() ? totalRatesQ4.Average() : -1) +
                    tableData);

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

        private static void CompileOptimizedPartials(string sourceDir, string targetDir)
        {
            var locationparts = sourceDir.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            var location = locationparts[locationparts.Length - 2] + "\\" + locationparts[locationparts.Length - 1];
            var folderName = locationparts[locationparts.Length - 1];
            var utilName = locationparts[locationparts.Length - 3];
            int utilId = 0;
            switch (utilName)
            {
                case "DRG":
                    utilId = 0;
                    break;
                case "MDC":
                    utilId = 1;
                    break;
                case "CCS":
                    utilId = 2;
                    break;
                case "PRCCS":
                    utilId = 3;
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
                    if (!_ipNationalTotals[utilId].Keys.Contains(utilValue))
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
                        dic = (Dictionary<int, object>)formatter.Deserialize(pbfs);
                    }
                    else
                    {
                        var tempdic = (Dictionary<int, object>)formatter.Deserialize(pbfs);

                        int quarter = 0;
                        foreach (var hosp in tempdic)
                        {
                            if (dic.Keys.Contains(hosp.Key))
                            {
                                var dichosp = (Dictionary<int, object>)dic[hosp.Key];
                                foreach (var cat in (Dictionary<int, object>)hosp.Value)
                                {
                                    if (cat.Key == QUARTERS)
                                    {
                                        quarter = (int)cat.Value;
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

                                                AddDischarges(
                                                    diccatGroup,
                                                    (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                                AddCosts(
                                                    diccatGroup,
                                                    (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
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

            GC.Collect(0, GCCollectionMode.Forced, false);

            int fileCount = 0;
            float? averageCost;

            fileCount = dic.Count + 2;
            ASCIIEncoding acii = new ASCIIEncoding();

            //------   --------//
            int processed = 0;
            int progress = 0;
            //Console.WriteLine(drg.Key);

            Dictionary<int, object> combinedDetails = new Dictionary<int, object>();

            Dictionary<int, object> summary = new Dictionary<int, object>();
            Directory.CreateDirectory(targetDir);

            var levelIdGroupData = new StringBuilder();
            var idGroupData = new StringBuilder();
            var catIdGroupData = new StringBuilder();
            var catValueGroupData = new StringBuilder();
            var dischargeGroupData = new StringBuilder();
            var meanCostsGroupData = new StringBuilder();
            var medianCostsGroupData = new StringBuilder();
            var rateDischargesGroupData = new StringBuilder();

            // Quarterly values
            var q1DischargeGroupData = new StringBuilder();
            var q1MeanCostsGroupData = new StringBuilder();
            var q1MedianCostsGroupData = new StringBuilder();
            var q1RateDischargesGroupData = new StringBuilder();

            var q2DischargeGroupData = new StringBuilder();
            var q2MeanCostsGroupData = new StringBuilder();
            var q2MedianCostsGroupData = new StringBuilder();
            var q2RateDischargesGroupData = new StringBuilder();

            var q3DischargeGroupData = new StringBuilder();
            var q3MeanCostsGroupData = new StringBuilder();
            var q3MedianCostsGroupData = new StringBuilder();
            var q3RateDischargesGroupData = new StringBuilder();

            var q4DischargeGroupData = new StringBuilder();
            var q4MeanCostsGroupData = new StringBuilder();
            var q4MedianCostsGroupData = new StringBuilder();
            var q4RateDischargesGroupData = new StringBuilder();

            using (var fs = File.Create(targetDir + @"\details.js"))
            {
                var fileContent = "$.monahrq.Region={\"NationalData\":[{" + _ipNationalTotals[utilId][utilValue] + "}]," + "\"TableData\":[{";
                //var catGroupData = "";

                foreach (var hosp in dic)
                {
                    var curentHospitalData = GetAndInitIfNeeded(summary, hosp.Key);

                    levelIdGroupData.Append(hosp.Key + ",");

                    idGroupData.Append("[");
                    catIdGroupData.Append("[");
                    catValueGroupData.Append("[");
                    dischargeGroupData.Append("[");
                    meanCostsGroupData.Append("[");
                    medianCostsGroupData.Append("[");
                    rateDischargesGroupData.Append("[");

                    // Quarterly values
                    q1DischargeGroupData.Append("[");
                    q1MeanCostsGroupData.Append("[");
                    q1MedianCostsGroupData.Append("[");
                    q1RateDischargesGroupData.Append("[");

                    q2DischargeGroupData.Append("[");
                    q2MeanCostsGroupData.Append("[");
                    q2MedianCostsGroupData.Append("[");
                    q2RateDischargesGroupData.Append("[");

                    q3DischargeGroupData.Append("[");
                    q3MeanCostsGroupData.Append("[");
                    q3MedianCostsGroupData.Append("[");
                    q3RateDischargesGroupData.Append("[");

                    q4DischargeGroupData.Append("[");
                    q4MeanCostsGroupData.Append("[");
                    q4MedianCostsGroupData.Append("[");
                    q4RateDischargesGroupData.Append("[");

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
                                    AddDischarges(curentHospitalData,(Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                    AddCosts(curentHospitalData, (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
                                }

                                AddDischarges(combinedDetailsCatGroup, (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                AddCosts(combinedDetailsCatGroup, (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);

                                if (((Dictionary<int, List<int?>>) ((Dictionary<int, object>) catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum() > _suppression)
                                {
                                    var costs = ((Dictionary<int, List<float?>>) ((Dictionary<int, object>) catGroup.Value)[COSTS]).SelectMany(d => d.Value).ToList();
                                    averageCost = costs.Average();
                                    var medianCosts = costs.Median(averageCost);

                                    averageCost = averageCost ?? -1;
                                    medianCosts = medianCosts ?? -1;

                                    string populationKey = string.Format("{0}-{1}-{2}", hosp.Key, cat.Key, catGroup.Key);
                                    if (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)) == "Region")
                                    {
                                        populationKey = string.Format("{0}-{1}-{2}", folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1), cat.Key, catGroup.Key);
                                    }

                                    var discharges = ((Dictionary<int, object>) catGroup.Value)[DISCHARGES] != null
                                        ? ((Dictionary<int, List<int?>>) ((Dictionary<int, object>) catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                                        : 0;

                                    discharges = discharges ?? 0;

                                    float? rate = -1;

                                    if (_areaPopulationStrats.Keys.Contains(populationKey))
                                        rate = ((discharges/(float) _areaPopulationStrats[populationKey])*_scale);

                                    rate = rate ?? -1;

                                    dischargeGroupData.Append(discharges + ",");
                                    meanCostsGroupData.Append(averageCost + ",");
                                    medianCostsGroupData.Append(medianCosts + ",");
                                    rateDischargesGroupData.Append(rate + ",");


                                    for (var q = 1; q <= 4; q++)
                                    {
                                            var costs2 = ((Dictionary<int, List<float?>>) ((Dictionary<int, object>) catGroup.Value)[COSTS])[q].Select(d => d).ToList();
                                            var averageCost2 = costs2.Average();
                                            var medianCosts2 = costs2.Median(averageCost2);

                                            averageCost2 = averageCost2 ?? -1;
                                            medianCosts2 = medianCosts2 ?? -1;

                                            var discharges2 = ((Dictionary<int, object>) catGroup.Value)[DISCHARGES] != null
                                                                                        ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>) catGroup.Value)[DISCHARGES])[q].Select(d => d).Sum()
                                                                                        : 0;

                                            float? rate2 = -1;

                                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                                                rate2 = ((discharges2/(float) _areaPopulationStrats[populationKey])*_scale);

                                            rate2 = rate2 ?? -1;

                                            if (q == 1)
                                            {
                                                q1DischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0
                                                    ? discharges2.Value.ToString()
                                                    : "null") + ",");
                                                q1MeanCostsGroupData.Append(averageCost2 + ",");
                                                q1MedianCostsGroupData.Append(medianCosts2 + ",");
                                                q1RateDischargesGroupData.Append((rate2.HasValue ? rate2.ToString() : "null") +
                                                                                 ",");
                                            }

                                            if (q == 2)
                                            {
                                                q2DischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0
                                                    ? discharges2.Value.ToString()
                                                    : "null") + ",");
                                                q2MeanCostsGroupData.Append(averageCost2 + ",");
                                                q2MedianCostsGroupData.Append(medianCosts2 + ",");
                                                q2RateDischargesGroupData.Append((rate2.HasValue ? rate2.ToString() : "null") +
                                                                                 ",");
                                            }
                                            if (q == 3)
                                            {
                                                q3DischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0
                                                    ? discharges2.Value.ToString()
                                                    : "null") + ",");
                                                q3MeanCostsGroupData.Append(averageCost2 + ",");
                                                q3MedianCostsGroupData.Append(medianCosts2 + ",");
                                                q3RateDischargesGroupData.Append((rate2.HasValue ? rate2.ToString() : "null") +
                                                                                 ",");
                                            }
                                            if (q == 4)
                                            {
                                                q4DischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0
                                                    ? discharges2.Value.ToString()
                                                    : "null") + ",");
                                                q4MeanCostsGroupData.Append(averageCost2 + ",");
                                                q4MedianCostsGroupData.Append(medianCosts2 + ",");
                                                q4RateDischargesGroupData.Append((rate2.HasValue ? rate2.ToString() : "null") +
                                                                                 ",");
                                            }
                                    }
                                }
                                else
                                {
                                    dischargeGroupData.Append("-2,");
                                    meanCostsGroupData.Append("-2,");
                                    medianCostsGroupData.Append("-2,");
                                    rateDischargesGroupData.Append("-2,");

                                    q1DischargeGroupData.Append("-2,");
                                    q1MeanCostsGroupData.Append("-2,");
                                    q1MedianCostsGroupData.Append("-2,");
                                    q1RateDischargesGroupData.Append("-2,");

                                    q2DischargeGroupData.Append("-2,");
                                    q2MeanCostsGroupData.Append("-2,");
                                    q2MedianCostsGroupData.Append("-2,");
                                    q2RateDischargesGroupData.Append("-2,");

                                    q3DischargeGroupData.Append("-2,");
                                    q3MeanCostsGroupData.Append("-2,");
                                    q3MedianCostsGroupData.Append("-2,");
                                    q3RateDischargesGroupData.Append("-2,");

                                    q4DischargeGroupData.Append("-2,");
                                    q4MeanCostsGroupData.Append("-2,");
                                    q4MedianCostsGroupData.Append("-2,");
                                    q4RateDischargesGroupData.Append("-2,");
                                }
                                //catGroupData += "}";
                            }

                            GC.Collect();
                        }
                    }

                    idGroupData = new StringBuilder(idGroupData.ToString().SubStrBeforeLast(",") + "],");
                    catIdGroupData = new StringBuilder(catIdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    catValueGroupData = new StringBuilder(catValueGroupData.ToString().SubStrBeforeLast(",") + "],");
                    dischargeGroupData = new StringBuilder(dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    meanCostsGroupData = new StringBuilder(meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    medianCostsGroupData = new StringBuilder(medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    rateDischargesGroupData = new StringBuilder(rateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q1DischargeGroupData = new StringBuilder(q1DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1MeanCostsGroupData = new StringBuilder(q1MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1MedianCostsGroupData = new StringBuilder(q1MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1RateDischargesGroupData = new StringBuilder(q1RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q2DischargeGroupData = new StringBuilder(q2DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2MeanCostsGroupData = new StringBuilder(q2MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2MedianCostsGroupData = new StringBuilder(q2MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2RateDischargesGroupData = new StringBuilder(q2RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q3DischargeGroupData = new StringBuilder(q3DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3MeanCostsGroupData = new StringBuilder(q3MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3MedianCostsGroupData = new StringBuilder(q3MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3RateDischargesGroupData = new StringBuilder(q3RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");


                    q4DischargeGroupData = new StringBuilder(q4DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4MeanCostsGroupData = new StringBuilder(q4MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4MedianCostsGroupData = new StringBuilder(q4MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4RateDischargesGroupData = new StringBuilder(q4RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                    ((Dictionary<int, object>)hosp.Value).Clear();

                    GC.Collect();
                }

                #region Codes for LevelID=0 (Summary)
                idGroupData.Append("[");
                catIdGroupData.Append("[");
                catValueGroupData.Append("[");
                dischargeGroupData.Append("[");
                meanCostsGroupData.Append("[");
                medianCostsGroupData.Append("[");
                rateDischargesGroupData.Append("[");

                // Quarterly values
                q1DischargeGroupData.Append("[");
                q1MeanCostsGroupData.Append("[");
                q1MedianCostsGroupData.Append("[");
                q1RateDischargesGroupData.Append("[");

                q2DischargeGroupData.Append("[");
                q2MeanCostsGroupData.Append("[");
                q2MedianCostsGroupData.Append("[");
                q2RateDischargesGroupData.Append("[");

                q3DischargeGroupData.Append("[");
                q3MeanCostsGroupData.Append("[");
                q3MedianCostsGroupData.Append("[");
                q3RateDischargesGroupData.Append("[");

                q4DischargeGroupData.Append("[");
                q4MeanCostsGroupData.Append("[");
                q4MedianCostsGroupData.Append("[");
                q4RateDischargesGroupData.Append("[");

                foreach (var cat in combinedDetails)
                {
                    foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                    {
                        idGroupData.Append(targetDir.SubStrAfterLast("_").Replace("\\", null) + ",");
                        catIdGroupData.Append(cat.Key + ",");
                        catValueGroupData.Append(catGroup.Key + ",");

                        if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                            > _suppression)
                        {
                            var costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])
                                    .SelectMany(d => d.Value)
                                    .ToList();
                            var averageCost2 = costs.Average();
                            var medianCosts2 = costs.Median(averageCost2);

                            averageCost2 = averageCost2 ?? -1;
                            medianCosts2 = medianCosts2 ?? -1;

                            string populationKey = string.Format("{0}-{1}-{2}", cat.Key, cat.Key, catGroup.Key);
                            if (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)) == "Region")
                            {
                                populationKey = string.Format(
                                    "{0}-{1}-{2}",
                                    folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1),
                                    cat.Key,
                                    catGroup.Key);
                            }

                            var discharges2 = ((Dictionary<int, object>)catGroup.Value)[DISCHARGES] != null
                                ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                                : 0;

                            discharges2 = discharges2 ?? 0;

                            float? rate2 = -1;
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                                rate2 = ((discharges2 / (float)_areaPopulationStrats[populationKey]) * _scale) ?? -1;

                            dischargeGroupData.Append(discharges2 + ",");
                            meanCostsGroupData.Append(averageCost2 + ",");
                            medianCostsGroupData.Append(medianCosts2 + ",");
                            rateDischargesGroupData.Append(rate2 + ",");

                            for (var q = 1; q <= 4; q++)
                            {
                                var costs2 = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])[q]
                                        .Select(d => d)
                                        .ToList();
                                var averageCost3 = costs2.Average();
                                var medianCosts3 = costs2.Median(averageCost3);

                                averageCost3 = averageCost3 ?? -1;
                                medianCosts3 = medianCosts3 ?? -1;

                                var discharges3 = ((Dictionary<int, object>)catGroup.Value)[DISCHARGES] != null
                                    ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])[q].Select(d => d).Sum()
                                    : 0;
                                discharges3 = discharges3 ?? 0;

                                float? rate3 = -1;
                                if (_areaPopulationStrats.Keys.Contains(populationKey))
                                    rate3 = ((discharges3 / (float)_areaPopulationStrats[populationKey]) * _scale) ?? -1;

                                switch (q)
                                {
                                    case 1:
                                        q1DischargeGroupData.Append((discharges3.HasValue && discharges3.Value > 0
                                                    ? discharges3.Value.ToString()
                                                    : "null") + ",");
                                        q1MeanCostsGroupData.Append(averageCost3 + ",");
                                        q1MedianCostsGroupData.Append(medianCosts3 + ",");
                                        q1RateDischargesGroupData.Append((rate3.HasValue ? rate3.ToString() : "null") +
                                                                                 ",");
                                        break;
                                    case 2:
                                        q2DischargeGroupData.Append((discharges3.HasValue && discharges3.Value > 0
                                                    ? discharges3.Value.ToString()
                                                    : "null") + ",");
                                        q2MeanCostsGroupData.Append(averageCost3 + ",");
                                        q2MedianCostsGroupData.Append(medianCosts3 + ",");
                                        q2RateDischargesGroupData.Append((rate3.HasValue ? rate3.ToString() : "null") +
                                                                                 ",");
                                        break;
                                    case 3:
                                        q3DischargeGroupData.Append((discharges3.HasValue && discharges3.Value > 0
                                                    ? discharges3.Value.ToString()
                                                    : "null") + ",");
                                        q3MeanCostsGroupData.Append(averageCost3 + ",");
                                        q3MedianCostsGroupData.Append(medianCosts3 + ",");
                                        q3RateDischargesGroupData.Append((rate3.HasValue ? rate3.ToString() : "null") +
                                                                                 ",");
                                        break;
                                    case 4:
                                        q4DischargeGroupData.Append((discharges3.HasValue && discharges3.Value > 0
                                                    ? discharges3.Value.ToString()
                                                    : "null") + ",");
                                        q4MeanCostsGroupData.Append(averageCost3 + ",");
                                        q4MedianCostsGroupData.Append(medianCosts3 + ",");
                                        q4RateDischargesGroupData.Append((rate3.HasValue ? rate3.ToString() : "null") +
                                                                                 ",");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            dischargeGroupData.Append("-2,");
                            meanCostsGroupData.Append("-2,");
                            medianCostsGroupData.Append("-2,");
                            rateDischargesGroupData.Append("-2,");

                            // Quarterly values
                            q1DischargeGroupData.Append("-2,");
                            q1MeanCostsGroupData.Append("-2,");
                            q1MedianCostsGroupData.Append("-2,");
                            q1RateDischargesGroupData.Append("-2,");

                            q2DischargeGroupData.Append("-2,");
                            q2MeanCostsGroupData.Append("-2,");
                            q2MedianCostsGroupData.Append("-2,");
                            q2RateDischargesGroupData.Append("-2,");

                            q3DischargeGroupData.Append("-2,");
                            q3MeanCostsGroupData.Append("-2,");
                            q3MedianCostsGroupData.Append("-2,");
                            q3RateDischargesGroupData.Append("-2,");

                            q4DischargeGroupData.Append("-2,");
                            q4MeanCostsGroupData.Append("-2,");
                            q4MedianCostsGroupData.Append("-2,");
                            q4RateDischargesGroupData.Append("-2,");
                        }
                    }
                }

                idGroupData = new StringBuilder(idGroupData.ToString().SubStrBeforeLast(",") + "],");
                catIdGroupData = new StringBuilder(catIdGroupData.ToString().SubStrBeforeLast(",") + "],");
                catValueGroupData = new StringBuilder(catValueGroupData.ToString().SubStrBeforeLast(",") + "],");
                dischargeGroupData = new StringBuilder(dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                meanCostsGroupData = new StringBuilder(meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                medianCostsGroupData = new StringBuilder(medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                rateDischargesGroupData = new StringBuilder(rateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                q1DischargeGroupData = new StringBuilder(q1DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1MeanCostsGroupData = new StringBuilder(q1MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1MedianCostsGroupData = new StringBuilder(q1MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1RateDischargesGroupData = new StringBuilder(q1RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                q2DischargeGroupData = new StringBuilder(q2DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2MeanCostsGroupData = new StringBuilder(q2MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2MedianCostsGroupData = new StringBuilder(q2MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2RateDischargesGroupData = new StringBuilder(q2RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                q3DischargeGroupData = new StringBuilder(q3DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3MeanCostsGroupData = new StringBuilder(q3MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3MedianCostsGroupData = new StringBuilder(q3MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3RateDischargesGroupData = new StringBuilder(q3RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");


                q4DischargeGroupData = new StringBuilder(q4DischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4MeanCostsGroupData = new StringBuilder(q4MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4MedianCostsGroupData = new StringBuilder(q4MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4RateDischargesGroupData = new StringBuilder(q4RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "],");

                combinedDetails.Clear();
                GC.Collect();
                #endregion

                //fileContent += catGroupData + "]};";
                fileContent += "\"LevelID\":[" + levelIdGroupData.ToString().SubStrBeforeLast(",") + ", 0]," +
                               "\"ID\":[" + idGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"CatID\":[" + catIdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"CatVal\":[" + catValueGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Discharges\":[" + dischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MeanCosts\":[" + meanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MedianCosts\":[" + medianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"RateDischarges\":[" + rateDischargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               // Q1
                               "\"Q1_Discharges\":[" + q1DischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MeanCosts\":[" + q1MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MedianCosts\":[" + q1MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_RateDischarges\":[" + q1RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               // Q2
                               "\"Q2_Discharges\":[" + q2DischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MeanCosts\":[" + q2MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MedianCosts\":[" + q2MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_RateDischarges\":[" + q2RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               // Q3
                               "\"Q3_Discharges\":[" + q3DischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MeanCosts\":[" + q3MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MedianCosts\":[" + q3MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_RateDischarges\":[" + q3RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               // Q4
                               "\"Q4_Discharges\":[" + q4DischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MeanCosts\":[" + q4MeanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MedianCosts\":[" + q4MedianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_RateDischarges\":[" + q4RateDischargesGroupData.ToString().SubStrBeforeLast(",") + "]" + 
                               "}]};";

                //fileContent += catGroupData + "]};";
                byte[] result = acii.GetBytes(fileContent);
                fs.Write(result, 0, result.Length);
                processed++;
                progress = processed * 100 / fileCount;
                if (_logMod == 0)
                    Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            }
            dic.Clear();
            GC.Collect();

            processed++;

            progress = processed * 100 / fileCount;
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            // Summary files now
            using (var fsSummary = File.Create(targetDir + @"\summary.js"))
            {
                var nationalData = "$.monahrq.Region = {\"NationalData\" : [{" + _ipNationalTotals[utilId][utilValue] + "}],";
                const string totalData = "\"TotalData\" :[{{\"Discharges\":{0},\"MeanCosts\":{1},\"MedianCosts\":{2},\"RateDischarges\":{3}," +
                                         "\"Q1_Discharges\":{4},\"Q1_MeanCosts\":{5},\"Q1_MedianCosts\":{6},\"Q1_RateDischarges\":{7}," +
                                         "\"Q2_Discharges\":{8},\"Q2_MeanCosts\":{9},\"Q2_MedianCosts\":{10},\"Q2_RateDischarges\":{11}," +
                                         "\"Q3_Discharges\":{12},\"Q3_MeanCosts\":{13},\"Q3_MedianCosts\":{14},\"Q3_RateDischarges\":{15}," +
                                         "\"Q4_Discharges\":{16},\"Q4_MeanCosts\":{17},\"Q4_MedianCosts\":{18},\"Q4_RateDischarges\":{19}}}],";
                var tableData = "\"TableData\" : [";
                //float? MeanCosts;

                var totalDischarges = new List<int?>();
                var totalPopulation = 0;
                var totalCosts = new List<float?>();
                var totalDischargesQ1 = new List<int?>();
                var totalRatesQ1 = new List<float>();
                var totalCostsQ1 = new List<float?>();
                var totalDischargesQ2 = new List<int?>();
                var totalRatesQ2 = new List<float>();
                var totalCostsQ2 = new List<float?>();
                var totalDischargesQ3 = new List<int?>();
                var totalRatesQ3 = new List<float>();
                var totalCostsQ3 = new List<float?>();

                var totalDischargesQ4 = new List<int?>();
                var totalRatesQ4 = new List<float>();
                var totalCostsQ4 = new List<float?>();

                foreach (var hosp in summary)
                {
                    tableData += "{";

                    switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
                    {
                        case "DRG":
                        case "MDC":
                        case "CCS":
                        case "PRCCS":
                            tableData +=
                                "\"RegionID\":" + hosp.Key;
                            break;
                        default:
                            tableData += "\"ID\":" + hosp.Key;
                            break;
                    }

                    if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum() > _suppression)
                    {
                        var costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]).SelectMany(d => d.Value).ToList();
                        var averageCost2 = costs.Average();
                        var medianCosts2 = costs.Median(averageCost2);

                        averageCost2 = averageCost2 ?? -1;
                        medianCosts2 = medianCosts2 ?? -1;

                        string populationKey = string.Format("{0}-0-0", hosp.Key);
                        if (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)) == "Region")
                        {
                            populationKey = string.Format("{0}-0-0", folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1));
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                            {
                                totalPopulation = _areaPopulationStrats[populationKey];
                            }
                        }
                        else
                        {
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                            {
                                totalPopulation += _areaPopulationStrats[populationKey];
                            }
                        }

                        var discharges2 = ((Dictionary<int, object>)hosp.Value)[DISCHARGES] != null
                            ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                            : -1;

                        discharges2 = discharges2 ?? -1;

                        float? rate2 = -1;
                        if (_areaPopulationStrats.Keys.Contains(populationKey))
                            rate2 = ((discharges2 / (float)_areaPopulationStrats[populationKey]) * _scale) ?? -1;

                        tableData += ",\"Discharges\":" + discharges2 +
                                     ",\"MeanCosts\":" + averageCost2 +
                                     ",\"MedianCosts\":" + medianCosts2 +
                                     ",\"RateDischarges\":" + rate2;

                        for (var q = 1; q <= 4; q++)
                        {
                            //if (!q.In(_reportQuarters.ToList())) continue;

                            var costs3 = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].Select(d => d).ToList();
                            var averageCost3 = costs3.Average();
                            var medianCosts3 = costs3.Median(averageCost3);

                            averageCost3 = averageCost3 ?? -1;
                            medianCosts3 = medianCosts3 ?? -1;

                            var discharges3 = ((Dictionary<int, object>)hosp.Value)[DISCHARGES] != null
                                ? ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].Select(d => d).Sum()
                                : -1;

                            discharges3 = discharges3 ?? -1;

                            float? rate3 = -1;
                            if (_areaPopulationStrats.Keys.Contains(populationKey))
                                rate3 = (discharges3 / (float)(_areaPopulationStrats[populationKey] / 4M) * _scale) ?? -1;

                            if (rate3 == 0) rate3 = -1;

                            tableData += ",\"Q" + q + "_Discharges\":" + discharges3 +
                                         ",\"Q" + q + "_MeanCosts\":" + averageCost3 +
                                         ",\"Q" + q + "_MedianCosts\":" + medianCosts3 +
                                         ",\"Q" + q + "_RateDischarges\":" + rate3;

                            if (q == 1)
                            {
                                totalDischargesQ1.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ1.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ1.Add(rate3.Value);
                            }

                            if (q == 2)
                            {
                                totalDischargesQ2.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ2.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ2.Add(rate3.Value);
                            }

                            if (q == 3)
                            {
                                totalDischargesQ3.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ3.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ3.Add(rate3.Value);
                            }

                            if (q == 4)
                            {
                                totalDischargesQ4.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].ToList());
                                totalCostsQ4.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].ToList());
                                if (_areaPopulationStrats.Keys.Contains(populationKey) && (rate3.HasValue && rate3.Value > 0))
                                    totalRatesQ4.Add(rate3.Value);
                            }
                        }
                    }
                    else
                    {
                        tableData += ",\"Discharges\":-2,\"MeanCosts\":-2,\"MedianCosts\":-2,\"RateDischarges\":-2" +
                                     ",\"Q1_Discharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MedianCosts\":-2,\"Q1_RateDischarges\":-2" +
                                     ",\"Q2_Discharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MedianCosts\":-2,\"Q2_RateDischarges\":-2" +
                                     ",\"Q3_Discharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MedianCosts\":-2,\"Q3_RateDischarges\":-2" +
                                     ",\"Q4_Discharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MedianCosts\":-2,\"Q4_RateDischarges\":-2";

                    }
                    tableData += "},";

                    totalDischarges.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value.ToList())); //(int)((Dictionary<int, object>)hosp.Value)[DISCHARGES];
                    totalCosts.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]).SelectMany(d => d.Value).ToList());
                }
                tableData += "]};";

                float? totalRate = -1;
                if (totalPopulation > 0)
                    totalRate = (totalDischarges.Select(d => d).Sum() / (float)totalPopulation * _scale) ?? -1;

                var totalMeanCosts = totalCosts.Count > 0 ? totalCosts.Select(d => d).ToList().Average() : -1;
                var totalMedianCosts = totalCosts.Count > 0 ? totalCosts.Select(d => d).ToList().Median(totalCosts.Select(d => d).ToList().Average()) : -1;

                totalMeanCosts = totalMeanCosts ?? -1;
                totalMedianCosts = totalMedianCosts ?? -1;

                //int?   totalDischargesQ1 = totalDischarges.Where(d => d.Quarter ==1).Select(d => d.Value).Sum() ?? -1;
                //var totalCostsQ1 = totalCosts.Where(d => d.Quarter == 1).Select(d => d.Value).ToList();
                //float? totalMEanCostsQ1 = totalCostsQ1.Count > 0 ? totalCostsQ1.Average() ?? -1 : -1;
                //float? totalMedianCostsQ1 = totalCostsQ1.Count > 0 ? totalCostsQ1.Median(totalCostsQ1.Average()) ?? -1 : -1;
                //float? totalRateQ1 = -1;




                byte[] summaryResult = acii.GetBytes(nationalData + 
                    string.Format(totalData,
                        totalDischarges.Select(d => d.Value).Sum(), 
                        totalMeanCosts, 
                        totalMedianCosts, 
                        totalRate,

                        totalDischargesQ1.Select(d => d.Value).Sum(),
                        totalCostsQ1.Count > 0 ? totalCostsQ1.Select(d => d).ToList().Average() ?? -1 : -1,
                        totalCostsQ1.Count > 0 ? totalCostsQ1.Select(d => d).ToList().Median(totalCostsQ1.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ1.Any() ? totalRatesQ1.Average() : -1,

                        totalDischargesQ2.Select(d => d.Value).Sum(),
                        totalCostsQ2.Count > 0 ? totalCostsQ2.Select(d => d).ToList().Average() ?? -1 : -1,
                        totalCostsQ2.Count > 0 ? totalCostsQ2.Select(d => d).ToList().Median(totalCostsQ2.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ2.Any() ? totalRatesQ2.Average() : -1,

                        totalDischargesQ3.Select(d => d.Value).Sum(),
                        totalCostsQ3.Count > 0 ? totalCostsQ3.Select(d => d).ToList().Average() ?? -1 : -1,
                        totalCostsQ3.Count > 0 ? totalCostsQ3.Select(d => d).ToList().Median(totalCostsQ3.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ3.Any() ? totalRatesQ3.Average() : -1,

                        totalDischargesQ4.Select(d => d.Value).Sum(),
                        totalCostsQ4.Count > 0 ? totalCostsQ4.Select(d => d).ToList().Average() ?? -1 : -1,
                        totalCostsQ4.Count > 0 ? totalCostsQ4.Select(d => d).ToList().Median(totalCostsQ4.Select(d => d).Average()) ?? -1 : -1,
                        totalRatesQ4.Any() ? totalRatesQ4.Average() : -1) +
                        tableData);

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

        private static void ArrangeData(Dictionary<int, object> drgData, int hospitalid, int ageid, int raceid, int sexid, int primaryPayerid, float? totalCost, int dischargeQuarter,  
                                        out Dictionary<int, object> hospitalData, out Dictionary<int, object> ageData, out Dictionary<int, object> ageGroupData, 
                                        out Dictionary<int, object> raceData, out Dictionary<int, object> raceGroupData, out Dictionary<int, object> sexData, 
                                        out Dictionary<int, object> sexGroupData)
        {
            hospitalData = GetAndInitIfNeeded(drgData, hospitalid);

            ageData = GetAndInitIfNeeded(hospitalData, 1);
            ageGroupData = GetAndInitIfNeeded(ageData, ageid);

            sexData = GetAndInitIfNeeded(hospitalData, 2);
            sexGroupData = GetAndInitIfNeeded(sexData, sexid);

            raceData = GetAndInitIfNeeded(hospitalData, 4);
            raceGroupData = GetAndInitIfNeeded(raceData, raceid);

            AddDischarge2(ageGroupData, dischargeQuarter);
            AddDischarge2(sexGroupData, dischargeQuarter);
            AddDischarge2(raceGroupData, dischargeQuarter);

            AddCost2(ageGroupData, totalCost, dischargeQuarter);
            AddCost2(sexGroupData, totalCost, dischargeQuarter);
            AddCost2(raceGroupData, totalCost, dischargeQuarter);
        }

        //private static void AddDischarges(Dictionary<int, object> groupData, List<RptValue<int?>> numberOfDischarges)
        //{
        //    if (!groupData.ContainsKey(DISCHARGES))
        //    {
        //        groupData.Add(DISCHARGES, new List<RptValue<int?>>());
        //    }

        //    if (numberOfDischarges.Any(d => !d.Value.HasValue))
        //    {
        //        var tempDischarges = new List<RptValue<int?>>();

        //        numberOfDischarges.ForEach(d => { tempDischarges.Add(!d.Value.HasValue ? new RptValue<int?> {Quarter = d.Quarter, Value = 1} : d); });

        //        numberOfDischarges = tempDischarges.ToList();
        //    }

        //    ((List<RptValue<int?>>)groupData[DISCHARGES]).AddRange(numberOfDischarges);
        //}

        private static void AddDischarges(Dictionary<int, object> groupData, Dictionary<int, List<int?>> numberOfDischarges)
        {
            if (!groupData.ContainsKey(DISCHARGES))
            {
                var quarterly = new Dictionary<int, List<int?>>();

                for (var i = 0; i <= 4; i++)
                    quarterly.Add(i, new List<int?>());

                groupData.Add(DISCHARGES, quarterly);
            }

            foreach(var item in numberOfDischarges)
                ((Dictionary<int, List<int?>>)groupData[DISCHARGES])[item.Key].AddRange(item.Value);
        }

        //private static void AddDischarge(Dictionary<int, object> groupData, int dischargeQuarter, int numberOfDischarges = 1)
        //{
        //    if (!groupData.Keys.Contains(DISCHARGES))
        //    {
        //        groupData.Add(DISCHARGES, new List<RptValue<int?>>());
        //    }
        //    ((List<RptValue<int?>>)groupData[DISCHARGES]).Add(new RptValue<int?> { Quarter = dischargeQuarter, Value = numberOfDischarges });
        //}

        private static void AddDischarge2(Dictionary<int, object> groupData, int dischargeQuarter, int numberOfDischarges = 1)
        {
            if (!groupData.Keys.Contains(DISCHARGES))
            {
                var quarterly = new Dictionary<int, List<int?>>();

                for (var i = 0; i <= 4; i++)
                    quarterly.Add(i, new List<int?>());

                groupData.Add(DISCHARGES, quarterly);;
            }
           ((Dictionary<int, List<int?>>)groupData[DISCHARGES])[dischargeQuarter].Add(numberOfDischarges);
        }


        //private static void AddCost(Dictionary<int, object> groupData, float? totalCost, int dischargeQuarter)
        //{
        //    if (!groupData.Keys.Contains(COSTS))
        //    {
        //        groupData.Add(COSTS, new List<RptValue<float?>>());
        //    }
        //    ((List<RptValue<float?>>)groupData[COSTS]).Add(new RptValue<float?> { Quarter = dischargeQuarter, Value = totalCost });
        //}

        private static void AddCost2(Dictionary<int, object> groupData, float? totalCost, int dischargeQuarter)
        {
            if (!groupData.Keys.Contains(COSTS))
            {
                var quarterly = new Dictionary<int, List<float?>>();

                for (var i = 0; i <= 4; i++)
                    quarterly.Add(i, new List<float?>());

                groupData.Add(COSTS, quarterly);
            }
            ((Dictionary<int, List<float?>>)groupData[COSTS])[dischargeQuarter].Add(totalCost);
        }

        //private static void AddCost(Dictionary<int, object> groupData, List<float?> totalCost, int dischargeQuarter)
        //{
        //    if (!groupData.Keys.Contains(COSTS))
        //    {
        //        groupData.Add(COSTS, new List<RptValue<float?>>());
        //    }
        //    ((List<RptValue<float?>>)groupData[COSTS]).AddRange(totalCost.Select(cost => new RptValue<float?> { Quarter = dischargeQuarter, Value = cost }));
        //}

        //private static void AddCosts(Dictionary<int, object> groupData, List<RptValue<float?>> totalCost)
        //{
        //    if (!groupData.Keys.Contains(COSTS))
        //    {
        //        groupData.Add(COSTS, new List<RptValue<float?>>());
        //    }
        //    ((List<RptValue<float?>>)groupData[COSTS]).AddRange(totalCost);
        //}

        private static void AddCosts(Dictionary<int, object> groupData, Dictionary<int, List<float?>> totalCosts)
        {
            if (!groupData.Keys.Contains(COSTS))
            {
                var quarterly = new Dictionary<int, List<float?>>();

                for (var i = 0; i <= 4; i++)
                    quarterly.Add(i, new List<float?>());

                groupData.Add(COSTS, quarterly);
            }

            foreach(var item in totalCosts)
                ((Dictionary<int, List<float?>>)groupData[COSTS])[item.Key].AddRange(item.Value);
        }
    }

    [Serializable]
    internal class RptValue<T>
    {
        public T Value { get; set; }
        public int Quarter { get; set; }
    }
}
