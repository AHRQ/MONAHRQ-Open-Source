-- =============================================
-- Author:		Hossam Ahmed
-- Modifier:	Shafiul Alam
-- Project:		MONAHRQ 5.0 Build 2
-- Create date: 08-07-2014
-- Modified date: 10-15-2015
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 AHRQ Targets to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'Inpatient Discharge'
-- =============================================
--BEGIN TRY

-- Disable nonclustered Index

IF OBJECT_ID('tempdb..#IndexList') IS NOT NULL
DROP TABLE #IndexList 

SELECT identity(INT,1,1) rownum,
	   i.name 	
	   INTO #IndexList 
 FROM sys.indexes i
WHERE i.[object_id]=OBJECT_ID('[@@DESTINATION@@].[dbo].[Targets_InpatientTargets]')
AND i.[type]=2

DECLARE @LoopIterator INT=1,
		@LoopCount INT= (SELECT COUNT(1) FROM #IndexList),
		@sql NVARCHAR(800)
		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_InpatientTargets] DISABLE'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

DECLARE @IsDBVersion1 BIT =0
IF OBJECT_ID('[@@SOURCE@@].[dbo].SchemaVersions') IS NULL
SET @IsDBVersion1=1

DECLARE @Dataset_Id INT,
        @DatasetType NVARCHAR(100),
        @DatasetSummary_Id INT;
        
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
WHERE  t.Name = 'Inpatient Discharge';

OPEN @Dataset_Cursor;

FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id
WHILE @@fetch_status = 0
BEGIN
DECLARE @NewDataSetId INT, @NewSummeryId INT, @NewTypeId INT
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
FROM [@@SOURCE@@].[dbo].[ContentItemRecords] c
WHERE c.[Id] =  @Dataset_Id;
SELECT @NewDataSetId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[ContentItemRecords]');

INSERT INTO [@@DESTINATION@@].[dbo].[ContentPartTransactionRecords]
SELECT tr.[Code],
       tr.[Message],
       tr.[Extension],
       tr.[Data],
       @NewDataSetId
FROM   [@@SOURCE@@].[dbo].[ContentPartTransactionRecords] tr
WHERE  tr.[ContentItemRecord_Id] = @Dataset_Id;

INSERT INTO [@@DESTINATION@@].[dbo].[Targets_InpatientTargets] (
       [Key]
      ,[Age]
      ,[AgeInDays]
      ,[Race]
      ,[Sex]
      ,[PrimaryPayer]
      ,[PatientStateCountyCode]
      ,[LocalHospitalID]
      ,[DischargeDisposition]
      ,[AdmissionType]
      ,[AdmissionSource]
      ,[LengthOfStay]
      ,[DischargeDate]
      ,[DischargeYear]
      ,[DischargeQuarter]
      ,[DaysOnMechVentilator]
      ,[BirthWeightGrams]
      ,[TotalCharge]
      ,[DRG]
      ,[MDC]
      ,[PrincipalDiagnosis]
      ,[DiagnosisCode2]
      ,[DiagnosisCode3]
      ,[DiagnosisCode4]
      ,[DiagnosisCode5]
      ,[DiagnosisCode6]
      ,[DiagnosisCode7]
      ,[DiagnosisCode8]
      ,[DiagnosisCode9]
      ,[DiagnosisCode10]
      ,[DiagnosisCode11]
      ,[DiagnosisCode12]
      ,[DiagnosisCode13]
      ,[DiagnosisCode14]
      ,[DiagnosisCode15]
      ,[DiagnosisCode16]
      ,[DiagnosisCode17]
      ,[DiagnosisCode18]
      ,[DiagnosisCode19]
      ,[DiagnosisCode20]
      ,[DiagnosisCode21]
      ,[DiagnosisCode22]
      ,[DiagnosisCode23]
      ,[DiagnosisCode24]
      ,[PrincipalProcedure]
      ,[ProcedureCode2]
      ,[ProcedureCode3]
      ,[ProcedureCode4]
      ,[ProcedureCode5]
      ,[ProcedureCode6]
      ,[ProcedureCode7]
      ,[ProcedureCode8]
      ,[ProcedureCode9]
      ,[ProcedureCode10]
      ,[ProcedureCode11]
      ,[ProcedureCode12]
      ,[ProcedureCode13]
      ,[ProcedureCode14]
      ,[ProcedureCode15]
      ,[ProcedureCode16]
      ,[ProcedureCode17]
      ,[ProcedureCode18]
      ,[ProcedureCode19]
      ,[ProcedureCode20]
      ,[ProcedureCode21]
      ,[ProcedureCode22]
      ,[ProcedureCode23]
      ,[ProcedureCode24]
      ,[CustomStratifier1]
      ,[CustomStratifier2]
      ,[CustomStratifier3]
      ,[PatientID]
      ,[BirthDate]
      ,[AdmissionDate]
      ,[EDServices]      
	  ,[HRRRegionID]
      ,[HSARegionID]
      ,[CustomRegionID]
      ,[PatientZipCode]
      ,[ContentItemRecord_Id])
