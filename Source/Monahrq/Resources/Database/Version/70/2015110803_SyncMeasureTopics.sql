BEGIN TRY 
	
	IF (COLUMNPROPERTY(OBJECT_ID(N'Measures_MeasureTopics'), 'Id', 'IsIdentity')) IS NULL
		BEGIN
			if exists(select * from sys.columns where Name = N'Id' AND Object_ID = Object_ID(N'Measures_MeasureTopics'))
				BEGIN
					ALTER TABLE Measures_MeasureTopics DROP COLUMN Id
				END
		END

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

