/*
 *      Name:           spUtilEDGetSummaryDataByClinical
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *		Description:    Add data from the IP tables for DRG to the intermediate temporary table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilEDGetSummaryDataByClinical' 
)
   DROP PROCEDURE dbo.spUtilEDGetSummaryDataByClinical
GO

CREATE PROCEDURE dbo.spUtilEDGetSummaryDataByClinical
	@ReportID uniqueidentifier, @CCSID int = 0

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

    -- Get all the records combined.
    SELECT
		ISNULL(NumEdVisits, -1) AS NumEdVisits,
		ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
		ISNULL(DiedEd, -1) AS DiedEd,
		ISNULL(DiedHosp, -1) AS DiedHosp
    FROM dbo.Temp_UtilED_Hospital
    WHERE ID = @ReportID
		AND HospitalID = 0
		AND CCSID = @CCSID
		AND CatID = 0
		AND CatVal = 0

    -- Individial hospital listings
    SELECT HospitalID, RegionID, CountyID, Zip, HospitalType,
		ISNULL(NumEdVisits, -1) AS NumEdVisits,
		ISNULL(NumAdmitHosp, -1) AS NumAdmitHosp,
		ISNULL(DiedEd, -1) AS DiedEd,
		ISNULL(DiedHosp, -1) AS DiedHosp
    FROM dbo.Temp_UtilED_Hospital
    WHERE ID = @ReportID
		AND HospitalID <> 0
		AND CCSID = @CCSID
		AND CatID = 0
		AND CatVal = 0
END
GO
