BEGIN	TRY	


UPDATE [dbo].[Wings_Targets]
SET Wing_Id = (SELECT TOP 1 Id FROM Wings WHERE Name ='Physician Data')
WHERE Name ='Physician Data'

UPDATE [dbo].[Wings_Targets]
SET Wing_Id = (SELECT TOP 1 Id FROM Wings WHERE Name ='Nursing Home Compare Data')
WHERE Name ='Nursing Home Compare Data'


/*
 * Bug# 4421	
*/
UPDATE [dbo].[Measures]
SET Name = 'SCIP-INF-3'
WHERE Name = 'SCIP-INF-3a'

UPDATE [dbo].[Measures]
SET Name = 'SCIP-INF-2'
WHERE Name = 'SCIP-INF-2a'

UPDATE [dbo].[Measures]
SET Name = 'SCIP-INF-1'
WHERE Name = 'SCIP-INF-1a'

UPDATE [dbo].[Measures]
SET [Description] = 'Patients who had one of eleven common patient safety problems in the hospital.',
[ClinicalTitle] = 'Patient safety composite for selected indicators (PSI 03-Pressure Ulcer Rate,  PSI 06- Iatrogenic Pneumothorax Rate, PSI 07-Central Venous Catheter-Related Blood Stream Infection Rate, PSI 08 - Postoperative Hip Fracture Rate,  PSI 09 - Perioperative Hemorrhage or Hematoma Rate, PSI 10 -  Postoperative Physiologic and Metabolic Derangement Rate, PSI 11 - Postoperative Respiratory Failure Rate, PSI 12 - Perioperative Pulmonary Embolism or Deep Vein Thrombosis Rate, PSI 13 -  Postoperative Sepsis Rate, PSI 14- Postoperative Wound Dehiscence Rate, PSI 15 - Accidental Puncture or Laceration Rate)'
WHERE [Name] = 'PSI 90'

MERGE INTO [dbo].[TopicCategories] T
USING (VALUES('Emergency department (ED)','Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.Infections Ratings about health care associated infections, or HAIs. Health care-associated infections  are infections that people acquire while they are receiving treatment for another condition in a health care setting. HAIs can be acquired anywhere health care is delivered, including  hospitals, clinics, or nursing homes. HAIs may be caused by a bacteria or virus.'),
('Prevention and Treatment','Ratings about serious medical conditions that could have been prevented if patients are provided the right care at the right time.'),
('Deaths or returns to the hospital', 'Ratings about numbers of deaths or returns to the hospital, also called readmissions.  A high number of deaths or returns to the hospital may mean the hospital is not treating people effectively. A higher number of deaths or returns to the hospital may mean the hospital is not treating people effectively. Measures provide information on the number of deaths and returns to the hospital organized by Health Topic or Condition.')) Temp (Col1, Col2) on Temp.Col1 = T.Name
WHEN MATCHED THEN 
	UPDATE SET T.LongTitle = Temp.Col2;


-- Update Inpatient Target RegionIds
INSERT INTO [dbo].[Targets_InpatientTargets](
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
      ,[Dataset_id]
	  ,[PointOfOrigin])
