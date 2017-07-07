-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0 Build 1
-- Create date: 12-22-2014
-- Description:	This is the update script from older MONAHRQ 5.0 edited Hospitals and Custom Regions to the new 
--              MONAHRQ 6.0
--				'Hospitals and Custom Regions'
-- =============================================

BEGIN TRY
/*
DECLARE @BackupTable VARCHAR(50),
        @sql VARCHAR(1000)
SELECT @BackupTable ='Hospitals_'+CONVERT(VARCHAR(10), GETDATE(), 112)+'_Bkp'


-- First Get Registry
DECLARE @HospitalRegistry_Id INT;
SELECT @HospitalRegistry_Id = r.Id FROM [dbo].[Hospitals_HospitalRegistries] r;

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hospitals_HospitalHospitalCategories]') AND type in (N'U'))
BEGIN
	
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID('Hospitals_HospitalCategories'))
		BEGIN
		SELECT 
		@sql='ALTER TABLE [dbo].['+OBJECT_NAME(fk.parent_object_id)+'] DROP CONSTRAINT '+NAME 
		FROM sys.foreign_keys fk
		WHERE fk.referenced_object_id= OBJECT_ID('Hospitals_HospitalCategories')

		EXEC (@sql)
		END	
		
		-- Copy over custom Hospital categories
		-- Hospitals_HospitalCategories table has been removed
		-- All data from Hospitals_HospitalCategories are copied to Categories table
		-- This is done to incorporate nursing home / physician category
INSERT INTO dbo.Categories
		(
			CategoryType,
			Name,
			IsSourcedFromBaseData,
			Version,
			CategoryID,
			Registry_Id
		)
		SELECT
		    'HospitalCategory',
			hhc.Name,
			hhc.IsSourcedFromBaseData,
			hhc.Version,
			hhc.CategoryID,
			@HospitalRegistry_Id Registry_Id
		FROM
			dbo.Hospitals_HospitalCategories hhc
		WHERE hhc.IsSourcedFromBaseData=0
		AND NOT EXISTS (SELECT 1 FROM dbo.Categories hhc2
		                WHERE hhc2.IsSourcedFromBaseData=0
		                AND hhc.IsSourcedFromBaseData=0
		                AND RTRIM(LTRIM(hhc2.Name))=RTRIM(LTRIM(hhc.Name))
		                AND hhc2.CategoryID=hhc.CategoryID)

	
		/*******************************************
		 *  Updating latest hospital category Id 
		 *  from Categories table
		 *******************************************/
UPDATE Hospitals_HospitalHospitalCategories
		SET HospitalCategory_Id = b.ID
		FROM Hospitals_HospitalHospitalCategories a
		INNER JOIN Hospitals_HospitalCategories hhc
		ON a.HospitalCategory_Id=hhc.Id
		INNER JOIN Categories b 
		ON hhc.CategoryID=b.CategoryID
		WHERE b.IsSourcedFromBaseData=0
		AND hhc.IsSourcedFromBaseData=0


END


SET IDENTITY_INSERT [Regions] ON 

-- Copy over all egions
INSERT INTO [dbo].[Regions] (Id, [RegionType],[Name],[IsSourcedFromBaseData],[Version],
	Code,[ImportRegionId],[State],[HospitalRegistry_id],[City])
SELECT	[Id], [RegionType], [Name], [IsSourcedFromBaseData], [Version],
		CASE a.RegionType WHEN 'HospitalServiceArea' THEN 'HSA'
		WHEN 'HealthReferralRegion' THEN 'HRR' ELSE 'CUS' END +CAST([ImportRegionId] AS VARCHAR(60))+(SELECT bs.Abbreviation FROM dbo.Base_States bs
								WHERE bs.Id=a.State_Id) Code,
		[ImportRegionId],
		(SELECT bs.Abbreviation FROM dbo.Base_States bs
		 WHERE bs.Id=a.State_Id) [State],
		@HospitalRegistry_Id,
		[City]
FROM [dbo].[Hospitals_Regions] a
		
SET IDENTITY_INSERT [Regions] OFF	 					

	-- check whether this is versions later than 5.0.1
