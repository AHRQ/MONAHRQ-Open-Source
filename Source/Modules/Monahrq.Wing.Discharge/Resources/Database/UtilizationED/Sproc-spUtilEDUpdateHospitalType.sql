/*
 *      Name:           spUtilEDUpdateHospitalType
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *		Description:    Add hospital types to the Temp_UtilED_Hospital table
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilEDUpdateHospitalType' 
)
   DROP PROCEDURE dbo.spUtilEDUpdateHospitalType
GO

CREATE PROCEDURE dbo.spUtilEDUpdateHospitalType
	@ReportID uniqueidentifier, @Hospitals IDsTableType READONLY
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
                            WHERE
								SubTable.Hospital_Id = ParentTable.Hospital_Id AND
								SubTable.Hospital_Id IN (SELECT Id FROM @Hospitals)	
                            FOR XML PATH('') 
                        ), 1, 1,'')
            FROM dbo.Hospitals_HospitalCategories ParentTable WITH (NOLOCK)
			WHERE ParentTable.Hospital_Id IN (SELECT Id FROM @Hospitals)	
        )
    UPDATE dbo.Temp_UtilED_Hospital
    SET dbo.Temp_UtilED_Hospital.HospitalType = HospitalTypes.HospitalCategoryID
    FROM dbo.Temp_UtilED_Hospital WITH (NOLOCK)
        JOIN HospitalTypes WITH (NOLOCK) ON dbo.Temp_UtilED_Hospital.HospitalID = HospitalTypes.HospitalID
    WHERE dbo.Temp_UtilED_Hospital.ID = @ReportID
END