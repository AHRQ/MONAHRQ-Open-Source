using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Modules.Wings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;

// TODO: NOTE: No discharge date found in ED wing, just discharge year.
// TODO: NOTE: "PrincipalDiagnosis" in IP dataset, "PrimaryDiagnosis" in ED dataset. Standardize?
// TODO: Add indexes on tables.

namespace Monahrq.Reports.EmergencyDischarge
{
    static class Constants
    {
        public const string WingGuid = "3DC9EB41-45B4-42D6-9FF5-CADEACD04B85";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);
    }

    [Export(typeof(IReportGenerator))]
    [WingModuleAttribute(typeof(Module), Constants.WingGuid, "Emergency Discharge Report", "Generates the Emergency Discharge report.")]
    //[WingModuleAttribute(typeof(Module), Constants.WingGuid, "Emergency Discharge Report", "Generates the Emergency Discharge report.",
    //    DependsOnModuleNames = new string[] { "Base Report", "Base Data" })]

    public partial class Module : WingModule, IReportGenerator
    {
        ConnectionStringSettings ConnectionSettings { get; set; }
        public ILogWriter Logger { get; private set; }

        private const string jsonDomain = "$.monahrq.emergencydischarge=";
        private string CatIdVals { get; set; }
        private string CcsdxHospitalSummary { get; set; }
        private string CcsdxHospitalDetails { get; set; }

        private string ReportID { get; set; }
        private string OutputTargetFolder { get; set; }
        private string Hospitals { get; set; }
        private string IPDataSets { get; set; }
        private string EDDataSets { get; set; }

        private string BaseDataDir { get; set; }
        private string EDDataDir { get; set; }
        private string CCSDataDir { get; set; }
        private string CCSHospitalNamesDataDir { get; set; }
        private string CCSHospitalCountiesDataDir { get; set; }
        private string CCSHospitalRegionsDataDir { get; set; }
        private string CCSHospitalZipCodesDataDir { get; set; }
        private string CCSHospitalTypesDataDir { get; set; }
        
        private DataTable CCSList { get; set; }
        private DataTable HospitalList { get; set; }
        private DataTable CountyList { get; set; }
        private DataTable RegionList { get; set; }
        private DataTable ZipCodeList { get; set; }
        private DataTable HospTypeList { get; set; }

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



        #region Tables

        #region tableUtilEdDetails
        private string[] tableUtilEdDetails = new string[]
        {
             "CREATE TABLE Temp_UtilED_Details("
            ,"    ID uniqueidentifier NOT NULL,"
	        ,"    HospID varchar(12) NULL,"
	        ,"    RegionID int NULL,"
	        ,"    CCSID int NULL,"
	        ,"    CatID int NULL,"
	        ,"    CatVal int NULL,"
	        ,"    NumEdVisits int NULL,"
	        ,"    NumAdmitHosp int NULL,"
	        ,"    DiedEd int NULL,"
	        ,"    DiedHosp int NULL"
            ,") ON [PRIMARY]"
        };
        #endregion tableUtilEdDetails

        #region tableUtilEdSummary
        private string[] tableUtilEdSummary = new string[]
        {
             "CREATE TABLE Temp_UtilED_Summary("
            ,"    ID uniqueidentifier NOT NULL,"
	        ,"    HospID varchar(12) NULL,"
	        ,"    RegionID int NULL,"
	        ,"    CCSID int NULL,"
	        ,"    NumEdVisits int NULL,"
	        ,"    NumAdmitHosp int NULL,"
	        ,"    DiedEd int NULL,"
	        ,"    DiedHosp int NULL"
            ,") ON [PRIMARY]"
        };
        #endregion tableUtilEdSummary

        #region tableUtilEdNationalTotals
        private string[] tableUtilEdNationalTotals = new string[]
        {
             "CREATE TABLE Temp_UtilED_NationalTotals("
	        ,"    CCSID int NULL,"
	        ,"    NumEdVisits int NULL,"
	        ,"    NumEdVisitsStdErr int NULL,"
	        ,"    NumAdmitHosp int NULL,"
	        ,"    NumAdmitHospStdErr int NULL,"
	        ,"    DiedEd int NULL,"
	        ,"    DiedEdStdErr int NULL,"
	        ,"    DiedHosp int NULL,"
	        ,"    DiedHospStdErr int NULL"
            ,") ON [PRIMARY]"
        };
        #endregion tableUtilEdNationalTotals

        #endregion Tables


        #region Views

        #region viewUtilEdDischarges
        private string[] viewUtilEdDischarges = new string[]
        {
             "CREATE VIEW vwUtilEDDischarges AS"
            ,"SELECT Id, PrincipalDiagnosis as PrimaryDiagnosis, DischargeDisposition, DischargeYear, HospitalID, Age, Race, Sex, PrimaryPayer, EDServices, 1 AS DataSource"
            ,"FROM Monahrq_Wing_Discharge_Inpatient_InpatientTarget"
            ,"UNION ALL"
            ,"SELECT Id, PrimaryDiagnosis, DischargeDisposition, DischargeYear, HospitalID, Age, Race, Sex, PrimaryPayer, '' AS EDServices, 2 AS DataSource"
            ,"FROM Monahrq_Wing_Discharge_TreatAndRelease_TreatAndReleaseTarget"
        };
        #endregion viewUtilEdDischarges

        #endregion Views


        #region SProcs

        // TODO: Pass in data sets
        // TODO: Are we limiting by year / quarter?
        #region spUtilEDInitializeData
        private string[] spUtilEDInitializeData = new string[]
        {
             "-- TODO: Pass in data sets"
            ,"-- TODO: Are we limiting by year / quarter?"
            ,""
            ,"-- Setup a intermediate temporary table"
            ,"CREATE TABLE #Temp_UtilED_Prep"
            ,"("
            ,"    Id int,"
            ,"    Died int,"
            ,"    DischargeYear int,"
            ,"    HospitalID nvarchar(12),"
            ,"    RegionID int,"
            ,"    CCSID int,"
            ,"    Age int,"
            ,"    Race int,"
            ,"    Sex int,"
            ,"    PrimaryPayer int,"
            ,"    DataSource int"
            ,")"
            ,""
            ,"-- Add data from the IP and ED tables to the intermediate temporary table"
            ,"INSERT INTO #Temp_UtilED_Prep"
            ,"SELECT      IP.Id, (CASE WHEN IP.DischargeDisposition = 'Deceased' THEN 1 ELSE 0 END) AS Died,"
            ,"            IP.DischargeYear, IP.HospitalID, Hosp.SelectedRegion_id AS RegionID, CCS.CCSID, IP.Age, Race.Id, Sex.Id, Payer.Id, 1 AS DataSource"
            ,"FROM        Monahrq_Wing_Discharge_Inpatient_InpatientTarget AS IP"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_Hospitals_Hospital AS Hosp ON Hosp.Id = IP.HospitalID"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_ICD9toCCSCrosswalk AS CCS ON IP.PrincipalDiagnosis = CCS.ICD9ID"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_Race AS Race ON IP.Race = Race.Name"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_Sex AS Sex ON IP.Sex = Sex.Name"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_Payer AS Payer ON IP.PrimaryPayer = Payer.Name"
            ,"WHERE       EDServices = 'EDReported' AND CCS.CCSID IS NOT NULL"
            ,""
            ,"INSERT INTO #Temp_UtilED_Prep"
            ,"SELECT      ED.Id, (CASE WHEN ED.DischargeDisposition = 'Deceased' THEN 1 ELSE 0 END) AS Died,"
            ,"            ED.DischargeYear, ED.HospitalID, Hosp.SelectedRegion_id AS RegionID, CCS.CCSID, ED.Age, Race.Id, Sex.Id, Payer.Id, 2 AS DataSource"
            ,"FROM        Monahrq_Wing_Discharge_TreatAndRelease_TreatAndReleaseTarget AS ED"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_Hospitals_Hospital AS Hosp ON Hosp.Id = ED.HospitalID"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_ICD9toCCSCrosswalk AS CCS ON ED.PrimaryDiagnosis = CCS.ICD9ID"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_Race AS Race ON ED.Race = Race.Name"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_Sex AS Sex ON ED.Sex = Sex.Name"
            ,"            LEFT JOIN Monahrq_Infrastructure_Entities_Domain_BaseData_Payer AS Payer ON ED.PrimaryPayer = Payer.Name"
            ,"WHERE       CCS.CCSID IS NOT NULL"
            ,""
            ,"CREATE INDEX IDX_Temp_UtilED_Prep ON #Temp_UtilED_Prep(Died, CCSID)"
            ,""
            ,"-- Copy the summary data into the summary table."
            ,"INSERT INTO Temp_UtilED_Summary"
            ,"    SELECT @ReportID"
            ,"           ,HospitalID  -- HospID"
            ,"           ,RegionID    -- RegID"
            ,"           ,CCSID       -- CSSDX"
            ,"           ,COUNT(*)    -- NumEDVisits"
            ,"           ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp"
            ,"           ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd"
            ,"           ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp"
            ,"    FROM #Temp_UtilED_Prep"
            ,"    GROUP BY HospitalID, RegionID, CCSID"
            ,"    ORDER BY CCSID;"
            ,""
            ,"-- Copy the data for page totals into the details table."
            ,"INSERT INTO Temp_UtilED_Details"
            ,"    SELECT @ReportID"
            ,"           ,HospitalID  -- HospID"
            ,"           ,RegionID    -- RegID"
            ,"           ,CCSID       -- CSSDX"
            ,"           ,0           -- CatID"
            ,"           ,0           -- CatVal"
            ,"           ,COUNT(*)    -- NumEDVisits"
            ,"           ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp"
            ,"           ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd"
            ,"           ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp"
            ,"    FROM #Temp_UtilED_Prep"
            ,"    GROUP BY HospitalID, RegionID, CCSID;"
            ,""
            ,"-- Copy the data for age into the details table."
            ,"INSERT INTO Temp_UtilED_Details"
            ,"    SELECT @ReportID"
            ,"           ,HospitalID  -- HospID"
            ,"           ,RegionID    -- RegID"
            ,"           ,CCSID       -- CSSDX"
            ,"           ,1           -- CatID"
            ,"           ,CASE WHEN (Age < 18) THEN 1					    -- CatVal"
            ,"                 WHEN (Age >= 18 AND Age <= 44) THEN 2"
            ,"                 WHEN (Age >= 45 AND Age <= 64) THEN 3"
            ,"                 WHEN (Age >= 65) THEN 4"
            ,"                 ELSE 0"
            ,"            END"
            ,"           ,COUNT(*)    -- NumEDVisits"
            ,"           ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp"
            ,"           ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd"
            ,"           ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp"
            ,"    FROM #Temp_UtilED_Prep"
            ,"    GROUP BY HospitalID, RegionID, CCSID,"
            ,"            CASE WHEN (Age < 18) THEN 1"
            ,"                 WHEN (Age >= 18 AND Age <= 44) THEN 2"
            ,"                 WHEN (Age >= 45 AND Age <= 64) THEN 3"
            ,"                 WHEN (Age >= 65) THEN 4"
            ,"                 ELSE 0"
            ,"            END;"
            ,""
            ,"-- Copy the data for gender into the details table."
            ,"INSERT INTO Temp_UtilED_Details"
            ,"    SELECT @ReportID"
            ,"           ,HospitalID  -- HospID"
            ,"           ,RegionID    -- RegID"
            ,"           ,CCSID       -- CSSDX"
            ,"           ,2           -- CatID"
            ,"           ,Sex         -- CatVAL"
            ,"           ,COUNT(*)    -- NumEDVisits"
            ,"           ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp"
            ,"           ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd"
            ,"           ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp"
            ,"    FROM #Temp_UtilED_Prep"
            ,"    GROUP BY HospitalID, RegionID, CCSID, Sex;"
            ,""
            ,"-- Copy the data for payer into the details table."
            ,"INSERT INTO Temp_UtilED_Details"
            ,"    SELECT @ReportID"
            ,"           ,HospitalID    -- HospID"
            ,"           ,RegionID		-- RegID"
            ,"           ,CCSID			-- CSSDX"
            ,"           ,3				-- CatID"
            ,"           ,PrimaryPayer	-- CatVAL"
            ,"           ,COUNT(*)		-- NumEDVisits"
            ,"           ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp"
            ,"           ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd"
            ,"           ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp"
            ,"    FROM #Temp_UtilED_Prep"
            ,"    GROUP BY HospitalID, RegionID, CCSID, PrimaryPayer;"
            ,""
            ,"-- Copy the data for race into the details table."
            ,"INSERT INTO Temp_UtilED_Details"
            ,"    SELECT @ReportID"
            ,"           ,HospitalID  -- HospID"
            ,"           ,RegionID    -- RegID"
            ,"           ,CCSID       -- CSSDX"
            ,"           ,4           -- CatID"
            ,"           ,Race        -- CatVAL"
            ,"           ,COUNT(*)    -- NumEDVisits"
            ,"           ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp"
            ,"           ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd"
            ,"           ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp"
            ,"    FROM #Temp_UtilED_Prep"
            ,"    GROUP BY HospitalID, RegionID, CCSID, Race;"
            ,""
            ,"DROP TABLE #Temp_UtilED_Prep"
        };
        private string spUtilEDInitializeDataParams = "@ReportID uniqueidentifier";
        #endregion spUtilEDInitializeData

        #region spUtilEDGetSummaryDataByCCSID
        private string[] spUtilEDGetSummaryDataByCCSID = new string[]
        {
             "    -- One hospital's data per row for all CCSDX conditions combined."
            ,"    -- Note: Should only be run once, so didn't bother setting up a specific table / query for this data."
            ,"    --       This would have just been extra overhead up front to make the final query run faster."
            ,"    IF @CCSID = '0'"
            ,"        BEGIN"
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      Temp_UtilED_NationalTotals"
            ,"            WHERE     CCSID = 0"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     ID = @ReportID"
            ,""
            ,"            SELECT    HospID, RegionID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     ID = @ReportID"
            ,"            GROUP BY  HospID, RegionID"
            ,"        END"
            ,""
            ,"    -- One hospital's data per row for one CCSDX condition."
            ,"    ELSE"
            ,"        BEGIN"
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      Temp_UtilED_NationalTotals"
            ,"            WHERE     CCSID = @CCSID"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     CCSID = @CCSID AND ID = @ReportID"
            ,""
            ,"            SELECT    HospID, RegionID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     CCSID = @CCSID AND ID = @ReportID"
            ,"            GROUP BY  HospID, RegionID"
            ,"        END"
        };
        private string spUtilEDGetSummaryDataByCCSIDParams = "@ReportID UNIQUEIDENTIFIER, @CCSID NVARCHAR(25) = '0'";
        #endregion spUtilEDGetSummaryDataByCCSID

        #region spUtilEDGetSummaryDataByHospID
        private string[] spUtilEDGetSummaryDataByHospID = new string[]
        {
             "    -- One CCSDX condition per row hospital's data per row for all CCSDX conditions combined."
            ,"    -- Note: Should only be run once, so didn't bother setting up a specific table / query for this data."
            ,"    --       This would have just been extra overhead up front to make the final query run faster."
            ,"    IF @HospID = '0'"
            ,"        BEGIN"
            ,"            SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     ID IN ("
            ,"                          SELECT *"
            ,"                          FROM   @ReportIDs"
            ,"                      )"
            ,"            GROUP BY  CCSID"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     ID IN ("
            ,"                          SELECT *"
            ,"                          FROM   @ReportIDs"
            ,"                      )"
            ,""
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      Temp_UtilED_NationalTotals"
            ,"            WHERE     CCSID = 0"
            ,"        END"
            ,""
            ,"    -- One hospital's data per row for one CCSDX condition."
            ,"    ELSE IF @RegionID <> '0'"
            ,"        BEGIN"
            ,"            SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     RegionID = @RegionID AND "
            ,"                      ID IN ("
            ,"                          SELECT *"
            ,"                          FROM   @ReportIDs"
            ,"                      )"
            ,"            GROUP BY  CCSID"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     RegionID = @RegionID AND "
            ,"                      ID IN ("
            ,"                          SELECT *"
            ,"                          FROM   @ReportIDs"
            ,"                      )"
            ,""
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      Temp_UtilED_NationalTotals"
            ,"            WHERE     CCSID = 0"
            ,"        END"
            ,""
            ,"    -- One hospital's data per row for one CCSDX condition."
            ,"    ELSE"
            ,"        BEGIN"
            ,"            SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     HospID = @HospID AND "
            ,"                      ID IN ("
            ,"                          SELECT *"
            ,"                          FROM   @ReportIDs"
            ,"                      )"
            ,"            GROUP BY  CCSID"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     HospID = @HospID AND "
            ,"                      ID IN ("
            ,"                          SELECT *"
            ,"                          FROM   @ReportIDs"
            ,"                      )"
            ,""
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      Temp_UtilED_NationalTotals"
            ,"            WHERE     CCSID = 0"
            ,"        END"
        };
        private string spUtilEDGetSummaryDataByHospIDParams = "@ReportIDs UniqueIDsTableType READONLY, @HospID NVARCHAR(25) = '0', @RegionID NVARCHAR(25) = '0'";
        #endregion spUtilEDGetSummaryDataByHOSPID

        #region spUtilEDGetDetailData
        private string[] spUtilEDGetDetailData = new string[]
        {
             "    -- Get the details page for all CCSDX conditions across all hospitals."
            ,"    IF @CCSID = '0' AND @HospID = '0'"
            ,"        BEGIN"
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      UtilEd_NationalTotals"
            ,"            WHERE     CCSID = 0"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     ID = @ReportID"
            ,""
            ,"            SELECT D.*, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp"
            ,"            FROM UtilEd_CatIdVals D LEFT JOIN "
            ,"            ("
            ,"                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"                FROM      Temp_UtilED_Details"
            ,"                GROUP BY  CatID, CatVal"
            ,"            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal"
            ,"            WHERE     ID = @ReportID"
            ,"        END"
            ,""
            ,"    -- Get the details page for all CCSDX conditions at one hospital."
            ,"    ELSE IF @CCSID = '0' AND @HospID <> '0'"
            ,"        BEGIN"
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      UtilEd_NationalTotals"
            ,"            WHERE     CCSID = 0"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     HospID = @HospID AND ID = @ReportID"
            ,""
            ,"            SELECT D.*, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp"
            ,"            FROM UtilEd_CatIdVals D LEFT JOIN "
            ,"            ("
            ,"                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"                FROM      Temp_UtilED_Details"
            ,"                WHERE     HospID = @HospID AND ID = @ReportID"
            ,"                GROUP BY  CatID, CatVal"
            ,"            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal"
            ,"         END"
            ,""
            ,"    -- Get the details page for one CCSDX condition across all hospitals."
            ,"    ELSE IF @CCSID <> '0' AND @HospID = '0'"
            ,"        BEGIN"
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      UtilEd_NationalTotals"
            ,"            WHERE     CCSID = @CCSID"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     CCSID = @CCSID AND ID = @ReportID"
            ,""
            ,"            SELECT D.*, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp"
            ,"            FROM UtilEd_CatIdVals D LEFT JOIN "
            ,"            ("
            ,"                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"                FROM      Temp_UtilED_Details"
            ,"                WHERE     CCSID = @CCSID AND ID = @ReportID"
            ,"                GROUP BY  CatID, CatVal"
            ,"            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal"
            ,"        END"
            ,""
            ,"    -- Get the details page for one CCSDX condition at one hospital."
            ,"    ELSE"
            ,"        BEGIN"
            ,"            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr"
            ,"            FROM      UtilEd_NationalTotals"
            ,"            WHERE     CCSID = @CCSID"
            ,""
            ,"            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp"
            ,"            FROM      Temp_UtilED_Summary"
            ,"            WHERE     CCSID = @CCSID AND HospID = @HospID AND ID = @ReportID"
            ,""
            ,"            SELECT D.*, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp"
            ,"            FROM UtilEd_CatIdVals D LEFT JOIN "
            ,"            ("
            ,"                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp"
            ,"                FROM      Temp_UtilED_Details"
            ,"                WHERE     CCSID = @CCSID AND HospID = @HospID AND ID = @ReportID"
            ,"                GROUP BY  CatID, CatVal"
            ,"            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal"
            ,"        END"
        };
        private string spUtilEDGetDetailDataParams = "@ReportID UNIQUEIDENTIFIER, @CCSID NVARCHAR(25) = '0', @HospID NVARCHAR(25) = '0'";
        #endregion spUtilEDGetDetailData

        #endregion SProcs


        protected override void OnInitialize()
        {
            base.OnInitialize();
            CatIdVals = "UtilED_CatIdVals_" + System.Guid.NewGuid().ToString().Replace("-", "_");
            CcsdxHospitalSummary = "Temp_UtilED_Summary_" + System.Guid.NewGuid().ToString().Replace("-", "_");
            CcsdxHospitalDetails = "Temp_UtilED_Detailss_" + System.Guid.NewGuid().ToString().Replace("-", "_");

            // NOTE: Testing for getting data out.
            //using (var conn = EntityProvider.CreateConnection())
            //{
            //    DataTable dt = new DataTable();
            //    conn.Open();
            //    using (var cmd = EntityProvider.CreateCommand())
            //    {
            //        cmd.Connection = conn;
            //        cmd.CommandText = "spUtilEDInitializeStratificationVals";
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        //cmd.CommandTimeout = MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeoutSecs;
            //        cmd.Parameters.Add(new SqlParameter("TableName", CatIdVals));
            //        using (var da = EntityProvider.CreateDataAdapter())
            //        {
            //            da.SelectCommand = cmd;
            //            da.Fill(dt);
            //        }
            //    }
            //}
        }

        protected override void OnWingAdded()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            base.OnWingAdded();

            // Setup the ED Utilization specific tables.
            ConnectionSettings.ExecuteNonQuery(tableUtilEdDetails);
            ConnectionSettings.ExecuteNonQuery(tableUtilEdSummary);
            ConnectionSettings.ExecuteNonQuery(tableUtilEdNationalTotals);

            // Setup the ED Utilization specific views.
            ConnectionSettings.ExecuteNonQuery(viewUtilEdDischarges);

            // Import the national totals
            // TODO: Import the national totals

            // Setup the needed sprocs
            AddSproc("spUtilEDInitializeData", spUtilEDInitializeData, spUtilEDInitializeDataParams);
            AddSproc("spUtilEDGetSummaryDataByCCSID", spUtilEDGetSummaryDataByCCSID, spUtilEDGetSummaryDataByCCSIDParams);
            AddSproc("spUtilEDGetSummaryDataByHospID", spUtilEDGetSummaryDataByHospID, spUtilEDGetSummaryDataByHospIDParams);
            AddSproc("spUtilEDGetDetailData", spUtilEDGetDetailData, spUtilEDGetDetailDataParams);

            // TODO: Temp export for testing
            //GenerateReport("C:\\Temp\\Monahrq5Test");
        }

        public void InitGenerator()
        {

        }

        public bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            return true;
        }

        public void GenerateReport(Website website)
        {

        }

        // TODO: Delete once base clase is figured out.
        public void GenerateReport(string outputTargetFolder)
        {
            
        }
                
        public void GenerateReport(string outputTargetFolder, string hospitals, string ipDatasets, string edDatasets)
        {
            try
            {
                // Note: Playing it safe and re-running the spUtilEDInitializeStratificationVals before every generation in case
                //       the base data table was updated (a good possibility over time) after this module was first loaded.

                OutputTargetFolder = outputTargetFolder;
                Hospitals = hospitals;
                IPDataSets = ipDatasets;
                EDDataSets = edDatasets;

                // Initialize the data for this report.
                InitializeReportData();

                // Make sure the base directories are created.
                CreateBaseDirectories();
                
                // Generate the json files for the report.
                GenerateJsonFiles();

                // Generate any HTML files for the report.
                GenerateHtml();
            }
            catch(Exception ex)
                    {
                Logger.Write(ex);
            }
                    }

        private void InitializeReportData()
                    {
            // Save a report ID for this particular report run.
            ReportID = new Guid().ToString();

            // Initialize the base stratification values (shouldn't change except on base data updates, but playing it safe).
            RunSproc("spUtilEDInitializeStratificationVals", "ReportID", ReportID);

            // Save the needed ED data out to the temporary tables.
            RunSproc("spUtilEDInitializeData", "ReportID", ReportID);

            // Get the lists needed to generate the json files.
            CCSList = new DataTable();
            CCSList = RunDTSproc("spGetCCS");

            HospitalList = new DataTable();
            HospitalList = RunDTSproc("spGetHospitals", "@Hospitals", Hospitals);

            CountyList = new DataTable();
            HospitalList = RunDTSproc("spGetHospitalCounties", "@Hospitals", Hospitals);

            RegionList = new DataTable();
            RegionList = RunDTSproc("spGetHospitalRegions", "@Hospitals", Hospitals);

            ZipCodeList = new DataTable();
            HospitalList = RunDTSproc("spGetHospitalZips", "@Hospitals", Hospitals);

            HospTypeList = new DataTable();
            HospitalList = RunDTSproc("spGetHospitalTypes", "@Hospitals", Hospitals);
                    }

        private void CreateBaseDirectories()
                    {
            BaseDataDir = Path.Combine(OutputTargetFolder, "Data", "Base");
            if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);

            EDDataDir = Path.Combine(OutputTargetFolder, "Data", "EmergencyDischarge");
            if (!Directory.Exists(EDDataDir)) Directory.CreateDirectory(BaseDataDir);

            CCSDataDir = Path.Combine(EDDataDir, "CCS", "CCS");
            if (!Directory.Exists(CCSDataDir)) Directory.CreateDirectory(CCSDataDir);

            CCSHospitalCountiesDataDir = Path.Combine(EDDataDir, "CCS", "County");
            if (!Directory.Exists(CCSHospitalCountiesDataDir)) Directory.CreateDirectory(CCSHospitalCountiesDataDir);

            CCSHospitalRegionsDataDir = Path.Combine(EDDataDir, "CCS", "Region");
            if (!Directory.Exists(CCSHospitalRegionsDataDir)) Directory.CreateDirectory(CCSHospitalRegionsDataDir);

            CCSHospitalZipCodesDataDir = Path.Combine(EDDataDir, "CCS", "ZipCode");
            if (!Directory.Exists(CCSHospitalZipCodesDataDir)) Directory.CreateDirectory(CCSHospitalZipCodesDataDir);

            CCSHospitalNamesDataDir = Path.Combine(EDDataDir, "CCS", "Hospital");
            if (!Directory.Exists(CCSHospitalNamesDataDir)) Directory.CreateDirectory(CCSHospitalNamesDataDir);

            CCSHospitalTypesDataDir = Path.Combine(EDDataDir, "CCS", "HospitalType");
            if (!Directory.Exists(CCSHospitalTypesDataDir)) Directory.CreateDirectory(CCSHospitalTypesDataDir);            
                    }

        private void GenerateJsonFiles()
                    {
            // Get a list of supporting data tables needed to generate the json files.
            // TODO: // Save these to json in the base data module. Just get the list here for looping through.

            // Generate any report specific json files

            // Generate the main json files
            GenerateJsonFileCombinations(CCSDataDir, "CCS_", "spUtilEdGetSummaryDataByCCSID", CCSList, "@CCSID", "CCSID", HospitalList, "@HospID", "Id");
            GenerateJsonFileCombinations(CCSHospitalNamesDataDir, "Hospital_", "spUtilEdGetSummaryDataByHospID", HospitalList, "@HospID", "Id", CCSList, "@CCSID", "CCSID");
            GenerateJsonFileCombinations(CCSHospitalTypesDataDir, "HospType_", "spUtilEdGetSummaryDataByHospID", HospTypeList, "@HospTypeID", "HospitalTypeID", CCSList, "@CCSID", "CCSID");
            GenerateJsonFileCombinations(CCSHospitalCountiesDataDir, "County_", "spUtilEdGetSummaryDataByHospID", CountyList, "@CountyID", "CountyID", CCSList, "@CCSID", "CCSID");
            GenerateJsonFileCombinations(CCSHospitalRegionsDataDir, "Region_", "spUtilEdGetSummaryDataByHospID", RegionList, "@RegionID", "RegionID", CCSList, "@CCSID", "CCSID");
            GenerateJsonFileCombinations(CCSHospitalZipCodesDataDir, "ZipCode_", "spUtilEdGetSummaryDataByHospID", ZipCodeList, "@ZipCode", "Zip", CCSList, "@CCSID", "CCSID");
                    }

        private void GenerateJsonFileCombinations(string dataDir, string subDir, string summarySproc,
                                                  DataTable outerDataTable, string outerParam, string outerParamName,
                                                  DataTable innerDataTable, string innerParam, String innerParamName)
        {
            // TODO: Check that we're getting region data and total for hospitals selected.

            // Hospital Names

            DataSet ds = new DataSet();

            // Create summary page for all outer records combined
            // \CCS\Outer\Outer_0\summary.json
            ds = RunDSSproc(summarySproc);
            ds.Tables[0].TableName = "NationalData";
            ds.Tables[1].TableName = "TotalData";
            ds.Tables[2].TableName = "TableData";
            SaveDStoJSON(ds, Path.Combine(dataDir, subDir + "0", "summary.js"), jsonDomain);

            // Create details page for all outer and inner records combined.
            // \CCS\Outer\Outer_0\details.json
            ds = RunDSSproc("spUtilEdGetDetailData");
            ds.Tables[0].TableName = "NationalData";
            ds.Tables[1].TableName = "TotalData";
            ds.Tables[2].TableName = "TableData";
            SaveDStoJSON(ds, Path.Combine(dataDir, subDir + "0", "details.js"), jsonDomain);

            // Create details page for all outer records combined for one inner record.
            // \CCS\Outer\Outer_0\details_[InnterID].json
            foreach (DataRow innerRow in innerDataTable.Rows)
                    {
                ds = RunDSSproc("spUtilEdGetDetailData", innerParam, innerRow[innerParamName].ToString());
                ds.Tables[0].TableName = "NationalData";
                ds.Tables[1].TableName = "TotalData";
                ds.Tables[2].TableName = "TableData";
                SaveDStoJSON(ds, Path.Combine(dataDir, subDir + "0", "details_" + innerRow[innerParamName].ToString() + ".js"), jsonDomain);
                    }

            foreach (DataRow outerRow in outerDataTable.Rows)
            {
                // Create summary page for one outer record.
                // \CCS\Outer\Outer_[OuterID]\summary.json
                ds = RunDSSproc(summarySproc, outerParam, outerRow[outerParamName].ToString());
                ds.Tables[0].TableName = "NationalData";
                ds.Tables[1].TableName = "TotalData";
                ds.Tables[2].TableName = "TableData";
                SaveDStoJSON(ds, Path.Combine(dataDir, subDir + outerRow[outerParamName], "summary.js"), jsonDomain);

                // Create details page for one outer record and all inner records combiner.
                // \CCS\Outer\Outer_[OuterID]\details.json
                ds = RunDSSproc("spUtilEdGetDetailData", outerParam, outerRow[outerParamName].ToString());
                ds.Tables[0].TableName = "NationalData";
                ds.Tables[1].TableName = "TotalData";
                ds.Tables[2].TableName = "TableData";
                SaveDStoJSON(ds, Path.Combine(dataDir, subDir + outerRow[outerParamName], "details.js"), jsonDomain);

                // Create details page for one outer and inner record.
                // \CCS\Outer\Outer_[OuterID]\details_[InnerID].json
                foreach (DataRow innerRow in innerDataTable.Rows)
                    {
                    ds = RunDSSproc("spUtilEdGetDetailData", outerParam, outerRow[outerParamName].ToString(), innerParam, innerRow[innerParamName].ToString());
                    ds.Tables[0].TableName = "NationalData";
                    ds.Tables[1].TableName = "TotalData";
                    ds.Tables[2].TableName = "TableData";
                    SaveDStoJSON(ds, Path.Combine(dataDir, subDir + outerRow[outerParamName], "details_" + innerRow[innerParamName].ToString() + ".js"), jsonDomain);
                }
            }
                    }

        private void GenerateHtml()
                    {
            
                    }


        // TODO: Refactor the below into base class
        DbProviderFactory EntityProvider
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
            string[] sprocEnd = { "END" };
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
                        cmd.CommandTimeout = (int)MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds;
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
    
        private DataTable RunDTSproc(string sql, string parmName1 = null, string parmVal1 = null, string parmName2 = null, string parmVal2 = null)
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
                        cmd.CommandTimeout = (int)MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds;
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

        private DataSet RunDSSproc(string sql, string parmName1 = null, string parmVal1 = null, string parmName2 = null, string parmVal2 = null)
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
                        cmd.CommandTimeout = (int)MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds;
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
    }
}
