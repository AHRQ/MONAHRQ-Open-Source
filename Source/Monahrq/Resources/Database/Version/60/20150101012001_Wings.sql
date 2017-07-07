-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0 Build 1
-- Create date: 12-22-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Reports table to the new 
--              MONAHRQ 6.0 database schema.
--				'Wings Data'
-- =============================================

BEGIN TRY

IF NOT EXISTS (SELECT 1 FROM [Wings] WHERE NAME IN ('Nursing Home Compare Data')) 
INSERT INTO [Wings]([Name],[Description],[WingGUID])
VALUES('Nursing Home Compare Data','Provides Services for Nursing Home Compare Data','c289056e-5144-4352-9a2b-14c76a6c86f3');

IF NOT EXISTS (SELECT 1 FROM [Wings] WHERE NAME IN ('Physician Data')) 
INSERT INTO [Wings]([Name],[Description],[WingGUID])
VALUES('Physician Data','Physician Data','bb53cfd5-6912-4f8a-a955-bd78119c871f');

IF OBJECT_ID('tempdb..#Wings_targetsForeignKeys') IS NOT NULL
DROP TABLE #Wings_targetsForeignKeys

SELECT identity(INT,1,1) rownum,
OBJECT_NAME(fk.parent_object_id) parentTable,NAME 
INTO #Wings_targetsForeignKeys
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id= OBJECT_ID('Wings_targets') 

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #Wings_targetsForeignKeys),
		@sql NVARCHAR(800)

DECLARE @BackupTable VARCHAR(50)
SELECT @BackupTable ='Measures_'+CONVERT(VARCHAR(10), GETDATE(), 112)+'_Bkp'

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@BackupTable) AND type in (N'U'))
BEGIN
SET @sql ='SELECT * INTO [dbo].['+@BackupTable+'] From Measures'
EXEC (@sql)
END

WHILE (@LoopIterator<=@LoopCount)
BEGIN
	
	SELECT @sql='ALTER TABLE [dbo].['+parentTable+'] DROP CONSTRAINT '+NAME
	FROM #Wings_targetsForeignKeys i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	
	SET @LoopIterator=@LoopIterator+1
END

IF OBJECT_ID('TempWingIdFeatureIdMapping') IS NOT NULL
DROP TABLE TempWingIdFeatureIdMapping

SELECT wf.Name,wt.Feature_id,w.Id Wing_Id,w.Name WingName
INTO TempWingIdFeatureIdMapping
FROM Wings w
	LEFT OUTER JOIN  Wings_Targets wt ON w.Id = wt.Wing_id
	LEFT OUTER JOIN Wings_Features wf ON wf.Id = wt.Feature_id


IF(OBJECT_ID(N'[Wings_Targets_temp]')) IS NOT NULL
	DROP TABLE	[Wings_Targets_temp]

