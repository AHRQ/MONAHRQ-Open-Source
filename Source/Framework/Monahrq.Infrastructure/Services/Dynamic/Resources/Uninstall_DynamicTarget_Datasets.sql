/*
 * Uninstall target datasets and target tables
 */
-- Website datasets
delete from [dbo].[Websites_WebsiteDatasets] where [Id] in (
	select distinct wd.[Id]
	from [dbo].[Websites_WebsiteDatasets] wd 
		INNER JOIN [Wings_Datasets] d on d.Id = wd.Dataset_Id
	where d.[ContentType_Id]=[@@DynamicTargetId@@]
);

-- TODO update indexes

-- truncate and drop dynamic target dataset target tables
declare @DynamicTargetTableName nvarchar(250);
select distinct @DynamicTargetTableName = t.[DbSchemaName] from Wings_Targets t
where t.[Id]=[@@DynamicTargetId@@] AND UPPER(t.[Name]) = UPPER('[@@DynamicTargetName@@]') AND t.[IsCustom] = 1;

if exists (select * from INFORMATION_SCHEMA.TABLES t
where t.TABLE_NAME = @DynamicTargetTableName ) 
begin
	declare @Sql nvarchar(1000) = N'truncate table [' + @DynamicTargetTableName + N'];';
	select @Sql += N'drop table [' + @DynamicTargetTableName + N'];';
	exec(@sql);
end

delete from [dbo].[Wings_DatasetVersions] where [Dataset_Id] in ( select Distinct d.[Id] from [dbo].[Wings_Datasets] d where d.[ContentType_Id] = [@@DynamicTargetId@@] );
delete from [dbo].[Wings_DatasetTransactionRecords] where [Dataset_Id] in ( select Distinct d.[Id] from [dbo].[Wings_Datasets] d where d.[ContentType_Id] = [@@DynamicTargetId@@] );
delete from [dbo].[Wings_Datasets] where [Id] in ( select Distinct d.[Id] from [dbo].[Wings_Datasets] d where d.[ContentType_Id] = [@@DynamicTargetId@@] );