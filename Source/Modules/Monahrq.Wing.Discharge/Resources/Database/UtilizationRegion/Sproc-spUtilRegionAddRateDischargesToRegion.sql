/*
 *      Name:           spUtilRegionAddRateDischargesToRegion
 *      Used In:        RegionReportGenerator.cs
 *      Description:    Get stratified data for Region report generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUtilRegionAddRateDischargesToRegion]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[spUtilRegionAddRateDischargesToRegion]
GO

CREATE PROCEDURE [dbo].[spUtilRegionAddRateDischargesToRegion]
@ReportID uniqueidentifier, @ReportYear int = 2013, @Scale decimal(19,7) = 1000.0

AS
BEGIN
    SET NOCOUNT ON;

	UPDATE [dbo].[Temp_UtilRegion_Region]
	SET RateDischarges = ((CAST(UR.Discharges AS decimal(17,3)) * @Scale ) / 
							CAST(HR.[Population] AS decimal(17,3)))
	FROM [dbo].[Temp_UtilRegion_Region] AS UR
		LEFT JOIN [dbo].[RegionPopulationStrats] AS HR ON HR.RegionID = UR.RegionID
	WHERE UR.RegionID <> 0
		AND UR.CatID = HR.CatID
		AND UR.CatVal = HR.CatVal
		AND UR.Discharges IS NOT NULL
		AND UR.Discharges > 0
		AND HR.[Population] IS NOT NULL
		AND HR.[Population] > 0
		AND HR.[Year] = @ReportYear
		AND UR.ID = @ReportID

END
