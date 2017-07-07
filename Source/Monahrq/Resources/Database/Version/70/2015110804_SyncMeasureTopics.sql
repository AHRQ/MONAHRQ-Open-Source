BEGIN TRY 
	
	
	if (COLUMNPROPERTY(OBJECT_ID(N'Measures_MeasureTopics'), 'Id', 'IsIdentity')) IS NULL
	begin	
		alter table [dbo].[Measures_MeasureTopics] add Id int identity;
		alter table [dbo].[Measures_MeasureTopics] add constraint [PK_MeasureTopicsV7]
			primary key clustered (Id asc)
			with (pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on);
	end
	alter table [dbo].[Measures_MeasureTopics] alter column Measure_Id INT NULL;
	alter table [dbo].[Measures_MeasureTopics] alter column Topic_Id INT NULL;

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

