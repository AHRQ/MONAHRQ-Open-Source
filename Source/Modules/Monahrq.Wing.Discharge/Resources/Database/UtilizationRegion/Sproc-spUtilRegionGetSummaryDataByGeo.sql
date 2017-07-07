/*
 *      Name:           spUtilRegionGetSummaryDataByGeo
 *      Used In:        RegionReportGenerator.cs
 *		Description:    Prep the data for region report generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilRegionGetSummaryDataByGeo' 
)
   DROP PROCEDURE dbo.spUtilRegionGetSummaryDataByGeo
GO

CREATE PROCEDURE dbo.spUtilRegionGetSummaryDataByGeo
--	@ReportID uniqueidentifier, @UtilTypeID int, @UtilID int = 0, @HospitalID int = 0, @RegionID int = 0,
--	@CountyID int = 0, @HospitalCategoryID int = 0

	@ReportID uniqueidentifier, @UtilTypeID int = 0, @RegionID int = 0

AS
BEGIN
    SET NOCOUNT ON;

	-- Get the national totals.
	IF @UtilTypeID = 1 
		BEGIN
			SELECT * FROM Base_IPNationalTotalsDRGs WHERE DRGID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID = 2
		BEGIN
			SELECT * FROM Base_IPNationalTotalsMDCs WHERE MDCID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID = 3
		BEGIN
			SELECT * FROM Base_IPNationalTotalsDXCCs WHERE DXCCSID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID = 4
		BEGIN
			SELECT * FROM Base_IPNationalTotalsPRCCs WHERE PRCCSID = 0 AND Region = 0
		END 


	-- get summary for _0 ----
	IF @RegionID = 0 
		BEGIN
			-- One hospital's data per row for one DRG condition.
			-- Get total per UtilTypeID
			SELECT    
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) as RateDischarges,

				ISNULL(MeanCharges, -1) AS MeanCharges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MeanLOS, -1) AS MeanLOS,

				ISNULL(MedianCharges, -1) AS MedianCharge,
				ISNULL(MedianCosts, -1) AS MedianCosts,
				ISNULL(MedianLOS, -1) AS MedianLOS
			FROM Temp_UtilRegion_Region
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID = 0 AND
				CatID = 0 AND
				CatVal = 0 AND
				RegionID = 0 
		
			-- Get total per UtilTypeID
			SELECT    
				UtilID as ID,
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) as RateDischarges,

				ISNULL(MeanCharges, -1) AS MeanCharges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MeanLOS, -1) AS MeanLOS,

				ISNULL(MedianCharges, -1) AS MedianCharge,
				ISNULL(MedianCosts, -1) AS MedianCosts,
				ISNULL(MedianLOS, -1) AS MedianLOS
			FROM Temp_UtilRegion_Region
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID <> 0 AND
				CatID = 0 AND
				CatVal = 0 AND
				RegionID = 0 
		END


	-- get summary for _RegionID----
	IF @RegionID <> 0 
		BEGIN
			-- One hospital's data per row for one DRG condition.
			-- Get total per UtilTypeID
			SELECT    
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) as RateDischarges,

				ISNULL(MeanCharges, -1) AS MeanCharges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MeanLOS, -1) AS MeanLOS,

				ISNULL(MedianCharges, -1) AS MedianCharge,
				ISNULL(MedianCosts, -1) AS MedianCosts,
				ISNULL(MedianLOS, -1) AS MedianLOS
			FROM  Temp_UtilRegion_Region
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID =0 AND
				CatID = 0 AND
				CatVal = 0 AND
				RegionID =@RegionID  
		
			-- Get total per UtilTypeID
			SELECT    
				UtilID as ID,
				ISNULL(Discharges, -1) AS Discharges,
				ISNULL(RateDischarges, -1) as RateDischarges,

				ISNULL(MeanCharges, -1) AS MeanCharges,
				ISNULL(MeanCosts, -1) AS MeanCosts,
				ISNULL(MeanLOS, -1) AS MeanLOS,

				ISNULL(MedianCharges, -1) AS MedianCharge,
				ISNULL(MedianCosts, -1) AS MedianCosts,
				ISNULL(MedianLOS, -1) AS MedianLOS
			FROM Temp_UtilRegion_Region
			WHERE     
				ID = @ReportID AND
				UtilTypeID = @UtilTypeID AND
				UtilID <> 0 AND
				RegionID = @RegionID AND 
				CatID = 0 AND
				CatVal = 0 
	END

END
GO