SELECT [Key]
      ,[Age]
      ,[AgeInDays]
      ,CASE [Race] 
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
	   END [Race]
      ,Case [Sex]
			WHEN 'Exclude from Dataset' THEN -1
			WHEN 'Male' THEN 1
			WHEN 'Female' THEN 2
	   END [Sex]
      ,Case [PrimaryPayer]
            WHEN 'Exclude from Dataset' THEN -1
            WHEN 'Missing' THEN 0 
            WHEN 'Medicare' THEN 1
            WHEN 'Medicaid' THEN 2
            WHEN 'Private including HMO' THEN 3
			WHEN 'Self-pay' THEN 4
            WHEN 'No Charge' THEN 5
			WHEN 'Other' THEN 6
            WHEN 'Retain Value' THEN 99
	   END [PrimaryPayer]
      ,[PatientStateCountyCode]
      ,[LocalHospitalID]
      ,CASE [DischargeDisposition]
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
       END [DischargeDisposition]
      ,CASE[AdmissionType]
			WHEN 'Exclude from Dataset' THEN -1
			WHEN 'Missing' THEN 0
			WHEN 'Emergency' THEN 1
			WHEN 'Urgent' THEN 2
			WHEN 'Elective' THEN 3
			WHEN 'Newborn' THEN 4
			WHEN 'Trauma Center' THEN 5
			WHEN 'Other' THEN 6
			--ELSE 0
	  END [AdmissionType]
      ,CASE [AdmissionSource]
            WHEN 'Exclude from Dataset' THEN -1 
			WHEN 'Missing' THEN 1
            WHEN 'Another hospital' THEN 2 
            WHEN 'Another fac. incl. LTC' THEN 3 
            WHEN 'Court/Law enforcement' THEN 4 
            WHEN 'Routine/Birth/Other' THEN 5
			--ELSE 1
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
            WHEN EDServices IS NULL THEN -1 
            WHEN EDServices = 'Exclude from Dataset' THEN -1
            WHEN EDServices = 'No ED Services Reported' THEN 0
            WHEN EDServices = 'NoEdReported' THEN 0
            WHEN EDServices = 'EdReported' THEN 1
            WHEN EDServices = 'ED Services Reported' THEN 1
            ELSE -1
       END [EDServices]    
	  ,(SELECT TOP 1 r.ImportRegionId
	              FROM [dbo].[Regions] r
					WHERE  UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION'
					AND RTRIM(LTRIM(r.Name)) = (SELECT RTRIM(LTRIM(r.Name))
										FROM   [dbo].[Hospitals_Regions] r
                                     WHERE r.ImportRegionId=ip.[HRRRegionID]
								     AND UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION'
									 )
	               AND r.[State] = (SELECT bs.Abbreviation
	                                   FROM   [dbo].[Hospitals_Regions] r
	                                       INNER JOIN [dbo].Base_States bs
	                                       ON bs.Id = r.State_Id
                                     WHERE r.ImportRegionId=ip.[HRRRegionID]
								     AND UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION'
									 )) [HRRRegionID]
      ,(SELECT TOP 1 r.ImportRegionId
				  FROM   [dbo].[Regions] r
					WHERE  UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA'
					AND RTRIM(LTRIM(r.Name)) = (SELECT RTRIM(LTRIM(r.Name))
										FROM   [dbo].[Hospitals_Regions] r
                                     WHERE r.ImportRegionId=ip.[HSARegionID]
								     AND UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA'
									 )
	               AND r.[State] = (SELECT bs.Abbreviation
	                                   FROM   [dbo].[Hospitals_Regions] r
	                                       INNER JOIN [dbo].Base_States bs
	                                       ON bs.Id = r.State_Id
                                     WHERE r.ImportRegionId=ip.[HSARegionID]
								     AND UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA'
									 )) [HSARegionID]
      ,(SELECT TOP 1 r.ImportRegionId
					FROM   [dbo].[Regions] r
						WHERE  UPPER(r.[RegionType]) = 'CUSTOMREGION'
						AND RTRIM(LTRIM(r.Name)) = (SELECT TOP(1) RTRIM(LTRIM(r.Name))
										FROM   [dbo].[Hospitals_Regions] r
                                     WHERE r.ImportRegionId=ip.[CustomRegionID]
								     AND UPPER(r.[RegionType]) = 'CUSTOMREGION'
									 )
	               AND r.[State] = (SELECT TOP(1) bs.Abbreviation
	                                   FROM   [dbo].[Hospitals_Regions] r
	                                       INNER JOIN [dbo].Base_States bs
	                                       ON bs.Id = r.State_Id
                                     WHERE r.ImportRegionId=ip.[CustomRegionID]
								     AND UPPER(r.[RegionType]) = 'CUSTOMREGION'
									 ))[CustomRegionID]
      ,ip.[PatientZipCode] [PatientZipCode]  
      ,[ContentItemRecord_Id] Dataset_id, 
	  (Case PointOfOrigin 
	  WHEN 'Exclude from Dataset' THEN -1
	  WHEN 'Missing' THEN 0
	  WHEN 'Non-health care facility point of origin' THEN 1
	  WHEN 'Clinic' THEN 2
	  WHEN 'Transfer from a hospital (different facility)'  THEN 4
	  WHEN 'Transfer from nursing facility OR (w/admin type = newborn) born in this hospital' THEN 5
      WHEN 'Transfer from another health care facility OR (w/admin type = newborn) born outside this hospital'  THEN 6
	  WHEN 'Emergency room'  THEN 7
	  WHEN 'Court/law enforcement'  THEN 8
	  WHEN 'Transfer from another Home Health Agency' THEN 11
	  WHEN 'Readmission to Same Home Health Agency' THEN 12
	  WHEN 'Transfer from one distinct unit of the hospital to another distinct unit of the same hospital' THEN 13
	  WHEN 'Transfer from ambulatory surgery center' THEN 14
	  WHEN 'Transfer from hospice and is under a hospice plan of care or enrolled in a hospice program' THEN 15
	  END) AS PointOfOrigin
FROM Targets_InpatientTargetsTemp ip
		join [ContentItemRecords] cit on cit.Id = ip.ContentItemRecord_Id;

UPDATE [dbo].[Wings_Targets]
SET [Name] = (CASE UPPER([Name])
				WHEN 'INPATIENT DATA' THEN 'Inpatient Data'
				WHEN 'TREAT AND RELEASE DISCHARGE DATA' THEN 'ED Treat And Release'
			  END)
WHERE UPPER([Name])IN ('INPATIENT DATA','TREAT AND RELEASE DISCHARGE DATA');

