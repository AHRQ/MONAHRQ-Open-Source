/*
 *      Name:           spUtilIPGetDetailData
 *      Used In:        InpatientReportGenerator.cs
 *		Description:    Get stratified data for IP report generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilIPGetDetailData' 
)
   DROP PROCEDURE dbo.spUtilIPGetDetailData
GO

CREATE PROCEDURE dbo.spUtilIPGetDetailData
	@ReportID uniqueidentifier, @UtilTypeID int, @UtilID int = 0, @HospitalID int = 0, @RegionID int = 0,
	@CountyID int = 0, @HospitalCategoryID int = 0
AS
BEGIN
    SET NOCOUNT ON;
	
    -- Get the national totals.
    IF @UtilTypeID = 1
	    BEGIN
		    SELECT *
            FROM Base_IPNationalTotalsDRGs
            WHERE DRGID = @UtilID AND Region = 0
	    END 
    ELSE IF @UtilTypeID = 2
	    BEGIN
		    SELECT *
            FROM Base_IPNationalTotalsMDCs
            WHERE MDCID = @UtilID AND Region = 0
	    END 
    ELSE IF @UtilTypeID = 3
	    BEGIN
		    SELECT *
            FROM Base_IPNationalTotalsDXCCs
            WHERE DXCCSID = @UtilID AND Region = 0
	    END 
    ELSE IF @UtilTypeID = 4
	    BEGIN
		    SELECT *
            FROM Base_IPNationalTotalsPRCCs
            WHERE PRCCSID = @UtilID AND Region = 0
	    END 


    -- get summary for _0 ----
    IF @HospitalID = 0 AND @RegionID = 0 AND @CountyID = 0 AND @HospitalCategoryID = 0
        BEGIN
			-- Get all the records combined.
		    SELECT ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_Hospital
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID = 0
                AND HospitalID = 0
		
			-- Individial listings
		    SELECT UtilID as ID,
		        CatID as CatID,
		        CatVal as CatVal, 
			    ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_Hospital
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID <> 0
                AND HospitalID = 0
    END


    -- get summary for _HospitalID----
    ELSE IF @HospitalID <> 0
        BEGIN
			-- Get all the records combined.
		    SELECT ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_Hospital
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID = 0
                AND HospitalID = @HospitalID  
		
			-- Individial listings
		    SELECT UtilID as ID,
		        CatID as CatID,
		        CatVal as CatVal, 
			    ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_Hospital
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID <> 0
                AND HospitalID = @HospitalID
        END


    -- get summary for _RegionID----
    ELSE IF @RegionID <> 0
        BEGIN
			-- Get all the records combined.
		    SELECT ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_Region
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID = 0
                AND RegionID = @RegionID  
		
			-- Individial listings
		    SELECT UtilID as ID,
		        CatID as CatID,
		        CatVal as CatVal, 
			    ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_Region
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID <> 0
                AND RegionID = @RegionID
        END


    -- get summary for _County----
    ELSE IF @CountyID <> 0
        BEGIN
			-- Get all the records combined.
		    SELECT ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_County
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID = 0
                AND CountyID = @CountyID  
		
			-- Individial listings
		    SELECT UtilID as ID,
		        CatID as CatID,
		        CatVal as CatVal, 
			    ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_County
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID <> 0
                AND CountyID = @CountyID
        END


    -- get summary for _HospitalCategoryID----
    ELSE IF @HospitalCategoryID <> 0
        BEGIN
			-- Get all the records combined.
		    SELECT ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_HospitalType
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID = 0
                AND HospitalTypeID = @HospitalCategoryID  
		
			-- Individial listings
		    SELECT UtilID as ID,
		        CatID as CatID,
		        CatVal as CatVal, 
			    ISNULL(Discharges, -1) AS Discharges,
			    ISNULL(MeanCharges, -1) AS MeanCharges,
			    ISNULL(MeanCosts, -1) AS MeanCosts,
			    ISNULL(MeanLOS, -1) AS MeanLOS,
			    ISNULL(MedianCharges, -1) AS MedianCharges,
			    ISNULL(MedianCosts, -1) AS MedianCosts,
			    ISNULL(MedianLOS, -1) AS MedianLOS
            FROM Temp_UtilIP_HospitalType
            WHERE ID = @ReportID
                AND UtilTypeID = @UtilTypeID
                AND UtilID = @UtilID
                AND CatID <> 0
                AND HospitalTypeID = @HospitalCategoryID
        END
END
GO