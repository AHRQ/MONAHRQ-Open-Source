/*
 *      Name:           spUtilEDGetRecordsInEDTarget
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
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spUtilEDGetRecordsInEDTarget' 
)
   DROP PROCEDURE dbo.spUtilEDGetRecordsInEDTarget
GO

CREATE PROCEDURE dbo.spUtilEDGetRecordsInEDTarget
	@Hospitals IDsTableType READONLY, @EDDataset IDsTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;
	
	SELECT COUNT(*) AS EDRows
	FROM dbo.Targets_TreatAndReleaseTargets AS ED
		LEFT JOIN Hospitals AS Hosp ON ED.LocalHospitalID = Hosp.LocalHospitalID AND Hosp.IsArchived=0 AND Hosp.IsDeleted=0
	WHERE
		Hosp.Id IN (
			SELECT Id
			FROM @Hospitals
		) AND
		ED.Dataset_Id IN (
			SELECT Id
			FROM @EDDataset
		);
END
GO
