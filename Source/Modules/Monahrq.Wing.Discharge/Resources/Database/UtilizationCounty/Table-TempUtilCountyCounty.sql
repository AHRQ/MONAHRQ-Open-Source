/*
 *      Name:           Temp_UtilCounty_County
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Used to store temporary County data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilCounty_County', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilCounty_County
GO

CREATE TABLE dbo.Temp_UtilCounty_County
(
	ID uniqueidentifier not null,
    CountyID int not null,

	UtilTypeID int not null,		-- UtilTypeID - 1 = DRG, 2 = MDC, 3 = DXCCS, 4 = PRCCS
	UtilID int not null,			-- ID for the utilization (i.e. DRG = 1-594)

    CatID int not null,				-- 1 = , 2 = , 3 = , 4 =
    CatVal int not null,

    Discharges int null,
    RateDischarges decimal(17,3) null,
    MeanCosts int null,
    MedianCosts int null
)
GO

-- Overall clustered index for the table
CREATE CLUSTERED INDEX IDX_Temp_UtilCounty_County
    ON dbo.Temp_UtilCounty_County (UtilTypeID, UtilID, CountyID)
GO

CREATE INDEX IDX_Temp_UtilCounty_County_ID_01
	ON dbo.Temp_UtilCounty_County (ID, CountyID, UtilTypeID, UtilID, CatID, CatVal)
	INCLUDE (Discharges, RateDischarges, MeanCosts, MedianCosts)
GO