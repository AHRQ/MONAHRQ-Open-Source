
BEGIN TRY 
			/* website report new format for selectedYears  and also change the length for SelectedYears column*/
		
			Declare @quaters nvarchar(max)

			IF(Object_ID(N'tempdb..#AllSelectedYears ')) IS NOT NULL 
				DROP TABLE #AllSelectedYears 
			
			CREATE TABLE #AllSelectedYears 
			(
				reportId int, 
				[Year] nvarchar(20), 
				IsSelected nvarchar(10), 
				IsDefault nvarchar(10), 
				Quarters nvarchar(max),
				Json as '{"id":'+ Cast(reportId as nvarchar) 
					+',"IsSelected":' +IsSelected  
					+',"IsDefault":' +IsDefault 
					+',"Year":' +[Year]  
					+',"Quarters": '+ Quarters 
					+ '}' PERSISTED 
			)
			
			;WITH AllQuarters (Quart, Name)
			AS (
				SELECT DISTINCT DischargeQuarter, wt.Name
				FROM Targets_InpatientTargets ip	
					inner join Wings_Datasets wd ON wd.Id = ip.Dataset_Id
					inner join Wings_Targets wt ON wd.ContentType_Id = wt.Id
				WHERE DischargeQuarter is not null 
				
				UNION ALL
				
				SELECT DISTINCT DischargeQuarter, wt.Name
				FROM Targets_TreatAndReleaseTargets ed
					inner join Wings_Datasets wd ON wd.Id = ed.Dataset_Id
					inner join Wings_Targets wt ON wd.ContentType_Id = wt.Id
				WHERE DischargeQuarter is not null 
			)
			
			SELECT @quaters = '[' + STUFF((
					SELECT ', {"id":'+ Cast(ROW_NUMBER() OVER(ORDER BY Quart) AS NVARCHAR), 
					+',"IsSelected":true',  
					+',"Text": "Quarter '+ CAST(Quart AS NVARCHAR)
					+ '"}' 
					FROM AllQuarters
					FOR XML PATH(''), TYPE
			).value('.','varchar(max)'),1,1,'') + ']'
			
			;WITH DefaultYearCTE (id, [Year], IsDefault)
			AS
			(
				SELECT wr.Id, max(item), 'true'
				FROM Websites_WebsiteReports wr
					cross apply fnList2Table(wr.SelectedYears, ',')	
				GROUP BY wr.Id
			) 
			
			INSERT INTO #AllSelectedYears (reportId, [Year], IsSelected, IsDefault, Quarters)
			SELECT id, [Year], 'true',IsDefault, @quaters
			FROM DefaultYearCTE 
				
				UNION 
					
			SELECT wr.Id, item,'true', 'false', @quaters
			FROM Websites_WebsiteReports wr
					CROSS APPLY fnList2Table(wr.SelectedYears, ',')		
					CROSS JOIN DefaultYearCTE df
			WHERE df.id = wr.Id and df.Year != item
			
			
			DECLARE @reportId INT, @sql NVARCHAR(max)
			
			DECLARE DistinctIdsCursor CURSOR 
			FOR SELECT DISTINCT reportId FROM #AllSelectedYears
			
			OPEN DistinctIdsCursor
			FETCH NEXT FROM DistinctIdsCursor INTO @reportId 
			WHILE @@FETCH_STATUS = 0
				BEGIN 
					;WITH TempCTE(reportId, [Year], IsSelected, IsDefault, Json)
					AS
					(
						SELECT reportId, [Year], IsSelected, IsDefault, Json
						FROM #AllSelectedYears 
						WHERE reportId = @reportId
					)
			
					SELECT @sql  = '['+ STUFF((
					SELECT ', '+ Json 
					FROM TempCTE
					FOR XML PATH(''), TYPE
					).value('.','varchar(max)'),1,1,'') + ']'
			
			
					UPDATE Websites_WebsiteReports
					SET tempSelectedYears = @sql
					WHERE Id = @reportId 
					and SelectedYears is not null
							
					FETCH NEXT FROM DistinctIdsCursor INTO @reportId 
				END
			CLOSE DistinctIdsCursor
			DEALLOCATE DistinctIdsCursor	 


	UPDATE Reports
	SET Datasets = 'Hospital Compare Data,Inpatient Discharge,Medicare Provider Charge Data,AHRQ-QI Composite Data,AHRQ-QI Provider Data'
	WHERE Name = 'Hospital Profile Report'
			

	ALTER TABLE Websites_WebsiteReports DROP COLUMN [SelectedYears] 
	EXEC sp_rename 'Websites_WebsiteReports.tempSelectedYears', 'SelectedYears', 'COLUMN'


	--  Drop Websites_WebsiteReports Constraints.
	DECLARE @sqlDFK NVARCHAR(MAX) = N'';

	SELECT		@sqlDFK += N'' +
						'ALTER TABLE ' + 
						QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' +
						QUOTENAME(OBJECT_NAME(parent_object_id)) + 
						' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
	FROM		sys.foreign_keys fk
	where		QUOTENAME(OBJECT_NAME(fk.parent_object_id)) = '[Websites_WebsiteReports]';

	--PRINT @sql;
	EXEC sp_executesql @sqlDFK;

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