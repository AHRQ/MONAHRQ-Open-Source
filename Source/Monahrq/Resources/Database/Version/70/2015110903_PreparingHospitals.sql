BEGIN TRY

	IF(OBJECT_ID(N'[HospitalsTemp]')) IS NOT NULL 
	BEGIN 

		DECLARE @Min INT, @Max INT, @i INT

		SELECT @Min = Min(Id), @Max = Max(Id)
		FROM [HospitalsTemp]

		set @i = @Min
		WHILE (@i <= @Max)
			BEGIN 
				MERGE INTO Hospitals target 
				USING (
							SELECT Name,IsSourcedFromBaseData,Version,CCR,CmsProviderID,LocalHospitalId,Description,Employees,TotalBeds,HospitalOwnership,PhoneNumber,FaxNumber,MedicareMedicaidProvider,EmergencyService,TraumaService,UrgentCareService,PediatricService,PediatricICUService,CardiacCatherizationService,PharmacyService,DiagnosticXRayService,ForceLocalHospitalIdValidation,IsDeleted,IsArchived,ArchiveDate,LinkedHospitalId,Address,City,State,County,Zip,Latitude,Longitude,Registry_Id
							FROM [HospitalsTemp]
							WHERE Id between @i and @i+500
						)
				AS SOURCE (Name,IsSourcedFromBaseData,Version,CCR,CmsProviderID,LocalHospitalId,Description,Employees,TotalBeds,HospitalOwnership,PhoneNumber,FaxNumber,MedicareMedicaidProvider,EmergencyService,TraumaService,UrgentCareService,PediatricService,PediatricICUService,CardiacCatherizationService,PharmacyService,DiagnosticXRayService,ForceLocalHospitalIdValidation,IsDeleted,IsArchived,ArchiveDate,LinkedHospitalId,Address,City,State,County,Zip,Latitude,Longitude,Registry_Id) 
				ON source.CMSPRoviderId = target.CMSProviderId and source.IsSourcedFromBaseData = target.IsSourcedFromBaseData
				WHEN MATCHED THEN 
					UPDATE SET 
							target.Name = source.Name,
							target.IsSourcedFromBaseData = source.IsSourcedFromBaseData,
							target.Version = source.Version,
							target.CCR = source.CCR,
							target.CmsProviderID = source.CmsProviderID,
							target.LocalHospitalId = source.LocalHospitalId,
							target.Description = source.Description,
							target.Employees = source.Employees,
							target.TotalBeds = source.TotalBeds,
							target.HospitalOwnership = source.HospitalOwnership,
							target.PhoneNumber = source.PhoneNumber,
							target.FaxNumber = source.FaxNumber,
							target.MedicareMedicaidProvider = source.MedicareMedicaidProvider,
							target.EmergencyService = source.EmergencyService,
							target.TraumaService = source.TraumaService,
							target.UrgentCareService = source.UrgentCareService,
							target.PediatricService = source.PediatricService,
							target.PediatricICUService = source.PediatricICUService,
							target.CardiacCatherizationService = source.CardiacCatherizationService,
							target.PharmacyService = source.PharmacyService,
							target.DiagnosticXRayService = source.DiagnosticXRayService,
							target.ForceLocalHospitalIdValidation = source.ForceLocalHospitalIdValidation,
							target.ArchiveDate = source.ArchiveDate,
							target.IsArchived = source.IsArchived,
							target.LinkedHospitalId = source.LinkedHospitalId,
							target.Address = source.Address,
							target.City = source.City,
							target.State = source.State,
							target.County = source.County,
							target.Zip = source.Zip,
							target.Latitude = source.Latitude,
							target.Longitude = source.Longitude,
							target.Registry_Id = source.Registry_Id 
				WHEN NOT MATCHED THEN 
					INSERT (Name,IsSourcedFromBaseData,Version,CCR,CmsProviderID,LocalHospitalId,Description,Employees,TotalBeds,HospitalOwnership,PhoneNumber,FaxNumber,MedicareMedicaidProvider,EmergencyService,TraumaService,UrgentCareService,PediatricService,PediatricICUService,CardiacCatherizationService,PharmacyService,DiagnosticXRayService,ForceLocalHospitalIdValidation,IsDeleted,IsArchived,ArchiveDate,LinkedHospitalId,Address,City,State,County,Zip,Latitude,Longitude,Registry_Id) 
					VALUES(source.Name,source.IsSourcedFromBaseData,source.Version,source.CCR,source.CmsProviderID,source.LocalHospitalId,source.Description,source.Employees,source.TotalBeds,source.HospitalOwnership,source.PhoneNumber,source.FaxNumber,source.MedicareMedicaidProvider,source.EmergencyService,source.TraumaService,source.UrgentCareService,source.PediatricService,source.PediatricICUService,source.CardiacCatherizationService,source.PharmacyService,source.DiagnosticXRayService,source.ForceLocalHospitalIdValidation,source.IsDeleted,source.IsArchived,source.ArchiveDate,source.LinkedHospitalId,source.Address,source.City,source.State,source.County,source.Zip,source.Latitude,source.Longitude,source.Registry_Id);

				set @i = @i + 500
			END
	END

	-- Add check to clean up any custom hospitals
	declare @hospId int;
	declare @archivedHospId int;
	declare @cmsProviderId nvarchar(6);
	declare @nonArchivedCmsProviderId nvarchar(6)
	declare @IsSourcedFromBaseData bit; 
	declare @IsArchived bit;
	declare @IsArchived2 bit;
	declare @LinkedHospitalId int;
	declare @name nvarchar(256);
	declare @IsDeleted bit;


	DECLARE db_cursor CURSOR FOR  
	Select T1.[Id], T1.CmsProviderID, T1.[IsSourcedFromBaseData], T1.IsArchived,T1.name,t1.IsDeleted
	from [dbo].[Hospitals] T1
	inner join
	(Select CmsProviderID,IsArchived
	from [dbo].[Hospitals]
	Where [IsDeleted] = 0 and [IsArchived] = 0
	group by CmsProviderID,IsArchived
	having count(*)>1) T2
	on T1.CmsProviderID=T2.CmsProviderID
	Order by  T1.CmsProviderID, T1.[IsSourcedFromBaseData]

	OPEN db_cursor   
	FETCH NEXT FROM db_cursor INTO @hospId, @cmsProviderId, @IsSourcedFromBaseData, @IsArchived, @name, @IsDeleted

	WHILE @@FETCH_STATUS = 0   
	BEGIN   
		   if(@IsSourcedFromBaseData = 0)
		   BEGIN
				SET @LinkedHospitalId = @hospId;
				SET @nonArchivedCmsProviderId = @cmsProviderId;
		   END

		   IF (@IsSourcedFromBaseData = 1 AND @LinkedHospitalId > 0 AND @nonArchivedCmsProviderId IS NOT NULL)
		   BEGIN
			   UPDATE dbo.[Hospitals]
			   SET	LinkedHospitalId = @LinkedHospitalId,
					IsArchived = 1,
					ArchiveDate = getdate()
			   WHERE 
					[Id] = @hospId AND [IsSourcedFromBaseData]=1
			
				SET @LinkedHospitalId = 0;
				SET @nonArchivedCmsProviderId = null;
		   END

		   FETCH NEXT FROM db_cursor INTO @hospId, @cmsProviderId, @IsSourcedFromBaseData, @IsArchived, @name, @IsDeleted   
	END   

	CLOSE db_cursor   
	DEALLOCATE db_cursor

	
	--  Update Latitude & Longitiude from base to custom hospitals (where null)
	update			custh
	set				custh.Latitude = baseh.Latitude
				,	custh.Longitude = baseh.Longitude
	from			Hospitals custh
		inner join	Hospitals baseh on baseh.LinkedHospitalId = custh.Id
	where			custh.Latitude is null
		or			custh.Longitude is null
	   

	-- Update SchemaVerisions table
	UPDATE SchemaVersions
	SET [FileName] = 'POSHospitals-2015-12'
	WHERE Upper(Name) = Upper('Hospitals')

END TRY

BEGIN CATCH
DECLARE @ERRORMESSAGE VARCHAR(5000);
    DECLARE @ERRORSEVERITY INT;
    DECLARE @ERRORSTATE INT;

    SELECT @ERRORMESSAGE = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY = ERROR_SEVERITY(),
           @ERRORSTATE = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE, @ERRORSEVERITY, @ERRORSTATE); 

END CATCH