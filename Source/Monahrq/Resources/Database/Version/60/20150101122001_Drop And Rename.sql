-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0 Build 1
-- Create date: 12-22-2014
-- Description:	This is the update script to drop unused
--              tables , columns , indexes and keys
-- =============================================

BEGIN TRY

DECLARE @name VARCHAR(100),
        @sql VARCHAR(1500),
        @LoopIterator INT=1,
		@LoopCount INT,
		@BackupTable VARCHAR(100),
		@Indexname VARCHAR(200)

/*******************************************
 *  Drop unused Wings table
 *******************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Wings_Generators]') AND type in (N'U'))
DROP TABLE [dbo].[Wings_Generators]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Wings_Features]') AND type in (N'U'))
DROP TABLE [dbo].[Wings_Features]


--/*******************************************
-- *      HOSPITALS
-- *******************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hospitals_HospitalCategories]') AND type in (N'U'))
BEGIN	
	ALTER TABLE [dbo].[Hospitals_HospitalHospitalCategories] DROP CONSTRAINT [FK8FDFC394683B7C43]
	ALTER TABLE dbo.Hospitals_HospitalCategories DROP CONSTRAINT [FK4D09F60A70435D65]
	DROP TABLE [dbo].Hospitals_HospitalCategories
END

IF EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='HospitalCategory_Id' AND c.[object_id]=OBJECT_ID('Hospitals_HospitalHospitalCategories'))
EXEC sp_rename 'dbo.Hospitals_HospitalHospitalCategories.HospitalCategory_Id', 'Category_Id', 'COLUMN';

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hospitals_HospitalHospitalCategories]') AND type in (N'U'))
EXEC sp_rename 'dbo.Hospitals_HospitalHospitalCategories', 'Hospitals_HospitalCategories';

ALTER TABLE [dbo].[Hospitals_HospitalCategories]  WITH CHECK ADD  CONSTRAINT [FK4D09F60AF0DC9B9A] FOREIGN KEY(Hospital_Id) REFERENCES [dbo].[Hospitals] (Id)
ALTER TABLE [dbo].[Hospitals_HospitalCategories] CHECK CONSTRAINT [FK4D09F60AF0DC9B9A]

--/*******************************************
-- * Drop column
-- *******************************************/
 
--IF EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='SelectedRegion_Id' AND c.[object_id]=OBJECT_ID('Hospitals'))
--BEGIN
--ALTER TABLE Hospitals DROP COLUMN SelectedRegion_Id;
--END

-- IF EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='State_id' AND c.[object_id]=OBJECT_ID('Hospitals'))
--BEGIN
--ALTER TABLE Hospitals DROP COLUMN State_id;
--END

-- IF EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='County_id' AND c.[object_id]=OBJECT_ID('Hospitals'))
--BEGIN
--ALTER TABLE Hospitals DROP COLUMN County_id;
--END

--/*******************************************
-- *  Hospitals_RegionPopulationStrats
-- *******************************************/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hospitals_RegionPopulationStrats]') AND type in (N'U'))
--DROP TABLE [dbo].Hospitals_RegionPopulationStrats

--/*******************************************
-- *  Hospitals_RegionPopulations
-- *******************************************/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hospitals_RegionPopulations]') AND type in (N'U'))
--DROP TABLE [dbo].Hospitals_RegionPopulations

--/*******************************************
-- *  Hospitals_HospitalCategories
-- *******************************************/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hospitals_HospitalCategories]') AND type in (N'U'))
--DROP TABLE [dbo].Hospitals_HospitalCategories


--/*******************************************
-- *   BASE_COUNTIES
-- *******************************************/
 
--IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_COUNTY_STATE]') AND parent_object_id = OBJECT_ID(N'[dbo].[Base_counties]'))
--ALTER TABLE [dbo].Base_counties DROP CONSTRAINT FK_COUNTY_STATE

--IF EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='State_id' AND c.[object_id]=OBJECT_ID('Base_counties'))
--BEGIN
--ALTER TABLE Base_counties DROP COLUMN State_id;
--END

--/*******************************************
-- *  WINGS
-- *******************************************/

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Wings_Generators]') AND type in (N'U'))
--DROP TABLE [dbo].[Wings_Generators]

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Wings_Features]') AND type in (N'U'))
--DROP TABLE [dbo].[Wings_Features]

--/*******************************************
-- *  Base_ZipCodeToHRRAndHSAs
-- *******************************************/

--IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Base_ZipCodeToHRRAndHSAs'))
--BEGIN
--SELECT 
--@sql='ALTER TABLE [dbo].[Base_ZipCodeToHRRAndHSAs] DROP CONSTRAINT '+NAME 
--FROM sys.foreign_keys fk
--WHERE fk.parent_object_id= OBJECT_ID('Base_ZipCodeToHRRAndHSAs')

--EXEC (@sql)
--END

--IF EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='State_id' AND c.[object_id]=OBJECT_ID('Base_ZipCodeToHRRAndHSAs'))
--ALTER TABLE Base_ZipCodeToHRRAndHSAs DROP COLUMN State_id

--/*******************************************
-- *  CONTENT ITEM REOCRDS
-- *******************************************/
 
---- ContentPartTransactionRecords Backup table
--SELECT @BackupTable ='ContentPartTransactionRecords_'+CONVERT(VARCHAR(10), GETDATE(), 112)+'_Bkp'

--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@BackupTable) AND type in (N'U'))
--BEGIN
--SET @sql ='SELECT * INTO [dbo].['+@BackupTable+'] From ContentPartTransactionRecords'
--EXEC (@sql)
--END

---- ContentItemRecords Backup table
--SELECT @BackupTable ='ContentItemRecords_'+CONVERT(VARCHAR(10), GETDATE(), 112)+'_Bkp'