SELECT
[Key]
      ,[Age]
      ,[AgeInDays]
      ,CASE 
            WHEN Race = 'Exclude' THEN 'Exclude from Dataset'
            WHEN Race = 'AsianPacificIsland' THEN 'Asian or Pacific Island'
            WHEN Race = 'NativeAmerican' THEN 'Native American'
            WHEN Race = 'Retain ' THEN 'Retain Value'
            ELSE Race
       END [Race]
      ,CASE 
            WHEN Sex = 'Exclude' THEN 'Exclude from Dataset'
            ELSE Sex
       END [Sex]
      ,CASE 
            WHEN PrimaryPayer = 'Exclude' THEN 'Exclude from Dataset'
            WHEN PrimaryPayer = 'Private' THEN 'Private including HMO'
            WHEN PrimaryPayer = 'SelfPay' THEN 'Self-pay'
            WHEN PrimaryPayer = 'NoCharge' THEN 'No Charge'
            WHEN PrimaryPayer = 'Retain' THEN 'Retain Value'
            ELSE PrimaryPayer
       END [PrimaryPayer]
      ,[PatientStateCountyCode]
      ,[LocalHospitalID]
      ,CASE 
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
       END [DischargeDisposition]
      ,CASE 
            WHEN AdmissionType = 'Exclude' THEN 'Exclude from Dataset'
            WHEN AdmissionType = 'Trauma' THEN 'Trauma Center'
            ELSE AdmissionType
       END [AdmissionType]
      ,CASE 
            WHEN AdmissionSource = 'Exclude' THEN 'Exclude from Dataset'
            WHEN AdmissionSource = 'OtherHospital' THEN 'Another hospital'
            WHEN AdmissionSource = 'OtherFacility' THEN 'Another fac. incl. LTC'
            WHEN AdmissionSource = 'LegalSystem' THEN 'Court/Law enforcement'
            WHEN AdmissionSource = 'Routine' THEN 'Routine/Birth/Other'
            ELSE AdmissionSource
       END [AdmissionSource]
      ,[LengthOfStay]
      ,[DischargeDate]
      ,[DischargeYear]
      ,[DischargeQuarter]
      ,[DaysOnMechVentilator]
      ,[BirthWeightGrams]
      ,[TotalCharge]
      ,[DRG]
      ,[MDC]
      ,[PrincipalDiagnosis]
      ,[DiagnosisCode2]
      ,[DiagnosisCode3]
      ,[DiagnosisCode4]
      ,[DiagnosisCode5]
      ,[DiagnosisCode6]
      ,[DiagnosisCode7]
      ,[DiagnosisCode8]
      ,[DiagnosisCode9]
      ,[DiagnosisCode10]
      ,[DiagnosisCode11]
      ,[DiagnosisCode12]
      ,[DiagnosisCode13]
      ,[DiagnosisCode14]
      ,[DiagnosisCode15]
      ,[DiagnosisCode16]
      ,[DiagnosisCode17]
      ,[DiagnosisCode18]
      ,[DiagnosisCode19]
      ,[DiagnosisCode20]
      ,[DiagnosisCode21]
      ,[DiagnosisCode22]
      ,[DiagnosisCode23]
      ,[DiagnosisCode24]
      ,[PrincipalProcedure]
      ,[ProcedureCode2]
      ,[ProcedureCode3]
      ,[ProcedureCode4]
      ,[ProcedureCode5]
      ,[ProcedureCode6]
      ,[ProcedureCode7]
      ,[ProcedureCode8]
      ,[ProcedureCode9]
      ,[ProcedureCode10]
      ,[ProcedureCode11]
      ,[ProcedureCode12]
      ,[ProcedureCode13]
      ,[ProcedureCode14]
      ,[ProcedureCode15]
      ,[ProcedureCode16]
      ,[ProcedureCode17]
      ,[ProcedureCode18]
      ,[ProcedureCode19]
      ,[ProcedureCode20]
      ,[ProcedureCode21]
      ,[ProcedureCode22]
      ,[ProcedureCode23]
      ,[ProcedureCode24]
      ,[CustomStratifier1]
      ,[CustomStratifier2]
      ,[CustomStratifier3]
      ,[PatientID]
      ,[BirthDate]
      ,[AdmissionDate]
      ,CASE 
            WHEN EDServices = 'Exclude' THEN 'Exclude from Dataset'
            WHEN EDServices = 'NoEdReported' THEN 'No ED Services Reported'
            WHEN EDServices = 'EdReported' THEN 'ED Services Reported'
            ELSE EDServices
       END [EDServices]    
	  ,CASE WHEN @IsDBVersion1=1 THEN NULL
	        ELSE (SELECT TOP 1 r.ImportRegionId
					FROM   [@@DESTINATION@@].[dbo].[Hospitals_Regions] r
					WHERE  UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION'
					AND RTRIM(LTRIM(r.Name)) = (SELECT RTRIM(LTRIM(r.Name))
										FROM   [@@SOURCE@@].[dbo].[Hospitals_Regions] r
                                     WHERE r.ImportRegionId=ip.[HRRRegionID]
								     AND UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION'
									 )
	               AND r.State_Id = (SELECT bs2.Id
	                                   FROM   [@@SOURCE@@].[dbo].[Hospitals_Regions] r
	                                       INNER JOIN [@@SOURCE@@].[dbo].Base_States bs
	                                       ON bs.Id = r.State_Id
	                                       INNER JOIN  [@@DESTINATION@@].dbo.Base_States bs2
	                                       ON bs2.FIPSState=bs.FIPSState
                                     WHERE r.ImportRegionId=ip.[HRRRegionID]
								     AND UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION'
									 )) END [HRRRegionID]
      ,CASE WHEN @IsDBVersion1=1 THEN NULL
	        ELSE (SELECT TOP 1 r.ImportRegionId
				  FROM   [@@DESTINATION@@].[dbo].[Hospitals_Regions] r
					WHERE  UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA'
					AND RTRIM(LTRIM(r.Name)) = (SELECT RTRIM(LTRIM(r.Name))
										FROM   [@@SOURCE@@].[dbo].[Hospitals_Regions] r
                                     WHERE r.ImportRegionId=ip.[HSARegionID]
								     AND UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA'
									 )
	               AND r.State_Id = (SELECT bs2.Id
	                                   FROM   [@@SOURCE@@].[dbo].[Hospitals_Regions] r
	                                       INNER JOIN [@@SOURCE@@].[dbo].Base_States bs
	                                       ON bs.Id = r.State_Id
	                                       INNER JOIN  [@@DESTINATION@@].dbo.Base_States bs2
	                                       ON bs2.FIPSState=bs.FIPSState
                                     WHERE r.ImportRegionId=ip.[HSARegionID]
								     AND UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA'
									 )) END [HSARegionID]
      ,CASE WHEN @IsDBVersion1=1 THEN NULL
	        ELSE  (SELECT TOP 1 r.ImportRegionId
					FROM   [@@DESTINATION@@].[dbo].[Hospitals_Regions] r
						WHERE  UPPER(r.[RegionType]) = 'CUSTOMREGION'
						AND RTRIM(LTRIM(r.Name)) = (SELECT RTRIM(LTRIM(r.Name))
										FROM   [@@SOURCE@@].[dbo].[Hospitals_Regions] r
                                     WHERE r.ImportRegionId=ip.[CustomRegionID]
								     AND UPPER(r.[RegionType]) = 'CUSTOMREGION'
									 )
	               AND r.State_Id = (SELECT bs2.Id
	                                   FROM   [@@SOURCE@@].[dbo].[Hospitals_Regions] r
	                                       INNER JOIN [@@SOURCE@@].[dbo].Base_States bs
	                                       ON bs.Id = r.State_Id
	                                       INNER JOIN  [@@DESTINATION@@].dbo.Base_States bs2
	                                       ON bs2.FIPSState=bs.FIPSState
                                     WHERE r.ImportRegionId=ip.[CustomRegionID]
								     AND UPPER(r.[RegionType]) = 'CUSTOMREGION'
									 )) END [CustomRegionID]
      ,CASE WHEN @IsDBVersion1=1 THEN NULL ELSE ip.[PatientZipCode] END  [PatientZipCode]  
      ,@NewDataSetId
			FROM [@@SOURCE@@].[dbo].[Targets_InpatientTargets] ip
			WHERE [ContentItemRecord_Id] =  @Dataset_Id 

		FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id, @DatasetType, @DatasetSummary_Id
	END

	--END TRY
	--BEGIN CATCH
		 --IF @@TRANCOUNT > 0
			--ROLLBACK TRANSACTION;
	--	 SELECT 0;
	--END CATCH

	--IF @@TRANCOUNT > 0
	--		COMMIT TRANSACTION;

	CLOSE @Dataset_Cursor;
	DEALLOCATE @Dataset_Cursor;


