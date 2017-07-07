/*
 *      Name:           spUtilCountyGetSummaryDataByGeo
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Get summary data by geographic dimension for County report generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUtilCountyGetSummaryDataByGeo]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[spUtilCountyGetSummaryDataByGeo]
GO

CREATE PROCEDURE [dbo].[spUtilCountyGetSummaryDataByGeo]
@ReportID uniqueidentifier, @CountyID int = 0, @UtilTypeID int = 0

AS
BEGIN
    SET NOCOUNT ON;

	-- Get the national totals.
	IF @UtilTypeID=1 
		BEGIN
			SELECT * FROM Base_IPNationalTotalsDRGs WHERE DRGID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID=2
		BEGIN
			SELECT * FROM Base_IPNationalTotalsMDCs WHERE MDCID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID=3
		BEGIN
			SELECT * FROM Base_IPNationalTotalsDXCCs WHERE DXCCSID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID=4
		BEGIN
			SELECT * FROM Base_IPNationalTotalsPRCCs WHERE PRCCSID = 0 AND Region = 0
		END 

	-- get summary for _0 ----
	IF @CountyID  = 0 
		BEGIN
			-- One hospital's data per row for one DRG condition.

			-- Get total per UtilTypeID
			SELECT    
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) AS RateDischarges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MedianCosts, -1) AS MedianCosts
			FROM Temp_UtilCounty_County
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID = 0 AND
				CatID = 0 AND
				CatVal = 0 AND
				CountyID = 0 
		
			-- Get total per UtilTypeID
			SELECT    
				UtilID as ID,
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) AS RateDischarges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MedianCosts, -1) AS MedianCosts
			FROM Temp_UtilCounty_County
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID <> 0 AND
				CatID = 0 AND
				CatVal = 0 AND
				CountyID = 0 
		END

	-- get summary for _CountyID----
	ELSE IF @CountyID  <> 0 
		BEGIN
			-- One hospital's data per row for one DRG condition.

			-- Get total per UtilTypeID
			SELECT    
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) AS RateDischarges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MedianCosts, -1) AS MedianCosts
			FROM Temp_UtilCounty_County
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID = 0 AND
				CatID = 0 AND
				CatVal = 0 AND
				CountyID = @CountyID  
		
			-- Get total per UtilTypeID
			SELECT    
				UtilID as ID,
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) AS RateDischarges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MedianCosts, -1) AS MedianCosts
			FROM Temp_UtilCounty_County
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID <> 0 AND
				CountyID = @CountyID AND 
				CatID = 0 AND
				CatVal = 0 
		END
END
