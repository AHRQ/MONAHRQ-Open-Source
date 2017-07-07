-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 5.0 Build 2
-- Create date: 10-15-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 AHRQ Targets to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'Reports'
-- =============================================
--BEGIN TRY
DECLARE @Dataset_Id UNIQUEIDENTIFIER;
DECLARE @Dataset_Cursor CURSOR;

SET @Dataset_Cursor =  CURSOR FOR
SELECT r.Id
FROM   [@@SOURCE@@].[dbo].Reports r
WHERE IsDefaultReport=0
-- To avoid duplicate entry
AND NOT EXISTS (SELECT 1 FROM 
				[@@DESTINATION@@].dbo.[Reports] r2
                WHERE RTRIM(LTRIM(UPPER(r.Name)))=RTRIM(LTRIM(UPPER(r2.Name)))
                AND r.Category=r2.Category
                AND r.Datasets=r2.Datasets);

OPEN @Dataset_Cursor;
FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id
WHILE @@fetch_status = 0

BEGIN
DECLARE @NewReportId UNIQUEIDENTIFIER;
SET @NewReportId=NEWID();

/***************************************
 *				Reports
 ***************************************/ 
 

INSERT INTO [@@DESTINATION@@].dbo.[Reports]
SELECT
	@NewReportId,
	r.Name,
	r.ReportProfile,
	r.Category,
	r.Audiences,
	r.Datasets,
	r.RequiresCostToChargeRatio,
	r.RequiresCmsProviderId,
	r.ReportAttributes,
	r.[Description],
	r.Footnote,
	r.ComparisonKeyIconSetName,
	r.SourceTemplateId,
	r.SourceTemplateXml,
	r.IsDefaultReport,
	r.Filter,
	r.FilterItemsXml,
	r.InterpretationText,
	r.ShowInterpretationText
FROM
	[@@SOURCE@@].dbo.[Reports] r
WHERE  r.[Id] = @Dataset_Id;

/*******************************************
 *      Reports_ComparisonKeyIconSets
 *******************************************/ 

INSERT [@@DESTINATION@@].[dbo].[Reports_ComparisonKeyIconSets]
SELECT
	rckis.Name,
	rckis.BestImage,
	rckis.BelowImage,
	rckis.BetterImage,
	rckis.NotEnoughDataImage,
	rckis.AverageImage,
	@NewReportId Report_id
FROM
	[@@SOURCE@@].dbo.[Reports_ComparisonKeyIconSets] rckis
WHERE rckis.Report_id=@Dataset_Id

/*******************************************
 *      Reports_ReportColumns
 *******************************************/ 

INSERT INTO [@@DESTINATION@@].[dbo].[Reports_ReportColumns]
SELECT
	rrc.Id,
	rrc.Name,
	@NewReportId Report_Id
FROM
	[@@SOURCE@@].dbo.[Reports_ReportColumns] rrc
WHERE rrc.Report_Id=@Dataset_Id


	FETCH NEXT FROM @Dataset_Cursor INTO @Dataset_Id
END

SELECT 1;
