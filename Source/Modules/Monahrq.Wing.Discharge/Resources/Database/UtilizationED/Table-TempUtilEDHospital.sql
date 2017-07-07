/*
 *      Name:           Temp_UtilED_Hospital
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *      Description:    Used to store temporary ED data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilED_Hospital', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilED_Hospital
GO

CREATE TABLE dbo.Temp_UtilED_Hospital
(
    ID uniqueidentifier not null,
    HospitalID int not null,
    RegionID int null,
    CountyID int null,
    Zip nvarchar(12) null,
    HospitalType nvarchar(MAX) null,

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
CREATE CLUSTERED INDEX IDX_Temp_UtilED_Hospital
	ON dbo.Temp_UtilED_Hospital (CCSID, CatID, CatVal, HospitalID)
GO

-- Additional non-clustered indexed observed by SQL that should be added. Thses are individual ones for different prep and aggregation.
CREATE INDEX ix_Temp_UtilED_Hospital_01 ON dbo.Temp_UtilED_Hospital (ID, HospitalID, CCSID, CatID, CatVal)
	INCLUDE (NumEdVisits, NumAdmitHosp, DiedEd, DiedHosp)

CREATE INDEX ix_Temp_UtilED_Hospital_02 ON dbo.Temp_UtilED_Hospital (ID, HospitalID, RegionID, CountyID, CCSID, CatID, CatVal)
	INCLUDE (NumEdVisits, NumAdmitHosp, DiedEd, DiedHosp)
