BEGIN TRY

-- Script to just delete if needed
--;with UNIQUE_C2C as
--(
--	SELECT  (row_number() over (partition by [ProviderID],[Year] order by [ProviderID],[Year],[Id])) as RowId, 
--			[Id],
--			[ProviderID],
--			[Ratio],
--			[Year] 
--	FROM [dbo].[Base_CostToCharges]
--) 
--delete from [dbo].[Base_CostToCharges] 
--Where [Id] not in (
--					select min(distinct bc.[Id]) 'Id' 
--					from  [dbo].[Base_CostToCharges] bc
--						join UNIQUE_C2C on	bc.Id = UNIQUE_C2C.Id 
--					where RowId = 1 
--					Group By bc.[ProviderID], bc.[Year]
--				  );

TRUNCATE TABLE [dbo].[Base_CostToCharges];

DELETE FROM [dbo].[SchemaVersions] 
WHERE UPPER([Name]) = UPPER('Base_CostToCharges');
  --AND [Year] IN (2010, 2013) 
  --AND [FileName] IN ('CostToChargeRatio-2010','CostToChargeRatio-2013');



END TRY

BEGIN CATCH
DECLARE @ERRORMESSAGE VARCHAR(5000);
    DECLARE @ERRORSEVERITY INT;
    DECLARE @ERRORSTATE INT;

    SELECT @ERRORMESSAGE = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY = ERROR_SEVERITY(),
           @ERRORSTATE = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE, @ERRORSEVERITY, @ERRORSTATE); 

END CATCH