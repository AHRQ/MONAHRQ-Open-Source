SET NOCOUNT ON;

-- ======================================================================
-- insert/update physicians
-- ======================================================================
DECLARE @Npi [bigint];
DECLARE @PacId [nvarchar](17);
DECLARE @ProfEnrollId [nvarchar](20);
DECLARE @FirstName [nvarchar](50);
DECLARE @MiddleName [nvarchar](50);
DECLARE @LastName [nvarchar](50);
DECLARE @Suffix [nvarchar](20);
DECLARE @Gender [nvarchar](10);
DECLARE @Credential [nvarchar](50);
DECLARE @MedicalSchoolName [nvarchar](255);
DECLARE @GraduationYear [int];
DECLARE @CouncilBoardCertification [bit];
DECLARE @PrimarySpecialty [nvarchar](255);
DECLARE @SecondarySpecialty1 [nvarchar](255);
DECLARE @SecondarySpecialty2 [nvarchar](255);
DECLARE @SecondarySpecialty3 [nvarchar](255);
DECLARE @SecondarySpecialty4 [nvarchar](255);
DECLARE @AcceptsMedicareAssignment [nvarchar](2);
DECLARE @ParticipatesInERX [bit];
DECLARE @ParticipatesInPQRS [bit];
DECLARE @ParticipatesInEHR [bit];
DECLARE @State [nvarchar](150);
DECLARE @IsEdited [bit];
DECLARE @Version [bigint];

DECLARE physicians_cursor CURSOR FOR 
SELECT DISTINCT [NPI],[PacId],[ProfEnrollId],[FirstName],[MiddleName],[LastName],[Suffix],(CASE [Gender] 
				WHEN 'M' THEN 'Male' 
				WHEN 'F' THEN 'Female'
				ELSE NULL
			END) AS [Gender],[Credential],[MedicalSchoolName],[GraduationYear],[CouncilBoardCertification],[PrimarySpecialty],
				[SecondarySpecialty1],[SecondarySpecialty2],[SecondarySpecialty3],[SecondarySpecialty4],[AcceptsMedicareAssignment],[ParticipatesInERX],[ParticipatesInPQRS],[ParticipatesInEHR],[State],ISNULL([IsEdited],0) 'IsEdited',[Version]
FROM	[dbo].[Targets_PhysicianTargets] 
WHERE	[State] = '[@@State@@]'
ORDER BY [LastName],[NPI];

OPEN physicians_cursor

