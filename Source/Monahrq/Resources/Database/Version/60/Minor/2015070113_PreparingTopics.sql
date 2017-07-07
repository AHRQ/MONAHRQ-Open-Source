BEGIN TRY

delete from Measures_MeasureTopics
delete from Topics

MERGE INTO Topics target
USING ( 
		SELECT distinct temp.Name,temp.LongTitle,temp.Description,temp.WingTargetName,temp.IsUserCreated,temp.ConsumerLongTitle,tc.Id as TopicCategory_Id
	  	FROM TopicTemp temp
			Inner join TopicToTopicCategoryTemp ttc on temp.TopicCategoryName = ttc.TopicCategoryName 
			INNER JOIN TopicCategories tc on temp.TopicCategoryName = tc.Name 
	 ) as source (Name,LongTitle, Description, WingTargetName, IsUserCreated, ConsumerLongTitle, TopicCategory_Id)
ON source.Name  = target.Name and target.LongTitle = source.LongTitle  and target.WingTargetName = source.WingTargetName and target.IsUserCreated = source.IsUserCreated and target.ConsumerLongTitle = source.ConsumerLongTitle
and target.TopicCategory_Id = source.TopicCategory_Id
WHEN MATCHED THEN 
	UPDATE SET	target.Name = source.Name, 
				target.LongTitle = source.LongTitle ,
				target.Description = source.Description , 
				target.WingTargetName = source.WingTargetName,
				target.IsUserCreated = source.IsUserCreated ,
				target.ConsumerLongTitle = source.ConsumerLongTitle,
				target.TopicCategory_Id = source.TopicCategory_Id
WHEN NOT MATCHED THEN 
	INSERT (Name, LongTitle, Description, WingTargetName, IsUserCreated, ConsumerLongTitle, TopicCategory_Id)
	VALUES(source.Name, source.LongTitle, source.Description, source.WingTargetName, source.IsUserCreated, source.ConsumerLongTitle, source.TopicCategory_Id);

MERGE INTO Measures_MeasureTopics target
USING ( SELECT t.Id Topic_Id, m.Id as Measure_Id
		FROM MeasureTopicTemp mtt
			inner join Topics t on mtt.TopicName = t.Name
			inner join TopicCategories tc on tc.Name = mtt.TopicCategoryName and t.TopicCategory_Id = tc.Id
			inner join Measures m on m.Name = mtt.MeasureName
	  ) as source(Topic_Id, Measure_Id)
on target.Topic_Id = source.Topic_Id and target.Measure_Id = source.Measure_Id
WHEN MATCHED THEN 
	update set target.Topic_Id = source.Topic_Id, 
			   target.Measure_Id = source.Measure_Id		
WHEN NOT MATCHED THEN 
	insert (Topic_Id, Measure_Id)
	values(source.Topic_Id, source.Measure_Id); 

END TRY
BEGIN CATCH
DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 

END CATCH