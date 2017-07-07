-- =============================================
-- Author:		Jason Duffus
-- Modified by:	Shafiul Alam
-- Project:		MONAHRQ 5.0 Build 2
-- Create date: 08-07-2014
-- Modified date: 10-15-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 edited Hospitals and Custom Regions to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'Hospitals and Custom Regions'
-- Modification: 1. Hospitals_HospitalCategories is included for copying data
--				 2. Hospitals_RegionPopulationStrats is included in copying data
--               3. DELETE duplicate non base hospital data which has not link hospital association with Base
-- =============================================

--BEGIN TRY
DECLARE @IsDBVersion1 BIT =0
IF OBJECT_ID('[@@SOURCE@@].[dbo].SchemaVersions') IS NULL
SET @IsDBVersion1=1

DECLARE @ARCHIVE_INSERT_COUNT INT, @CUSTOM_INSERT_COUNT INT;

-- First Get Registry
DECLARE @HospitalRegistry_Id INT;
SELECT @HospitalRegistry_Id = r.Id FROM [@@DESTINATION@@].[dbo].[Hospitals_HospitalRegistries] r;
		
		-- Copy over custom Hospital categories
		INSERT INTO [@@DESTINATION@@].dbo.Hospitals_HospitalCategories
		(
			Name,
			IsSourcedFromBaseData,
			Version,
			CategoryID,
			Registry_Id
		)
		SELECT
			hhc.Name,
			hhc.IsSourcedFromBaseData,
			hhc.Version,
			hhc.CategoryID,
			@HospitalRegistry_Id Registry_Id
		FROM
			[@@SOURCE@@].dbo.Hospitals_HospitalCategories hhc
		WHERE hhc.IsSourcedFromBaseData=0
		AND NOT EXISTS (SELECT 1 FROM [@@DESTINATION@@].dbo.Hospitals_HospitalCategories hhc2
		                WHERE hhc2.IsSourcedFromBaseData=0
		                AND hhc.IsSourcedFromBaseData=0
		                AND RTRIM(LTRIM(hhc2.Name))=RTRIM(LTRIM(hhc.Name)))
		
		-- Copy over all custom regions
		INSERT INTO [@@DESTINATION@@].[dbo].[Hospitals_Regions] 
							([RegionType],[Name],[IsSourcedFromBaseData],[Version],[ImportRegionId],[State_Id],[HospitalRegistry_id],[City])
		SELECT  [RegionType],
				[Name],
				[IsSourcedFromBaseData],
				[Version],
				[ImportRegionId],
				[State_Id],
				@HospitalRegistry_Id,
				null
		FROM [@@SOURCE@@].[dbo].[Hospitals_Regions] a
		WHERE UPPER([RegionType]) = 'CUSTOMREGION'
		AND NOT EXISTS (SELECT 1 FROM 
							[@@DESTINATION@@].[dbo].[Hospitals_Regions] b 
							WHERE UPPER(a.[RegionType]) = UPPER(b.[RegionType]) 
							AND a.[ImportRegionId] = b.[ImportRegionId] 
							AND a.[Name] = b.[name]
							AND a.[City] = b.[City]
							AND UPPER(b.[RegionType]) = 'CUSTOMREGION')