FETCH NEXT FROM physicians_cursor INTO @Npi,@PacId,@ProfEnrollId,@FirstName,@MiddleName,@LastName,@Suffix,@Gender,@Credential,@MedicalSchoolName,@GraduationYear,@CouncilBoardCertification,@PrimarySpecialty,
									   @SecondarySpecialty1,@SecondarySpecialty2,@SecondarySpecialty3,@SecondarySpecialty4,@AcceptsMedicareAssignment,@ParticipatesInERX,@ParticipatesInPQRS,@ParticipatesInEHR,
									   @State,@IsEdited,@Version

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT TOP 1 p.[Npi] FROM [dbo].[Physicians] p WHERE p.[Npi] = @Npi /*AND p.[PacId] = @PacId AND p.[ProfEnrollId] = @ProfEnrollId*/)
		BEGIN
			INSERT INTO [dbo].[Physicians]([Npi],[PacId],[ProfEnrollId],[FirstName],[MiddleName],[LastName],[Suffix],[Gender],[Credentials],[MedicalSchoolName],[GraduationYear],[CouncilBoardCertification],[PrimarySpecialty],
							               [SecondarySpecialty1],[SecondarySpecialty2],[SecondarySpecialty3],[SecondarySpecialty4],[AcceptsMedicareAssignment],[ParticipatesInERX],[ParticipatesInPQRS],[ParticipatesInEHR],[States],[IsEdited],[Version])
            VALUES (@Npi,@PacId,@ProfEnrollId,@FirstName,@MiddleName,@LastName,@Suffix,@Gender,@Credential,@MedicalSchoolName,@GraduationYear,@CouncilBoardCertification,@PrimarySpecialty,
					@SecondarySpecialty1,@SecondarySpecialty2,@SecondarySpecialty3,@SecondarySpecialty4,@AcceptsMedicareAssignment,@ParticipatesInERX,@ParticipatesInPQRS,@ParticipatesInEHR,@State,@IsEdited,@Version);
		END
	ELSE
		BEGIN
			UPDATE [dbo].[Physicians]
			--SET [Npi]=@Npi,[PacId]=@PacId,[ProfEnrollId]=@ProfEnrollId
			SET  [FirstName] = @FirstName
				,[MiddleName] = @MiddleName
				,[LastName] = @LastName
				,[Suffix] = @Suffix
				,[Gender] = @Gender
				,[Credentials] = @Credential			
				,[MedicalSchoolName] = @MedicalSchoolName
				,[GraduationYear] = @GraduationYear
				,[CouncilBoardCertification] = @CouncilBoardCertification
				,[PrimarySpecialty] = @PrimarySpecialty
				,[SecondarySpecialty1] = @SecondarySpecialty1
				,[SecondarySpecialty2] = @SecondarySpecialty2
				,[SecondarySpecialty3] = @SecondarySpecialty3
				,[SecondarySpecialty4] = @SecondarySpecialty4
				,[AcceptsMedicareAssignment] = @AcceptsMedicareAssignment
				,[ParticipatesInERX] = @ParticipatesInERX
				,[ParticipatesInPQRS] = @ParticipatesInPQRS
				,[ParticipatesInEHR] = @ParticipatesInEHR
				,[States] = @State
				,[IsEdited] = @IsEdited
				,[Version] = @Version
			WHERE [Npi] = @Npi; /*AND [PacId] = @PacId AND [ProfEnrollId] = @ProfEnrollId;*/

		END

	-- Get the next physician.
    FETCH NEXT FROM physicians_cursor INTO @Npi,@PacId,@ProfEnrollId,@FirstName,@MiddleName,@LastName,@Suffix,@Gender,@Credential,@MedicalSchoolName,@GraduationYear,@CouncilBoardCertification,@PrimarySpecialty,
									       @SecondarySpecialty1,@SecondarySpecialty2,@SecondarySpecialty3,@SecondarySpecialty4,@AcceptsMedicareAssignment,@ParticipatesInERX,@ParticipatesInPQRS,@ParticipatesInEHR,
									       @State,@IsEdited,@Version
END 
CLOSE physicians_cursor;
DEALLOCATE physicians_cursor;

-- =====================================================================
-- insert/update physician addresses
-- =====================================================================
DECLARE @PhysicianId INT;
DECLARE @Npi2 [bigint];
DECLARE @PacId2 [nvarchar](17);
DECLARE @ProfEnrollId2 [nvarchar](20);
DECLARE @Line1 [nvarchar](255);
DECLARE @Line2 [nvarchar](255);
DECLARE @MarkerofAdressLine2Suppression bit;
DECLARE @City [nvarchar](150);
DECLARE @State2 [nvarchar](150);
DECLARE @ZipCode [nvarchar](12);
DECLARE @Version2 [bigint];
DECLARE @AddressIndex [int] = 0;
DECLARE @OldPhysicianId [int] = null;

DECLARE physicianaddresses_cursor CURSOR FOR 
SELECT DISTINCT P.[Id] 'PhysicianId', TPT.[Npi],TPT.[PacId],TPT.[ProfEnrollId],TPT.[Line1],TPT.[Line2],TPT.[MarkerofAdressLine2Suppression],TPT.[City],TPT.[State],
				(case len(TPT.[ZipCode])
					WHEN 4 THEN '0' + TPT.[ZipCode]					
					WHEN 8 THEN '0' + TPT.[ZipCode]
					ELSE TPT.[ZipCode]
			     END) 'ZipCode',
				 TPT.[Version]
FROM [dbo].[Targets_PhysicianTargets] TPT
	 INNER JOIN [dbo].[Physicians] P ON P.[Npi] = TPT.[Npi]
WHERE TPT.[GroupPracticePacId] IS NULL AND TPT.[State] = '[@@State@@]'
ORDER BY TPT.[Npi],TPT.[Line1];

OPEN physicianaddresses_cursor

FETCH NEXT FROM physicianaddresses_cursor INTO @PhysicianId,@Npi2,@PacId2,@ProfEnrollId2,@Line1,@Line2,@MarkerofAdressLine2Suppression,@City,@State2,@ZipCode,@Version2

