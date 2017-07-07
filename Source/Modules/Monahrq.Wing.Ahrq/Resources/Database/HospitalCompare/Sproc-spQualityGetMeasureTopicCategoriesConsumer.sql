
IF(object_ID (N'spQualityGetMeasureTopicCategoriesConsumer')) IS NOT NULL 
	DROP PROCEDURE spQualityGetMeasureTopicCategoriesConsumer
GO 

CREATE PROCEDURE spQualityGetMeasureTopicCategoriesConsumer @ReportId uniqueidentifier, @IsCostQualityIncluded bit
AS 
	BEGIN 
		
		
    SET NOCOUNT ON;
		
		IF(OBJECT_ID(N'#TempTable')) IS NOT NULL 
			DROP TABLE #TempTable


		SELECT DISTINCT	dbo.TopicCategories.Id AS TopicCategoryID
					,	dbo.TopicCategories.Name
					,	dbo.TopicCategories.LongTitle
					,	dbo.TopicCategories.[ConsumerDescription]
					,	dbo.TopicCategories.[CategoryType] as Type
					,	dbo.TopicCategories.[TipsChecklist]
					,	dbo.TopicCategories.[TopicIcon]
					,	dbo.TopicCategories.[Facts]

		INTO			#TempTable
		FROM			dbo.TopicCategories
		    JOIN		dbo.Topics ON dbo.TopicCategories.Id = dbo.Topics.TopicCategory_id
		    JOIN		dbo.Measures_MeasureTopics ON dbo.Topics.Id = dbo.Measures_MeasureTopics.Topic_Id
		WHERE			dbo.Measures_MeasureTopics.Measure_Id IN (
							SELECT DISTINCT	MeasureID AS ID
							FROM			dbo.Temp_Quality_Measures
							WHERE			ReportID = @ReportID
						)
		
		
		if(@IsCostQualityIncluded = 0)
			BEGIN 
				DELETE FROM #TempTable
				WHERE Name = 'Health Care Cost and Quality'
			END 

		SELECT			[TopicCategoryID]
					,	[Name]
					,	[LongTitle]
					,	[ConsumerDescription]
					,	[TipsChecklist]
					,	[TopicIcon]
					,	[Facts]
		FROM			#TempTable
		ORDER BY		Name

	END


