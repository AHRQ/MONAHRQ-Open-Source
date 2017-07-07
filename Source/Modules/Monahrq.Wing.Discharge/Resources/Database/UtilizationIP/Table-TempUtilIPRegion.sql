/*
 *      Name:           Temp_UtilIP_Region
 *      Used In:        InpatientReportGenerator.cs
 *      Description:    Used to store temporary IP data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilIP_Region', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilIP_Region
GO

CREATE TABLE dbo.Temp_UtilIP_Region
(
	ID uniqueidentifier not null,
    RegionID int not null,
	
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

CREATE CLUSTERED INDEX IDX_Temp_UtilIP_Region
    ON dbo.Temp_UtilIP_Region (UtilTypeID, UtilID, RegionID)
GO

-- 12 Uses
CREATE INDEX IDX_Temp_UtilIP_Region_01
	ON dbo.Temp_UtilIP_Region (ID, RegionID, UtilTypeID, UtilID, CatID, CatVal)
	INCLUDE (Discharges, MeanCharges, MeanCosts, MeanLOS, MedianCharges, MedianCosts, MedianLOS)
GO