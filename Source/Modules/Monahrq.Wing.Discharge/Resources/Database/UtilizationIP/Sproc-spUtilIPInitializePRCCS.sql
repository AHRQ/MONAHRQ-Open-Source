/*
 *      Name:           spUtilIPInitializePRCCS
 *      Used In:        InpatientReportGenerator.cs
 *		Description:    Add data from the IP tables for PRCCS to the intermediate temporary table
 */

-- UtilTypeID = DRG/MDC/DXCCS/PRCCS designation  (i.e. 1=DRG, 2=MDC, 3=DXCCS, 4=PRCCS)
-- UtilID = Actual DRG/MDC/DXCCS/PRCCS code/number  (i.e. 465 = DRG code 465)
-- CatID = Stratification grouping - 0 = totals/summary, 1 = Age, 2 = Sex, 3 = Payer, 4 = Race
-- CatVal = Stratification value - 1 = <18 years old for age, 2 = 18-44, etc.

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilIPInitializePRCCS' 
)
   DROP PROCEDURE dbo.spUtilIPInitializePRCCS
GO

CREATE PROCEDURE dbo.spUtilIPInitializePRCCS
	@ReportID uniqueidentifier, @ReportYear varchar(4), @Hospitals IDsTableType READONLY, @IPDataset IDsTableType READONLY, @RegionType nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
	
	INSERT INTO Temp_UtilIP_Prep
	SELECT      
		@ReportID AS ID,
		Hosp.Id AS HospitalID,
		ISNULL([dbo].[fnGetHospitalRegion](Hosp.Id, @RegionType), -1) AS RegionID,
		ISNULL(C.Id, -1) AS CountyID,

		4 AS UtilTypeID,		-- UtilType is PRCCS
		PRCCS.PRCCSID AS UtilID,
	
		IP.DischargeYear AS DischargeYear,
		CASE WHEN (IP.Age < 18) THEN 1
				WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2
				WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3
				WHEN (IP.Age >= 65) THEN 4
				ELSE 0
		END AS Age,
		ISNULL(Race.Id, 0) AS Race,
		Sex.Id AS Sex,
		ISNULL(Payer.Id, 0) AS PrimaryPayer,
		IP.LengthOfStay AS LengthOfStay,
		IP.TotalCharge AS TotalCharge,
		dbo.fnGetCostToChargeRatio(IP.DischargeYear, Hosp.CMSProviderID) * IP.TotalCharge AS TotalCost
	FROM Targets_InpatientTargets AS IP
		LEFT JOIN (Hospitals AS Hosp LEFT OUTER JOIN Base_Counties C ON C.CountyFips = Hosp.County) ON IP.LocalHospitalID = Hosp.LocalHospitalID
	    LEFT JOIN Base_ICD9toPRCCSCrosswalks AS PRCCS ON IP.PrincipalDiagnosis = PRCCS.ICD9ID
		LEFT JOIN Base_Races AS Race ON ISNULL(IP.Race, 0) = Race.Id
		LEFT JOIN Base_Sexes AS Sex ON IP.Sex = Sex.Id
		LEFT JOIN Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 0) = Payer.Id
	WHERE PRCCS.PRCCSID IS NOT NULL AND
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
