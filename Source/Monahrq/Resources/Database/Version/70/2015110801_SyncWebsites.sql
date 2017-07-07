BEGIN TRY 

		
IF(OBJECT_ID(N'Websites_WebsiteThemes')) IS NULL
	CREATE TABLE [dbo].[Websites_WebsiteThemes](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[SelectedTheme] [nvarchar](100) NULL,
		[BrandColor] [nvarchar](50) NULL,
		[Brand2Color] [nvarchar](50) NULL,
		[AccentColor] [nvarchar](50) NULL,
		[SelectedFont] [nvarchar](100) NULL,
		[BackgroundColor] [nvarchar](50) NULL,
		[BodyTextColor] [nvarchar](50) NULL,
		[LinkTextColor] [nvarchar](50) NULL,
		[AudienceType] [nvarchar](255) NULL,
		[Website_Id] [int] NULL Constraint Website_WebsiteThemesConstraint FOREIGN KEY  REFERENCES Websites ([Id]),
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

-- Prep Websites_WebsiteReports SelectedYears 
IF NOT EXISTS(SELECT * FROM Information_Schema.Columns WHERE TABLE_NAME = 'Websites_WebsiteReports' and COLUMN_NAME = 'tempSelectedYears')
	ALTER TABLE Websites_WebsiteReports ADD tempSelectedYears NVARCHAR(MAX)


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


BEGIN TRY 
		
	IF OBJECT_ID(N'tempdb..#ThemesTemp') is not null 
		drop table #ThemesTemp
	
		
	CREATE TABLE #ThemesTemp 
	(
		PreviousThemeName nvarchar(50),
		SelectedTheme nvarchar(50), 
		BrandColor nvarchar(50),
		Brand2Color nvarchar(50),
		AccentColor nvarchar(50), 
		BackgroundColor nvarchar(50), 
		BodyTextColor nvarchar(50), 
		LinkTextColor nvarchar(50)
	)
	
	INSERT INTO #ThemesTemp (PreviousThemeName, SelectedTheme, BrandColor, AccentColor,  BackgroundColor, BodyTextColor, LinkTextColor, Brand2Color) 
	VALUES ('Default', 'Default (Blue)','#3778a8', '#F68F40',  '#e0f5ff','#3e464c','#2d69a4', '#265273')
	INSERT INTO #ThemesTemp (PreviousThemeName, SelectedTheme, BrandColor, AccentColor,  BackgroundColor, BodyTextColor, LinkTextColor, Brand2Color) 
	VALUES ('Theme A', 'Purple Theme','#7D5EAA', '#EDA61A',  '#ecf3da','#2b320d','#51582A', '#411B49')
	INSERT INTO #ThemesTemp (PreviousThemeName, SelectedTheme, BrandColor, AccentColor,  BackgroundColor, BodyTextColor, LinkTextColor, Brand2Color) 
	VALUES ('Theme B', 'Green Theme','#2ABC98', '#2954A2',  '#f6f3e4','#3f3c32','#247f8b', '#278970')
	INSERT INTO #ThemesTemp (PreviousThemeName, SelectedTheme, BrandColor, AccentColor,  BackgroundColor, BodyTextColor, LinkTextColor, Brand2Color) 
	VALUES ('Theme C', 'Pink Theme','#DB456A', '#3EC3CA',  '#e0efed','#00231e','#005550', '#841442')

	INSERT INTO Websites_WebsiteThemes (Website_Id, SelectedTheme, BrandColor ,AccentColor , SelectedFont , BackgroundColor , BodyTextColor , LinkTextColor, AudienceType) 
	SELECT w.Id,  temp.SelectedTheme, w.BrandColor, w.AccentColor, w.SelectedFont, temp.BackgroundColor, temp.BodyTextColor, temp.LinkTextColor, 'Professionals'
	FROM Websites w
		inner join #ThemesTemp temp on w.SelectedTheme = temp.PreviousThemeName

	ALTER TABLE Websites drop column SelectedTheme
	ALTER TABLE Websites drop column BrandColor
	ALTER TABLE Websites drop column AccentColor
	ALTER TABLE Websites drop column SelectedFont

END TRY 
BEGIN CATCH

  SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 
END CATCH
