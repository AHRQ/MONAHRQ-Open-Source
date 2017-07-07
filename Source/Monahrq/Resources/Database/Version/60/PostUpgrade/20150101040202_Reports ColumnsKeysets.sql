/*******************************************
 *  Report columns
 *******************************************/

BEGIN TRY

	-- Drop all auto generated foreign key(s) from Reports_ReportColumns table
	DECLARE @SQL varchar(4000)=''
	SELECT @SQL = @SQL + 'ALTER TABLE ' + s.name+'.'+t.name + ' DROP CONSTRAINT [' + RTRIM(f.name) +'];' + CHAR(13)
	FROM sys.Tables t
	INNER JOIN sys.foreign_keys f ON f.parent_object_id = t.object_id
	INNER JOIN sys.schemas     s ON s.schema_id = f.schema_id
	WHERE  t.name = 'Reports_ReportColumns'
	AND f.name != 'FK_Reports_Owns_ReportColumns';

	EXEC(@SQL);

	DECLARE @Reports_Cursor CURSOR;

	DECLARE @ReportId int;
	DECLARE @ReportName varchar(500);
	DECLARE @ReportType varchar(2000);
  
	SET @Reports_Cursor = CURSOR FOR
	SELECT distinct 
		r.[Id], 
		r.[Name],
		(select r2.SourceTemplateXml.value('(/ReportManifest/@Name)[1]', 'varchar(2000)')  
		FROM [dbo].[Reports] r2 WHERE r2.[Id] = r.[Id]) 'ReportType'
	FROM [dbo].[Reports] r
	--WHERE UPPER(LTRIM(RTRIM(r.[ReportType]))) NOT LIKE '%ED UTILIZATION DETAILED REPORT (%'
	--							  OR UPPER(r.[ReportType]) LIKE '%IP UTILIZATION DETAILED REPORT (%'
	--							  OR UPPER(r.[ReportType]) LIKE '%UTILIZATION RATES BY COUNTY,%'
	--							  OR UPPER(r.[ReportType]) LIKE '%UTILIZATION RATES BY REGION,%'

	OPEN @Reports_Cursor;
	FETCH next FROM @Reports_Cursor INTO @ReportId, @ReportName, @ReportType

	WHILE @@fetch_status = 0
	BEGIN

		-- insert report columns missing after initializing reports module via application.

		IF UPPER(@ReportType)='AVOIDABLE STAYS MAPS BY COUNTY PROFILE REPORT'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Numerator',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Denominator',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Rates',0,1,5,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Cost Savings',0,1,7,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='AVOIDABLE STAYS MAPS BY COUNTY REPORT'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Numerator',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Denominator',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Rates',0,1,5,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Cost Savings',0,1,7,NULL,@ReportId);
		END
	
		IF UPPER(@ReportType)='HOSPITALS LISTINGS BASED ON THE SEARCH FROM THE HOME PAGE'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Hospital Name',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Type of Hospital',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('How patients rate this hospital overall',0,1,5,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='HOSPITALS QUALITY RATINGS - DETAIL TABULAR VIEW'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Numerator',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Denominator',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Observed Rate',0,1,5,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Observed Lower-bound Confidence Interval (CI)',0,1,7,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Observed Upper-bound Confidence Interval (CI)',0,1,9,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Expected Rate',0,1,11,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Risk-Adjusted Rate',0,1,13,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Risk-Adjusted Lower-bound Confidence Interval (CI)',0,1,15,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Risk-Adjusted Upper-bound Confidence Interval (CI)',0,1,17,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='INPATIENT HOSPITAL DISCHARGE UTILIZATION REPORT'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Number of discharges',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean charges in dollars',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean cost in dollars',0,1,5,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Median cost in dollars',0,1,7,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean length of stay in days',0,1,9,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='INPATIENT HOSPITAL DISCHARGE UTILIZATION TRENDING REPORT'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Number of discharges',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean charges in dollars',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean cost in dollars',0,1,5,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Median cost in dollars',0,1,7,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean length of stay in days',0,1,9,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='REGION RATES REPORT'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Number of discharges',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean cost in dollars',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Rate of Discharges Per 1000 Persons',0,1,5,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='REGION RATES DETAIL REPORT'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Number of discharges',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean cost in dollars',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Rate of Discharges Per [x] Persons',0,1,5,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='REGION RATES TRENDING REPORT'
		BEGIN
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Number of discharges',0,1,1,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Mean cost in dollars',0,1,3,NULL,@ReportId);
			INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
				VALUES('Rate of Discharges Per 1000 Persons',0,1,5,NULL,@ReportId);
		END

		IF UPPER(@ReportType)='UTILIZATION RATES BY REGION, DETAIL REPORT (POPULATION) – TRENDING REPORT'
		BEGIN
		INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
			VALUES('Number of discharges',0,1,1,NULL,@ReportId);
		INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
			VALUES('Mean cost in dollars',0,1,3,NULL,@ReportId);
		INSERT INTO [dbo].[Reports_ReportColumns]([Name],[IsMeasure],[IsIncluded],[Index],[MeasureCode],[Report_Id])
			VALUES('Rate of Discharges Per [x] Persons',0,1,5,NULL,@ReportId);
		END

		FETCH NEXT FROM @Reports_Cursor INTO @ReportId, @ReportName, @ReportType
	END

	CLOSE @Reports_Cursor;
	DEALLOCATE @Reports_Cursor;

	-- Recreate new foreign key index

	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reports_Owns_ReportColumns]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reports_ReportColumns]'))
	BEGIN
		ALTER TABLE [dbo].[Reports_ReportColumns]  WITH CHECK ADD  CONSTRAINT [FK_Reports_Owns_ReportColumns] FOREIGN KEY([Report_id])
		REFERENCES [dbo].[Reports] ([Id])
		ALTER TABLE [dbo].[Reports_ReportColumns] CHECK CONSTRAINT [FK_Reports_Owns_ReportColumns]
	END

	/*
	Report Icon Key Sets
	*/
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet1','\Resources\IconSet1\best.gif','\Resources\IconSet1\below.gif','\Resources\IconSet1\better.gif','\Resources\IconSet1\notenoughdata.gif','\Resources\IconSet1\average.gif',0,0,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITAL COMPARISON REPORT - ICONS AND CONFIDENCE INTERVALS'));
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet2','\Resources\IconSet2\best.gif','\Resources\IconSet2\below.gif','\Resources\IconSet2\better.gif','\Resources\IconSet2\notenoughdata.gif','\Resources\IconSet2\average.gif',0,1,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITAL COMPARISON REPORT - ICONS AND CONFIDENCE INTERVALS'));
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet1','\Resources\IconSet1\best.gif','\Resources\IconSet1\below.gif','\Resources\IconSet1\better.gif','\Resources\IconSet1\notenoughdata.gif','\Resources\IconSet1\average.gif',0,0,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITAL COMPARISON REPORT - ICONS'));
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet2','\Resources\IconSet2\best.gif','\Resources\IconSet2\below.gif','\Resources\IconSet2\better.gif','\Resources\IconSet2\notenoughdata.gif','\Resources\IconSet2\average.gif',0,1,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITAL COMPARISON REPORT - ICONS'));
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet1','\Resources\IconSet1\best.gif','\Resources\IconSet1\below.gif','\Resources\IconSet1\better.gif','\Resources\IconSet1\notenoughdata.gif','\Resources\IconSet1\average.gif',0,0,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITALS QUALITY RATINGS - ICONS WITH RATES AND CONFIDENCE INTERVALS'));
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet2','\Resources\IconSet2\best.gif','\Resources\IconSet2\below.gif','\Resources\IconSet2\better.gif','\Resources\IconSet2\notenoughdata.gif','\Resources\IconSet2\average.gif',0,1,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITALS QUALITY RATINGS - ICONS WITH RATES AND CONFIDENCE INTERVALS'));
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet1','\Resources\IconSet1\best.gif','\Resources\IconSet1\below.gif','\Resources\IconSet1\better.gif','\Resources\IconSet1\notenoughdata.gif','\Resources\IconSet1\average.gif',0,0,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITALS QUALITY RATINGS - ICONS'));
	--INSERT INTO [dbo].[Reports_ComparisonKeyIconSets]   ([Name],[BestImage],[BelowImage],[BetterImage],[NotEnoughDataImage],[AverageImage],[IsIncluded],[INDEX],[Report_Id])
	--	VALUES('IconSet2','\Resources\IconSet2\best.gif','\Resources\IconSet2\below.gif','\Resources\IconSet2\better.gif','\Resources\IconSet2\notenoughdata.gif','\Resources\IconSet2\average.gif',0,1,(SELECT TOP 1 id FROM Reports r2 WHERE UPPER(@ReportType)='HOSPITALS QUALITY RATINGS - ICONS'));

	--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reports_Owns_ComparisonKeyIcons]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reports_ComparisonKeyIconSets]'))
	--BEGIN
	--	ALTER TABLE [dbo].[Reports_ComparisonKeyIconSets]  WITH CHECK ADD  CONSTRAINT [FK_Reports_Owns_ComparisonKeyIcons] FOREIGN KEY([Report_id])
	--	REFERENCES [dbo].[Reports] ([Id])
	--	ALTER TABLE [dbo].[Reports_ComparisonKeyIconSets] CHECK CONSTRAINT [FK_Reports_Owns_ComparisonKeyIcons]
	--END

	SELECT 1;

END TRY
BEGIN CATCH
    DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
END CATCH;