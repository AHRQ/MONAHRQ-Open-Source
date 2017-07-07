-- =============================================
-- Author:		Hossam Ahmed
-- Modified by: Shafiul Alam		
-- Project:		MONAHRQ 5.0 Build 2
-- Create date: 08-07-2014
-- Modified date: 10-15-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 AHRQ Targets to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'ED Treat And Release'
-- =============================================
--BEGIN TRY

-- Disable nonclustered Index

IF OBJECT_ID('tempdb..#IndexList') IS NOT NULL
DROP TABLE #IndexList 

SELECT identity(INT,1,1) rownum,
	   i.name 	
	   INTO #IndexList 
 FROM sys.indexes i
WHERE i.[object_id]=OBJECT_ID('[@@DESTINATION@@].[dbo].[Targets_TreatAndReleaseTargets]')
AND i.[type]=2

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #IndexList),
		@sql NVARCHAR(800)
		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_TreatAndReleaseTargets] DISABLE'
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
WHERE  t.Name = 'ED Treat And Release';

OPEN @Dataset_Cursor;
FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id
WHILE @@fetch_status = 0

BEGIN
DECLARE @NewDataSetId     INT,
        @NewSummeryId     INT,
        @NewTypeId        INT

INSERT INTO [@@DESTINATION@@].[dbo].[ContentItemSummaryRecords]
SELECT s.[Data]
FROM   [@@SOURCE@@].[dbo].[ContentItemSummaryRecords] s
       INNER JOIN [@@SOURCE@@].[dbo].[ContentItemRecords] c
            ON  c.[Summary_Id] = s.[Id]
WHERE  c.[Id] = @Dataset_Id;

SELECT @NewSummeryId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[ContentItemSummaryRecords]');
SELECT @NewTypeId = s.Id FROM [@@DESTINATION@@].[dbo].[ContentTypeRecords] s WHERE s.Name = @DatasetType;

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
FROM   [@@SOURCE@@].[dbo].[ContentItemRecords] c
WHERE  c.[Id] = @Dataset_Id

SELECT @NewDataSetId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[ContentItemRecords]');

INSERT INTO [@@DESTINATION@@].[dbo].[ContentPartTransactionRecords]
SELECT tr.[Code],
       tr.[Message],
       tr.[Extension],
       tr.[Data],
       @NewDataSetId
FROM   [@@SOURCE@@].[dbo].[ContentPartTransactionRecords] tr
WHERE  tr.[ContentItemRecord_Id] = @Dataset_Id

INSERT INTO [@@DESTINATION@@].[dbo].[Targets_TreatAndReleaseTargets]
SELECT [Key],
       [Age],
       CASE 
            WHEN Race = 'Exclude' THEN 'Exclude from Dataset'
            WHEN Race = 'AsianPacificIsland' THEN 'Asian or Pacific Island'
            WHEN Race = 'NativeAmerican' THEN 'Native American'
            WHEN Race = 'Retain ' THEN 'Retain Value'
            ELSE Race
       END [Race],
       CASE 
            WHEN Sex = 'Exclude' THEN 'Exclude from Dataset'
            ELSE Sex
       END [Sex],
       CASE 
            WHEN PrimaryPayer = 'Exclude' THEN 'Exclude from Dataset'
            WHEN PrimaryPayer = 'Private' THEN 'Private including HMO'
            WHEN PrimaryPayer = 'SelfPay' THEN 'Self-pay'
            WHEN PrimaryPayer = 'NoCharge' THEN 'No Charge'
            WHEN PrimaryPayer = 'Retain' THEN 'Retain Value'
            ELSE PrimaryPayer
       END [PrimaryPayer],
       [PatientStateCountyCode],
       [LocalHospitalID],
       CASE 
            WHEN DischargeDisposition = 'Exclude' THEN 'Exclude from Dataset'
            WHEN DischargeDisposition = 'ShortTerm' THEN 'Short-term hospital'
            WHEN DischargeDisposition = 'NursingFacility' THEN 
                 'Skilled nursing facility'
            WHEN DischargeDisposition = 'IntermediateCare' THEN 
                 'Intermediate care'
            WHEN DischargeDisposition = 'OtherFacility' THEN 
                 'Another type of facility'
            WHEN DischargeDisposition = 'HomeHealthCare' THEN 'Home health care'
            WHEN DischargeDisposition = 'AMA' THEN 'Against medical advice'
            WHEN DischargeDisposition = 'Deceased' THEN 'Died'
            WHEN DischargeDisposition = 'DischargedAliveDestUnknown' THEN 
                 'Discharged alive'
            ELSE DischargeDisposition
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
       CASE 
            WHEN HospitalTraumaLevel = 'Exclude' THEN 'Exclude from Dataset'
            WHEN HospitalTraumaLevel = 'NotTraumaCenter' THEN 
                 'Not a trauma center '
            WHEN HospitalTraumaLevel = 'Level_1' THEN 'Trauma center level I '
            WHEN HospitalTraumaLevel = 'Level_2' THEN 'Trauma center level II '
            WHEN HospitalTraumaLevel = 'Level_3' THEN 'Trauma center level III '
            WHEN HospitalTraumaLevel = 'Level_4' THEN 'Trauma center level IV '
            WHEN HospitalTraumaLevel = 'Level_1_2' THEN 
                 'Trauma center level I or II '
            WHEN HospitalTraumaLevel = 'Level_1_2_3' THEN 
                 'Trauma center level I, II, or III '
            ELSE HospitalTraumaLevel
       END [HospitalTraumaLevel],
       [NumberofDiagnoses],
       @NewDataSetId
