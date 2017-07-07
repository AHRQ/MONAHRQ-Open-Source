 BEGIN TRY	
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Base_Counties' and column_name = 'CountySSA' )
		ALTER TABLE Base_Counties ADD CountySSA nvarchar(3) NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Base_Counties' and column_name = 'State' )
		ALTER TABLE Base_Counties ADD State nvarchar(5) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Hospitals' and column_name = 'State' )
		ALTER TABLE Hospitals ADD State nvarchar(10) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Hospitals' and column_name = 'County' )
		ALTER TABLE Hospitals ADD County nvarchar(10) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Hospitals_HospitalCategories' and column_name = 'Category_Id' )
		ALTER TABLE Hospitals_HospitalCategories ADD Category_Id int DEFAULT(1) 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Measures' and column_name = 'ClassType' )
		ALTER TABLE Measures ADD ClassType nvarchar(20)   
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Measures' and column_name = 'UsedInCalculations' )
		ALTER TABLE Measures ADD UsedInCalculations BIT NULL DEFAULT(0)
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports' and column_name = 'ReportType' )
		ALTER TABLE Reports ADD ReportType nvarchar(255)   
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports' and column_name = 'IsCustom' )
		ALTER TABLE Reports ADD IsCustom BIT NOT NULL DEFAULT(0)
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports' and column_name = 'IsTrending' )
		ALTER TABLE Reports ADD IsTrending BIT NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports' and column_name = 'ReportSql' )
		ALTER TABLE Reports ADD	ReportSql NVARCHAR(max) NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports_ComparisonKeyIconSets' and column_name = 'IsIncluded' )
		ALTER TABLE Reports_ComparisonKeyIconSets ADD IsIncluded bit DEFAULT(1) NOT NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports_ComparisonKeyIconSets' and column_name = 'INDEX' )
		ALTER TABLE Reports_ComparisonKeyIconSets ADD [INDEX] INT NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports_ReportColumns' and column_name = 'IsMeasure' )
		ALTER TABLE Reports_ReportColumns ADD IsMeasure BIT NOT NULL DEFAULT(0)
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports_ReportColumns' and column_name = 'IsIncluded' )
		ALTER TABLE Reports_ReportColumns ADD IsIncluded BIT DEFAULT(1) NOT NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports_ReportColumns' and column_name = 'Index' )
		ALTER TABLE Reports_ReportColumns ADD [Index] INT NULL DEFAULT(1)
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Reports_ReportColumns' and column_name = 'MeasureCode' )
		ALTER TABLE Reports_ReportColumns ADD [MeasureCode] VARCHAR(20) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='TopicCategories' and column_name = 'TopicType' )
		ALTER TABLE TopicCategories ADD TopicType nvarchar(25) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Websites' and column_name = 'StateContext' )
		ALTER TABLE Websites ADD StateContext nvarchar(max) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Websites' and column_name = 'RegionTypeContext' )
		ALTER TABLE Websites ADD RegionTypeContext nvarchar(150) NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Websites_WebsiteReports' and column_name = 'SelectedYears' )
		ALTER TABLE Websites_WebsiteReports ADD SelectedYears nvarchar(255) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Websites_WebsiteReports' and column_name = 'DefaultSelectedYear' )
		ALTER TABLE Websites_WebsiteReports ADD DefaultSelectedYear nvarchar(20) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Wings_Elements' and column_name = 'Type' )
		ALTER TABLE Wings_Elements ADD Type nvarchar(50) NULL 
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Wings_Scopes' and column_name = 'IsCustom' )
		ALTER TABLE Wings_Scopes ADD IsCustom bit NOT NULL DEFAULT(0)
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='SchemaVersions' and column_name = 'Month' )
		ALTER TABLE SchemaVersions ADD [Month] INT NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='SchemaVersions' and column_name = 'Year' )
		ALTER TABLE SchemaVersions ADD [Year] INT NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='SchemaVersions' and column_name = 'VersionType' )
		ALTER TABLE SchemaVersions ADD [VersionType] INT NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='SchemaVersions' and column_name = 'FileName' )
		ALTER TABLE SchemaVersions ADD [FileName] NVARCHAR(255) NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='SchemaVersions' and column_name = 'Major' )
		ALTER TABLE SchemaVersions ADD [Major] INT NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='SchemaVersions' and column_name = 'Minor' )
		ALTER TABLE SchemaVersions ADD [Minor] INT NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='SchemaVersions' and column_name = 'Milestone' )
		ALTER TABLE SchemaVersions ADD [Milestone] INT NULL
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Websites' and column_name = 'Description' )
		Alter Table Websites Alter Column [Description] nvarchar(2000) NULL;
		
	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Targets_HospitalCompareTargets' and column_name = 'Dataset_Id' )	
		begin 
			ALTER TABLE [dbo].[Targets_HospitalCompareTargets] ADD [Dataset_Id] INT NULL;
			ALTER TABLE [dbo].[Targets_HospitalCompareTargets] ADD CONSTRAINT [FK_TARGETS_HospitalCompareTargets_Datasets] FOREIGN KEY (Dataset_id)  REFERENCES [dbo].[Wings_Datasets](Id)
		end

	If not exists(select * from INFORMATION_SCHEMA.Columns where table_name ='Targets_HospitalCompareFootnotes' and column_name = 'Dataset_Id' )
		BEGIN
			ALTER TABLE [dbo].[Targets_HospitalCompareFootnotes] ADD [Dataset_Id] INT NULL;
			ALTER TABLE [dbo].[Targets_HospitalCompareFootnotes] ADD CONSTRAINT [FK_TARGETS_HospitalCompareFootnotes_Datasets] FOREIGN KEY (Dataset_id) REFERENCES [dbo].[Wings_Datasets](Id)
		END
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_NursingHomes_Cateorigies'  )
		BEGIN 
			ALTER TABLE [dbo].[NursingHomes]  WITH CHECK ADD  CONSTRAINT [FK_NursingHomes_Cateorigies] FOREIGN KEY([CategoryType_Id]) REFERENCES [dbo].[Categories] ([Id])
			ALTER TABLE [dbo].[NursingHomes] CHECK CONSTRAINT [FK_NursingHomes_Cateorigies]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_TARGETS_NursingHomeTargets_Datasets'  )
		begin 
			ALTER TABLE [dbo].[Targets_NursingHomeTargets]  WITH CHECK ADD  CONSTRAINT [FK_TARGETS_NursingHomeTargets_Datasets] FOREIGN KEY([Dataset_Id]) REFERENCES [dbo].[Wings_Datasets] ([Id])
			ALTER TABLE [dbo].[Targets_NursingHomeTargets] CHECK CONSTRAINT [FK_TARGETS_NursingHomeTargets_Datasets]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_TARGETS_PhysicianTargets_Datasets'  )
		begin 
			ALTER TABLE [dbo].[Targets_PhysicianTargets]  WITH CHECK ADD  CONSTRAINT [FK_TARGETS_PhysicianTargets_Datasets] FOREIGN KEY([Dataset_Id]) REFERENCES [dbo].[Wings_Datasets] ([Id])
			ALTER TABLE [dbo].[Targets_PhysicianTargets] CHECK CONSTRAINT [FK_TARGETS_PhysicianTargets_Datasets]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_PAH_Physicians' )
		begin 
			ALTER TABLE [dbo].[Physicians_AffiliatedHospitals]  WITH CHECK ADD  CONSTRAINT [FK_PAH_Physicians] FOREIGN KEY([Physician_Id]) REFERENCES [dbo].[Physicians] ([Id])
			ALTER TABLE [dbo].[Physicians_AffiliatedHospitals] CHECK CONSTRAINT [FK_PAH_Physicians]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_PMP_Physicians' )
		begin 
			ALTER TABLE [dbo].[Physicians_MedicalPractices]  WITH CHECK ADD  CONSTRAINT [FK_PMP_Physicians] FOREIGN KEY([Physician_Id]) REFERENCES [dbo].[Physicians] ([Id])
			ALTER TABLE [dbo].[Physicians_MedicalPractices] CHECK CONSTRAINT [FK_PMP_Physicians]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_PMP_MedicalPractices' )
		begin 
			ALTER TABLE [dbo].[Physicians_MedicalPractices]  WITH CHECK ADD  CONSTRAINT [FK_PMP_MedicalPractices] FOREIGN KEY([MedicalPractice_Id]) REFERENCES [dbo].[MedicalPractices] ([Id])
			ALTER TABLE [dbo].[Physicians_MedicalPractices] CHECK CONSTRAINT [FK_PMP_MedicalPractices]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_Addresses_MedicalPractices' )
		begin 
			ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_MedicalPractices] FOREIGN KEY([MedicalPractice_Id]) REFERENCES [dbo].[MedicalPractices] ([Id])
			ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_MedicalPractices]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_Addresses_Physicians' )
		begin 
			ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_Physicians] FOREIGN KEY([Physician_Id]) REFERENCES [dbo].[Physicians] ([Id])
			ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_Physicians]
		end
	
	If not exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS where CONSTRAINT_NAME ='FK_Categories_HospitalRegistries' )
		begin 
			ALTER TABLE [dbo].[Categories]  WITH NOCHECK ADD  CONSTRAINT [FK_Categories_HospitalRegistries] FOREIGN KEY([Registry_Id]) REFERENCES [dbo].[Hospitals_HospitalRegistries] ([Id])
			ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [FK_Categories_HospitalRegistries]
		end

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
               @ErrorState); -- State.
END CATCH