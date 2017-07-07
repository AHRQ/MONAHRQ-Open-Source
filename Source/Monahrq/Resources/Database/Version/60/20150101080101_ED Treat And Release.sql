-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0
-- Create date: 12-20-2014
-- Modified date: 
-- Description:	This is the update script from older MONAHRQ 5.0 AHRQ Targets to the new 
--              MONAHRQ 6.0 database schema.
--				'ED Treat And Release'
-- =============================================
BEGIN TRY

-- Disable nonclustered Index

IF OBJECT_ID('tempdb..#IndexList') IS NOT NULL
DROP TABLE #IndexList 

SELECT identity(INT,1,1) rownum,
	   i.name 	
	   INTO #IndexList 
 FROM sys.indexes i
WHERE i.[object_id]=OBJECT_ID('[dbo].[Targets_TreatAndReleaseTargets]')
AND i.[type]=2

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #IndexList),
		@sql NVARCHAR(800)
		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [dbo].[Targets_TreatAndReleaseTargets] DISABLE'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END



DECLARE @NewDataSetId     INT,
        @NewSummeryId     INT,
        @NewTypeId        INT

IF NOT EXISTS (SELECT * FROM Wings_Datasets wt
WHERE wt.ContentType_Id= (SELECT id FROM Wings_Targets wt2
                          WHERE wt2.Name='ED Treat And Release') )
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
WHERE  ctr.Name = 'ED Treat And Release';

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
WHERE  ctr.Name = 'ED Treat And Release';

END

 IF OBJECT_ID('Targets_TreatAndReleaseTargetsTempForUpgrade') IS NOT NULL
    DROP TABLE [dbo].[Targets_TreatAndReleaseTargetsTempForUpgrade]

SELECT [Key],
       [Age],
       CASE [Race] 
			WHEN 'Exclude from Dataset' THEN -1
			WHEN 'Missing 'THEN 0
			WHEN 'White' THEN 1
			WHEN 'Black' THEN 2
			WHEN 'Hispanic' THEN 3
			WHEN 'Asian or Pacific Island' THEN 4
			WHEN 'Native American' THEN 5
			WHEN 'Other' THEN 6
			WHEN 'Retain Value' THEN 99
			--ELSE 0
	   END [Race],
       Case [Sex]
			WHEN 'Exclude from Dataset' THEN -1
			WHEN 'Male' THEN 1
			WHEN 'Female' THEN 2
	   END [Sex],
       Case [PrimaryPayer]
            WHEN 'Exclude from Dataset' THEN -1
            WHEN 'Missing' THEN 0 
            WHEN 'Medicare' THEN 1
            WHEN 'Medicaid' THEN 2
            WHEN 'Private including HMO' THEN 3
			WHEN 'Self-pay' THEN 4
            WHEN 'No Charge' THEN 5
			WHEN 'Other' THEN 6
            WHEN 'Retain Value' THEN 99
	   END [PrimaryPayer],
       [PatientStateCountyCode],
       [LocalHospitalID],
       CASE [DischargeDisposition]
            WHEN 'Exclude from Dataset' THEN -1
			WHEN 'Missing' THEN 0
			WHEN 'Routing' THEN 1
			WHEN 'Short-term hospital' THEN 2 
            WHEN 'Skilled nursing facility' THEN 3
            WHEN 'Intermediate care' THEN 4
            WHEN 'Another type of facility' THEN 5
            WHEN 'Home health care' THEN 6
            WHEN 'Against medical advice' THEN 7
            WHEN 'Died' THEN 20
            WHEN 'Discharged alive' THEN 99
			when 'Discharged alive, destination unknown' then 99
            --ELSE 0
       END [DischargeDisposition],
       [DischargeYear],
       [PrimaryDiagnosis],
       [DiagnosisCode2],
       [DiagnosisCode3],
       [DiagnosisCode4],
       [DiagnosisCode5],
       [DiagnosisCode6],
       [DiagnosisCode7],
       [DiagnosisCode8],
       [DiagnosisCode9],
       [DiagnosisCode10],
       [DiagnosisCode11],
       [DiagnosisCode12],
       [DiagnosisCode13],
       [DiagnosisCode14],
       [DiagnosisCode15],
       [DiagnosisCode16],
       [DiagnosisCode17],
       [DiagnosisCode18],
       [DiagnosisCode19],
       [DiagnosisCode20],
       CASE [HospitalTraumaLevel]
			WHEN 'Exclude from Dataset' THEN -1
			WHEN 'Not a trauma center' THEN 0
			WHEN 'Trauma center level I' THEN 1
			WHEN 'Trauma center level II' THEN 2
			WHEN 'Trauma center level III' THEN 3
			WHEN 'Trauma center level IV' THEN 4
			WHEN 'Trauma center level I or II' THEN 8
			WHEN 'Trauma center level I, II, or III' THEN 9
	   END [HospitalTraumaLevel],
       [NumberofDiagnoses],
       [ContentItemRecord_Id] Dataset_id