FROM   [@@SOURCE@@].[dbo].[Targets_TreatAndReleaseTargets]
WHERE  [ContentItemRecord_Id] = @Dataset_Id 

		FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id
	END

	--END TRY
	--BEGIN CATCH
	--	 SELECT 0;
	--END CATCH

	CLOSE @Dataset_Cursor;
	DEALLOCATE @Dataset_Cursor;

	

--update [@@DESTINATION@@].[dbo].[Targets_TreatAndReleaseTargets]  
--set 
--Sex=
--	case
--	when Sex='Exclude' then 'Exclude from Dataset'
--	else
--	Sex
--	end,
--Race=
--	case
--	when Race='Exclude' then 'Exclude from Dataset'
--    when Race='AsianPacificIsland' then 'Asian or Pacific Island'
--    when Race='NativeAmerican' then 'Native American'
--    when Race='Retain ' then 'Retain Value'
--	else
--	Race
--	end,
--PrimaryPayer=
--	case
--    when PrimaryPayer='Exclude' then 'Exclude from Dataset'
--    when PrimaryPayer='Private' then 'Private including HMO'
--    when PrimaryPayer='SelfPay' then 'Self-pay'
--    when PrimaryPayer='NoCharge' then 'No Charge'
--    when PrimaryPayer='Retain' then 'Retain Value'
--	else
--	PrimaryPayer
--	end,
--HospitalTraumaLevel=
--	case
--	when HospitalTraumaLevel='Exclude' then 'Exclude from Dataset'
--	when HospitalTraumaLevel='NotTraumaCenter' then 'Not a trauma center '
--	when HospitalTraumaLevel='Level_1' then 'Trauma center level I '
--	when HospitalTraumaLevel='Level_2' then 'Trauma center level II '
--	when HospitalTraumaLevel='Level_3' then 'Trauma center level III '
--	when HospitalTraumaLevel='Level_4' then 'Trauma center level IV '
--	when HospitalTraumaLevel='Level_1_2' then 'Trauma center level I or II '
--	when HospitalTraumaLevel='Level_1_2_3' then 'Trauma center level I, II, or III '
--	else
--	HospitalTraumaLevel
--	end,
--DischargeDisposition=
--	case
--	when DischargeDisposition='Exclude' then 'Exclude from Dataset'
--	when DischargeDisposition='ShortTerm' then 'Short-term hospital'
--	when DischargeDisposition='NursingFacility' then 'Skilled nursing facility'
--	when DischargeDisposition='IntermediateCare' then 'Intermediate care'
--	when DischargeDisposition='OtherFacility' then 'Another type of facility'
--	when DischargeDisposition='HomeHealthCare' then 'Home health care'
--	when DischargeDisposition='AMA' then 'Against medical advice'
--	when DischargeDisposition='Deceased' then 'Died'
--	when DischargeDisposition='DischargedAliveDestUnknown' then 'Discharged alive'
--	else
--	DischargeDisposition
--	end

SET @LoopIterator=1		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_TreatAndReleaseTargets] REBUILD'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

SELECT 1;
