/*
 *      Name:           spUtilCountyAddRateDischargesToAllCombined
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Get stratified data for County report generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUtilCountyAddRateDischargesToAllCombined]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[spUtilCountyAddRateDischargesToAllCombined]
GO

CREATE PROCEDURE [dbo].[spUtilCountyAddRateDischargesToAllCombined]
@ReportID uniqueidentifier, @ReportYear int = 2013, @Scale decimal(19,7) = 1000.0

AS
BEGIN
    SET NOCOUNT ON;

	WITH CombinedCounties (CatID, CatVal, [Population]) AS (
		SELECT CatID, CatVal, SUM([Population]) AS [Population]
		FROM Base_AreaPopulationStrats
		WHERE StateCountyFIPS IN
		(
			SELECT DISTINCT C.CountyFIPS
			FROM [dbo].[Temp_UtilCounty_County] AS UC
				LEFT JOIN dbo.Base_Counties C ON C.Id = UC.CountyID
			WHERE UC.CountyID <> 0
				AND UC.ID = @ReportID
		)
		AND [Year] = @ReportYear
		GROUP BY CatID, CatVal
	)
	UPDATE [dbo].[Temp_UtilCounty_County]
	SET RateDischarges = ((CAST(UC.Discharges AS decimal(17,3)) * @Scale ) / 
							CAST(C.[Population] AS decimal(17,3)))
	FROM [dbo].[Temp_UtilCounty_County] AS UC
		LEFT JOIN CombinedCounties C ON C.CatID = UC.CatID AND C.CatVal = UC.CatVal
	WHERE CountyID = 0
		AND UC.Discharges IS NOT NULL
		AND UC.Discharges > 0
		AND C.[Population] IS NOT NULL
		AND C.[Population] > 0
		AND UC.ID = @ReportID

END
