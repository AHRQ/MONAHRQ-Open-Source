/*
 *      Name:           Temp_UtilCounty_Prep
 *      Used In:        CountyReportGenerator.cs
 *      Description:    Used to store temporary county data utilized in the json file generation.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Temp_UtilCounty_Prep]') AND TYPE IN (N'U', N'TT'))
	DROP TABLE [dbo].[Temp_UtilCounty_Prep];

CREATE TABLE [dbo].[Temp_UtilCounty_Prep]
(
	ID uniqueidentifier not null,
	CountyID int not null,
	StateCountyFIPS int not null,

	UtilTypeID int not null,
	UtilID int not null,

    DischargeYear int null,
    Age int null,
    Race int null,
    Sex int null,
    PrimaryPayer int null,
	TotalCost decimal(19,2) null
);

-- Shortened clustered index so <= 3 columns and <= 16 bytes in length
/*
CREATE CLUSTERED INDEX IDX_Temp_UtilCounty_Prep
	ON dbo.Temp_UtilCounty_Prep (UtilTypeID, UtilID, CountyID)
*/

-- Additional non-clustered indexed observed by SQL that should be added. Thses are individual ones for different prep and aggregation.
CREATE INDEX ix_Temp_UtilCounty_Prep_01 ON dbo.Temp_UtilCounty_Prep (ID, CountyID, StateCountyFIPS, UtilTypeID, UtilID)
	INCLUDE (Age, Race, Sex, PrimaryPayer, TotalCost)
