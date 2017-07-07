/*
 * Uninstall reports
 */
delete from Websites_WebsiteReports where [Id] in (
	select distinct WR.[Id]
	from Websites_WebsiteReports wr 
		INNER JOIN Reports r on r.Id = wr.Report_Id
	where r.[Datasets] like '%[@@DynamicTargetName@@]%'
);

-- TODO update indexes

delete from [dbo].[Reports_Filters] where [Report_id] in ( select distinct r1.[Id] from reports r1 where r1.[Datasets] like '%[@@DynamicTargetName@@]%' );
delete from [dbo].[Reports_ComparisonKeyIconSets] where [Report_id] in ( select distinct r1.[Id] from reports r1 where r1.[Datasets] like '%[@@DynamicTargetName@@]%' );
delete from [dbo].[Reports_ReportColumns] where [Report_id] in ( select distinct r1.[Id] from reports r1 where r1.[Datasets] like '%[@@DynamicTargetName@@]%' );
delete from [dbo].[Reports] Where [Id] in ( select distinct r1.[Id] from reports r1 where r1.[Datasets] like '%[@@DynamicTargetName@@]%' );


/*
 * Uninstall datasets and target tables
 */
-- Website datasets
delete from Websites_WebsiteDatasets where [Id] in (
	select distinct wd.[Id]
	from [dbo].[Websites_WebsiteDatasets] wd 
		INNER JOIN [Wings_Datasets] d on d.Id = wd.Dataset_Id
	where d.[ContentType_Id] = [@@DynamicTargetId@@]
);