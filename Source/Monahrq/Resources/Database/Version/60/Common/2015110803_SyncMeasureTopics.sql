BEGIN TRY 
	
	
	if (Object_ID(N'Measures_MeasureTopics')) is not null
	begin
		if exists(select * from sys.columns where Name = N'Id' AND Object_ID = Object_ID(N'Measures_MeasureTopics'))
		begin
			alter table Measures_MeasureTopics drop column Id
		end
	end

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