IF OBJECT_ID(N'FKB2029719F64562AC') IS NOT NULL ALTER table Targets_AhrqTargets drop constraint	FKB2029719F64562AC												
IF OBJECT_ID(N'FK_Targets_InpatientTargets_ContentITemRecord') IS NOT NULL  alter table Targets_InpatientTargets drop constraint	FK_Targets_InpatientTargets_ContentITemRecord
IF OBJECT_ID(N'FK_Targets_TreatAndReleaseTargets_ContentITemRecord') IS NOT NULL alter table Targets_TreatAndReleaseTargets drop constraint	FK_Targets_TreatAndReleaseTargets_ContentITemRecord
IF OBJECT_ID(N'FK_Targets_HospitalCompareFootnotes_ContentITemRecord')IS NOT NULL  alter table Targets_HospitalCompareFootnotes drop constraint	FK_Targets_HospitalCompareFootnotes_ContentITemRecord
IF OBJECT_ID(N'FK_Targets_HospitalCompareTargets_ContentITemRecord') IS NOT NULL alter TABLE Targets_HospitalCompareTargets  drop constraint	FK_Targets_HospitalCompareTargets_ContentITemRecord
IF OBJECT_ID(N'FK_Targets_MedicareProviderChargeTargets_ContentITemRecord') IS NOT NULL  alter TABLE Targets_MedicareProviderChargeTargets  drop constraint	FK_Targets_MedicareProviderChargeTargets_ContentITemRecord
IF OBJECT_ID(N'FK79BBF9D0E8BFB9C1') IS NOT NULL  alter table Websites_WebsiteDatasets drop constraint	FK79BBF9D0E8BFB9C1														

IF OBJECT_ID('Targets_InpatientTargetsTemp') IS NOT NULL
    DROP TABLE Targets_InpatientTargetsTemp

IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'Targets_InpatientTargets')
	ALTER TABLE	Targets_InpatientTargets DROP COLUMN ContentItemRecord_Id

IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_TARGETS_AHRQTARGETS' AND object_id = OBJECT_ID(N'Targets_AhrqTargets'))
	DROP INDEX [IX_TARGETS_AHRQTARGETS] ON [dbo].[Targets_AhrqTargets]
IF OBJECT_ID(N'FK_Targets_HospitalCompareFootnotes_ContentITemRecord') IS NOT NULL
	ALTER TABLE Targets_HospitalCompareFootnotes DROP CONSTRAINT FK_Targets_HospitalCompareFootnotes_ContentITemRecord

IF OBJECT_ID(N'FK_Targets_HospitalCompareTargets_ContentITemRecord') IS NOT NULL
	ALTER TABLE Targets_HospitalCompareTargets DROP CONSTRAINT FK_Targets_HospitalCompareTargets_ContentITemRecord

IF OBJECT_ID(N'FK_Targets_HospitalCompareTargets_ContentITemRecord') IS NOT NULL
	ALTER TABLE Targets_AhrqTargets DROP CONSTRAINT FK_Targets_HospitalCompareTargets_ContentITemRecord

IF OBJECT_ID(N'FK630384EEF64562AC') IS NOT NULL
	ALTER TABLE ContentItemVersionRecords DROP CONSTRAINT [FK630384EEF64562AC]

IF OBJECT_ID(N'FK7A3D58C5F64562AC') IS NOT NULL
	ALTER TABLE ContentPartTransactionRecords DROP CONSTRAINT [FK7A3D58C5F64562AC]

IF OBJECT_ID(N'FK7DB73EC5B05F82BA') IS NOT NULL
	ALTER TABLE [dbo].[ContentItemRecords] DROP CONSTRAINT [FK7DB73EC5B05F82BA]
IF OBJECT_ID(N'FK7DB73EC54162829A') IS NOT NULL
	ALTER TABLE [dbo].[ContentItemRecords] DROP CONSTRAINT [FK7DB73EC54162829A]
IF OBJECT_ID(N'DF__ContentIt__IsFin__03317E3D') IS NOT NULL
	ALTER TABLE [dbo].[ContentItemRecords] DROP CONSTRAINT [DF__ContentIt__IsFin__03317E3D]

IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'ContentItemRecords')
	ALTER TABLE	ContentItemRecords DROP COLUMN ContentItemRecord_Id

IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'ContentItemVersionRecords')
	ALTER TABLE	ContentItemVersionRecords DROP COLUMN ContentItemRecord_Id

IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'ContentPartTransactionRecords')
	ALTER TABLE	ContentPartTransactionRecords DROP COLUMN ContentItemRecord_Id

IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'ContentItemSummaryRecord')
	ALTER TABLE	ContentItemSummaryRecord DROP COLUMN ContentItemRecord_Id

IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'Summary_Id' AND TABLE_NAME = 'ContentItemSummaryRecord')
	ALTER TABLE	ContentItemRecords DROP COLUMN Summary_Id


IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'Targets_InpatientTargets')
	ALTER TABLE	Targets_InpatientTargets DROP COLUMN ContentItemRecord_Id
IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'Targets_AhrqTargets')
	ALTER TABLE	Targets_AhrqTargets DROP COLUMN ContentItemRecord_Id
IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'Targets_HospitalCompareFootnotes')
	ALTER TABLE	Targets_HospitalCompareFootnotes DROP COLUMN ContentItemRecord_Id
IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'Targets_HospitalCompareTargets')
	ALTER TABLE	Targets_HospitalCompareTargets DROP COLUMN ContentItemRecord_Id
IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'Targets_MedicareProviderChargeTargets')
	ALTER TABLE	Targets_MedicareProviderChargeTargets DROP COLUMN ContentItemRecord_Id
IF EXISTS(SELECT * FROM Information_Schema.columns WHERE COLUMN_NAME = 'ContentItemRecord_Id' AND TABLE_NAME = 'Targets_TreatAndReleaseTargets')
	ALTER TABLE	Targets_TreatAndReleaseTargets DROP COLUMN ContentItemRecord_Id

IF OBJECT_ID(N'ContentItemRecords') IS NOT NULL DROP TABLE ContentItemRecords
IF OBJECT_ID(N'Measures_MetadataItems') IS NOT NULL DROP TABLE Measures_MetadataItems
IF OBJECT_ID(N'ContentItemSummaryRecords') IS NOT NULL DROP TABLE ContentItemSummaryRecords
IF OBJECT_ID(N'ContentItemVersionRecords') IS NOT NULL DROP TABLE ContentItemVersionRecords
IF OBJECT_ID(N'ContentPartTransactionRecords') IS NOT NULL DROP TABLE ContentPartTransactionRecords
IF OBJECT_ID(N'ContentTypeRecords') IS NOT NULL DROP TABLE ContentTypeRecords
IF OBJECT_ID(N'Measures_20150311_Bkp') IS NOT NULL DROP TABLE Measures_20150311_Bkp
IF OBJECT_ID(N'TempWingIdFeatureIdMapping') IS NOT NULL DROP TABLE TempWingIdFeatureIdMapping
IF OBJECT_ID(N'RegionTemp') IS NOT NULL DROP TABLE RegionTemp
IF OBJECT_ID(N'Base_Audits') IS NOT NULL DROP TABLE Base_Audits
IF OBJECT_ID(N'Hospitals_Regions') IS NOT NULL DROP TABLE Hospitals_Regions
IF OBJECT_ID(N'Hospitals_RegionPopulationStrats') IS NOT NULL DROP TABLE Hospitals_RegionPopulationStrats

IF object_id(N'[dbo].[TempTopics]') IS NOT NULL
	DROP TABLE [dbo].[TempTopics]

