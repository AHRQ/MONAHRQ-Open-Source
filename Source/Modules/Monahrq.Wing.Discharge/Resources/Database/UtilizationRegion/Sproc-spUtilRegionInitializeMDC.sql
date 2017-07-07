/*
 *      Name:           spUtilRegionInitializeMDC
 *      Used In:        RegionReportGenerator.cs
 *		Description:    Prep the data for region report generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilRegionInitializeMDC' 
)
   DROP PROCEDURE dbo.spUtilRegionInitializeMDC
GO

CREATE PROCEDURE dbo.spUtilRegionInitializeMDC
	@ReportID uniqueidentifier, @RegionType nvarchar(50), @ReportYear varchar(4), @IPDataset IDsTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;

	INSERT INTO Temp_UtilRegion_Prep
	SELECT @ReportID AS Id,
		CASE
			WHEN (IP.CustomRegionID IS NOT NULL) THEN IP.CustomRegionID
			WHEN (@RegionType = 'HealthReferralRegion') THEN ISNULL(IP.HRRRegionID, -1)
			WHEN (@RegionType = 'HospitalServiceArea') THEN ISNULL(IP.HSARegionID, -1)
			ELSE -1
		END AS RegionID,

		2 as UtilTypeID,
		IP.MDC AS UtilID,
	
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
		IP.TotalCharge * dbo.fnGetCostToChargeRatio(@ReportYear, Hosp.CMSProviderID) AS TotalCost   -- TODO: cost to charge conversion
	FROM Targets_InpatientTargets AS IP
		LEFT JOIN Base_Races AS Race ON ISNULL(IP.Race, 0) = Race.Id
		LEFT JOIN Base_Sexes AS Sex ON IP.Sex = Sex.Id
		LEFT JOIN Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 0) = Payer.Id
		LEFT JOIN Hospitals AS HOSP ON IP.LocalHospitalID = HOSP.LocalHospitalId AND HOSP.IsArchived=0 AND HOSP.IsDeleted=0
	WHERE IP.MDC IS NOT NULL AND
        IP.Dataset_id IN (
			SELECT Id
			FROM @IPDataset
        );

    UPDATE Temp_UtilRegion_Prep
    SET UtilID = 99
    WHERE ID = @ReportID AND UtilTypeID = 2 AND UtilID = 0

END
GO