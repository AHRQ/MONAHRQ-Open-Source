/*
 *      Name:           Temp_UtilIP_County
 *      Used In:        InpatientReportGenerator.cs
 *      Description:    Used to store temporary IP data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilIP_County', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilIP_County
GO

CREATE TABLE dbo.Temp_UtilIP_County
(
	ID uniqueidentifier not null,
    CountyID int not null,
	
	UtilTypeID int not null,		-- UtilTypeID - 1 = DRG, 2 = MDC, 3 = DXCCS, 4 = PRCCS
	UtilID int not null,			-- ID for the utilization (i.e. DRG = 1-594)

    CatID int not null,				-- 1 = , 2 = , 3 = , 4 =
    CatVal int not null,

    Discharges int null,
    MeanCharges int null,
    MeanCosts int null,
    MeanLOS decimal(9,3) null,
    MedianCharges int null,
    MedianCosts int null,
    MedianLOS decimal(9,3) null
)
GO

-- Overall clustered index for the table
CREATE CLUSTERED INDEX IDX_Temp_UtilIP_County
    ON dbo.Temp_UtilIP_County (UtilTypeID, UtilID, CountyID)
GO

-- 88 Uses
CREATE INDEX IDX_Temp_UtilIP_County_ID_01
	ON dbo.Temp_UtilIP_County (ID, CountyID, UtilTypeID, UtilID, CatID, CatVal)
	INCLUDE (Discharges, MeanCharges, MeanCosts, MeanLOS, MedianCharges, MedianCosts, MedianLOS)
GO