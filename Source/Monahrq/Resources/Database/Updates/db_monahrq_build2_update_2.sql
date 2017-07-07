-- =============================================
-- Author:		Jason Duffus
-- Modified by: Shafiul Alam
-- Project:		MONAHRQ 5.0 Build 2
-- Create date: 08-07-2014
-- Modified date: 10-15-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 AHRQ Targets to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'AHRQ QI Data'
-- =============================================

--BEGIN TRY
-- Disable nonclustered Index

IF OBJECT_ID('tempdb..#IndexList') IS NOT NULL
DROP TABLE #IndexList 

SELECT identity(INT,1,1) rownum,
	   i.name 	
	   INTO #IndexList 
 FROM sys.indexes i
WHERE i.[object_id]=OBJECT_ID('[@@DESTINATION@@].[dbo].[Targets_AhrqTargets]')
AND i.[type]=2

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #IndexList),
		@sql NVARCHAR(800)
		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_AhrqTargets] DISABLE'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

	DECLARE @Dataset_Id INT, @DatasetType NVARCHAR(100), @DatasetSummary_Id INT;
	DECLARE @Dataset_Cursor CURSOR;

	SET @Dataset_Cursor =  CURSOR FOR
	SELECT c.Id,
	       t.Name,
	       s.Id
	FROM   [@@SOURCE@@].[dbo].[ContentItemRecords] c
	       INNER JOIN [@@SOURCE@@].[dbo].[ContentTypeRecords] t
	            ON  t.Id = c.[ContentType_Id]
	       INNER JOIN [@@SOURCE@@].[dbo].[ContentItemSummaryRecords] s
	            ON  s.Id = c.[Summary_Id]
	WHERE  t.Name IN ('AHRQ-QI Area Data', 'AHRQ-QI Composite Data', 
	                 'AHRQ-QI Provider Data');

	OPEN @Dataset_Cursor;
	FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id

	WHILE @@fetch_status = 0
	BEGIN

		DECLARE @NewDataSetId INT, @NewSummeryId INT, @NewTypeId INT

		PRINT 'ContentItemRecordId: ' + CAST(@Dataset_Id AS NVARCHAR);
		PRINT 'ContentItemRecordId: ' + @DatasetType;
		PRINT 'ContentItemSummaryRecordId: ' + CAST(@DatasetSummary_Id AS NVARCHAR);
		Print char(13);

		-- ContentItemSummaryRecords
		-- ***************************
		INSERT INTO [@@DESTINATION@@].[dbo].[ContentItemSummaryRecords]
		SELECT s.[Data]
		FROM   [@@SOURCE@@].[dbo].[ContentItemSummaryRecords] s
		       INNER JOIN [@@SOURCE@@].[dbo].[ContentItemRecords] c
		            ON  c.[Summary_Id] = s.[Id]
		WHERE  c.[Id] = @Dataset_Id;

		SELECT @NewSummeryId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[ContentItemSummaryRecords]');
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
				@NewSummeryId
		FROM [@@SOURCE@@].[dbo].[ContentItemRecords] c
		WHERE c.[Id] =  @Dataset_Id

		SELECT @NewDataSetId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[ContentItemRecords]');

		PRINT 'Finished inserting ContentItemRecords. Id: ' + CAST(@NewDataSetId AS NVARCHAR);

		-- ContentPartTransactionRecords
		-- ***************************
		INSERT INTO [@@DESTINATION@@].[dbo].[ContentPartTransactionRecords]
		SELECT tr.[Code],
		       tr.[Message],
		       tr.[Extension],
		       tr.[Data],
		       @NewDataSetId
		FROM   [@@SOURCE@@].[dbo].[ContentPartTransactionRecords] tr
		WHERE  tr.[ContentItemRecord_Id] = @Dataset_Id

		PRINT 'Finished inserting ContentPartTransactionRecords.';

		INSERT INTO [@@DESTINATION@@].[dbo].[Targets_AhrqTargets]
		SELECT
				[TargetType], 
				Replace(Replace((CASE
				     WHEN LOWER([TargetType]) = 'composite' THEN (CASE 
																WHEN UPPER([MeasureCode]) = 'IQICOND' THEN 'IQI 91'
																WHEN UPPER([MeasureCode]) = 'IQIPROC' THEN 'IQI 90'
																WHEN UPPER([MeasureCode]) = 'PSICOMP' THEN 'PSI 90'
														        ELSE [MeasureCode]
															END)
				ELSE SUBSTRING([MeasureCode],1,3) + ' ' + 
						LTRIM(CASE
						WHEN LEN(RTRIM(LTRIM(SUBSTRING([MeasureCode],4,LEN([MeasureCode]))))) >= 2 THEN SUBSTRING([MeasureCode],4,LEN([MeasureCode]))
						ELSE '0' + SUBSTRING([MeasureCode],4,LEN([MeasureCode]))
						END)
				END), N'  ', N' '), N'  ', N' '), 
				[Stratification]
			,[LocalHospitalID]
			,[CountyFIPS]
			,[ObservedNumerator]
			,[ObservedDenominator]
			,[ObservedRate]
			,[ObservedCIHigh]
			,[ObservedCILow]
			,[RiskAdjustedRate]
			,[RiskAdjustedCIHigh]
			,[RiskAdjustedCILow]
			,[ExpectedRate]
			,[StandardErr]
			,[Threshold]
			,[NatBenchmarkRate]
			,[NatRating]
			,[PeerBenchmarkRate]
			,[PeerRating]
			,[TotalCost]
			,@NewDataSetId
		FROM [@@SOURCE@@].[dbo].[Targets_AhrqTargets]
		WHERE [ContentItemRecord_Id] =  @Dataset_Id 

	FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id
END

--END TRY
--BEGIN CATCH
		--IF @@TRANCOUNT > 0
		--ROLLBACK TRANSACTION;

		--PRINT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE AS ErrorMessage;

		--SELECT -1;
--END CATCH

--IF @@TRANCOUNT > 0
--		COMMIT TRANSACTION;

CLOSE @Dataset_Cursor;
DEALLOCATE @Dataset_Cursor;

SET @LoopIterator=1		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_AhrqTargets] REBUILD'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

SELECT 1;