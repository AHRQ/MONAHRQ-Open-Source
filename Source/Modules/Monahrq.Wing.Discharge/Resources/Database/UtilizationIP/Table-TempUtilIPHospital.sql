/*
 *      Name:           Temp_UtilIP_Hospital
 *      Used In:        InpatientReportGenerator.cs
 *      Description:    Used to store temporary IP data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilIP_Hospital', 'U') IS NOT NULL
  DROP TABLE dbo.Temp_UtilIP_Hospital
GO

CREATE TABLE dbo.Temp_UtilIP_Hospital
(
	ID uniqueidentifier not null,
    HospitalID int not null,
	RegionID int not null,
    CountyID int not null,
    Zip nvarchar(12) not null,
    HospitalType nvarchar(MAX) not null,

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
CREATE CLUSTERED INDEX IDX_Temp_UtilIP_Hospital
    ON dbo.Temp_UtilIP_Hospital (UtilTypeID, UtilID, HospitalID)
GO

-- Index (228 uses)
CREATE INDEX IDX_Temp_UtilIP_Hospital_01
	ON dbo.Temp_UtilIP_Hospital (ID, HospitalID, UtilTypeID, UtilID, CatID, CatVal)
	INCLUDE (Discharges, MeanCharges, MeanCosts, MeanLOS, MedianCharges, MedianCosts, MedianLOS)
GO

-- Index (24 uses) 
CREATE INDEX IDX_Temp_UtilIP_Hospital_02
	ON dbo.Temp_UtilIP_Hospital (ID, HospitalID, RegionID, CountyID, Zip, UtilTypeID, UtilID, CatID, CatVal)
	INCLUDE (HospitalType, Discharges, MeanCharges, MeanCosts, MeanLOS, MedianCharges, MedianCosts, MedianLOS)
GO

/*
-- Index (1 uses) 
CREATE INDEX IDX_Temp_UtilIP_Hospital_03
	ON dbo.Temp_UtilIP_Hospital (ID, HospitalID)
	INCLUDE (UtilTypeID, UtilID, CatID, CatVal)

-- Index (1 uses) 
CREATE INDEX IDX_Temp_UtilIP_Hospital_04
	ON dbo.Temp_UtilIP_Hospital (ID)
	INCLUDE (HospitalID, UtilTypeID, UtilID, CatID, CatVal)
*/
