/*
 *      Name:           spUtilIPUpdateHospitalType
 *      Used In:        InpatientReportGenerator.cs
 *		Description:    Add hospital types to the Temp_UtilIP_Hospital table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilIPUpdateHospitalType' 
)
   DROP PROCEDURE dbo.spUtilIPUpdateHospitalType
GO

CREATE PROCEDURE dbo.spUtilIPUpdateHospitalType
	@ReportID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

	WITH
        HospitalTypes AS
        (
            SELECT ParentTable.Hospital_Id AS HospitalID,
                HospitalCategoryID =
                    STUFF((
                            SELECT ','+ CAST(SubTable.Category_Id AS NVARCHAR(MAX))
                            FROM dbo.Hospitals_HospitalCategories SubTable WITH (NOLOCK)
                            WHERE SubTable.Hospital_Id = ParentTable.Hospital_Id
                            FOR XML PATH('') 
                        ), 1, 1,'')
            FROM dbo.Hospitals_HospitalCategories ParentTable WITH (NOLOCK)
        )
    UPDATE dbo.Temp_UtilIP_Hospital
    SET dbo.Temp_UtilIP_Hospital.HospitalType = HospitalTypes.HospitalCategoryID
    FROM dbo.Temp_UtilIP_Hospital WITH (NOLOCK)
        JOIN HospitalTypes WITH (NOLOCK) ON dbo.Temp_UtilIP_Hospital.HospitalID = HospitalTypes.HospitalID
    WHERE dbo.Temp_UtilIP_Hospital.ID = @ReportID
END