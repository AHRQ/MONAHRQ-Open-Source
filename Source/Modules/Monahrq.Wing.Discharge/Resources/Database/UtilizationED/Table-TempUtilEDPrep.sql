/*
 *      Name:           Temp_UtilED_Prep
 *      Used In:        TreatAndReleaseReportGenerator.cs
 *      Description:    Used to store temporary ED data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilED_Prep', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilED_Prep
GO

CREATE TABLE dbo.Temp_UtilED_Prep
(
	ID uniqueidentifier not null,
	HospitalId int not null,
    RegionID int not null,
    CountyID int not null,

    CCSID int not null,
    DataSource int not null,

    Died int null,
    DischargeYear int null,
    Age int null,
    Race int null,
    Sex int null,
    PrimaryPayer int null
)
GO

-- Shortened clustered index so <= 3 columns and <= 16 bytes in length
/*
CREATE CLUSTERED INDEX IDX_Temp_UtilED_Prep
	ON dbo.Temp_UtilED_Prep (CCSID, DataSource, HospitalID)
*/

-- Additional non-clustered indexed observed by SQL that should be added. Thses are individual ones for different prep and aggregation.
CREATE INDEX ix_Temp_UtilED_Prep_01 ON dbo.Temp_UtilED_Prep (ID, HospitalID, RegionID, CountyID, CCSID, DataSource)
	INCLUDE (Died, DischargeYear, Age, Race, Sex, PrimaryPayer)
