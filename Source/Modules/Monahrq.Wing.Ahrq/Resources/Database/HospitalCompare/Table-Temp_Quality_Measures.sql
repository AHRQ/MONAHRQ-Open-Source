/*
 *      Name:           Temp_Quality_Measures
 *      Version:        1.0
 *      Last Updated:   5/1/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to store temporary quality measures utilized in the json file generation.
 */

IF OBJECT_ID('dbo.Temp_Quality_Measures', 'U') IS NOT NULL
	DROP TABLE dbo.Temp_Quality_Measures
GO

CREATE TABLE dbo.Temp_Quality_Measures (
    ReportID UNIQUEIDENTIFIER NOT NULL,
    MeasureID int NOT NULL,
    MeasureName nvarchar(255) NULL,
    MeasureSource nvarchar(20) NULL,
    MeasureType nvarchar(max) NULL,
    HigherScoresAreBetter bit null,
    HigherScoresAreBetterDescription nvarchar(100) NULL,
    TopicsID nvarchar(max) NULL,
    -- National and peer info
    NatLabel nvarchar(100) NULL,
    NatRateAndCI nvarchar(100) NULL,
    NatTop10Label nvarchar(100) NULL,
    NatTop10 nvarchar(100) NULL,
    PeerLabel nvarchar(100) NULL,
    PeerRateAndCI nvarchar(100) NULL,
    PeerTop10Label nvarchar(100) NULL,
    PeerTop10 nvarchar(100) NULL,
    Footnote nvarchar(max) NULL,
    BarHeader nvarchar(100) NULL,
    BarFooter nvarchar(100) NULL,
    -- Columns descriptions
    ColDesc1 nvarchar(100) NULL,
    ColDesc2 nvarchar(100) NULL,
    ColDesc3 nvarchar(100) NULL,
    ColDesc4 nvarchar(100) NULL,
    ColDesc5 nvarchar(100) NULL,
    ColDesc6 nvarchar(100) NULL,
    ColDesc7 nvarchar(100) NULL,
    ColDesc8 nvarchar(100) NULL,
    ColDesc9 nvarchar(100) NULL,
    ColDesc10 nvarchar(100) NULL,
    -- National rates
    NatCol1 nvarchar(100) NULL,
    NatCol2 nvarchar(100) NULL,
    NatCol3 nvarchar(100) NULL,
    NatCol4 nvarchar(100) NULL,
    NatCol5 nvarchar(100) NULL,
    NatCol6 nvarchar(100) NULL,
    NatCol7 nvarchar(100) NULL,
    NatCol8 nvarchar(100) NULL,
    NatCol9 nvarchar(100) NULL,
    NatCol10 nvarchar(100) NULL,
    -- Peer rates
    PeerCol1 nvarchar(100) NULL,
    PeerCol2 nvarchar(100) NULL,
    PeerCol3 nvarchar(100) NULL,
    PeerCol4 nvarchar(100) NULL,
    PeerCol5 nvarchar(100) NULL,
    PeerCol6 nvarchar(100) NULL,
    PeerCol7 nvarchar(100) NULL,
    PeerCol8 nvarchar(100) NULL,
    PeerCol9 nvarchar(100) NULL,
    PeerCol10 nvarchar(100) NULL,
    -- Extra stuff for help popup
    SelectedTitle nvarchar(max) NULL,
    PlainTitle nvarchar(max) NULL,
    ClinicalTitle nvarchar(max) NULL,
    MeasureDescription nvarchar(max) NULL,
	
	SelectedTitleConsumer nvarchar(max) NULL,
	PlainTitleConsumer nvarchar(max) NULL,
	MeasureDescriptionConsumer nvarchar(max) NULL,

    Bullets nvarchar(max) NULL,
    StatisticsAvailable nvarchar(max) NULL,
    MoreInformation nvarchar(max) NULL,
    URL nvarchar(500) NULL,
    URLTitle nvarchar(500) NULL,
    DataSourceURL nvarchar(500) NULL,
    DataSourceURLTitle nvarchar(500) NULL
)
GO