CREATE TABLE Wings_Targets_temp(
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[Name] [NVARCHAR](255) NOT NULL,
	[Publisher] [NVARCHAR](255) NOT NULL,
	[PublisherEmail] [NVARCHAR](255) NULL,
	[PublisherWebsite] [NVARCHAR](255) NULL,
	[IsDisabled] [BIT] NOT NULL DEFAULT (0),
	[IsCustom] [BIT] NOT NULL DEFAULT (0),
	[IsTrendingEnabled] [BIT] NOT NULL DEFAULT (0),
	[AllowMultipleImports] [BIT] NOT NULL DEFAULT (0),
	[IsReferenceTarget] [BIT] NOT NULL DEFAULT (0),
	[DisplayOrder] [INT] NOT NULL DEFAULT (0),
	[DbSchemaName] [NVARCHAR](150) NULL,
	[FilePath] [NVARCHAR](256) NULL,
	[ImportSQLScript] [NVARCHAR](MAX) NULL,
	[AddMeausersSqlScript] [NVARCHAR](MAX) NULL,
	[AddReportsSqlScript] [NVARCHAR](MAX) NULL,
	[ClrType] [NVARCHAR](255) NULL,
	[Description] [NVARCHAR](256) NULL,
	[Guid] [UNIQUEIDENTIFIER] NOT NULL,
	[Wing_id] [INT] NULL,
	[VersionNumber] [NVARCHAR](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	
SET IDENTITY_INSERT [Wings_Targets_temp] ON

INSERT INTO [Wings_Targets_temp]([Id],Wing_Id, [Name],[Publisher],[PublisherEmail],[PublisherWebsite],[IsDisabled],[IsCustom],[IsTrendingEnabled],[AllowMultipleImports],[IsReferenceTarget],[DisplayOrder],[DbSchemaName],[ImportSQLScript],[AddMeausersSqlScript],[AddReportsSqlScript],[ClrType],[Description],[Guid],[VersionNumber])
select we.Target_Id, w.Id, case w.Name 
							when 'Inpatient Data' then 'Inpatient Discharge'
							when 'Treat and Release Discharge Data' then 'ED Treat And Release'
							else w.Name end,'Agency for Healthcare Research and Quality (AHRQ)','moanhrq@ahrq.gov','http://monahrq.ahrq.gov/', 
	  'False','False',case w.Name 
							when 'Inpatient Data' then 'True'
							when 'Treat and Release Discharge Data' then 'True'
							else 'False' end,'False','False', case w.Name 
											when 'Inpatient Data' then 0
											when 'Treat and Release Discharge Data' then 1
											when 'AHRQ-QI Area Data'	then 2
											when 'AHRQ-QI Composite Data' then 3
											when 'AHRQ-QI Provider Data' then 4
											when 'Hospital Compare Data' then 5
											when 'Medicare Provider Charge Data' then 6
											else 7
											end,NULL,NULL,NULL,NULL,Replace(wf.ClrType,'Version=5','Version=6') as ClrType ,wf.Description, wf.Guid, '6.0.20164' 
from Wings_Elements we
	INNER JOIN Wings_Features wf on we.Target_id = wf.Id
	Inner JOIN Wings_Targets wt on wt.[Feature_id] = wf.Id
	Inner Join Wings w on w.Id = wt.Wing_Id
group by we.Target_Id,w.Id, w.Name, wf.Id,wf.ClrType, wf.Description, wf.Guid

SET IDENTITY_INSERT [Wings_Targets_temp] OFF

INSERT INTO [Wings_Targets_temp]([Name],[Publisher],[PublisherEmail],[PublisherWebsite],[IsDisabled],[IsCustom],[IsTrendingEnabled],[AllowMultipleImports],[IsReferenceTarget],[DisplayOrder],[DbSchemaName],[ImportSQLScript],[AddMeausersSqlScript],[AddReportsSqlScript],[ClrType],[Description],[Guid],[VersionNumber])
VALUES('Nursing Home Compare Data','Agency for Healthcare Research and Quality (AHRQ)','moanhrq@ahrq.gov','http://monahrq.ahrq.gov/','False','False','False','False','False','5',NULL,NULL,NULL,NULL,'Monahrq.Wing.NursingHomeCompare.NursingHomeTarget, Monahrq.Wing.NursingHomeCompare, Version=6.0.5477.20164, Culture=neutral, PublicKeyToken=null','Mapping target for Nursing home Compare Data','13751d1a-4dcc-451e-b4b0-8a4a8714da76','6.0.20164');
INSERT INTO [Wings_Targets_temp]([Name],[Publisher],[PublisherEmail],[PublisherWebsite],[IsDisabled],[IsCustom],[IsTrendingEnabled],[AllowMultipleImports],[IsReferenceTarget],[DisplayOrder],[DbSchemaName],[ImportSQLScript],[AddMeausersSqlScript],[AddReportsSqlScript],[ClrType],[Description],[Guid],[VersionNumber])
VALUES('Physician Data','Agency for Healthcare Research and Quality (AHRQ)','moanhrq@ahrq.gov','http://monahrq.ahrq.gov/','False','False','False','False','False','6',NULL,NULL,NULL,NULL,'Monahrq.Wing.Physician.PhysicianTarget, Monahrq.Wing.Physician, Version=6.0.5477.20163, Culture=neutral, PublicKeyToken=null','Mapping Physician Data','9db4571f-6d73-4516-970b-59ed09c51413','6.0.20163');
/*******************************************
 *  Insert Nursing Home and Physician Element
 *******************************************/

IF NOT EXISTS (SELECT 1 FROM Wings_Elements we 
           WHERE we.Target_Id 
           IN (SELECT id FROM Wings_Targets_temp wt 
               WHERE wt.Name IN ('Physician Data','Nursing Home Compare Data')))
BEGIN
INSERT INTO [dbo].[Wings_Elements]([Name],[Description],[IsRequired],[Hints],[LongDescription],[Ordinal],[Type],[Scope_id],[DependsOn_id],[Target_Id])
VALUES('provnum','provnum','False','','provnum','2',NULL,NULL,NULL,(SELECT id FROM Wings_Targets_temp wt WHERE wt.Name='Nursing Home Compare Data'));
INSERT INTO [dbo].[Wings_Elements]([Name],[Description],[IsRequired],[Hints],[LongDescription],[Ordinal],[Type],[Scope_id],[DependsOn_id],[Target_Id])
VALUES('DRG Definition','DRG Definition','True','','DRG Definition','0',NULL,NULL,NULL,(SELECT id FROM Wings_Targets_temp wt WHERE wt.Name='Physician Data'));
INSERT INTO [dbo].[Wings_Elements]([Name],[Description],[IsRequired],[Hints],[LongDescription],[Ordinal],[Type],[Scope_id],[DependsOn_id],[Target_Id])
VALUES('Provider Id','Provider Id','True','','Provider Id','1',NULL,NULL,NULL,(SELECT id FROM Wings_Targets_temp wt WHERE wt.Name='Physician Data'));
INSERT INTO [dbo].[Wings_Elements]([Name],[Description],[IsRequired],[Hints],[LongDescription],[Ordinal],[Type],[Scope_id],[DependsOn_id],[Target_Id])
VALUES('Total Discharges','Total Discharges','True','','Total Discharges','8',NULL,NULL,NULL,(SELECT id FROM Wings_Targets_temp wt WHERE wt.Name='Physician Data'));
INSERT INTO [dbo].[Wings_Elements]([Name],[Description],[IsRequired],[Hints],[LongDescription],[Ordinal],[Type],[Scope_id],[DependsOn_id],[Target_Id])
VALUES('Average Covered Charges','Average Covered Charges','True','','Average Covered Charges','9',NULL,NULL,NULL,(SELECT id FROM Wings_Targets_temp wt WHERE wt.Name='Physician Data'));
INSERT INTO [dbo].[Wings_Elements]([Name],[Description],[IsRequired],[Hints],[LongDescription],[Ordinal],[Type],[Scope_id],[DependsOn_id],[Target_Id])
VALUES('Average Total Payments','Average Total Payments','True','','Average Total Payments','10',NULL,NULL,NULL,(SELECT id FROM Wings_Targets_temp wt WHERE wt.Name='Physician Data'));
END


/*******************************************
 *  Add foreign key
 *******************************************/

SET @LoopIterator=1

WHILE (@LoopIterator<=@LoopCount)
BEGIN
	
SET @sql=''	
SELECT @sql='IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N''[dbo].['+NAME+']'') AND parent_object_id = OBJECT_ID(N''[dbo].['+parentTable+']''))
              BEGIN
               ALTER TABLE [dbo].['+parentTable+']  WITH CHECK ADD  CONSTRAINT ['+NAME+'] FOREIGN KEY(['+CASE WHEN i.parentTable='Wings_Datasets' THEN 'ContentType_Id' ELSE 'Target_id' END+'])
			   REFERENCES [dbo].[Wings_Targets_temp] ([Id]);
			   ALTER TABLE [dbo].['+parentTable+'] CHECK CONSTRAINT ['+NAME+']
			  END'
			FROM #Wings_targetsForeignKeys i
	 WHERE rownum=@LoopIterator
	 
	
	EXEC (@sql)
	
	SET @LoopIterator=@LoopIterator+1
END	

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Wings_Targets]') AND type in (N'U'))
	DROP TABLE [dbo].[Wings_Targets]


EXEC sp_rename [Wings_Targets_temp], [Wings_Targets]

ALTER TABLE [dbo].[Wings_Targets]  WITH CHECK ADD  CONSTRAINT [Wing_owns_Target_FK] FOREIGN KEY([Wing_id])
REFERENCES [dbo].[Wings] ([Id])
ALTER TABLE [dbo].[Wings_Targets] CHECK CONSTRAINT [Wing_owns_Target_FK]


SELECT 1;

END TRY
BEGIN CATCH
    DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;