CREATE TABLE [dbo].[TempTopics](
	[Topic] [nvarchar](255) NULL,
	[Subtopic] [nvarchar](255) NULL,
	[Text Currently Displaying] [nvarchar](max) NULL,
	[Suggested Text] [nvarchar](max) NULL,
	[Measure Codes] [nvarchar](255) NULL,
	[MeasureName] [varchar](100) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'All Causes', NULL, N'A higher number of deaths or returns to the hospital related to patient care, no matter why the patient was in the hospital, may mean the hospital is not treating people effectively. ', N'READM-HOSP-ALL-WIDE', N'READM-HOSP-ALL-WIDE')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart attack and chest pain', NULL, N'A heart attack, also called an AMI or acute myocardial infarction, happens when the arteries leading to the heart become blocked and the blood supply slows or stops. A high number of deaths or returns to the hospital related to care for a heart attack may mean the hospital is not treating people effectively for these conditions.', N'MORT-30-AMI, IQI 32, IQI 15', N'MORT-30-AMI')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart attack and chest pain', NULL, N'A heart attack, also called an AMI or acute myocardial infarction, happens when the arteries leading to the heart become blocked and the blood supply slows or stops. A high number of deaths or returns to the hospital related to care for a heart attack may mean the hospital is not treating people effectively for these conditions.', N'MORT-30-AMI, IQI 32, IQI 15', N' IQI 32')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart attack and chest pain', NULL, N'A heart attack, also called an AMI or acute myocardial infarction, happens when the arteries leading to the heart become blocked and the blood supply slows or stops. A high number of deaths or returns to the hospital related to care for a heart attack may mean the hospital is not treating people effectively for these conditions.', N'MORT-30-AMI, IQI 32, IQI 15', N' IQI 15')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart failure', NULL, N'Heart failure or congestive heart failure is a weakening of the heart''s pumping power that prevents the body from getting enough oxygen and nutrients to meet its needs. A high number of deaths or returns to the hospital related to care for a heart failure may mean the hospital is not treating people effectively for these conditions.', N'READM-30-HF, MORT-30-HF, IQI 16', N'READM-30-HF')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart failure', NULL, N'Heart failure or congestive heart failure is a weakening of the heart''s pumping power that prevents the body from getting enough oxygen and nutrients to meet its needs. A high number of deaths or returns to the hospital related to care for a heart failure may mean the hospital is not treating people effectively for these conditions.', N'READM-30-HF, MORT-30-HF, IQI 16', N' MORT-30-HF')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart failure', NULL, N'Heart failure or congestive heart failure is a weakening of the heart''s pumping power that prevents the body from getting enough oxygen and nutrients to meet its needs. A high number of deaths or returns to the hospital related to care for a heart failure may mean the hospital is not treating people effectively for these conditions.', N'READM-30-HF, MORT-30-HF, IQI 16', N' IQI 16')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart surgeries and procedures', NULL, N'Heart surgeries and procedures related to the heart include  angioplasty and coronary bypass surgery. A high number of deaths or returns to the hospital related to heart surgeries or procedures may mean the hospital is not treating people effectively for these conditions or procedures.', N'IQI 30, IQI 12', N'IQI 30')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Heart surgeries and procedures', NULL, N'Heart surgeries and procedures related to the heart include  angioplasty and coronary bypass surgery. A high number of deaths or returns to the hospital related to heart surgeries or procedures may mean the hospital is not treating people effectively for these conditions or procedures.', N'IQI 30, IQI 12', N' IQI 12')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Other surgeries', NULL, N'Surgeries other than heart surgery such as brain surgery (craniotomy) and hip replacement surgery. A high number of deaths or returns to the hospital related to other surgeries may mean the hospital is not treating people effectively when they have these surgeries.', N'IQI 08, IQI 14, IQI 13, IQI 11, IQI 09', N'IQI 08')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Other surgeries', NULL, N'Surgeries other than heart surgery such as brain surgery (craniotomy) and hip replacement surgery. A high number of deaths or returns to the hospital related to other surgeries may mean the hospital is not treating people effectively when they have these surgeries.', N'IQI 08, IQI 14, IQI 13, IQI 11, IQI 09', N' IQI 14')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Other surgeries', NULL, N'Surgeries other than heart surgery such as brain surgery (craniotomy) and hip replacement surgery. A high number of deaths or returns to the hospital related to other surgeries may mean the hospital is not treating people effectively when they have these surgeries.', N'IQI 08, IQI 14, IQI 13, IQI 11, IQI 09', N' IQI 13')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Other surgeries', NULL, N'Surgeries other than heart surgery such as brain surgery (craniotomy) and hip replacement surgery. A high number of deaths or returns to the hospital related to other surgeries may mean the hospital is not treating people effectively when they have these surgeries.', N'IQI 08, IQI 14, IQI 13, IQI 11, IQI 09', N' IQI 11')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Other surgeries', NULL, N'Surgeries other than heart surgery such as brain surgery (craniotomy) and hip replacement surgery. A high number of deaths or returns to the hospital related to other surgeries may mean the hospital is not treating people effectively when they have these surgeries.', N'IQI 08, IQI 14, IQI 13, IQI 11, IQI 09', N' IQI 09')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Patient safety', NULL, N'Deaths or returns to the hospital related to both surgical and nonsurgical care. A high number of deaths or returns to the hospital related to how well the hospital keeps patients safe may mean the hospital is not taking steps to prevent safety problems from happening.', N'PSI 02, IQI 19, IQI 18 ', N'PSI 02')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Patient safety', NULL, N'Deaths or returns to the hospital related to both surgical and nonsurgical care. A high number of deaths or returns to the hospital related to how well the hospital keeps patients safe may mean the hospital is not taking steps to prevent safety problems from happening.', N'PSI 02, IQI 19, IQI 18 ', N' IQI 19')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Patient safety', NULL, N'Deaths or returns to the hospital related to both surgical and nonsurgical care. A high number of deaths or returns to the hospital related to how well the hospital keeps patients safe may mean the hospital is not taking steps to prevent safety problems from happening.', N'PSI 02, IQI 19, IQI 18 ', N' IQI 18 ')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Pneumonia', NULL, N'Pneumonia is a serious lung infection that causes difficulty breathing, fever, cough and fatigue. A high number of deaths or returns to the hospital related to care for pneumonia may mean the hospital is not treating people with pneumonia effectively.', N'MORT-30-PN, IQI 20, READM-30-PN', N'MORT-30-PN')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Pneumonia', NULL, N'Pneumonia is a serious lung infection that causes difficulty breathing, fever, cough and fatigue. A high number of deaths or returns to the hospital related to care for pneumonia may mean the hospital is not treating people with pneumonia effectively.', N'MORT-30-PN, IQI 20, READM-30-PN', N' IQI 20')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Pneumonia', NULL, N'Pneumonia is a serious lung infection that causes difficulty breathing, fever, cough and fatigue. A high number of deaths or returns to the hospital related to care for pneumonia may mean the hospital is not treating people with pneumonia effectively.', N'MORT-30-PN, IQI 20, READM-30-PN', N' READM-30-PN')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Stroke', NULL, N'A stroke happens when the blood supply to the brain stops.  A high number of deaths or returns to the hospital related to care for a stroke may mean the hospital is not treating people who have had a stroke effectively.', N'IQI 17', N'IQI 17')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Deaths or returns to the hospital', N'Surgical patient safety', NULL, N'Deaths or returns to the hospital related to surgical care. A high number of deaths or returns to the hospital related to surgical care may mean the hospital is not taking steps to prevent surgical safety problems from happening.', N'PSI 04', N'PSI 04')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Emergency department (ED)', N'Throughput', NULL, N'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.', N'ED-1b, ED-2b, OP-18b, OP-20, OP-21, OP-22, PC-01', N'ED-1b')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Emergency department (ED)', N'Throughput', NULL, N'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.', N'ED-1b, ED-2b, OP-18b, OP-20, OP-21, OP-22, PC-01', N' ED-2b')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Emergency department (ED)', N'Throughput', NULL, N'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.', N'ED-1b, ED-2b, OP-18b, OP-20, OP-21, OP-22, PC-01', N' OP-18b')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Emergency department (ED)', N'Throughput', NULL, N'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.', N'ED-1b, ED-2b, OP-18b, OP-20, OP-21, OP-22, PC-01', N' OP-20')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Emergency department (ED)', N'Throughput', NULL, N'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.', N'ED-1b, ED-2b, OP-18b, OP-20, OP-21, OP-22, PC-01', N' OP-21')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Emergency department (ED)', N'Throughput', NULL, N'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.', N'ED-1b, ED-2b, OP-18b, OP-20, OP-21, OP-22, PC-01', N' OP-22')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Emergency department (ED)', N'Throughput', NULL, N'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.', N'ED-1b, ED-2b, OP-18b, OP-20, OP-21, OP-22, PC-01', N' PC-01')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Infections', N'Healthcare-associated', NULL, N'Health care-associated infections  are infections that people acquire while they are receiving treatment for another condition in a health care setting. HAIs can be acquired anywhere health care is delivered, including  hospitals, clinics, or nursing homes. HAIs may be caused by a bacteria or virus. 