--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@BackupTable) AND type in (N'U'))
--BEGIN
--SET @sql ='SELECT * INTO [dbo].['+@BackupTable+'] From ContentItemRecords'
--EXEC (@sql)
--END

---- ContentItemSummaryRecords Backup table
--SELECT @BackupTable ='ContentItemSummaryRecords_'+CONVERT(VARCHAR(10), GETDATE(), 112)+'_Bkp'

--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@BackupTable) AND type in (N'U'))
--BEGIN
--SET @sql ='SELECT * INTO [dbo].['+@BackupTable+'] From ContentItemSummaryRecords'
--EXEC (@sql)
--END

--IF OBJECT_ID('tempdb..#ContentItemRecordsFkey') IS NOT NULL
--DROP TABLE #ContentItemRecordsFkey
 
--SELECT 
--IDENTITY(INT,1,1) rownum,
--OBJECT_NAME(fk.constraint_object_id) fkey,OBJECT_NAME(fk.parent_object_id) parent,c.name ColumnName
--INTO #ContentItemRecordsFkey
-- FROM sys.foreign_key_columns fk
-- INNER JOIN sys.[columns] c
-- ON fk.parent_column_id=c.column_id
-- AND fk.parent_object_id=c.[object_id]
--WHERE fk.referenced_object_id= OBJECT_ID('ContentItemRecords')
--AND OBJECT_NAME(fk.parent_object_id) <> 'Websites_WebsiteDatasets'


--SET     @LoopIterator=1
--SET		@LoopCount = (SELECT COUNT(1) FROM #ContentItemRecordsFkey)

--/*******************************************
-- *  Drop foreign key , index and column
-- *******************************************/
--DECLARE @IsIndexExist BIT=0
--IF  EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Targets_TreatAndReleaseTargets]') 
--            AND name = N'IX_Targets_InpatientTargets_DRG_MDC_ContentItemRecord_Id')
--            SET @IsIndexExist=1

--WHILE (@LoopIterator<=@LoopCount)
--BEGIN
	
--	SET @sql=''
	
--	SELECT @sql='ALTER TABLE [dbo].['+parent+'] DROP CONSTRAINT '+fkey
--	FROM #ContentItemRecordsFkey i
--	WHERE rownum=@LoopIterator
	
--	EXEC (@sql)

--	SET @Indexname=null
	
--	SELECT TOP(1) @Indexname=ix.name,@name=i.parent 
--	FROM sys.index_columns ic
--	INNER JOIN sys.[columns] c
--	ON c.[object_id] = ic.[object_id]
--	AND c.column_id = ic.column_id
--	INNER JOIN sys.indexes ix
--	ON ix.[object_id] = ic.[object_id]
--	AND ix.index_id = ic.index_id
--	INNER JOIN #ContentItemRecordsFkey i
--	ON i.parent=object_name(ic.[object_id])
--	AND i.ColumnName=c.name
--	WHERE i.rownum=@LoopIterator

--    SET @sql=''

--	IF @Indexname IS NOT NULL
--	BEGIN
--	SET @sql='DROP INDEX ['+@Indexname+'] ON [dbo].['+@name+'] '
--	EXEC (@sql)	
--	END
	
--	SELECT @sql='IF EXISTS (SELECT 1 FROM sys.[columns] c WHERE c.name='''+ColumnName+''' AND c.[object_id]=OBJECT_ID('''+parent+'''))
--	             ALTER TABLE [dbo].['+parent+'] DROP COLUMN '+ColumnName
--	FROM #ContentItemRecordsFkey i
--	WHERE rownum=@LoopIterator
	
--	EXEC (@sql)
	
--	SET @LoopIterator=@LoopIterator+1
--END

--/**************************************************************************************************
-- *  This index is available in 5.0.2.So, after dropping ContentItemRecord_Id column from Target IP
-- *  table,below index is created using dataset_id column
-- **************************************************************************************************/

--IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Targets_TreatAndReleaseTargets]') 
--            AND name = N'IX_Targets_InpatientTargets_DRG_MDC_ContentItemRecord_Id') 
--   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Targets_TreatAndReleaseTargets]') 
--            AND name = N'IX_Targets_InpatientTargets_DRG_MDC_DataSet_Id')
--   AND @IsIndexExist=1
--BEGIN
--	CREATE NONCLUSTERED INDEX [IX_Targets_InpatientTargets_DRG_MDC_DataSet_Id] ON [dbo].[Targets_InpatientTargets] 
--(
--	[DRG] ASC,
--	[MDC] ASC,
--	[DataSet_Id] ASC
--)
--END        

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('ContentPartTransactionRecords') AND type in (N'U'))
--DROP TABLE ContentPartTransactionRecords

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('ContentItemRecords') AND type in (N'U'))
--DROP TABLE ContentItemRecords

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('ContentItemVersionRecords') AND type in (N'U'))
--DROP TABLE ContentItemVersionRecords

--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('ContentItemSummaryRecords') AND type in (N'U'))
--DROP TABLE ContentItemSummaryRecords
 

 IF (OBJECT_ID(N'Base_CostToChargeToDRGs')) IS NOT NULL 
	TRUNCATE TABLE Base_CostToChargeToDRGs

IF(OBJECT_ID(N'Base_CostToChargeToDXCCs')) IS NOT NULL 
	TRUNCATE TABLE Base_CostToChargeToDXCCs

IF(OBJECT_ID(N'Base_AreaPopulationStrats')) IS NOT NULL 
	TRUNCATE TABLE Base_AreaPopulationStrats


/*******************************************
 *  Drop old Temp_Quality tables
 *******************************************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Temp_Quality]') AND type in (N'U'))
DROP TABLE [dbo].[Temp_Quality]

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