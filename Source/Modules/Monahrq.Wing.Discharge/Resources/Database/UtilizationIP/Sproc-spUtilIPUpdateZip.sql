/*
 *      Name:           spUtilIPUpdateZip
 *      Used In:        InpatientReportGenerator.cs
 *		Description:    Add zip codes to the Temp_UtilIP_Hospital table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilIPUpdateZip' 
)
   DROP PROCEDURE dbo.spUtilIPUpdateZip
GO

CREATE PROCEDURE dbo.spUtilIPUpdateZip
	@ReportID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE dbo.Temp_UtilIP_Hospital
	SET dbo.Temp_UtilIP_Hospital.Zip = dbo.Hospitals.Zip
	FROM dbo.Temp_UtilIP_Hospital JOIN dbo.Hospitals
		ON dbo.Temp_UtilIP_Hospital.HospitalID = dbo.Hospitals.Id
	WHERE dbo.Temp_UtilIP_Hospital.ID = @ReportID
END