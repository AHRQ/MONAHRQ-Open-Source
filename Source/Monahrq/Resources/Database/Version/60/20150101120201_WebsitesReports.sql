
BEGIN TRY 
		


	--  Drop Websites_WebsiteReports Constraints.
	DECLARE @sql NVARCHAR(MAX) = N'';

	SELECT		@sql += N'' +
						'ALTER TABLE ' + 
						QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' +
						QUOTENAME(OBJECT_NAME(parent_object_id)) + 
						' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
	FROM		sys.foreign_keys fk
	where		QUOTENAME(OBJECT_NAME(fk.parent_object_id)) = '[Websites_WebsiteReports]';

	--PRINT @sql;
	EXEC sp_executesql @sql;

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