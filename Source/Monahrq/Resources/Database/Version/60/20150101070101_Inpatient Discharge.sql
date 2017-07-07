-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0
-- Create date: 12-20-2014
-- Modified date: 
-- Description:	This is the update script from older MONAHRQ 5.0  AHRQ Targets to the new 
--              MONAHRQ 6.0  database schema.
--				'Inpatient Discharge'
-- =============================================
BEGIN TRY

-- Disable nonclustered Index

IF OBJECT_ID('tempdb..#IndexList') IS NOT NULL
DROP TABLE #IndexList 

SELECT identity(INT,1,1) rownum,
	   i.name 	
	   INTO #IndexList 
 FROM sys.indexes i
WHERE i.[object_id]=OBJECT_ID('[dbo].[Targets_InpatientTargets]')
AND i.[type]=2

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #IndexList),
		@sql NVARCHAR(800)
		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [dbo].[Targets_InpatientTargets] DISABLE'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

DECLARE @Dataset_Id INT,
        @DatasetType NVARCHAR(100),
        @DatasetSummary_Id INT;


IF NOT EXISTS (SELECT 1 FROM Wings_Datasets wt
WHERE wt.ContentType_Id= (SELECT id FROM Wings_Targets wt2
                          WHERE UPPER(wt2.Name)='INPATIENT DATA') )
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
	LEFT OUTER JOIN dbo.ContentItemSummaryRecords cisr ON cisr.Id = cir.Summary_Id
	INNER JOIN ContentTypeRecords ctr ON cir.ContentType_Id=ctr.Id
WHERE UPPER(ctr.Name) = 'INPATIENT DISCHARGE';

SET IDENTITY_INSERT dbo.Wings_Datasets OFF


INSERT INTO [dbo].[Wings_DatasetTransactionRecords]
SELECT tr.[Code], tr.[Message], tr.[Extension], tr.[Data], tr.ContentItemRecord_Id
FROM   [dbo].[ContentPartTransactionRecords] tr
		INNER JOIN dbo.ContentItemRecords cir ON cir.Id = tr.ContentItemRecord_Id
		INNER JOIN ContentTypeRecords ctr ON cir.ContentType_Id=ctr.Id
WHERE   UPPER(ctr.Name) = 'INPATIENT DISCHARGE';

END;

IF OBJECT_ID('Targets_InpatientTargetsTemp') IS NOT NULL
    DROP TABLE Targets_InpatientTargetsTemp

SELECT * INTO Targets_InpatientTargetsTemp FROM [dbo].[Targets_InpatientTargets]  

/*******************************************
 *  TRUNCATE table 
 *******************************************/
 
 TRUNCATE TABLE [dbo].[Targets_InpatientTargets]
 