IF EXISTS(SELECT 1 FROM dbo.SchemaVersions sv
          WHERE sv.Version='5.0.2') 
			INSERT INTO dbo.RegionPopulationStrats
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
					FROM dbo.Hospitals_RegionPopulations hrps
					WHERE hrps.RegionType=0
					AND NOT EXISTS (SELECT 1 FROM dbo.RegionPopulationStrats hrps2
									WHERE hrps.RegionID=hrps2.RegionID
									AND hrps2.RegionType=0)
		ELSE
        							
		INSERT INTO dbo.RegionPopulationStrats
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
		FROM dbo.Hospitals_RegionPopulationStrats hrps
		WHERE hrps.RegionType=0
		AND NOT EXISTS (SELECT 1 FROM dbo.RegionPopulationStrats hrps2
						WHERE hrps.RegionID=hrps2.RegionID
						AND hrps.CatID=hrps2.CatID
						AND hrps.CatVal=hrps2.CatVal
						AND hrps2.RegionType=0)

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[HospitalRegistry_owns_HospitalServiceArea_FK]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals_Regions]'))
ALTER TABLE [dbo].[Hospitals_Regions] DROP CONSTRAINT [HospitalRegistry_owns_HospitalServiceArea_FK]

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[HospitalRegistry_owns_HospitalServiceArea_FK]') AND parent_object_id = OBJECT_ID(N'[dbo].[Regions]'))
BEGIN
ALTER TABLE [dbo].[Regions]  WITH NOCHECK ADD  CONSTRAINT [HospitalRegistry_owns_HospitalServiceArea_FK] FOREIGN KEY([HospitalRegistry_id])
REFERENCES [dbo].[Hospitals_HospitalRegistries] ([Id])
ALTER TABLE [dbo].[Regions] CHECK CONSTRAINT [HospitalRegistry_owns_HospitalServiceArea_FK]
END

/*******************************************
 *  Dropping all Region related foreign keys
 *  To update with latest Region Id from 
 *  Regions tables
 *******************************************/
   
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospital_HealthReferralRegion]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK_Hospital_HealthReferralRegion]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospitals_CustomRegion]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK_Hospitals_CustomRegion]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospitals_HospitalServiceArea]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK_Hospitals_HospitalServiceArea]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospitals_SelectedRegion]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK_Hospitals_SelectedRegion]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospitals_State]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT FK_Hospitals_State

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospitals_County]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT FK_Hospitals_County

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@BackupTable) AND type in (N'U'))
BEGIN
SET @sql ='SELECT * INTO [dbo].['+@BackupTable+'] From Hospitals'
EXEC (@sql)
END

/*******************************************
           * Update Hospital Service area 
           *******************************************/
 UPDATE Hospitals
          SET
          	HospitalServiceArea_Id = b.Id
          FROM Hospitals a
          INNER JOIN (SELECT h.Id Hospital_id ,r2.Id FROM [dbo].[Hospitals_Regions] r 
                         INNER JOIN Regions r2
                         ON r.ImportRegionId=r2.ImportRegionId
                        AND r.State_Id=(SELECT id FROM Base_States bs
                                        WHERE r2.[State]=bs.Abbreviation)
                         INNER JOIN Hospitals h
                         ON h.[HospitalServiceArea_Id] = r.[Id]  
                      WHERE UPPER(r.[RegionType]) = 'HOSPITALSERVICEAREA'
                      AND UPPER(r2.[RegionType]) = 'HOSPITALSERVICEAREA') b
         ON a.id=b.Hospital_id
         
 /*******************************************
           * Update Hospital Custom Region 
           *******************************************/
 UPDATE Hospitals
          SET
          	CustomRegion_Id = b.Id
          FROM Hospitals a
          INNER JOIN (SELECT h.Id Hospital_id ,r2.Id FROM [dbo].[Hospitals_Regions] r 
                         INNER JOIN Regions r2
                         ON r.ImportRegionId=r2.ImportRegionId
                        AND r.State_Id=(SELECT id FROM Base_States bs
                                        WHERE r2.[State]=bs.Abbreviation)
                         INNER JOIN Hospitals h
                         ON h.CustomRegion_Id = r.[Id]  
                      WHERE UPPER(r.[RegionType]) = 'CUSTOMREGION'
                      AND UPPER(r2.[RegionType]) = 'CUSTOMREGION') b
         ON a.id=b.Hospital_id
         
 /*******************************************
           * Update Health Referral Region 
           *******************************************/
 UPDATE Hospitals
          SET
          	HealthReferralRegion_Id = b.Id
          FROM Hospitals a
          INNER JOIN (SELECT h.Id Hospital_id ,r2.Id FROM [dbo].[Hospitals_Regions] r 
                         INNER JOIN Regions r2
                         ON r.ImportRegionId=r2.ImportRegionId
                        AND r.State_Id=(SELECT id FROM Base_States bs
                                        WHERE r2.[State]=bs.Abbreviation)
                         INNER JOIN Hospitals h
                         ON h.HealthReferralRegion_Id = r.[Id]  
                      WHERE UPPER(r.[RegionType]) = 'HEALTHREFERRALREGION'
                      AND UPPER(r2.[RegionType]) = 'HEALTHREFERRALREGION') b
         ON a.id=b.Hospital_id                 