A high number of health care associated infections may mean the hospital is not doing a good job at preventing infections.', N'HAI-2, HAI-5, HAI-6, IMM-1a, IMM-2', N'HAI-2')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Infections', N'Healthcare-associated', NULL, N'Health care-associated infections  are infections that people acquire while they are receiving treatment for another condition in a health care setting. HAIs can be acquired anywhere health care is delivered, including  hospitals, clinics, or nursing homes. HAIs may be caused by a bacteria or virus. 
A high number of health care associated infections may mean the hospital is not doing a good job at preventing infections.', N'HAI-2, HAI-5, HAI-6, IMM-1a, IMM-2', N' HAI-5')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Infections', N'Healthcare-associated', NULL, N'Health care-associated infections  are infections that people acquire while they are receiving treatment for another condition in a health care setting. HAIs can be acquired anywhere health care is delivered, including  hospitals, clinics, or nursing homes. HAIs may be caused by a bacteria or virus. 
A high number of health care associated infections may mean the hospital is not doing a good job at preventing infections.', N'HAI-2, HAI-5, HAI-6, IMM-1a, IMM-2', N' HAI-6')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Infections', N'Healthcare-associated', NULL, N'Health care-associated infections  are infections that people acquire while they are receiving treatment for another condition in a health care setting. HAIs can be acquired anywhere health care is delivered, including  hospitals, clinics, or nursing homes. HAIs may be caused by a bacteria or virus. 
A high number of health care associated infections may mean the hospital is not doing a good job at preventing infections.', N'HAI-2, HAI-5, HAI-6, IMM-1a, IMM-2', N' IMM-1a')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Infections', N'Healthcare-associated', NULL, N'Health care-associated infections  are infections that people acquire while they are receiving treatment for another condition in a health care setting. HAIs can be acquired anywhere health care is delivered, including  hospitals, clinics, or nursing homes. HAIs may be caused by a bacteria or virus. 
A high number of health care associated infections may mean the hospital is not doing a good job at preventing infections.', N'HAI-2, HAI-5, HAI-6, IMM-1a, IMM-2', N' IMM-2')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Prevention and Treatment', N'Blood Clot', NULL, N'Information about the number of patients who developed a blood clot while in the hospital and did not get treatment that could have prevented it.', N'VTE-6', N'VTE-6')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Summary Scores', N'Deaths', NULL, N'Summary score that combines more than one rating related to the number of patients who die in the hospital into one score, including death as a result of medical conditions or major surgeries.', N'IQI 90, IQI 91', N'IQI 90')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Summary Scores', N'Deaths', NULL, N'Summary score that combines more than one rating related to the number of patients who die in the hospital into one score, including death as a result of medical conditions or major surgeries.', N'IQI 90, IQI 91', N' IQI 91')
INSERT [dbo].[TempTopics] ([Topic], [Subtopic], [Text Currently Displaying], [Suggested Text], [Measure Codes], [MeasureName]) VALUES (N'Summary Scores', N'Patient safety', NULL, N'Summary score that combine more than one rating related to how well the hospital keeps patients safe into one score.', N'PSI 90', N'PSI 90')



