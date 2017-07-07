/*
 *      Name:           spUtilCountyGetRecordsInIPTarget
 *      Used In:        InpatientReportGenerator.cs
 *		Description:    Add data from the IP tables for DRG to the intermediate temporary table
 */

-- UtilTypeID = DRG/MDC/DXCCS/PRCCS designation  (i.e. 1=DRG, 2=MDC, 3=DXCCS, 4=PRCCS)
-- UtilID = Actual DRG/MDC/DXCCS/PRCCS code/number  (i.e. 465 = DRG code 465)
-- CatID = Stratification grouping - 0 = totals/summary, 1 = Age, 2 = Sex, 3 = Payer, 4 = Race
-- CatVal = Stratification value - 1 = <18 years old for age, 2 = 18-44, etc.

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilCountyGetRecordsInIPTarget' 
)
   DROP PROCEDURE dbo.spUtilCountyGetRecordsInIPTarget
GO

CREATE PROCEDURE dbo.spUtilCountyGetRecordsInIPTarget
	@IPDataset IDsTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;
	
	SELECT COUNT(*) AS IPRows
	FROM Targets_InpatientTargets AS IP
	WHERE IP.PatientStateCountyCode IS NOT NULL AND
			IP.Dataset_Id IN (
				SELECT Id
				FROM @IPDataset
			);
END
GO
