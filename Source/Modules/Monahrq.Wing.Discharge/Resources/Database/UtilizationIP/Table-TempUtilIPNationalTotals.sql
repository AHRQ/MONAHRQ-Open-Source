/*
 *      Name:           Temp_UtilIP_NationalTotals
 *      Used In:        InpatientReportGenerator.cs
 *      Description:    Used to store temporary IP data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilIP_NationalTotals', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilIP_NationalTotals
GO

CREATE TABLE dbo.Temp_UtilIP_NationalTotals
(
	UtilTypeID int not null,			-- UtilTypeID - 1 = DRG, 2 = MDC, 3 = DXCCS, 4 = PRCCS
	UtilID int not null,
	Discharges int not null,
	DischargesStdErr int not null,
	Charges int not null,
	ChargesStdErr int not null,
	Costs int not null,
	CostsStdErr int not null,
	LOS int not null,
	LOSStdErr int not null
)
GO

CREATE CLUSTERED INDEX IDX_Temp_UtilIP_NationalTotals
    ON dbo.Temp_UtilIP_NationalTotals (UtilTypeID, UtilID)
GO
