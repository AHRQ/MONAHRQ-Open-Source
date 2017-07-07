/*
 *      Name:           spQualityGetMeasureTopics
 *      Version:        1.0
 *      Last Updated:   6/17/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to get quality measure topics for use in json file generation.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityGetMeasureTopics' 
)
	DROP PROCEDURE dbo.spQualityGetMeasureTopics
GO

CREATE PROCEDURE [dbo].[spQualityGetMeasureTopics]
	@ReportID uniqueidentifier, @AudienceType nvarchar(30) = null
AS
BEGIN
    SET NOCOUNT ON;

WITH MeasureTopics AS
    (
        SELECT			ParentTable.Topic_Id AS TopicID
					,	MeasuresID =
							STUFF((
								SELECT		','+ CAST(SubTable.Measure_Id AS NVARCHAR(MAX))
								FROM		Measures_MeasureTopics SubTable
								WHERE		SubTable.Topic_Id = ParentTable.Topic_Id
									AND		SubTable.Measure_Id IN (
												SELECT DISTINCT	MeasureID AS ID
												FROM			dbo.Temp_Quality_Measures
												WHERE			ReportID = @ReportID
											)
								FOR XML PATH('') 
							), 1, 1,'')
		FROM			Measures_MeasureTopics ParentTable
    )
SELECT DISTINCT	dbo.Topics.Id AS TopicID
			,	dbo.Topics.Name
			,	case 
					when @AudienceType = 'consumer' then dbo.Topics.ConsumerLongTitle 
					else dbo.Topics.LongTitle 
				end as LongTitle
			,	dbo.Topics.[Description]
			,	dbo.Topics.TopicCategory_id AS TopicCategoryID
			,	MeasureTopics.MeasuresID
FROM			dbo.Topics
    JOIN		dbo.Measures_MeasureTopics ON dbo.Topics.Id = dbo.Measures_MeasureTopics.Topic_Id
    JOIN		MeasureTopics ON Topics.Id = MeasureTopics.TopicID
WHERE			dbo.Measures_MeasureTopics.Measure_Id IN (
					SELECT DISTINCT	MeasureID AS ID
					FROM			dbo.Temp_Quality_Measures
					WHERE			ReportID = @ReportID
				)
ORDER BY dbo.Topics.Name

END