-- check whether this is versions later than 5.0.1
    IF(@IsDBVersion1=0)
    BEGIN
        
		-- copy over custom Hospital region population
    	-- To copy Hospital region population data from 5.0.2 to 5.0.3
        IF EXISTS(SELECT 1 FROM [@@SOURCE@@].dbo.SchemaVersions sv
				  WHERE sv.Version='5.0.2') 
			INSERT INTO [@@DESTINATION@@].dbo.Hospitals_RegionPopulationStrats
					(
						RegionType,
						RegionID,
						CatID,
						CatVal,
						[Year],
						[Population]
					)
			SELECT
						hrps.RegionType,
						hrps.RegionID,
						CASE WHEN hrps.AgeGroup>0 THEN 1
							 WHEN hrps.Sex>0 THEN 2
							 WHEN hrps.Race>0 THEN 4
							 ELSE 0
							 END CatID,
						CASE WHEN hrps.AgeGroup>0 THEN AgeGroup
							 WHEN hrps.Sex>0 THEN Sex
							 WHEN hrps.Race>0 THEN Race
							 ELSE 0
							 END CatVal,
						hrps.[Year],
						hrps.[Population]
					FROM [@@SOURCE@@].dbo.Hospitals_RegionPopulations hrps
					WHERE hrps.RegionType=0
					AND NOT EXISTS (SELECT 1 FROM [@@DESTINATION@@].dbo.Hospitals_RegionPopulationStrats hrps2
									WHERE hrps.RegionID=hrps2.RegionID
									AND hrps2.RegionType=0)
		ELSE
			INSERT INTO [@@DESTINATION@@].dbo.Hospitals_RegionPopulationStrats
			(
				RegionType,
				RegionID,
				CatID,
				CatVal,
				[Year],
				[Population]
			)
			SELECT
				hrps.RegionType,
				hrps.RegionID,
				hrps.CatID,
				hrps.CatVal,
				hrps.[Year],
				hrps.[Population]
			FROM [@@SOURCE@@].dbo.Hospitals_RegionPopulationStrats hrps
			WHERE hrps.RegionType=0
			AND NOT EXISTS (SELECT 1 FROM [@@DESTINATION@@].dbo.Hospitals_RegionPopulationStrats hrps2
							WHERE hrps.RegionID=hrps2.RegionID
							AND hrps.CatID=hrps2.CatID
							AND hrps.CatVal=hrps2.CatVal
							AND hrps2.RegionType=0)
    END


DECLARE @Hospital_Id INT,
        @CmsProvider_Id NVARCHAR(50),
        @LocalHospital_Id NVARCHAR(50),
        @StateAbbrev VARCHAR(5),
        @IsArchived BIT,
        @IsBase BIT,
        @LinkedNonArchivedHospital_Id INT;
DECLARE @Hospitals_Cursor CURSOR ;

