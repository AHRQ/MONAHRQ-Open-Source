-- =============================================
-- Author:		Jason Duffus
-- Modified by: Shafiul Alam
-- Project:		MONAHRQ 5.0 Build 2
-- Create date: 08-07-2014
-- Modified by: 10-15-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 Medicare Provider Charge Data Targets to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'Medicare Provider Charge Data'
-- =============================================
--BEGIN TRY

-- Disable nonclustered Index

IF OBJECT_ID('tempdb..#IndexList') IS NOT NULL
DROP TABLE #IndexList 

SELECT identity(INT,1,1) rownum,
	   i.name 	
	   INTO #IndexList 
 FROM sys.indexes i
WHERE i.[object_id]=OBJECT_ID('[@@DESTINATION@@].[dbo].[Targets_MedicareProviderChargeTargets]')
AND i.[type]=2

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #IndexList),
		@sql NVARCHAR(800)
		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_MedicareProviderChargeTargets] DISABLE'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

	DECLARE @Dataset_Id INT, @DatasetType NVARCHAR(100), @DatasetSummary_Id INT;
	DECLARE @Dataset_Cursor CURSOR;

	SET @Dataset_Cursor = CURSOR FOR
	SELECT c.Id,
	       t.Name,
	       s.Id
	FROM   [@@SOURCE@@].[dbo].[ContentItemRecords] c
	       INNER JOIN [@@SOURCE@@].[dbo].[ContentTypeRecords] t
	            ON  t.Id = c.[ContentType_Id]
	       LEFT OUTER JOIN [@@SOURCE@@].[dbo].[ContentItemSummaryRecords] s
	            ON  s.Id = c.[Summary_Id]
	WHERE  t.Name IN ('Medicare Provider Charge Data');

	OPEN @Dataset_Cursor;
	FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id

	WHILE @@fetch_status = 0
	BEGIN

		DECLARE @NewDataSetId INT, @NewSummeryId INT, @NewTypeId INT
		SELECT @NewTypeId = s.Id FROM [@@DESTINATION@@].[dbo].[ContentTypeRecords] s WHERE s.Name = @DatasetType;

		PRINT 'Finished inserting Summary Records. Summery Id: ' + CAST(@NewSummeryId AS NVARCHAR);

		-- ContentItemRecords
		-- ***************************
		INSERT INTO [@@DESTINATION@@].[dbo].[ContentItemRecords]
		SELECT c.[Data], 
				c.[File], 
				c.[Description], 
				c.[DateImported], 
				c.[ReportingQuarter], 
				c.[ReportingYear], 
			    c.[DRGMDCMappingStatus], 
				c.[DRGMDCMappingStatusMessage], 
				c.[IsFinished], 
				@NewTypeId, 
				null
		FROM [@@SOURCE@@].[dbo].[ContentItemRecords] c
		WHERE c.[Id] =  @Dataset_Id

		SELECT @NewDataSetId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[ContentItemRecords]');

		PRINT 'Finished inserting ContentItemRecords. Id: ' + CAST(@NewDataSetId AS NVARCHAR);

		INSERT INTO [@@DESTINATION@@].[dbo].[Targets_MedicareProviderChargeTargets]
		SELECT [DRG_Id],
		       [DRG],
		       [Provider_Id],
		       [TotalDischarges],
		       [AverageCoveredCharges],
		       [AverageTotalPayments],
		       @NewDataSetId
		FROM   [@@SOURCE@@].[dbo].[Targets_MedicareProviderChargeTargets]
		WHERE  [ContentItemRecord_Id] = @Dataset_Id 

	FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id
END

--END TRY
--BEGIN CATCH
--		SELECT -1;
--END CATCH

CLOSE @Dataset_Cursor;
DEALLOCATE @Dataset_Cursor;

SET @LoopIterator=1		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_MedicareProviderChargeTargets] REBUILD'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

SELECT 1;