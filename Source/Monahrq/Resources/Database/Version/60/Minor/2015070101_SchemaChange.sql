 BEGIN TRY	

		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Hospitals' AND COLUMN_NAME = 'Latitude') 
			ALTER TABLE Hospitals ADD Latitude float
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Hospitals' AND COLUMN_NAME = 'Longitude') 
			ALTER TABLE Hospitals ADD Longitude float
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Measures' AND COLUMN_NAME = 'SupportsCost') 
			ALTER TABLE Measures ADD SupportsCost bit DEFAULT ((0))
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Measures' AND COLUMN_NAME = 'ConsumerDescription') 
			ALTER TABLE Measures ADD ConsumerDescription nvarchar(MAX) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Measures' AND COLUMN_NAME = 'ConsumerPlainTitle') 
			ALTER TABLE Measures ADD ConsumerPlainTitle nvarchar(MAX) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Temp_Quality' AND COLUMN_NAME = 'Id') 
			BEGIN 
				ALTER TABLE Temp_Quality ADD Id INT IDENTITY(1,1)
				ALTER TABLE Temp_Quality ADD PRIMARY KEY (Id)
			END
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Temp_Quality' AND COLUMN_NAME = 'Name') 
			ALTER TABLE Temp_Quality ADD Name nvarchar(255) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'TopicCategories' AND COLUMN_NAME = 'WingTargetName') 
			ALTER TABLE TopicCategories ADD WingTargetName nvarchar(255) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'TopicCategories' AND COLUMN_NAME = 'CategoryType') 
			ALTER TABLE TopicCategories ADD CategoryType nvarchar(20) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'TopicCategories' AND COLUMN_NAME = 'IsUserCreated') 
			ALTER TABLE TopicCategories ADD IsUserCreated bit DEFAULT ((0))
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'TopicCategories' AND COLUMN_NAME = 'ConsumerDescription') 
			ALTER TABLE TopicCategories ADD ConsumerDescription nvarchar(MAX) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Topics' AND COLUMN_NAME = 'WingTargetName') 
			ALTER TABLE Topics ADD WingTargetName nvarchar(255) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Topics' AND COLUMN_NAME = 'IsUserCreated') 
			ALTER TABLE Topics ADD IsUserCreated bit DEFAULT ((0))
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Topics' AND COLUMN_NAME = 'ConsumerLongTitle') 
			ALTER TABLE Topics ADD ConsumerLongTitle nvarchar(MAX) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Websites' AND COLUMN_NAME = 'DefaultAudience') 
			ALTER TABLE Websites ADD DefaultAudience nvarchar(255) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Websites' AND COLUMN_NAME = 'Audiences') 
			ALTER TABLE Websites ADD Audiences nvarchar(255) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Websites' AND COLUMN_NAME = 'BannerName') 
			ALTER TABLE Websites ADD BannerName nvarchar(50) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Wings_Targets' AND COLUMN_NAME = 'TemplateFileName') 
			ALTER TABLE Wings_Targets ADD TemplateFileName nvarchar(256) NULL
		IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME = 'Wings_Targets' AND COLUMN_NAME = 'ImportType') 
			ALTER TABLE Wings_Targets ADD ImportType nvarchar(50) NULL
		
		IF(Object_ID(N'MeasureTopicTemp')) IS NOT NULL 
			DROP TABLE MeasureTopicTemp
		
		CREATE TABLE [dbo].[MeasureTopicTemp](
			[MeasureName] [nvarchar](255) NOT NULL,
			[TopicName] [nvarchar](255) NOT NULL,
			[TopicCategoryName] [nvarchar](255) NOT NULL
		) ON [PRIMARY]
		
		
		IF(Object_ID(N'TopicToTopicCategoryTemp')) IS NOT NULL 
			DROP TABLE TopicToTopicCategoryTemp
		
		CREATE TABLE [dbo].[TopicToTopicCategoryTemp](
			[TopicCategoryName] [nvarchar](255) NOT NULL,
			[TopicName] [nvarchar](255) NOT NULL
		) ON [PRIMARY]


		IF(Object_ID(N'TopicTemp')) IS NOT NULL 
			DROP TABLE TopicTemp
		
		CREATE TABLE [dbo].[TopicTemp](
			[Id] [int] NOT NULL identity(1,1),
			[Name] [nvarchar](255) NOT NULL,
			[LongTitle] [nvarchar](max) NULL,
			[Description] [nvarchar](max) NULL,
			[WingTargetName] [nvarchar](255) NULL,
			[IsUserCreated] [bit] NOT NULL,
			[ConsumerLongTitle] [nvarchar](max) NULL,
			[TopicCategory_id] [int] NULL,
			[TopicCategoryName] [nvarchar](255) NOT NULL
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


		IF(OBJECT_ID(N'MeasureWingTargetTemp')) IS NOT NULL 
			DROP TABLE MeasureWingTargetTemp
		
		CREATE TABLE [dbo].[MeasureWingTargetTemp](
			[MeasureName] [nvarchar](255) NOT NULL,
			[TargetName] [nvarchar](255) NOT NULL
		) ON [PRIMARY]
		

		IF(OBJECT_ID(N'BaseCostToChargesTemp')) IS NOT NULL 
			DROP TABLE BaseCostToChargesTemp
		
		CREATE TABLE [dbo].[BaseCostToChargesTemp](
			[ProviderID] nvarchar(max), 
			[Ratio] nvarchar(max), 
			[Year] nvarchar(max)
		) ON [PRIMARY]


		IF(OBJECT_ID(N'BaseMSDRGsTemp')) IS NOT NULL 
			DROP TABLE BaseMSDRGsTemp
		
		CREATE TABLE [dbo].[BaseMSDRGsTemp](
			[Description] NVARCHAR(255), 
			FirstYear INT, 
			LastYear INT,
			[Version] DECIMAL (19,5),
			MDCID INT,
			MSDRGID INT
		) ON [PRIMARY]
		

END TRY	
BEGIN CATCH
		DECLARE @ErrorMessage VARCHAR(5000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
		       @ErrorSeverity = ERROR_SEVERITY(),
		       @ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage,  @ErrorSeverity, @ErrorState); 
END CATCH