SET @Hospitals_Cursor = CURSOR FOR
WITH HospitalsCTE (HospitalId, CmsProviderID, LocalHospitalId, StateAbbr, IsArchived, IsBase, LinkedNonArchivedHospitalId) AS
(
	-- Get Build 1 Base hospital which are archived
		SELECT  h.[Id], h.[CmsProviderID], h.[LocalHospitalId], s.[Abbreviation], h.[IsArchived], h.[IsSourcedFromBaseData] 'IsBase', h.[LinkedHospitalId] 
		FROM [@@SOURCE@@].[dbo].[Hospitals] h 	
			inner join  [@@SOURCE@@].[dbo].[Base_States] s on s.[Id] = h.[State_Id]		  
		WHERE h.[IsArchived] = 1 and h.[IsDeleted] = 0 and h.[IsSourcedFromBaseData] = 1 and h.[State_Id] is not null
   
    UNION ALL
    
    -- Get Build 1 Base hospital which are NOT archived and used in Website publish but NOT available Build 2
    SELECT  h.[Id], h.[CmsProviderID], h.[LocalHospitalId], s.[Abbreviation], h.[IsArchived], h.[IsSourcedFromBaseData] 'IsBase', h.[LinkedHospitalId] 
		FROM [@@SOURCE@@].[dbo].[Hospitals] h 	
			inner join  [@@SOURCE@@].[dbo].[Base_States] s on s.[Id] = h.[State_Id]		  
		WHERE h.[IsArchived] = 0 and h.[IsDeleted] = 0 and h.[IsSourcedFromBaseData] = 1 and h.[State_Id] is NOT NULL
		  AND EXISTS (SELECT 1 FROM [@@SOURCE@@].[dbo].Websites_WebsiteHospitals wwh
	            WHERE h.Id=wwh.Hospital_Id)
		  AND NOT EXISTS (SELECT 1 FROM [@@DESTINATION@@].dbo.Hospitals h2
                    WHERE rtrim(ltrim(h2.Name))=rtrim(ltrim(h.Name))
                    AND h2.CmsProviderID=h.CmsProviderID) 

	UNION ALL 
	
	-- Get Build 1 custom hospital not created from Base hospital
		SELECT h.[Id], h.[CmsProviderID], h.[LocalHospitalId], s.[Abbreviation], h.[IsArchived], h.[IsSourcedFromBaseData] 'IsBase' , h.[LinkedHospitalId] 
		FROM [@@SOURCE@@].[dbo].[Hospitals] h
			inner join  [@@SOURCE@@].[dbo].[Base_States] s on s.[Id] = h.[State_Id]
		WHERE h.[IsArchived] = 0 and h.[IsDeleted] = 0 and h.[IsSourcedFromBaseData] = 0 and h.[State_Id] is NOT NULL 
		AND h.[Id] not in (
										SELECT DISTINCT h.[LinkedHospitalId] 
										FROM [@@SOURCE@@].[dbo].[Hospitals] h 	
											inner join  [@@SOURCE@@].[dbo].[Base_States] s on s.[Id] = h.[State_Id]		  
										WHERE h.[IsArchived] = 1 and h.[IsDeleted] = 0 and h.[IsSourcedFromBaseData] = 1 and h.[State_Id] is not null
										--SELECT DISTINCT h2.[CmsProviderID]
										--FROM [@@SOURCE@@].[dbo].[Hospitals] h2 
										--WHERE /*h2.[LinkedHospitalId] is not null  and*/ h.[IsSourcedFromBaseData] = 1 and h2.[IsArchived] = 1 and h.[IsDeleted] = 0 --and h.[State_Id] is not null
										)
)
SELECT DISTINCT h.HospitalId, h.CmsProviderID, h.LocalHospitalId, h.StateAbbr, h.IsArchived, h.IsBase, LinkedNonArchivedHospitalId
FROM HospitalsCTE h
ORDER BY h.IsBase DESC,h.IsArchived DESC, h.CmsProviderID DESC;

OPEN @Hospitals_Cursor;

FETCH next FROM @Hospitals_Cursor INTO @Hospital_Id, @CmsProvider_Id, @LocalHospital_Id, @StateAbbrev, @IsArchived, @IsBase, @LinkedNonArchivedHospital_Id

