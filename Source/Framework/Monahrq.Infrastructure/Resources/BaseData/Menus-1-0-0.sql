BEGIN TRY 
--Menus

Merge INTO Menu m
USING(VALUES('consumer','main','standard','Home',1,NULL,NULL,'{"$id":"1","Name":"top.consumer.home","ActiveName":["top.consumer.home"], "IgnoreName":[],"Params":{"$id":"2"}}','All'),
			('consumer','main','standard','Browse Health<br>Topics',5,'HOSPITAL',NULL,'{"$id":"1","Name":"top.consumer.hospitals.health-topics","ActiveName":["top.consumer.hospitals.health-topics"], "IgnoreName":[],"Params":{"$id":"2"}}','Hospital Compare Data,AHRQ-QI Provider Data,AHRQ-QI Composite Data'),
			('consumer','main','standard','Compare<br>Hospitals',2,'HOSPITAL',NULL,'{"$id":"1","Name":"top.consumer.hospitals.location","ActiveName":["top.consumer.hospitals"], "IgnoreName":["top.consumer.hospitals.health-topics"],"Params":{"$id":"2"}}','AHRQ-QI Composite Data,AHRQ-QI Provider Data,Hospital Compare Data,Medicare Provider Charge Data'),
			('consumer','main','standard','Compare<br>Nursing Homes',3,'NURSINGHOME',NULL,'{"$id":"1","Name":"top.consumer.nursing-homes.location","ActiveName":["top.consumer.nursing-homes"], "IgnoreName":[],"Params":{"$id":"2"}}','Nursing Home Compare Data,NH-CAHPS Survey Results Data,Nursing Home Deficiency Matrix Data'),
			('consumer','main','standard','Find<br>Doctors',4,'PHYSICIAN',NULL,'{"$id":"1","Name":"top.consumer.physicians.search","ActiveName":["top.consumer.physicians"], "IgnoreName":[],"Params":{"$id":"2"}}','Physician Data,CG-CAHPS Survey Results Data,Physician HEDIS Measures Data'),
			('consumer','main','standard','About<br>This Site',5,NULL,NULL,'{"$id":"1","Name":"top.consumer.about-ratings","ActiveName":["top.consumer.about-ratings"], "IgnoreName":[],"Params":{"$id":"2"}}','All'),
			('professional','main','standard','Home',1,NULL,NULL,'{"$id":"1","Name":"top.professional.home","ActiveName":["top.professional.home"], "IgnoreName":[],"Params":{"$id":"2"}}','All'),
			('professional','main','standard','Hospitals',2,'HOSPITAL',NULL,'{"$id":"1","Name":"top.professional.quality-ratings","ActiveName":["top.professional.quality-ratings","top.professional.usage-data"], "IgnoreName":[],"Params":{"$id":"2"}}','AHRQ-QI Composite Data,AHRQ-QI Provider Data,Hospital Compare Data,Inpatient Discharge,Medicare Provider Charge Data,AHRQ-QI Area Data,ED Treat And Release'),
			('professional','main','standard','Nursing Homes',3,'NURSINGHOME',NULL,'{"$id":"1","Name":"top.professional.nursing-homes.location","ActiveName":["top.professional.nursing-homes"], "IgnoreName":[],"Params":{"$id":"2"}}','Nursing Home Compare Data,NH-CAHPS Survey Results Data,Nursing Home Deficiency Matrix Data'),
			('professional','main','standard','Physicians',4,'PHYSICIAN',NULL,'{"$id":"1","Name":"top.professional.physicians.find-physician","ActiveName":["top.professional.physicians"], "IgnoreName":[],"Params":{"$id":"2"}}','Physician Data,CG-CAHPS Survey Results Data,Physician HEDIS Measures Data'),
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
	DECLARE @ERRORMESSAGE VARCHAR(5000);
    DECLARE @ERRORSEVERITY INT;
    DECLARE @ERRORSTATE INT;

    SELECT @ERRORMESSAGE = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY = ERROR_SEVERITY(),
           @ERRORSTATE = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE, @ERRORSEVERITY, @ERRORSTATE); 
END CATCH 


BEGIN TRY

--SubMenus
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