 BEGIN TRY	
 
-- Creating New tables
 
IF(OBJECT_ID(N'Base_NHProviderToLatLongs')) IS NULL
	CREATE TABLE [dbo].[Base_NHProviderToLatLongs]
	(
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[ProviderId] [NVARCHAR](8) NOT NULL,
		[Latitude] [FLOAT] NOT NULL,
		[Longitude] [FLOAT] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


IF(OBJECT_ID(N'Wings_Datasets')) IS NULL
	CREATE TABLE [dbo].[Wings_Datasets]
	(
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[SummaryData] [NVARCHAR](MAX) NULL,
		[File] [NVARCHAR](255) NULL,
		[Description] [NVARCHAR](255) NULL,
		[DateImported] [DATETIME] NULL,
		[ReportingQuarter] [NVARCHAR](20) NULL,
		[ReportingYear] [NVARCHAR](20) NULL,
		[ProviderStates] [NVARCHAR](50) NULL,
		[ProviderUseRealtime] [BIT] DEFAULT(0) NULL,
		[DRGMDCMappingStatus] [NVARCHAR](255) NULL,
		[DRGMDCMappingStatusMessage] [NVARCHAR](500) NULL,
		[IsFinished] [BIT] NULL,
		[ContentType_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	

IF(OBJECT_ID(N'Wings_DatasetTransactionRecords')) IS NULL
	CREATE TABLE [dbo].[Wings_DatasetTransactionRecords]
	(
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Code] [INT] NULL,
		[Message] [NVARCHAR](255) NULL,
		[Extension] [INT] NULL,
		[Data] [NVARCHAR](MAX) NULL,
		[Dataset_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


IF(OBJECT_ID(N'Wings_DatasetVersions')) IS NULL
	CREATE TABLE [dbo].[Wings_DatasetVersions]
	(
			[Id] [INT] IDENTITY(1,1) NOT NULL,
			[Number] [INT] NULL,
			[Published] [BIT] NULL,
			[Latest] [BIT] NULL,
			[Dataset_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Base_ProviderSpecialities')) IS NULL
	CREATE TABLE [dbo].[Base_ProviderSpecialities](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Name] [NVARCHAR](256) NOT NULL,
		[ProviderTaxonomy] [NVARCHAR](2000) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[Name] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[Name] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	

IF(OBJECT_ID(N'Categories')) IS NULL
	CREATE TABLE [dbo].[Categories]
	(
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[CategoryType] [NVARCHAR](50) NOT NULL,
		[Name] [NVARCHAR](255) NOT NULL,
		[CategoryID] [INT] NULL,
		[Version] [DECIMAL](19, 5) NULL,
		[IsSourcedFromBaseData] [BIT] NOT NULL,
		[Registry_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Regions')) IS NULL
	CREATE TABLE [dbo].[Regions]
	(
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[RegionType] [NVARCHAR](255) NOT NULL,
		[Name] [NVARCHAR](255) NOT NULL,
		[IsSourcedFromBaseData] [BIT] NOT NULL,
		[Version] [DECIMAL](19, 5) NULL,
		[Code] [NVARCHAR](15) NOT NULL,
		[ImportRegionId] [INT] NULL,
		[State] [NVARCHAR](3) NULL,
		[HospitalRegistry_id] [INT] NULL,
		[City] [NVARCHAR](255) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[Code] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[Code] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'RegionPopulationStrats')) IS NULL
	CREATE TABLE [dbo].[RegionPopulationStrats]
	(
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[RegionType] [INT] NULL,
		[RegionID] [INT] NULL,
		[CatID] [INT] NULL,
		[CatVal] [INT] NULL,
		[Year] [INT] NULL,
		[Population] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


IF(OBJECT_ID(N'Reports_Filters')) IS NULL
	CREATE TABLE [dbo].[Reports_Filters](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Name] [NVARCHAR](256) NOT NULL,
		[FilterType] [NVARCHAR](30) NULL,
		[Value] [BIT] NOT NULL,
		[IsRadioButton] [BIT] NOT NULL,
		[RadioGroupName] [NVARCHAR](150) NULL,
		[Report_id] [INT] NULL,
		[Index] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
IF(OBJECT_ID(N'Websites_WebsiteNursingHomes')) IS NULL
	CREATE TABLE [dbo].[Websites_WebsiteNursingHomes](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[NursingHome_ProviderId] [NVARCHAR](10) NOT NULL,
		[Website_Id] [INT] NULL,
		[Index] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Base_ConsumerPriceIndices')) IS NULL
	CREATE TABLE [dbo].[Base_ConsumerPriceIndices]
	(
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Year] [INT] NULL,
		[Value] [REAL] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Base_HospitalTraumaLevels')) IS NULL
	CREATE TABLE [dbo].[Base_HospitalTraumaLevels](
		[Id] [INT] NOT NULL,
		[Name] [NVARCHAR](255) NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[Name] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

--New 6.0 Objects
IF(OBJECT_ID(N'NursingHomes')) IS NULL
	CREATE TABLE [dbo].[NursingHomes](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Name] [NVARCHAR](255) NOT NULL,
		[LegalBusinessName] [NVARCHAR](150) NULL,
		[ProviderId] [NVARCHAR](10) NULL,
		[Address] [NVARCHAR](250) NULL,
		[City] [NVARCHAR](100) NULL,
		[State] [NVARCHAR](3) NOT NULL,
		[Zip] [NVARCHAR](12) NULL,
		[CountySSA] [NVARCHAR](5) NULL,
		[CountyName] [NVARCHAR](150) NULL,
		[Phone] [NVARCHAR](15) NULL,
		[NumberCertBeds] [INT] NULL,
		[NumberResidCertBeds] [INT] NULL,
		[ParticipationDate] [DATETIME] NULL,
		[Certification] [NVARCHAR](255) NULL,
		[ResFamCouncil] [NVARCHAR](50) NULL,
		[SprinklerStatus] [NVARCHAR](50) NULL,
		[InHospital] [BIT] NULL,
		[IsCCRCFacility] [BIT] NOT NULL DEFAULT(0),
		[IsSFFacility] [BIT] NOT NULL DEFAULT(0),
		[Description] [NVARCHAR](1000) NULL,
		[Accreditation] [NVARCHAR](200) NULL,
		[Ownership] [NVARCHAR](200) NULL,
		[InRetirementCommunity] [BIT] NULL,
		[HasSpecialFocus] [BIT] NULL,
		[ChangedOwnership_12Mos] [BIT] NOT NULL DEFAULT(0),
		[IsDeleted] [BIT] NOT NULL DEFAULT(0),
		[FileDate] [DATETIME] NULL,
		[CategoryType_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'NursingHomeMeasureCodeMapping')) IS NULL
	CREATE TABLE [dbo].[NursingHomeMeasureCodeMapping](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[AccessCode] [NVARCHAR](50) NOT NULL,
		[MonahrqCode] [NVARCHAR](50) NOT NULL,
	 CONSTRAINT [PK_NursingHomeMeasureCodeMapping] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'NursingHomes_Audits')) IS NULL
	CREATE TABLE [dbo].[NursingHomes_Audits](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Action] [NVARCHAR](20) NOT NULL,
		[EntityTypeName] [NVARCHAR](50) NOT NULL,
		[Owner_Id] [INT] NOT NULL,
		[PropertyName] [NVARCHAR](150) NULL,
		[OldPropertyValue] [NVARCHAR](500) NULL,
		[NewPropertyValue] [NVARCHAR](500) NULL,
		[CreateDate] [DATETIME] NOT NULL,
		[ProviderId] [NVARCHAR](255) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Targets_NursingHomeTargets')) IS NULL
	CREATE TABLE [dbo].[Targets_NursingHomeTargets](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[ProviderId] [NVARCHAR](12) NULL,
		[OverallRating] [INT] NULL,
		[SurveyRating] [INT] NULL,
		[QualityRating] [INT] NULL,
		[StaffingRating] [INT] NULL,
		[SurveyScore] [NVARCHAR](12) NULL,
		[QM401Score] [DECIMAL](19, 1) NULL,
		[QM402Score] [DECIMAL](19, 1) NULL,
		[QM403Score] [DECIMAL](19, 1) NULL,
		[QM404Score] [DECIMAL](19, 1) NULL,
		[QM405Score] [DECIMAL](19, 1) NULL,
		[QM406Score] [DECIMAL](19, 1) NULL,
		[QM407Score] [DECIMAL](19, 1) NULL,
		[QM408Score] [DECIMAL](19, 1) NULL,
		[QM409Score] [DECIMAL](19, 1) NULL,
		[QM410Score] [DECIMAL](19, 1) NULL,
		[QM411Score] [DECIMAL](19, 1) NULL,
		[QM415Score] [DECIMAL](19, 1) NULL,
		[QM419Score] [DECIMAL](19, 1) NULL,
		[QM424Score] [DECIMAL](19, 1) NULL,
		[QM425Score] [DECIMAL](19, 1) NULL,
		[QM426Score] [DECIMAL](19, 1) NULL,
		[QM430Score] [DECIMAL](19, 1) NULL,
		[QM434Score] [DECIMAL](19, 1) NULL,
		[QM401State] [DECIMAL](19, 1) NULL,
		[QM402State] [DECIMAL](19, 1) NULL,
		[QM403State] [DECIMAL](19, 1) NULL,
		[QM404State] [DECIMAL](19, 1) NULL,
		[QM405State] [DECIMAL](19, 1) NULL,
		[QM406State] [DECIMAL](19, 1) NULL,
		[QM407State] [DECIMAL](19, 1) NULL,
		[QM408State] [DECIMAL](19, 1) NULL,
		[QM409State] [DECIMAL](19, 1) NULL,
		[QM410State] [DECIMAL](19, 1) NULL,
		[QM411State] [DECIMAL](19, 1) NULL,
		[QM415State] [DECIMAL](19, 1) NULL,
		[QM419State] [DECIMAL](19, 1) NULL,
		[QM424State] [DECIMAL](19, 1) NULL,
		[QM425State] [DECIMAL](19, 1) NULL,
		[QM426State] [DECIMAL](19, 1) NULL,
		[QM430State] [DECIMAL](19, 1) NULL,
		[QM434State] [DECIMAL](19, 1) NULL,
		[QM401Nation] [DECIMAL](19, 1) NULL,
		[QM402Nation] [DECIMAL](19, 1) NULL,
		[QM403Nation] [DECIMAL](19, 1) NULL,
		[QM404Nation] [DECIMAL](19, 1) NULL,
		[QM405Nation] [DECIMAL](19, 1) NULL,
		[QM406Nation] [DECIMAL](19, 1) NULL,
		[QM407Nation] [DECIMAL](19, 1) NULL,
		[QM408Nation] [DECIMAL](19, 1) NULL,
		[QM409Nation] [DECIMAL](19, 1) NULL,
		[QM410Nation] [DECIMAL](19, 1) NULL,
		[QM411Nation] [DECIMAL](19, 1) NULL,
		[QM415Nation] [DECIMAL](19, 1) NULL,
		[QM419Nation] [DECIMAL](19, 1) NULL,
		[QM424Nation] [DECIMAL](19, 1) NULL,
		[QM425Nation] [DECIMAL](19, 1) NULL,
		[QM426Nation] [DECIMAL](19, 1) NULL,
		[QM430Nation] [DECIMAL](19, 1) NULL,
		[QM434Nation] [DECIMAL](19, 1) NULL,
		[StaffingScore] [NVARCHAR](12) NULL,
		[MostSevereHealthDeficiency] [NVARCHAR](12) NULL,
		[MostSevereFireSafetyDeficiency] [NVARCHAR](12) NULL,
		[FileDate] [DATETIME] NULL,
		[ReportedCNAStaffingHoursperResidentperDay] [NVARCHAR](12) NULL,
		[ReportedLPNStaffingHoursperResidentperDay] [NVARCHAR](12) NULL,
		[ReportedRNStaffingHoursperResidentperDay] [NVARCHAR](12) NULL,
		[LicensedStaffingHoursperResidentperDay] [NVARCHAR](12) NULL,
		[TotalNurseStaffingHoursperResidentperDay] [NVARCHAR](12) NULL,
		[PhysicalTherapistStaffingHoursperResidentPerDay] [NVARCHAR](12) NULL,
		[HealthSurveyDate] [DATETIME] NULL,
		[FireSafetySurveyDate] [DATETIME] NULL,
		[TotalHealthDeficiencies] [DECIMAL](19, 1) NULL,
		[TotalFireSafetyDeficiencies] [DECIMAL](19, 1) NULL,
		[ReportedCNAStaffingHoursperResidentperDayState] [NVARCHAR](12) NULL,
		[ReportedLPNStaffingHoursperResidentperDayState] [NVARCHAR](12) NULL,
		[ReportedRNStaffingHoursperResidentperDayState] [NVARCHAR](12) NULL,
		[LicensedStaffingHoursperResidentperDayState] [NVARCHAR](12) NULL,
		[TotalNurseStaffingHoursperResidentperDayState] [NVARCHAR](12) NULL,
		[PhysicalTherapistStaffingHoursperResidentPerDayState] [NVARCHAR](12) NULL,
		[HealthSurveyDateState] [DATETIME] NULL,
		[FireSafetySurveyDateState] [DATETIME] NULL,
		[TotalHealthDeficienciesState] [DECIMAL](19, 1) NULL,
		[TotalFireSafetyDeficienciesState] [DECIMAL](19, 1) NULL,
		[ReportedCNAStaffingHoursperResidentperDayNation] [NVARCHAR](12) NULL,
		[ReportedLPNStaffingHoursperResidentperDayNation] [NVARCHAR](12) NULL,
		[ReportedRNStaffingHoursperResidentperDayNation] [NVARCHAR](12) NULL,
		[LicensedStaffingHoursperResidentperDayNation] [NVARCHAR](12) NULL,
		[TotalNurseStaffingHoursperResidentperDayNation] [NVARCHAR](12) NULL,
		[PhysicalTherapistStaffingHoursperResidentPerDayNation] [NVARCHAR](12) NULL,
		[HealthSurveyDateNation] [DATETIME] NULL,
		[FireSafetySurveyDateNation] [DATETIME] NULL,
		[TotalHealthDeficienciesNation] [DECIMAL](19, 1) NULL,
		[TotalFireSafetyDeficienciesNation] [DECIMAL](19, 1) NULL,
		[Dataset_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Targets_PhysicianTargets')) IS NULL
	CREATE TABLE [dbo].[Targets_PhysicianTargets](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Npi] [BIGINT] NOT NULL,
		[PacId] [NVARCHAR](17) NULL,
		[ProfEnrollId] [NVARCHAR](20) NULL,
		[FirstName] [NVARCHAR](50) NULL,
		[MiddleName] [NVARCHAR](50) NULL,
		[LastName] [NVARCHAR](50) NULL,
		[Suffix] [NVARCHAR](20) NULL,
		[Gender] [NVARCHAR](10) NULL,
		[Credential] [NVARCHAR](255) NULL,
		[MedicalSchoolName] [NVARCHAR](255) NULL,
		[GraduationYear] [INT] NULL,
		[CouncilBoardCertification] [BIT] NULL DEFAULT (0),
		[PrimarySpecialty] [NVARCHAR](255) NULL,
		[SecondarySpecialty1] [NVARCHAR](255) NULL,
		[SecondarySpecialty2] [NVARCHAR](255) NULL,
		[SecondarySpecialty3] [NVARCHAR](255) NULL,
		[SecondarySpecialty4] [NVARCHAR](255) NULL,
		[AllSecondarySpecialties] [NVARCHAR](255) NULL,
		[AcceptsMedicareAssignment] [NVARCHAR](2) NULL,
		[ParticipatesInERX] [BIT] NULL,
		[ParticipatesInPQRS] [BIT] NULL,
		[ParticipatesInEHR] [BIT] NULL,
		[HospitalAffiliationCCN1] [NVARCHAR](7) NULL,
		[HospitalAffiliationLBN1] [NVARCHAR](255) NULL,
		[HospitalAffiliationCCN2] [NVARCHAR](7) NULL,
		[HospitalAffiliationLBN2] [NVARCHAR](255) NULL,
		[HospitalAffiliationCCN3] [NVARCHAR](7) NULL,
		[HospitalAffiliationLBN3] [NVARCHAR](255) NULL,
		[HospitalAffiliationCCN4] [NVARCHAR](7) NULL,
		[HospitalAffiliationLBN4] [NVARCHAR](255) NULL,
		[HospitalAffiliationCCN5] [NVARCHAR](7) NULL,
		[HospitalAffiliationLBN5] [NVARCHAR](255) NULL,
		[GroupPracticePacId] [NVARCHAR](15) NULL,
		[OrgLegalName] [NVARCHAR](150) NULL,
		[DBAName] [NVARCHAR](150) NULL,
		[NumberofGroupPracticeMembers] [INT] NULL,
		[Line1] [NVARCHAR](255) NOT NULL,
		[Line2] [NVARCHAR](255) NULL,
		[MarkerofAdressLine2Suppression] [BIT] NULL,
		[City] [NVARCHAR](150) NULL,
		[State] [NVARCHAR](3) NOT NULL,
		[ZipCode] [NVARCHAR](12) NULL,
		[IsEdited] [BIT] NULL,
		[Version] [BIGINT] NULL,
		[Dataset_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
IF(OBJECT_ID(N'Physicians')) IS NULL
	CREATE TABLE [dbo].[Physicians](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Npi] [BIGINT] NOT NULL,
		[PacId] [NVARCHAR](17) NULL,
		[ProfEnrollId] [NVARCHAR](20) NULL,
		[FirstName] [NVARCHAR](50) NULL,
		[MiddleName] [NVARCHAR](50) NULL,
		[LastName] [NVARCHAR](50) NULL,
		[Suffix] [NVARCHAR](20) NULL,
		[Gender] [NVARCHAR](10) NULL,
		[Credentials] [NVARCHAR](50) NULL,
		[ForeignLanguages] [NVARCHAR](255) NULL,
		[MedicalSchoolName] [NVARCHAR](255) NULL,
		[GraduationYear] [INT] NULL,
		[CouncilBoardCertification] [BIT] NULL DEFAULT (0),
		[PrimarySpecialty] [NVARCHAR](255) NULL,
		[SecondarySpecialty1] [NVARCHAR](255) NULL,
		[SecondarySpecialty2] [NVARCHAR](255) NULL,
		[SecondarySpecialty3] [NVARCHAR](255) NULL,
		[SecondarySpecialty4] [NVARCHAR](255) NULL,
		[AcceptsMedicareAssignment] [NVARCHAR](2) NULL,
		[ParticipatesInERX] [BIT] NULL,
		[ParticipatesInPQRS] [BIT] NULL,
		[ParticipatesInEHR] [BIT] NULL,
		[States] [NVARCHAR](150) NULL,
		[Version] [BIGINT] NULL,
		[IsDeleted] [BIT] NOT NULL DEFAULT (0),
		[IsEdited] [BIT] NOT NULL DEFAULT (0),
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[Npi] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[Npi] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Physicians_AffiliatedHospitals')) IS NULL
	CREATE TABLE [dbo].[Physicians_AffiliatedHospitals](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Hospital_CmsProviderId] [NVARCHAR](7) NOT NULL,
		[Version] [BIGINT] NULL,
		[Physician_Id] [INT] NULL,
		[Index] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Physicians_Audits')) IS NULL
	CREATE TABLE [dbo].[Physicians_Audits](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Action] [INT] NOT NULL,
		[EntityTypeName] [NVARCHAR](50) NOT NULL,
		[Owner_Id] [INT] NOT NULL,
		[PropertyName] [NVARCHAR](50) NULL,
		[OldPropertyValue] [NVARCHAR](500) NULL,
		[NewPropertyValue] [NVARCHAR](500) NULL,
		[Version] [BIGINT] NULL,
		[CreateDate] [DATETIME] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'MedicalPractices')) IS NULL
	CREATE TABLE [dbo].[MedicalPractices](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[Name] [NVARCHAR](150) NOT NULL,
		[GroupPracticePacId] [NVARCHAR](15) NOT NULL,
		[DBAName] [NVARCHAR](150) NULL,
		[NumberofGroupPracticeMembers] [INT] NULL,
		[State] [NVARCHAR](3) NULL,
		[IsEdited] [BIT] NOT NULL DEFAULT (0),
		[Version] [BIGINT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[GroupPracticePacId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	UNIQUE NONCLUSTERED 
	(
		[GroupPracticePacId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

IF(OBJECT_ID(N'Physicians_MedicalPractices')) IS NULL		
	CREATE TABLE [dbo].[Physicians_MedicalPractices](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AssociatedPMPAddresses] [nvarchar](max) NULL,
	[Version] [bigint] NULL,
	[Physician_Id] [int] NULL,
	[MedicalPractice_Id] [int] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

IF(OBJECT_ID(N'Addresses')) IS NULL
	CREATE TABLE [dbo].[Addresses](
		[Id] [INT] IDENTITY(1,1) NOT NULL,
		[AddressType] [NVARCHAR](30) NOT NULL,
		[Line1] [NVARCHAR](255) NOT NULL,
		[Line2] [NVARCHAR](255) NULL,
		[Line3] [NVARCHAR](255) NULL,
		[City] [NVARCHAR](150) NULL,
		[State] [NVARCHAR](3) NOT NULL,
		[ZipCode] [NVARCHAR](12) NULL,
		[Index] [INT] NULL DEFAULT(0),
		[Version] [BIGINT] NULL,
		[MedicalPractice_Id] [INT] NULL,
		[Physician_Id] [INT] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

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