-- ======================================================================
-- update medical practice address (TODO: only if changed) 
-- ======================================================================
DECLARE @GroupPracticePacId5 [nvarchar](15);
DECLARE @GroupName5 [nvarchar](150);
DECLARE @DBAName5 [nvarchar](150);
DECLARE @NumberofGroupPracticeMembers5 [int];
DECLARE @State5 [nvarchar](150);
DECLARE @IsEdited5 [bit];
DECLARE @Version5 [bigint];

DECLARE medPractices_cursor CURSOR FOR 
SELECT DISTINCT [GroupPracticePacId],[OrgLegalName],[DBAName],[NumberofGroupPracticeMembers],[State],ISNULL([IsEdited],0) 'IsEdited',[Version]
FROM	[dbo].[Targets_PhysicianTargets] 
WHERE	ISNULL([GroupPracticePacId],'') <> '' AND ISNULL([OrgLegalName],'') <> '' AND [State] = '[@@State@@]'
ORDER BY [GroupPracticePacId],[OrgLegalName];

OPEN medPractices_cursor

FETCH NEXT FROM medPractices_cursor INTO @GroupPracticePacId5,@GroupName5,@DBAName5,@NumberofGroupPracticeMembers5,@State5,@IsEdited5,@Version5
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT TOP 1 mp.[GroupPracticePacId] FROM [dbo].[MedicalPractices] mp WHERE mp.[GroupPracticePacId] = @GroupPracticePacId5 /*AND mp.[State] = @State5*/)
		BEGIN
			INSERT INTO [dbo].[MedicalPractices]([Name],[GroupPracticePacId],[DBAName],[NumberofGroupPracticeMembers],[State],[IsEdited],[Version])
            VALUES (@GroupName5,@GroupPracticePacId5,@DBAName5,@NumberofGroupPracticeMembers5,@State5,@IsEdited5,@Version5);
		END
	ELSE
		BEGIN
			UPDATE [dbo].[MedicalPractices]
			SET  [Name] = @GroupName5
				,[DBAName] = @DBAName5-- might concat
				,[NumberofGroupPracticeMembers] = @NumberofGroupPracticeMembers5
				--,[State] = (CASE
				--			[State]
				--				)
				,[IsEdited] = @IsEdited5
				,[Version] = @Version5
			WHERE [GroupPracticePacId] = @GroupPracticePacId5
			  --AND [State] = @State5;
		END
    -- Get the next medical practice.
    FETCH NEXT FROM medPractices_cursor INTO @GroupPracticePacId5,@GroupName5,@DBAName5,@NumberofGroupPracticeMembers5,@State5,@IsEdited5,@Version5
END 
CLOSE medPractices_cursor;
DEALLOCATE medPractices_cursor;
-- =====================================================================
-- insert/update MedicalPractice addresses
-- =====================================================================
DECLARE @MedicalPracticeId6 INT;
DECLARE @GroupPracticePacId6 [nvarchar](17);
DECLARE @Line16 [nvarchar](255);
DECLARE @Line26 [nvarchar](255);
DECLARE @MarkerofAdressLine2Suppression6 bit;
DECLARE @City6 [nvarchar](150);
DECLARE @State6 [nvarchar](150);
DECLARE @ZipCode6 [nvarchar](12);
DECLARE @Version6 [bigint];
DECLARE @AddressIndex2 [int];
DECLARE @OldGroupPracticeId6 [int];

DECLARE medPract_addresses_cursor CURSOR FOR 
SELECT DISTINCT MP.[Id] 'MedicalPracticeId', TPT.[GroupPracticePacId],TPT.[Line1],TPT.[Line2],TPT.[MarkerofAdressLine2Suppression],TPT.[City],TPT.[State],
				(case len(TPT.[ZipCode])
					WHEN 4 THEN '0' + TPT.[ZipCode]					
					WHEN 8 THEN '0' + TPT.[ZipCode]
					ELSE TPT.[ZipCode]
			     END) 'ZipCode',
				 TPT.[Version]
FROM [dbo].[Targets_PhysicianTargets] TPT
	 INNER JOIN [MedicalPractices] MP ON MP.[GroupPracticePacId] = TPT.[GroupPracticePacId]
WHERE TPT.[GroupPracticePacId] IS NOT NULL AND ISNULL([OrgLegalName],'') <> '' AND TPT.[State] = '[@@State@@]'
ORDER BY MP.[Id],TPT.[GroupPracticePacId],TPT.[Line1];

OPEN medPract_addresses_cursor

FETCH NEXT FROM medPract_addresses_cursor INTO @MedicalPracticeId6,@GroupPracticePacId6,@Line16,@Line26,@MarkerofAdressLine2Suppression6,@City6,@State6,@ZipCode6,@Version6

