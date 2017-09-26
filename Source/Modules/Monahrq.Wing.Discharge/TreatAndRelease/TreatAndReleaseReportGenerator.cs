using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Generators;

using Monahrq.Sdk.Services.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensions;

namespace Monahrq.Wing.Discharge.TreatAndRelease
{
	/// <summary>
	/// Generates the report data/.json files for Treat and Release Discharge reports
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	/// <seealso cref="Monahrq.Sdk.Generators.IReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new[] { "2AAF7FBA-7102-4C66-8598-A70597E2F827" },
                     new[] { "ED Treat And Release" },
                     new[] { typeof(TreatAndReleaseTarget) },
                     11)]
    public class TreatAndReleaseReportGenerator : BaseReportGenerator, IReportGenerator
    {
		/// <summary>
		/// The json domain
		/// </summary>
		private const string jsonDomain = "$.monahrq.emergencydischarge=";

		/// <summary>
		/// Gets or sets the report identifier.
		/// </summary>
		/// <value>
		/// The report identifier.
		/// </value>
		private string ReportID { get; set; }
		/// <summary>
		/// Gets or sets the ip dataset i ds.
		/// </summary>
		/// <value>
		/// The ip dataset i ds.
		/// </value>
		private DataTable IPDatasetIDs { get; set; }
		/// <summary>
		/// Gets or sets the ed dataset i ds.
		/// </summary>
		/// <value>
		/// The ed dataset i ds.
		/// </value>
		private DataTable EDDatasetIDs { get; set; }

		/// <summary>
		/// Gets or sets the base data dir.
		/// </summary>
		/// <value>
		/// The base data dir.
		/// </value>
		private string BaseDataDir { get; set; }
		/// <summary>
		/// Gets or sets the ed data dir.
		/// </summary>
		/// <value>
		/// The ed data dir.
		/// </value>
		private string EDDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS data dir.
		/// </summary>
		/// <value>
		/// The CCS data dir.
		/// </value>
		private string CCSDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital names data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital names data dir.
		/// </value>
		private string CCSHospitalNamesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital counties data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital counties data dir.
		/// </value>
		private string CCSHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital regions data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital regions data dir.
		/// </value>
		private string CCSHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital zip codes data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital zip codes data dir.
		/// </value>
		private string CCSHospitalZipCodesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital types data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital types data dir.
		/// </value>
		private string CCSHospitalTypesDataDir { get; set; }

		/// <summary>
		/// Gets or sets the CCS list.
		/// </summary>
		/// <value>
		/// The CCS list.
		/// </value>
		private DataTable CCSList { get; set; }
		/// <summary>
		/// Gets or sets the hospital list.
		/// </summary>
		/// <value>
		/// The hospital list.
		/// </value>
		private DataTable HospitalList { get; set; }
		/// <summary>
		/// Gets or sets the county list.
		/// </summary>
		/// <value>
		/// The county list.
		/// </value>
		private DataTable CountyList { get; set; }
		/// <summary>
		/// Gets or sets the region list.
		/// </summary>
		/// <value>
		/// The region list.
		/// </value>
		private DataTable RegionList { get; set; }

		/// <summary>
		/// Gets or sets the hospital type list.
		/// </summary>
		/// <value>
		/// The hospital type list.
		/// </value>
		private DataTable HospitalTypeList { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TreatAndReleaseReportGenerator"/> class.
		/// </summary>
		public TreatAndReleaseReportGenerator()
            : base()
        { }


		#region Tables

		#region tableUtilEdPrep

		//        private const string tableUtilEdPrep = @"
		//            /*
		//             *      Name:           Temp_UtilED_Prep
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        InpatientReportGenerator.cs
		//             *      Description:    Used to store temporary IP data utilized in the json file generation.
		//             */
		//
		//            CREATE TABLE Temp_UtilED_Prep
		//            (
		//	            ID uniqueidentifier NOT NULL,
		//	            HospitalId int,
		//                RegionID int,
		//                CountyID int,
		//                Zip nvarchar(12),
		//                HospitalType nvarchar(MAX),
		//
		//                CCSID int,
		//                DataSource int,
		//
		//                Died int,
		//                DischargeYear int,
		//                Age int,
		//                Race int,
		//                Sex int,
		//                PrimaryPayer int
		//            )
		//
		//            CREATE INDEX IDX_Temp_UtilED_Prep_HospitalId ON Temp_UtilED_Prep(HospitalId);
		//
		//            CREATE INDEX IDX_Temp_UtilED_Prep_CCSID ON Temp_UtilED_Prep(CCSID);
		//
		//            CREATE INDEX IDX_Temp_UtilED_Prep_SummaryGroup ON Temp_UtilED_Prep(HospitalID, RegionID, CountyID, Zip, CCSID);
		//        ";

		#endregion tableUtilEdPrep

		#region tableUtilEdDetails

		//        private const string tableUtilEdDetails = @"
		//            /*
		//             *      Name:           Temp_UtilED_Details
		//             *      Version:        1.0
		//             *      Last Updated:   3/28/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *      Description:    Used to store temporary ED data utilized in the json file generation.
		//             */
		//
		//            CREATE TABLE Temp_UtilED_Details (
		//                    ID uniqueidentifier NOT NULL,
		//                    HospitalID int NOT NULL,
		//                    RegionID int NULL,
		//                    CountyID int NULL,
		//                    Zip nvarchar(12) NULL,
		//                    HospitalType nvarchar(MAX) NULL,
		//                    CCSID int NULL,
		//                    CatID int NULL,
		//                    CatVal int NULL,
		//                    NumEdVisits int NULL,
		//                    NumAdmitHosp int NULL,
		//                    DiedEd int NULL,
		//                    DiedHosp int NULL
		//                ) ON [PRIMARY]
		//
		//                CREATE INDEX IDX_Temp_UtilED_Details_Id ON Temp_UtilED_Details(ID);
		//
		//                CREATE INDEX IDX_Temp_UtilED_Details_IdAndHospitalID ON Temp_UtilED_Details(ID, HospitalID);
		//
		//                CREATE INDEX IDX_Temp_UtilED_Details_IdAndRegionID ON Temp_UtilED_Details(ID, RegionID);
		//
		//                CREATE INDEX IDX_Temp_UtilED_Details_IdAndZip ON Temp_UtilED_Details(ID, Zip);
		//
		//                CREATE INDEX IDX_Temp_UtilED_Details_IdAndCountyID ON Temp_UtilED_Details(ID, CountyID);
		//
		//                CREATE INDEX IDX_Temp_UtilED_Details_SummaryGroup
		//                    ON Temp_UtilED_Details(ID, CCSID, HospitalID, RegionID, CountyID, Zip);
		//            ";

		#endregion tableUtilEdDetails

		#region tableUtilEdSummary

		//        private const string tableUtilEdSummary = @"
		//            /*
		//             *      Name:           Temp_UtilED_Summary
		//             *      Version:        1.0
		//             *      Last Updated:   3/28/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *      Description:    Used to store temporary ED data utilized in the json file generation.
		//             */
		//
		//            CREATE TABLE Temp_UtilED_Summary (
		//                    ID uniqueidentifier NOT NULL,
		//                    HospitalID int NOT NULL,
		//                    RegionID int NULL,
		//                    CountyID int NULL,
		//                    Zip nvarchar(12) NULL,
		//                    HospitalType nvarchar(MAX) NULL,
		//                    CCSID int NULL,
		//                    NumEdVisits int NULL,
		//                    NumAdmitHosp int NULL,
		//                    DiedEd int NULL,
		//                    DiedHosp int NULL
		//                ) ON [PRIMARY]
		//
		//            CREATE INDEX IDX_Temp_UtilED_Summary_Id ON Temp_UtilED_Summary(ID);
		//
		//            CREATE INDEX IDX_Temp_UtilED_Summary_IdAndHospitalID ON Temp_UtilED_Summary(ID, HospitalID);
		//
		//            CREATE INDEX IDX_Temp_UtilED_Summary_IdAndRegionID ON Temp_UtilED_Summary(ID, RegionID);
		//
		//            CREATE INDEX IDX_Temp_UtilED_Summary_IdAndZip ON Temp_UtilED_Summary(ID, Zip);
		//
		//            CREATE INDEX IDX_Temp_UtilED_Summary_IdAndCountyID ON Temp_UtilED_Summary(ID, CountyID);
		//
		//            CREATE INDEX IDX_Temp_UtilED_Summary_SummaryGroup
		//                ON Temp_UtilED_Summary(ID, CCSID, HospitalID, RegionID, CountyID, Zip);
		//            ";

		#endregion tableUtilEdSummary

		#endregion Tables


		#region SProcs

		#region spUtilEDInitializeIPVisits

		//        private const string spUtilEDInitializeIPVisits = @"
		//            /*
		//             *      Name:           spUtilEDInitializeIPVisits
		//                *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//                *      Used In:        TreatAndReleaseReportGenerator.cs
		//                *      Description:    Used to get all the needed data for the given report.
		//                */
		//
		//            -- Add data from the IP table to the intermediate temporary table
		//            INSERT INTO Temp_UtilED_Prep
		//            SELECT      @ReportID AS ID, Hosp.Id AS HospitalID, ISNULL([dbo].[fnGetHospitalRegion](Hosp.Id, @RegionType), -1) AS RegionID,
		//                        Hosp.County_id AS CountyID, Hosp.Zip, '' AS HospitalType, CCS.DXCCSID, 1 AS DataSource, 
		//			            (CASE WHEN IP.DischargeDisposition = 'Died' THEN 1 ELSE 0 END) AS Died,
		//                        IP.DischargeYear, 
		//                        CASE WHEN (IP.Age < 18) THEN 1					    -- CatVal
		//                            WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2
		//                            WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3
		//                            WHEN (IP.Age >= 65) THEN 4
		//                            ELSE 0
		//                        END AS Age, Race.Id, Sex.Id, Payer.Id
		//            FROM        dbo.Targets_InpatientTargets AS IP
		//                        LEFT JOIN dbo.Hospitals AS Hosp ON Hosp.LocalHospitalID = IP.LocalHospitalID
		//                        LEFT JOIN dbo.Base_ICD9toDXCCSCrosswalks AS CCS ON IP.PrincipalDiagnosis = CCS.ICD9ID
		//                        LEFT JOIN dbo.Base_Races AS Race ON ISNULL(IP.Race, 'Missing') = Race.Name
		//                        LEFT JOIN dbo.Base_Sexes AS Sex ON IP.Sex = Sex.Name
		//                        LEFT JOIN dbo.Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 'Missing') = Payer.Name
		//            WHERE       EDServices = 'ED Services Reported' AND
		//                        CCS.DXCCSID IS NOT NULL AND
		//                        Hosp.Id IN (
		//                            SELECT Id
		//                            FROM @Hospitals
		//                        ) AND
		//                        IP.Dataset_id IN (
		//                            SELECT Id
		//                            FROM @IPDataset
		//                        );
		//        ";

		//private string spUtilEDInitializeIPVisitsParams = "@ReportID uniqueidentifier, @ReportYear varchar(4), @Hospitals IDsTableType READONLY, @IPDataset IDsTableType READONLY, @EDDataset IDsTableType READONLY, @RegionType nvarchar(50)";

		#endregion spUtilEDInitializeIPVisits

		#region spUtilEDInitializeEDVisits

		//        private const string spUtilEDInitializeEDVisits = @"
		//            /*
		//             *      Name:           spUtilEDInitializeEDVisits
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *      Description:    Used to get all the needed data for the given report.
		//             */
		//            
		//            -- Add data from the IP table to the intermediate temporary table
		//            INSERT INTO Temp_UtilED_Prep
		//            SELECT      @ReportID AS ID, Hosp.Id AS HospitalID, ISNULL([dbo].[fnGetHospitalRegion](Hosp.Id, @RegionType), -1) AS RegionID,
		//                        Hosp.County_id AS CountyID, Hosp.Zip, '' AS HospitalType, CCS.DXCCSID, 2 AS DataSource,
		//			            (CASE WHEN ED.DischargeDisposition = 'Died' THEN 1 ELSE 0 END) AS Died,
		//                        ED.DischargeYear,
		//			            CASE WHEN (ED.Age < 18) THEN 1					    -- CatVal
		//                            WHEN (ED.Age >= 18 AND ED.Age <= 44) THEN 2
		//                            WHEN (ED.Age >= 45 AND ED.Age <= 64) THEN 3
		//                            WHEN (ED.Age >= 65) THEN 4
		//                            ELSE 0
		//                        END AS Age, Race.Id, Sex.Id, Payer.Id
		//            FROM        dbo.Targets_TreatAndReleaseTargets AS ED
		//                        LEFT JOIN dbo.Hospitals AS Hosp ON Hosp.LocalHospitalID = ED.LocalHospitalID
		//                        LEFT JOIN dbo.Base_ICD9toDXCCSCrosswalks AS CCS ON ED.PrimaryDiagnosis = CCS.ICD9ID
		//                        LEFT JOIN dbo.Base_Races AS Race ON ISNULL(ED.Race, 'Missing') = Race.Name
		//                        LEFT JOIN dbo.Base_Sexes AS Sex ON ED.Sex = Sex.Name
		//                        LEFT JOIN dbo.Base_Payers AS Payer ON ISNULL(ED.PrimaryPayer, 'Missing') = Payer.Name
		//            WHERE       CCS.DXCCSID IS NOT NULL AND
		//                        Hosp.Id IN (
		//                            SELECT Id
		//                            FROM @Hospitals
		//                        ) AND
		//                        ED.Dataset_id IN (
		//                            SELECT Id
		//                            FROM @EDDataset
		//                        );
		//        ";

		//        private string spUtilEDInitializeEDVisitsParams = "@ReportID uniqueidentifier, @ReportYear varchar(4), @Hospitals IDsTableType READONLY, @IPDataset IDsTableType READONLY, @EDDataset IDsTableType READONLY, @RegionType nvarchar(50)";

		#endregion spUtilEDInitializeEDVisits

		#region spUtilEDAddHospitalTypes

		//        private const string spUtilEDAddHospitalTypes = @"
		//            /*
		//             *      Name:           spUtilEDAddHospitalTypes
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        InpatientReportGenerator.cs
		//             *		DescrEDtion:    Add hospital type to prep table
		//             */
		//
		//            WITH
		//                HospitalTypes AS
		//                (
		//                    SELECT ParentTable.Hospital_Id AS HospitalID,
		//                        HospitalCategoryID =
		//                            STUFF((
		//                                    SELECT ','+ CAST(SubTable.Category_Id AS NVARCHAR(MAX))
		//                                    FROM Hospitals_HospitalCategories SubTable
		//                                    WHERE SubTable.Hospital_Id = ParentTable.Hospital_Id
		//                                    FOR XML PATH('') 
		//                                ), 1, 1,'')
		//                    FROM Hospitals_HospitalCategories ParentTable
		//                )
		//            UPDATE Temp_UtilED_Prep
		//            SET Temp_UtilED_Prep.HospitalType = ISNULL(HospitalTypes.HospitalCategoryID , '')
		//            FROM Temp_UtilED_Prep
		//                LEFT JOIN HospitalTypes ON Temp_UtilED_Prep.HospitalID = HospitalTypes.HospitalID
		//            WHERE ID = @ReportID
		//        ";

		//        private string spUtilEDAddHospitalTypesParams = "@ReportID uniqueidentifier";

		#endregion spUtilEDAddHospitalTypes

		#region spUtilEDGenSummary

		//        private const string spUtilEDGenSummary = @"
		//            /*
		//             *      Name:           spUtilEDGenSummary
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *		Description:    Copy the summary data into the summary table.
		//             */
		//            
		//            INSERT INTO dbo.Temp_UtilED_Summary
		//                SELECT @ReportID
		//                        ,HospitalID      -- HospitalID
		//                        ,RegionID        -- RegID
		//                        ,CountyID        -- CountyID
		//                        ,Zip             -- Zip Code
		//                        ,HospitalType    -- Hospital Type
		//                        ,CCSID           -- CSSDX
		//                        ,COUNT(*)        -- NumEDVisits
		//                        ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp
		//                        ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd
		//                        ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp
		//            FROM Temp_UtilED_Prep
		//            WHERE ID = @ReportID
		//                GROUP BY HospitalID, RegionID, CountyID, Zip, HospitalType, CCSID
		//                ORDER BY CCSID;
		//        ";

		//        private string spUtilEDGenSummaryParams = "@ReportID uniqueidentifier";

		#endregion spUtilEDGenSummary

		#region spUtilEDGenStratAllCombined

		//        private const string spUtilEDGenStratAllCombined = @"
		//            /*
		//             *      Name:           spUtilEDGenStratAllCombined
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *		Description:    Copy the detailed data for all data combined into the summary table.
		//             */
		//
		//            INSERT INTO dbo.Temp_UtilED_Details
		//                SELECT @ReportID
		//                        ,HospitalID      -- HospitalID
		//                        ,RegionID        -- RegID
		//                        ,CountyID        -- CountyID
		//                        ,Zip             -- Zip Code
		//                        ,HospitalType    -- Hospital Type
		//                        ,CCSID           -- CSSDX
		//                        ,0               -- CatID
		//                        ,0               -- CatVal
		//                        ,COUNT(*)        -- NumEDVisits
		//                        ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp
		//                        ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd
		//                        ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp
		//            FROM Temp_UtilED_Prep
		//            WHERE ID = @ReportID
		//                GROUP BY HospitalID, RegionID, CountyID, Zip, HospitalType, CCSID;
		//        ";

		//        private string spUtilEDGenStratAllCombinedParams = "@ReportID uniqueidentifier";

		#endregion spUtilEDGenStratAllCombined

		#region spUtilEDGenStratAge

		//        private const string spUtilEDGenStratAge = @"
		//            /*
		//             *      Name:           spUtilEDGenStratAge
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *		Description:    Copy the detailed data for age into the summary table.
		//             */
		//
		//            INSERT INTO dbo.Temp_UtilED_Details
		//                SELECT @ReportID
		//                        ,HospitalID      -- HospitalID
		//                        ,RegionID        -- RegID
		//                        ,CountyID        -- CountyID
		//                        ,Zip             -- Zip Code
		//                        ,HospitalType    -- Hospital Type
		//                        ,CCSID           -- CSSDX
		//                        ,1               -- CatID
		//                        ,Age             -- CatVal
		//                        ,COUNT(*)        -- NumEDVisits
		//                        ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp
		//                        ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd
		//                        ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp
		//            FROM Temp_UtilED_Prep
		//            WHERE ID = @ReportID
		//                GROUP BY HospitalID, RegionID, CountyID, Zip, HospitalType, CCSID, Age;
		//        ";

		//        private string spUtilEDGenStratAgeParams = "@ReportID uniqueidentifier";

		#endregion spUtilEDGenStratAge

		#region spUtilEDGenStratSex

		//        private const string spUtilEDGenStratSex = @"
		//            /*
		//             *      Name:           spUtilEDGenStratSex
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *		Description:    Copy the detailed data for sex into the summary table.
		//             */
		//
		//            INSERT INTO dbo.Temp_UtilED_Details
		//                SELECT @ReportID
		//                        ,HospitalID      -- HospitalID
		//                        ,RegionID        -- RegID
		//                        ,CountyID        -- CountyID
		//                        ,Zip             -- Zip Code
		//                        ,HospitalType    -- Hospital Type
		//                        ,CCSID           -- CSSDX
		//                        ,2               -- CatID
		//                        ,Sex             -- CatVAL
		//                        ,COUNT(*)        -- NumEDVisits
		//                        ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp
		//                        ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd
		//                        ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp
		//            FROM Temp_UtilED_Prep
		//            WHERE ID = @ReportID
		//                GROUP BY HospitalID, RegionID, CountyID, Zip, HospitalType, CCSID, Sex;
		//        ";

		//        private string spUtilEDGenStratSexParams = "@ReportID uniqueidentifier";

		#endregion spUtilEDGenStratSex

		#region spUtilEDGenStratPrimaryPayer

		//        private const string spUtilEDGenStratPrimaryPayer = @"
		//            /*
		//             *      Name:           spUtilEDGenStratPrimaryPayer
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *		Description:    Copy the detailed data for primary payer into the summary table.
		//             */
		//
		//            INSERT INTO dbo.Temp_UtilED_Details
		//                SELECT @ReportID
		//                        ,HospitalID      -- HospitalID
		//                        ,RegionID		  -- RegID
		//                        ,CountyID        -- CountyID
		//                        ,Zip             -- Zip Code
		//                        ,HospitalType    -- Hospital Type
		//                        ,CCSID			  -- CSSDX
		//                        ,3				  -- CatID
		//                        ,PrimaryPayer	  -- CatVAL
		//                        ,COUNT(*)		  -- NumEDVisits
		//                        ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp
		//                        ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd
		//                        ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp
		//            FROM Temp_UtilED_Prep
		//            WHERE ID = @ReportID
		//                GROUP BY HospitalID, RegionID, CountyID, Zip, HospitalType, CCSID, PrimaryPayer;
		//        ";

		//        private string spUtilEDGenStratPrimaryPayerParams = "@ReportID uniqueidentifier";

		#endregion spUtilEDGenStratPrimaryPayer

		#region spUtilEDGenStratRace

		//        private const string spUtilEDGenStratRace = @"
		//            /*
		//             *      Name:           spUtilEDGenStratRace
		//             *      Version:        1.0
		//             *      Last Updated:   8/29/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *		Description:    Copy the detailed data for race into the summary table.
		//             */
		//
		//            INSERT INTO dbo.Temp_UtilED_Details
		//                SELECT @ReportID
		//                        ,HospitalID      -- HospitalID
		//                        ,RegionID        -- RegID
		//                        ,CountyID        -- CountyID
		//                        ,Zip             -- Zip Code
		//                        ,HospitalType    -- Hospital Type
		//                        ,CCSID           -- CSSDX
		//                        ,4               -- CatID
		//                        ,Race            -- CatVAL
		//                        ,COUNT(*)        -- NumEDVisits
		//                        ,COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END)                    -- NumAdmitHosp
		//                        ,COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedEd
		//                        ,COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END)     -- DiedHosp
		//            FROM Temp_UtilED_Prep
		//            WHERE ID = @ReportID
		//                GROUP BY HospitalID, RegionID, CountyID, Zip, HospitalType, CCSID, Race;
		//        ";

		//        private string spUtilEDGenStratRaceParams = "@ReportID uniqueidentifier";

		#endregion spUtilEDGenStratRace


		#region spUtilEDGetSummaryDataByCCSID

		//        private const string spUtilEDGetSummaryDataByCCSID = @"
		//            /*
		//             *      Name:           spUtilEDGetSummaryDataByCCSID
		//             *      Version:        1.0
		//             *      Last Updated:   5/14/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *      Description:    Used to get summary ED data by CCSID.
		//             */
		//
		//            CREATE TABLE #Temp_Combined
		//            (
		//                NumEdVisits int null,
		//                NumAdmitHosp int null,
		//                DiedEd int null,
		//                DiedHosp int null
		//            )
		//
		//            CREATE TABLE #Temp_List
		//            (
		//                HospitalID int,
		//                RegionID int null,
		//                CountyID int null,
		//                Zip nvarchar(12),
		//                HospitalType nvarchar(max),
		//                NumEdVisits int null,
		//                NumAdmitHosp int null,
		//                DiedEd int null,
		//                DiedHosp int null
		//            )
		//
		//            -- One hospital's data per row for all CCSDX conditions combined.
		//            -- Note: Should only be run once, so didn't bother setting up a specific table / query for this data.
		//            --       This would have just been extra overhead up front to make the final query run faster.
		//            IF @CCSID = '0'
		//                BEGIN
		//                    -- Get the national totals.
		//                    SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr
		//                    FROM      dbo.Base_EDNationalTotals
		//                    WHERE     CCSID = 0
		//
		//                    -- Get all the records combined.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM      dbo.Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID
		//
		//                    -- Individial hospital listings
		//                    INSERT INTO #Temp_List
		//                    SELECT    HospitalID, RegionID, CountyID, Zip, HospitalType, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM      dbo.Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID
		//                    GROUP BY  HospitalID, RegionID, CountyID, Zip, HospitalType
		//                END
		//
		//            -- One hospital's data per row for one CCSDX condition.
		//            ELSE
		//                BEGIN
		//                    -- Get the national totals.
		//                    SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr
		//                    FROM      dbo.Base_EDNationalTotals
		//                    WHERE     CCSID = @CCSID
		//
		//                    -- Get all the records combined.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM      dbo.Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND CCSID = @CCSID
		//
		//                    -- Individial hospital listings
		//                    INSERT INTO #Temp_List
		//                    SELECT    HospitalID, RegionID, CountyID, Zip, HospitalType, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM      dbo.Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND CCSID = @CCSID
		//                    GROUP BY  HospitalID, RegionID, CountyID, Zip, HospitalType
		//                END
		//
		//
		//            -- Supress the data
		//            UPDATE #Temp_Combined
		//            SET NumEdVisits = -2
		//            WHERE NumEdVisits < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET NumAdmitHosp = -2
		//            WHERE NumAdmitHosp < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET DiedED = -2
		//            WHERE DiedED < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET DiedHosp = -2
		//            WHERE DiedHosp < @Suppression
		//
		//
		//            UPDATE #Temp_List
		//            SET NumEdVisits = -2
		//            WHERE NumEdVisits < @Suppression
		//
		//            UPDATE #Temp_List
		//            SET NumAdmitHosp = -2
		//            WHERE NumAdmitHosp < @Suppression
		//
		//            UPDATE #Temp_List
		//            SET DiedED = -2
		//            WHERE DiedED < @Suppression
		//
		//            UPDATE #Temp_List
		//            SET DiedHosp = -2
		//            WHERE DiedHosp < @Suppression
		//
		//
		//            -- Return the data tables.
		//            SELECT * FROM #Temp_Combined
		//            SELECT * FROM #Temp_List
		//            ";

		//        private string spUtilEDGetSummaryDataByCCSIDParams = "@ReportID uniqueidentifier, @CCSID nvarchar(25) = '0', @Suppression decimal(19,0) = 10.0";

		#endregion spUtilEDGetSummaryDataByCCSID

		#region spUtilEDGetSummaryDataByHospitalID

		//        private const string spUtilEDGetSummaryDataByHospitalID = @"
		//            /*
		//             *      Name:           spUtilEDGetSummaryDataByHospitalID
		//             *      Version:        1.0
		//             *      Last Updated:   5/14/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *      Description:    Used to get summary ED data by HospitalID.
		//             */
		//
		//            CREATE TABLE #Temp_Combined
		//            (
		//                NumEdVisits int null,
		//                NumAdmitHosp int null,
		//                DiedEd int null,
		//                DiedHosp int null
		//            )
		//
		//            CREATE TABLE #Temp_List
		//            (
		//                CCSID int,
		//                NumEdVisits int null,
		//                NumAdmitHosp int null,
		//                DiedEd int null,
		//                DiedHosp int null
		//            )
		//
		//
		//            -- Gets CCSDX records for hospitals. Details records has one CCSDX record per row.
		//
		//            -- One hospital's data per row for one CCSDX condition.
		//            IF @HospitalID <> '0'
		//                BEGIN
		//                    -- Get all the records combined for the hospital.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND HospitalID = @HospitalID
		//
		//                    -- Get a list of CCSDX records for all hospitals.
		//                    INSERT INTO #Temp_List
		//                    SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND HospitalID = @HospitalID
		//                    GROUP BY  CCSID
		//                END
		//
		//            -- One regions's data per row for one CCSDX condition.
		//            ELSE IF @RegionID <> '0'
		//                BEGIN
		//                    -- Get all the records combined for the region.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND RegionID = @RegionID
		//
		//                    -- Get a list of CCSDX records for all hospitals in one region.
		//                    INSERT INTO #Temp_List
		//                    SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND RegionID = @RegionID
		//                    GROUP BY  CCSID
		//                END
		//
		//            -- One zip codes' data per row for one CCSDX condition.
		//            ELSE IF @Zip <> '0'
		//                BEGIN
		//                    -- Get all the records combined for the region.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND Zip = @Zip
		//
		//                    -- Get a list of CCSDX records for all hospitals in one zip code.
		//                    INSERT INTO #Temp_List
		//                    SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND Zip = @Zip
		//                    GROUP BY  CCSID
		//                END
		//
		//            -- One county's data per row for one CCSDX condition.
		//            ELSE IF @CountyID <> '0'
		//                BEGIN
		//                    -- Get all the records combined for the region.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND CountyID = @CountyID
		//
		//                    -- Get a list of CCSDX records for all hospitals in one county.
		//                    INSERT INTO #Temp_List
		//                    SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID AND CountyID = @CountyID
		//                    GROUP BY  CCSID
		//                END
		//
		//            -- One county's data per row for one CCSDX condition.
		//            ELSE IF @HospitalCategoryID <> '0'
		//                BEGIN
		//                    -- Get all the records combined for the region.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT        ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM          Temp_UtilED_Summary
		//                    RIGHT JOIN    Hospitals_HospitalCategories HospCat ON
		//                                    Temp_UtilED_Summary.HospitalID = HospCat.Hospital_Id
		//                    WHERE         ID = @ReportID AND Category_Id = @HospitalCategoryID
		//
		//                    -- Get a list of CCSDX records for all hospitals in one county.
		//                    INSERT INTO #Temp_List
		//                    SELECT        CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM          Temp_UtilED_Summary
		//                    RIGHT JOIN    Hospitals_HospitalCategories HospCat ON
		//                                    Temp_UtilED_Summary.HospitalID = HospCat.Hospital_Id
		//                    WHERE         ID = @ReportID AND Category_Id = @HospitalCategoryID
		//                    GROUP BY      CCSID
		//                END
		//
		//            ELSE --IF @HospitalID = '0'
		//                BEGIN
		//                    -- Get all the records combined.
		//                    INSERT INTO #Temp_Combined
		//                    SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID
		//
		//                    -- Get a list of CCSDX records for all hospitals combined.
		//                    INSERT INTO #Temp_List
		//                    SELECT    CCSID, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                    FROM      Temp_UtilED_Summary
		//                    WHERE     ID = @ReportID
		//                    GROUP BY  CCSID
		//                END
		//
		//
		//            -- Supress the data
		//            UPDATE #Temp_Combined
		//            SET NumEdVisits = -2
		//            WHERE NumEdVisits < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET NumAdmitHosp = -2
		//            WHERE NumAdmitHosp < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET DiedED = -2
		//            WHERE DiedED < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET DiedHosp = -2
		//            WHERE DiedHosp < @Suppression
		//
		//
		//            UPDATE #Temp_List
		//            SET NumEdVisits = -2
		//            WHERE NumEdVisits < @Suppression
		//
		//            UPDATE #Temp_List
		//            SET NumAdmitHosp = -2
		//            WHERE NumAdmitHosp < @Suppression
		//
		//            UPDATE #Temp_List
		//            SET DiedED = -2
		//            WHERE DiedED < @Suppression
		//
		//            UPDATE #Temp_List
		//            SET DiedHosp = -2
		//            WHERE DiedHosp < @Suppression
		//
		//
		//            -- Get the national totals.
		//            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr
		//            FROM      Base_EDNationalTotals
		//            WHERE     CCSID = 0
		//
		//            SELECT * FROM #Temp_Combined
		//
		//            SELECT * FROM #Temp_List
		//            ";

		//        private string spUtilEDGetSummaryDataByHospitalIDParams = "@ReportID uniqueidentifier, @HospitalID nvarchar(25) = '0', @RegionID int = 0, " +
		//                                                                  "@CountyID int = 0, @HospitalCategoryID int = 0, @Zip nvarchar(12) = '0', @Suppression decimal(19,0) = 10.0";

		#endregion spUtilEDGetSummaryDataByHospitalID

		#region spUtilEDGetDetailData

		//        private const string spUtilEDGetDetailData = @"
		//            /*
		//             *      Name:           spUtilEDGetDetailData
		//             *      Version:        1.0
		//             *      Last Updated:   5/14/14
		//             *      Used In:        TreatAndReleaseReportGenerator.cs
		//             *      Description:    Used to get detailed ED data.
		//             */
		//
		//            -- Setup temporary tables for suppression.
		//            -- Note: Have to do it here vs. the report temp tables in the database because we do grouping of results in here.
		//            CREATE TABLE #Temp_Combined
		//            (
		//                NumEdVisits int null,
		//                NumAdmitHosp int null,
		//                DiedEd int null,
		//                DiedHosp int null
		//            )
		//
		//            CREATE TABLE #Temp_Stratified
		//            (
		//                CatID int,
		//                CatVal int,
		//                Name varchar(max) null,
		//                NumEdVisits int null,
		//                NumAdmitHosp int null,
		//                DiedEd int null,
		//                DiedHosp int null
		//            )
		//
		//
		//            IF @CCSID = '0'
		//                BEGIN
		//                    -- Get the details page for all CCSDX conditions at one hospital.
		//                    IF @HospitalID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND HospitalID = @HospitalID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND HospitalID = @HospitalID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One regions's data per row for one CCSDX condition.
		//                    ELSE IF @RegionID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND RegionID = @RegionID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND RegionID = @RegionID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One zip codes' data per row for one CCSDX condition.
		//                    ELSE IF @Zip <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND Zip = @Zip
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND Zip = @Zip
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One county's data per row for one CCSDX condition.
		//                    ELSE IF @CountyID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND CountyID = @CountyID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND CountyID = @CountyID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One county's data per row for one CCSDX condition.
		//                    ELSE IF @HospitalCategoryID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            RIGHT JOIN    Hospitals_HospitalCategories HospCat ON
		//                                            Temp_UtilED_Summary.HospitalID = HospCat.Hospital_Id
		//                            WHERE         ID = @ReportID AND Category_Id = @HospitalCategoryID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT        CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM          Temp_UtilED_Details
		//                                RIGHT JOIN    Hospitals_HospitalCategories HospCat ON
		//                                                Temp_UtilED_Details.HospitalID = HospCat.Hospital_Id
		//                                WHERE         ID = @ReportID AND Category_Id = @HospitalCategoryID
		//                                GROUP BY      CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- Get the details page for all CCSDX conditions across all hospitals.
		//                    ELSE --IF @HospitalID = '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    END
		//
		//            ELSE IF @CCSID <> '0'
		//                BEGIN
		//                    -- Get the details page for one CCSDX condition at one hospital.
		//                    IF @HospitalID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND CCSID = @CCSID AND HospitalID = @HospitalID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND CCSID = @CCSID AND HospitalID = @HospitalID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One regions's data per row for one CCSDX condition.
		//                    ELSE IF @RegionID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND CCSID = @CCSID AND RegionID = @RegionID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND CCSID = @CCSID AND RegionID = @RegionID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One zip codes' data per row for one CCSDX condition.
		//                    ELSE IF @Zip <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND CCSID = @CCSID AND Zip = @Zip
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND CCSID = @CCSID AND Zip = @Zip
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One county's data per row for one CCSDX condition.
		//                    ELSE IF @CountyID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND CCSID = @CCSID AND CountyID = @CountyID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND CCSID = @CCSID AND CountyID = @CountyID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- One county's data per row for one CCSDX condition.
		//                    ELSE IF @HospitalCategoryID <> '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            RIGHT JOIN    Hospitals_HospitalCategories HospCat ON
		//                                            Temp_UtilED_Summary.HospitalID = HospCat.Hospital_Id
		//                            WHERE         ID = @ReportID AND CCSID = @CCSID AND Category_Id = @HospitalCategoryID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT        CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM          Temp_UtilED_Details
		//                                RIGHT JOIN    Hospitals_HospitalCategories HospCat ON
		//                                                Temp_UtilED_Details.HospitalID = HospCat.Hospital_Id
		//                                WHERE         ID = @ReportID AND CCSID = @CCSID AND Category_Id = @HospitalCategoryID
		//                                GROUP BY      CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//
		//                    -- Get the details page for one CCSDX condition across all hospitals.
		//                    ELSE --IF @HospitalID = '0'
		//                        BEGIN
		//                            -- Get All The Records Combined.
		//                            INSERT INTO #Temp_Combined
		//                            SELECT    ISNULL(SUM(NumEdVisits), 0) AS NumEdVisits, ISNULL(SUM(NumAdmitHosp), 0) AS NumAdmitHosp, ISNULL(SUM(DiedEd), 0) AS DiedEd, ISNULL(SUM(DiedHosp), 0) AS DiedHosp
		//                            FROM      Temp_UtilED_Summary
		//                            WHERE     ID = @ReportID AND CCSID = @CCSID AND CCSID = @CCSID
		//
		//                            INSERT INTO #Temp_Stratified
		//                            SELECT D.CatID, D.CatVal, D.Name, ISNULL(C.NumEdVisits,0) AS NumEdVisits, ISNULL(C.NumAdmitHosp,0) AS NumAdmitHosp, ISNULL(C.DiedEd,0) AS DiedEd, ISNULL(C.DiedHosp,0) AS DiedHosp
		//                            FROM Base_StratificationVals D LEFT JOIN 
		//                            (
		//                                SELECT    CatID, CatVal, SUM(NumEdVisits) AS NumEdVisits, SUM(NumAdmitHosp) AS NumAdmitHosp, SUM(DiedEd) AS DiedEd, SUM(DiedHosp) AS DiedHosp
		//                                FROM      Temp_UtilED_Details
		//                                WHERE     ID = @ReportID AND CCSID = @CCSID AND CCSID = @CCSID
		//                                GROUP BY  CatID, CatVal
		//                            ) C ON D.CatID = C.CatID AND D.CatVal = C.CatVal
		//                        END
		//                END
		//
		//            -- Supress the data
		//            UPDATE #Temp_Combined
		//            SET NumEdVisits = -2
		//            WHERE NumEdVisits < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET NumAdmitHosp = -2
		//            WHERE NumAdmitHosp < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET DiedED = -2
		//            WHERE DiedED < @Suppression
		//
		//            UPDATE #Temp_Combined
		//            SET DiedHosp = -2
		//            WHERE DiedHosp < @Suppression
		//
		//
		//            UPDATE #Temp_Stratified
		//            SET NumEdVisits = -2
		//            WHERE NumEdVisits < @Suppression
		//
		//            UPDATE #Temp_Stratified
		//            SET NumAdmitHosp = -2
		//            WHERE NumAdmitHosp < @Suppression
		//
		//            UPDATE #Temp_Stratified
		//            SET DiedED = -2
		//            WHERE DiedED < @Suppression
		//
		//            UPDATE #Temp_Stratified
		//            SET DiedHosp = -2
		//            WHERE DiedHosp < @Suppression
		//
		//
		//            -- Return the data tables.
		//
		//            -- Get the national totals.
		//            SELECT    NumEdVisits, NumEdVisitsStdErr, NumAdmitHosp, NumAdmitHospStdErr, DiedEd, DiedEdStdErr, DiedHosp, DiedHospStdErr
		//            FROM      Base_EDNationalTotals
		//            WHERE     CCSID = @CCSID
		//
		//            SELECT * FROM #Temp_Combined
		//            SELECT * FROM #Temp_Stratified
		//
		//
		//            DROP TABLE #Temp_Combined
		//            DROP TABLE #Temp_Stratified
		//        ";

		//        private string spUtilEDGetDetailDataParams = "@ReportID uniqueidentifier, @CCSID nvarchar(25) = '0', @HospitalID nvarchar(25) = '0', @RegionID int = 0, " +
		//                                                     "@CountyID int = 0, @HospitalCategoryID int = 0, @Zip nvarchar(12) = '0', @Suppression decimal(19,0) = 10.0";

		#endregion spUtilEDGetDetailData

		#endregion SProcs

		#region SQLCode

		#region MeanSQL
		/// <summary>
		/// The mean SQL
		/// </summary>
		private const string MeanSql = @"
	        INSERT INTO %%DestTableName%%
            SELECT      
		        '%%ReportID%%',
		        %%MeanSelect%%,
		        %%UtilID%% AS CCSID,
		        %%CatID%% AS CatID,
		        %%CatVal%% AS CatVal,

	            COUNT(*) AS NumEdVisits,
	            COUNT(CASE WHEN DataSource = 1 THEN 1 ELSE NULL END) AS NumAdmitHosp,
	            COUNT(CASE WHEN (DataSource = 2 AND Died = 1) THEN 1 ELSE NULL END) AS DiedEd,
	            COUNT(CASE WHEN (DataSource = 1 AND Died = 1) THEN 1 ELSE NULL END) AS DiedHosp
	        FROM %%FromTable%%
	        WHERE ID = '%%ReportID%%'
	        %%GroupBy%%;
        ";
		#endregion MeanSQL

		#endregion SQLCode

		/// <summary>
		/// Called when [installed].
		/// </summary>
		private void OnInstalled()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            //base.OnWingAdded();

            string[] scriptFiles ={
                                        "Table-TempUtilEDCounty.sql",
                                        "Table-TempUtilEDHospital.sql",
                                        "Table-TempUtilEDHospitalType.sql",
                                        "Table-TempUtilEDPrep.sql",
                                        "Table-TempUtilEDRegion.sql",
                                        "Sproc-spUtilEDGetDetailData.sql",
                                        "Sproc-spUtilEDGetSummaryDataByClinical.sql",
                                        "Sproc-spUtilEDGetSummaryDataByGeo.sql",
                                        "Sproc-spUtilEDGetRecordsInEDTarget.sql",
                                        "Sproc-spUtilEDGetRecordsInIPTarget.sql",
                                        "Sproc-spUtilEDInitializeEDVisits.sql",
                                        "Sproc-spUtilEDInitializeIPVisits.sql",
                                        "Sproc-spUtilEDUpdateHospitalType.sql",
                                        "Sproc-spUtilEDUpdateZip.sql"
                                  };

            RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\UtilizationED"), scriptFiles);
        }

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
        {
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Emergency Discharge reports" });
            // Following should only run once, but this procedure is running every time on application startup.
            // OnInstalled();
        }

		/// <summary>
		/// Validates the dependencies.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <param name="validationResults">The validation results.</param>
		/// <returns></returns>
		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            return true;
        }

		/// <summary>
		/// Generates the report.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <param name="publishTask">The publish task.</param>
		public override void GenerateReport(Website website, PublishTask publishTask = PublishTask.Full)
        {
            if (publishTask == PublishTask.PreviewOnly)
            {
                // Do nothing for previews
                return;
            }

            CurrentWebsite = website;

            // Re-indexing Treat and Release target table
            ReIndexTargetTableIndexes();

            //         System.IO.Directory.CreateDirectory(Path.Combine(website.OutPutDirectory, "Data", "InpatientUtilization"));
            var hospitalIDs = website.Hospitals.Where(h => !h.Hospital.IsArchived && !h.Hospital.IsDeleted).Select(wh => wh.Hospital.Id).ToList();
            string hospitalIdList = "";
            foreach (var item in hospitalIDs)
            {
                if (!string.IsNullOrEmpty(hospitalIdList))
                {
                    hospitalIdList += ",";
                }
                hospitalIdList += item.ToString();
            }
            int ipContentItemRecord;
            int edContentItemRecord;
            foreach (WebsiteDataset dataSet in website.Datasets)
            {
                switch (dataSet.Dataset.ContentType.Name.ToUpper())
                {
                    case "INPATIENT DISCHARGE":
                        ipContentItemRecord = dataSet.Dataset.Id;
                        break;
                    case "ED TREAT AND RELEASE":
                        edContentItemRecord = dataSet.Dataset.Id;
                        break;
                }
            }

            decimal edVisitsSuppression = GetSuppression("ED-01");
            decimal admitHospitalSuppression = GetSuppression("ED-02");
            decimal diedEdSuppression = GetSuppression("ED-03");
            decimal diedHospitalSuppression = GetSuppression("ED-04");

            string regionType = CurrentWebsite.RegionTypeContext;
            switch (CurrentWebsite.RegionTypeContext.ToUpper())
            {
                case "HEALTHREFERRALREGION":
                    regionType = "HealthReferralRegion_Id";
                    break;
                case "HOSPITALSERVICEAREA":
                    regionType = "HospitalServiceArea_Id";
                    break;
                case "CUSTOMREGION":
                    regionType = "CustomRegion_Id";
                    break;
            }
            var ipDatasets = website.Datasets.Where(d => d.Dataset.ContentType.Name.ToUpper() == "INPATIENT DISCHARGE").ToList();
            var edDatasets = website.Datasets.Where(d => d.Dataset.ContentType.Name.ToUpper() == "ED TREAT AND RELEASE").ToList();
            foreach (var edDataset in edDatasets)
            {
                LogMessage(string.Format("Generating {0} Report Data for year {1}", "Emergency Discharge", edDataset.Dataset.ReportingYear));
                edContentItemRecord = edDataset.Dataset.Id;

                var ipDs = ipDatasets.FirstOrDefault(d => d.Dataset.ReportingYear == edDataset.Dataset.ReportingYear);
                ipContentItemRecord = ipDs == null ? 0 : ipDs.Dataset.Id;

                var process = new Process();
                var psi = new ProcessStartInfo();
                const string fileName = @"Modules\Generators\EDGenerator\EDGenerator.exe";
                if (!File.Exists(fileName)) return;
                psi.FileName = fileName;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(
                    "-d \"{0}\" -c \"{1}\" -h \"{2}\" -i {3} -r {4} -l 1 -e {5} -ED_VISITS_SUPPRESSION {6} -ADMIT_HOSPITAL_SUPPRESSION {7} -DIED_ED_SUPPRESSION {8} -DIED_HOSPITAL_SUPPRESSION {9} -o {10}",
                    Path.Combine(website.OutPutDirectory, "Data", "EmergencyDischarge",
                        edDataset.Dataset.ReportingYear),
                    MonahrqConfiguration.SettingsGroup.MonahrqSettings().EntityConnectionSettings.ConnectionString,
                    hospitalIdList,
                    ipContentItemRecord,
                    regionType,
                    edContentItemRecord,
                    edVisitsSuppression,
                    admitHospitalSuppression,
                    diedEdSuppression,
                    diedHospitalSuppression,
                    (website.UtilizationReportCompression.HasValue && website.UtilizationReportCompression.Value));

                process.StartInfo = psi;
                process.Start();
                do
                {
                    var logMessage = process.StandardOutput.ReadLineAsync().Result;
                    if (!string.IsNullOrEmpty(logMessage))
                        LogMessage(logMessage);

                } while (!process.HasExited); //.WaitForExit(43200000)
                process.Close();
                process.Dispose();

                var tempPath = Path.GetTempPath() + "Monahrq\\Generators\\EDGenerator\\";
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }

            /*
            try
            {
                if (publishTask == PublishTask.PreviewOnly)
                {
                    // Do nothing for previews
                    return;
                }

                // Start the timer.
                DateTime groupStart = DateTime.Now;

                // This is the one that should be called first.
                base.GenerateReport(website);

                // Initialize the data for this report.
                InitializeReportData();

                // Make sure the base directories are created.
                CreateBaseDirectories();

                // Generate the json files for the report.
                GenerateJsonFiles();

                // Write out the complete time for generation.
                Logger.Write(string.Format("Utilization ED - Generation completed in {0:c}", DateTime.Now - groupStart));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            */
        }

		/// <summary>
		/// Res the index target table indexes.
		/// </summary>
		private void ReIndexTargetTableIndexes()
        {
            Logger.Write("Start re-indexing Treat and Release target table: Targets_TreatAndReleaseTargets");

            var query = @"ALTER INDEX ALL ON [dbo].[Targets_TreatAndReleaseTargets] REBUILD;";

            if (CurrentWebsite.Datasets.Any(ds => ds.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")) && 
                CurrentWebsite.Reports.Any(rpt => rpt.Report.SourceTemplate.RptId.In(new [] { "4E94E281-8E7E-4B02-8567-72B4FA239F9E", "2AAF7FBA-7102-4C66-8598-A70597E2F825", "47426256-5F4F-4996-8F4A-3A344E39D90E", "2AAF7FBA-7102-4C66-8598-A70597E2F824" } )))
                query = @"ALTER INDEX ALL ON [dbo].[Targets_InpatientTargets] REBUILD;" + Environment.NewLine + query;

            base.ExecuteNonQuery(query);

            Logger.Write("Finished re-indexing Treat and Release target table: Targets_TreatAndReleaseTargets");

        }

		/// <summary>
		/// Initializes the report data.
		/// </summary>
		private void InitializeReportData()
        {
            try
            {
                #region Get base information about the website - hospitals, measures, datasets, etc.

                // Get the needed DataSets
                IPDatasetIDs = new DataTable();
                IPDatasetIDs.Columns.Add("ID", typeof(int));
                EDDatasetIDs = new DataTable();
                EDDatasetIDs.Columns.Add("ID", typeof(int));

                foreach (WebsiteDataset dataSet in CurrentWebsite.Datasets)
                {
                    switch (dataSet.Dataset.ContentType.Name)
                    {
                        case "Inpatient Discharge":
                            // Add a new IP dataset
                            IPDatasetIDs.Rows.Add((dataSet.Dataset.Id));
                            break;
                        case "ED Treat And Release":
                            // Add a new ED dataset
                            EDDatasetIDs.Rows.Add((dataSet.Dataset.Id));
                            break;
                    }
                }

                #endregion Get base information about the website - hospitals, measures, datasets, etc.

                #region Generate the specific data for this report.

                DateTime groupStart;
                TimeSpan groupTimeDiff;

                // Save a report ID for this particular report run.
                ReportID = Guid.NewGuid().ToString();

                var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
                var hospitalIDParams = new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("@Hospitals", HospitalIds) };

                // Get the lists needed to generate the json files.
                CCSList = new DataTable();
                CCSList = RunSprocReturnDataTable("spGetDXCCS");

                var selectedRegionContextType = new KeyValuePair<string, object>("@RegionType", configService.HospitalRegion.SelectedRegionType.Name);

                HospitalList = new DataTable();
                HospitalList = RunSprocReturnDataTable("spGetHospitals", new KeyValuePair<string, object>("@Hospitals", HospitalIds), selectedRegionContextType);

                CountyList = new DataTable();
                CountyList = RunSprocReturnDataTable("spGetHospitalCounties", hospitalIDParams);

                RegionList = new DataTable();
                RegionList = RunSprocReturnDataTable("spGetHospitalRegions", new KeyValuePair<string, object>("@Hospitals", HospitalIds), selectedRegionContextType);

                HospitalTypeList = new DataTable();
                HospitalTypeList = RunSprocReturnDataTable("spGetHospitalTypes", hospitalIDParams);

                // Get the number of rows in the target tables
                DataTable IPRows = RunSprocReturnDataTable("spUtilEDGetRecordsInIPTarget",
                        new KeyValuePair<string, object>("@Hospitals", HospitalIds),
                        new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs));
                if (IPRows.Rows.Count == 1)
                {
                    Logger.Write(string.Format("Utilization IP - Rows in IP Dataset: {0:n0}", IPRows.Rows[0]["IPRows"]));
                }

                // Get the number of rows in the target tables
                DataTable EDRows = RunSprocReturnDataTable("spUtilEDGetRecordsInEDTarget",
                        new KeyValuePair<string, object>("@Hospitals", HospitalIds),
                        new KeyValuePair<string, object>("@EDDataset", EDDatasetIDs));
                if (EDRows.Rows.Count == 1)
                {
                    Logger.Write(string.Format("Utilization IP - Rows in ED Dataset: {0:n0}", EDRows.Rows[0]["EDRows"]));
                }

                // Prep the IP data.
                groupStart = DateTime.Now;
                PrepData();
                groupTimeDiff = DateTime.Now - groupStart;
                Logger.Write(string.Format("Utilization ED - Prepping Data - Prepping data completed in {0:c}", groupTimeDiff));

                // Aggregate the data.
                groupStart = DateTime.Now;
                AggregateData();
                groupTimeDiff = DateTime.Now - groupStart;
                Logger.Write(string.Format("Utilization ED - Aggregating Data - Aggregation completed in {0:c}", groupTimeDiff));

                // Suppress the data.
                groupStart = DateTime.Now;
                LogMessage("Utilization ED - Suppressing Data");
                SuppressData();
                groupTimeDiff = DateTime.Now - groupStart;
                Logger.Write(string.Format("Utilization ED - Suppressing data - Suppression completed in {0:c}", groupTimeDiff));

                #endregion Generate the specific data for this report.
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error initializing report \"{0}\"", this.ActiveReport?.Name);
            }
        }

		/// <summary>
		/// Preps the data.
		/// </summary>
		private void PrepData()
        {
            // Add minimally needed data to prep table

            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var initParams = new KeyValuePair<string, object>[] {
                            new KeyValuePair<string, object>("@ReportID", ReportID),
                            new KeyValuePair<string, object>("@ReportYear", this.CurrentWebsite.ReportedYear),
                            new KeyValuePair<string, object>("@Hospitals", HospitalIds), 
                            new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs),
                            new KeyValuePair<string, object>("@EDDataset", EDDatasetIDs),
                            new KeyValuePair<string, object>("@RegionType", configService.HospitalRegion.SelectedRegionType.Name)
                        };

            DateTime groupStartAll = DateTime.Now;
            EnableDisableTableIndexes(true, "Temp_UtilED_Prep");
            LogMessage("Utilization ED - Prepping Data - Prepping IP Visit data.");
            RunSproc("spUtilEDInitializeIPVisits", "Utilization ED - Prepping Data - Prepping IP visit data.", initParams);
            LogMessage("Utilization ED - Prepping Data - Prepping ED Visit data.");
            RunSproc("spUtilEDInitializeEDVisits", "Utilization ED - Prepping Data - Prepping ED visit data.", initParams);
            EnableDisableTableIndexes(false, "Temp_UtilED_Prep");
            TimeSpan groupTimeDiffAll = DateTime.Now - groupStartAll;
            Logger.Write(string.Format("Utilization ED - Prepping Data - Prepped data for all sections in {0:c}", groupTimeDiffAll));
        }

		/// <summary>
		/// Aggregates the data.
		/// </summary>
		private void AggregateData()
        {
            // Aggregate the data into the temp tables.

            string[] ipTables = { "Combined", "Hospital", "County", "Region", "HospitalType" };
            string[] ipStrats = { "Combined", "Age", "Sex", "PrimaryPayer", "Race" };

            // Disable the temp table indexes.
            EnableDisableTableIndexes(true, "Temp_UtilED_Hospital");
            EnableDisableTableIndexes(true, "Temp_UtilED_County");
            EnableDisableTableIndexes(true, "Temp_UtilED_Region");
            EnableDisableTableIndexes(true, "Temp_UtilED_HospitalType");

            // Loop through the different tables and strat combinations, modify the aggregation script and generate the report data.
            foreach (var ipTable in ipTables)
            {
                foreach (var ipStrat in ipStrats)
                {
                    string finalMeanSql = "";
                    string destTableName = "";
                    string meanSelect = "";
                    string catId = "";
                    string fromTable = "";
                    string groupBy = "";

                    switch (ipTable)
                    {
                        case "Combined":
                            destTableName = "Temp_UtilED_Hospital";
                            meanSelect = "0, 0, 0, '', ''";
                            fromTable = "Temp_UtilED_Prep";
                            break;
                        case "Hospital":
                            destTableName = "Temp_UtilED_Hospital";
                            meanSelect = "Temp_UtilED_Prep.HospitalID, Temp_UtilED_Prep.RegionID, Temp_UtilED_Prep.CountyID, '', ''";
                            fromTable = "Temp_UtilED_Prep";
                            groupBy = "GROUP BY Temp_UtilED_Prep.HospitalID, Temp_UtilED_Prep.RegionID, Temp_UtilED_Prep.CountyID";
                            break;
                        case "County":
                            destTableName = "Temp_UtilED_County";
                            meanSelect = "Temp_UtilED_Prep.CountyID";
                            fromTable = "Temp_UtilED_Prep";
                            groupBy = "GROUP BY Temp_UtilED_Prep.CountyID";
                            break;
                        case "Region":
                            destTableName = "Temp_UtilED_Region";
                            meanSelect = "Temp_UtilED_Prep.RegionID";
                            fromTable = "Temp_UtilED_Prep";
                            groupBy = "GROUP BY Temp_UtilED_Prep.RegionID";
                            break;
                        case "HospitalType":
                            destTableName = "Temp_UtilED_HospitalType";
                            meanSelect = "Hosp2HospCat.Category_Id";
                            fromTable = "Hospitals_HospitalCategories Hosp2HospCat JOIN Temp_UtilED_Prep ON Hosp2HospCat.Hospital_Id = Temp_UtilED_Prep.HospitalID";
                            groupBy = "GROUP BY Hosp2HospCat.Category_Id";
                            break;
                    }
                    switch (ipStrat)
                    {
                        case "Combined":
                            catId = "0";
                            break;
                        case "Age":
                            catId = "1";
                            break;
                        case "Sex":
                            catId = "2";
                            break;
                        case "PrimaryPayer":
                            catId = "3";
                            break;
                        case "Race":
                            catId = "4";
                            break;
                    }
                    string catVal = (ipStrat == "Combined") ? "0" : "Temp_UtilED_Prep." + ipStrat;
                    if (ipStrat != "Combined")
                    {
                        groupBy = (groupBy.Length == 0 ? "GROUP BY " : groupBy + ", ") + "Temp_UtilED_Prep." + ipStrat;
                    }

                    // Setup the message for this section
                    string message = "Utilization ED - Aggregating Data - ";
                    message = message + ((ipStrat == "Combined") ? "Summary data" : "Stratified data for " + ((ipStrat == "PrimaryPayer") ? "primary payer" : ipStrat.ToLower()));
                    message = message + ((ipTable == "Combined") ? " for all data combined" : " by " + ((ipTable == "HospitalType") ? "hospital type" : ipTable.ToLower()) + ".");
                    LogMessage(message);

                    // Setup the base sql for generating the means.
                    string baseMeanSql = MeanSql
                        .Replace("%%DestTableName%%", destTableName)
                        .Replace("%%MeanSelect%%", meanSelect)
                        .Replace("%%CatID%%", catId)
                        .Replace("%%CatVal%%", catVal)
                        .Replace("%%FromTable%%", fromTable)
                        .Replace("%%ReportID%%", ReportID);

                    // Customize the base means sql for all util types combined.
                    finalMeanSql = baseMeanSql
                        .Replace("%%UtilID%%", "0")
                        .Replace("%%GroupBy%%", groupBy);

                    ExecuteNonQuery(finalMeanSql, "Utilization ED - Aggregating Data - UtilType " + ipTable + " by " + ipStrat);

                    // Customize the base means sql for individual util types.
                    finalMeanSql = baseMeanSql
                        .Replace("%%UtilID%%", "Temp_UtilED_Prep.CCSID")
                        .Replace("%%GroupBy%%", (groupBy.Length == 0 ? "GROUP BY " : groupBy + ", ") + "Temp_UtilED_Prep.CCSID");
                    ExecuteNonQuery(finalMeanSql, "Utilization ED - Aggregating Data - UtilTypeUtil " + ipTable + " by " + ipStrat);
                }
            }

            // Add the zip codes and hospital types to hospital table only.
            var initParams = new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("@ReportID", ReportID) };
            RunSproc("spUtilEDUpdateZip", "Utilization ED - Aggregating Data - Adding zip codes.", initParams);
            RunSproc("spUtilEDUpdateHospitalType", "Utilization ED - Aggregating Data - Adding hospital types.", initParams);

            // Reenable the indexes.
            EnableDisableTableIndexes(false, "Temp_UtilED_Hospital");
            EnableDisableTableIndexes(false, "Temp_UtilED_County");
            EnableDisableTableIndexes(false, "Temp_UtilED_Region");
            EnableDisableTableIndexes(false, "Temp_UtilED_HospitalType");
        }

		/// <summary>
		/// Suppresses the data.
		/// </summary>
		private void SuppressData()
        {
            string[] ipTables = { "Hospital", "County", "Region", "HospitalType" };

            // get suppression values
            decimal edVisitsSuppression = GetSuppression("ED-01");
            decimal admitHospitalSuppression = GetSuppression("ED-02");
            decimal diedEd = GetSuppression("ED-03");
            decimal diedHospital = GetSuppression("ED-04");

            foreach (var ipTable in ipTables)
            {
                string sql = "UPDATE Temp_UtilED_" + ipTable + " SET NumEdVisits = -2 WHERE NumEdVisits > 0 AND NumEdVisits < " + edVisitsSuppression;
                ExecuteNonQuery(sql, "Utilization ED - Suppressing Data - Suppressing NumEdVisits in " + ipTable);

                sql = "UPDATE Temp_UtilED_" + ipTable + " SET NumAdmitHosp = -2 WHERE NumAdmitHosp > 0 AND NumAdmitHosp < " + admitHospitalSuppression;
                ExecuteNonQuery(sql, "Utilization ED - Suppressing Data - Suppressing NumAdmitHosp in " + ipTable);

                sql = "UPDATE Temp_UtilED_" + ipTable + " SET DiedEd = -2 WHERE DiedEd > 0 AND DiedEd < " + diedEd;
                ExecuteNonQuery(sql, "Utilization ED - Suppressing Data - Suppressing DiedEd in " + ipTable);

                sql = "UPDATE Temp_UtilED_" + ipTable + " SET DiedHosp = -2 WHERE DiedHosp > 0 AND DiedHosp < " + diedHospital;
                ExecuteNonQuery(sql, "Utilization ED - Suppressing Data - Suppressing DiedHosp in " + ipTable);
            }
        }

		/// <summary>
		/// Creates the base directories.
		/// </summary>
		private void CreateBaseDirectories()
        {
            try
            {
                BaseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
                if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);

                EDDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "EmergencyDischarge");
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
            catch (Exception ex)
            {
                Logger.Write(ex, "Error creating directories for report \"{0}\"", this.ActiveReport?.Name);
            }
        }

		/// <summary>
		/// Generates the json files.
		/// </summary>
		private void GenerateJsonFiles()
        {
            try
            {
                // Generate any report specific json files

                // Setup the base parameters being passed in.
                KeyValuePair<string, object>[] baseOptions = new KeyValuePair<string, object>[] 
                {
                    new KeyValuePair<string, object>("@ReportID", ReportID),
                };

                string sectionStart = "Utilization ED - Output Data - Starting generation of output files for {0} section.";
                string sectionEnd = "Utilization ED - Output Data - Generated output files for {0} section in {1:c}";

                // Reset the timer in the base file that tracks the json conversion and file write time.
                FileIOTime = TimeSpan.Zero;

                TimeSpan elapsedTime;

                #region DXCCS Output
                //Output the five DXCCS sections
                DateTime groupStart = DateTime.Now;

                LogMessage(string.Format(sectionStart, "DXCCSs by clinical dimension"));
                elapsedTime = GenerateUtilizationJsonFileCombinations(CCSDataDir, "CCS_", jsonDomain, "spUtilEDGetSummaryDataByClinical", "spUtilEDGetDetailData",
                    baseOptions, CCSList, "@CCSID", "Id", HospitalList, "@HospitalID", "ID");
                Logger.Write(string.Format(sectionEnd, "DXCCSs by clinical dimension", elapsedTime));

                LogMessage(string.Format(sectionStart, "DXCCSs by hospital"));
                elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalNamesDataDir, "Hospital_", jsonDomain, "spUtilEDGetSummaryDataByGeo", "spUtilEDGetDetailData",
                    baseOptions, HospitalList, "@HospitalID", "ID", CCSList, "@CCSID", "Id");
                Logger.Write(string.Format(sectionEnd, "DXCCSs by clinical dimension", elapsedTime));

                LogMessage(string.Format(sectionStart, "DXCCSs by county"));
                elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalCountiesDataDir, "County_", jsonDomain, "spUtilEDGetSummaryDataByGeo", "spUtilEDGetDetailData",
                    baseOptions, CountyList, "@CountyID", "CountyID", CCSList, "@CCSID", "Id");
                Logger.Write(string.Format(sectionEnd, "DXCCSs by clinical dimension", elapsedTime));

                LogMessage(string.Format(sectionStart, "DXCCSs by region"));
                elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilEDGetSummaryDataByGeo", "spUtilEDGetDetailData",
                    baseOptions, RegionList, "@RegionID", "RegionID", CCSList, "@CCSID", "Id");
                Logger.Write(string.Format(sectionEnd, "DXCCSs by clinical dimension", elapsedTime));

                LogMessage(string.Format(sectionStart, "DXCCSs by clinical dimension"));
                elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalTypesDataDir, "HospitalType_", jsonDomain, "spUtilEDGetSummaryDataByGeo", "spUtilEDGetDetailData",
                    baseOptions, HospitalTypeList, "@HospitalCategoryID", "HospitalTypeID", CCSList, "@CCSID", "Id");
                Logger.Write(string.Format(sectionEnd, "DXCCSs by clinical dimension", elapsedTime));

                TimeSpan groupTimeDiff = DateTime.Now - groupStart;
                Logger.Write(string.Format("Utilization ED - Output - Generated output files for DXCCS sections in {0:c}", groupTimeDiff));
                #endregion DXCCS Output
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error generating JSON files for report {0}", this.ActiveReport?.Description);
            }
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
            return true;
        }
    }
}
