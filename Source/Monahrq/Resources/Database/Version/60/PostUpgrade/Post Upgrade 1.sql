BEGIN TRY	

	/*
	*	Merge Categories from pervious temp table
	*/
	DECLARE @MaxCategoryId INT;
	SELECT @MaxCategoryId =	MAX(CategoryId) FROM Categories;

	MERGE INTO dbo.Categories c
	USING ( SELECT *
		    FROM Hospitals_HospitalCategoriesTemp t
			) AS Temp ON Temp.CategoryID = c.CategoryID  
	WHEN MATCHED THEN	
		UPDATE SET c.Name = Temp.Name
	WHEN NOT MATCHED THEN 
		INSERT (CategoryType, Name, CategoryID,Version,IsSourcedFromBaseData,Registry_Id)
		VALUES('HospitalCategory',Temp.Name, @MaxCategoryId + 1,Temp.Version ,Temp.IsSourcedFromBaseData,Temp.Registry_Id);


	/*
	*	Merge Regions from Hospitals_Regions table
	*/
	MERGE INTO [dbo].[Regions] r
	USING ( 
			SELECT RegionType,Name, IsSourcedFromBaseData, Version, ImportRegionId, Abbreviation, HospitalRegistry_id, City 
			FROM RegionTemp
		  ) hrs ON r.RegionType = hrs.RegionType
	AND r.Name = hrs.Name AND r.ImportRegionId = hrs.ImportRegionId 
	AND r.City = hrs.City AND r.State = hrs.Abbreviation
	WHEN MATCHED THEN 
		UPDATE SET r.Name = hrs.Name, 
		r.RegionType = hrs.RegionType, 
		r.IsSourcedFromBaseData = hrs.IsSourcedFromBaseData, 
		r.HospitalRegistry_id = hrs.HospitalRegistry_id
	WHEN NOT MATCHED THEN 
		INSERT (RegionType, Name, IsSourcedFromBaseData, Version, Code, ImportRegionId, State, HospitalRegistry_id, City)
		VALUES(hrs.RegionType, hrs.Name, hrs.IsSourcedFromBaseData, hrs.Version, 
							CASE hrs.RegionType WHEN 'HospitalServiceArea' THEN 'HSA'
												WHEN 'HealthReferralRegion' THEN 'HRR' 
												ELSE 'CUS' + CAST([ImportRegionId] AS VARCHAR(60))+ hrs.Abbreviation 
												END, hrs.ImportRegionId, hrs.Abbreviation, hrs.HospitalRegistry_id, hrs.City);

	/*
	*  Migrate Hospitals
	*/
	DECLARE @HospitalID INT
	DECLARE	@LinkedHospitalId INT
	DECLARE	@CmsProviderId NVARCHAR(50)
	DECLARE	@ArchiveDate DATE 
	DECLARE @NewHospitalId INT
	DECLARE @IsArchived BIT, @IsBaseData BIT, @IsDeleted BIT	
	DECLARE @OldHSARegionId INT, @OldHRRRegionId INT, @OldCustomRegionId INT
	DECLARE ArchivedHospitals CURSOR FOR 
		SELECT Id, LinkedHospitalId, CmsProviderId, ArchiveDate, IsArchived, HospitalServiceArea_Id, HealthReferralRegion_Id, CustomRegion_Id, IsSourcedFromBaseData, IsDeleted
		FROM HospitalsTemp 
	
	OPEN ArchivedHospitals
	FETCH NEXT FROM ArchivedHospitals INTO @HospitalID, @LinkedHospitalId, @CmsProviderId, 
										   @ArchiveDate, @IsArchived, @OldHSARegionId, @OldHRRRegionId, @OldCustomRegionId, @IsBaseData, @IsDeleted
	
	WHILE @@FETCH_STATUS = 0
	BEGIN	
		PRINT 'Hospital ID: '+ CAST(@HospitalID AS VARCHAR) + ' Linked Hospital Id:' +CAST(@LinkedHospitalId AS VARCHAR)
	
		-- Archive base hospitals
		IF(@IsBaseData = 1)
			BEGIN	
				PRINT 'Updating Base data Hospital with CMSProviderId = '+ @CmsProviderId
				UPDATE [dbo].[Hospitals] 
				SET IsArchived = @IsArchived, 
					ArchiveDate = @ArchiveDate,
					IsDeleted = @IsDeleted  
				WHERE CmsProviderID = @CmsProviderId 
					AND IsSourcedFromBaseData = @IsBaseData
			END	
		ELSE
			Begin
				--insert newly created hospitals
				PRINT 'Insert user created hospital'
				INSERT INTO [dbo].[Hospitals] ( [Name], [IsSourcedFromBaseData], [Version], [CCR] ,
		          [CmsProviderID] ,[LocalHospitalId], [Address], [City], [Zip], [Description] ,
		          [Employees], [TotalBeds], [HospitalOwnership], [PhoneNumber], [FaxNumber] ,
		          [MedicareMedicaidProvider], [EmergencyService], [TraumaService], [UrgentCareService] ,
		          [PediatricService], [PediatricICUService], [CardiacCatherizationService], [PharmacyService] ,
		          [DiagnosticXRayService], [ForceLocalHospitalIdValidation], [IsDeleted], [LinkedHospitalId] ,
		          [IsArchived] ,[ArchiveDate] ,[HospitalServiceArea_Id] ,
		          [HealthReferralRegion_Id] ,[CustomRegion_Id] ,
		          [Registry_Id] ,[State] ,[County])
				SELECT TOP 1 h.[Name], [IsSourcedFromBaseData], [Version], [CCR] ,
				          [CmsProviderID] ,[LocalHospitalId], [Address], [City], [Zip], [Description] ,
				          [Employees], [TotalBeds], [HospitalOwnership], [PhoneNumber], [FaxNumber] ,
				          [MedicareMedicaidProvider], [EmergencyService], [TraumaService], [UrgentCareService] ,
				          [PediatricService], [PediatricICUService], [CardiacCatherizationService], [PharmacyService] ,
				          [DiagnosticXRayService], [ForceLocalHospitalIdValidation], [IsDeleted], [LinkedHospitalId] ,
				          [IsArchived] ,[ArchiveDate], dbo.fnGetNewRegionId(@OldHSARegionId,'HOSPITALSERVICEAREA',Abbreviation),
				          dbo.fnGetNewRegionId(@OldHRRRegionId ,'HEALTHREFERRALREGION',Abbreviation),dbo.fnGetNewRegionId(@OldCustomRegionId,'CUSTOMREGION',Abbreviation),
				          NULL,Abbreviation, CountyFIPS
				FROM HospitalsTemp  h
				WHERE h.Id = @HospitalID AND h.IsSourcedFromBaseData = 0
	
				SELECT @NewHospitalId = IDENT_CURRENT(N'Hospitals')
				
				IF(@NewHospitalId IS NOT NULL) 
					BEGIN 
						PRINT 'Update Hospital Category table'
						MERGE INTO [dbo].[Hospitals_HospitalCategories] hhc 
						USING (	
								SELECT @NewHospitalId AS HosNewId, C.Id
								FROM [dbo].[Hospitals_HospitalHospitalCategoriesTemp] hhcTemp
									INNER JOIN	[dbo].[Categories] C ON c.Name = hhcTemp.Name
								WHERE hhcTemp.CmsProviderID = @CmsProviderId
							  ) AS	Temp ON hhc.Hospital_Id = Temp.HosNewId AND hhc.Category_Id = Temp.Id	
						WHEN NOT MATCHED THEN 
							INSERT (Hospital_Id, Category_Id)
			 				VALUES	(Temp.HosNewId, Temp.Id);
					END
			END
            
	FETCH NEXT FROM ArchivedHospitals INTO @HospitalID, @LinkedHospitalId, @CmsProviderId, 
										   @ArchiveDate, @IsArchived, @OldHSARegionId, @OldHRRRegionId, @OldCustomRegionId, @IsBaseData, @IsDeleted
	END	
	
	CLOSE	ArchivedHospitals
	DEALLOCATE ArchivedHospitals


	DELETE FROM Websites_WebsiteHospitals

	MERGE INTO [dbo].[Websites_WebsiteHospitals] wh
	USING (	SELECT wht.CCR, h.Id AS HospitalId, wht.Website_Id, wht.[Index] 
			FROM Websites_WebsiteHospitalsTemp wht
				INNER JOIN [dbo].[Hospitals] h ON wht.CmsProviderId = h.CmsProviderId
			WHERE h.IsDeleted = 0 AND h.IsArchived = 0) AS Temp ON Temp.HospitalId = wh.Hospital_Id
	WHEN NOT MATCHED THEN	
		INSERT (CCR, [Hospital_Id], [Website_Id], [INDEX])
		VALUES( Temp.CCR, Temp.HospitalId, Temp.Website_Id, Temp.[Index]);


	--Delete Temp tables
	if OBJECT_ID(N'HospitalsTemp') IS NOT NULL
		DROP TABLE HospitalsTemp

	if OBJECT_ID(N'Hospitals_HospitalCategoriesTemp') IS NOT NULL	
		DROP TABLE Hospitals_HospitalCategoriesTemp

	if OBJECT_ID(N'Hospitals_HospitalHospitalCategoriesTemp') IS NOT NULL	
		DROP TABLE Hospitals_HospitalHospitalCategoriesTemp

	if OBJECT_ID(N'Websites_WebsiteHospitalsTemp') IS NOT	NULL
		DROP TABLE Websites_WebsiteHospitalsTemp

	IF OBJECT_ID(N'fnGetNewRegionId') IS NOT NULL
		DROP FUNCTION fnGetNewRegionId

END TRY

BEGIN CATCH
DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 

END CATCH