/*******************************************
          *  Updating State and County FIPS
          *  
          *******************************************/

 UPDATE Hospitals SET [State] = (SELECT bs.Abbreviation
                                           FROM base_states bs
                                         WHERE hospitals.State_id = bs.id),
                              [County] = (SELECT bc.CountyFIPS
                                            FROM Base_Counties bc 
                                          WHERE Hospitals.County_id =bc.Id);


/*******************************************
 *  Backup of Old Object
 *******************************************/

/*******************************************
 *  Hospitals_Regions
 *******************************************/
 
SELECT @BackupTable ='Hospitals_Regions'+CONVERT(VARCHAR(10), GETDATE(), 112)+'_Bkp'
 
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(@BackupTable) AND type in (N'U'))
BEGIN
SET @sql ='SELECT * INTO [dbo].['+@BackupTable+'] From Hospitals_Regions'
EXEC (@sql)
END

                                         
/*****************************************************
 *  Creating Region Id foreign Keys in Hospitals table
 *****************************************************/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospital_HealthReferralRegion]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
BEGIN
ALTER TABLE [dbo].[Hospitals]  WITH CHECK ADD  CONSTRAINT [FK_Hospital_HealthReferralRegion] FOREIGN KEY([HealthReferralRegion_Id])
REFERENCES [dbo].[Regions] ([Id])
ALTER TABLE [dbo].[Hospitals] CHECK CONSTRAINT [FK_Hospital_HealthReferralRegion]
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospitals_CustomRegion]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
BEGIN

ALTER TABLE [dbo].[Hospitals]  WITH CHECK ADD  CONSTRAINT [FK_Hospitals_CustomRegion] FOREIGN KEY([CustomRegion_Id])
REFERENCES [dbo].[Regions] ([Id])
ALTER TABLE [dbo].[Hospitals] CHECK CONSTRAINT [FK_Hospitals_CustomRegion]

END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Hospitals_HospitalServiceArea]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hospitals]'))
BEGIN

ALTER TABLE [dbo].[Hospitals]  WITH CHECK ADD  CONSTRAINT [FK_Hospitals_HospitalServiceArea] FOREIGN KEY([HospitalServiceArea_Id])
REFERENCES [dbo].[Regions] ([Id])
ALTER TABLE [dbo].[Hospitals] CHECK CONSTRAINT [FK_Hospitals_HospitalServiceArea]

END
*/
--
--Eliminate duplicated Custom Regions by re-sequencing them
--

