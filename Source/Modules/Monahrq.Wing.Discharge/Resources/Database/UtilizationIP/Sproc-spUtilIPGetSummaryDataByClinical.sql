/*
 *      Name:           spUtilIPGetSummaryDataByClinical
 *      Used In:        InpatientReportGenerator.cs
 *		Description:    Get summary data by clinical dimension for IP report generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilIPGetSummaryDataByClinical' 
)
   DROP PROCEDURE dbo.spUtilIPGetSummaryDataByClinical
GO

CREATE PROCEDURE dbo.spUtilIPGetSummaryDataByClinical
	@ReportID uniqueidentifier, @UtilTypeID int = 0, @UtilID int = 0
AS
BEGIN
    SET NOCOUNT ON;
	
	/***** Get the national totals. *****/
	IF @UtilTypeID = 1
		BEGIN
			SELECT *
			FROM Base_IPNationalTotalsDRGs
			WHERE DRGID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID = 2
		BEGIN
			SELECT *
			FROM Base_IPNationalTotalsMDCs
			WHERE MDCID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID = 3
		BEGIN
			SELECT *
			FROM Base_IPNationalTotalsDXCCs
			WHERE DXCCSID = 0 AND Region = 0
		END 
	ELSE IF @UtilTypeID = 4
		BEGIN
			SELECT *
			FROM Base_IPNationalTotalsPRCCs
			WHERE PRCCSID = 0 AND Region = 0
		END


	-- Get the values from the temp tables
	SELECT ISNULL(Discharges, -1) AS Discharges,
		ISNULL(MeanCharges, -1) AS MeanCharges,
		ISNULL(MeanCosts, -1) AS MeanCosts,
		ISNULL(MeanLOS, -1) AS MeanLOS,
		ISNULL(MedianCharges, -1) AS MedianCharges,
		ISNULL(MedianCosts, -1) AS MedianCosts,
		ISNULL(MedianLOS, -1) AS MedianLOS
	FROM  Temp_UtilIP_Hospital
	WHERE ID = @ReportID
		AND HospitalID = 0
		AND UtilTypeID = @UtilTypeID
		AND UtilID = @UtilID
		AND CatID = 0
		AND CatVal = 0

	/***** One hospital's data per row for one UtilTypeID (DRG/MDC/DXCCS/PRCCS) condition *****/
	-- Get the values from the temp tables
	SELECT HospitalID, RegionID, CountyID, Zip, HospitalType,
		ISNULL(Discharges, -1) AS Discharges,
		ISNULL(MeanCharges, -1) AS MeanCharges,
		ISNULL(MeanCosts, -1) AS MeanCosts,
		ISNULL(MeanLOS, -1) AS MeanLOS,
		ISNULL(MedianCharges, -1) AS MedianCharges,
		ISNULL(MedianCosts, -1) AS MedianCosts,
		ISNULL(MedianLOS, -1) AS MedianLOS
	FROM Temp_UtilIP_Hospital
	WHERE ID = @ReportID
		AND HospitalID <> 0
		AND UtilTypeID = @UtilTypeID
		AND UtilID = @UtilID
		AND CatID = 0
		AND CatVal = 0 
END
GO