IF OBJECT_ID (N'[dbo].[Targets_InpatientTargets]') IS NOT NULL 
	DROP TABLE [dbo].[Targets_InpatientTargets]
  
 CREATE TABLE [dbo].[Targets_InpatientTargets](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](20) NULL,
	[Age] [int] NULL,
	[AgeInDays] [int] NULL,
	[Race] [int] NULL,
	[Sex] [int] NULL,
	[PrimaryPayer] [int] NULL,
	[PatientStateCountyCode] [nvarchar](5) NULL,
	[LocalHospitalID] [nvarchar](12) NULL,
	[DischargeDisposition] [int] NULL,
	[AdmissionType] [int] NULL,
	[AdmissionSource] [int] NULL,
	[PointOfOrigin] [int] NULL,
	[LengthOfStay] [int] NULL,
	[DischargeDate] [datetime] NULL,
	[DischargeYear] [int] NULL,
	[DischargeQuarter] [int] NULL,
	[DaysOnMechVentilator] [int] NULL,
	[BirthWeightGrams] [int] NULL,
	[TotalCharge] [int] NULL,
	[DRG] [int] NULL,
	[MDC] [int] NULL,
	[PrincipalDiagnosis] [nvarchar](10) NULL,
	[DiagnosisCode2] [nvarchar](10) NULL,
	[DiagnosisCode3] [nvarchar](10) NULL,
	[DiagnosisCode4] [nvarchar](10) NULL,
	[DiagnosisCode5] [nvarchar](10) NULL,
	[DiagnosisCode6] [nvarchar](10) NULL,
	[DiagnosisCode7] [nvarchar](10) NULL,
	[DiagnosisCode8] [nvarchar](10) NULL,
	[DiagnosisCode9] [nvarchar](10) NULL,
	[DiagnosisCode10] [nvarchar](10) NULL,
	[DiagnosisCode11] [nvarchar](10) NULL,
	[DiagnosisCode12] [nvarchar](10) NULL,
	[DiagnosisCode13] [nvarchar](10) NULL,
	[DiagnosisCode14] [nvarchar](10) NULL,
	[DiagnosisCode15] [nvarchar](10) NULL,
	[DiagnosisCode16] [nvarchar](10) NULL,
	[DiagnosisCode17] [nvarchar](10) NULL,
	[DiagnosisCode18] [nvarchar](10) NULL,
	[DiagnosisCode19] [nvarchar](10) NULL,
	[DiagnosisCode20] [nvarchar](10) NULL,
	[DiagnosisCode21] [nvarchar](10) NULL,
	[DiagnosisCode22] [nvarchar](10) NULL,
	[DiagnosisCode23] [nvarchar](10) NULL,
	[DiagnosisCode24] [nvarchar](10) NULL,
	[PrincipalProcedure] [nvarchar](10) NULL,
	[ProcedureCode2] [nvarchar](10) NULL,
	[ProcedureCode3] [nvarchar](10) NULL,
	[ProcedureCode4] [nvarchar](10) NULL,
	[ProcedureCode5] [nvarchar](10) NULL,
	[ProcedureCode6] [nvarchar](10) NULL,
	[ProcedureCode7] [nvarchar](10) NULL,
	[ProcedureCode8] [nvarchar](10) NULL,
	[ProcedureCode9] [nvarchar](10) NULL,
	[ProcedureCode10] [nvarchar](10) NULL,
	[ProcedureCode11] [nvarchar](10) NULL,
	[ProcedureCode12] [nvarchar](10) NULL,
	[ProcedureCode13] [nvarchar](10) NULL,
	[ProcedureCode14] [nvarchar](10) NULL,
	[ProcedureCode15] [nvarchar](10) NULL,
	[ProcedureCode16] [nvarchar](10) NULL,
	[ProcedureCode17] [nvarchar](10) NULL,
	[ProcedureCode18] [nvarchar](10) NULL,
	[ProcedureCode19] [nvarchar](10) NULL,
	[ProcedureCode20] [nvarchar](10) NULL,
	[ProcedureCode21] [nvarchar](10) NULL,
	[ProcedureCode22] [nvarchar](10) NULL,
	[ProcedureCode23] [nvarchar](10) NULL,
	[ProcedureCode24] [nvarchar](10) NULL,
	[CustomStratifier1] [nvarchar](20) NULL,
	[CustomStratifier2] [nvarchar](20) NULL,
	[CustomStratifier3] [nvarchar](20) NULL,
	[PatientID] [nvarchar](20) NULL,
	[BirthDate] [datetime] NULL,
	[AdmissionDate] [datetime] NULL,
	[EDServices] [int] NULL,
	[HRRRegionID] [int] NULL,
	[HSARegionID] [int] NULL,
	[CustomRegionID] [int] NULL,
	[PatientZipCode] [nvarchar](5) NULL,
	[Dataset_Id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[Targets_InpatientTargets]  WITH CHECK ADD  CONSTRAINT [FK_TARGETS_InpatientTargets_DATASETS] FOREIGN KEY([Dataset_Id])
REFERENCES [dbo].[Wings_Datasets] ([Id])
ALTER TABLE [dbo].[Targets_InpatientTargets] CHECK CONSTRAINT [FK_TARGETS_InpatientTargets_DATASETS]

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