;With 
--Get all Duplicated, Custom RegionIds
Hospitals_Regions_CustomIds as (
Select ImportRegionId as DuplicatedRegionId from [dbo].[Hospitals_Regions] 
	WHERE RegionType = 'CustomRegion'
	group by ImportRegionId
	having count(*) > 1
)
--Create sequence numbers in each group of duplicated RegionIds 
--to exclude the first RegionIds from being resequenced
, Hospitals_Regions_WithGroupRegionSeq as (
	select *, Row_Number() Over (Partition by ImportRegionId Order by ImportRegionId) as GroupRegionSeq 
	from [dbo].[Hospitals_Regions] A join Hospitals_Regions_CustomIds B
	on A.ImportRegionId = B.DuplicatedRegionId
	Where RegionType = 'CustomRegion'
)
--Add a new RowNumber for the Duplicated, Custom RegionIds except the first one in each group
, Hospitals_Regions_WithRowNumber as (
	select *, Row_Number() Over (Order by Id) as CalcRowNumber 
	from Hospitals_Regions_WithGroupRegionSeq
	Where GroupRegionSeq <> 1
)
--Determine the top CustomRegionId to be used as seed in the creation of the new RegionIds
, Hospitals_Regions_TopCustomId as ( 
Select Max(ImportRegionId) as TopImportRegionId from [dbo].[Hospitals_Regions] WHERE RegionType = 'CustomRegion'
)
--Calculate the new ImportRegionId 
, Hospitals_Regions_NewImportRegionId as (
select *, (R.CalcRowNumber + TopImportRegionId) as NewImportRegionID from Hospitals_Regions_WithRowNumber as R, Hospitals_Regions_TopCustomId as T
)
Update HR
Set HR.ImportRegionId = nHR.NewImportRegionID
--SELECT HR.Id, nHR.Id, HR.ImportRegionId, nHR.newImportRegionId
FROM [dbo].[Hospitals_Regions] as HR
		Join
	 Hospitals_Regions_NewImportRegionId nHR
	 On HR.ID = nHR.Id
--
--  Create Temp Tables for 
--
SELECT h.*, bs.Abbreviation, bc.CountyFIPS INTO HospitalsTemp 
FROM Hospitals h 
	INNER JOIN	dbo.Base_States bs ON h.State_id = bs.Id
	INNER JOIN	dbo.Base_Counties bc ON h.County_Id = bc.Id	
WHERE IsArchived = 1 OR IsSourcedFromBaseData = 0 OR IsDeleted = 1

SELECT hr.RegionType,hr.Name, hr.IsSourcedFromBaseData, hr.Version, hr.ImportRegionId, bs.Abbreviation, hr.HospitalRegistry_id, hr.City 
INTO RegionTemp
FROM dbo.Hospitals_Regions hr 
	INNER JOIN [dbo].[Base_States] bs ON hr.State_Id = bs.Id

SELECT  * INTO Hospitals_HospitalCategoriesTemp FROM Hospitals_HospitalCategories

SELECT h.CmsProviderID, hhc.HospitalCategory_Id, c.Name, c.CategoryID
INTO Hospitals_HospitalHospitalCategoriesTemp
FROM dbo.Hospitals_HospitalHospitalCategories hhc
	INNER JOIN dbo.Hospitals h ON h.Id = hhc.Hospital_Id
	INNER JOIN dbo.Hospitals_HospitalCategories c ON c.Id = hhc.HospitalCategory_Id
WHERE h.IsDeleted = 0 AND h.CmsProviderID IS NOT NULL	
GROUP BY h.CmsProviderID, hhc.HospitalCategory_Id, c.Name, c.CategoryID 

SELECT wh.CCR, wh.Website_Id, wh.[Index], h.CmsProviderID
INTO Websites_WebsiteHospitalsTemp
FROM dbo.Hospitals h
	INNER JOIN dbo.Websites_WebsiteHospitals wh ON h.Id = wh.Hospital_Id
WHERE h.CmsProviderID IS NOT NULL AND h.IsDeleted = 0 
GROUP BY wh.CCR, wh.Website_Id, wh.[Index], h.CmsProviderID

DELETE FROM Hospitals_HospitalHospitalCategories
DELETE FROM Hospitals_HospitalCategories
DELETE FROM Websites_WebsiteHospitals
DELETE FROM Hospitals


