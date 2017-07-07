BEGIN TRY 
	
	IF(OBJECT_ID(N'Menu')) IS NOT NULL 
		DROP TABLE Menu

	CREATE TABLE [dbo].[Menu](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Product] [nvarchar](40) NULL,
		[Name] [nvarchar](40) NULL,
		[Type] [nvarchar](40) NULL,
		[Label] [nvarchar](200) NULL,
		[Priority] [int] NULL,
		[Entity] [nvarchar](50) NULL,
		[Classes] [nvarchar](200) NULL,
		[DataSets] [nvarchar](200) NULL,
		[Routes] [nvarchar](max) NULL,
		[Target] int NULL,
		[ParentId] [int] NULL constraint [SubMenuConstraint] REFERENCES [dbo].[Menu] ([Id]),
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


	IF(OBJECT_ID(N'Websites_WebsiteMenus')) IS NOT NULL 
		DROP TABLE Websites_WebsiteMenus

	CREATE TABLE [dbo].[Websites_WebsiteMenus](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Menu] [nvarchar](max) NULL,
		[Website_Id] [int] NULL REFERENCES Websites (Id),
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


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
--Menus

Merge INTO Menu m
USING(VALUES('consumer','main','standard','Home',1,NULL,NULL,'{"$id":"1","Name":"top.consumer.home","ActiveName":["top.consumer.home"], "IgnoreName":[],"Params":{"$id":"2"}}','All'),
			('consumer','main','standard','Browse Health<br>Topics',5,'HOSPITAL',NULL,'{"$id":"1","Name":"top.consumer.hospitals.health-topics","ActiveName":["top.consumer.hospitals.health-topics"], "IgnoreName":[],"Params":{"$id":"2"}}','Hospital Compare Data,AHRQ-QI Provider Data,AHRQ-QI Composite Data'),
			('consumer','main','standard','Compare<br>Hospitals',2,'HOSPITAL',NULL,'{"$id":"1","Name":"top.consumer.hospitals.location","ActiveName":["top.consumer.hospitals"], "IgnoreName":["top.consumer.hospitals.health-topics"],"Params":{"$id":"2"}}','AHRQ-QI Composite Data,AHRQ-QI Provider Data,Hospital Compare Data,Medicare Provider Charge Data'),
			('consumer','main','standard','Compare<br>Nursing Homes',3,'NURSINGHOME',NULL,'{"$id":"1","Name":"top.consumer.nursing-homes.location","ActiveName":["top.consumer.nursing-homes"], "IgnoreName":[],"Params":{"$id":"2"}}','Nursing Home Compare Data,NH-CAHPS Survey Results Data,Nursing Home Deficiency Matrix Data'),
			('consumer','main','standard','Find<br>Doctors',4,'PHYSICIAN',NULL,'{"$id":"1","Name":"top.consumer.physicians.search","ActiveName":["top.consumer.physicians"], "IgnoreName":[],"Params":{"$id":"2"}}','Physician Data,CG-CAHPS Survey Results Data,Medical Practice HEDIS Measures Data'),
			('consumer','main','standard','About<br>This Site',5,NULL,NULL,'{"$id":"1","Name":"top.consumer.about-ratings","ActiveName":["top.consumer.about-ratings"], "IgnoreName":[],"Params":{"$id":"2"}}','All'),
			('professional','main','standard','Home',1,NULL,NULL,'{"$id":"1","Name":"top.professional.home","ActiveName":["top.professional.home"], "IgnoreName":[],"Params":{"$id":"2"}}','All'),
			('professional','main','standard','Hospitals',2,'HOSPITAL',NULL,'{"$id":"1","Name":"top.professional.quality-ratings","ActiveName":["top.professional.quality-ratings","top.professional.usage-data"], "IgnoreName":[],"Params":{"$id":"2"}}','AHRQ-QI Composite Data,AHRQ-QI Provider Data,Hospital Compare Data,Inpatient Discharge,Medicare Provider Charge Data,AHRQ-QI Area Data,ED Treat And Release'),
			('professional','main','standard','Nursing Homes',3,'NURSINGHOME',NULL,'{"$id":"1","Name":"top.professional.nursing-homes.location","ActiveName":["top.professional.nursing-homes"], "IgnoreName":[],"Params":{"$id":"2"}}','Nursing Home Compare Data,NH-CAHPS Survey Results Data,Nursing Home Deficiency Matrix Data'),
			('professional','main','standard','Physicians',4,'PHYSICIAN',NULL,'{"$id":"1","Name":"top.professional.physicians.find-physician","ActiveName":["top.professional.physicians"], "IgnoreName":[],"Params":{"$id":"2"}}','Physician Data,CG-CAHPS Survey Results Data,Medical Practice HEDIS Measures Data'),
			('professional','main','standard','Additional Reports',5,NULL,NULL,'{"$id":"1","Name":"top.professional.flutters","ActiveName":["top.professional.flutters"], "IgnoreName":[],"Params":{"$id":"2"}}','Dynamic Targets Module'),
			('professional','main','standard','Resources',6,NULL,NULL,'{"$id":"1","Name":"top.professional.resources","ActiveName":["top.professional.resources"], "IgnoreName":[],"Params":{"$id":"2","page":"AboutHealthCareQuality"}}','All'),
			('professional','main','standard','About Us',7,NULL,NULL,'{"$id":"1","Name":"top.professional.about-us","ActiveName":["top.professional.about-us"], "IgnoreName":[],"Params":{"$id":"2"}}','All')) 
