using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Modules.Wings;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Practices.ServiceLocation;
using System.Collections;

// TODO: Add indexes on tables.

namespace Monahrq.Reports.Base
{
    internal static class Constants
    {
        public const string WingGuid = "99D7076B-7514-4F6C-84AD-70F5CE01703E";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);
    }

    //[Export(typeof (IReportGenerator))]
    //[WingModuleAttribute(typeof (Module), Constants.WingGuid, "Base Report", "Generates the base report items.",
    //    DependsOnModuleNames = new string[] {"Base Data"})]

    [Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(null, new[] { "Base Data Report", "Base Data" }, null)]
    public partial class Module : BaseReportGenerator, IReportGenerator //WingModule, IReportGenerator
    {

        private ConnectionStringSettings ConnectionSettings { get; set; }
        public ILogWriter Logger { get; private set; }

        private string ReportID { get; set; }
        private string OutputTargetFolder { get; set; }
        private string Hospitals { get; set; }

        private string BaseDataDir { get; set; }

        public string[] ReportIds { get { return new string[] { }; } }

        [ImportingConstructor]
        public Module(IConfigurationService configService,
            [Import(LogNames.Session)] ILogWriter logger,
            IDomainSessionFactoryProvider sessionFactoryProvider,
            Monahrq.Infrastructure.Configuration.IConfigurationService configurationService)
        {
            ConnectionSettings = configService.ConnectionSettings;
            Logger = Logger ?? NullLogger.Instance;
        }

        #region UserDefinedTypes

        #region idsTableType

        private string[] idsTableType = new string[]
        {
            "CREATE TYPE IDsTableType AS TABLE (ID int)"
        };

        #endregion idsTableType

        #region uniqueIDsTableType

        private string[] uniqueIDsTableType = new string[]
        {
            "CREATE TYPE UniqueIDsTableType AS TABLE (ID uniqueidentifier)"
        };

        #endregion uniqueIDsTableType
        
        #region stringsTableType
        
        private string[] stringsTableType = new string[]
        {
            "CREATE TYPE StringsTableType AS TABLE (ID NVARCHAR(MAX))"
        };
        
        #endregion stringsTableType

        #endregion UserDefinedTypes


        #region Tables

        #region tableStratification

        private string[] tableStratification = new string[]
        {
            "CREATE TABLE Temp_Stratification"
            , "("
            , "    ID uniqueidentifier NOT NULL,"
            , "    CatID int NOT NULL,"
            , "    CatVal int NOT NULL"
            , ") ON [PRIMARY]"
        };

        #endregion tableStratification

        #endregion Tables


        #region SProcs

        #region spGetAdmissionSource

        private string[] spGetAdmissionSource = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_AdmissionSource"
        };

        #endregion spGetAdmissionSource

        #region spGetAdmissionType

        private string[] spGetAdmissionType = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_AdmissionType"
        };

        #endregion spGetAdmissionType

        #region spGetAge

        private string[] spGetAge = new string[]
        {
             "    SELECT 1 AS Id, '<18' AS Name"
            ,"    UNION"
            ,"    SELECT 2, '18-44'"
            ,"    UNION"
            ,"    SELECT 3, '45-64'"
            ,"    UNION"
            ,"    SELECT 4, '65+'"
        };

        #endregion spGetAge

        #region spGetDispositionCode

        private string[] spGetDispositionCode = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_DispositionCode"
        };

        #endregion spGetDispositionCode

        #region spGetPayer

        private string[] spGetPayer = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_Payer"
        };

        #endregion spGetPayer

        #region spGetPointOfOrigin

        private string[] spGetPointOfOrigin = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_PointOfOrigin"
        };

        #endregion spGetPointOfOrigin

        #region spGetRace

        private string[] spGetRace = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_Race"
        };

        #endregion spGetRace

        #region spGetSex

        private string[] spGetSex = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_Sex"
        };

        #endregion spGetSex

        #region spGetStratification

        private string[] spGetStratification = new string[]
        {
             "    SELECT 0 AS Id, 'Total' AS Name, 'Total' AS Caption"
            ,"    UNION"
            ,"    SELECT 1, 'Age', 'Age Group'"
            ,"    UNION"
            ,"    SELECT 2, 'Sex', 'Gender'"
            ,"    UNION"
            ,"    SELECT 3, 'Payer', 'Payer'"
            ,"    UNION"
            ,"    SELECT 4, 'Race', 'Race/Ethnicity'"
        };

        #endregion spGetStratification

        #region spGetHospitals

        private string[] spGetHospitals = new string[]
        {
            // ID, Name, County, Region, Zip, Type
            "    SELECT Id, Name, Zip, SelectedRegion_Id AS RegionID"
            , "    FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_Hospital"
            , "    WHERE Id IN ("
            , "        SELECT ID"
            , "        FROM @IDs"
            , "    )"
        };

        private string spGetHospitalsParams = "@IDs IDsTableType READONLY";

        #endregion spGetHospitals

        #region spGetHospitalsByState

        private string[] spGetHospitalsByState = new string[]
        {
            // ID, Name, County, Region, Zip, Type
             "    SELECT Hospitals.Id, Hospitals.Name, Zip, SelectedRegion_Id AS RegionID"
            ,"    FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_Hospital AS Hospitals"
            ,"         LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_State AS States"
            ,"                   ON Hospitals.State_id = States.Id"
            ,"    WHERE States.Abbreviation IN ("
            ,"        SELECT ID"
            ,"        FROM @States"
            ,"    )"
        };

        private string spGetHospitalsByStateParams = "@States StringsTableType READONLY";

        #endregion spGetHospitalsByState

        #region spGetHospitalTypes

        private string[] spGetHospitalTypes = new string[]
        {
            "    SELECT Id AS HospitalTypeID, Name"
            , "    FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_HospitalCategory"
            , "    WHERE Id IN ("
            , "        SELECT HospitalCategory_Id AS Id"
            , "        FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_HospitalHospitalCategory"
            , "        WHERE Hospital_Id IN ("
            , "            SELECT ID"
            , "            FROM @IDs"
            , "        )"
            , "    )"
        };

        private string spGetHospitalTypesParams = "@IDs IDsTableType READONLY";

        #endregion spGetHospitalTypes

        #region spGetCounties

        // Get the active Counties only for the hospitals being included in the website.
        private string[] spGetHospitalCounties = new string[]
        {
            "    SELECT 0 AS CountyID, 'Unkown' AS CountyName"
            , "    UNION"
            , "    SELECT Id AS CountyID, Name AS CountyName"
            , "    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_County"
            , "    WHERE Id IN"
            , "    ("
            , "        SELECT DISTINCT ISNULL([County_Id], 0) AS CountyID"
            , "        FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_Hospital"
            , "        WHERE Id IN ("
            , "            SELECT ID"
            , "            FROM @IDs"
            , "        )"
            , "    )"
        };

        private string spGetHospitalCountiesParams = "@IDs IDsTableType READONLY";

        #endregion spGetCounties

        #region spGetRegions

        // Get the active regions only for the hospitals being included in the website.
        private string[] spGetHospitalRegions = new string[]
        {
            "    SELECT 0 AS RegionID, 'Unkown' AS Name"
            , "    UNION"
            , "    SELECT Id AS RegionID, Name"
            , "    FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_Region"
            , "    WHERE Id IN"
            , "    ("
            , "        SELECT DISTINCT ISNULL([SelectedRegion_Id], 0) AS RegionID"
            , "        FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_Hospital"
            , "        WHERE Id IN ("
            , "            SELECT ID"
            , "            FROM @IDs"
            , "        )"
            , "    )"
        };

        private string spGetHospitalRegionsParams = "@IDs IDsTableType READONLY";

        #endregion spGetRegions

        #region spGetHospitalZips

        private string[] spGetHospitalZips = new string[]
        {
            "    SELECT DISTINCT Zip"
            , "    FROM Monahrq_Infrastructure_Entities_Domain_Hospitals_Hospital"
            , "    WHERE Id IN ("
            , "        SELECT ID"
            , "        FROM @IDs"
            , "    )"
        };

        private string spGetHospitalZipsParams = "@IDs IDsTableType READONLY";

        #endregion spGetHospitalZips


        #region spGetCCS

        private string[] spGetCCS = new string[]
        {
            // ID, Name, County, Region, Zip, Type
            "    SELECT CategoryID, DXCCSID AS CCSID, Description"
            , "    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_DXCCSLabels"
        };

        #endregion spGetCCS

        #region spGetCCSCategories

        private string[] spGetCCSCategories = new string[]
        {
            // ID, Name, County, Region, Zip, Type
            "    SELECT id, Name"
            , "    FROM Monahrq_Infrastructure_Entities_Domain_BaseData_DXCCSCategories"
        };

        #endregion spGetCCSCategories


        #region spInitializeStratificationVals

        private string[] spInitializeStratificationVals = new string[]
        {
            "INSERT INTO Temp_Stratification"
            , "VALUES"
            , "    (@ReportID, 0,0),"
            , "    (@ReportID, 1,1),"
            , "    (@ReportID, 1,2),"
            , "    (@ReportID, 1,3),"
            , "    (@ReportID, 1,4)"
            , ""
            , "INSERT INTO Temp_Stratification"
            , "SELECT @ReportID, 2, Id"
            , "FROM Monahrq_Infrastructure_Entities_Domain_BaseData_Sex"
            , ""
            , "INSERT INTO Temp_Stratification"
            , "SELECT @ReportID, 3, Id"
            , "FROM Monahrq_Infrastructure_Entities_Domain_BaseData_Payer"
            , ""
            , "INSERT INTO Temp_Stratification"
            , "SELECT @ReportID, 4, Id"
            , "FROM Monahrq_Infrastructure_Entities_Domain_BaseData_Race"
        };

        private string spInitializeStratificationValsParams = "@ReportID uniqueidentifier";

        #endregion spInitializeStratificationVals

        #endregion SProcs


        //protected override void OnInitialize()
        //{
        //    base.OnInitialize();
        //}

        //protected override void OnWingAdded()
        private void OnWingAdded()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            //base.OnWingAdded();

            // Add user defined types
            //AddType("IDsTableType", idsTableType);
            CreateOrUpdateDbObject("IDsTableType", idsTableType.ToString(), "", DataObjectTypeEnum.Type);
            AddType("UniqueIDsTableType", uniqueIDsTableType);
            AddType("StringsTableType", stringsTableType);

            // Add tables
            //ConnectionSettings.ExecuteNonQuery(tableStratification);
            CreateOrUpdateDbObject("Temp_Stratification", tableStratification.ToString(), "", DataObjectTypeEnum.Table);

            // Add the sprocs
            //AddSproc("spGetAdmissionSource", spGetAdmissionSource);
            CreateOrUpdateDbObject("spGetAdmissionSource", spGetAdmissionSource.ToString(), "", DataObjectTypeEnum.StoredProcedure);
            AddSproc("spGetAdmissionType", spGetAdmissionType);
            AddSproc("spGetAge", spGetAge);
            AddSproc("spGetDispositionCode", spGetDispositionCode);
            AddSproc("spGetPayer", spGetPayer);
            AddSproc("spGetPointOfOrigin", spGetPointOfOrigin);
            AddSproc("spGetRace", spGetRace);
            AddSproc("spGetSex", spGetSex);
            AddSproc("spGetStratification", spGetStratification);

            //AddSproc("spGetHospitals", spGetHospitals, spGetHospitalsParams);
            CreateOrUpdateDbObject("spGetHospitals", spGetHospitals.ToString(), spGetHospitalsParams, DataObjectTypeEnum.StoredProcedure);
            AddSproc("spGetHospitalsByState", spGetHospitalsByState, spGetHospitalsByStateParams);
            AddSproc("spGetHospitalCounties", spGetHospitalCounties, spGetHospitalCountiesParams);
            AddSproc("spGetHospitalRegions", spGetHospitalRegions, spGetHospitalRegionsParams);
            AddSproc("spGetHospitalZips", spGetHospitalZips, spGetHospitalZipsParams);
            AddSproc("spGetHospitalTypes", spGetHospitalTypes, spGetHospitalTypesParams);

            AddSproc("spGetCCS", spGetCCS);
            AddSproc("spGetCCSCategories", spGetCCSCategories);


            // Initialize tables
            AddSproc("spInitializeStratificationVals", spInitializeStratificationVals, spInitializeStratificationValsParams);
        }

        public override void InitGenerator()
        {
            // This runs every time the application starts up.

            // TODO: The following should only be run once, but we don't have the infrastructure setup like we do in wings yet.
            OnWingAdded();

            try
            {
                OutputTargetFolder = "D:\\Temp\\Monahrq5\\";

                DataTable hospitalsDt = new DataTable();
                DataTable statesDT = new DataTable();
                statesDT.Columns.Add("ID", typeof(string));
                statesDT.Rows.Add("VT");
                statesDT.Rows.Add("NH");

                using (var conn = new SqlConnection(ServiceLocator.Current.GetInstance<IConfigurationService>().ConnectionSettings.ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "spGetHospitalsByState";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = (int)MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds;
                    SqlParameter sqlParam = cmd.Parameters.AddWithValue("@States", statesDT);
                    sqlParam.SqlDbType = SqlDbType.Structured;
                    using (SqlDataAdapter da = new SqlDataAdapter())
                    {
                        da.SelectCommand = cmd;
                        da.Fill(hospitalsDt);
                    }
                    conn.Close();
                }
                
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        public bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            return true;
        }

        public void GenerateReport(Website website)
        {
            OutputTargetFolder = website.Metadata.OutPutDirectory;
            
            //StringBuilder measureList = new StringBuilder();
            //foreach (WebsiteMeasure measure in website.Measures)
            //{
            //    measureList.Append("'" + measure.Id.ToString() +"',");
            //}
            //measureList.Length--;

            //website.Metadata.SelectedReportingStates
            //website.Metadata.SelectedZipCodeRadii

            StringBuilder statesList = new StringBuilder();
            foreach (string state in website.Metadata.SelectedReportingStates)
            {
                statesList.Append("'" + state + "',");
            }
            statesList.Length--;

            DataTable statesDT = new DataTable();
            statesDT.Columns.Add("ID", typeof(string));
            foreach (string state in website.Metadata.SelectedReportingStates)
            {
                statesDT.Rows.Add(state);
            }

            //Hospitals = ?

        }

        // TODO: Delete once base clase is figured out.
        public void GenerateReport(string outputTargetFolder)
        {

        }
        
        public void GenerateReport()
        {
            try
            {
                //OutputTargetFolder = outputTargetFolder;
                //Hospitals = hospitals;

                // Initialize the data for this report.
                InitializeReportData();

                // Make sure the base directories are created.
                CreateBaseDirectories();

                // Generate the json files for the report.
                GenerateJsonFiles();

                // Generate any HTML files for the report.
                GenerateHtml();
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        private void InitializeReportData()
        {
            // Save a report ID for this particular report run.
            ReportID = new Guid().ToString();
        }

        private void CreateBaseDirectories()
        {
            // Make sure the base directories are created.
            BaseDataDir = Path.Combine(OutputTargetFolder, "Data", "Base");
            if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);
        }

        private void GenerateJsonFiles()
        {
            // Generate the json data files for the labels and other base data.
            // NOTE: This is assuming that the directories are deleted before website generation like 4.1.
            //       We are checking if it exists because it could be created in another report.
            if (!File.Exists(Path.Combine(BaseDataDir, "AdmissionSource.js")))
                SaveDTtoJSON(RunDTSproc("spGetAdmissionSource"), Path.Combine(BaseDataDir, "AdmissionSource.js"), "$.monahrq.AdmissionSource=");

            if (!File.Exists(Path.Combine(BaseDataDir, "AdmissionType.js")))
                SaveDTtoJSON(RunDTSproc("spGetAdmissionType"), Path.Combine(BaseDataDir, "AdmissionType.js"), "$.monahrq.AdmissionType=");

            if (!File.Exists(Path.Combine(BaseDataDir, "Age.js")))
                SaveDTtoJSON(RunDTSproc("spGetAge"), Path.Combine(BaseDataDir, "Age.js"), "$.monahrq.Age=");

            if (!File.Exists(Path.Combine(BaseDataDir, "DispositionCode.js")))
                SaveDTtoJSON(RunDTSproc("spGetDispositionCode"), Path.Combine(BaseDataDir, "DispositionCode.js"), "$.monahrq.DispositionCode=");

            if (!File.Exists(Path.Combine(BaseDataDir, "Payer.js")))
                SaveDTtoJSON(RunDTSproc("spGetPayer"), Path.Combine(BaseDataDir, "Payer.js"), "$.monahrq.Payer=");

            if (!File.Exists(Path.Combine(BaseDataDir, "PointOfOrigin.js")))
                SaveDTtoJSON(RunDTSproc("spGetPointOfOrigin"), Path.Combine(BaseDataDir, "PointOfOrigin.js"), "$.monahrq.PointOfOrigin=");

            if (!File.Exists(Path.Combine(BaseDataDir, "Race.js")))
                SaveDTtoJSON(RunDTSproc("spGetRace"), Path.Combine(BaseDataDir, "Race.js"), "$.monahrq.Race=");

            if (!File.Exists(Path.Combine(BaseDataDir, "Sex.js")))
                SaveDTtoJSON(RunDTSproc("spGetSex"), Path.Combine(BaseDataDir, "Sex.js"), "$.monahrq.Sex=");

            if (!File.Exists(Path.Combine(BaseDataDir, "Stratification.js")))
                SaveDTtoJSON(RunDTSproc("spGetStratification"), Path.Combine(BaseDataDir, "Stratification.js"), "$.monahrq.Stratification=");

            // Export the hospital information.
            if (!File.Exists(Path.Combine(BaseDataDir, "Hospitals.js")))
                SaveDTtoJSON(RunDTSproc("spGetHospitals", "@Hospitals", Hospitals), Path.Combine(BaseDataDir, "Hospitals.js"), "$.monahrq.Hospitals=");

            if (!File.Exists(Path.Combine(BaseDataDir, "HospitalCounties.js")))
                SaveDTtoJSON(RunDTSproc("spGetHospitalCounties", "@Hospitals", Hospitals), Path.Combine(BaseDataDir, "HospitalCounties.js"), "$.monahrq.HospitalCounties=");

            if (!File.Exists(Path.Combine(BaseDataDir, "HospitalRegions.js")))
                SaveDTtoJSON(RunDTSproc("spGetHospitalRegions", "@Hospitals", Hospitals), Path.Combine(BaseDataDir, "hospitalRegions.js"), "$.monahrq.HospitalRegions=");

            if (!File.Exists(Path.Combine(BaseDataDir, "HospitalZips.js")))
                SaveDTtoJSON(RunDTSproc("spGetHospitalZips", "@Hospitals", Hospitals), Path.Combine(BaseDataDir, "HospitalZips.js"), "$.monahrq.HospitalZips=");

            if (!File.Exists(Path.Combine(BaseDataDir, "HospitalTypes.js")))
                SaveDTtoJSON(RunDTSproc("spGetHospitalTypes", "@Hospitals", Hospitals), Path.Combine(BaseDataDir, "HospitalTypes.js"), "$.monahrq.HospitalTypes=");

            // Export the clinical dimensions.

            if (!File.Exists(Path.Combine(BaseDataDir, "CCS.js")))
                SaveDTtoJSON(RunDTSproc("spGetCCS"), Path.Combine(BaseDataDir, "CCS.js"), "$.monahrq.CCS=");

            if (!File.Exists(Path.Combine(BaseDataDir, "CCSCategories.js")))
                SaveDTtoJSON(RunDTSproc("spGetCCSCategories"), Path.Combine(BaseDataDir, "CCS.js"), "$.monahrq.CCSCategories=");
        }

        private void GenerateHtml()
        {

        }


        // TODO: Refactor the below into base class
        private DbProviderFactory EntityProvider
        {
            get { return ServiceLocator.Current.GetInstance<IConfigurationService>().EntityProviderFactory; }
        }

        private void AddSproc(string sprocName, string[] sproc, string sprocParams = "")
        {
            // Drop the sproc if it already exists.
            ConnectionSettings.ExecuteNonQuery(
                new string[]
                {
                    string.Format(
                        "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'P', N'PC')) ",
                        sprocName)
                    , string.Format("DROP PROCEDURE {0}", sprocName)
                });

            // Concat the different part of the sprocs together.
            // TODO: Could be converted to an extension method.
            string[] sprocStart =
            {
                string.Format("CREATE PROCEDURE {0}", sprocName)
                , string.Format("    {0}", sprocParams)
                , "AS"
                , "BEGIN"
                , "    SET NOCOUNT ON;"
            };
            string[] sprocEnd = {"END"};
            string[] fullSproc = new string[sprocStart.Length + sproc.Length + sprocEnd.Length];
            sprocStart.CopyTo(fullSproc, 0);
            sproc.CopyTo(fullSproc, sprocStart.Length);
            sprocEnd.CopyTo(fullSproc, sprocStart.Length + sproc.Length);

            // Add the sproc.
            ConnectionSettings.ExecuteNonQuery(fullSproc);
        }

        private void AddType(string typeName, string[] typeCreateString)
        {
            // Drop the sproc if it already exists.
            ConnectionSettings.ExecuteNonQuery(
                new string[]
                {
                    string.Format("IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = '{0}') ",
                        typeName)
                    , string.Format("DROP TYPE {0}", typeName)
                });

            // Add the sproc.
            ConnectionSettings.ExecuteNonQuery(typeCreateString);
        }

        private void RunSproc(string sql,
            string parmName1 = null, string parmVal1 = null,
            string parmName2 = null, string parmVal2 = null,
            string parmName3 = null, string parmVal3 = null)
        {
            try
            {
                using (var conn = EntityProvider.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = EntityProvider.CreateCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout =
                            (int) MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds;
                        if (parmName1 != null)
                            cmd.Parameters.Add(new SqlParameter(parmName1, parmVal1));
                        if (parmName2 != null)
                            cmd.Parameters.Add(new SqlParameter(parmName2, parmVal2));
                        if (parmName3 != null)
                            cmd.Parameters.Add(new SqlParameter(parmName3, parmVal3));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        private DataTable RunDTSproc(string sql, string parmName1 = null, string parmVal1 = null,
            string parmName2 = null, string parmVal2 = null)
        {
            try
            {
                using (var conn = EntityProvider.CreateConnection())
                {
                    DataTable dt = new DataTable();
                    conn.Open();
                    using (var cmd = EntityProvider.CreateCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout =
                            (int) MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds;
                        if (parmName1 != null)
                            cmd.Parameters.Add(new SqlParameter(parmName1, parmVal1));
                        if (parmName2 != null)
                            cmd.Parameters.Add(new SqlParameter(parmName2, parmVal2));
                        using (var da = EntityProvider.CreateDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(dt);
                        }
                    }
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return null;
            }
        }

        private DataSet RunDSSproc(string sql, string parmName1 = null, string parmVal1 = null, string parmName2 = null,
            string parmVal2 = null)
        {
            // Setup the ED Utilization sproc with up to two optional parameters, get the data back (DataSet), and write the json file out.

            DataSet ds = new DataSet();

            try
            {
                using (var conn = EntityProvider.CreateConnection())
                {
                    using (var cmd = EntityProvider.CreateCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout =
                            (int) MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds;
                        if (parmName1 != null)
                            cmd.Parameters.Add(new SqlParameter(parmName1, parmVal1));
                        if (parmName2 != null)
                            cmd.Parameters.Add(new SqlParameter(parmName2, parmVal2));
                        using (var da = EntityProvider.CreateDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(ds);
                        }
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return null;
            }
        }

        private void SaveDStoJSON(DataSet ds, string jsonFile, string jsonHeader)
        {
            using (StreamWriter sw = new StreamWriter(jsonFile))
            {
                sw.Write(jsonHeader);
                sw.Write(JsonConvert.SerializeObject(ds, Formatting.Indented));
                sw.WriteLine(";");
            }
        }

        private void SaveDTtoJSON(DataTable dt, string jsonFile, string jsonHeader)
        {
            using (StreamWriter sw = new StreamWriter(jsonFile))
            {
                sw.Write(jsonHeader);
                sw.Write(JsonConvert.SerializeObject(dt, Formatting.Indented));
                sw.WriteLine(";");
            }
        }

        protected override bool LoadReportData()
        {
            return true;
        }

        protected override bool OuputDataFiles()
        {
            return true;
        }
    }
}