WHILE @@FETCH_STATUS = 0
BEGIN

	--SELECT DISTINCT @AddressId2 = a.[Id] FROM [dbo].[Addresses] a 
	--									WHERE UPPER(a.[AddressType]) = 'MEDICALPRACTICE' AND a.[MedicalPractice_Id] = @MedicalPracticeId6 
	--										 AND lower(ltrim(rtrim(a.[Line1]))) = lower(ltrim(rtrim(@Line16))) 
	--										 AND lower(ltrim(rtrim(ISNULL(a.[Line2],'-1')))) = lower(ltrim(rtrim(ISNULL(@Line26,'-1')))) 
	--										 AND lower(ltrim(rtrim(a.[City]))) = lower(ltrim(rtrim(@City6)))
	--										 AND lower(ltrim(rtrim(a.[State]))) = lower(ltrim(rtrim(@State6)))
	--										 AND lower(ltrim(rtrim(a.[ZipCode]))) = lower(ltrim(rtrim(@ZipCode6)))


	IF NOT EXISTS (SELECT TOP 1 a.[Id] FROM [dbo].[Addresses] a 
				   WHERE UPPER(a.[AddressType]) = 'MEDICALPRACTICE' 
				     AND a.[MedicalPractice_Id] = @MedicalPracticeId6 
				     AND lower(ltrim(rtrim(a.[Line1]))) = lower(ltrim(rtrim(@Line16))) 
					 AND lower(ltrim(rtrim(ISNULL(a.[Line2],'-1')))) = lower(ltrim(rtrim(ISNULL(@Line26,'-1')))) 
					 AND lower(ltrim(rtrim(a.[City]))) = lower(ltrim(rtrim(@City6)))
					 AND lower(ltrim(rtrim(a.[State]))) = lower(ltrim(rtrim(@State6)))
					 AND lower(ltrim(rtrim(Left(a.[ZipCode],5)))) = lower(ltrim(rtrim(Left(@ZipCode6,5)))))
	BEGIN
		INSERT INTO [dbo].[Addresses]([AddressType],[Line1],[Line2],[Line3],[City],[State],[ZipCode],[Index],[Version],[MedicalPractice_Id],[Physician_Id])
		VALUES ('MedicalPractice',
				@Line16,
				@Line26,
				null,
				@City6,
				@State6,
				(CASE LEN(@ZipCode6)
					WHEN 4 THEN '0' + @ZipCode6					
					WHEN 8 THEN '0' + @ZipCode6
					ELSE @ZipCode6
			     END),
				 ISNULL((SELECT (MAX(ISNULL(a.[Index],-1)) + 1) FROM [dbo].[Addresses] a WHERE UPPER(a.[AddressType])='MEDICALPRACTICE' AND a.[State]='[@@State@@]'  AND a.[MedicalPractice_Id]=@MedicalPracticeId6),0),
				 @Version6,
				 @MedicalPracticeId6,
				 null);		
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Addresses]
		SET [Line1] = @Line16,
			[Line2] = @Line26,
			[City] = @City6,
			--[State] = ISNULL([State],@State6),
			[ZipCode] = (case len(@ZipCode6)
					WHEN 4 THEN '0' + @ZipCode6					
					WHEN 8 THEN '0' + @ZipCode6
					ELSE @ZipCode6
			     END),
			[Version] = @Version6
		WHERE UPPER([AddressType]) = 'MEDICALPRACTICE' AND [MedicalPractice_Id] = @MedicalPracticeId6 AND [Id] = (SELECT TOP 1 a.[Id] FROM [dbo].[Addresses] a 
										WHERE UPPER(a.[AddressType]) = 'MEDICALPRACTICE' AND a.[MedicalPractice_Id] = @MedicalPracticeId6 
											 AND lower(ltrim(rtrim(a.[Line1]))) = lower(ltrim(rtrim(@Line16))) 
											 AND lower(ltrim(rtrim(ISNULL(a.[Line2],'-1')))) = lower(ltrim(rtrim(ISNULL(@Line26,'-1')))) 
											 AND lower(ltrim(rtrim(a.[City]))) = lower(ltrim(rtrim(@City6)))
											 AND lower(ltrim(rtrim(a.[State]))) = lower(ltrim(rtrim(@State6)))
											 AND lower(ltrim(rtrim(left(a.[ZipCode],5)))) = lower(ltrim(rtrim(Left(@ZipCode6,5)))))
	END

	FETCH NEXT FROM medPract_addresses_cursor INTO @MedicalPracticeId6,@GroupPracticePacId6,@Line16,@Line26,@MarkerofAdressLine2Suppression6,@City6,@State6,@ZipCode6,@Version6
END 
CLOSE medPract_addresses_cursor;
DEALLOCATE medPract_addresses_cursor;