Temp (Product,Menu,Type,Label,Priority,Entity,Classes,Routes, Datasets)
on Temp.Product = m.Product and Temp.Menu = Temp.Menu and Temp.Label = m.Label and Temp.Type = m.Type and m.Datasets = Temp.Datasets
WHEN MATCHED THEN 
UPDATE SET m.Product  = Temp.Product,   
	   m.Name	   = temp.Menu, 	   
	   m.Type	   = Temp.Type,	   
	   m.Label	   = Temp.Label,
	   m.Priority = Temp.Priority,
	   m.Entity   = Temp.Entity,   
	   m.Classes  = Temp.Classes,  
	   m.Routes   = Temp.Routes, 
	   m.Datasets = Temp.Datasets
WHEN NOT MATCHED THEN 
	INSERT ([Product],[Name],[Type],[Label],[Priority],[Entity],[Classes],[Routes],[Datasets])
	VALUES(Temp.Product, Temp.Menu,Temp.Type,Temp.Label,Temp.Priority,Temp.Entity,Temp.Classes,Temp.Routes, Temp.Datasets);

END TRY
BEGIN CATCH

    SELECT @ERRORMESSAGE = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY = ERROR_SEVERITY(),
           @ERRORSTATE = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE, @ERRORSEVERITY, @ERRORSTATE); 
END CATCH 


BEGIN TRY
--SubMenus
--Product,Menu,Type,Label,Priority,Entity,Clasees,Routes,DataSets
;with TempCTE (Product,Menu,Type,Label,Priority,Entity,Classes,Routes,DataSets, ParentLabel)
as 
(
select 'professional','main','standard','Quality Ratings',1,NULL,NULL,'{"$id":"1","Name":"top.professional.quality-ratings","ActiveName":["top.professional.quality-ratings"], "IgnoreName":[],"Params":{"$id":"2"}}','AHRQ-QI Composite Data,AHRQ-QI Provider Data,Hospital Compare Data,Medicare Provider Charge Data','Hospitals'
union
select 'professional','main','standard','Service Use Rates',2,NULL,NULL,'{"$id":"1","Name":"top.professional.usage-data","ActiveName":["top.professional.usage-data"], "IgnoreName":[],"Params":{"$id":"2"}}','Inpatient Discharge,Medicare Provider Charge Data,AHRQ-QI Area Data,ED Treat And Release','Hospitals'
)

Merge into Menu m
using 
(
	select cte.Product,cte.Menu,cte.Type,cte.Label,cte.Priority,cte.Entity,cte.Classes,cte.Routes, cte.Datasets, m.Id
	from TempCTE cte
	left join Menu m on m.Product = cte.Product and cte.ParentLabel = m.Label 
) SubMenu (Product,Menu,Type,Label,Priority,Entity,Classes,Routes, Datasets, ParentId)
on SubMenu.Product = m.Product and SubMenu.Label = m.Label and SubMenu.Type = m.Type and SubMenu.ParentId = m.ParentId
WHEN MATCHED THEN 
UPDATE SET m.Product  = SubMenu.Product,   
	   m.Name	   = SubMenu.Menu, 	   
	   m.Type	   = SubMenu.Type,	   
	   m.Label	   = SubMenu.Label,
	   m.Priority = SubMenu.Priority,
	   m.Entity   = SubMenu.Entity,   
	   m.Classes  = SubMenu.Classes,  
	   m.Routes   = SubMenu.Routes, 
	   m.Datasets = SubMenu.Datasets,
	   m.ParentId = SubMenu.ParentId
