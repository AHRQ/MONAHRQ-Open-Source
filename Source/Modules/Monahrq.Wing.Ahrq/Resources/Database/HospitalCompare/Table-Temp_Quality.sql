/*
 *      Name:           Temp_Quality
 *      Version:        1.0
 *      Last Updated:   4/9/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to store temporary quality data utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_Quality', 'U') IS NOT NULL
	DROP TABLE dbo.Temp_Quality
GO

CREATE TABLE dbo.Temp_Quality(
    ReportID UNIQUEIDENTIFIER NOT NULL,
    MeasureID int NOT NULL,
    HospitalID int NULL,
    CountyID int NULL,
    RegionID int NULL,
    ZipCode nvarchar(12) NULL,
    HospitalType nvarchar(MAX) NULL,
    RateAndCI nvarchar(255),
    NatRating nvarchar(20) NULL,
    NatFilled int NULL,
    PeerRating nvarchar(20) NULL,
    PeerFilled int NULL,
    Col1 nvarchar(100) NULL,
    Col2 nvarchar(100) NULL,
    Col3 nvarchar(100) NULL,
    Col4 nvarchar(100) NULL,
    Col5 nvarchar(100) NULL,
    Col6 nvarchar(100) NULL,
    Col7 nvarchar(100) NULL,
    Col8 nvarchar(100) NULL,
    Col9 nvarchar(100) NULL,
    Col10 nvarchar(100) NULL
)
GO