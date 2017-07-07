BEGIN TRY 

		IF(Object_ID(N'MeasureTopicTemp')) IS NOT NULL 
			DROP TABLE MeasureTopicTemp
			
		IF(Object_ID(N'TopicToTopicCategoryTemp')) IS NOT NULL 
			DROP TABLE TopicToTopicCategoryTemp
		
		IF(Object_ID(N'TopicTemp')) IS NOT NULL 
			DROP TABLE TopicTemp
		
		IF(Object_ID(N'HospitalsTemp')) IS NOT NULL 
			DROP TABLE [HospitalsTemp]

		IF(OBJECT_ID(N'BaseCostToChargesTemp')) IS NOT NULL 
			DROP TABLE BaseCostToChargesTemp

		IF(OBJECT_ID(N'BaseMSDRGsTemp')) IS NOT NULL 
			DROP TABLE BaseMSDRGsTemp
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