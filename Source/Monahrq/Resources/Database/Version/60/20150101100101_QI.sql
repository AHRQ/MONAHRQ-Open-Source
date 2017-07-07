-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0
-- Create date: 12-20-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 AHRQ Targets to the new 
--              MONAHRQ 6.0 database schema.
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
WHERE i.[object_id]=OBJECT_ID('[dbo].[Targets_AhrqTargets]')
AND i.[type]=2

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #IndexList),
		@sql NVARCHAR(800)
		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [dbo].[Targets_AhrqTargets] DISABLE'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

IF NOT EXISTS (SELECT 1 FROM Wings_Datasets wt
WHERE wt.ContentType_Id IN (SELECT id FROM Wings_Targets wt2
                          WHERE wt2.Name in ('AHRQ-QI Area Data', 'AHRQ-QI Composite Data', 
	                 'AHRQ-QI Provider Data')) )
BEGIN
	

SET IDENTITY_INSERT dbo.Wings_Datasets ON

INSERT INTO dbo.Wings_Datasets
(
	id,
	SummaryData,
	[File],
	[Description],
	DateImported,
	ReportingQuarter,
	ReportingYear,
	DRGMDCMappingStatus,
	DRGMDCMappingStatusMessage,
	IsFinished,
	ContentType_Id
)
SELECT cir.Id, cisr.[Data],cir.[File],cir.[Description],cir.DateImported,cir.ReportingQuarter,
       cir.ReportingYear,cir.DRGMDCMappingStatus,cir.DRGMDCMappingStatusMessage,
       cir.IsFinished, (SELECT TOP(1) wt.Id FROM dbo.Wings_Targets wt
						WHERE LTRIM(RTRIM(wt.Name))=(SELECT LTRIM(RTRIM(ctr.Name))
                               FROM dbo.ContentTypeRecords ctr
                             WHERE id=cir.ContentType_Id))
  FROM dbo.ContentItemRecords cir
LEFT OUTER JOIN dbo.ContentItemSummaryRecords cisr
ON cisr.Id = cir.Summary_Id
INNER JOIN ContentTypeRecords ctr
ON cir.ContentType_Id=ctr.Id
WHERE  ctr.Name in ('AHRQ-QI Area Data', 'AHRQ-QI Composite Data', 
	                 'AHRQ-QI Provider Data');

SET IDENTITY_INSERT dbo.Wings_Datasets OFF

INSERT INTO [dbo].[Wings_DatasetTransactionRecords]
SELECT tr.[Code],
       tr.[Message],
       tr.[Extension],
       tr.[Data],
       tr.ContentItemRecord_Id
FROM   [dbo].[ContentPartTransactionRecords] tr
INNER JOIN dbo.ContentItemRecords cir
ON cir.Id = tr.ContentItemRecord_Id
INNER JOIN ContentTypeRecords ctr
ON cir.ContentType_Id=ctr.Id
WHERE  ctr.Name in ('AHRQ-QI Area Data', 'AHRQ-QI Composite Data', 
	                 'AHRQ-QI Provider Data');
END

 IF OBJECT_ID('Targets_AhrqTargetsTempForUpgrade') IS NOT NULL
    DROP TABLE [dbo].[Targets_AhrqTargetsTempForUpgrade]	
	
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
				END), N'  ', N' '), N'  ', N' ') MeasureCode, 
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
			,[ContentItemRecord_Id] Dataset_id
		INTO [Targets_AhrqTargetsTempForUpgrade]	
		FROM [dbo].[Targets_AhrqTargets]

/*******************************************
 *  TRUNCATE table 
  
 *******************************************/

TRUNCATE TABLE [dbo].[Targets_AhrqTargets]  

 IF OBJECT_ID(N'[FKB2029719F64562AC]') IS NOT NULL
	ALTER TABLE [dbo].[Targets_AhrqTargets] DROP CONSTRAINT [FKB2029719F64562AC] ;

ALTER TABLE [dbo].[Targets_AhrqTargets] ADD [Dataset_Id] INT NULL;
ALTER TABLE [dbo].[Targets_AhrqTargets] ADD CONSTRAINT [FK_TARGETS_AhrqTargets_DATASETS] FOREIGN KEY (Dataset_id) REFERENCES [dbo].[Wings_Datasets](Id)


INSERT INTO Targets_AhrqTargets
(
	TargetType,
	MeasureCode,
	Stratification,
	LocalHospitalID,
	CountyFIPS,
	ObservedNumerator,
	ObservedDenominator,
	ObservedRate,
	ObservedCIHigh,
	ObservedCILow,
	RiskAdjustedRate,
	RiskAdjustedCIHigh,
	RiskAdjustedCILow,
	ExpectedRate,
	StandardErr,
	Threshold,
	NatBenchmarkRate,
	NatRating,
	PeerBenchmarkRate,
	PeerRating,
	TotalCost,
    Dataset_Id
)
SELECT * FROM [Targets_AhrqTargetsTempForUpgrade]


SET @LoopIterator=1		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [dbo].[Targets_AhrqTargets] REBUILD'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END


 IF OBJECT_ID('Targets_AhrqTargetsTempForUpgrade') IS NOT NULL
    DROP TABLE Targets_AhrqTargetsTempForUpgrade


SELECT 1;