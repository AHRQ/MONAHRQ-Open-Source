 
 declare @sql as nvarchar(max);
 IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Hospitals_CustomRegion_ImportRegionId')
	begin 
		set @sql =  'CREATE UNIQUE NONCLUSTERED INDEX IDX_Hospitals_CustomRegion_ImportRegionId
			ON Regions(RegionType,ImportRegionId,State)
			WHERE ImportRegionId IS NOT NULL';
		exec(@sql);
	end

IF NOT EXISTS ( SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_GetStateIdFromZipCode') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	set @sql = 'CREATE PROCEDURE [dbo].[sp_GetStateIdFromZipCode]
					@zipcode nvarchar(20)
				AS
				BEGIN
					SET NOCOUNT ON;

					with RegionsCTE(Id, [Type], Name, Importregion_Id, State) as
					(
						SELECT distinct [Id]
						  ,[RegionType] ''Type''
						  ,[Name]
						  ,[ImportRegionId] ''Importregion_Id''
						  ,[State]
						FROM [dbo].[Regions]
					)
					SELECT distinct r.[State]
					FROM [dbo].[Base_ZipCodes] z
						left outer join RegionsCTE  r 
							on r.Importregion_Id = z.[HRRNumber] or r.Importregion_Id = z.[HSANumber] 
					WHERE z.Zip = @zipcode

					SET NOCOUNT OFF;
				END';
	exec(@sql);
END

-- ***********************************************************************************************************
-- Non clustered indexes needed around physician and medical practice related tables
-- ***********************************************************************************************************

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Addresses_MedicalPractice_1')
	begin 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Addresses_MedicalPractice_1]
ON [dbo].[Addresses] ([MedicalPractice_Id])
INCLUDE ([City])';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_2')
	begin 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_2]
ON [dbo].[Targets_PhysicianTargets] ([State],[GroupPracticePacId])
INCLUDE ([Npi])';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_3')
BEGIN 
		set @sql = 'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_3]
ON [dbo].[Targets_PhysicianTargets] ([GroupPracticePacId],[State])
INCLUDE ([Npi],[PacId],[ProfEnrollId],[Line1],[Line2],[MarkerofAdressLine2Suppression],[City],[ZipCode],[Version]);';
	exec(@sql);
END

--IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_4')
--BEGIN 
--		set @sql = 'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_4]
--ON [dbo].[Targets_PhysicianTargets] ([State])
--INCLUDE ([GroupPracticePacId],[OrgLegalName],[DBAName],[NumberofGroupPracticeMembers],[Version]);';
--	exec(@sql);
--END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_4')
BEGIN 
		set @sql = 'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_4]
ON [dbo].[Targets_PhysicianTargets] ([State])
INCLUDE ([GroupPracticePacId],[OrgLegalName],[DBAName],[NumberofGroupPracticeMembers],[IsEdited],[Version])';
	exec(@sql);
END

--IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Physicians_MedicalPractices_2')
--BEGIN 
--		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Physicians_MedicalPractices_2]
--ON [dbo].[Physicians_MedicalPractices] ([MedicalPractice_Id])
--INCLUDE ([Physician_Id])';
--	exec(@sql);
--END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Physicians_HospitalAffiliations_2')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Physicians_HospitalAffiliations_2]
ON [dbo].[Physicians_AffiliatedHospitals] ([Physician_Id])
INCLUDE ([Hospital_CmsProviderId])';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_AddressesMedicalPractice')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_AddressesMedicalPractice]
ON [dbo].[Addresses] ([State])
INCLUDE ([MedicalPractice_Id])';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_PhysicianAddress')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_PhysicianAddress]
ON [dbo].[Addresses] ([State])';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_PhysicianAddress2')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_PhysicianAddress2]
ON [dbo].[Addresses] ([Physician_Id])';
	exec(@sql);
END

--IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_PMP_AssociatedAddresses')
--BEGIN 
--		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_PMP_AssociatedAddresses]
--ON [dbo].[Physicians_MedicalPractices] ([Physician_Id],[MedicalPractice_Id])
--INCLUDE ([AssociatedPMPAddresses])';
--	exec(@sql);
--END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Measures')
BEGIN 
	set @sql = 'UPDATE Measures 
			SET MeasureType = ''Quality Measures''
			WHERE Name = ''NH-QM-01''';
	exec(@sql);
END


IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_InpatientTargets_Dataset')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX IDX_Targets_InpatientTargets_Dataset
ON [dbo].[Targets_InpatientTargets] ([Dataset_Id],[PrincipalDiagnosis])
INCLUDE ([LocalHospitalID],[TotalCharge],[PrincipalProcedure])';
	exec(@sql);
END

--/*
-- * Drop all Target table foreign keys 
-- */
--DECLARE @ForeignKeyName NVARCHAR(MAX), @TableName NVARCHAR(MAX), @stmt NVARCHAR(MAX)

--DECLARE TablesWithForeignKeys CURSOR  FOR 
--		SELECT object_name(parent_object_id) AS TableName, NAME, 'ALTER TABLE '+object_name(parent_object_id)+' DROP CONSTRAINT '+NAME AS [SQL]
--		FROM sys.foreign_keys
--		WHERE referenced_object_id = OBJECT_ID(N'Wings_Datasets')
--		AND object_name(parent_object_id)  like 'Targets_%'

--OPEN TablesWithForeignKeys

--FETCH NEXT FROM  TablesWithForeignKeys 
--	  INTO @TableName, @ForeignKeyName , @stmt

--WHILE @@fetch_status = 0
--	BEGIN 
--		PRINT @sql
--		EXEC sp_executesql @sql
--		FETCH NEXT FROM  TablesWithForeignKeys INTO @TableName, @ForeignKeyName, @stmt
--	END 	
		
IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_Build2a')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_Build2a] ON [dbo].[Targets_PhysicianTargets] 
(
	[GroupPracticePacId] ASC,
	[Npi] ASC,
	[Id] ASC
)';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_Build2b')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_Build2b] ON [dbo].[Targets_PhysicianTargets] 
(
	[GroupPracticePacId] ASC
)
INCLUDE ( [OrgLegalName],
[DBAName],
[NumberofGroupPracticeMembers],
[State],
[IsEdited],
[Version]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_Build2c')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_Build2c] ON [dbo].[Targets_PhysicianTargets] 
(
	[GroupPracticePacId] ASC,
	[Npi] ASC
)
INCLUDE ( [ZipCode],
[PacId],
[ProfEnrollId],
[Line1],
[Line2],
[MarkerofAdressLine2Suppression],
[City],
[State],
[Version]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_Build2d')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_Build2d] ON [dbo].[Targets_PhysicianTargets] 
(
	[State] ASC
)
INCLUDE ( [Npi],
[PacId],
[ProfEnrollId],
[FirstName],
[MiddleName],
[LastName],
[Suffix],
[Gender],
[Credential],
[MedicalSchoolName],
[GraduationYear],
[CouncilBoardCertification],
[PrimarySpecialty],
[SecondarySpecialty1],
[SecondarySpecialty2],
[SecondarySpecialty3],
[SecondarySpecialty4],
[AcceptsMedicareAssignment],
[ParticipatesInERX],
[ParticipatesInPQRS],
[ParticipatesInEHR],
[IsEdited],
[Version]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_Build2e')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_Build2e] ON [dbo].[Targets_PhysicianTargets] 
(
	[GroupPracticePacId] ASC,
	[Npi] ASC,
	[Id] ASC
)
INCLUDE ( [ZipCode],
[OrgLegalName],
[DBAName],
[NumberofGroupPracticeMembers],
[Line1],
[Line2],
[City],
[State],
[IsEdited],
[Version]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_MedicalPractices_Build2a')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_MedicalPractices_Build2a] ON [dbo].[MedicalPractices] 
(
	[State] ASC
)
INCLUDE ( [Id],
[GroupPracticePacId]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Physicians_MedicalPractices_Build2a')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Physicians_MedicalPractices_Build2a] ON [dbo].[Physicians_MedicalPractices] 
(
	[Physician_Id] ASC,
	[MedicalPractice_Id] ASC,
	[Id] ASC
)
INCLUDE ( [AssociatedPMPAddresses]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_Build2e')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_Build2e] ON [dbo].[Targets_PhysicianTargets] 
(
	[GroupPracticePacId] ASC,
	[Npi] ASC
)WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_PhysicianTargets_Build2f')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_PhysicianTargets_Build2f] ON [dbo].[Targets_PhysicianTargets] 
(
	[State] ASC,
	[Npi] ASC
)
INCLUDE ( [PacId],
[ProfEnrollId],
[HospitalAffiliationCCN1],
[HospitalAffiliationCCN2],
[HospitalAffiliationCCN3],
[HospitalAffiliationCCN4],
[HospitalAffiliationCCN5]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Physicians_Build2a')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Physicians_Build2a] ON [dbo].[Physicians] 
(
	[States] ASC
)
INCLUDE ( [Id],
[Npi]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Addresses_Build2a')
BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Addresses_Build2a] ON [dbo].[Addresses] 
(
	[AddressType] ASC,
	[Line1] ASC,
	[State] ASC,
	[Id] ASC,
	[MedicalPractice_Id] ASC
)
INCLUDE ( [Line2],
[City],
[ZipCode]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]';
	exec(@sql);
END

-- Drop if found. this was a mysterious index in a test database that was causing saving issues with website reports.
IF EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'NonClusteredIndex-20160419-124110')
BEGIN 
	set @sql = 'DROP INDEX [NonClusteredIndex-20160419-124110] ON [dbo].[Websites_WebsiteReports]';
	exec(@sql);
END


IF ((SELECT DISTINCT [Version] FROM [dbo].[SchemaVersions] WHERE UPPER([Name]) = 'DATABASE SCHEMA') = '7.0.0')
BEGIN
	IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_InpatientTargets_DischargeQuarter')
	BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_InpatientTargets_DischargeQuarter] ON [dbo].[Targets_InpatientTargets] ([Dataset_Id],[DischargeQuarter])';
		exec(@sql);
	END

	ALTER INDEX [IDX_Targets_InpatientTargets_DischargeQuarter] ON [dbo].[Targets_InpatientTargets] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON);

	IF NOT EXISTS(SELECT i.* FROM SYS.INDEXES AS i WITH (NOLOCK) WHERE i.[name] = 'IDX_Targets_TreatAndReleaseTargets_DischargeQuarter')
	BEGIN 
		set @sql =  'CREATE NONCLUSTERED INDEX [IDX_Targets_TreatAndReleaseTargets_DischargeQuarter] ON [dbo].[Targets_TreatAndReleaseTargets] ([Dataset_Id],[DischargeQuarter])';
		exec(@sql);
	END

	ALTER INDEX [IDX_Targets_TreatAndReleaseTargets_DischargeQuarter] ON [dbo].[Targets_TreatAndReleaseTargets] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON);
END