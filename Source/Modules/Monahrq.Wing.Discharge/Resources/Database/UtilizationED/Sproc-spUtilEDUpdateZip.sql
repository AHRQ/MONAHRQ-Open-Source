/*
 *      Name:           spUtilEDUpdateZip
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *		Description:    Add zip codes to the Temp_UtilED_Hospital table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilEDUpdateZip' 
)
   DROP PROCEDURE dbo.spUtilEDUpdateZip
GO

CREATE PROCEDURE dbo.spUtilEDUpdateZip
	@ReportID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE dbo.Temp_UtilED_Hospital
	SET dbo.Temp_UtilED_Hospital.Zip = dbo.Hospitals.Zip
	FROM dbo.Temp_UtilED_Hospital JOIN dbo.Hospitals
		ON dbo.Temp_UtilED_Hospital.HospitalID = dbo.Hospitals.Id
	WHERE dbo.Temp_UtilED_Hospital.ID = @ReportID
END