INTO [dbo].[Targets_TreatAndReleaseTargetsTempForUpgrade]
FROM   [dbo].[Targets_TreatAndReleaseTargets] ip

       
/*******************************************
 *  TRUNCATE table   
 *******************************************/
 
 TRUNCATE TABLE [dbo].[Targets_TreatAndReleaseTargets]
 
 IF OBJECT_ID(N'FK_Targets_TreatAndReleaseTargets_ContentITemRecord') IS NOT NULL
	ALTER TABLE [dbo].[Targets_TreatAndReleaseTargets] DROP CONSTRAINT FK_Targets_TreatAndReleaseTargets_ContentITemRecord;

 IF OBJECT_ID(N'[dbo].[Targets_TreatAndReleaseTargets]') IS NOT NULL
	DROP TABLE [dbo].[Targets_TreatAndReleaseTargets]

CREATE TABLE [dbo].[Targets_TreatAndReleaseTargets](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](20) NULL,
	[Age] [int] NULL,
	[Race] [int] NULL,
	[Sex] [int] NULL,
	[PrimaryPayer] [int] NULL,
	[PatientStateCountyCode] [nvarchar](5) NULL,
	[LocalHospitalID] [nvarchar](12) NULL,
	[DischargeDisposition] [int] NULL,
	[DischargeYear] [int] NULL,
	[PrimaryDiagnosis] [nvarchar](10) NULL,
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
	[HospitalTraumaLevel] [int] NULL,
	[NumberofDiagnoses] [int] NULL,
	[Dataset_Id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[Targets_TreatAndReleaseTargets]  WITH CHECK ADD  CONSTRAINT [FK_TARGETS_TreatAndReleaseTargets_DATASETS] FOREIGN KEY([Dataset_Id]) REFERENCES [dbo].[Wings_Datasets] ([Id])
ALTER TABLE [dbo].[Targets_TreatAndReleaseTargets] CHECK CONSTRAINT [FK_TARGETS_TreatAndReleaseTargets_DATASETS]

INSERT INTO [dbo].[Targets_TreatAndReleaseTargets]
(
	[Key],
	Age,
	Race,
	Sex,
	PrimaryPayer,
	PatientStateCountyCode,
	LocalHospitalID,
	DischargeDisposition,
	DischargeYear,
	PrimaryDiagnosis,
	DiagnosisCode2,
	DiagnosisCode3,
	DiagnosisCode4,
	DiagnosisCode5,
	DiagnosisCode6,
	DiagnosisCode7,
	DiagnosisCode8,
	DiagnosisCode9,
	DiagnosisCode10,
	DiagnosisCode11,
	DiagnosisCode12,
	DiagnosisCode13,
	DiagnosisCode14,
	DiagnosisCode15,
	DiagnosisCode16,
	DiagnosisCode17,
	DiagnosisCode18,
	DiagnosisCode19,
	DiagnosisCode20,
	HospitalTraumaLevel,
	NumberofDiagnoses,
	Dataset_Id
)
SELECT * FROM [Targets_TreatAndReleaseTargetsTempForUpgrade];

 IF OBJECT_ID('Targets_TreatAndReleaseTargetsTempForUpgrade') IS NOT NULL
    DROP TABLE Targets_TreatAndReleaseTargetsTempForUpgrade

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