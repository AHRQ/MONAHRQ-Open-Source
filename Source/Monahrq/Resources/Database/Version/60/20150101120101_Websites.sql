-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0 Build 1
-- Create date: 12-22-2014
-- Description:	This is the update script from older MONAHRQ 5.0 edited Hospitals and Custom Regions to the new 
--              MONAHRQ 6.0
--				'Websites'
-- =============================================

BEGIN TRY

DECLARE @foreignKeyName VARCHAR(400),
        @sql VARCHAR(4000)

SELECT @foreignKeyName=object_name(fkc.constraint_object_id) FROM sys.foreign_key_columns fkc
WHERE fkc.parent_object_id=OBJECT_ID('Websites_WebsiteDatasets')
AND fkc.referenced_object_id=OBJECT_ID('ContentItemRecords')

SELECT @sql='ALTER TABLE [dbo].Websites_WebsiteDatasets DROP CONSTRAINT '+@foreignKeyName
--EXEC (@sql)

SELECT @sql='IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N''[dbo].['+@foreignKeyName+']'') AND parent_object_id = OBJECT_ID(N''Websites_WebsiteDatasets''))	
			 BEGIN
				ALTER TABLE [dbo].[Websites_WebsiteDatasets]  WITH CHECK ADD  CONSTRAINT ['+@foreignKeyName+'] FOREIGN KEY([Dataset_Id])
				REFERENCES [dbo].[Wings_Datasets] ([Id])
				ALTER TABLE [dbo].[Websites_WebsiteDatasets] CHECK CONSTRAINT ['+@foreignKeyName+']
				END'
				
--EXEC (@sql)

SELECT 1;

END TRY
BEGIN CATCH
    DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
END CATCH;
