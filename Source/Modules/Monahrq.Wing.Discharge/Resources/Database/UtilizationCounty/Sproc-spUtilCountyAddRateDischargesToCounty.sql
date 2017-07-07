/*
 *      Name:           spUtilCountyAddRateDischargesToCounty
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Get stratified data for County report generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUtilCountyAddRateDischargesToCounty]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[spUtilCountyAddRateDischargesToCounty]
GO

CREATE PROCEDURE [dbo].[spUtilCountyAddRateDischargesToCounty]
@ReportID uniqueidentifier, @ReportYear int = 2013, @Scale decimal(19,7) = 1000.0

AS
BEGIN
    SET NOCOUNT ON;

	UPDATE [dbo].[Temp_UtilCounty_County]
	SET RateDischarges = ((CAST(UC.Discharges AS decimal(17,3)) * @Scale ) / 
							CAST(AP.[Population] AS decimal(17,3)))
	FROM [dbo].[Temp_UtilCounty_County] AS UC
		LEFT JOIN dbo.Base_Counties C ON C.Id = UC.CountyID
		LEFT JOIN Base_AreaPopulationStrats AS AP ON AP.StateCountyFIPS = C.CountyFIPS
	WHERE CountyID <> 0
		AND UC.CatID = AP.CatID
		AND UC.CatVal = AP.CatVal
		AND UC.Discharges IS NOT NULL
		AND UC.Discharges > 0
		AND AP.[Population] IS NOT NULL
		AND AP.[Population] > 0
		AND AP.[Year] = @ReportYear
		AND UC.ID = @ReportID

END
