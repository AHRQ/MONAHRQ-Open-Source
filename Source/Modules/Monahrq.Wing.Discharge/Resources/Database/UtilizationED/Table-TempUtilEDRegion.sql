/*
 *      Name:           Temp_UtilED_Region
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *      Description:    Used to store temporary ED data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilED_Region', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilED_Region
GO

CREATE TABLE dbo.Temp_UtilED_Region
(
    ID uniqueidentifier not null,
    RegionID int not null,

    CCSID int not null,
    CatID int not null,
    CatVal int not null,

    NumEdVisits int null,
    NumAdmitHosp int null,
    DiedEd int null,
    DiedHosp int null
)
GO

-- Shortened clustered index so <= 3 columns and <= 16 bytes in length
CREATE CLUSTERED INDEX IDX_Temp_UtilED_Region
	ON dbo.Temp_UtilED_Region (CCSID, CatID, CatVal, RegionID)
GO

-- Additional non-clustered indexed observed by SQL that should be added. Thses are individual ones for different prep and aggregation.
CREATE INDEX ix_Temp_UtilED_Region_01 ON dbo.Temp_UtilED_Region (ID, RegionID, CCSID, CatID, CatVal)
	INCLUDE (NumEdVisits, NumAdmitHosp, DiedEd, DiedHosp)