UPDATE [dbo].[Topics]
SET LongTitle = tt.[Suggested Text]
FROM [dbo].[Measures] m
	INNER JOIN [dbo].[Measures_MeasureTopics] mt on m.Id = mt.Measure_Id
	INNER JOIN [dbo].[Topics] t on t.Id = mt.Topic_Id
	INNER JOIN [dbo].[TopicCategories] tc on tc.Id = t.TopicCategory_id
	INNER JOIN [dbo].[TempTopics] tt on tt.[MeasureName] = m.Name AND tt.SubTopic = t.Name AND tt.Topic = tc.Name


IF object_id(N'[dbo].[TempTopics]') IS NOT NULL
	DROP TABLE [dbo].[TempTopics]

	/*
 * Bug# 4420	
*/

Declare @TopicCategoryId int,@TopicName nvarchar(max), @TopicLongTitle nvarchar(max)
DECLARE @Name nvarchar(255), @LongTitle nvarchar(MAX), @Description nvarchar(MAX), @TopicType nvarchar(25)
set @Name = 'Deaths or returns to the hospital'
set @LongTitle= 'Ratings about numbers of deaths or returns to the hospital, also called readmissions. A high number of deaths or returns to the hospital may mean the hospital is not treating people effectively. A higher number of deaths or returns to the hospital may mean the hospital is not treating people effectively. Measures provide information on the number of deaths and returns to the hospital organized by Health Topic or Condition.'
Set @Description= 'A heart attack, also called an AMI or acute myocardial infarction, happens when the arteries leading to the heart become blocked and the blood supply slows or stops. A high number of deaths or returns to the hospital related to care for a heart attack may mean the hospital is not treating people effectively for these conditions.'
set @TopicType = 'Hospital'

MERGE INTO [dbo].[TopicCategories] tc
USING(
	VALUES(@Name,@LongTitle,@Description,@TopicType)
	) Temp (Name, LongTitle, [Description], TopicType) on Upper(tc.Name) = Upper(Temp.Name) --and Upper(tc.LongTitle) = Upper(Temp.LongTitle) 
									--and Upper(tc.[Description]) = Upper(Temp.[Description]) and  Upper(tc.TopicType) = Upper(Temp.TopicType) 
WHEN NOT MATCHED THEN 
	INSERT (Name, LongTitle, [Description], TopicType)
	VALUES(Temp.Name, Temp.LongTitle, Temp.[Description], Temp.TopicType);

select @TopicCategoryId = tc.Id FROM [dbo].[TopicCategories] tc WHERE  tc.Name = @Name --and tc.LongTitle = @LongTitle and  tc.[Description] = @Description and  tc.TopicType = @TopicType
set @TopicName = 'Heart failure'
set @TopicLongTitle = 'Heart failure or congestive heart failure is a weakening of the heart''s pumping power that prevents the body from getting enough oxygen and nutrients to meet its needs. A high number of deaths or returns to the hospital related to care for a heart failure may mean the hospital is not treating people effectively for these conditions.'

IF @TopicCategoryId is not null
	BEGIN 
		MERGE INTO [dbo].[Topics] t
		USING(VALUES(@TopicName,@TopicLongTitle,@TopicCategoryId)) Temp (Name, LongTitle, TopicCategory_Id) 
		ON Upper(t.Name) = Upper(Temp.Name) and Upper(t.LongTitle) = Upper(Temp.LongTitle)  and t.TopicCategory_Id = Temp.TopicCategory_Id
		WHEN NOT MATCHED THEN 
			INSERT (Name, LongTitle,TopicCategory_Id)
			VALUES (Temp.Name, Temp.LongTitle, Temp.TopicCategory_Id);


		MERGE INTO [dbo].[Measures_MeasureTopics] mmt
		using( SELECT Id , (SELECT TOP 1 Id FROM Topics WHERE Name = @TopicName and TopicCategory_Id = @TopicCategoryId and LongTitle = @TopicLongTitle)  
			   FROM [dbo].[Measures] 
			   WHERE Name = 'READM-30-AMI') Temp (Measure_Id, Topic_Id) ON mmt.Measure_Id = Temp.Measure_Id and mmt.Topic_Id = Temp.Topic_Id 
		WHEN NOT MATCHED THEN 
			INSERT (Measure_Id, Topic_Id)
			VALUES(Temp.Measure_Id, Temp.Topic_Id);
	END 

