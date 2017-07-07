/*
 *      Name:           spUtilRegionAddRateDischargesToAllCombined
 *      Used In:        RegionReportGenerator.cs
 *      Description:    Get stratified data for Region report generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUtilRegionAddRateDischargesToAllCombined]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[spUtilRegionAddRateDischargesToAllCombined]
GO

CREATE PROCEDURE [dbo].[spUtilRegionAddRateDischargesToAllCombined]
@ReportID uniqueidentifier, @ReportYear int = 2013, @Scale decimal(19,7) = 1000.0

AS
BEGIN
    SET NOCOUNT ON;

	WITH CombinedRegions (CatID, CatVal, [Population]) AS (
		SELECT CatID, CatVal, SUM([Population]) AS [Population]
		FROM [dbo].[RegionPopulationStrats]
		WHERE RegionID IN
		(
			SELECT DISTINCT HR.RegionID
			FROM [dbo].[Temp_UtilRegion_Region] AS UR
				LEFT JOIN [dbo].[RegionPopulationStrats] AS HR ON HR.RegionID = UR.RegionID
			WHERE UR.RegionID <> 0
				AND UR.ID = @ReportID
		)
		AND [Year] = @ReportYear
		GROUP BY CatID, CatVal
	)
	UPDATE [dbo].[Temp_UtilRegion_Region]
	SET RateDischarges = ((CAST(UR.Discharges AS decimal(17,3)) * @Scale ) / 
							CAST(CR.[Population] AS decimal(17,3)))
	FROM [dbo].[Temp_UtilRegion_Region] AS UR
		LEFT JOIN CombinedRegions CR ON CR.CatID = UR.CatID AND CR.CatVal = UR.CatVal
	WHERE RegionID = 0
		AND UR.Discharges IS NOT NULL
		AND UR.Discharges > 0
		AND CR.[Population] IS NOT NULL
		AND CR.[Population] > 0
		AND UR.ID = @ReportID

END