WHILE @@fetch_status = 0
BEGIN
	PRINT 'Hospital Id: ' + cast(@Hospital_Id AS NVARCHAR);
	PRINT 'CmsProvider Id: ' + @CmsProvider_Id;
	IF(@LocalHospital_Id is not null)
	BEGIN
		PRINT 'Local Hospital Id: ' + @LocalHospital_Id;
	END
	PRINT 'State: ' + @StateAbbrev;
	PRINT 'Is Archived: ' + CAST(@IsArchived AS NVARCHAR);
	PRINT 'Is Base: ' + CAST(@IsBase AS NVARCHAR);
	IF(@LinkedNonArchivedHospital_Id IS NOT NULL)
	BEGIN
		PRINT 'Linked Nonarchived Hospital Id: ' + CAST(@LinkedNonArchivedHospital_Id AS NVARCHAR);
	END

	/******************************************************************************
	 * Handle Archived Base Hospitals and their counterpart linked custom hospitals.
	 *****************************************************************************/ 
	 
	IF (
	       @IsArchived = 1
	       AND @IsBase=1
	       AND @LinkedNonArchivedHospital_Id IS NOT NULL
	   )
	   
	BEGIN
		PRINT '****************************'
		Print 'Inside Archived Hospital';
		PRINT '****************************'

		
			DECLARE @ArchiveHosp_Id int, @NewLinkedHosp_Id INT;

			-- First Archive Hospital
			UPDATE [@@DESTINATION@@].[dbo].[Hospitals] 
			SET [IsArchived] = 1, [ArchiveDate] = GeTDATE()
			WHERE [CmsProviderID] = @CmsProvider_Id and [IsSourcedFromBaseData] = 1 and [IsDeleted] = 0;
			
			
			SELECT @ArchiveHosp_Id = [Id] FROM [@@DESTINATION@@].[dbo].[Hospitals] 
										WHERE [CmsProviderID] = @CmsProvider_Id AND [IsArchived] = 1;
			PRINT 'Archived Hospital New Id: ' + cast(@ArchiveHosp_Id as varchar);

			IF(@LinkedNonArchivedHospital_Id IS NOT NULL)
			BEGIN
				DECLARE @CountyFIPs NVARCHAR(10), @FIPSState NVARCHAR(200), @HSAImportRegionId INT, @HRRImportRegionId INT, @CUSTOMRegionId INT;
				SELECT @CountyFIPs = c.[CountyFIPs] FROM [@@SOURCE@@].[dbo].[Base_Counties] c
														INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[County_id] = c.[Id]
													WHERE oh.[Id] = @LinkedNonArchivedHospital_Id AND c.[State_Id] = oh.[State_Id]
												  
				SELECT @FIPSState = s.[FIPSState] FROM [@@DESTINATION@@].[dbo].[Base_States] s WHERE upper(s.[Abbreviation]) = upper(@StateAbbrev);
												  
				SELECT @HSAImportRegionId = r.[ImportRegionId] FROM [@@SOURCE@@].[dbo].[Hospitals_Regions] r
														INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[HospitalServiceArea_Id] = r.[Id]
													WHERE oh.[Id] = @LinkedNonArchivedHospital_Id AND UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA';		
												  
				SELECT @HRRImportRegionId = r.[ImportRegionId] FROM [@@SOURCE@@].[dbo].[Hospitals_Regions] r
														INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[HealthReferralRegion_Id] = r.[Id]
													WHERE oh.[Id] = @LinkedNonArchivedHospital_Id AND UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION';	

				SELECT @CUSTOMRegionId = r.[ImportRegionId] FROM [@@SOURCE@@].[dbo].[Hospitals_Regions] r
														INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[CustomRegion_Id] = r.[Id]
														INNER JOIN [@@SOURCE@@].[dbo].[Base_States] s ON s.[Id] = oh.[State_Id]
													WHERE oh.[Id] = @LinkedNonArchivedHospital_Id AND UPPER(r.[RegionType]) = 'CUSTOMREGION';

				INSERT INTO [@@DESTINATION@@].[dbo].[Hospitals] 
				SELECT 
						h1.[Name]
						,0
						,h1.[Version]
						,h1.[CCR]
						,h1.[CmsProviderID]
						,h1.[LocalHospitalId]
						,h1.[Address]
						,h1.[City]
						,h1.[Zip]
						,h1.[Description]
						,h1.[Employees]
						,h1.[TotalBeds]
						,h1.[HospitalOwnership]
						,h1.[PhoneNumber]
						,h1.[FaxNumber]
						,h1.[MedicareMedicaidProvider]
						,h1.[EmergencyService]
						,h1.[TraumaService]
						,h1.[UrgentCareService]
						,h1.[PediatricService]
						,h1.[PediatricICUService]
						,h1.[CardiacCatherizationService]
						,h1.[PharmacyService]
						,h1.[DiagnosticXRayService]
						,h1.[ForceLocalHospitalIdValidation]
						,0
						,NULL
						,0
						,NULL
						,(SELECT TOP 1 s.[Id] FROM [@@DESTINATION@@].[dbo].[Base_States] s WHERE s.[FIPSState] = @FIPSState)
						,(SELECT TOP 1 c.[Id] FROM [@@DESTINATION@@].[dbo].[Base_Counties] c WHERE c.[CountyFIPS] = @CountyFIPs)
						,(SELECT TOP 1 r.[Id] FROM [@@DESTINATION@@].[dbo].[Hospitals_Regions] r 
											WHERE UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA' AND r.[ImportRegionId] = @HSAImportRegionId)
						,(SELECT TOP 1 r.[Id] FROM [@@DESTINATION@@].[dbo].[Hospitals_Regions] r 
											WHERE UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION' AND r.[ImportRegionId] = @HRRImportRegionId)
						,(SELECT TOP 1 r.[Id] FROM [@@DESTINATION@@].[dbo].[Hospitals_Regions] r 
											INNER JOIN [@@SOURCE@@].[dbo].[Base_States] s ON s.[Id] = r.[State_Id]
											WHERE UPPER(r.[RegionType]) = 'CUSTOMREGION' AND r.[ImportRegionId] = @CustomRegionId AND s.[Abbreviation] = @StateAbbrev)
						,[SelectedRegion_Id] -- TODO: Finish Selected Region
						,@HospitalRegistry_Id
				FROM [@@SOURCE@@].[dbo].[Hospitals] h1
				WHERE h1.[Id] = @LinkedNonArchivedHospital_Id
				AND h1.IsSourcedFromBaseData=0
				AND h1.IsDeleted=0
				AND h1.IsArchived=0  
				AND NOT EXISTS(SELECT 1 FROM [@@DESTINATION@@].[dbo].Hospitals h2
				               WHERE RTRIM(LTRIM(UPPER(h1.Name)))=RTRIM(LTRIM(UPPER(h2.Name)))
				               AND h1.CmsProviderID=h2.CmsProviderID
				               AND h1.LocalHospitalId = h2.LocalHospitalId
				               AND h2.IsSourcedFromBaseData=0
				               AND h2.IsDeleted=0
				               AND h2.IsArchived=0 );
				-- Get New Id
				SET @NewLinkedHosp_Id = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[Hospitals]');  --SCOPE_IDENTITY(); 

				PRINT 'Linked Hospital New Id: ' + cast(@NewLinkedHosp_Id AS NVARCHAR);

				    -- Copy hospital and category mapping for Link hospital
				    INSERT INTO [@@DESTINATION@@].[dbo].[Hospitals_HospitalHospitalCategories]
				      (
				        [Hospital_Id],
				        [HospitalCategory_Id]
				      )
				    SELECT DISTINCT
				           @NewLinkedHosp_Id,
				           hc.[Id]
				    FROM   [@@DESTINATION@@].[dbo].[Hospitals_HospitalCategories]  hc
				           INNER JOIN [@@SOURCE@@].[dbo].[Hospitals_HospitalCategories]  hc1
				                ON  RTRIM(LTRIM(hc1.[Name])) = RTRIM(LTRIM(hc.[Name]))
				           INNER JOIN [@@SOURCE@@].[dbo].[Hospitals_HospitalHospitalCategories] hhc
				                ON  hhc.[HospitalCategory_Id] = hc1.[Id]
				    WHERE  hhc.[Hospital_Id] = @LinkedNonArchivedHospital_Id
				    AND NOT EXISTS(SELECT 1 FROM [@@DESTINATION@@].[dbo].[Hospitals_HospitalHospitalCategories] b
				                   WHERE b.[Hospital_Id]=@NewLinkedHosp_Id)
				    
				    -- Finally set LinkedHospital id to new Id
				    UPDATE [@@DESTINATION@@].[dbo].[Hospitals]
				    SET    [LinkedHospitalId] = CAST(@NewLinkedHosp_Id AS NVARCHAR(200))
				    WHERE  [Id] = @ArchiveHosp_Id;
				

				PRINT ('Archived Hospital Id: ' + cast(@ArchiveHosp_Id AS NVARCHAR));
				PRINT ('Linked Hospital New Id: ' + cast(@NewLinkedHosp_Id AS NVARCHAR));
			END
		
	END

	-- Handle Base Hospitals
	--IF(@IsArchived = 0 AND @IsBase = 1) 
	--BEGIN
	--	PRINT '****************************'
	--	PRINT 'Inside Base Hospital'
	--	PRINT '****************************'
	--END

	/**************************************************
	* Handle Custom Hospitals
	* Hanble Base NonArchived hospital does not exist int build 2 but Website is built with this hospital
	***************************************************/
	
	IF (@IsArchived = 0 AND @IsBase = 0)
	OR (@IsArchived = 0 AND @IsBase = 1 )
	BEGIN
		
		DECLARE @IsBaseHospitalExists BIT=0
		
		IF(@IsArchived = 0 AND @IsBase = 1)
		SET @IsBaseHospitalExists=1
		
		
		DECLARE @NewHosp_Id INT;

		PRINT '****************************'
		PRINT 'Inside Custom Hospital'
		PRINT '****************************'

		DECLARE @CountyFIPs2 NVARCHAR(10), @FIPSState2 NVARCHAR(200), @HSAImportRegionId2 INT, @HRRImportRegionId2 INT;
		SELECT @CountyFIPs2 = c.[CountyFIPs] FROM [@@SOURCE@@].[dbo].[Base_Counties] c
												INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[County_id] = c.[Id]
											WHERE oh.[Id] = @Hospital_Id;			
												  
		SELECT @FIPSState2 = s.[FIPSState] FROM [@@SOURCE@@].[dbo].[Base_States] s
												INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[State_id] = s.[Id]
											WHERE oh.[Id] = @Hospital_Id;			
												  
		SELECT @HSAImportRegionId2 = r.[ImportRegionId] FROM [@@SOURCE@@].[dbo].[Hospitals_Regions] r
												INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[HospitalServiceArea_Id] = r.[Id]
											WHERE oh.[Id] = @Hospital_Id;		
												  
		SELECT @HRRImportRegionId2 = r.[ImportRegionId] FROM [@@SOURCE@@].[dbo].[Hospitals_Regions] r
												INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] oh ON oh.[HealthReferralRegion_Id] = r.[Id]
											WHERE oh.[Id] = @Hospital_Id;	
		
		INSERT INTO [@@DESTINATION@@].[dbo].[Hospitals] 
		SELECT 
				CASE WHEN @IsBaseHospitalExists=1 THEN h1.[Name]+' [Deprecated]' ELSE h1.[Name] END [Name]  
				,0
				,h1.[Version]
				,h1.[CCR]
				,h1.[CmsProviderID]
				,h1.[LocalHospitalId]
				,h1.[Address]
				,h1.[City]
				,h1.[Zip]
				,h1.[Description]
				,h1.[Employees]
				,h1.[TotalBeds]
				,h1.[HospitalOwnership]
				,h1.[PhoneNumber]
				,h1.[FaxNumber]
				,h1.[MedicareMedicaidProvider]
				,h1.[EmergencyService]
				,h1.[TraumaService]
				,h1.[UrgentCareService]
				,h1.[PediatricService]
				,h1.[PediatricICUService]
				,h1.[CardiacCatherizationService]
				,h1.[PharmacyService]
				,h1.[DiagnosticXRayService]
				,h1.[ForceLocalHospitalIdValidation]
				,0
				,null
				,0
				,null
				,(SELECT TOP 1 s.[Id] FROM [@@DESTINATION@@].[dbo].[Base_States] s WHERE s.[FIPSState] = @FIPSState2)
				,(SELECT TOP 1 c.[Id] FROM [@@DESTINATION@@].[dbo].[Base_Counties] c WHERE c.[CountyFIPS] = @CountyFIPs2)
				,(SELECT TOP 1 r.[Id] FROM [@@DESTINATION@@].[dbo].[Hospitals_Regions] r 
									WHERE UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA' and r.[ImportRegionId] = @HSAImportRegionId2)
				,(SELECT TOP 1 r.[Id] FROM [@@DESTINATION@@].[dbo].[Hospitals_Regions] r 
									WHERE UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION' and r.[ImportRegionId] = @HRRImportRegionId2)
				,[CustomRegion_Id] -- TODO: Finish Custom Region
				,[SelectedRegion_Id] -- TODO: Finish Selected Region
				,@HospitalRegistry_Id
		FROM [@@SOURCE@@].[dbo].[Hospitals] h1
		WHERE h1.[Id] = @Hospital_Id

		-- Get New Id
		SET @NewHosp_Id = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[Hospitals]'); --SCOPE_IDENTITY(); 
		
		-- If any hospital comes as base in Build 2 then need to link Build 1 custom hospital
		-- with that Build 2 Base hospital
		
		IF(@IsArchived = 0 AND @IsBase = 0)
		    UPDATE [@@DESTINATION@@].[dbo].[Hospitals] 
			SET [LinkedHospitalId]=@NewHosp_Id, [IsArchived] = 1, [ArchiveDate] = GeTDATE()
			WHERE [CmsProviderID] = @CmsProvider_Id and [IsSourcedFromBaseData] = 1 and [IsDeleted] = 0
			AND LinkedHospitalId IS NULL;
			

		PRINT 'New Hospital Id: ' + CAST(@NewHosp_Id AS VARCHAR(MAX));

		-- Copy hospital category information
		INSERT INTO [@@DESTINATION@@].[dbo].[Hospitals_HospitalCategories]
		  (
		    [Hospital_Id],
		    [Category_Id]
		  )
		SELECT DISTINCT @NewHosp_Id,
		       hc.[Id]
		FROM   [@@DESTINATION@@].[dbo].[Categories] hc
		       INNER JOIN [@@SOURCE@@].[dbo].[Categories] hc1
		            ON  RTRIM(LTRIM(hc1.[Name])) = RTRIM(LTRIM(hc.[Name]))
		       INNER JOIN [@@SOURCE@@].[dbo].[Hospitals_HospitalCategories] hhc
		            ON  hhc.[Category_Id] = hc1.[Id]
		WHERE  hhc.[Hospital_Id] = @Hospital_Id
		AND NOT EXISTS(SELECT 1 FROM [@@DESTINATION@@].[dbo].[Hospitals_HospitalCategories] b
				                   WHERE b.[Hospital_Id]=@NewHosp_Id)
	END

	--PRINT Char(10);
	SET @ARCHIVE_INSERT_COUNT = @ARCHIVE_INSERT_COUNT + 1;
	SET @CUSTOM_INSERT_COUNT = @CUSTOM_INSERT_COUNT + 1;

	FETCH next FROM @Hospitals_Cursor INTO @Hospital_Id, @CmsProvider_Id, @LocalHospital_Id, @StateAbbrev, @IsArchived, @IsBase, @LinkedNonArchivedHospital_Id
END
 

PRINT 'ARCIVE INSERTED #: ' + cast( @ARCHIVE_INSERT_COUNT as nvarchar(max));
PRINT 'CUSTOM INSERTED #: ' + cast( @CUSTOM_INSERT_COUNT as nvarchar(max));

close @Hospitals_Cursor;
deallocate @Hospitals_Cursor;


/***********************************************************************
 *			Soft Delete duplicate data which are custom with same CmsProviderId.
 *			In such case ,IsDeleted=1 needs to be updated for that
 *			specific custom hospital that has no linkedHospital information
 *			association with the Base hospital Id.   
 **********************************************************************/

UPDATE [@@DESTINATION@@].[dbo].Hospitals
SET IsDeleted = 1
WHERE  id IN (SELECT id
             FROM   [@@DESTINATION@@].[dbo].Hospitals
             WHERE  CmsProviderID IN (SELECT CmsProviderID
                                       FROM   [@@DESTINATION@@].[dbo].Hospitals h
                                       WHERE  IsSourcedFromBaseData = 0
                                       AND    IsDeleted=0
                                       GROUP BY CmsProviderID
                                       HAVING COUNT(1) > 1)
                     AND IsSourcedFromBaseData = 0
                     AND IsDeleted=0
            EXCEPT
			SELECT LinkedHospitalId
			FROM   [@@DESTINATION@@].[dbo].Hospitals
			WHERE  LinkedHospitalId IS NOT NULL)

SELECT 1;