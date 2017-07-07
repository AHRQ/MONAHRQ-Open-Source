/*
 * Uninstall associated measures and website measures
 */
declare @DynamicTargetTableName nvarchar(250);
select distinct @DynamicTargetTableName = t.[DbSchemaName] from Wings_Targets t
where t.[Id]=[@@DynamicTargetId@@] AND UPPER(t.[Name]) = UPPER('[@@DynamicTargetName@@]') AND t.[IsCustom] = 1;

delete from [dbo].[Websites_WebsiteMeasures] where [id] in (
select wm.[Id]
	from Websites_WebsiteMeasures wm 
		INNER JOIN Measures m on m.Id = ISNULL(wm.OverrideMeasure_Id, wm.OriginalMeasure_Id)
	where m.[Target_id] = [@@DynamicTargetId@@]
);

-- TODO update indexes

delete from [dbo].[Measures_MeasureTopics] where [Measure_Id] in ( select distinct  m.[Id] from Measures m where m.[Target_id]=[@@DynamicTargetId@@] );
delete from [dbo].[Measures] where [Target_id]=[@@DynamicTargetId@@];

delete from [dbo].[Topics] where UPPER([WingTargetName]) = UPPER('[@@DynamicTargetName@@]');
delete from [dbo].[TopicCategories] where UPPER([WingTargetName]) = UPPER('[@@DynamicTargetName@@]');