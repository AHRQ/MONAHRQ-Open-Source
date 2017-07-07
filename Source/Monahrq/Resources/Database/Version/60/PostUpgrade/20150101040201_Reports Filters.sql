/*******************************************
 *  Report filter table
 *******************************************/

BEGIN TRY

	-- Drop all auto generated foreign key(s) from Reports_ReportColumns table
	DECLARE @SQL varchar(4000)=''
	SELECT @SQL = @SQL + 'ALTER TABLE ' + s.name + '.' + t.name + ' DROP CONSTRAINT [' + RTRIM(f.name) +'];' + CHAR(13)
	FROM sys.Tables t
	INNER JOIN sys.foreign_keys f ON f.parent_object_id = t.object_id
	INNER JOIN sys.schemas     s ON s.schema_id = f.schema_id
	WHERE  t.name = 'Reports_Filters'
	AND f.name != 'Report_owns_RptFilter_FK';

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
	WHERE UPPER(LTRIM(RTRIM(r.[ReportType]))) NOT LIKE '%ED UTILIZATION DETAILED REPORT (%'
								  OR UPPER(r.[ReportType]) LIKE '%IP UTILIZATION DETAILED REPORT (%'
								  OR UPPER(r.[ReportType]) LIKE '%UTILIZATION RATES BY COUNTY,%'
								  OR UPPER(r.[ReportType]) LIKE '%UTILIZATION RATES BY REGION,%'

	OPEN @Reports_Cursor;
	FETCH next FROM @Reports_Cursor INTO @ReportId, @ReportName, @ReportType

	WHILE @@fetch_status = 0
	BEGIN

	IF UPPER(@ReportType) = 'COUNTY RATES REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','ConditionsAndDiagnosis',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','ConditionsAndDiagnosis',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','ConditionsAndDiagnosis',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','ConditionsAndDiagnosis',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','County',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'COUNTY RATES DETAIL REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','ConditionsAndDiagnosis',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','ConditionsAndDiagnosis',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','ConditionsAndDiagnosis',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','ConditionsAndDiagnosis',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','County',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'COUNTY RATES TRENDING REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','ConditionsAndDiagnosis',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','ConditionsAndDiagnosis',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','ConditionsAndDiagnosis',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','ConditionsAndDiagnosis',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','County',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'ED UTILIZATION DETAILED REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','ConditionsAndDiagnosis',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'EMERGENCY DEPARTMENT TREAT-AND-RELEASE (ED) UTILIZATION REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
			 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','ConditionsAndDiagnosis',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'EMERGENCY DEPARTMENT TREAT-AND-RELEASE (ED) UTILIZATION TRENDING REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','ConditionsAndDiagnosis',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'HOSPITAL COMPARISON REPORT - BAR CHART'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'HOSPITAL COMPARISON REPORT - ICONS AND CONFIDENCE INTERVALS'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'HOSPITAL COMPARISON REPORT - ICONS'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'HOSPITALS LISTINGS BASED ON THE SEARCH FROM THE HOME PAGE'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'HOSPITAL PROFILE REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Basic Descriptive Data','Display',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Cost and Charge Data (All Patients)','Display',0,1,'CostChargeDataGroup',@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Cost and Charge Data (Medicare)','Display',1,1,'CostChargeDataGroup',@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Map','Display',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Overall Patient Experience Ratings','Display',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'HOSPITALS QUALITY RATINGS - BAR CHART'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'HOSPITALS QUALITY RATINGS - DETAIL TABULAR VIEW'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'HOSPITALS QUALITY RATINGS - ICONS WITH RATES AND CONFIDENCE INTERVALS'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'HOSPITALS QUALITY RATINGS - ICONS'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'INPATIENT HOSPITAL DISCHARGE UTILIZATION REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','DRGsDischarges',1,0,NULL,@ReportId,4);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','DRGsDischarges',1,0,NULL,@ReportId,5);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','DRGsDischarges',1,0,NULL,@ReportId,6);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','DRGsDischarges',1,0,NULL,@ReportId,7);
	END

	IF UPPER(@ReportType) = 'INPATIENT HOSPITAL DISCHARGE UTILIZATION TRENDING REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','DRGsDischarges',1,0,NULL,@ReportId,4);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','DRGsDischarges',1,0,NULL,@ReportId,5);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','DRGsDischarges',1,0,NULL,@ReportId,6);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','DRGsDischarges',1,0,NULL,@ReportId,7);
	END

	IF UPPER(@ReportType) = 'INPATIENT UTILIZATION DETAIL REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Name','Hospital',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Hospital Type','Hospital',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Region','Hospital',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','Hospital',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','DRGsDischarges',1,0,NULL,@ReportId,4);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','DRGsDischarges',1,0,NULL,@ReportId,5);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','DRGsDischarges',1,0,NULL,@ReportId,6);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','DRGsDischarges',1,0,NULL,@ReportId,7);
	END

	IF UPPER(@ReportType) = 'NURSING HOME PROFILE REPORT WITH SUMMARY AND INDIVIDUAL MEASURE SCORE BY DOMAIN'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Basic Descriptive Data','Display',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Map','Display',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Overall Score','Display',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Quality Measures','Display',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Inspection','Display',1,0,NULL,@ReportId,4);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Staffing','Display',1,0,NULL,@ReportId,5);
	END

	IF UPPER(@ReportType) = 'FIND NURSING HOME WITH OVERALL AND DOMAIN LEVEL SUMMARY RATINGS REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Nursing Home Name','NursingHomeFilters',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Nursing Home Type (Medicare/Medicaid/Both)','NursingHomeFilters',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('In Hospital','NursingHomeFilters',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('County','GeoLocation',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Zip','GeoLocation',1,0,NULL,@ReportId,4);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Show on Map','Display',1,0,NULL,@ReportId,5);
	END

	IF UPPER(@ReportType) = 'PHYSICIAN LISTING REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Physician Name','PhysicianFilters',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Practice Name','PhysicianFilters',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Geo Location','PhysicianFilters',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Zip','PhysicianFilters',1,0,NULL,@ReportId,3);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Specialty','PhysicianFilters',1,0,NULL,@ReportId,4);
	END

	IF UPPER(@ReportType) = 'PHYSICIAN PROFILE'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Basic Descriptive Data','Display',1,0,NULL,(SELECT TOP 1 id FROM Reports r2 WHERE r2.ReportType='Physician Profile'),0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Map','Display',1,0,NULL,(SELECT TOP 1 id FROM Reports r2 WHERE r2.ReportType='Physician Profile'),1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Affiliated Hospitals','Display',1,0,NULL,(SELECT TOP 1 id FROM Reports r2 WHERE r2.ReportType='Physician Profile'),2);
	END

	IF UPPER(@ReportType) = 'REGION RATES REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','DRGsDischarges',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','DRGsDischarges',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','DRGsDischarges',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','DRGsDischarges',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'REGION RATES DETAIL REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','DRGsDischarges',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','DRGsDischarges',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','DRGsDischarges',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','DRGsDischarges',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'REGION RATES TRENDING REPORT'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Major Diagnosis Category','DRGsDischarges',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Diagnosis Related Group','DRGsDischarges',1,0,NULL,@ReportId,1);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Health Condition or Topic','DRGsDischarges',1,0,NULL,@ReportId,2);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('Procedure','DRGsDischarges',1,0,NULL,@ReportId,3);
	END

	IF UPPER(@ReportType) = 'INFOGRAPHICS REPORT -  SURGICAL SAFETY'
	BEGIN
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('CMS Measures','ActiveSections',1,0,NULL,@ReportId,0);
		INSERT INTO [dbo].[Reports_Filters]([Name],[FilterType],[Value],[IsRadioButton],[RadioGroupName],[Report_id],[Index])
				 VALUES('AHRQ QI Measures','ActiveSections',1,0,NULL,@ReportId,1);
	END

	FETCH NEXT FROM @Reports_Cursor INTO @ReportId, @ReportName, @ReportType
	END

	CLOSE @Reports_Cursor;
	DEALLOCATE @Reports_Cursor;

	IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[Report_owns_RptFilter_FK]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reports_Filters]'))
	BEGIN
	ALTER TABLE [dbo].[Reports_Filters]  WITH CHECK ADD  CONSTRAINT [Report_owns_RptFilter_FK] FOREIGN KEY([Report_id])
	REFERENCES [dbo].[Reports] ([Id])
	ALTER TABLE [dbo].[Reports_Filters] CHECK CONSTRAINT [Report_owns_RptFilter_FK]
	END

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