WHEN NOT MATCHED THEN 
	INSERT ([Product],[Name],[Type],[Label],[Priority],[Entity],[Classes],[Routes],[Datasets],[ParentId])
	VALUES(SubMenu.Product, SubMenu.Menu,SubMenu.Type,SubMenu.Label,SubMenu.Priority,SubMenu.Entity,SubMenu.Classes,SubMenu.Routes, SubMenu.Datasets,SubMenu.ParentId);

END TRY
BEGIN CATCH

    SELECT @ERRORMESSAGE = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY = ERROR_SEVERITY(),
           @ERRORSTATE = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE, @ERRORSEVERITY, @ERRORSTATE); 
END CATCH


BEGIN TRY 
-- Insert/Update Menu config type
	;With MenuConfigCTE (product,menu,type,targetLabel, targetType,entity,classes)
	as 
	(
		select 'professional', 'main', 'menu-config',null,null,null,'nav,nav-tabs'
		union
		select 'professional','main','menu-config','Hospitals','standard',null,'nav-tabs-secondary'
		union
		select 'professional','main','menu-config','Additional Reports','standard',null,'nav-tabs-secondary'
		union
		select 'professional','main','menu-config','Quality Ratings','standard',null, 'nav-tabs-tertiary'
		union
		select 'professional','main','menu-config', 'Service Use Rates','standard', null, 'nav-tabs-tertiary'
	) 

	Merge into Menu m
using 
(
	select cte.Product, cte.Menu, cte.Type,cte.Entity,cte.Classes, m.Id
	from MenuConfigCTE cte
		left join [Menu] m on m.Product = cte.Product and cte.targetLabel = m.Label and cte.targetType = m.Type 

) MenuConfig (Product,Menu,Type,Entity,Classes, Target)
on MenuConfig.Product = m.Product and MenuConfig.Menu = m.Name and MenuConfig.Type = m.Type and MenuConfig.Classes = m.Classes
--WHEN MATCHED THEN 
--UPDATE SET m.Product  = MenuConfig.Product,   
--	   m.Name		  = MenuConfig.Menu, 	   
--	   m.Type		  = MenuConfig.Type,	   
--	   m.Entity		  = MenuConfig.Entity,   
--	   m.Classes	  = MenuConfig.Classes,  
--	   m.Target		  = MenuConfig.Target
WHEN NOT MATCHED THEN 
	INSERT ([Product],[Name],[Type],[Entity],[Classes],[Target])
	VALUES(MenuConfig.Product, MenuConfig.Menu,MenuConfig.Type,MenuConfig.Entity,MenuConfig.Classes, MenuConfig.Target);

END TRY 

BEGIN CATCH
	SELECT @ERRORMESSAGE = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY = ERROR_SEVERITY(),
           @ERRORSTATE = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE, @ERRORSEVERITY, @ERRORSTATE); 
END CATCH


--BEGIN TRY 
--	--  
--	IF(OBJECT_ID(N'tempdb..#TempMenu')) IS NOT NULL
--		DROP TABLE #TempMenu

--	CREATE TABLE #TempMenu
--	(
--		WebsiteId int, 
--		MenuId int, 
--		[Id] [int] NOT NULL,
--		[Product] [nvarchar](40) NULL,
--		[Menu] [nvarchar](40) NULL,
--		[Type] [nvarchar](40) NULL,
--		[Target] [nvarchar](200) NULL,
--		[ParentId] [int] NULL,
--		[Label] [nvarchar](200) NULL,
--		[Priority] [int] NULL,
--		[Entity] [nvarchar](50) NULL,
--		[Classes] [nvarchar](200) NULL,
--		[Routes] [nvarchar](max) NULL, 
--		[DataSets][nvarchar](max) NULL, 
--		[SubMenus][nvarchar](max) NULL, 
--		JsonFormat AS '{"id":' + cast(Id as nvarchar)
--					 +',"product" : ' + case when [Product] is null then 'null' else '"'+[Product] +'"'end +''
--					 +',"IsSelected" : true'
--					 +',"menu" : ' + case when [Menu] is null then 'null' else '"'+[Menu] +'"'end +''
--					 +',"type" : ' + case when [Type] is null then 'null' else '"'+[Type] +'"'end +''
--					 +',"target" : ' + case when [target] is null then 'null' else cast([priority] as nvarchar) end +''
--					 +',"parent" :' + case when ParentId is null then 'null' else cast(ParentId as nvarchar) end +''
--					 +',"label" : "' + label +'"'
--					 +',"priority" : '+ case when [priority] is null then 'null' else cast([priority] as nvarchar) end+''
--					 +',"entity" : ' + case when entity is null then 'null' else '"'+entity+'"' end +''
--					 +',"classes" :[]'
--					 +',"route" : ' + case when [routes] is null then 'null' else ''+[routes]+'' end +''
--					 +',"DataSets" : ' + case when [DataSets] is null then 'null' else DataSets end +''
--					 +',"SubMenus" : ' + case when [SubMenus] is null then '[]' else '['+ SubMenus +']' end +''
--					 + '}'
--					  PERSISTED			 	
--	)

