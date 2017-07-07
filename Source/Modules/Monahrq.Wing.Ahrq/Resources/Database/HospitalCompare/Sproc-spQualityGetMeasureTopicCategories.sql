/*
 *      Name:           spQualityGetMeasureTopicCategories
 *      Version:        1.0
 *      Last Updated:   6/17/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to get quality measure topic categories for use in json file generation.
 */
 
IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityGetMeasureTopicCategories' 
)
	DROP PROCEDURE dbo.spQualityGetMeasureTopicCategories
GO

CREATE PROCEDURE [dbo].[spQualityGetMeasureTopicCategories]
	@ReportID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

SELECT DISTINCT	dbo.TopicCategories.Id AS TopicCategoryID
			,	dbo.TopicCategories.Name
			,	dbo.TopicCategories.LongTitle
			,	dbo.TopicCategories.[Description]
		--	,	dbo.TopicCategories.[CategoryType] as Type
	
			,	dbo.TopicCategories.[TopicFacts1]
			,	dbo.TopicCategories.[TopicFacts2]
			,	dbo.TopicCategories.[TopicFacts3]
			,	dbo.TopicCategories.[TipsChecklist]
			,	dbo.TopicCategories.[TopicIcon]
FROM			dbo.TopicCategories
    JOIN		dbo.Topics ON dbo.TopicCategories.Id = dbo.Topics.TopicCategory_id
    JOIN		dbo.Measures_MeasureTopics ON dbo.Topics.Id = dbo.Measures_MeasureTopics.Topic_Id
WHERE			dbo.Measures_MeasureTopics.Measure_Id IN (
					SELECT DISTINCT	MeasureID AS ID
					FROM			dbo.Temp_Quality_Measures
					WHERE			ReportID = @ReportID
				)
ORDER BY dbo.TopicCategories.Name

END
