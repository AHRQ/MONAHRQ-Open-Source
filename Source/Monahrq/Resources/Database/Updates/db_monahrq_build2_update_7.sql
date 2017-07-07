-- =============================================
-- Author:		Jason Duffus
-- Project:		MONAHRQ 5.0 Build 2
-- Modified by: Shafiul Alam
-- Create date: 08-07-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 edited Websites to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'Websites'
-- Modification: 1. Created VersionToVersionWebsiteMappingTemp temp table to track current and new
--				    website.This will be required during execution of "Measure upgrade script"
--					in order to popuplate Website_measures table.
-- =============================================

DECLARE @Website_Id INT
DECLARE @Websites_Cursor CURSOR;

/******************************************************************
 *  Create table to preserve Current and New Website Id.
 *  This is required to update Websites_WebsiteMeasures
 *  Table from Build 1 to Build 2 
 *********************************************************************/

IF OBJECT_ID('[@@DESTINATION@@].[dbo].VersionToVersionWebsiteMappingTemp') IS NOT NULL
DROP TABLE [@@DESTINATION@@].[dbo].VersionToVersionWebsiteMappingTemp

CREATE TABLE [@@DESTINATION@@].[dbo].VersionToVersionWebsiteMappingTemp
( CurrentWebsiteId INT,
  NewWebsiteId INT )
  
SET @Websites_Cursor = CURSOR FOR
SELECT DISTINCT  [Id] FROM [@@SOURCE@@].[dbo].[Websites]

OPEN @Websites_Cursor;

FETCH next FROM @Websites_Cursor INTO @Website_Id

