/*
 *      Name:           spCountyGetDetailData
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Get stratified data for County report generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCountyGetDetailData]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[spCountyGetDetailData]
GO

CREATE PROCEDURE [dbo].[spCountyGetDetailData]
@ReportID uniqueidentifier, @CountyID int = 0, @UtilTypeID int = 0, @UtilID int = 0

AS
BEGIN
    SET NOCOUNT ON;

	-- Get the national totals.
	IF @UtilTypeID=1 
		BEGIN
			SELECT * FROM Base_IPNationalTotalsDRGs WHERE DRGID = @UtilID AND Region = 0
		END 
	ELSE IF @UtilTypeID=2
		BEGIN
			SELECT * FROM Base_IPNationalTotalsMDCs WHERE MDCID = @UtilID AND Region = 0
		END 
	ELSE IF @UtilTypeID=3
		BEGIN
			SELECT * FROM Base_IPNationalTotalsDXCCs WHERE DXCCSID = @UtilID AND Region = 0
		END 
	ELSE IF @UtilTypeID=4
		BEGIN
			SELECT * FROM Base_IPNationalTotalsPRCCs WHERE PRCCSID = @UtilID AND Region = 0
		END 

-- get summary for _0 ----
IF @CountyID = 0 
    BEGIN
		-- One hospital's data per row for one DRG condition.

        -- Get total per UtilTypeID
		SELECT
			ISNULL(Discharges, -1) AS Discharges,
			ISNULL(RateDischarges, -1) AS RateDischarges,
			ISNULL(MeanCosts, -1) AS MeanCosts,
			ISNULL(MedianCosts, -1) AS MedianCosts
        FROM  Temp_UtilCounty_County
        WHERE     
			ID = @ReportID AND
			UtilTypeID = @UtilTypeID AND
			UtilID = @UtilID AND
			CatID = 0 AND
			CatVal = 0 AND
			CountyID = 0 
			 
        -- Get total per UtilTypeID
		SELECT    
			UtilID as ID,
			CatID as CatID,
			CatVal as CatVal, 
			ISNULL(Discharges, -1) AS Discharges,
			ISNULL(RateDischarges, -1) AS RateDischarges,
			ISNULL(MeanCosts, -1) AS MeanCosts,
			ISNULL(MedianCosts, -1) AS MedianCosts
        FROM Temp_UtilCounty_County
        WHERE     
			ID = @ReportID AND
			UtilTypeID = @UtilTypeID AND
			UtilID = @UtilID AND
			CatID <> 0 AND
			CatVal <> 0 AND
			CountyID = 0 
	END

-- get summary for _HospitalID----
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
			UtilID = @UtilID AND
			CatID = 0 AND
			CatVal = 0 AND
			CountyID = @CountyID  

        -- Get total per UtilTypeID
		SELECT    
			CatID As CatID,
			CatVal As CatVal,
			ISNULL(Discharges, -1) AS Discharges,
			ISNULL(RateDischarges, -1) AS RateDischarges,
			ISNULL(MeanCosts, -1) AS MeanCosts,
			ISNULL(MedianCosts, -1) AS MedianCosts
        FROM Temp_UtilCounty_County
        WHERE     
			ID = @ReportID AND
			UtilTypeID = @UtilTypeID AND
			UtilID = @UtilID AND
			CountyID = @CountyID AND 
			CatID <> 0 AND
			CatVal <> 0 
	END

END
