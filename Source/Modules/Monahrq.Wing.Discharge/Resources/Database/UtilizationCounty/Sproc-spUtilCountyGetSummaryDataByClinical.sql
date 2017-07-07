/*
 *      Name:           spUtilCountyGetSummaryDataByClinical
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Get summary data by clinical dimension for County report generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUtilCountyGetSummaryDataByClinical]') AND TYPE IN (N'P', N'PC'))
	DROP PROCEDURE [dbo].[spUtilCountyGetSummaryDataByClinical]
GO

CREATE PROCEDURE [dbo].[spUtilCountyGetSummaryDataByClinical]
@ReportID uniqueidentifier, @UtilTypeID int = 0, @UtilID int = 0

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
		CountyID = 0 AND
		CatID = 0 AND
		CatVal = 0
		
    -- Get total per UtilTypeID
	SELECT    
		CountyID,
		ISNULL(Discharges, -1) AS Discharges,
		ISNULL(RateDischarges, -1) AS RateDischarges,
		ISNULL(MeanCosts, -1) AS MeanCosts,
		ISNULL(MedianCosts, -1) AS MedianCosts
    FROM Temp_UtilCounty_County
    WHERE     
		ID = @ReportID AND
		UtilTypeID = @UtilTypeID AND
		UtilID = @UtilID AND
		CountyID <> 0 AND
		CatID = 0 AND
		CatVal = 0

END