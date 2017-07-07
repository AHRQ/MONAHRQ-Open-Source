/*
 *      Name:           spUtilRegionGetTargetRegions
 *      Used In:        RegionReportGenerator.cs
 *		Description:    Prep the data for region report generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilRegionGetTargetRegions' 
)
   DROP PROCEDURE dbo.spUtilRegionGetTargetRegions
GO

CREATE PROCEDURE dbo.spUtilRegionGetTargetRegions
	@IPDataset IDsTableType READONLY, @RegionType nvarchar(30)
AS
BEGIN
    SET NOCOUNT ON;

	;WITH PATIENT_REGIONS(RegionID, Name, RegionType) as
	(
		SELECT DISTINCT 
			CASE
				WHEN (IP.CustomRegionID IS NOT NULL) THEN IP.CustomRegionID
				WHEN (@RegionType = 'HealthReferralRegion') THEN ISNULL(IP.HRRRegionID, -1)
				WHEN (@RegionType = 'HospitalServiceArea') THEN ISNULL(IP.HSARegionID, -1)
				ELSE -1
			END AS RegionID,
			CASE
				WHEN (Regions.Name IS NOT NULL) THEN Regions.Name + N', ' + Regions.[State]
				ELSE 'Unknown'
			END AS Name,
			CASE
				WHEN (IP.CustomRegionID IS NOT NULL) THEN 'CustomRegion'
				ELSE @RegionType
			END AS RegionType
		FROM Targets_InpatientTargets IP
			LEFT JOIN Regions ON Regions.ImportRegionId = 
				(CASE
					WHEN (IP.CustomRegionID IS NOT NULL) THEN IP.CustomRegionID
					WHEN (@RegionType = 'HealthReferralRegion') THEN ISNULL(IP.HRRRegionID, -1)
					WHEN (@RegionType = 'HospitalServiceArea') THEN ISNULL(IP.HSARegionID, -1)
					ELSE -1
				END) AND
				Regions.RegionType = 
					(CASE
						WHEN (IP.CustomRegionID IS NOT NULL) THEN 'CustomRegion'
						ELSE @RegionType
					END)
		WHERE
			IP.Dataset_id IN (
				SELECT Id
				FROM @IPDataset
			)
	)
	SELECT DISTINCT
		(CASE 
		WHEN pr.[Name] = 'Unknown' THEN -1
		ELSE pr.[RegionID]
		END) 'RegionID',
		pr.[Name],
		pr.[RegionType]
	FROM PATIENT_REGIONS PR;
END
GO