WHILE @@FETCH_STATUS = 0
BEGIN

	IF NOT EXISTS (SELECT TOP 1 a.[Id] FROM [dbo].[Addresses] a 
				   WHERE UPPER(a.[AddressType]) = 'PHYSICIAN' AND a.[Physician_Id] = @PhysicianId 
				     AND lower(ltrim(rtrim(a.[Line1]))) = lower(ltrim(rtrim(@Line1)))  
					 AND lower(ltrim(rtrim(ISNULL(a.[Line2],'-1')))) = lower(ltrim(rtrim(ISNULL(@Line2,'-1')))) 
					 AND lower(ltrim(rtrim(a.[City]))) = lower(ltrim(rtrim(@City)))
					 AND lower(ltrim(rtrim(a.[State]))) = lower(ltrim(rtrim(@State2)))
					 AND lower(ltrim(rtrim(Left(a.[ZipCode],5)))) = lower(ltrim(rtrim(Left(@ZipCode,5)))))
	BEGIN
		-- PRINT 'INSIDE INSERT';
		INSERT INTO [dbo].[Addresses]([AddressType],[Line1],[Line2],[Line3],[City],[State],[ZipCode],[Index],[Version],[MedicalPractice_Id],[Physician_Id])
		VALUES ('Physician',
				@Line1,
				@Line2,
				null,
				@City,
				@State2,
				(case len(@ZipCode)
					WHEN 4 THEN '0' + @ZipCode					
					WHEN 8 THEN '0' + @ZipCode
					ELSE @ZipCode
			    END),
				ISNULL((SELECT (MAX(ISNULL(a.[Index],0)) + 1) FROM [dbo].[Addresses] a WHERE UPPER(a.[AddressType])='PHYSICIAN' AND a.[State]='[@@State@@]' AND a.[Physician_Id]=@PhysicianId),0),
				@Version2,
				null,
				@PhysicianId);		
	END
	ELSE
	BEGIN
		-- PRINT 'INSIDE UPDATE';
		UPDATE [dbo].[Addresses]
		SET [Line1] = @Line1,
			[Line2] = @Line2,
			[City] = @City,
			--[State] = ISNULL([State],@State2),
			[ZipCode] = (case len(@ZipCode)
					WHEN 4 THEN '0' + @ZipCode					
					WHEN 8 THEN '0' + @ZipCode
					ELSE @ZipCode
			    END),
			[Version] = @Version2
		WHERE UPPER([AddressType]) = 'PHYSICIAN' AND [Physician_Id] = @PhysicianId AND [Id]= (SELECT TOP 1 a.[Id] FROM [dbo].[Addresses] a 
											WHERE UPPER(a.[AddressType]) = 'PHYSICIAN' AND a.[Physician_Id] = @PhysicianId 
											  AND lower(ltrim(rtrim(a.[Line1]))) = lower(ltrim(rtrim(@Line1))) 
											  AND lower(ltrim(rtrim(ISNULL(a.[Line2],'-1')))) = lower(ltrim(rtrim(ISNULL(@Line2,'-1')))) 
											  AND lower(ltrim(rtrim(a.[City]))) = lower(ltrim(rtrim(@City)))
											  AND lower(ltrim(rtrim(a.[State]))) = lower(ltrim(rtrim(@State2)))
											  AND lower(ltrim(rtrim(Left(a.[ZipCode],5)))) = lower(ltrim(rtrim(Left(@ZipCode,5)))));
	END

	FETCH NEXT FROM physicianaddresses_cursor INTO @PhysicianId,@Npi2,@PacId2,@ProfEnrollId2,@Line1,@Line2,@MarkerofAdressLine2Suppression,@City,@State2,@ZipCode,@Version2