--	;WITH MenuTemps (MenuId, Datasets, product)
--	AS
--	(
--		SELECT Id, Temp.item, product
--		FROM Menu m
--		 CROSS APPLY fnList2Table(m.DataSets, ',') as Temp
--	), WebsiteMenuIds(WebsiteId, MenuId)
--	AS
--	(
--		SELECT w.Id, mt.MenuId --,w.Name, r.Name, Temp.item
--		FROM Websites w
--			INNER JOIN Websites_WebsiteReports wr on w.Id = wr.Website_Id
--			INNER JOIN Reports r on r.Id = wr.Report_Id
--				CROSS APPLY [dbo].[fnList2Table](r.Datasets, ',') as Temp
--			INNER JOIN MenuTemps mt on (mt.Datasets = Temp.item or mt.Datasets = 'All')
--		WHERE mt.product in (select case when aud.item = 'Consumers' then 'consumer' else 'professional' end from fnList2Table(w.Audiences, ',') as aud)
--		GROUP BY w.Id,  mt.MenuId --,w.Name, r.Name, Temp.item
--	)
	
	
--	INSERT INTO #TempMenu(WebsiteId, MenuId,Id, Product,Menu,Type,Label,Priority,Entity,Classes,Routes,Target,ParentId, DataSets)
--	SELECT WebsiteId, MenuId, Id, Product,Name,Type, Label, Priority, Entity, Classes, Routes, Target, ParentId 
--	,(SELECT CASE WHEN DataSets is not null 
--			   THEN  '[' + STUFF((
--									SELECT ',"' + Item +'"'
--									FROM fnList2Table(DataSets, ',') 
--									FOR XML PATH(''), TYPE
--									).value('.','varchar(max)'),1,1,'') 
--					+']'
--				ELSE null																 
--			END
--	)
--	FROM WebsiteMenuIds wm
--		INNER JOIN Menu m on wm.MenuId = m.Id

--/*-----------------   HANDLE WEBSITE MENUS WITH SUBMENUS    ----------------- */
--	;WITH ParentMenu
--	AS
--	(
--		SELECT * 
--		FROM #TempMenu p
--		WHERE p.id  in (SELECT ParentId FROM #TempMenu WHERE ParentId is not null) 
--		and ParentId is null 
--	)
	
--	UPDATE #TempMenu 
--	SET SubMenus = (
--					SELECT TOP 1 STUFF((
--							SELECT ','+ JsonFormat 
--							FROM #TempMenu m
--							WHERE m.ParentId = p.Id
--							GROUP BY JsonFormat 
--							FOR XML PATH(''), TYPE 
--							).value('.','varchar(max)'),1,1,'')  
--					FROM ParentMenu p
--					)
--	FROM #TempMenu tm
--		INNER JOIN ParentMenu pm on tm.Id = pm.Id

--/*-----------------   INSERT Websites_WebsiteMenus ----------------- */
--	MERGE INTO Websites_WebsiteMenus target
--	USING 
--	(
--		SELECT JsonFormat, WebsiteId 
--		FROM #TempMenu
--		WHERE ParentId IS NULL
--	) source(menu, websiteId) 
--	ON target.Menu = source.Menu and target.Website_Id = source.websiteId
--	WHEN MATCHED THEN 
--		UPDATE SET target.Menu = source.Menu,
--		target.Website_Id = source.websiteId
--	WHEN NOT MATCHED THEN 
--		INSERT (Menu, Website_Id)
--		VALUES (source.Menu, source.websiteId);

--END TRY 
--BEGIN CATCH 

--	  SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
--           @ErrorSeverity = ERROR_SEVERITY(),
--           @ErrorState = ERROR_STATE();

--    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 

--END CATCH