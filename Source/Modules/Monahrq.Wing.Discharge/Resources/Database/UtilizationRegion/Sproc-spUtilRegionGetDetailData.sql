/*
 *      Name:           spUtilRegionGetDetailData
 *      Used In:        RegionReportGenerator.cs
 *		Description:    Prep the data for region report generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilRegionGetDetailData' 
)
   DROP PROCEDURE dbo.spUtilRegionGetDetailData
GO

CREATE PROCEDURE dbo.spUtilRegionGetDetailData
--	@ReportID uniqueidentifier, @UtilTypeID int, @UtilID int = 0, @HospitalID int = 0, @RegionID int = 0,
--	@CountyID int = 0, @HospitalCategoryID int = 0

	@ReportID uniqueidentifier, @UtilTypeID int = 0, @UtilID int = 0, @RegionID int = 0

AS
BEGIN
    SET NOCOUNT ON;

	-- get summary for _0 ----
	IF @RegionID = 0 
		BEGIN
			-- Get the national totals.
			IF @UtilTypeID = 1 
				BEGIN
					SELECT * FROM Base_IPNationalTotalsDRGs WHERE DRGID = @UtilID AND Region = 0
				END 
			ELSE IF @UtilTypeID = 2
				BEGIN
					SELECT * FROM Base_IPNationalTotalsMDCs WHERE MDCID = @UtilID AND Region = 0
				END 
			ELSE IF @UtilTypeID = 3
				BEGIN
					SELECT * FROM Base_IPNationalTotalsDXCCs WHERE DXCCSID = @UtilID AND Region = 0
				END 
			ELSE IF @UtilTypeID = 4
				BEGIN
					SELECT * FROM Base_IPNationalTotalsPRCCs WHERE PRCCSID = @UtilID AND Region = 0
				END 

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
				UtilID = @UtilID AND
				CatID = 0 AND
				CatVal = 0 AND
				RegionID = 0 
		
			-- Get total per UtilTypeID
			SELECT    
				CatID as CatID,
				CatVal as CatVal, 

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
				UtilID = @UtilID AND
				CatID <> 0 AND
				CatVal <> 0 AND
				RegionID = 0 
		END

	-- get summary for _HospitalID----
	IF  @RegionID  <> 0 
		BEGIN
			-- Get the national totals.
			SELECT
				0 AS Discharges,
				0 AS MeanCharges,
				0 AS MeanCosts,
				0 AS MeanLOS,
				0 AS MedianCharge,
				0 AS MedianCosts,
				0 AS MedianLOS

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
				UtilID  =@UtilID AND
				CatID = 0 AND
				CatVal = 0 AND
				RegionID = @RegionID  

			-- Get total per UtilTypeID
			SELECT    
				CatID As CatID,
				CatVal As CatVal,
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
				UtilID = @UtilID AND
				RegionID = @RegionID AND 
				CatID <> 0 AND
				CatVal <> 0 
	END

END
GO