END 
CLOSE physicianaddresses_cursor;
DEALLOCATE physicianaddresses_cursor;
-- =====================================================================
-- insert/update physicians and hosital affiliations
-- =====================================================================
DECLARE @PhysicianId2 INT;
DECLARE @Npi3 [bigint];
DECLARE @PacId3 [nvarchar](17);
DECLARE @ProfEnrollId3 [nvarchar](20);
DECLARE @HospitalAffiliationCCN1 [nvarchar](10);
DECLARE @HospitalAffiliationCCN2 [nvarchar](10);
DECLARE @HospitalAffiliationCCN3 [nvarchar](10);
DECLARE @HospitalAffiliationCCN4 [nvarchar](10);
DECLARE @HospitalAffiliationCCN5 [nvarchar](10);
DECLARE @State3 [nvarchar](3);
DECLARE physicianhospAffil_cursor CURSOR FOR 
SELECT DISTINCT P.[Id] 'PhysicianId',TP.[Npi],TP.[PacId],TP.[ProfEnrollId],ISNULL(TP.[HospitalAffiliationCCN1], '-1') 'HospitalAffiliationCCN1',ISNULL(TP.[HospitalAffiliationCCN2], '-1') 'HospitalAffiliationCCN2',
	                ISNULL(TP.[HospitalAffiliationCCN3], '-1') 'HospitalAffiliationCCN3',ISNULL(TP.[HospitalAffiliationCCN4], '-1') 'HospitalAffiliationCCN4',ISNULL(TP.[HospitalAffiliationCCN5], '-1') 'HospitalAffiliationCCN5',TP.[State]
FROM [dbo].[Targets_PhysicianTargets] TP 
	 INNER JOIN [dbo].[Physicians] P ON P.[Npi] = TP.[Npi] /*AND P.[PacId] = TP.[PacId] AND P.[ProfEnrollId] = TP.[ProfEnrollId]*/