set @Name = 'Heart failure'
set @LongTitle= 'Ratings about care for heart failure.  Heart failure or congestive heart failure is a weakening of the heart''s pumping power that prevents the body from getting enough oxygen and nutrients to meet its needs.'
Set @Description= 'Heart failure or congestive heart failure is a weakening of the heart''s pumping power that prevents the body from getting enough oxygen and nutrients to meet its needs. A high number of deaths or returns to the hospital related to care for a heart failure may mean the hospital is not treating people effectively for these conditions.'
set @TopicType = 'Hospital'

MERGE INTO [dbo].[TopicCategories] tc
USING(
	VALUES(@Name,@LongTitle,@Description,@TopicType)
	) Temp (Name, LongTitle, [Description], TopicType) on Upper(tc.Name) = Upper(Temp.Name)  --and Upper(tc.LongTitle) = Upper(Temp.LongTitle) 
									--and Upper(tc.[Description]) = Upper(Temp.[Description]) and  Upper(tc.TopicType) = Upper(Temp.TopicType) 
WHEN NOT MATCHED THEN 
	INSERT (Name, LongTitle, [Description], TopicType)
	VALUES(Temp.Name, Temp.LongTitle, Temp.[Description], Temp.TopicType);

select @TopicCategoryId = tc.Id FROM [dbo].[TopicCategories] tc WHERE  tc.Name = @Name --and tc.LongTitle = @LongTitle and  tc.[Description] = @Description and  tc.TopicType = @TopicType
set @TopicName = 'Results of care'
set @TopicLongTitle = 'Information on what happened to patients while being cared for in the hospital or after leaving the hospital. These ratings are sometimes called outcome measures.'

IF @TopicCategoryId is not null
	BEGIN 
		MERGE INTO [dbo].[Topics] t
		USING(VALUES(@TopicName,@TopicLongTitle,@TopicCategoryId)) Temp (Name, LongTitle, TopicCategory_Id) 
		ON Upper(t.Name) = Upper(Temp.Name) and Upper(t.LongTitle) = Upper(Temp.LongTitle)  and t.TopicCategory_Id = Temp.TopicCategory_Id
		WHEN NOT MATCHED THEN 
			INSERT (Name, LongTitle,TopicCategory_Id)
			VALUES (Temp.Name, Temp.LOngTitle, Temp.TopicCategory_Id);


		MERGE INTO [dbo].[Measures_MeasureTopics] mmt
		using( SELECT Id , (SELECT TOP 1 Id FROM Topics WHERE Name = @TopicName and TopicCategory_Id = @TopicCategoryId and LongTitle = @TopicLongTitle)  
			   FROM [dbo].[Measures] 
			   WHERE Name = 'READM-30-AMI') Temp (Measure_Id, Topic_Id) ON mmt.Measure_Id = Temp.Measure_Id and mmt.Topic_Id = Temp.Topic_Id 
		WHEN NOT MATCHED THEN 
			INSERT (Measure_Id, Topic_Id)
			VALUES(Temp.Measure_Id, Temp.Topic_Id);
	END 

UPDATE [dbo].[Topics]
SET LongTitle = 'These ratings show how satisfied patients say they are with the way hospital staff communicated with them. Good communication means that hospital staff explained things clearly, listened carefully, and treated patients with courtesy and respect. These ratings are collected from patient surveys.'
FROM [dbo].[Topics] t
	inner join [dbo].[Measures_MeasureTopics] mt on t.Id = mt.Topic_Id
	inner join [dbo].[Measures] m on m.Id = mt.Measure_Id
	inner join [dbo].[TopicCategories] tc on t.TopicCategory_id = tc.Id
WHERE t.Name = 'Communication' and tc.Name = 'Patient survey results'


UPDATE [dbo].[Topics]
SET LongTitle = 'These ratings show how satisfied patients say they are with the physical environment in the hospital. A good physical environment means that patients received help quickly, their pain was well-controlled, and the patient room was clean and quiet. This type of quality rating appears only in the ""Patient survey results"" health topic. These ratings are collected from patient surveys.'
FROM [dbo].[Topics] t
	inner join [dbo].[Measures_MeasureTopics] mt on t.Id = mt.Topic_Id
	inner join [dbo].[Measures] m on m.Id = mt.Measure_Id
	inner join [dbo].[TopicCategories] tc on t.TopicCategory_id = tc.Id
WHERE t.Name = 'Environment' and tc.Name = 'Patient survey results'
 
SELECT 1;

END TRY
BEGIN CATCH

END CATCH
/* Bug 4573 PC-01 Topics change */
;update Measures_MeasureTopics set Topic_Id = 9
where Measure_Id = 140  and Topic_Id = 21