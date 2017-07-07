/*
 *      Name:           spQualityGetMeasure
 *      Version:        1.0
 *      Last Updated:   5/1/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to get detailed info about one quality measure for use in json file generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityGetMeasure' 
)
	DROP PROCEDURE dbo.spQualityGetMeasure
GO

CREATE PROCEDURE [dbo].[spQualityGetMeasure]
	@ReportID uniqueidentifier, @MeasureID int
AS
BEGIN
    SET NOCOUNT ON;

SELECT  tqm.MeasureID, 
		tqm.MeasureName AS MeasuresName, 
		tqm.MeasureSource, 
		tqm.MeasureType,
		tqm.HigherScoresAreBetter, 
		tqm.HigherScoresAreBetterDescription, 
		tqm.TopicsID,
		tqm.NatLabel,
		tqm.NatRateAndCI, 
		tqm.NatTop10Label, 
		tqm.NatTop10, 
		tqm.PeerLabel, 
		tqm.PeerRateAndCI, 
		tqm.PeerTop10Label, 
		tqm.PeerTop10, 
		tqm.Footnote, 
		tqm.BarHeader, 
		tqm.BarFooter,
		tqm.ColDesc1,
		tqm.ColDesc2, 
		tqm.ColDesc3, 
		tqm.ColDesc4, 
		tqm.ColDesc5, 
		tqm.ColDesc6, 
		tqm.ColDesc7, 
		tqm.ColDesc8, 
		tqm.ColDesc9, 
		tqm.ColDesc10,
		tqm.NatCol1, 
		tqm.NatCol2, 
		tqm.NatCol3, 
		tqm.NatCol4, 
		tqm.NatCol5, 
		tqm.NatCol6, 
		tqm.NatCol7, 
		tqm.NatCol8, 
		tqm.NatCol9, 
		tqm.NatCol10,
		tqm.PeerCol1, 
		tqm.PeerCol2, 
		tqm.PeerCol3, 
		tqm.PeerCol4, 
		tqm.PeerCol5, 
		tqm.PeerCol6, 
		tqm.PeerCol7, 
		tqm.PeerCol8, 
		tqm.PeerCol9, 
		tqm.PeerCol10,
		tqm.SelectedTitle, 
		tqm.PlainTitle, 
		tqm.ClinicalTitle, 
		tqm.MeasureDescription,
		tqm.SelectedTitleConsumer,
		tqm.PlainTitleConsumer,
		tqm.MeasureDescriptionConsumer,
		tqm.Bullets, 
		tqm.StatisticsAvailable, 
		tqm.MoreInformation, 
		tqm.URL, 
		tqm.URLTitle, 
		tqm.DataSourceURL, 
		tqm.DataSourceURLTitle,
		m.SupportsCost
FROM dbo.Temp_Quality_Measures tqm
	INNER JOIN Measures m on tqm.MeasureID = m.Id
WHERE MeasureID = @MeasureID AND ReportID = @ReportID

END