BEGIN TRY

	MERGE INTO TopicToTopicCategoryTemp target 
	USING(Values ('Utilization','Inpatient Hospital Utilization'),
				 ('Utilization','ED Utilization'),
				 ('Patient safety','Recommended care'),
				 ('Patient safety','Results of care - Deaths'),
				 ('Patient safety','Results of care - Complications'),
				 ('Patient survey results','Communication'),
				 ('Patient survey results','Environment'),
				 ('Patient survey results','Satisfaction overall'),
				 ('Avoidable Stays Maps','Chronic Lung Conditions'),
				 ('Avoidable Stays Maps','Summary scores'),
				 ('Avoidable Stays Maps','Diabetes'),
				 ('Avoidable Stays Maps','Heart Conditions'),
				 ('Avoidable Stays Maps','Other Conditions'),
				 ('Avoidable Stays Maps','Patient Safety'),
				 ('Avoidable Stays Maps','Procedure Rates'),
				 ('Child health','Child health'),
				 ('Childbirth','Practice patterns'),
				 ('Childbirth','Results of care'),
				 ('COPD(Chronic Obstructive Pulmonary Disease)','Results of care'),
				 ('Deaths or returns to the hospital','All Causes'),
				 ('Deaths or returns to the hospital','Summary scores'),
				 ('Deaths or returns to the hospital','Heart attack and chest pain'),
				 ('Deaths or returns to the hospital','Heart failure'),
				 ('Deaths or returns to the hospital','Heart surgeries and procedures'),
				 ('Deaths or returns to the hospital','Patient safety'),
				 ('Deaths or returns to the hospital','Other surgeries'),
				 ('Deaths or returns to the hospital','Pneumonia'),
				 ('Deaths or returns to the hospital','Stroke'),
				 ('Deaths or returns to the hospital','Surgical patient safety'),
				 ('Emergency department (ED)','Throughput'),
				 ('Heart attack and chest pain','Recommended care - Inpatient'),
				 ('Heart attack and chest pain','Recommended care - Outpatient'),
				 ('Heart attack and chest pain','Results of care'),
				 ('Heart failure','Recommended care'),
				 ('Heart failure','Results of care'),
				 ('Heart surgeries and procedures','Recommended care'),
				 ('Heart surgeries and procedures','Results of care'),
				 ('Hip or knee replacement surgery','Results of care'),
				 ('Imaging','Practice patterns'),
				 ('Infections','Healthcare-associated'),
				 ('Nursing care','Results of care - Complications'),
				 ('Nursing care','Results of care - Deaths'),
				 ('Other surgeries','Practice patterns'),
				 ('Other surgeries','Results of care'),
				 ('Other surgeries','Recommended care'),
				 ('Pneumonia','Recommended care'),
				 ('Pneumonia','Results of care'),
				 ('Prevention and Treatment','Blood Clot'),
				 ('Stroke','Results of care'),
				 ('Summary Scores','Deaths'),
				 ('Summary Scores','Patient safety'),
				 ('Surgical patient safety','Recommended care before surgery'),
				 ('Surgical patient safety','Recommended care after surgery'),
				 ('Surgical patient safety','Results of care'),
				 ('Women health','Women health'),
				 ('Health Care Cost and Quality','Hip Replacement Cost and Quality'),
				 ('General Topic 1','General Subtopic 1'),
				 ('General Topic 1','General Subtopic 2'),
				 ('General Topic 2','General Subtopic 3'),
				 ('General Topic 2','General Subtopic 4'),
				 ('COPD','Results of care'),
				 ('Nursing Home','Health Inspection'),
				 ('Nursing Home','Quality Measures'),
				 ('Nursing Home','Staffing')) as source (TopicCategoryName,TopicName) on source.TopicCategoryName = target.TopicCategoryName and source.TopicName = target.TopicName
	 WHEN MATCHED THEN 
			UPDATE SET target.TopicCategoryName = source.TopicCategoryName,
					   target.TopicName = source.TopicName 
	 WHEN NOT MATCHED THEN 
		   INSERT (TopicCategoryName,TopicName) 
		   VALUES(source.TopicCategoryName,source.TopicName);


	 Insert into TopicToTopicCategoryTemp(TopicCategoryName,TopicName)
	 SELECT tc.Name, tt.Name 
	 FROM TopicTemp tt
			inner join TopicCategories tc on tt.TopicCategoryName = tc.Name
	 WHERE not exists (SELECT * FROM TopicToTopicCategoryTemp tct WHERE tt.Name = tct.TopicName and tt.TopicCategoryName = tct.TopicCategoryName)


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