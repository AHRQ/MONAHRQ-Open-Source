/*
 *      Name:           Temp_UtilRegion_Region
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Used to store temporary County data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilRegion_Region', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilRegion_Region
GO

CREATE TABLE dbo.Temp_UtilRegion_Region
(
	ID uniqueidentifier not null,
    RegionID int not null,

	UtilTypeID int not null,		-- UtilTypeID - 1 = DRG, 2 = MDC, 3 = DXCCS, 4 = PRCCS
	UtilID int not null,			-- ID for the utilization (i.e. DRG = 1-594)

    CatID int not null,				-- 1 = , 2 = , 3 = , 4 =
    CatVal int not null,

    Discharges int null,
    RateDischarges decimal(17,3) null,
	MeanCharges int null,
    MeanCosts int null,
    MeanLOS decimal(9,3) null,
    MedianCharges int null,
    MedianCosts int null,
    MedianLOS decimal(9,3) null
)
GO

-- Overall clustered index for the table
CREATE CLUSTERED INDEX IDX_Temp_UtilRegion_Region
    ON dbo.Temp_UtilRegion_Region (UtilTypeID, UtilID, RegionID)
GO

CREATE INDEX IDX_Temp_UtilRegion_Region_ID_01
	ON dbo.Temp_UtilRegion_Region (ID, RegionID, UtilTypeID, UtilID, CatID, CatVal)
	INCLUDE (Discharges, RateDischarges, MeanCosts, MedianCosts)
GO