-- Update table values now

--update [@@DESTINATION@@].[dbo].[Targets_InpatientTargets]  
--set 
--Sex=
--	case
--	when Sex='Exclude' then 'Exclude from Dataset'
--	else
--	Sex
--	end,
--AdmissionSource=
--	case
--	when AdmissionSource='Exclude' then 'Exclude from Dataset'
--	when AdmissionSource='OtherHospital' then 'Another hospital'
--	when AdmissionSource='OtherFacility' then 'Another fac. incl. LTC'
--	when AdmissionSource='LegalSystem' then 'Court/Law enforcement'
--	when AdmissionSource='Routine' then 'Routine/Birth/Other'
--	else
--	AdmissionSource
--	end,
--AdmissionType=
--	case
--	when AdmissionType='Exclude' then 'Exclude from Dataset'
--	when AdmissionType='Trauma' then 'Trauma Center'
--	else
--	AdmissionType
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
--	end,
--EDServices =
--	case
--	when  EDServices ='Exclude' then 'Exclude from Dataset'
--	when  EDServices ='NoEdReported' then 'No ED Services Reported'
--	when  EDServices ='EdReported' then 'ED Services Reported'
--	else
--	EDServices
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
--	end
--,PointOfOrigin=
--	case
--	when PointOfOrigin='Exclude' then 'Exclude from Dataset'
--	when PointOfOrigin='NonHealthCare' then 'Non-health care facility point of origin'
--	when PointOfOrigin='TransferFromOther' then 'Transfer from a hospital (different facility)'
--	when PointOfOrigin='TransferInternal' then 'Transfer from nursing facility OR (w/admin type = newborn) born in this hospital'
--	when PointOfOrigin='TransferExternal' then 'Transfer from another health care facility OR (w/admin type = newborn) born outside this hospital'
--	when PointOfOrigin='ER' then 'Emergency room'
--	when PointOfOrigin='LegalSystem' then 'Court/law enforcement'
--	when PointOfOrigin='OtherHealthAgency' then 'Transfer from another Home Health Agency'
--	when PointOfOrigin='ReadminFromSame' then 'Readmission to Same Home Health Agency'
--	when PointOfOrigin='TransferDistrict' then 'Transfer from one distinct unit of the hospital to another distinct unit of the same hospital'
--	when PointOfOrigin='TransferAmbulatory' then 'Transfer from ambulatory surgery center'
--	when PointOfOrigin='TransferHospice' then 'Transfer from hospice and is under a hospice plan of care or enrolled in a hospice program'
--	else
--	PointOfOrigin
--  end

SET @LoopIterator=1		
WHILE (@LoopIterator<=@LoopCount)
BEGIN
	SELECT @sql='ALTER INDEX ['+i.name+'] ON [@@DESTINATION@@].[dbo].[Targets_InpatientTargets] REBUILD'
	FROM #IndexList i
	WHERE rownum=@LoopIterator
	
	EXEC (@sql)
	SET @LoopIterator=@LoopIterator+1
END

SELECT 1;