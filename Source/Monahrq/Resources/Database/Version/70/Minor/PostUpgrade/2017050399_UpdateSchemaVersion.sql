BEGIN TRY 

	UPDATE SchemaVersions SET Version = '7.4.0.8' where [Name] = 'Database Schema'

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