WHERE TP.[State] = '[@@State@@]'
ORDER BY TP.[Npi];
OPEN physicianhospAffil_cursor
FETCH NEXT FROM physicianhospAffil_cursor INTO @PhysicianId2,@Npi3,@PacId3,@ProfEnrollId3,@HospitalAffiliationCCN1,@HospitalAffiliationCCN2,@HospitalAffiliationCCN3,@HospitalAffiliationCCN4,@HospitalAffiliationCCN5,@State3
WHILE @@FETCH_STATUS = 0
BEGIN
    -- affiliation 1
	IF (@HospitalAffiliationCCN1 <> '-1')
	BEGIN
		--IF NOT EXISTS (SELECT TOP 1 PAH.[Physician_Id] FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
		--			   WHERE UPPER(RTRIM(LTRIM(PAH.[Hospital_CmsProviderId]))) = @HospitalAffiliationCCN1 AND PAH.[Physician_Id] = @PhysicianId2)
		IF ((SELECT COUNT(PAH.[Hospital_CmsProviderId]) FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
			 WHERE UPPER(RTRIM(LTRIM(PAH.[Hospital_CmsProviderId])))=UPPER(RTRIM(LTRIM(@HospitalAffiliationCCN1))) AND PAH.[Physician_Id] = @PhysicianId2) = 0)
		BEGIN
			INSERT INTO [dbo].[Physicians_AffiliatedHospitals]([Hospital_CmsProviderId],[Physician_Id],[Index])
			VALUES (ltrim(rtrim(@HospitalAffiliationCCN1)),
					@PhysicianId2,
					ISNULL((SELECT (MAX(ISNULL(pa.[Index],0)) + 1) FROM [dbo].[Physicians_AffiliatedHospitals] pa WHERE pa.[Physician_Id]=@PhysicianId2),0));
		END
	END
	-- affiliation 2
	IF (@HospitalAffiliationCCN2 <> '-1')
		BEGIN
		IF ((SELECT COUNT(PAH.[Hospital_CmsProviderId]) FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
					   WHERE UPPER(RTRIM(LTRIM(PAH.[Hospital_CmsProviderId]))) = UPPER(RTRIM(LTRIM(@HospitalAffiliationCCN2))) AND PAH.[Physician_Id] = @PhysicianId2) = 0)
		BEGIN
			INSERT INTO [dbo].[Physicians_AffiliatedHospitals]([Hospital_CmsProviderId],[Physician_Id],[Index])
			VALUES (ltrim(rtrim(@HospitalAffiliationCCN2)),
					@PhysicianId2,
					ISNULL((SELECT (MAX(ISNULL(pa.[Index],0)) + 1) FROM [dbo].[Physicians_AffiliatedHospitals] pa WHERE pa.[Physician_Id]=@PhysicianId2),0));
		END
	END
	-- affiliation 3
	IF (@HospitalAffiliationCCN3 <> '-1')
		BEGIN
		--IF NOT EXISTS (SELECT TOP 1 PAH.[Physician_Id] FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
		--			   WHERE PAH.[Hospital_CmsProviderId] = @HospitalAffiliationCCN3 AND PAH.[Physician_Id] = @PhysicianId2)
		IF ((SELECT COUNT(PAH.[Hospital_CmsProviderId]) FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
					   WHERE UPPER(RTRIM(LTRIM(PAH.[Hospital_CmsProviderId])))=UPPER(RTRIM(LTRIM(@HospitalAffiliationCCN3))) AND PAH.[Physician_Id] = @PhysicianId2) = 0)
		BEGIN
			INSERT INTO [dbo].[Physicians_AffiliatedHospitals]([Hospital_CmsProviderId],[Physician_Id],[Index])
			VALUES (ltrim(rtrim(@HospitalAffiliationCCN3)),
					@PhysicianId2,
					ISNULL((SELECT (MAX(ISNULL(pa.[Index],0)) + 1) FROM [dbo].[Physicians_AffiliatedHospitals] pa WHERE pa.[Physician_Id]=@PhysicianId2),0));
		END
	END
	-- affiliation 4
	IF (@HospitalAffiliationCCN4 <> '-1')
		BEGIN
		--IF NOT EXISTS (SELECT TOP 1 PAH.[Physician_Id] FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
		--			   WHERE PAH.[Hospital_CmsProviderId] = @HospitalAffiliationCCN4 AND PAH.[Physician_Id] = @PhysicianId2)
		IF ((SELECT COUNT(PAH.[Hospital_CmsProviderId]) FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
			 WHERE UPPER(RTRIM(LTRIM(PAH.[Hospital_CmsProviderId])))=UPPER(RTRIM(LTRIM(@HospitalAffiliationCCN4))) AND PAH.[Physician_Id] = @PhysicianId2) = 0)
		BEGIN
			INSERT INTO [dbo].[Physicians_AffiliatedHospitals]([Hospital_CmsProviderId],[Physician_Id],[Index])
			VALUES (ltrim(rtrim(@HospitalAffiliationCCN4)),
					@PhysicianId2,
					ISNULL((SELECT (MAX(ISNULL(pa.[Index],0)) + 1) FROM [dbo].[Physicians_AffiliatedHospitals] pa WHERE pa.[Physician_Id]=@PhysicianId2),0));
		END
	END
	-- affiliation 5
	IF (@HospitalAffiliationCCN5 <> '-1')
		BEGIN
		--IF NOT EXISTS (SELECT TOP 1 PAH.[Physician_Id] FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
		--			   WHERE PAH.[Hospital_CmsProviderId] = @HospitalAffiliationCCN5 AND PAH.[Physician_Id] = @PhysicianId2)
		IF ((SELECT COUNT(PAH.[Hospital_CmsProviderId]) FROM [dbo].[Physicians_AffiliatedHospitals] PAH 
			 WHERE UPPER(RTRIM(LTRIM(PAH.[Hospital_CmsProviderId])))=UPPER(RTRIM(LTRIM(@HospitalAffiliationCCN5))) AND PAH.[Physician_Id] = @PhysicianId2) = 0)
		BEGIN
			INSERT INTO [dbo].[Physicians_AffiliatedHospitals]([Hospital_CmsProviderId],[Physician_Id],[Index])
			VALUES (ltrim(rtrim(@HospitalAffiliationCCN5)),
					@PhysicianId2,
					ISNULL((SELECT (MAX(ISNULL(pa.[Index],0)) + 1) FROM [dbo].[Physicians_AffiliatedHospitals] pa WHERE pa.[Physician_Id]=@PhysicianId2),0));
		END
	END
	FETCH NEXT FROM physicianhospAffil_cursor INTO @PhysicianId2,@Npi3,@PacId3,@ProfEnrollId3,@HospitalAffiliationCCN1,@HospitalAffiliationCCN2,@HospitalAffiliationCCN3,@HospitalAffiliationCCN4,@HospitalAffiliationCCN5,@State3
END 
CLOSE physicianhospAffil_cursor;
DEALLOCATE physicianhospAffil_cursor;