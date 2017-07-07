/*
 * Name:			Temp_UtilIP_Prep
 * Used In:			InpatientReportGenerator.cs
 * Description:		Used to store temporary IP data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_UtilIP_Prep', 'U') IS NOT NULL
	DROP TABLE dbo.Temp_UtilIP_Prep
GO

CREATE TABLE dbo.Temp_UtilIP_Prep
(
	ID uniqueidentifier not null,
	HospitalID int not null,
	RegionID int not null,
	CountyID int not null,
	
	UtilTypeID int not null,			-- UtilTypeID - 1 = DRG, 2 = MDC, 3 = DXCCS, 4 = PRCCS
	UtilID int not null,

	DischargeYear int null,
	Age int null,
	Race int null,
	Sex int null,
	PrimaryPayer int null,
	LengthOfStay decimal(9,3) null,
	TotalCharge decimal(19,2) null,
	TotalCost decimal(19,2) null
)
GO

-- Shortened clustered index so <= 3 columns and <= 16 bytes in length
/*
CREATE CLUSTERED INDEX IDX_Temp_UtilIP_Prep
	ON dbo.Temp_UtilIP_Prep (UtilTypeID, UtilID, HospitalID)
*/

-- Additional non-clustered indexed observed by SQL that should be added. Thses are individual ones for different prep and aggregation.
CREATE INDEX ix_Temp_UtilIP_Prep_01 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, RegionID, CountyID, UtilTypeID, UtilID)
	INCLUDE (Age, Race, Sex, PrimaryPayer, LengthOfStay, TotalCharge, TotalCost)
/*
-- NOTE: These were removed. They were only being utilized once each per generation. Adding these in caused the prep time to skyrocket.
--       It went from 17 minutes for all 4 util types, to over 22 just for the first 3. Removing and testing.

CREATE INDEX ix_Temp_UtilIP_Prep_01 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Age, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_02 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Age, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_03 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Age, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_04 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Sex, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_05 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Sex, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_06 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Sex, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_07 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Race, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_08 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Race, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_09 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Race, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_10 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, PrimaryPayer, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_11 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, PrimaryPayer, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_12 ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, PrimaryPayer, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_13 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_14 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_15 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_16 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Age, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_17 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Age, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_18 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Age, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_19 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Sex, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_20 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Sex, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_21 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Sex, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_22 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Race, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_23 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Race, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_24 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, Race, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_25 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, PrimaryPayer, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_26 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, PrimaryPayer, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_27 ON dbo.Temp_UtilIP_Prep (ID, HospitalID, UtilTypeID, PrimaryPayer, LengthOfStay)
CREATE INDEX ix_Temp_UtilIP_Prep_28 ON dbo.Temp_UtilIP_Prep (ID, RegionID, UtilTypeID, TotalCost)
CREATE INDEX ix_Temp_UtilIP_Prep_29 ON dbo.Temp_UtilIP_Prep (ID, RegionID, UtilTypeID, TotalCharge)
CREATE INDEX ix_Temp_UtilIP_Prep_30 ON dbo.Temp_UtilIP_Prep (ID, RegionID, UtilTypeID, LengthOfStay)

*/
/*
-- This was a second group that was never included.
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (UtilTypeID, UtilID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (UtilTypeID, UtilID, Age)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (UtilTypeID, UtilID, Sex)

CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (HospitalID, UtilTypeID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (HospitalID, UtilTypeID, Age)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (HospitalID, UtilTypeID, Sex)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (HospitalID, UtilTypeID, Race)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (HospitalID, UtilTypeID, PrimaryPayer)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCost) INCLUDE (HospitalID, UtilTypeID, UtilID)


CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (UtilTypeID, UtilID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (UtilTypeID, UtilID, Age)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (UtilTypeID, UtilID, Sex)

CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (HospitalID, UtilTypeID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (HospitalID, UtilTypeID, Age)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (HospitalID, UtilTypeID, Sex)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (HospitalID, UtilTypeID, Race)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (HospitalID, UtilTypeID, PrimaryPayer)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, TotalCharge) INCLUDE (HospitalID, UtilTypeID, UtilID)


CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (UtilTypeID, UtilID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (UtilTypeID, UtilID, Age)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (UtilTypeID, UtilID, Sex)

CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, Age)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, Sex)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, Race)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, PrimaryPayer)

CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, UtilID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, UtilID, Age)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, UtilID, Sex)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, UtilID, Race)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, LengthOfStay) INCLUDE (HospitalID, UtilTypeID, UtilID, PrimaryPayer)

CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, TotalCost) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, TotalCharge) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, LengthOfStay) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Age, TotalCost) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Age, TotalCharge) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Age, LengthOfStay) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Sex, TotalCost) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Sex, TotalCharge) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Sex, LengthOfStay) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Race, TotalCost) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Race, TotalCharge) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, Race, LengthOfStay) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, PrimaryPayer, TotalCost) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, PrimaryPayer, TotalCharge) INCLUDE (HospitalID)
CREATE INDEX ix_Temp_UtilIP_Prep_ ON dbo.Temp_UtilIP_Prep (ID, UtilTypeID, PrimaryPayer, LengthOfStay) INCLUDE (HospitalID)
*/