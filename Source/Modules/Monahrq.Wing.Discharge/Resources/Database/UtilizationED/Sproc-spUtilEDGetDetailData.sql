/*
 *      Name:           spUtilEDGetDetailData
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *		Description:    Add data from the IP tables for DRG to the intermediate temporary table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilEDGetDetailData' 
)
   DROP PROCEDURE dbo.spUtilEDGetDetailData
GO

CREATE PROCEDURE dbo.spUtilEDGetDetailData
	@ReportID uniqueidentifier, @CCSID int = 0, @HospitalID int = 0, @RegionID int = 0, @CountyID int = 0, @HospitalCategoryID int = 0

AS
BEGIN
    SET NOCOUNT ON;
	
    -- Get the national totals.
    SELECT
		NumEdVisits,
		NumEdVisitsStdErr,
		NumAdmitHosp,
		NumAdmitHospStdErr,
		DiedEd,
		DiedEdStdErr,
		DiedHosp,
		DiedHospStdErr
    FROM dbo.Base_EDNationalTotals
    WHERE CCSID = @CCSID

    -- get summary for _0 ----
    IF @HospitalID = 0 AND @RegionID = 0 AND @CountyID = 0 AND @HospitalCategoryID = 0
        BEGIN
			-- Get all the records combined.
			SELECT
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_Hospital
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID = 0
				AND HospitalID = 0

			-- Individial listings
			SELECT 
				CCSID,
		        CatID as CatID,
		        CatVal as CatVal, 
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_Hospital
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID <> 0
				AND HospitalID = 0
		END


    -- get summary for _HospitalID----
    ELSE IF @HospitalID <> 0
        BEGIN
			-- Get all the records combined.
			SELECT
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_Hospital
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID = 0
				AND HospitalID = @HospitalID

			-- Individial listings
			SELECT
				CatID,
				CatVal,
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_Hospital
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID <> 0
				AND HospitalID = @HospitalID
		END


    -- get summary for _RegionID----
    ELSE IF @RegionID <> 0
        BEGIN
			-- Get all the records combined.
			SELECT
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_Region
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID = 0
				AND RegionID = @RegionID

			-- Individial listings
			SELECT
				CatID,
				CatVal,
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_Region
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID <> 0
				AND RegionID = @RegionID
		END

		
    -- get summary for _County----
    ELSE IF @CountyID <> 0
        BEGIN
			-- Get all the records combined.
			SELECT
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_County
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID = 0
				AND CountyID = @CountyID

			-- Individial listings
			SELECT
				CatID,
				CatVal,
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_County
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID <> 0
				AND CountyID = @CountyID
		END


    -- get summary for _HospitalCategoryID----
    ELSE IF @HospitalCategoryID <> 0
        BEGIN
			-- Get all the records combined.
			SELECT
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_HospitalType
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID = 0
				AND HospitalTypeID = @HospitalCategoryID

			-- Individial listings
			SELECT
				CatID,
				CatVal,
				ISNULL(NumEdVisits, -1) AS NumEdVisits,
				ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
				ISNULL(DiedEd, -1) AS DiedEd,
				ISNULL(DiedHosp, -1) AS DiedHosp
			FROM dbo.Temp_UtilED_HospitalType
			WHERE ID = @ReportID
				AND CCSID = @CCSID
				AND CatID <> 0
				AND HospitalTypeID = @HospitalCategoryID
		END
END
GO