WHILE @@fetch_status = 0
BEGIN

	DECLARE @NewWebsite_Id INT;

	INSERT INTO [@@DESTINATION@@].[dbo].[Websites] (
				[Name],[CurrentStatus],[ReportedYear],[Description],[Audience],[ReportedQuarter],[BrowserTitle],[Keywords],
				[AboutUsSectionSummary],[AboutUsSectionText],[SelectedReportingStatesXml],[SelectedZipCodeRadiiXml],[FeedbackTopicsXml],
				[GoogleAnalyticsKey],[GoogleMapsApiKey],[GeographicDescription],[OutPutDirectory],[IsStandardFeedbackForm],
				[CustomFeedbackFormUrl],[IncludeFeedbackFormInYourWebsite],[HeaderTitle],[SelectedTheme],[BrandColor],[AccentColor],[SelectedFont],
				[LogoImagePath],[LogoImageMemeType],[LogoImage],[BannerImagePath],[BannerImageMemeType],[BannerImage],[HomepageContentImagePath],[HomepageContentImageMemeType],[HomepageContentImage]
				)
	SELECT [Name],[CurrentStatus],[ReportedYear],[Description],[Audience],[ReportedQuarter],[BrowserTitle],[Keywords],
		   [AboutUsSectionSummary],[AboutUsSectionText],[SelectedReportingStatesXml],[SelectedZipCodeRadiiXml],[FeedbackTopicsXml],
		   [GoogleAnalyticsKey],[GoogleMapsApiKey],[GeographicDescription],[OutPutDirectory],[IsStandardFeedbackForm],
		   [CustomFeedbackFormUrl],[IncludeFeedbackFormInYourWebsite],[HeaderTitle],[SelectedTheme],[BrandColor],[AccentColor],[SelectedFont],
		   [LogoImagePath],[LogoImageMemeType],[LogoImage],[BannerImagePath],[BannerImageMemeType],[BannerImage],[HomepageContentImagePath],[HomepageContentImageMemeType],[HomepageContentImage]
	FROM [@@SOURCE@@].[dbo].[Websites]
	WHERE [Id] = @Website_Id;

	-- Retrieve Inserted Website Id.
	SELECT @NewWebsite_Id = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[Websites]');
	
	INSERT INTO [@@DESTINATION@@].[dbo].VersionToVersionWebsiteMappingTemp
	VALUES (@Website_Id,@NewWebsite_Id)

	Print 'Old Website Id: ' + cast(@Website_Id as nvarchar);
	Print 'New Website Id: ' + cast(@NewWebsite_Id as nvarchar);
	
	-- Copy over ActivityLogEntries
	INSERT INTO [@@DESTINATION@@].[dbo].[Websites_ActivityLogEntries]
	SELECT [Description], [Date], @NewWebsite_Id, [Index]
	FROM [@@SOURCE@@].[dbo].[Websites_ActivityLogEntries]
	WHERE [Website_Id] = @Website_Id;

	Declare @ReportingStatesXml xml;

	SELECT @ReportingStatesXml = w.SelectedReportingStatesXml	-- Get Select	 
	FROM [@@DESTINATION@@].[dbo].[Websites] w
	WHERE w.[Id] = @NewWebsite_Id;

	-- Copy over Datasets
	with WebsiteDataSets([Dataset_Id], [Website_Id], [Index], [File], [TypeName]) as
	(
		select wd.[Dataset_Id], wd.[Website_Id], wd.[Index], d.[File], dt.[Name]
		from [@@SOURCE@@].[dbo].[Websites_WebsiteDatasets] wd
				inner join [@@SOURCE@@].[dbo].[ContentItemRecords] d on d.[Id] = wd.[Dataset_Id]
				inner join [@@SOURCE@@].[dbo].[ContentTypeRecords] dt on d.[ContentType_Id] = dt.[Id]
		where wd.Website_Id = @Website_Id
	)
	INSERT INTO [@@DESTINATION@@].[dbo].[Websites_WebsiteDatasets]
	select d.[Id], @NewWebsite_Id, owd.[Index]
	from (([@@DESTINATION@@].[dbo].[ContentItemRecords] d 
			inner join [@@DESTINATION@@].[dbo].[ContentTypeRecords] dt on d.ContentType_Id = dt.[Id] )
			inner join WebsiteDataSets owd on UPPER(owd.[File]) = UPPER(d.[File]) and UPPER(dt.[Name]) = UPPER(owd.[TypeName]))
	WHERE owd.[Website_Id] = @Website_Id
	ORDER BY owd.[Website_Id];

	-- Copy over Measures


	-- Copy over Reports
	WITH WebsiteReports([Report_Id], [Website_Id], [Index], [Category], [IsDefaultReport], [Name], [Audiences], [Datasets]) AS
	(
		SELECT wr.[Report_Id], wr.[Website_Id], wr.[Index], r.[Category], r.[IsDefaultReport], r.[Name], r.[Audiences], r.[Datasets]
		FROM [@@SOURCE@@].[dbo].[Websites_WebsiteReports] wr
			 INNER JOIN [@@SOURCE@@].[dbo].[Reports] r on r.[Id] = wr.[Report_Id]
		WHERE wr.[Website_Id] = @website_id
	)
	INSERT INTO [@@DESTINATION@@].[dbo].[Websites_WebsiteReports] ([Report_Id], [Website_Id], [Index])
	SELECT r.[Id], @NewWebsite_Id, wr.[Index] from [@@DESTINATION@@].[dbo].[Reports] r
	  INNER JOIN WebsiteReports wr on UPPER(wr.[Name]) = UPPER(r.[Name])
	WHERE wr.[Website_Id] = @Website_Id;

	-- Copy over Hospitals

	SELECT (CASE
				WHEN wh.[CCR] IS NULL THEN [@@SOURCE@@].[dbo].[fnGetCostToChargeRatio](h.[CmsProviderId], w.[ReportedYear])
				ELSE wh.[CCR]
				END) 'CCR',
				h.[CMSProviderId],
				h.[LocalHospitalId],
			wh.[Hospital_Id],
			wh.[Website_Id],
			wh.[Index] 
	INTO #WEBSITE_HOISPITALS
	FROM [@@SOURCE@@].[dbo].[Websites_WebsiteHospitals] wh
		INNER join [@@SOURCE@@].[dbo].[Websites] w ON w.[Id] = wh.[Website_Id]
		INNER JOIN [@@SOURCE@@].[dbo].[Hospitals] h ON h.[Id] = wh.[Hospital_Id] AND h.[IsArchived] = 0 AND h.IsDeleted = 0
		INNER JOIN [@@SOURCE@@].[dbo].[Base_States] s ON s.[Id] = h.[State_Id]
	WHERE w.[Id] = @Website_Id
		AND s.[Abbreviation] in (SELECT r.nref.value('.','varchar(max)') 
								FROM @ReportingStatesXml.nodes('/ArrayOfString/*') r(nref))
	ORDER BY wh.[Website_Id];

	INSERT INTO [@@DESTINATION@@].[dbo].[Websites_WebsiteHospitals] ( [CCR],[Hospital_Id],[Website_Id],[Index] )
	SELECT wh.[CCR], h.[Id], @NewWebsite_Id, wh.[Index] --h.[Name], 'Old LocalHospitalId', h.[CmsProviderId], wh.[CmsProviderId] 'Old CmsProviderId'
	FROM [@@DESTINATION@@].[dbo].[Hospitals] h
		INNER JOIN (SELECT [CCR],[Hospital_Id],[Website_Id],[CMSProviderId],[LocalHospitalId], [Index] FROM #WEBSITE_HOISPITALS) wh 
			ON wh.[CMSProviderId] = h.[CmsProviderId]
	WHERE h.[IsArchived] = 0 AND h.[IsDeleted] = 0 AND wh.[Website_Id] = @Website_Id;

	DROP TABLE #WEBSITE_HOISPITALS;

FETCH NEXT FROM @Websites_Cursor INTO @Website_Id
END

CLOSE @Websites_Cursor;
DEALLOCATE @Websites_Cursor;