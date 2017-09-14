using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace IPGenerator
{
    /// <summary>
    /// The static extension method helper class.
    /// </summary>
    static class helper
    {
        /// <summary>
        /// Returns the given potion of the string after the last occurrence of the [search] string.
        /// an empty string [s] is returned if the [search] string is not found
        /// </summary>
        /// <param name="s">string to search</param>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public static string SubStrAfterLast(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(s.LastIndexOf(search) + search.Length) : string.Empty;
        }

        /// <summary>
        /// Subs the string before last.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="search">The search.</param>
        /// <returns></returns>
        public static string SubStrBeforeLast(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(0, s.LastIndexOf(search)) : s;
        }

        /// <summary>
        /// Ins the specified enumerable.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns></returns>
        public static bool In(this object obj, IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Contains(obj);
        }
        /// <summary>
        /// Medians the specified avarage.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="avarage">The avarage.</param>
        /// <returns></returns>
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

        /*
        public static float? Median(this IEnumerable<float?> source, float? avarage)
        {
            if (avarage.HasValue)
            {
                for (var i = 0; i < source.Count(); i++)
                {
                    if (!source.[i].HasValue)
                        source[i] = avarage;
                }
            }

            int count = source.Count();
            
            var orderedSource = source.OrderBy(p => p);
            float? median = orderedSource.ElementAt(count / 2) + orderedSource.ElementAt((count - 1) / 2);

            return median.HasValue ? median.Value : -1;
        }
        */
        /// <summary>
        /// Medians the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Medians the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
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

        /// <summary>
        /// To the partial array string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
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

        /// <summary>
        /// To the partial array string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
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

        /// <summary>
        /// To the partial array string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
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

    /// <summary>
    /// The Main class for the IP Utilization Report Generator console application.
    /// </summary>
    class Program
    {
        const int ID = 0,
                    CAT_ID = 1,
                    CAT_VAL = 2,
                    DISCHARGES = 3,
                    MEAN_CHARGES = 4,
                    MEAN_COSTS = 5,
                    MEAN_LOS = 6,
                    MEDIAN_CHARGES = 7,
                    MEDIAN_COSTS = 8,
                    MEDIAN_LOS = 9,
                    CHARGES = 10,
                    COSTS = 11,
                    LENGTH_OF_STAYS = 12,
                    QUARTERS = 13;

        private const string PROGRESS_BAR_REMAINING = "                                                  ";
        private const string PROGRESS_BAR_DONE = "##################################################";

        static Stopwatch _stopWatch;
        //static Dictionary<int, string[]> HospitalRegion;
        static Dictionary<int, HospitalAddressData> _hospitalRegion;
        static Dictionary<int, List<int>> _hospitalCategory;
        static int _flushCount = 0;

        static List<Dictionary<int, string>> _ipNationalTotals = null;
        static int _logMod = 0;
        static string _tempPath;
        static string _rootDir;
        static string _mainConnectionString;

        static string _websiteId;
        static string _contentItemRecored;
        static int _suppression;
        static string _regionType;
        static List<int> _reportQuarters;
        static int _reportYear;
        static int _timeout;
        static bool _applyOptimization;

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                _rootDir = ConfigurationManager.AppSettings["reportDir"];
                _websiteId = ConfigurationManager.AppSettings["websiteID"];
                _contentItemRecored = ConfigurationManager.AppSettings["ContentItemRecord"];
                _suppression = int.Parse(ConfigurationManager.AppSettings["Suppression"]);
                _regionType = ConfigurationManager.AppSettings["RegionType"];
                _reportQuarters = ConfigurationManager.AppSettings["ReportQuarter"] != null
                    ? new List<int>(ConfigurationManager.AppSettings["ReportQuarter"].Split('|').Select(int.Parse).ToList())
                    : new List<int> { 1, 2, 3, 4 }; //int.Parse(ConfigurationManager.AppSettings["ReportQuarter"] ?? "0");
                _reportYear = int.Parse(ConfigurationManager.AppSettings["ReportYear"] ?? DateTime.Now.Year.ToString());
                _timeout = int.Parse(ConfigurationManager.AppSettings["Timeout"]);
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
                        case "-wid":
                            _websiteId = args[i + 1];
                            break;
                        case "-i":
                            _contentItemRecored = args[i + 1];
                            break;
                        case "-s":
                            _suppression = int.Parse(args[i + 1]);
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
                        //case "-rq":
                        //    _reportQuarters = args[i + 1] != null
                        //                                ? new List<int>(args[i + 1].Split('|').Select(int.Parse).ToList())
                        //                                : new List<int> { 1, 2, 3, 4 };
                        //    break;
                        case "-ry":
                            _reportYear = int.Parse(args[i + 1]);
                            break;
                        case "-o":
                            _applyOptimization = bool.Parse(args[i + 1]);
                            break;
                    }
                }
                //-------------------------------------//

                var tempPath = Path.GetTempPath();
                Directory.CreateDirectory(tempPath + "Monahrq\\");
                Directory.CreateDirectory(tempPath + "Monahrq\\Generators\\");
                Directory.CreateDirectory(tempPath + "Monahrq\\Generators\\IPGenerator\\");
                _tempPath = tempPath + "Monahrq\\Generators\\IPGenerator\\" + Guid.NewGuid().ToString().Substring(0, 8);

                string rootDir = _rootDir;

                //---- delete old data -----//

                if (Directory.Exists(rootDir))
                {
                    if (_logMod == 0)
                        Console.WriteLine("deleteing old output and/or temp data ...");
                    
                    Directory.Delete(rootDir, true);

                    if (Directory.Exists(tempPath + "Monahrq\\Generators\\IPGenerator\\"))
                        Directory.Delete(tempPath + "Monahrq\\Generators\\IPGenerator\\", true);
                }

                _stopWatch = new Stopwatch();
                Stopwatch totalWatch = new Stopwatch();
                totalWatch.Start();

                InitializeHospitalRegion();
                InitializeHospitalCategory();

                string drgSQL = ConfigurationManager.AppSettings["DRG_SQL"].Replace("[websiteID]", _websiteId).Replace("[ContentItemRecord]", _contentItemRecored);
                string mdcSQL = ConfigurationManager.AppSettings["MDC_SQL"].Replace("[websiteID]", _websiteId).Replace("[ContentItemRecord]", _contentItemRecored);
                string ccsSQL = ConfigurationManager.AppSettings["CCS_SQL"].Replace("[websiteID]", _websiteId).Replace("[ContentItemRecord]", _contentItemRecored);
                string prccsSQL = ConfigurationManager.AppSettings["PRCCS_SQL"].Replace("[websiteID]", _websiteId).Replace("[ContentItemRecord]", _contentItemRecored);

                //------ compile partial files -----//
                InitIPNationalTotals();

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

                //------------------test
                // tempPath=@"C:\Users\Hossam\AppData\Local\Temp\Monahrq\Generators\IPGenerator\78cbd9be-1707-49e2-9018-881692f5cbda";
                //------------------

                //                var tempdirs = Directory.EnumerateDirectories(_tempPath);
                //                if (_logMod == 0)
                //                    Console.WriteLine();
                //#if DEBUG
                //                Console.WriteLine("Compiling");
                //#endif
                //                foreach (var dir in tempdirs)
                //                {
                //                    foreach (var l1 in Directory.EnumerateDirectories(dir))
                //                    {
                //                        Console.WriteLine("Compiling :" + dir.Substring(dir.LastIndexOf("\\", StringComparison.InvariantCulture)) + l1.Substring(l1.LastIndexOf("\\", StringComparison.InvariantCulture)));

                //                        foreach (var l2 in Directory.EnumerateDirectories(l1))
                //                        {
                //                            CompilePartials(l2, l2.Replace(_tempPath, rootDir));
                //                        }
                //                    }
                //                }

                TimeSpan ts = totalWatch.Elapsed;

                // Format and display the TimeSpan value. 
                string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                Console.WriteLine("Inpatient Hospital Discharge Data Total RunTime: " + elapsedTime);

                //---- clear temp data -----------------//
                //GC.Collect();
                //Directory.Delete(tempPath, true);

                //Console.WriteLine("Press any key to exit.");
               // Console.ReadKey();
            }
            catch (Exception e)
            {
                var exc = e.GetBaseException();
                Console.WriteLine("Exception:" + exc.Message);
                Console.WriteLine(exc.StackTrace);
                
            }
        }

        /// <summary>
        /// Compiles the partials per dimension.
        /// </summary>
        /// <param name="tempPath">The temporary path.</param>
        /// <param name="newPath">The new path.</param>
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

        /// <summary>
        /// Deleteds the temporary files.
        /// </summary>
        /// <param name="tempFilePath">The temporary file path.</param>
        private static void DeletedTempFiles(string tempFilePath)
        {
            if (Directory.Exists(tempFilePath))
            {
                Directory.Delete(tempFilePath, true);
            }
        }

        /// <summary>
        /// Initializes the ip national totals.
        /// </summary>
        private static void InitIPNationalTotals()
        {
            string ipNationalTotalsSQL = ConfigurationManager.AppSettings["IPNationalTotals_SQL"].Replace("[websiteID]", _websiteId);
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = ipNationalTotalsSQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();

            int quarter = 1;
            _ipNationalTotals = new List<Dictionary<int, string>>();
            do
            {
                var nationalTotal = new Dictionary<int, string>();
                _ipNationalTotals.Add(nationalTotal);
                nationalTotal.Add(-1, "\"Discharges\":-1,\"MeanCharges\":-1,\"MeanCost\":-1,\"MeanLOS\":-1,\"MedianCharges\":-1,\"MedianCost\":-1,\"MedianLOS\":-1," /*+
                                      "\"Q1_Discharges\":-1,\"Q1_MeanCharges\":-1,\"Q1_MeanCost\":-1,\"Q1_MeanLOS\":-1,\"Q1_MedianCharges\":-1,\"Q1_MedianCost\":-1,\"Q1_MedianLOS\":-1," +
                                      "\"Q2_Discharges\":-1,\"Q2_MeanCharges\":-1,\"Q2_MeanCost\":-1,\"Q2_MeanLOS\":-1,\"Q2_MedianCharges\":-1,\"Q2_MedianCost\":-1,\"Q2_MedianLOS\":-1," +
                                      "\"Q3_Discharges\":-1,\"Q3_MeanCharges\":-1,\"Q3_MeanCost\":-1,\"Q3_MeanLOS\":-1,\"Q3_MedianCharges\":-1,\"Q3_MedianCost\":-1,\"Q3_MedianLOS\":-1," +
                                      "\"Q4_Discharges\":-1,\"Q4_MeanCharges\":-1,\"Q4_MeanCost\":-1,\"Q4_MeanLOS\":-1,\"Q4_MedianCharges\":-1,\"Q4_MedianCost\":-1,\"Q4_MedianLOS\":-1,"*/);

                while (dataRead.Read())
                {
                    int id = dataRead.GetInt32(0);
                    string value = string.Format("\"Discharges\":{0},\"MeanCharges\":{1},\"MeanCost\":{2},\"MeanLOS\":{3},\"MedianCharges\":{4},\"MedianCost\":{5},\"MedianLOS\":{6},",
                        !dataRead.IsDBNull(1) ? dataRead.GetInt32(1).ToString() : "null",
                        !dataRead.IsDBNull(2) ? dataRead.GetDouble(2) : -1,
                        !dataRead.IsDBNull(3) ? dataRead.GetDouble(3) : -1,
                        !dataRead.IsDBNull(4) ? dataRead.GetFloat(4).ToString() : "null",
                        !dataRead.IsDBNull(5) ? dataRead.GetFloat(5) : -1,
                        !dataRead.IsDBNull(6) ? dataRead.GetFloat(6) : -1,
                        !dataRead.IsDBNull(7)
                        ? dataRead.GetFloat(7).ToString() : "null");

                    // Quarter 1
                    //value += string.Format("\"Q{7}_Discharges\":{0},\"Q{7}_MeanCharges\":{1},\"Q{7}_MeanCost\":{2},\"Q{7}_MeanLOS\":{3},\"Q{7}_MedianCharges\":{4},\"Q{7}_MedianCost\":{5},\"Q{7}_MedianLOS\":{6},",
                    //                      "null", -1, -1, "null", -1, -1, "null", 1);

                    //// Quarter 2
                    //quarter = quarter+1;
                    //value += string.Format("\"Q{7}_Discharges\":{0},\"Q{7}_MeanCharges\":{1},\"Q{7}_MeanCost\":{2},\"Q{7}_MeanLOS\":{3},\"Q{7}_MedianCharges\":{4},\"Q{7}_MedianCost\":{5},\"Q{7}_MedianLOS\":{6},",
                    //                      "null", -1, -1, "null", -1, -1, "null", 2);

                    //// Quarter 3
                    //quarter = quarter+1;
                    //value += string.Format("\"Q{7}_Discharges\":{0},\"Q{7}_MeanCharges\":{1},\"Q{7}_MeanCost\":{2},\"Q{7}_MeanLOS\":{3},\"Q{7}_MedianCharges\":{4},\"Q{7}_MedianCost\":{5},\"Q{7}_MedianLOS\":{6},",
                    //                      "null", -1, -1, "null", -1, -1, "null", 3);

                    //// Quarter 4
                    //quarter = quarter+1;
                    //value += string.Format("\"Q{7}_Discharges\":{0},\"Q{7}_MeanCharges\":{1},\"Q{7}_MeanCost\":{2},\"Q{7}_MeanLOS\":{3},\"Q{7}_MedianCharges\":{4},\"Q{7}_MedianCost\":{5},\"Q{7}_MedianLOS\":{6}",
                    //                      "null", -1, -1, "null", -1, -1, "null", 4);

                    nationalTotal.Add(id, value);
                }

            } while (dataRead.NextResult());
        }

        /// <summary>
        /// Compiles the partials.
        /// </summary>
        /// <param name="sourceDir">The source dir.</param>
        /// <param name="targetDir">The target dir.</param>
        private static void CompilePartials(string sourceDir, string targetDir)
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
            var dicID = sourceDir.Substring(sourceDir.LastIndexOf("_", StringComparison.InvariantCulture) + 1);
            Dictionary<int, object> dic = null;
            foreach (var pbf in Directory.EnumerateFiles(sourceDir, "PartialBuffer*"))
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

                        foreach (var hosp in tempdic)
                        {
                            if (dic.Keys.Contains(hosp.Key))
                            {
                                var dichosp = (Dictionary<int, object>)dic[hosp.Key];
                                foreach (var cat in (Dictionary<int, object>)hosp.Value)
                                {
                                    if (cat.Key == QUARTERS)
                                    {
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

                                                AddDischarges(diccatGroup, (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                                AddCharges(diccatGroup, (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES]);
                                                AddCosts(diccatGroup, (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
                                                AddLengthOfStays(diccatGroup, (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS]);
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

            var fileCount = 0;
            float? averageCost;

            fileCount = dic.Count + 2;
            ASCIIEncoding acii = new ASCIIEncoding();

            //------   --------//
            int processed = 0;
            int progress;
            //Console.WriteLine(drg.Key);

            Dictionary<int, object> combinedDetails = new Dictionary<int, object>();

            Dictionary<int, object> summary = new Dictionary<int, object>();
            Directory.CreateDirectory(targetDir);

            foreach (var hosp in (Dictionary<int, object>)dic)
            {

                #region Deatils
                Dictionary<int, object> curentHospitalData = GetAndInitIfNeeded(summary, hosp.Key);

                using (FileStream fs = File.Create(targetDir + @"\details_" + hosp.Key + @".js"))
                {

                    string fileContent = "$.monahrq.inpatientutilization = {\"NationalData\" : [{" + _ipNationalTotals[utilId][utilValue] + "}],"
                                         + "\"TableData\": [";
                    string catGroupData = "";

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
                            if (combinedDetails.ContainsKey(cat.Key))
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
                                if (combinedDetailsCat.ContainsKey(catGroup.Key))
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
                                    var dc = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES];
                                    AddDischarges(curentHospitalData, dc);
                                    var ch = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES];
                                    AddCharges(curentHospitalData, ch);
                                    var co = (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS];
                                    AddCosts(curentHospitalData, co);
                                    var los = (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS];
                                    AddLengthOfStays(curentHospitalData, los);

                                    //var dc = ((List<RptValue<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).ToList();
                                    //AddDischarges(curentHospitalData, dc);
                                    //var ch = (List<RptValue<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES];
                                    //AddCharges(curentHospitalData, ch);
                                    //var co = (List<RptValue<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS];
                                    //AddCosts(curentHospitalData, co);
                                    //var los = ((List<RptValue<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS]);
                                    //AddLengthOfStay(curentHospitalData, los);
                                }

                                var dc2 = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES];
                                AddDischarges(combinedDetailsCatGroup, dc2);
                                var ch2 = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES];
                                AddCharges(combinedDetailsCatGroup, ch2);
                                var co2 = (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS];
                                AddCosts(combinedDetailsCatGroup, co2);
                                var los2 = (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS];
                                AddLengthOfStays(combinedDetailsCatGroup, los2);

                                //var dc2 = ((List<RptValue<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                //AddDischarges(combinedDetailsCatGroup, dc2);
                                //var ch2 = ((List<RptValue<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES]);
                                //AddCharges(combinedDetailsCatGroup, ch2);
                                //var co2 = ((List<RptValue<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
                                //AddCosts(combinedDetailsCatGroup, co2);
                                //var los2 = ((List<RptValue<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS]);
                                //AddLengthOfStay(combinedDetailsCatGroup, los2);

                                if (!string.IsNullOrEmpty(catGroupData))
                                    catGroupData += ",";

                                catGroupData += "{\"ID\": " + hosp.Key + ",\"CatID\": " + cat.Key + ",\"CatVal\": " + catGroup.Key;

                                if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])
                                    .SelectMany(d => d.Value)
                                    .Sum() > _suppression)
                                {
                                    var co = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])
                                            .SelectMany(rpt => rpt.Value)
                                            .ToList();
                                    averageCost = co.Average();

                                    var ch = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES])
                                            .SelectMany(rpt => rpt.Value)
                                            .ToList();
                                    var meanCharges = ch.Average();

                                    averageCost = averageCost ?? -1;
                                    meanCharges = meanCharges ?? -1;

                                    var los = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS])
                                            .SelectMany(rpt => rpt.Value)
                                            .ToList();
                                    var meanLOS = los.Average();
                                    var medianLOS = los.Median();

                                    var medianCharges = ch.Median();
                                    var medianCosts = co.Median(averageCost);

                                    medianCharges = medianCharges ?? -1;
                                    medianCosts = medianCosts ?? -1;

                                    var discharges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])
                                            .SelectMany(d => d.Value)
                                            .Sum();

                                    catGroupData +=
                                            ",\"Discharges\": " + discharges +
                                            ",\"MeanCharges\" :" + meanCharges +
                                            ",\"MeanCosts\" :" + averageCost +
                                            ",\"MeanLOS\" :" + meanLOS +
                                            ",\"MedianCharges\" :" + medianCharges +
                                            ",\"MedianCosts\" :" + medianCosts +
                                            ",\"MedianLOS\" :" + medianLOS;


                                    for (var q = 1; q <= 4; q++)
                                    {
                                        if (q.In(_reportQuarters.ToList()))
                                        {
                                            var costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])[q]
                                                    .Select(rpt => rpt)
                                                    .ToList();
                                            var averageCost2 = costs.Average();

                                            var charges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES])[q]
                                                    .Select(rpt => rpt)
                                                    .ToList();
                                            var meanCharges2 = charges.Average();

                                            averageCost2 = averageCost2 ?? -1;
                                            meanCharges2 = meanCharges2 ?? -1;

                                            var los1 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS])[q]
                                                    .Select(rpt => rpt)
                                                    .ToList();
                                            double? meanLOS2 = los1 != null && los1.Any() ? los1.Average() : (double?)null;

                                            //var los2 = ((List<RptValue<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS]).Where(o => o.Quarter == q).Select(rpt => rpt.Value).ToList();
                                            var medianLOS2 = los1 != null && los1.Any() ? los1.Median() : (double?)null;

                                            var medianCharges2 = charges.Median();
                                            var medianCosts2 = costs.Median(averageCost);

                                            medianCharges2 = medianCharges2 ?? -1;
                                            medianCosts2 = medianCosts2 ?? -1;

                                            var discharges2 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])[q]
                                                    .Sum(d => d);


                                            catGroupData +=
                                                    ",\"Q" + q + string.Format(
                                                        "_Discharges\": {0}",
                                                        discharges2.HasValue && discharges2.Value > 0 ? discharges2.Value.ToString() : "null") +
                                                    ",\"Q" + q + "_MeanCharges\":" + meanCharges2 +
                                                    ",\"Q" + q + "_MeanCosts\":" + averageCost2 +
                                                    ",\"Q" + q + string.Format(
                                                        "_MeanLOS\": {0}",
                                                        meanLOS2.HasValue ? meanLOS2.Value.ToString() : "null") +
                                                    ",\"Q" + q + "_MedianCharges\" :" + medianCharges2 +
                                                    ",\"Q" + q + "_MedianCosts\" :" + medianCosts2 +
                                                    ",\"Q" + q + string.Format(
                                                        "_MedianLOS\": {0}",
                                                        medianLOS2.HasValue ? medianLOS2.ToString() : "null");
                                        }
                                        else
                                        {
                                            catGroupData +=
                                                    ",\"Q" + q + "_Discharges\": null" +
                                                    ",\"Q" + q + "_MeanCharges\": -1" +
                                                    ",\"Q" + q + "_MeanCosts\": -1" +
                                                    ",\"Q" + q + "_MeanLOS\": null" +
                                                    ",\"Q" + q + "_MedianCharges\": -1" +
                                                    ",\"Q" + q + "_MedianCosts\": -1" +
                                                    ",\"Q" + q + "_MedianLOS\": null";
                                        }
                                    }
                                }
                                else
                                {
                                    catGroupData +=
                                            ",\"Discharges\":-2,\"MeanCharges\":-2,\"MeanCosts\":-2,\"MeanLOS\":-2,\"MedianCharges\":-2,\"MedianCosts\":-2,\"MedianLOS\":-2"
                                            +
                                            ",\"Q1_Discharges\":-2,\"Q1_MeanCharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MeanLOS\":-2,\"Q1_MedianCharges\":-2,\"Q1_MedianCosts\":-2,\"Q1_MedianLOS\":-2"
                                            +
                                            ",\"Q2_Discharges\":-2,\"Q2_MeanCharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MeanLOS\":-2,\"Q2_MedianCharges\":-2,\"Q2_MedianCosts\":-2,\"Q2_MedianLOS\":-2"
                                            +
                                            ",\"Q3_Discharges\":-2,\"Q3_MeanCharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MeanLOS\":-2,\"Q3_MedianCharges\":-2,\"Q3_MedianCosts\":-2,\"Q3_MedianLOS\":-2"
                                            +
                                            ",\"Q4_Discharges\":-2,\"Q4_MeanCharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MeanLOS\":-2,\"Q4_MedianCharges\":-2,\"Q4_MedianCosts\":-2,\"Q4_MedianLOS\":-2";
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
                }
                ((Dictionary<int, object>)hosp.Value).Clear();

                GC.Collect();
            }
            dic.Clear();
            GC.Collect();
            #endregion

            #region Combined Details

            using (FileStream fsAll = File.Create(targetDir + @"\details.js"))
            {
                string fsAllString = "$.monahrq.inpatientutilization = {\"NationalData\" : [{" + _ipNationalTotals[utilId][utilValue] + "}],"
                                     + "\"TableData\" : [";
                foreach (var cat in combinedDetails)
                {
                    foreach (var catGroup in (Dictionary<int, object>)cat.Value)
                    {
                        fsAllString += "{\"ID\": " + dicID + ",\"CatID\": " + cat.Key + ",\"CatVal\": " + catGroup.Key;
                        if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                            > _suppression)
                        {
                            List<float?> cos = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])
                                    .SelectMany(rpt => rpt.Value)
                                    .ToList();
                            averageCost = cos.Average();

                            List<int?> charg = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES])
                                    .SelectMany(rpt => rpt.Value)
                                    .ToList();
                            var meanCharges = charg.Average();

                            averageCost = averageCost ?? -1;
                            meanCharges = meanCharges ?? -1;

                            List<int> los2 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS])
                                    .SelectMany(rpt => rpt.Value)
                                    .ToList();
                            var meanLOS = los2.Average();
                            var medianLOS = los2.Median();

                            var medianCharges = charg.Median();
                            var medianCosts = cos.Median(averageCost);

                            medianCharges = medianCharges ?? -1;
                            medianCosts = medianCosts ?? -1;

                            var discharges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])
                                    .SelectMany(d => d.Value)
                                    .Sum();

                            fsAllString +=
                                    string.Format(",\"Discharges\": {0}", discharges.HasValue ? discharges.Value.ToString() : "null") +
                                    ",\"MeanCharges\": " + meanCharges +
                                    ",\"MeanCosts\": " + averageCost +
                                    ",\"MeanLOS\": " + meanLOS +
                                    ",\"MedianCharges\": " + medianCharges +
                                    ",\"MedianCosts\": " + medianCosts +
                                    ",\"MedianLOS\": " + medianLOS;

                            for (var q = 1; q <= 4; q++)
                            {
                                //if (q.In(_reportQuarters.ToList()))
                                //{
                                List<float?> cos1 = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])[q]
                                        .Select(rpt => rpt)
                                        .ToList();
                                var averageCost1 = cos1.Average();

                                List<int?> charg1 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES])[q]
                                        .Select(rpt => rpt)
                                        .ToList();
                                var meanCharges1 = charg1.Average();

                                averageCost1 = averageCost1 ?? -1;
                                meanCharges1 = meanCharges1 ?? -1;

                                List<int> los3 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS])[q]
                                        .Select(rpt => rpt)
                                        .ToList();
                                var meanLOS1 = los3.Any() ? los3.Average().ToString() : "null";
                                var medianLOS1 = los3.Any() ? los3.Median().ToString() : "null";

                                var medianCharges1 = charg1.Median();
                                var medianCosts1 = cos1.Median(averageCost1);

                                medianCharges1 = medianCharges1 ?? -1;
                                medianCosts1 = medianCosts1 ?? -1;

                                var discharges1 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])[q].Sum(d => d);
                                //discharges1 = discharges1 ?? "null";

                                fsAllString +=
                                        ",\"Q" + q + "_Discharges\": " + (discharges1 == null || discharges1 == 0 ? "null" : discharges1.ToString()) +
                                        ",\"Q" + q + "_MeanCharges\" :" + meanCharges1 +
                                        ",\"Q" + q + "_MeanCosts\" :" + averageCost1 +
                                        ",\"Q" + q + "_MeanLOS\" :" + meanLOS1 +
                                        ",\"Q" + q + "_MedianCharges\" :" + medianCharges1 +
                                        ",\"Q" + q + "_MedianCosts\" :" + medianCosts1 +
                                        ",\"Q" + q + "_MedianLOS\" :" + medianLOS1;
                                //}
                                //else
                                //{
                                //    fsAllString +=
                                //                   ",\"Q" + q + "_Discharges\": null" +
                                //                   ",\"Q" + q + "_MeanCharges\": -1" +
                                //                   ",\"Q" + q + "_MeanCosts\": -1" +
                                //                   ",\"Q" + q + "_MeanLOS\": null" +
                                //                   ",\"Q" + q + "_MedianCharges\": -1" +
                                //                   ",\"Q" + q + "_MedianCosts\": -1" +
                                //                   ",\"Q" + q + "_MedianLOS\": null";
                                //}
                            }
                        }
                        else
                        {
                            fsAllString +=
                                    ",\"Discharges\":-2,\"MeanCharges\":-2,\"MeanCosts\":-2,\"MeanLOS\":-2,\"MedianCharges\":-2,\"MedianCosts\":-2,\"MedianLOS\":-2"
                                    +
                                    ",\"Q1_Discharges\":-2,\"Q1_MeanCharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MeanLOS\":-2,\"Q1_MedianCharges\":-2,\"Q1_MedianCosts\":-2,\"Q1_MedianLOS\":-2"
                                    +
                                    ",\"Q2_Discharges\":-2,\"Q2_MeanCharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MeanLOS\":-2,\"Q2_MedianCharges\":-2,\"Q2_MedianCosts\":-2,\"Q2_MedianLOS\":-2"
                                    +
                                    ",\"Q3_Discharges\":-2,\"Q3_MeanCharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MeanLOS\":-2,\"Q3_MedianCharges\":-2,\"Q3_MedianCosts\":-2,\"Q3_MedianLOS\":-2"
                                    +
                                    ",\"Q4_Discharges\":-2,\"Q4_MeanCharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MeanLOS\":-2,\"Q4_MedianCharges\":-2,\"Q4_MedianCosts\":-2,\"Q4_MedianLOS\":-2";
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

            #endregion

            progress = processed * 100 / fileCount;
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            #region Summary

            using (FileStream fsSummary = File.Create(targetDir + @"\summary.js"))
            {
                string nationalData = "$.monahrq.inpatientutilization = {\"NationalData\":[{" + _ipNationalTotals[utilId][utilValue] + "}],";
                const string totalData =
                        "\"TotalData\" :[{{\"Discharges\":{0},\"MeanCharges\":{1},\"MeanCosts\":{2},\"MeanLOS\":{3},\"MedianCharges\":{4},\"MedianCosts\":{5},\"MedianLOS\":{6},"
                        +
                        "\"Q1_Discharges\":{7},\"Q1_MeanCharges\":{8},\"Q1_MeanCosts\":{9},\"Q1_MeanLOS\":{10},\"Q1_MedianCharges\":{11},\"Q1_MedianCosts\":{12},\"Q1_MedianLOS\":{13},"
                        +
                        "\"Q2_Discharges\":{14},\"Q2_MeanCharges\":{15},\"Q2_MeanCosts\":{16},\"Q2_MeanLOS\":{17},\"Q2_MedianCharges\":{18},\"Q2_MedianCosts\":{19},\"Q2_MedianLOS\":{20},"
                        +
                        "\"Q3_Discharges\":{21},\"Q3_MeanCharges\":{22},\"Q3_MeanCosts\":{23},\"Q3_MeanLOS\":{24},\"Q3_MedianCharges\":{25},\"Q3_MedianCosts\":{26},\"Q3_MedianLOS\":{27},"
                        +
                        "\"Q4_Discharges\":{28},\"Q4_MeanCharges\":{29},\"Q4_MeanCosts\":{30},\"Q4_MeanLOS\":{31},\"Q4_MedianCharges\":{32},\"Q4_MedianCosts\":{33},\"Q4_MedianLOS\":{34}}}],";
                string tableData = "\"TableData\" : [";
                //float? MeanCosts;

                var totalDischarges = new List<KeyValuePair<int, List<int?>>>();
                var totalCharges = new List<KeyValuePair<int, List<int?>>>();
                var totalCosts = new List<KeyValuePair<int, List<float?>>>();
                var totalLos = new List<KeyValuePair<int, List<int>>>();

                foreach (var hosp in summary)
                {
                    tableData += "{";

                    switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
                    {
                        case "DRG":
                        case "MDC":
                        case "CCS":
                        case "PRCCS":
                            tableData += "\"HospitalID\":" + hosp.Key +
                                         ",\"RegionID\":" + _hospitalRegion[hosp.Key].RegionId +
                                         ",\"CountyID\":" + _hospitalRegion[hosp.Key].CountyId +
                                         ",\"Zip\":" + _hospitalRegion[hosp.Key].Zip +
                                         ",\"HospitalType\":" + _hospitalCategory[hosp.Key][0];
                            break;
                        default:
                            tableData += "\"ID\":" + hosp.Key;
                            break;
                    }

                    if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum()
                        > _suppression)
                    {
                        List<float?> costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])
                                .SelectMany(rpt => rpt.Value)
                                .ToList();
                        averageCost = costs.Average();
                        var medianCosts = costs.Median(averageCost);

                        averageCost = averageCost ?? -1;
                        medianCosts = medianCosts ?? -1;

                        var discharges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])
                                .SelectMany(d => d.Value)
                                .Sum();
                        discharges = discharges ?? 0;

                        List<int?> charges1 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[CHARGES])
                                .SelectMany(rpt => rpt.Value)
                                .ToList();
                        var meanCharges = charges1.Average();
                        var medianCharges = charges1.Median();

                        meanCharges = meanCharges ?? -1;
                        medianCharges = medianCharges ?? -1;

                        List<int> los1 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[LENGTH_OF_STAYS])
                                .SelectMany(rpt => rpt.Value)
                                .ToList();
                        var meanLOS = los1.Average();
                        var medianLOS = los1.Median();

                        tableData += ",\"Discharges\" :" + ((discharges == null || discharges == 0) ? "null" : discharges.Value.ToString())
                                     + ",\"MeanCharges\" :" + meanCharges + ",\"MeanCosts\" :" + averageCost + ",\"MeanLOS\" :" + meanLOS +
                                     ",\"MedianCharges\" :" + medianCharges + ",\"MedianCosts\" :" + medianCosts + ",\"MedianLOS\" :" + medianLOS;

                        for (var q = 1; q <= 4; q++)
                        {
                            //if (q.In(_reportQuarters.ToList()))
                            //{
                            var costsDic = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]);
                            List<float?> cost3 = costsDic[q].Select(c => c).ToList();
                            var averageCost2 = cost3.Average();
                            var medianCosts2 = cost3.Median(averageCost);

                            averageCost2 = averageCost2 ?? -1;
                            medianCosts2 = medianCosts2 ?? -1;

                            List<int?> charges3 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[CHARGES])[q]
                                    .Select(c => c)
                                    .ToList();
                            var meanCharges2 = charges3.Average();
                            var medianCharges2 = charges3.Median();

                            meanCharges2 = meanCharges2 ?? -1;
                            medianCharges2 = medianCharges2 ?? -1;

                            List<int> los3 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[LENGTH_OF_STAYS])[q]
                                    .Select(c => c)
                                    .ToList();
                            var meanLOS2 = los3.Any() ? los3.Average() : -1;
                            var medianLOS2 = los3.Any() ? los3.Median() : -1;

                            var discharges2 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].Sum(d => d);
                            discharges2 = discharges2 ?? 0;

                            tableData += ",\"Q" + q + "_Discharges\" :" + discharges2 +
                                         ",\"Q" + q + "_MeanCharges\" :" + meanCharges2 +
                                         ",\"Q" + q + "_MeanCosts\" :" + averageCost2 +
                                         ",\"Q" + q + "_MeanLOS\" :" + meanLOS2 +
                                         ",\"Q" + q + "_MedianCharges\" :" + medianCharges2 +
                                         ",\"Q" + q + "_MedianCosts\" :" + medianCosts2 +
                                         ",\"Q" + q + "_MedianLOS\" :" + medianLOS2;
                            //}
                            //else
                            //{
                            //    tableData += ",\"Q" + q + "_Discharges\":-2,\"Q" + q + "_MeanCharges\":-2,\"Q" + q + "_MeanCosts\":-2,\"Q" + q + "_MeanLOS\":-2,\"Q" + q + "_MedianCharges\":-2,\"Q" + q + "_MedianCosts\":-2,\"Q" + q + "_MedianLOS\":-2";
                            //}
                        }
                    }
                    else
                    {
                        tableData +=
                                ",\"Discharges\":-2,\"MeanCharges\":-2,\"MeanCosts\":-2,\"MeanLOS\":-2,\"MedianCharges\":-2,\"MedianCosts\":-2,\"MedianLOS\":-2"
                                +
                                ",\"Q1_Discharges\":-2,\"Q1_MeanCharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MeanLOS\":-2,\"Q1_MedianCharges\":-2,\"Q1_MedianCosts\":-2,\"Q1_MedianLOS\":-2"
                                +
                                ",\"Q2_Discharges\":-2,\"Q2_MeanCharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MeanLOS\":-2,\"Q2_MedianCharges\":-2,\"Q2_MedianCosts\":-2,\"Q2_MedianLOS\":-2"
                                +
                                ",\"Q3_Discharges\":-2,\"Q3_MeanCharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MeanLOS\":-2,\"Q3_MedianCharges\":-2,\"Q3_MedianCosts\":-2,\"Q3_MedianLOS\":-2"
                                +
                                ",\"Q4_Discharges\":-2,\"Q4_MeanCharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MeanLOS\":-2,\"Q4_MedianCharges\":-2,\"Q4_MedianCosts\":-2,\"Q4_MedianLOS\":-2";
                    }



                    tableData += "},";

                    totalDischarges.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).ToList());
                    totalCharges.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[CHARGES]).ToList());
                    totalCosts.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]).ToList());
                    totalLos.AddRange(((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[LENGTH_OF_STAYS]).ToList());
                }
                tableData += "]};";

                var totalMeanCharges = totalCharges.Count > 0 ? totalCharges.SelectMany(x => x.Value).ToList().Average(d => d) ?? -1 : -1;
                var totalMeanCosts = totalCosts.Count > 0 ? totalCosts.SelectMany(x => x.Value).ToList().Average(d => d) ?? -1 : -1;
                var totalMeanLOS = totalLos.Count > 0 ? totalLos.SelectMany(x => x.Value).ToList().Average(d => d) : -1;
                var totalMedianCharges = totalCharges.Count > 0 ? totalCharges.SelectMany(x => x.Value).ToList().Median() ?? -1 : -1;
                var totalMedianCosts = totalCosts.Count > 0 ? totalCosts.SelectMany(x => x.Value).ToList().Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOS = totalLos.Count > 0 ? totalLos.SelectMany(x => x.Value).ToList().Median() : -1;
                var totalDischargesString = totalDischarges.Count > 0
                    ? totalDischarges.SelectMany(x => x.Value).ToList().Sum(d => d).ToString()
                    : "null";

                // Q1
                var totalChargesQ1 = totalCharges.Where(d => d.Key == 1).Select(d => d).ToList();
                var totalMeanChargesQ1 = totalChargesQ1.Any() ? totalChargesQ1.SelectMany(d => d.Value).ToList().Average() ?? -1 : -1;

                var totalCostsQ1 = totalCosts.Where(d => d.Key == 1).Select(d => d).ToList();
                var totalMeanCostsQ1 = totalCostsQ1.Any() ? totalCostsQ1.SelectMany(d => d.Value).ToList().Average() ?? -1 : -1;

                var totalLOSQ1 = totalLos.Where(d => d.Key == 1).Select(d => d).ToList();
                var testLOSQ1 = totalLOSQ1.SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ1 = testLOSQ1.Any() ? testLOSQ1.Average() : -1;

                var totalMedianChargesQ1 = totalChargesQ1.Any() ? totalChargesQ1.SelectMany(d => d.Value).ToList().Median() ?? -1 : -1;
                var totalMedianCostsQ1 = totalCostsQ1.Any() ? totalCostsQ1.SelectMany(d => d.Value).ToList().Median(totalMeanCosts) ?? -1 : -1;

                var totalMedianLOSQ1 = testLOSQ1.Any() ? testLOSQ1.Median() : -1;

                var totalDischargesQ1 = totalDischarges.Where(d => d.Key == 1).Select(d => d).ToList();
                var totalDischargesStringQ1 = totalDischargesQ1.Any()
                    ? totalDischargesQ1.SelectMany(d => d.Value).ToList().Sum(d => d ?? 1).ToString()
                    : "null";

                // Q2
                var totalChargesQ2 = totalCharges.Where(d => d.Key == 2).Select(d => d).ToList();
                var totalMeanChargesQ2 = totalChargesQ2.Any() ? totalChargesQ2.SelectMany(d => d.Value).Average() ?? -1 : -1;

                var totalCostsQ2 = totalCosts.Where(d => d.Key == 2).Select(d => d).ToList();
                var totalMeanCostsQ2 = totalCostsQ2.Any() ? totalCostsQ2.SelectMany(d => d.Value).Average() ?? -1 : -1;

                var totalLOSQ2 = totalLos.Where(d => d.Key == 2).Select(d => d).ToList();
                var testLOSQ2 = totalLOSQ2.SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ2 = testLOSQ2.Any() ? testLOSQ2.Average() : -1;

                var totalMedianChargesQ2 = totalChargesQ2.Any() ? totalChargesQ2.SelectMany(d => d.Value).ToList().Median() ?? -1 : -1;
                var totalMedianCostsQ2 = totalCostsQ2.Any() ? totalCostsQ2.SelectMany(d => d.Value).ToList().Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOSQ2 = testLOSQ2.Any() ? testLOSQ2.Median() : -1;

                var totalDischargesQ2 = totalDischarges.Where(d => d.Key == 2).Select(d => d).ToList();
                var totalDischargesStringQ2 = totalDischargesQ2.Any()
                    ? totalDischargesQ2.SelectMany(d => d.Value).Sum(d => d ?? 1).ToString()
                    : "null";

                // Q3
                var totalChargesQ3 = totalCharges.Where(d => d.Key == 3).Select(d => d).ToList();
                var totalMeanChargesQ3 = totalChargesQ3.Any() ? totalChargesQ3.SelectMany(d => d.Value).Average() ?? -1 : -1;

                var totalCostsQ3 = totalCosts.Where(d => d.Key == 3).Select(d => d).ToList();
                var totalMeanCostsQ3 = totalCostsQ3.Any() ? totalCostsQ3.SelectMany(d => d.Value).Average() ?? -1 : -1;

                var totalLOSQ3 = totalLos.Where(d => d.Key == 3).Select(d => d).ToList();
                var testLOSQ3 = totalLOSQ3.SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ3 = testLOSQ3.Any() ? testLOSQ3.Average() : -1;

                var totalMedianChargesQ3 = totalChargesQ3.Any() ? totalChargesQ3.SelectMany(d => d.Value).ToList().Median() ?? -1 : -1;
                var totalMedianCostsQ3 = totalCostsQ3.Any() ? totalCostsQ3.SelectMany(d => d.Value).ToList().Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOSQ3 = testLOSQ3.Any() ? testLOSQ3.Median() : -1;

                var totalDischargesQ3 = totalDischarges.Where(d => d.Key == 3).Select(d => d).ToList();
                var totalDischargesStringQ3 = totalDischargesQ3.Any()
                    ? totalDischargesQ3.SelectMany(d => d.Value).ToList().Sum(d => d ?? 1).ToString()
                    : "null";

                // Q4
                var totalChargesQ4 = totalCharges.Where(d => d.Key == 4).Select(d => d).ToList();
                var totalMeanChargesQ4 = totalChargesQ4.Any() ? totalChargesQ4.SelectMany(d => d.Value).ToList().Average() ?? -1 : -1;

                var totalCostsQ4 = totalCosts.Where(d => d.Key == 4).Select(d => d).ToList();
                var totalMeanCostsQ4 = totalCostsQ4.Any() ? totalCostsQ4.SelectMany(d => d.Value).ToList().Average() ?? -1 : -1;

                var totalLOSQ4 = totalLos.Where(d => d.Key == 4).Select(d => d).ToList();
                var testLOSQ4 = totalLOSQ4.SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ4 = testLOSQ4.Any() ? testLOSQ4.Average() : -1;

                var totalMedianChargesQ4 = totalChargesQ4.Any() ? totalChargesQ4.SelectMany(d => d.Value).ToList().Median() ?? -1 : -1;
                var totalMedianCostsQ4 = totalCostsQ4.Any() ? totalCostsQ4.SelectMany(d => d.Value).ToList().Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOSQ4 = testLOSQ4.Any() ? testLOSQ4.Median() : -1;

                var totalDischargesQ4 = totalDischarges.Where(d => d.Key == 4).Select(d => d).ToList();
                var totalDischargesStringQ4 = totalDischargesQ4.Any()
                    ? totalDischargesQ4.SelectMany(d => d.Value).ToList().Sum(d => d ?? 1).ToString()
                    : "null";

                byte[] summaryResult = acii.GetBytes(
                    nationalData +
                    string.Format(
                        totalData,
                        totalDischargesString,
                        totalMeanCharges,
                        totalMeanCosts,
                        totalMeanLOS,
                        totalMedianCharges,
                        totalMedianCosts,
                        totalMedianLOS,

                        totalDischargesStringQ1,
                        totalMeanChargesQ1,
                        totalMeanCostsQ1,
                        totalMeanLOSQ1,
                        totalMedianChargesQ1,
                        totalMedianCostsQ1,
                        totalMedianLOSQ1,

                        totalDischargesStringQ2,
                        totalMeanChargesQ2,
                        totalMeanCostsQ2,
                        totalMeanLOSQ2,
                        totalMedianChargesQ2,
                        totalMedianCostsQ2,
                        totalMedianLOSQ2,

                        totalDischargesStringQ3,
                        totalMeanChargesQ3,
                        totalMeanCostsQ3,
                        totalMeanLOSQ3,
                        totalMedianChargesQ3,
                        totalMedianCostsQ3,
                        totalMedianLOSQ3,

                        totalDischargesStringQ4,
                        totalMeanChargesQ4,
                        totalMeanCostsQ4,
                        totalMeanLOSQ4,
                        totalMedianChargesQ4,
                        totalMedianCostsQ4,
                        totalMedianLOSQ4) + tableData);
                fsSummary.Write(summaryResult, 0, summaryResult.Length);
                processed++;
            }
            progress = processed * 100 / fileCount;
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            if (_logMod == 0)
                LogRuntime();
            #endregion
        }

        /// <summary>
        /// Compiles the optimized partials.
        /// </summary>
        /// <param name="sourceDir">The source dir.</param>
        /// <param name="targetDir">The target dir.</param>
        private static void CompileOptimizedPartials(string sourceDir, string targetDir)
        {
            //CompilePartials(sourceDir, targetDir);
            //return;

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
                    utilValue =
                        int.Parse(
                            folderName.Substring(folderName.LastIndexOf("_", StringComparison.InvariantCulture) + 1));
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
            var dicID = sourceDir.Substring(sourceDir.LastIndexOf("_", StringComparison.InvariantCulture) + 1);
            Dictionary<int, object> dic = null;
            foreach (var pbf in Directory.EnumerateFiles(sourceDir, "PartialBuffer*"))
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

                                                AddDischarges(
                                                    diccatGroup,
                                                    (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                                AddCharges(
                                                    diccatGroup,
                                                    (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES]);
                                                AddCosts(
                                                    diccatGroup,
                                                    (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
                                                AddLengthOfStays(
                                                    diccatGroup,
                                                    (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS]);

                                                //AddDischarges(diccatGroup, (List<RptValue<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]);
                                                //AddCharges(diccatGroup, (List<RptValue<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES]);
                                                //AddCosts(diccatGroup, (List<RptValue<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]);
                                                //AddLengthOfStay(diccatGroup, (List<RptValue<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS]);
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

            //var c = dic.Count;

            int fileCount = 0;
            float? averageCost;

            fileCount = dic.Count + 2;
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
            var dischargeGroupData = new StringBuilder();
            var meanChargeGroupData = new StringBuilder();
            var meanCostsGroupData = new StringBuilder();
            var meanLOSGroupData = new StringBuilder();
            var medianChargesGroupData = new StringBuilder();
            var medianCostsGroupData = new StringBuilder();
            var medianLOSGroupData = new StringBuilder();

            // Quarterly values
            var q1dischargeGroupData = new StringBuilder();
            var q1meanChargeGroupData = new StringBuilder();
            var q1meanCostsGroupData = new StringBuilder();
            var q1meanLOSGroupData = new StringBuilder();
            var q1medianChargesGroupData = new StringBuilder();
            var q1medianCostsGroupData = new StringBuilder();
            var q1medianLOSGroupData = new StringBuilder();

            var q2dischargeGroupData = new StringBuilder();
            var q2meanChargeGroupData = new StringBuilder();
            var q2meanCostsGroupData = new StringBuilder();
            var q2meanLOSGroupData = new StringBuilder();
            var q2medianChargesGroupData = new StringBuilder();
            var q2medianCostsGroupData = new StringBuilder();
            var q2medianLOSGroupData = new StringBuilder();

            var q3dischargeGroupData = new StringBuilder();
            var q3meanChargeGroupData = new StringBuilder();
            var q3meanCostsGroupData = new StringBuilder();
            var q3meanLOSGroupData = new StringBuilder();
            var q3medianChargesGroupData = new StringBuilder();
            var q3medianCostsGroupData = new StringBuilder();
            var q3medianLOSGroupData = new StringBuilder();

            var q4dischargeGroupData = new StringBuilder();
            var q4meanChargeGroupData = new StringBuilder();
            var q4meanCostsGroupData = new StringBuilder();
            var q4meanLOSGroupData = new StringBuilder();
            var q4medianChargesGroupData = new StringBuilder();
            var q4medianCostsGroupData = new StringBuilder();
            var q4medianLOSGroupData = new StringBuilder();

            using (var fs = File.Create(targetDir + @"\details.js"))
            {
                var fileContent = "$.monahrq.inpatientutilization = {\"NationalData\" : [{" +
                                 _ipNationalTotals[utilId][utilValue] + "}]," + "\"TableData\": [{";

                foreach (var hosp in (Dictionary<int, object>)dic)
                {
                    Dictionary<int, object> curentHospitalData = GetAndInitIfNeeded(summary, hosp.Key);

                    //string catGroupData = "";

                    levelIdGroupData.Append(hosp.Key + ",");

                    idGroupData.Append("[");
                    catIdGroupData.Append("[");
                    catValueGroupData.Append("[");
                    dischargeGroupData.Append("[");
                    meanChargeGroupData.Append("[");
                    meanCostsGroupData.Append("[");
                    meanLOSGroupData.Append("[");
                    medianChargesGroupData.Append("[");
                    medianCostsGroupData.Append("[");
                    medianLOSGroupData.Append("[");

                    q1dischargeGroupData.Append("[");
                    q1meanChargeGroupData.Append("[");
                    q1meanCostsGroupData.Append("[");
                    q1meanLOSGroupData.Append("[");
                    q1medianChargesGroupData.Append("[");
                    q1medianCostsGroupData.Append("[");
                    q1medianLOSGroupData.Append("[");

                    q2dischargeGroupData.Append("[");
                    q2meanChargeGroupData.Append("[");
                    q2meanCostsGroupData.Append("[");
                    q2meanLOSGroupData.Append("[");
                    q2medianChargesGroupData.Append("[");
                    q2medianCostsGroupData.Append("[");
                    q2medianLOSGroupData.Append("[");

                    q3dischargeGroupData.Append("[");
                    q3meanChargeGroupData.Append("[");
                    q3meanCostsGroupData.Append("[");
                    q3meanLOSGroupData.Append("[");
                    q3medianChargesGroupData.Append("[");
                    q3medianCostsGroupData.Append("[");
                    q3medianLOSGroupData.Append("[");

                    q4dischargeGroupData.Append("[");
                    q4meanChargeGroupData.Append("[");
                    q4meanCostsGroupData.Append("[");
                    q4meanLOSGroupData.Append("[");
                    q4medianChargesGroupData.Append("[");
                    q4medianCostsGroupData.Append("[");
                    q4medianLOSGroupData.Append("[");

                    foreach (var cat in (Dictionary<int, object>)hosp.Value)
                    {
                        if (cat.Key == QUARTERS) continue;
                        
                            Dictionary<int, object> combinedDetailsCat;
                            if (combinedDetails.ContainsKey(cat.Key))
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
                                if (combinedDetailsCat.ContainsKey(catGroup.Key))
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
                                var dc = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES];
                                AddDischarges(curentHospitalData, dc);
                                var ch = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES];
                                AddCharges(curentHospitalData, ch);
                                var co = (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS];
                                AddCosts(curentHospitalData, co);
                                var los = (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS];
                                AddLengthOfStays(curentHospitalData, los);

                            }

                            var dc2 = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES];
                            AddDischarges(combinedDetailsCatGroup, dc2);
                            var ch2 = (Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES];
                            AddCharges(combinedDetailsCatGroup, ch2);
                            var co2 = (Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS];
                            AddCosts(combinedDetailsCatGroup, co2);
                            var los2 = (Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS];
                            AddLengthOfStays(combinedDetailsCatGroup, los2);


                            if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum(d => d.Value) > _suppression)
                                {
                                    var co = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS]).SelectMany(rpt => rpt.Value).ToList();
                                    averageCost = co.Average();

                                    var ch = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES]).SelectMany(rpt => rpt.Value).ToList();
                                    var meanCharges = ch.Average();

                                    averageCost = averageCost ?? -1;
                                    meanCharges = meanCharges ?? -1;

                                    var los = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS]).SelectMany(rpt => rpt.Value).ToList();
                                    var meanLOS = los.Average();
                                    var medianLOS = los.Median();

                                    var medianCharges = ch.Median();
                                    var medianCosts = co.Median(averageCost);

                                    medianCharges = medianCharges ?? -1;
                                    medianCosts = medianCosts ?? -1;

                                    var discharges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum(d => d.Value);

                                    dischargeGroupData.Append(discharges + ",");
                                    meanChargeGroupData.Append(meanCharges + ",");
                                    meanCostsGroupData.Append(averageCost + ",");
                                    meanLOSGroupData.Append(meanLOS + ",");
                                    medianChargesGroupData.Append(medianCharges + ",");
                                    medianCostsGroupData.Append(medianCosts + ",");
                                    medianLOSGroupData.Append(medianLOS + ",");

                                    for (var q = 1; q <= 4; q++)
                                    {
                                            var costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])[q].Select(rpt => rpt).ToList();
                                            var averageCost2 = costs.Average();

                                            var charges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES])[q].Select(rpt => rpt).ToList();
                                            var meanCharges2 = charges.Average();

                                            averageCost2 = averageCost2 ?? -1;
                                            meanCharges2 = meanCharges2 ?? -1;

                                            var los1 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS])[q].Select(v => v).ToList();
                                            var meanLOS2 = los1 != null && los1.Any() ? los1.Average() : (double?)null;

                                            var medianLOS2 = los1 != null && los1.Any() ? los1.Median() : (double?)null;

                                            var medianCharges2 = charges.Median();
                                            var medianCosts2 = costs.Median(averageCost);

                                            medianCharges2 = medianCharges2 ?? -1;
                                            medianCosts2 = medianCosts2 ?? -1;

                                            var discharges2 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])[q].Sum(d => d);

                                            if (q == 1)
                                            {
                                                q1dischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0 ? discharges2.Value.ToString() : "null") + ",");
                                                q1meanChargeGroupData.Append(meanCharges2 + ",");
                                                q1meanCostsGroupData.Append(averageCost2 + ",");
                                                q1meanLOSGroupData.Append((meanLOS2.HasValue ? meanLOS2.Value.ToString() : "null") + ",");
                                                q1medianChargesGroupData.Append(medianCharges2 + ",");
                                                q1medianCostsGroupData.Append(medianCosts2 + ",");
                                                q1medianLOSGroupData.Append((medianLOS2.HasValue ? medianLOS2.ToString() : "null") + ",");
                                            }

                                            if (q == 2)
                                            {
                                                q2dischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0 ? discharges2.Value.ToString() : "null") + ",");
                                                q2meanChargeGroupData.Append(meanCharges2 + ",");
                                                q2meanCostsGroupData.Append(averageCost2 + ",");
                                                q2meanLOSGroupData.Append((meanLOS2.HasValue ? meanLOS2.Value.ToString() : "null") + ",");
                                                q2medianChargesGroupData.Append(medianCharges2 + ",");
                                                q2medianCostsGroupData.Append(medianCosts2 + ",");
                                                q2medianLOSGroupData.Append((medianLOS2.HasValue ? medianLOS2.ToString() : "null") + ",");
                                            }
                                            if (q == 3)
                                            {
                                                q3dischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0 ? discharges2.Value.ToString() : "null") + ",");
                                                q3meanChargeGroupData.Append(meanCharges2 + ",");
                                                q3meanCostsGroupData.Append(averageCost2 + ",");
                                                q3meanLOSGroupData.Append((meanLOS2.HasValue ? meanLOS2.Value.ToString() : "null") + ",");
                                                q3medianChargesGroupData.Append(medianCharges2 + ",");
                                                q3medianCostsGroupData.Append(medianCosts2 + ",");
                                                q3medianLOSGroupData.Append((medianLOS2.HasValue ? medianLOS2.ToString() : "null") + ",");
                                            }
                                            if (q == 4)
                                            {
                                                q4dischargeGroupData.Append((discharges2.HasValue && discharges2.Value > 0 ? discharges2.Value.ToString() : "null") + ",");
                                                q4meanChargeGroupData.Append(meanCharges2 + ",");
                                                q4meanCostsGroupData.Append(averageCost2 + ",");
                                                q4meanLOSGroupData.Append((meanLOS2.HasValue ? meanLOS2.Value.ToString() : "null") + ",");
                                                q4medianChargesGroupData.Append(medianCharges2 + ",");
                                                q4medianCostsGroupData.Append(medianCosts2 + ",");
                                                q4medianLOSGroupData.Append((medianLOS2.HasValue ? medianLOS2.ToString() : "null") + ",");
                                            }
                                    }
                                }
                                else
                                {
                                    dischargeGroupData.Append("-2,");
                                    meanChargeGroupData.Append("-2,");
                                    meanCostsGroupData.Append("-2,");
                                    meanLOSGroupData.Append("-2,");
                                    medianChargesGroupData.Append("-2,");
                                    medianCostsGroupData.Append("-2,");
                                    medianLOSGroupData.Append("-2,");

                                    q1dischargeGroupData.Append("-2,");
                                    q1meanChargeGroupData.Append("-2,");
                                    q1meanCostsGroupData.Append("-2,");
                                    q1meanLOSGroupData.Append("-2,");
                                    q1medianChargesGroupData.Append("-2,");
                                    q1medianCostsGroupData.Append("-2,");
                                    q1medianLOSGroupData.Append("-2,");

                                    q2dischargeGroupData.Append("-2,");
                                    q2meanChargeGroupData.Append("-2,");
                                    q2meanCostsGroupData.Append("-2,");
                                    q2meanLOSGroupData.Append("-2,");
                                    q2medianChargesGroupData.Append("-2,");
                                    q2medianCostsGroupData.Append("-2,");
                                    q2medianLOSGroupData.Append("-2,");

                                    q3dischargeGroupData.Append("-2,");
                                    q3meanChargeGroupData.Append("-2,");
                                    q3meanCostsGroupData.Append("-2,");
                                    q3meanLOSGroupData.Append("-2,");
                                    q3medianChargesGroupData.Append("-2,");
                                    q3medianCostsGroupData.Append("-2,");
                                    q3medianLOSGroupData.Append("-2,");

                                    q4dischargeGroupData.Append("-2,");
                                    q4meanChargeGroupData.Append("-2,");
                                    q4meanCostsGroupData.Append("-2,");
                                    q4meanLOSGroupData.Append("-2,");
                                    q4medianChargesGroupData.Append("-2,");
                                    q4medianCostsGroupData.Append("-2,");
                                    q4medianLOSGroupData.Append("-2,");
                                }
                            }

                        

                            GC.Collect();
                    }

                    idGroupData = new StringBuilder(idGroupData.ToString().SubStrBeforeLast(",") + "],");
                    catIdGroupData = new StringBuilder(catIdGroupData.ToString().SubStrBeforeLast(",") + "],");
                    catValueGroupData = new StringBuilder(catValueGroupData.ToString().SubStrBeforeLast(",") + "],");
                    dischargeGroupData = new StringBuilder(dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    meanChargeGroupData = new StringBuilder(meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    meanCostsGroupData = new StringBuilder(meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    meanLOSGroupData = new StringBuilder(meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                    medianChargesGroupData = new StringBuilder(medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                    medianCostsGroupData = new StringBuilder(medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    medianLOSGroupData = new StringBuilder(medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q1dischargeGroupData = new StringBuilder(q1dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1meanChargeGroupData = new StringBuilder(q1meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1meanCostsGroupData = new StringBuilder(q1meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1meanLOSGroupData = new StringBuilder(q1meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1medianChargesGroupData = new StringBuilder(q1medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1medianCostsGroupData = new StringBuilder(q1medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q1medianLOSGroupData = new StringBuilder(q1medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q2dischargeGroupData = new StringBuilder(q2dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2meanChargeGroupData = new StringBuilder(q2meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2meanCostsGroupData = new StringBuilder(q2meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2meanLOSGroupData = new StringBuilder(q2meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2medianChargesGroupData = new StringBuilder(q2medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2medianCostsGroupData = new StringBuilder(q2medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q2medianLOSGroupData = new StringBuilder(q2medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                    q3dischargeGroupData = new StringBuilder(q3dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3meanChargeGroupData = new StringBuilder(q3meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3meanCostsGroupData = new StringBuilder(q3meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3meanLOSGroupData = new StringBuilder(q3meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3medianChargesGroupData = new StringBuilder(q3medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3medianCostsGroupData = new StringBuilder(q3medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q3medianLOSGroupData = new StringBuilder(q3medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");


                    q4dischargeGroupData = new StringBuilder(q4dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4meanChargeGroupData = new StringBuilder(q4meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4meanCostsGroupData = new StringBuilder(q4meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4meanLOSGroupData = new StringBuilder(q4meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4medianChargesGroupData = new StringBuilder(q4medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4medianCostsGroupData = new StringBuilder(q4medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                    q4medianLOSGroupData = new StringBuilder(q4medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                    ((Dictionary<int, object>)hosp.Value).Clear();

                    GC.Collect();
                }

                #region Codes for LevelID=0 (Summary)
                idGroupData.Append("[");
                catIdGroupData.Append("[");
                catValueGroupData.Append("[");

                dischargeGroupData.Append("[");
                meanChargeGroupData.Append("[");
                meanCostsGroupData.Append("[");
                meanLOSGroupData.Append("[");
                medianChargesGroupData.Append("[");
                medianCostsGroupData.Append("[");
                medianLOSGroupData.Append("[");

                q1dischargeGroupData.Append("[");
                q1meanChargeGroupData.Append("[");
                q1meanCostsGroupData.Append("[");
                q1meanLOSGroupData.Append("[");
                q1medianChargesGroupData.Append("[");
                q1medianCostsGroupData.Append("[");
                q1medianLOSGroupData.Append("[");

                q2dischargeGroupData.Append("[");
                q2meanChargeGroupData.Append("[");
                q2meanCostsGroupData.Append("[");
                q2meanLOSGroupData.Append("[");
                q2medianChargesGroupData.Append("[");
                q2medianCostsGroupData.Append("[");
                q2medianLOSGroupData.Append("[");

                q3dischargeGroupData.Append("[");
                q3meanChargeGroupData.Append("[");
                q3meanCostsGroupData.Append("[");
                q3meanLOSGroupData.Append("[");
                q3medianChargesGroupData.Append("[");
                q3medianCostsGroupData.Append("[");
                q3medianLOSGroupData.Append("[");

                q4dischargeGroupData.Append("[");
                q4meanChargeGroupData.Append("[");
                q4meanCostsGroupData.Append("[");
                q4meanLOSGroupData.Append("[");
                q4medianChargesGroupData.Append("[");
                q4medianCostsGroupData.Append("[");
                q4medianLOSGroupData.Append("[");

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
                            List<float?> cos = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])
                                    .SelectMany(rpt => rpt.Value)
                                    .ToList();
                            averageCost = cos.Average();

                            List<int?> charg = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES])
                                    .SelectMany(rpt => rpt.Value)
                                    .ToList();
                            var meanCharges = charg.Average();

                            averageCost = averageCost ?? -1;
                            meanCharges = meanCharges ?? -1;

                            List<int> los2 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS])
                                    .SelectMany(rpt => rpt.Value)
                                    .ToList();
                            var meanLOS = los2.Average();
                            var medianLOS = los2.Median();

                            var medianCharges = charg.Median();
                            var medianCosts = cos.Median(averageCost);

                            medianCharges = medianCharges ?? -1;
                            medianCosts = medianCosts ?? -1;

                            var discharges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])
                                    .SelectMany(d => d.Value)
                                    .Sum();

                            dischargeGroupData.Append((discharges.HasValue ? discharges.Value.ToString() : "null") + ",");
                            meanChargeGroupData.Append(meanCharges + ",");
                            meanCostsGroupData.Append(averageCost + ",");
                            meanLOSGroupData.Append(meanLOS + ",");
                            medianChargesGroupData.Append(medianCharges + ",");
                            medianCostsGroupData.Append(medianCosts + ",");
                            medianLOSGroupData.Append(medianLOS + ",");


                            for (var q = 1; q <= 4; q++)
                            {

                                List<float?> cos1 = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)catGroup.Value)[COSTS])[q]
                                        .Select(rpt => rpt)
                                        .ToList();
                                var averageCost1 = cos1.Average();

                                List<int?> charg1 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[CHARGES])[q]
                                        .Select(rpt => rpt)
                                        .ToList();
                                var meanCharges1 = charg1.Average();

                                averageCost1 = averageCost1 ?? -1;
                                meanCharges1 = meanCharges1 ?? -1;

                                List<int> los3 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)catGroup.Value)[LENGTH_OF_STAYS])[q]
                                        .Select(rpt => rpt)
                                        .ToList();
                                var meanLOS1 = los3.Any() ? los3.Average().ToString() : "null";
                                var medianLOS1 = los3.Any() ? los3.Median().ToString() : "null";

                                var medianCharges1 = charg1.Median();
                                var medianCosts1 = cos1.Median(averageCost1);

                                medianCharges1 = medianCharges1 ?? -1;
                                medianCosts1 = medianCosts1 ?? -1;

                                var discharges1 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)catGroup.Value)[DISCHARGES])[q].Sum(d => d);

                                switch (q)
                                {
                                    case 1:
                                        q1dischargeGroupData.Append((discharges1 == null || discharges1 == 0 ? "null" : discharges1.ToString()) + ",");
                                        q1meanChargeGroupData.Append(meanCharges1 + ",");
                                        q1meanCostsGroupData.Append(averageCost1 + ",");
                                        q1meanLOSGroupData.Append(meanLOS1 + ",");
                                        q1medianChargesGroupData.Append(medianCharges1 + ",");
                                        q1medianCostsGroupData.Append(medianCosts1 + ",");
                                        q1medianLOSGroupData.Append(medianLOS1 + ",");
                                        break;
                                    case 2:
                                        q2dischargeGroupData.Append((discharges1 == null || discharges1 == 0 ? "null" : discharges1.ToString()) + ",");
                                        q2meanChargeGroupData.Append(meanCharges1 + ",");
                                        q2meanCostsGroupData.Append(averageCost1 + ",");
                                        q2meanLOSGroupData.Append(meanLOS1 + ",");
                                        q2medianChargesGroupData.Append(medianCharges1 + ",");
                                        q2medianCostsGroupData.Append(medianCosts1 + ",");
                                        q2medianLOSGroupData.Append(medianLOS1 + ",");
                                        break;
                                    case 3:
                                        q3dischargeGroupData.Append((discharges1 == null || discharges1 == 0 ? "null" : discharges1.ToString()) + ",");
                                        q3meanChargeGroupData.Append(meanCharges1 + ",");
                                        q3meanCostsGroupData.Append(averageCost1 + ",");
                                        q3meanLOSGroupData.Append(meanLOS1 + ",");
                                        q3medianChargesGroupData.Append(medianCharges1 + ",");
                                        q3medianCostsGroupData.Append(medianCosts1 + ",");
                                        q3medianLOSGroupData.Append(medianLOS1 + ",");
                                        break;
                                    case 4:
                                        q4dischargeGroupData.Append((discharges1 == null || discharges1 == 0 ? "null" : discharges1.ToString()) + ",");
                                        q4meanChargeGroupData.Append(meanCharges1 + ",");
                                        q4meanCostsGroupData.Append(averageCost1 + ",");
                                        q4meanLOSGroupData.Append(meanLOS1 + ",");
                                        q4medianChargesGroupData.Append(medianCharges1 + ",");
                                        q4medianCostsGroupData.Append(medianCosts1 + ",");
                                        q4medianLOSGroupData.Append(medianLOS1 + ",");
                                        break;


                                }


                            }
                        }
                        else
                        {
                            dischargeGroupData.Append("-2,");
                            meanChargeGroupData.Append("-2,");
                            meanCostsGroupData.Append("-2,");
                            meanLOSGroupData.Append("-2,");
                            medianChargesGroupData.Append("-2,");
                            medianCostsGroupData.Append("-2,");
                            medianLOSGroupData.Append("-2,");

                            q1dischargeGroupData.Append("-2,");
                            q1meanChargeGroupData.Append("-2,");
                            q1meanCostsGroupData.Append("-2,");
                            q1meanLOSGroupData.Append("-2,");
                            q1medianChargesGroupData.Append("-2,");
                            q1medianCostsGroupData.Append("-2,");
                            q1medianLOSGroupData.Append("-2,");

                            q2dischargeGroupData.Append("-2,");
                            q2meanChargeGroupData.Append("-2,");
                            q2meanCostsGroupData.Append("-2,");
                            q2meanLOSGroupData.Append("-2,");
                            q2medianChargesGroupData.Append("-2,");
                            q2medianCostsGroupData.Append("-2,");
                            q2medianLOSGroupData.Append("-2,");

                            q3dischargeGroupData.Append("-2,");
                            q3meanChargeGroupData.Append("-2,");
                            q3meanCostsGroupData.Append("-2,");
                            q3meanLOSGroupData.Append("-2,");
                            q3medianChargesGroupData.Append("-2,");
                            q3medianCostsGroupData.Append("-2,");
                            q3medianLOSGroupData.Append("-2,");

                            q4dischargeGroupData.Append("-2,");
                            q4meanChargeGroupData.Append("-2,");
                            q4meanCostsGroupData.Append("-2,");
                            q4meanLOSGroupData.Append("-2,");
                            q4medianChargesGroupData.Append("-2,");
                            q4medianCostsGroupData.Append("-2,");
                            q4medianLOSGroupData.Append("-2,");
                        }

                    }
                }

                idGroupData = new StringBuilder(idGroupData.ToString().SubStrBeforeLast(",") + "],");
                catIdGroupData = new StringBuilder(catIdGroupData.ToString().SubStrBeforeLast(",") + "],");
                catValueGroupData = new StringBuilder(catValueGroupData.ToString().SubStrBeforeLast(",") + "],");
                dischargeGroupData = new StringBuilder(dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                meanChargeGroupData = new StringBuilder(meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                meanCostsGroupData = new StringBuilder(meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                meanLOSGroupData = new StringBuilder(meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                medianChargesGroupData = new StringBuilder(medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                medianCostsGroupData = new StringBuilder(medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                medianLOSGroupData = new StringBuilder(medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                q1dischargeGroupData = new StringBuilder(q1dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1meanChargeGroupData = new StringBuilder(q1meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1meanCostsGroupData = new StringBuilder(q1meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1meanLOSGroupData = new StringBuilder(q1meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1medianChargesGroupData = new StringBuilder(q1medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1medianCostsGroupData = new StringBuilder(q1medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q1medianLOSGroupData = new StringBuilder(q1medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                q2dischargeGroupData = new StringBuilder(q2dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2meanChargeGroupData = new StringBuilder(q2meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2meanCostsGroupData = new StringBuilder(q2meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2meanLOSGroupData = new StringBuilder(q2meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2medianChargesGroupData = new StringBuilder(q2medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2medianCostsGroupData = new StringBuilder(q2medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q2medianLOSGroupData = new StringBuilder(q2medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                q3dischargeGroupData = new StringBuilder(q3dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3meanChargeGroupData = new StringBuilder(q3meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3meanCostsGroupData = new StringBuilder(q3meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3meanLOSGroupData = new StringBuilder(q3meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3medianChargesGroupData = new StringBuilder(q3medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3medianCostsGroupData = new StringBuilder(q3medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q3medianLOSGroupData = new StringBuilder(q3medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");


                q4dischargeGroupData = new StringBuilder(q4dischargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4meanChargeGroupData = new StringBuilder(q4meanChargeGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4meanCostsGroupData = new StringBuilder(q4meanCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4meanLOSGroupData = new StringBuilder(q4meanLOSGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4medianChargesGroupData = new StringBuilder(q4medianChargesGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4medianCostsGroupData = new StringBuilder(q4medianCostsGroupData.ToString().SubStrBeforeLast(",") + "],");
                q4medianLOSGroupData = new StringBuilder(q4medianLOSGroupData.ToString().SubStrBeforeLast(",") + "],");

                combinedDetails.Clear();
                GC.Collect();

                #endregion

                fileContent += "\"LevelID\":[" + levelIdGroupData.ToString().SubStrBeforeLast(",") + ", 0]," +
                               "\"ID\":[" + idGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"CatID\":[" + catIdGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"CatVal\":[" + catValueGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Discharges\":[" + dischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MeanCharges\":[" + meanChargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MeanCosts\":[" + meanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MeanLOS\":[" + meanLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MedianCharges\":[" + medianChargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MedianCosts\":[" + medianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"MedianLOS\":[" + medianLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                    // Q1
                               "\"Q1_Discharges\":[" + q1dischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MeanCharges\":[" + q1meanChargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MeanCosts\":[" + q1meanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MeanLOS\":[" + q1meanLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MedianCharges\":[" + q1medianChargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MedianCosts\":[" + q1medianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q1_MedianLOS\":[" + q1medianLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                    // Q2
                               "\"Q2_Discharges\":[" + q2dischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MeanCharges\":[" + q2meanChargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MeanCosts\":[" + q2meanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MeanLOS\":[" + q2meanLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MedianCharges\":[" + q2medianChargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MedianCosts\":[" + q2medianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q2_MedianLOS\":[" + q2medianLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                    // Q3
                               "\"Q3_Discharges\":[" + q3dischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MeanCharges\":[" + q3meanChargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MeanCosts\":[" + q3meanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MeanLOS\":[" + q3meanLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MedianCharges\":[" + q3medianChargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MedianCosts\":[" + q3medianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q3_MedianLOS\":[" + q3medianLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                    // Q4
                               "\"Q4_Discharges\":[" + q4dischargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MeanCharges\":[" + q4meanChargeGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MeanCosts\":[" + q4meanCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MeanLOS\":[" + q4meanLOSGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MedianCharges\":[" + q4medianChargesGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MedianCosts\":[" + q4medianCostsGroupData.ToString().SubStrBeforeLast(",") + "]," +
                               "\"Q4_MedianLOS\":[" + q4medianLOSGroupData.ToString().SubStrBeforeLast(",") + "]" +
                           
                               Environment.NewLine + "}]};";

                byte[] result = acii.GetBytes(fileContent);
                fs.Write(result, 0, result.Length);
                processed++;
                progress = processed * 100 / fileCount;
                if (_logMod == 0)
                    Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress,
                        PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2),
                        location);

            }
            dic.Clear();
            GC.Collect();


            processed++;

            progress = processed * 100 / fileCount;
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);

            using (var fsSummary = File.Create(targetDir + @"\summary.js"))
            {

                string nationalData = "$.monahrq.inpatientutilization = {\"NationalData\":[{" + _ipNationalTotals[utilId][utilValue] + "}],";

                string tableData = "\"TableData\" : [";
                //float? MeanCosts;

                var totalDischarges = new List<KeyValuePair<int, List<int?>>>();
                var totalCharges = new List<KeyValuePair<int, List<int?>>>();
                var totalCosts = new List<KeyValuePair<int, List<float?>>>();
                var totalLos = new List<KeyValuePair<int, List<int>>>();

                foreach (var hosp in summary)
                {
                    tableData += "{";

                    switch (folderName.Substring(0, folderName.LastIndexOf("_", StringComparison.InvariantCulture)))
                    {
                        case "DRG":
                        case "MDC":
                        case "CCS":
                        case "PRCCS":
                            tableData += "\"HospitalID\":" + hosp.Key +
                                             ",\"RegionID\":" + _hospitalRegion[hosp.Key].RegionId +
                                             ",\"CountyID\":" + _hospitalRegion[hosp.Key].CountyId +
                                             ",\"Zip\":" + _hospitalRegion[hosp.Key].Zip +
                                             ",\"HospitalType\":" + _hospitalCategory[hosp.Key][0];
                            break;
                        default:
                            tableData += "\"ID\":" + hosp.Key;
                            break;
                    }

                    if (((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum(d => d.Value) > _suppression)
                    {
                        List<float?> costs = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]).SelectMany(rpt => rpt.Value).ToList();
                        averageCost = costs.Average();
                        var medianCosts = costs.Median(averageCost);

                        averageCost = averageCost ?? -1;
                        medianCosts = medianCosts ?? -1;

                        var discharges = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).SelectMany(d => d.Value).Sum(d => d);
                        discharges = discharges ?? 0;

                        List<int?> charges1 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[CHARGES]).SelectMany(rpt => rpt.Value).ToList();
                        var meanCharges = charges1.Average();
                        var medianCharges = charges1.Median();

                        meanCharges = meanCharges ?? -1;
                        medianCharges = medianCharges ?? -1;

                        List<int> los1 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[LENGTH_OF_STAYS]).SelectMany(rpt => rpt.Value).ToList();
                        var meanLOS = los1.Average();
                        var medianLOS = los1.Median();

                        tableData += ",\"Discharges\" :" + ((discharges == null || discharges == 0) ? "null" : discharges.Value.ToString()) + ",\"MeanCharges\" :" + meanCharges + ",\"MeanCosts\" :" + averageCost + ",\"MeanLOS\" :" + meanLOS +
                                     ",\"MedianCharges\" :" + medianCharges + ",\"MedianCosts\" :" + medianCosts + ",\"MedianLOS\" :" + medianLOS;

                        for (var q = 1; q <= 4; q++)
                        {
                            //if (q.In(_reportQuarters.ToList()))
                            //{
                                List<float?> cost3 = ((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS])[q].Select(c => c).ToList();
                                var averageCost2 = cost3.Average();
                                var medianCosts2 = cost3.Median(averageCost);

                                averageCost2 = averageCost2 ?? -1;
                                medianCosts2 = medianCosts2 ?? -1;

                                List<int?> charges3 = ((Dictionary <int,List<int?>>)((Dictionary<int, object>)hosp.Value)[CHARGES])[q].Select(c => c).ToList();
                                var meanCharges2 = charges3.Average();
                                var medianCharges2 = charges3.Median();

                                meanCharges2 = meanCharges2 ?? -1;
                                medianCharges2 = medianCharges2 ?? -1;

                                List<int> los3 = ((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[LENGTH_OF_STAYS])[q].Select(c => c).ToList();
                                var meanLOS2 = los3.Any() ? los3.Average() : -1;
                                var medianLOS2 = los3.Any() ? los3.Median() : -1;

                                var discharges2 = ((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES])[q].Sum(d => d);
                                discharges2 = discharges2 ?? 0;

                                tableData += ",\"Q" + q + "_Discharges\" :" + discharges2 +
                                             ",\"Q" + q + "_MeanCharges\" :" + meanCharges2 +
                                             ",\"Q" + q + "_MeanCosts\" :" + averageCost2 +
                                             ",\"Q" + q + "_MeanLOS\" :" + meanLOS2 +
                                             ",\"Q" + q + "_MedianCharges\" :" + medianCharges2 +
                                             ",\"Q" + q + "_MedianCosts\" :" + medianCosts2 +
                                             ",\"Q" + q + "_MedianLOS\" :" + medianLOS2;
                            //}
                            //else
                            //{
                            //    tableData += ",\"Q" + q + "_Discharges\":-2,\"Q" + q + "_MeanCharges\":-2,\"Q" + q + "_MeanCosts\":-2,\"Q" + q + "_MeanLOS\":-2,\"Q" + q + "_MedianCharges\":-2,\"Q" + q + "_MedianCosts\":-2,\"Q" + q + "_MedianLOS\":-2";
                            //}
                        }
                    }
                    else
                    {
                        tableData += ",\"Discharges\":-2,\"MeanCharges\":-2,\"MeanCosts\":-2,\"MeanLOS\":-2,\"MedianCharges\":-2,\"MedianCosts\":-2,\"MedianLOS\":-2" +
                                     ",\"Q1_Discharges\":-2,\"Q1_MeanCharges\":-2,\"Q1_MeanCosts\":-2,\"Q1_MeanLOS\":-2,\"Q1_MedianCharges\":-2,\"Q1_MedianCosts\":-2,\"Q1_MedianLOS\":-2" +
                                     ",\"Q2_Discharges\":-2,\"Q2_MeanCharges\":-2,\"Q2_MeanCosts\":-2,\"Q2_MeanLOS\":-2,\"Q2_MedianCharges\":-2,\"Q2_MedianCosts\":-2,\"Q2_MedianLOS\":-2" +
                                     ",\"Q3_Discharges\":-2,\"Q3_MeanCharges\":-2,\"Q3_MeanCosts\":-2,\"Q3_MeanLOS\":-2,\"Q3_MedianCharges\":-2,\"Q3_MedianCosts\":-2,\"Q3_MedianLOS\":-2" +
                                     ",\"Q4_Discharges\":-2,\"Q4_MeanCharges\":-2,\"Q4_MeanCosts\":-2,\"Q4_MeanLOS\":-2,\"Q4_MedianCharges\":-2,\"Q4_MedianCosts\":-2,\"Q4_MedianLOS\":-2";
                    }



                    tableData += "},";

                    totalDischarges.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[DISCHARGES]).ToList());
                    totalCharges.AddRange(((Dictionary<int, List<int?>>)((Dictionary<int, object>)hosp.Value)[CHARGES]).ToList());
                    totalCosts.AddRange(((Dictionary<int, List<float?>>)((Dictionary<int, object>)hosp.Value)[COSTS]).ToList());
                    totalLos.AddRange(((Dictionary<int, List<int>>)((Dictionary<int, object>)hosp.Value)[LENGTH_OF_STAYS]).ToList());
                }
                tableData += "]};";

                var totalChargesFinal = totalCharges.SelectMany(d => d.Value).ToList();
                var totalMeanCharges = totalChargesFinal.Count > 0 ? totalChargesFinal.ToList().Average() ?? -1 : -1;

                var totalCostsFinal = totalCosts.SelectMany(d => d.Value).ToList();
                var totalMeanCosts = totalCostsFinal.Count > 0 ? totalCostsFinal.Average() ?? -1 : -1;

                var totalLOSFinal = totalLos.SelectMany(d => d.Value).ToList();
                var totalMeanLOS = totalLOSFinal.Count > 0 ? totalLOSFinal.Average() : -1;

                var totalMedianCharges = totalChargesFinal.Count > 0 ? totalChargesFinal.Median() ?? -1 : -1;
                var totalMedianCosts = totalCostsFinal.Count > 0 ? totalCostsFinal.Median(totalCostsFinal.Average()) ?? -1 : -1;
                var totalMedianLOS = totalLOSFinal.Count > 0 ? totalLOSFinal.Median() : -1;

                var totalDischargesFinal = totalDischarges.SelectMany(d => d.Value).ToList();
                var totalDischargesString = totalDischargesFinal.Count > 0 ? totalDischargesFinal.Sum().ToString() : "null";

                // Q1
                var totalChargesQ1 = totalCharges.Where(kvp => kvp.Key == 1).SelectMany(d => d.Value).ToList();
                var totalMeanChargesQ1 = totalChargesQ1.Any() ? totalChargesQ1.Average() ?? -1 : -1;

                var totalCostsQ1 = totalCosts.Where(kvp => kvp.Key == 1).SelectMany(d => d.Value).ToList();
                var totalMeanCostsQ1 = totalCostsQ1.Any() ? totalCostsQ1.Average() ?? -1 : -1;

                var totalLOSQ1 = totalLos.Where(kvp => kvp.Key == 1).SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ1 = totalLOSQ1.Any() ? totalLOSQ1.Average() : -1;

                var totalMedianChargesQ1 = totalChargesQ1.Any() ? totalChargesQ1.Median() ?? -1 : -1;
                var totalMedianCostsQ1 = totalCostsQ1.Any() ? totalCostsQ1.Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOSQ1 = totalLOSQ1.Any() ? totalLOSQ1.Median() : -1;

                var totalDischargesQ1 = totalDischarges.Where(kvp => kvp.Key == 1).SelectMany(d => d.Value).ToList();
                var totalDischargesStringQ1 = totalDischargesQ1.Any() ? totalDischargesQ1.Sum(d => d ?? 1).ToString() : "null";

                // Q2
                var totalChargesQ2 = totalCharges.Where(d => d.Key == 2).SelectMany(d => d.Value).ToList();
                var totalMeanChargesQ2 = totalChargesQ2.Any() ? totalChargesQ2.Average() ?? -1 : -1;

                var totalCostsQ2 = totalCosts.Where(d => d.Key == 2).SelectMany(d => d.Value).ToList();
                var totalMeanCostsQ2 = totalCostsQ2.Any() ? totalCostsQ2.Average() ?? -1 : -1;

                var totalLOSQ2 = totalLos.Where(d => d.Key == 2).SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ2 = totalLOSQ2.Any() ? totalLOSQ2.Average() : -1;

                var totalMedianChargesQ2 = totalChargesQ2.Any() ? totalChargesQ2.Median() ?? -1 : -1;
                var totalMedianCostsQ2 = totalCostsQ2.Any() ? totalCostsQ2.Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOSQ2 = totalLOSQ2.Any() ? totalLOSQ2.Median() : -1;

                var totalDischargesQ2 = totalDischarges.Where(d => d.Key == 2).SelectMany(d => d.Value).ToList();
                var totalDischargesStringQ2 = totalDischargesQ2.Any() ? totalDischargesQ2.Sum(d => d ?? 1).ToString() : "null";

                // Q3
                var totalChargesQ3 = totalCharges.Where(d => d.Key == 3).SelectMany(d => d.Value).ToList();
                var totalMeanChargesQ3 = totalChargesQ3.Any() ? totalChargesQ3.Average() ?? -1 : -1;

                var totalCostsQ3 = totalCosts.Where(d => d.Key == 3).SelectMany(d => d.Value).ToList();
                var totalMeanCostsQ3 = totalCostsQ3.Any() ? totalCostsQ3.Average() ?? -1 : -1;

                var totalLOSQ3 = totalLos.Where(d => d.Key == 3).SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ3 = totalLOSQ3.Any() ? totalLOSQ3.Average() : -1;

                var totalMedianChargesQ3 = totalChargesQ3.Any() ? totalChargesQ3.Median() ?? -1 : -1;
                var totalMedianCostsQ3 = totalCostsQ3.Any() ? totalCostsQ3.Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOSQ3 = totalLOSQ3.Any() ? totalLOSQ3.Median() : -1;

                var totalDischargesQ3 = totalDischarges.Where(d => d.Key == 3).SelectMany(d => d.Value).ToList();
                var totalDischargesStringQ3 = totalDischargesQ3.Any() ? totalDischargesQ3.Sum(d => d ?? 1).ToString() : "null";

                // Q4
                var totalChargesQ4 = totalCharges.Where(d => d.Key == 4).SelectMany(d => d.Value).ToList();
                var totalMeanChargesQ4 = totalChargesQ4.Any() ? totalChargesQ4.Average() ?? -1 : -1;

                var totalCostsQ4 = totalCosts.Where(d => d.Key == 4).SelectMany(d => d.Value).ToList();
                var totalMeanCostsQ4 = totalCostsQ4.Any() ? totalCostsQ4.Average() ?? -1 : -1;

                var totalLOSQ4 = totalLos.Where(d => d.Key == 4).SelectMany(d => d.Value).ToList();
                var totalMeanLOSQ4 = totalLOSQ4.Any() ? totalLOSQ4.Average() : -1;

                var totalMedianChargesQ4 = totalChargesQ4.Any() ? totalChargesQ4.Median() ?? -1 : -1;
                var totalMedianCostsQ4 = totalCostsQ4.Any() ? totalCostsQ4.Median(totalMeanCosts) ?? -1 : -1;
                var totalMedianLOSQ4 = totalLOSQ4.Any() ? totalLOSQ4.Median() : -1;

                var totalDischargesQ4 = totalDischarges.Where(d => d.Key == 4).SelectMany(d => d.Value).ToList();
                var totalDischargesStringQ4 = totalDischargesQ4.Any() ? totalDischargesQ4.Sum(d => d ?? 1).ToString() : "null";

                const string totalData = "\"TotalData\" :[{{\"Discharges\":{0},\"MeanCharges\":{1},\"MeanCosts\":{2},\"MeanLOS\":{3},\"MedianCharges\":{4},\"MedianCosts\":{5},\"MedianLOS\":{6}," +
                                        "\"Q1_Discharges\":{7},\"Q1_MeanCharges\":{8},\"Q1_MeanCosts\":{9},\"Q1_MeanLOS\":{10},\"Q1_MedianCharges\":{11},\"Q1_MedianCosts\":{12},\"Q1_MedianLOS\":{13}," +
                                        "\"Q2_Discharges\":{14},\"Q2_MeanCharges\":{15},\"Q2_MeanCosts\":{16},\"Q2_MeanLOS\":{17},\"Q2_MedianCharges\":{18},\"Q2_MedianCosts\":{19},\"Q2_MedianLOS\":{20}," +
                                        "\"Q3_Discharges\":{21},\"Q3_MeanCharges\":{22},\"Q3_MeanCosts\":{23},\"Q3_MeanLOS\":{24},\"Q3_MedianCharges\":{25},\"Q3_MedianCosts\":{26},\"Q3_MedianLOS\":{27}," +
                                        "\"Q4_Discharges\":{28},\"Q4_MeanCharges\":{29},\"Q4_MeanCosts\":{30},\"Q4_MeanLOS\":{31},\"Q4_MedianCharges\":{32},\"Q4_MedianCosts\":{33},\"Q4_MedianLOS\":{34}}}],";

                byte[] summaryResult = acii.GetBytes(nationalData +
                    string.Format(totalData,
                    totalDischargesString,
                    totalMeanCharges,
                    totalMeanCosts,
                    totalMeanLOS,
                    totalMedianCharges,
                    totalMedianCosts,
                    totalMedianLOS,

                    totalDischargesStringQ1,
                    totalMeanChargesQ1,
                    totalMeanCostsQ1,
                    totalMeanLOSQ1,
                    totalMedianChargesQ1,
                    totalMedianCostsQ1,
                    totalMedianLOSQ1,

                    totalDischargesStringQ2,
                    totalMeanChargesQ2,
                    totalMeanCostsQ2,
                    totalMeanLOSQ2,
                    totalMedianChargesQ2,
                    totalMedianCostsQ2,
                    totalMedianLOSQ2,

                    totalDischargesStringQ3,
                    totalMeanChargesQ3,
                    totalMeanCostsQ3,
                    totalMeanLOSQ3,
                    totalMedianChargesQ3,
                    totalMedianCostsQ3,
                    totalMedianLOSQ3,

                    totalDischargesStringQ4,
                    totalMeanChargesQ4,
                    totalMeanCostsQ4,
                    totalMeanLOSQ4,
                    totalMedianChargesQ4,
                    totalMedianCostsQ4,
                    totalMedianLOSQ4) + tableData);

                fsSummary.Write(summaryResult, 0, summaryResult.Length);
                processed++;
            }//end of using fsSummary
            
            progress = processed * 100 / fileCount;
            if (progress > 100)
                progress = 100;

#if DEBUG
            if (_logMod == 0)
                Console.Write("\r{4,-30} {0,4}/{1,-4} [{2,3}%] [{3}] ", processed, fileCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2), location);
#endif
            if (_logMod == 0)
                LogRuntime();

        }

        /// <summary>
        /// Initializes the hospital category.
        /// </summary>
        private static void InitializeHospitalCategory()
        {
            string hospitalCategorySQL = ConfigurationManager.AppSettings["HospitalCategory_SQL"].Replace("[websiteID]", _websiteId);
            _hospitalCategory = new Dictionary<int, List<int>>();
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = hospitalCategorySQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();

            while (dataRead.Read())
            {
                var hospid = dataRead.GetInt32(0);
                var categoryid = dataRead.GetInt32(1);

                if (!_hospitalCategory.Keys.Contains(hospid))
                    _hospitalCategory[hospid] = new List<int>();

                _hospitalCategory[hospid].Add(categoryid);
            }
        }

        /// <summary>
        /// Initializes the hospital region.
        /// </summary>
        private static void InitializeHospitalRegion()
        {
            string hospitalRegionSQL = ConfigurationManager.AppSettings["HospitalRegion_SQL"].Replace("[websiteID]", _websiteId).Replace("[RegionType]", _regionType);
            _hospitalRegion = new Dictionary<int, HospitalAddressData>();
            SqlConnection conn = new SqlConnection(_mainConnectionString);
            SqlCommand command = conn.CreateCommand();
            conn.Open();

            command.CommandText = hospitalRegionSQL;
            command.CommandTimeout = _timeout;
            SqlDataReader dataRead = command.ExecuteReader();
            int hospid;
            //int regionid;

            while (dataRead.Read())
            {
                hospid = dataRead.GetInt32(0);
                HospitalAddressData hospData1 = new HospitalAddressData();
                hospData1.HospitalId = hospid.ToString();
                //hospData1.HospitalId = dataRead.GetInt32(0).ToString();
                hospData1.RegionId = dataRead.GetInt32(1).ToString();
                hospData1.CountyFIPS = dataRead.IsDBNull(2) ? "-1" : dataRead.GetString(2);
                hospData1.Zip = dataRead.GetString(3);
                hospData1.CountyId = dataRead.GetInt32(4).ToString();
                _hospitalRegion[hospid] = hospData1;
            }
        }

        /// <summary>
        /// Generates the dimension.
        /// </summary>
        /// <param name="dimention">The dimention.</param>
        /// <param name="dimentionSQL">The dimention SQL.</param>
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

            var DRG_DRG_details = new Dictionary<int, object>();

            var DRG_Hospital_details = new Dictionary<int, object>();
            Dictionary<int, object> drgHospitalData;

            var DRG_County_details = new Dictionary<int, object>();
            Dictionary<int, object> drgCountyData;

            var DRG_Region_details = new Dictionary<int, object>();
            Dictionary<int, object> drgRegionData;

            var DRG_Category_details = new Dictionary<int, object>();
            Dictionary<int, object> drgCategoryData;

            int drGid;
            Dictionary<int, object> drgData;

            int hospitalid;
            Dictionary<int, object> hospitalData;

            int Ageid;
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

            int countyid;

            int dischargeQuarter;

            int? totalCharge;
            float? totalCost;
            int lengthOfStay;

            //Dictionary<int, object> HospitalMetaData = new Dictionary<int, object>();
            if (_logMod == 0)
                LogRuntime();

#if DEBUG
            Console.WriteLine("memory manipulation");
#endif

            var DRG0 = GetAndInitIfNeeded(DRG_DRG_details, 0);
            var Hospital0 = GetAndInitIfNeeded(DRG_Hospital_details, 0);
            var County0 = GetAndInitIfNeeded(DRG_County_details, 0);
            var Region0 = GetAndInitIfNeeded(DRG_Region_details, 0);
            var Category0 = GetAndInitIfNeeded(DRG_Category_details, 0);

            dataRead.Read();
            var rowCount = dataRead.GetInt32(0);
            int processed = 0;

            const int memoryFlushPoint = 1000; // original: 500; | works: 1000 | trial: 750
            const int rowsPerMemoryCheck = 500; // original: 100; | works: 500 | trial: 250
            const int maxPatchSize = 5000; // original: 1000; | works: 5000 | trial: 2500
            _flushCount = 0;

             dataRead.NextResult();
            //var rowCount = dataRead.
            while (dataRead.Read())
            {
                hospitalid = dataRead.GetInt32(0);
                countyid = dataRead.IsDBNull(1) ? -1 : dataRead.GetInt32(1); // int.Parse(dataRead.GetString(1));
                drGid = dataRead.GetInt32(2);

                Ageid = dataRead.GetInt32(3);
                raceid = dataRead.GetInt32(4);
                sexid = dataRead.GetInt32(5);
                primaryPayerid = dataRead.GetInt32(6);
                lengthOfStay = dataRead.GetInt32(7);
                totalCharge = dataRead.IsDBNull(8) ? (int?)null : dataRead.GetInt32(8);
                dischargeQuarter = dataRead.IsDBNull(10) ? 0 : dataRead.GetInt32(10);

                totalCost = null;
                try
                {
                    if (dataRead.IsDBNull(9))
                    {
                        totalCost = null;
                    }
                    else
                    {
                        if (dataRead.GetValue(9).GetType() == typeof(double))
                        {
                            totalCost = (float)(dataRead.GetDouble(9));
                        }
                        else
                        {
                            totalCost = dataRead.GetFloat(9);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                drgData = GetAndInitIfNeeded(DRG_DRG_details, drGid);
                ArrangeData(drgData, hospitalid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                DRG0 = GetAndInitIfNeeded(DRG_DRG_details, 0);
                ArrangeData(DRG0, hospitalid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                drgHospitalData = GetAndInitIfNeeded(DRG_Hospital_details, hospitalid);
                ArrangeData(drgHospitalData, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                Hospital0 = GetAndInitIfNeeded(DRG_Hospital_details, 0);
                ArrangeData(Hospital0, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                drgCountyData = GetAndInitIfNeeded(DRG_County_details, countyid);
                ArrangeData(drgCountyData, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                County0 = GetAndInitIfNeeded(DRG_County_details, 0);
                ArrangeData(County0, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                drgRegionData = GetAndInitIfNeeded(DRG_Region_details, int.Parse(_hospitalRegion[hospitalid].RegionId));
                ArrangeData(drgRegionData, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                Region0 = GetAndInitIfNeeded(DRG_Region_details, 0);
                ArrangeData(Region0, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);

                if (!_hospitalCategory.ContainsKey(hospitalid)) continue;

                foreach (var catid in _hospitalCategory[hospitalid])
                {
                    drgCategoryData = GetAndInitIfNeeded(DRG_Category_details, catid);
                    ArrangeData(drgCategoryData, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                    Category0 = GetAndInitIfNeeded(DRG_Category_details, 0);
                    ArrangeData(Category0, drGid, Ageid, raceid, sexid, primaryPayerid, totalCharge, totalCost, lengthOfStay, dischargeQuarter, out hospitalData, out ageData, out ageGroupData, out raceData, out raceGroupData, out sexData, out sexGroupData, out primaryPayerData, out primaryPayerGroupData);
                }

                processed++;
                var progress = processed * 100 / rowCount;
                if (_logMod == 0)
                    Console.Write("\r {0}/{1} [{2}%] [{3}] ", processed, rowCount, progress, PROGRESS_BAR_DONE.Substring(50 - (progress / 2)) + PROGRESS_BAR_REMAINING.Substring(progress / 2));

                //------------ MemoryCheck -------------//
                if (processed % rowsPerMemoryCheck == 0)
                {
                    if ( processed % maxPatchSize == 0)
                    {
                        flushMemory(dimention, acii, tempDir, DRG_DRG_details, DRG_Hospital_details, DRG_County_details, DRG_Region_details, DRG_Category_details, DRG0, Hospital0, County0, Region0, Category0);
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
                            flushMemory(dimention, acii, tempDir, DRG_DRG_details, DRG_Hospital_details, DRG_County_details, DRG_Region_details, DRG_Category_details, DRG0, Hospital0, County0, Region0, Category0);
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
            CreatePartialFiles(tempDir, dimention, dimention, DRG_DRG_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_DRG_details.Clear();
            DRG0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Hospital", "Hospital", DRG_Hospital_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_Hospital_details.Clear();
            Hospital0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "County", "County", DRG_County_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_County_details.Clear();
            County0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Region", "Region", DRG_Region_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_Region_details.Clear();
            Region0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "HospitalType", "HospitalType", DRG_Category_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_Category_details.Clear();
            Category0.Clear();
            GC.Collect();

            conn.Close();
            //Console.WriteLine("creating files");

            //CreateFiles(rootDir, dimention, dimention, DRG_DRG_details, progressBarRemaining, progressBarDone, acii);
            //CreateFiles(rootDir, "Hospital", "Hospital", DRG_Hospital_details, progressBarRemaining, progressBarDone, acii);
            //CreateFiles(rootDir, "County", "County", DRG_County_details, progressBarRemaining, progressBarDone, acii);
            //CreateFiles(rootDir, "Region", "Region", DRG_Region_details, progressBarRemaining, progressBarDone, acii);
            //CreateFiles(rootDir, "HospitalType", "HospitalType", DRG_Category_details, progressBarRemaining, progressBarDone, acii);
        }

        /// <summary>
        /// Flushes the memory.
        /// </summary>
        /// <param name="dimention">The dimention.</param>
        /// <param name="acii">The acii.</param>
        /// <param name="tempDir">The temporary dir.</param>
        /// <param name="DRG_DRG_details">The DRG DRG details.</param>
        /// <param name="DRG_Hospital_details">The DRG hospital details.</param>
        /// <param name="DRG_County_details">The DRG county details.</param>
        /// <param name="DRG_Region_details">The DRG region details.</param>
        /// <param name="DRG_Category_details">The DRG category details.</param>
        /// <param name="DRG0">The dr g0.</param>
        /// <param name="Hospital0">The hospital0.</param>
        /// <param name="County0">The county0.</param>
        /// <param name="Region0">The region0.</param>
        /// <param name="Category0">The category0.</param>
        private static void flushMemory(string dimention, ASCIIEncoding acii, string tempDir, Dictionary<int, object> DRG_DRG_details, Dictionary<int, object> DRG_Hospital_details, Dictionary<int, object> DRG_County_details, Dictionary<int, object> DRG_Region_details, Dictionary<int, object> DRG_Category_details, Dictionary<int, object> DRG0, Dictionary<int, object> Hospital0, Dictionary<int, object> County0, Dictionary<int, object> Region0, Dictionary<int, object> Category0)
        {
            //-------- flush ---------//
            if (_logMod == 0)
                LogRuntime();
#if DEBUG
            if (_logMod == 0)
                Console.WriteLine("\t..........flushing memory............");
#endif
            GC.Collect();

            CreatePartialFiles(tempDir, dimention, dimention, DRG_DRG_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_DRG_details.Clear();
            DRG0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Hospital", "Hospital", DRG_Hospital_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_Hospital_details.Clear();
            Hospital0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "County", "County", DRG_County_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_County_details.Clear();
            County0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "Region", "Region", DRG_Region_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_Region_details.Clear();
            Region0.Clear();
            GC.Collect();

            CreatePartialFiles(tempDir, "HospitalType", "HospitalType", DRG_Category_details, PROGRESS_BAR_REMAINING, PROGRESS_BAR_DONE, acii);
            DRG_Category_details.Clear();
            Category0.Clear();
            GC.Collect();

            _flushCount++;
        }

        /// <summary>
        /// Creates the partial files.
        /// </summary>
        /// <param name="rootDir">The root dir.</param>
        /// <param name="subDir1">The sub dir1.</param>
        /// <param name="subDir2">The sub dir2.</param>
        /// <param name="drgDRGDetails">The DRG DRG details.</param>
        /// <param name="progressBarRemaining">The progress bar remaining.</param>
        /// <param name="progressBarDone">The progress bar done.</param>
        /// <param name="acii">The acii.</param>
        private static void CreatePartialFiles(string rootDir, string subDir1, string subDir2, Dictionary<int, object> drgDRGDetails, string progressBarRemaining, string progressBarDone, ASCIIEncoding acii)
        {
            var location = rootDir.Substring(rootDir.LastIndexOf("\\") + 1) + "\\" + subDir1;
            int fileCount = drgDRGDetails.Count;

            int processed = 0;
            foreach (var drg in drgDRGDetails)
            {
                try
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

                    var progress = processed * 100 / fileCount;
                    if (_logMod == 0)
                        Console.Write("\r {4,-20}{0,4}/{1,-4} [{2,3}%] [{3}] \t", processed, fileCount, progress, progressBarDone.Substring(50 - (progress / 2)) + progressBarRemaining.Substring(progress / 2), location);
                }
                catch (Exception exc)
                {
                    Console.Write("Error occurred while trying to create temporary file at the following path: " + rootDir + "\\" + subDir1 + "\\" + subDir2 + "_" + drg.Key);
                    throw exc;
                }
            }

            drgDRGDetails.Clear();
            GC.Collect();
            if (_logMod == 0)
                LogRuntime();
        }

        /// <summary>
        /// Arranges the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="hospitalid">The hospitalid.</param>
        /// <param name="ageid">The ageid.</param>
        /// <param name="raceid">The raceid.</param>
        /// <param name="sexid">The sexid.</param>
        /// <param name="primaryPayerid">The primary payerid.</param>
        /// <param name="totalCharge">The total charge.</param>
        /// <param name="totalCost">The total cost.</param>
        /// <param name="lengthOfStay">The length of stay.</param>
        /// <param name="quarter">The quarter.</param>
        /// <param name="HospitalData">The hospital data.</param>
        /// <param name="AgeData">The age data.</param>
        /// <param name="AgeGroupData">The age group data.</param>
        /// <param name="RaceData">The race data.</param>
        /// <param name="RaceGroupData">The race group data.</param>
        /// <param name="SexData">The sex data.</param>
        /// <param name="SexGroupData">The sex group data.</param>
        /// <param name="PrimaryPayerData">The primary payer data.</param>
        /// <param name="PrimaryPayerGroupData">The primary payer group data.</param>
        private static void ArrangeData(Dictionary<int, object> data, int hospitalid, int ageid, int raceid, int sexid, int primaryPayerid, int? totalCharge, float? totalCost, int lengthOfStay, int quarter, out Dictionary<int, object> HospitalData, out Dictionary<int, object> AgeData, out Dictionary<int, object> AgeGroupData, out Dictionary<int, object> RaceData, out Dictionary<int, object> RaceGroupData, out Dictionary<int, object> SexData, out Dictionary<int, object> SexGroupData, out Dictionary<int, object> PrimaryPayerData, out Dictionary<int, object> PrimaryPayerGroupData)
        {
            HospitalData = GetAndInitIfNeeded(data, hospitalid);
            //HospitalData = AddQuarter(HospitalData, quarter);

            AgeData = GetAndInitIfNeeded(HospitalData, 1);
            AgeGroupData = GetAndInitIfNeeded(AgeData, ageid);

            SexData = GetAndInitIfNeeded(HospitalData, 2);
            SexGroupData = GetAndInitIfNeeded(SexData, sexid);

            PrimaryPayerData = GetAndInitIfNeeded(HospitalData, 3);
            PrimaryPayerGroupData = GetAndInitIfNeeded(PrimaryPayerData, primaryPayerid);

            RaceData = GetAndInitIfNeeded(HospitalData, 4);
            RaceGroupData = GetAndInitIfNeeded(RaceData, raceid);

            AddDischarge2(AgeGroupData, quarter);
            AddDischarge2(SexGroupData, quarter);
            AddDischarge2(PrimaryPayerGroupData, quarter);
            AddDischarge2(RaceGroupData, quarter);

            AddCharge2(AgeGroupData, totalCharge, quarter);
            AddCharge2(SexGroupData, totalCharge, quarter);
            AddCharge2(PrimaryPayerGroupData, totalCharge, quarter);
            AddCharge2(RaceGroupData, totalCharge, quarter);

            AddCost2(AgeGroupData, totalCost, quarter);
            AddCost2(SexGroupData, totalCost, quarter);
            AddCost2(PrimaryPayerGroupData, totalCost, quarter);
            AddCost2(RaceGroupData, totalCost, quarter);

            AddLengthOfStay2(AgeGroupData, lengthOfStay, quarter);
            AddLengthOfStay2(SexGroupData, lengthOfStay, quarter);
            AddLengthOfStay2(PrimaryPayerGroupData, lengthOfStay, quarter);
            AddLengthOfStay2(RaceGroupData, lengthOfStay, quarter);

            //AddQuarter(AgeGroupData, quarter);
            //AddQuarter(SexGroupData, quarter);
            //AddQuarter(PrimaryPayerGroupData, quarter);
            //AddQuarter(RaceGroupData, quarter);
        }

        /// <summary>
        /// Logs the runtime.
        /// </summary>
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

        /// <summary>
        /// Adds the length of stay.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="lengthOfStay">The length of stay.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddLengthOfStay(Dictionary<int, object> groupData, int lengthOfStay, int quarter)
        {
            if (!groupData.ContainsKey(LENGTH_OF_STAYS))
            {
                groupData.Add(LENGTH_OF_STAYS, new List<RptValue<int>>());
            }
            ((List<RptValue<int>>)groupData[LENGTH_OF_STAYS]).Add(new RptValue<int> { Value = lengthOfStay, Quarter = quarter });
        }

        /// <summary>
        /// Adds the length of stay2.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="lengthOfStay">The length of stay.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddLengthOfStay2(Dictionary<int, object> groupData, int lengthOfStay, int quarter)
        {
            if (!groupData.ContainsKey(LENGTH_OF_STAYS))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(LENGTH_OF_STAYS, quarterly);
            }

            ((Dictionary<int, List<int>>)groupData[LENGTH_OF_STAYS])[quarter].Add(lengthOfStay);
        }

        /// <summary>
        /// Adds the length of stay.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="lengthOfStay">The length of stay.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddLengthOfStay(Dictionary<int, object> groupData, List<int> lengthOfStay, int quarter)
        {
            if (!groupData.Keys.Contains(LENGTH_OF_STAYS))
            {
                groupData.Add(LENGTH_OF_STAYS, new List<RptValue<int>>());
            }
            ((List<RptValue<int>>)groupData[LENGTH_OF_STAYS]).AddRange(lengthOfStay.Select(los => new RptValue<int> { Value = los, Quarter = quarter }));
        }

        /// <summary>
        /// Adds the length of stays.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="lengthOfStays">The length of stays.</param>
        private static void AddLengthOfStays(Dictionary<int, object> groupData, Dictionary<int, List<int>> lengthOfStays)
        {

            if (!groupData.ContainsKey(LENGTH_OF_STAYS))
            {
                var quarterly = new Dictionary<int, List<int>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int>());
                }

                groupData.Add(LENGTH_OF_STAYS, quarterly);
            }

            foreach (var item in lengthOfStays)
                ((Dictionary<int, List<int>>)groupData[LENGTH_OF_STAYS])[item.Key].AddRange(item.Value);
        }

        /// <summary>
        /// Adds the length of stay.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="lengthOfStay">The length of stay.</param>
        private static void AddLengthOfStay(Dictionary<int, object> groupData, List<RptValue<int>> lengthOfStay)
        {
            if (!groupData.Keys.Contains(LENGTH_OF_STAYS))
            {
                groupData.Add(LENGTH_OF_STAYS, new List<RptValue<int>>());
            }
            ((List<RptValue<int>>)groupData[LENGTH_OF_STAYS]).AddRange(lengthOfStay);

            //if (!groupData.Keys.Contains(LENGTH_OF_STAYS))
            //{
            //    groupData.Add(LENGTH_OF_STAYS, new List<int>());
            //}
            //((List<int>)groupData[LENGTH_OF_STAYS]).AddRange(lengthOfStay);
        }

        /// <summary>
        /// Adds the cost.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCost">The total cost.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddCost(Dictionary<int, object> groupData, float? totalCost, int quarter)
        {
            if (!groupData.ContainsKey(COSTS))
            {
                groupData.Add(COSTS, new List<RptValue<float?>>());
            }
            ((List<RptValue<float?>>)groupData[COSTS]).Add(new RptValue<float?> { Value = totalCost, Quarter = quarter });

            //if (!groupData.ContainsKey(COSTS))
            //{
            //    groupData.Add(COSTS, new List<float?>());
            //}
            //((List<float?>)groupData[COSTS]).Add(totalCost);
        }

        /// <summary>
        /// Adds the cost2.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCost">The total cost.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddCost2(Dictionary<int, object> groupData, float? totalCost, int quarter)
        {
            if (!groupData.ContainsKey(COSTS))
            {
                var quarterly = new Dictionary<int, List<float?>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<float?>());
                }

                groupData.Add(COSTS, quarterly);
            }

            ((Dictionary<int, List<float?>>)groupData[COSTS])[quarter].Add(totalCost);
        }

        /// <summary>
        /// Adds the cost.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCosts">The total costs.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddCost(Dictionary<int, object> groupData, List<float?> totalCosts, int quarter)
        {
            if (!groupData.ContainsKey(COSTS))
            {
                groupData.Add(COSTS, new List<RptValue<float?>>());
            }
            ((List<RptValue<float?>>)groupData[COSTS]).AddRange(totalCosts.Select(tc => new RptValue<float?> { Value = tc, Quarter = quarter }));

            //if (!groupData.ContainsKey(COSTS))
            //{
            //    groupData.Add(COSTS, new List<float?>());
            //}
            //((List<float?>)groupData[COSTS]).AddRange(totalCosts);
        }

        /// <summary>
        /// Adds the costs.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCosts">The total costs.</param>
        private static void AddCosts(Dictionary<int, object> groupData, List<RptValue<float?>> totalCosts)
        {
            if (!groupData.ContainsKey(COSTS))
            {
                groupData.Add(COSTS, new List<RptValue<float?>>());
            }
            ((List<RptValue<float?>>)groupData[COSTS]).AddRange(totalCosts);

            //if (!groupData.ContainsKey(COSTS))
            //{
            //    groupData.Add(COSTS, new List<float?>());
            //}
            //((List<float?>)groupData[COSTS]).AddRange(totalCosts);
        }

        /// <summary>
        /// Adds the costs.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCosts">The total costs.</param>
        private static void AddCosts(Dictionary<int, object> groupData, Dictionary<int, List<float?>> totalCosts)
        {
            if (!groupData.ContainsKey(COSTS))
            {
                var quarterly = new Dictionary<int, List<float?>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<float?>());
                }

                groupData.Add(COSTS, quarterly);
            }

            foreach (var item in totalCosts)
                ((Dictionary<int, List<float?>>)groupData[COSTS])[item.Key].AddRange(item.Value);
        }

        /// <summary>
        /// Adds the charge.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCharge">The total charge.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddCharge(Dictionary<int, object> groupData, int? totalCharge, int quarter)
        {
            if (!groupData.ContainsKey(CHARGES))
            {
                groupData.Add(CHARGES, new List<RptValue<int?>>());
            }
            ((List<RptValue<int?>>)groupData[CHARGES]).Add(new RptValue<int?> { Value = totalCharge, Quarter = quarter });
        }

        /// <summary>
        /// Adds the charge2.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCharge">The total charge.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddCharge2(Dictionary<int, object> groupData, int? totalCharge, int quarter)
        {
            if (!groupData.ContainsKey(CHARGES))
            {
                var quarterly = new Dictionary<int, List<int?>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int?>());
                }

                groupData.Add(CHARGES, quarterly);
            }

            ((Dictionary<int, List<int?>>)groupData[CHARGES])[quarter].Add(totalCharge);
        }

        /// <summary>
        /// Adds the charge.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCharges">The total charges.</param>
        /// <param name="quarter">The quarter.</param>
        private static void AddCharge(Dictionary<int, object> groupData, List<int?> totalCharges, int quarter)
        {
            if (!groupData.ContainsKey(CHARGES))
            {
                groupData.Add(CHARGES, new List<RptValue<int?>>());
            }
            ((List<RptValue<int?>>)groupData[CHARGES]).AddRange(totalCharges.Select(tc => new RptValue<int?> { Value = tc, Quarter = quarter }));

            //if (!groupData.ContainsKey(CHARGES))
            //{
            //    groupData.Add(CHARGES, new List<int?>());
            //}
            //((List<int?>)groupData[CHARGES]).AddRange(totalCharges);
        }

        /// <summary>
        /// Adds the charges.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCharges">The total charges.</param>
        private static void AddCharges(Dictionary<int, object> groupData, Dictionary<int, List<int?>> totalCharges)
        {
            if (!groupData.ContainsKey(CHARGES))
            {
                var quarterly = new Dictionary<int, List<int?>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int?>());
                }

                groupData.Add(CHARGES, quarterly);
            }

            foreach (var item in totalCharges)
                ((Dictionary<int, List<int?>>) groupData[CHARGES])[item.Key].AddRange(item.Value);
        }

        /// <summary>
        /// Adds the charges.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="totalCharges">The total charges.</param>
        private static void AddCharges(Dictionary<int, object> groupData, List<RptValue<int?>> totalCharges)
        {
            if (!groupData.ContainsKey(CHARGES))
            {
                groupData.Add(CHARGES, new List<RptValue<int?>>());
            }
            ((List<RptValue<int?>>)groupData[CHARGES]).AddRange(totalCharges);
        }

        /// <summary>
        /// Adds the discharge.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="quarter">The quarter.</param>
        /// <param name="numberOfDischarges">The number of discharges.</param>
        private static void AddDischarge(Dictionary<int, object> groupData, int quarter, int? numberOfDischarges = 1)
        {
            if (!groupData.ContainsKey(DISCHARGES))
            {
                groupData.Add(DISCHARGES, new List<RptValue<int?>>());
            }

            ((List<RptValue<int?>>)groupData[DISCHARGES]).Add(new RptValue<int?> { Quarter = quarter, Value = numberOfDischarges });
        }

        /// <summary>
        /// Adds the discharge2.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="quarter">The quarter.</param>
        /// <param name="numberOfDischarges">The number of discharges.</param>
        private static void AddDischarge2(Dictionary<int, object> groupData, int quarter, int? numberOfDischarges = 1)
        {
            if (!groupData.ContainsKey(DISCHARGES))
            {
                var quarterly = new Dictionary<int, List<int?>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int?>());
                }

                groupData.Add(DISCHARGES, quarterly);
            }

            ((Dictionary<int, List<int?>>)groupData[DISCHARGES])[quarter].Add(numberOfDischarges);
            //((List<RptValue<int?>>)groupData[DISCHARGES]).Add(new RptValue<int?> { Quarter = quarter, Value = numberOfDischarges });
        }

        /// <summary>
        /// Adds the discharge.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="quarter">The quarter.</param>
        /// <param name="numberOfDischarges">The number of discharges.</param>
        private static void AddDischarge(Dictionary<int, object> groupData, int quarter, List<int?> numberOfDischarges)
        {
            if (!groupData.ContainsKey(DISCHARGES))
            {
                groupData.Add(DISCHARGES, new List<RptValue<int?>>());
            }

            //((RptValue<int?>)((List<RptValue<int?>>)groupData[DISCHARGES])) = ((RptValue<int?>)groupData[DISCHARGES]).Value + numberOfDischarges <= 0 ? 1 : numberOfDischarges;
            //((RptValue<int?>)groupData[DISCHARGES]).Quarter = quarter;

            if (numberOfDischarges.Any(d => !d.HasValue))
            {
                List<int?> tempDischarges = new List<int?>();

                numberOfDischarges.ForEach(d => { tempDischarges.Add(!d.HasValue ? 1 : d); });

                numberOfDischarges = tempDischarges;
            }

            ((List<RptValue<int?>>)groupData[DISCHARGES]).AddRange(numberOfDischarges.Select(d => new RptValue<int?> { Quarter = quarter, Value = d ?? 1 }).ToList());
            //groupData[DISCHARGES] = (RptValue<int?>)groupData[DISCHARGES];

            //if (!groupData.ContainsKey(DISCHARGES))
            //{
            //    groupData.Add(DISCHARGES, 0);
            //}
            //groupData[DISCHARGES] = (int)groupData[DISCHARGES] + numberOfDischarges;
        }

        /// <summary>
        /// Adds the discharges.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="numberOfDischarges">The number of discharges.</param>
        private static void AddDischarges(Dictionary<int, object> groupData, List<RptValue<int?>> numberOfDischarges)
        {
            if (!groupData.ContainsKey(DISCHARGES))
            {
                groupData.Add(DISCHARGES, new List<RptValue<int?>>());
            }

            ((List<RptValue<int?>>)groupData[DISCHARGES]).AddRange(numberOfDischarges.ToList());
        }

        /// <summary>
        /// Adds the discharges.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="numberOfDischarges">The number of discharges.</param>
        private static void AddDischarges(Dictionary<int, object> groupData, Dictionary<int, List<int?>> numberOfDischarges)
        {
            if (!groupData.ContainsKey(DISCHARGES))
            {
                var quarterly = new Dictionary<int, List<int?>>();

                for (var i = 0; i <= 4; i++)
                {
                    quarterly.Add(i, new List<int?>());
                }

                groupData.Add(DISCHARGES, quarterly);
                // groupData.Add(CHARGES, new List<RptValue<int?>>());
            }

            foreach (var item in numberOfDischarges)
                ((Dictionary<int, List<int?>>)groupData[DISCHARGES])[item.Key].AddRange(item.Value);
        }

        /// <summary>
        /// Adds the quarter.
        /// </summary>
        /// <param name="groupData">The group data.</param>
        /// <param name="quarter">The quarter.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the and initialize if needed.
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private static Dictionary<int, object> GetAndInitIfNeeded(Dictionary<int, object> dic, int id)
        {
            Dictionary<int, object> child;
            if (dic.ContainsKey(id))
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

        /// <summary>
        /// Gets the and initialize if needed2.
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private static Dictionary<int, Dictionary<int, object>> GetAndInitIfNeeded2(Dictionary<int, object> dic, int id)
        {
            Dictionary<int, Dictionary<int, object>> child;
            if (dic.ContainsKey(id))
            {
                child = dic[id] as Dictionary<int, Dictionary<int, object>>;
            }
            else
            {
                Dictionary<int, object> child2;
                child = new Dictionary<int, Dictionary<int, object>>();

                for (var i = 0; i <= 4; i++)
                {
                    child2 = new Dictionary<int, object>();
                    child.Add(i, child2);
                }
                dic.Add(id, child);

            }
            return child;
        }
    }

    /// <summary>
    /// The hospital address data struct used in holding hospital address data.
    /// </summary>
    internal struct HospitalAddressData
    {
        /// <summary>
        /// Gets or sets the hospital identifier.
        /// </summary>
        /// <value>
        /// The hospital identifier.
        /// </value>
        public string HospitalId { get; set; }
        /// <summary>
        /// Gets or sets the region identifier.
        /// </summary>
        /// <value>
        /// The region identifier.
        /// </value>
        public string RegionId { get; set; }
        /// <summary>
        /// Gets or sets the county identifier.
        /// </summary>
        /// <value>
        /// The county identifier.
        /// </value>
        public string CountyId { get; set; }
        /// <summary>
        /// Gets or sets the county fips.
        /// </summary>
        /// <value>
        /// The county fips.
        /// </value>
        public string CountyFIPS { get; set; }
        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>
        /// The zip.
        /// </value>
        public string Zip { get; set; }
    }

    //internal struct RptDataRow
    //{
    //    public int Discharges { get; set; }
    //    public int MeanLOS { get; set; }
    //    public int MedianLOS { get; set; }
    //    public float MeanCharges { get; set; }
    //    public float MedianCharges { get; set; }
    //    public float MeanCosts { get; set; }
    //    public float MedianCosts { get; set; }
    //}

    /// <summary>
    /// The report value object. Not used anymore.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable,
     Obsolete("Not used any more due to performance reasons. Do not use.")]
    internal class RptValue<T>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; set; }
        /// <summary>
        /// Gets or sets the quarter.
        /// </summary>
        /// <value>
        /// The quarter.
        /// </value>
        public int Quarter { get; set; }
    }
}
