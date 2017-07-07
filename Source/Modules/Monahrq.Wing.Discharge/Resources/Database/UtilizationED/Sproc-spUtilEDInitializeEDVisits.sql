/*
 *      Name:           spUtilEDInitializeEDVisits
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *		Description:    Add data from the IP tables for DRG to the intermediate temporary table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilEDInitializeEDVisits' 
)
   DROP PROCEDURE dbo.spUtilEDInitializeEDVisits
GO

CREATE PROCEDURE dbo.spUtilEDInitializeEDVisits
	@ReportID uniqueidentifier, @ReportYear varchar(4), @Hospitals IDsTableType READONLY,
	@IPDataset IDsTableType READONLY, @EDDataset IDsTableType READONLY, @RegionType nvarchar(50)

AS
BEGIN
    SET NOCOUNT ON;
	
    -- Add data from the IP table to the intermediate temporary table
    INSERT INTO Temp_UtilED_Prep
    SELECT      @ReportID AS ID,
				Hosp.Id AS HospitalID,
				ISNULL([dbo].[fnGetHospitalRegion](Hosp.Id, @RegionType), -1) AS RegionID,
                C.Id AS CountyID,
				
				CCS.DXCCSID AS CCSID,
				2 AS DataSource,
			    
				(CASE WHEN ED.DischargeDisposition = 20 THEN 1 ELSE 0 END) AS Died,
                ED.DischargeYear,
			    CASE WHEN (ED.Age < 18) THEN 1					    -- CatVal
                    WHEN (ED.Age >= 18 AND ED.Age <= 44) THEN 2
                    WHEN (ED.Age >= 45 AND ED.Age <= 64) THEN 3
                    WHEN (ED.Age >= 65) THEN 4
                    ELSE 0
                END AS Age,
				ISNULL(Race.Id, 0) AS Race,
				Sex.Id AS Sex,
				ISNULL(Payer.Id, 0) AS PrimaryPayer
    FROM        dbo.Targets_TreatAndReleaseTargets AS ED
                LEFT JOIN (Hospitals AS Hosp LEFT OUTER JOIN Base_Counties C ON C.CountyFips = Hosp.County) ON Hosp.LocalHospitalID = ED.LocalHospitalID
                LEFT JOIN dbo.Base_ICD9toDXCCSCrosswalks AS CCS ON ED.PrimaryDiagnosis = CCS.ICD9ID
                LEFT JOIN dbo.Base_Races AS Race ON ISNULL(ED.Race, 0) = Race.Id
                LEFT JOIN dbo.Base_Sexes AS Sex ON ED.Sex = Sex.Id
                LEFT JOIN dbo.Base_Payers AS Payer ON ISNULL(ED.PrimaryPayer, 0) = Payer.Id
    WHERE       CCS.DXCCSID IS NOT NULL AND
                Hosp.Id IN (
                    SELECT Id
                    FROM @Hospitals
                ) AND
                ED.Dataset_id IN (
                    SELECT Id
                    FROM @EDDataset
                );
END
GO
