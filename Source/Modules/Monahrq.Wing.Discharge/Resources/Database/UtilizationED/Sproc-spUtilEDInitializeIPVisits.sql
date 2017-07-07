/*
 *      Name:           spUtilEDInitializeIPVisits
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *		Description:    Add data from the IP tables for DRG to the intermediate temporary table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilEDInitializeIPVisits' 
)
   DROP PROCEDURE dbo.spUtilEDInitializeIPVisits
GO

CREATE PROCEDURE dbo.spUtilEDInitializeIPVisits
	@ReportID uniqueidentifier, @ReportYear varchar(4), @Hospitals IDsTableType READONLY,
	@IPDataset IDsTableType READONLY, @EDDataset IDsTableType READONLY, @RegionType nvarchar(50)

AS
BEGIN
    SET NOCOUNT ON;
	
    -- Add data from the ED table to the intermediate temporary table
    INSERT INTO Temp_UtilED_Prep
    SELECT      @ReportID AS ID,
				Hosp.Id AS HospitalID,
				ISNULL([dbo].[fnGetHospitalRegion](Hosp.Id, @RegionType), -1) AS RegionID,
                C.Id AS CountyID,
				
				CCS.DXCCSID AS CCSID,
				1 AS DataSource, 
			    
				IP.DischargeDisposition AS Died,
                IP.DischargeYear, 
                CASE WHEN (IP.Age < 18) THEN 1					    -- CatVal
                    WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2
                    WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3
                    WHEN (IP.Age >= 65) THEN 4
                    ELSE 0
                END AS Age,
				ISNULL(Race.Id, 0) AS Race,
				Sex.Id AS Sex,
				ISNULL(Payer.Id, 0) AS PrimaryPayer
    FROM        dbo.Targets_InpatientTargets AS IP
                LEFT JOIN (Hospitals AS Hosp LEFT OUTER JOIN Base_Counties C ON C.CountyFips = Hosp.County) ON Hosp.LocalHospitalID = IP.LocalHospitalID
                LEFT JOIN dbo.Base_ICD9toDXCCSCrosswalks AS CCS ON IP.PrincipalDiagnosis = CCS.ICD9ID
                LEFT JOIN dbo.Base_Races AS Race ON ISNULL(IP.Race, 0) = Race.Id
                LEFT JOIN dbo.Base_Sexes AS Sex ON IP.Sex = Sex.Id
                LEFT JOIN dbo.Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 0) = Payer.Id
    WHERE       EDServices = 1 AND
                CCS.DXCCSID IS NOT NULL AND
                Hosp.Id IN (
                    SELECT Id
                    FROM @Hospitals
                ) AND
                IP.Dataset_id IN (
                    SELECT Id
                    FROM @IPDataset
                );
END
GO