IF OBJECT_ID(N'[FK14DF834170435D65]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK14DF834170435D65]
IF OBJECT_ID(N'[FK_Hospitals_HospitalServiceArea]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK_Hospitals_HospitalServiceArea]
IF OBJECT_ID(N'[FK_Hospitals_CustomRegion]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK_Hospitals_CustomRegion]
IF OBJECT_ID(N'[FK_Hospital_HealthReferralRegion]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [FK_Hospital_HealthReferralRegion]
IF OBJECT_ID(N'[DF__Hospitals__IsArc__3BFFE745]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__IsArc__3BFFE745]
IF OBJECT_ID(N'[DF__Hospitals__IsDel__3B0BC30C]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__IsDel__3B0BC30C]
IF OBJECT_ID(N'[DF__Hospitals__Force__3A179ED3]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Force__3A179ED3]
IF OBJECT_ID(N'[DF__Hospitals__Diagn__39237A9A]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Diagn__39237A9A]
IF OBJECT_ID(N'[DF__Hospitals__Pharm__382F5661]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Pharm__382F5661]
IF OBJECT_ID(N'[DF__Hospitals__Cardi__373B3228]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Cardi__373B3228]
IF OBJECT_ID(N'[DF__Hospitals__Pedia__36470DEF]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Pedia__36470DEF]
IF OBJECT_ID(N'[DF__Hospitals__Pedia__3552E9B6]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Pedia__3552E9B6]
IF OBJECT_ID(N'[DF__Hospitals__Urgen__345EC57D]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Urgen__345EC57D]
IF OBJECT_ID(N'[DF__Hospitals__Traum__336AA144]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Traum__336AA144]
IF OBJECT_ID(N'[DF__Hospitals__Emerg__32767D0B]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Emerg__32767D0B]
IF OBJECT_ID(N'[DF__Hospitals__Medic__318258D2]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__Medic__318258D2]
IF OBJECT_ID(N'[DF__Hospitals__IsSou__308E3499]') IS NOT NULL	
	ALTER TABLE [dbo].[Hospitals] DROP CONSTRAINT [DF__Hospitals__IsSou__308E3499]

IF	OBJECT_ID(N'FK8FDFC394F0DC9B9A') IS NOT NULL
	ALTER TABLE [dbo].[Hospitals_HospitalHospitalCategories] DROP CONSTRAINT FK8FDFC394F0DC9B9A


DROP TABLE [dbo].[Websites_WebsiteHospitals]
DROP TABLE [dbo].[Hospitals]


CREATE TABLE [dbo].[Websites_WebsiteHospitals](
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[CCR] [NVARCHAR](255) NULL,
	[Hospital_Id] [INT] NOT NULL,
	[Website_Id] [INT] NULL,
	[INDEX] [INT] DEFAULT(1)
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Hospitals](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[IsSourcedFromBaseData] [bit] NOT NULL,
	[Version] [decimal](19, 5) NULL,
	[CCR] [decimal](19, 4) NULL,
	[CmsProviderID] [nvarchar](10) NULL,
	[LocalHospitalId] [nvarchar](20) NULL,
	[Address] [nvarchar](100) NULL,
	[City] [nvarchar](100) NULL,
	[Zip] [nvarchar](12) NULL,
	[Description] [nvarchar](1000) NULL,
	[Employees] [int] NULL,
	[TotalBeds] [int] NULL,
	[HospitalOwnership] [nvarchar](100) NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[FaxNumber] [nvarchar](50) NULL,
	[MedicareMedicaidProvider] [bit] NULL,
	[EmergencyService] [bit] NULL,
	[TraumaService] [bit] NULL,
	[UrgentCareService] [bit] NULL,
	[PediatricService] [bit] NULL,
	[PediatricICUService] [bit] NULL,
	[CardiacCatherizationService] [bit] NULL,
	[PharmacyService] [bit] NULL,
	[DiagnosticXRayService] [bit] NULL,
	[ForceLocalHospitalIdValidation] [bit] NULL,
	[IsDeleted] [bit] NULL,
	[LinkedHospitalId] [int] NULL,
	[IsArchived] [bit] NULL,
	[ArchiveDate] [datetime] NULL,
	[State] [nvarchar](10) NULL,
	[County] [nvarchar](10) NULL,
	[HospitalServiceArea_Id] [int] NULL,
	[HealthReferralRegion_Id] [int] NULL,
	[CustomRegion_Id] [int] NULL,
	[Registry_Id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [IsSourcedFromBaseData]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [MedicareMedicaidProvider]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [EmergencyService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [TraumaService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [UrgentCareService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [PediatricService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [PediatricICUService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [CardiacCatherizationService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [PharmacyService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [DiagnosticXRayService]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [ForceLocalHospitalIdValidation]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [IsDeleted]
ALTER TABLE [dbo].[Hospitals] ADD  DEFAULT ((0)) FOR [IsArchived]
ALTER TABLE [dbo].[Hospitals]  WITH CHECK ADD  CONSTRAINT [FK_Hospital_HealthReferralRegion] FOREIGN KEY([HealthReferralRegion_Id])
REFERENCES [dbo].[Regions] ([Id])
ALTER TABLE [dbo].[Hospitals] CHECK CONSTRAINT [FK_Hospital_HealthReferralRegion]
ALTER TABLE [dbo].[Hospitals]  WITH CHECK ADD  CONSTRAINT [FK_Hospitals_CustomRegion] FOREIGN KEY([CustomRegion_Id])
REFERENCES [dbo].[Regions] ([Id])
ALTER TABLE [dbo].[Hospitals] CHECK CONSTRAINT [FK_Hospitals_CustomRegion]
ALTER TABLE [dbo].[Hospitals]  WITH CHECK ADD  CONSTRAINT [FK_Hospitals_HospitalServiceArea] FOREIGN KEY([HospitalServiceArea_Id])
REFERENCES [dbo].[Regions] ([Id])
ALTER TABLE [dbo].[Hospitals] CHECK CONSTRAINT [FK_Hospitals_HospitalServiceArea]
ALTER TABLE [dbo].[Hospitals]  WITH CHECK ADD  CONSTRAINT [FK14DF834170435D65] FOREIGN KEY([Registry_Id])
REFERENCES [dbo].[Hospitals_HospitalRegistries] ([Id])
ALTER TABLE [dbo].[Hospitals] CHECK CONSTRAINT [FK14DF834170435D65]

ALTER TABLE [dbo].[Websites_WebsiteHospitals]  WITH CHECK ADD  CONSTRAINT [FK8BE74CF5DF7F29CF] FOREIGN KEY([Website_Id])
REFERENCES [dbo].[Websites] ([Id])
ALTER TABLE [dbo].[Websites_WebsiteHospitals] CHECK CONSTRAINT [FK8BE74CF5DF7F29CF]
ALTER TABLE [dbo].[Websites_WebsiteHospitals]  WITH CHECK ADD  CONSTRAINT [FK8BE74CF5F0DC9B9A] FOREIGN KEY([Hospital_Id])
REFERENCES [dbo].[Hospitals] ([Id])
ALTER TABLE [dbo].[Websites_WebsiteHospitals] CHECK CONSTRAINT [FK8BE74CF5F0DC9B9A]


IF OBJECT_ID(N'Base_Counties') IS NOT NULL
	DROP TABLE Base_Counties
IF OBJECT_ID(N'Base_ZipCodeToHRRAndHSAs') IS NOT NULL
	DROP TABLE Base_ZipCodeToHRRAndHSAs
IF OBJECT_ID(N'FKF63826C1F8B2387A') IS NOT NULL
	ALTER TABLE Hospitals_Regions DROP CONSTRAINT FKF63826C1F8B2387A
IF OBJECT_ID(N'Base_States') IS NOT NULL
	DROP TABLE Base_States

CREATE TABLE [dbo].[Base_States](
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[Name] [NVARCHAR](255) NOT NULL,
	[FIPSState] [NVARCHAR](255) NULL,
	[Abbreviation] [NVARCHAR](255) NULL,
	[MinX] [FLOAT] NULL,
	[MinY] [FLOAT] NULL,
	[MaxY] [FLOAT] NULL,
	[MaxX] [FLOAT] NULL,
	[X0] [FLOAT] NULL,
	[Y0] [FLOAT] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Base_ZipCodeToHRRAndHSAs](
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[Zip] [NVARCHAR](5) NULL,
	[HRRNumber] [INT] NULL,
	[HSANumber] [INT] NULL,
	[State] [NVARCHAR](5) NOT NULL,
	[StateFIPS] [NVARCHAR](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Base_Counties](
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[Name] [NVARCHAR](250) NULL,
	[CountyFIPS] [NVARCHAR](5) NULL,
	[CountySSA] [NVARCHAR](3) NULL,
	[State] [NVARCHAR](5) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]




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