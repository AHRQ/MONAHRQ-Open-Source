/*
 *      Name:           Temp_UtilED_County
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *      Description:    Used to store temporary ED data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilED_County', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilED_County
GO

CREATE TABLE dbo.Temp_UtilED_County
(
    ID uniqueidentifier not null,
    CountyID int null,

    CCSID int null,
    CatID int null,
    CatVal int null,

    NumEdVisits int null,
    NumAdmitHosp int null,
    DiedEd int null,
    DiedHosp int null
)
GO

-- Shortened clustered index so <= 3 columns and <= 16 bytes in length
CREATE CLUSTERED INDEX IDX_Temp_UtilED_County
	ON dbo.Temp_UtilED_County (CCSID, CatID, CatVal, CountyID)
GO

-- Additional non-clustered indexed observed by SQL that should be added. Thses are individual ones for different prep and aggregation.
CREATE INDEX ix_Temp_UtilED_County_01 ON dbo.Temp_UtilED_County (ID, CountyID, CCSID, CatID, CatVal)
	INCLUDE (NumEdVisits, NumAdmitHosp, DiedEd, DiedHosp)
