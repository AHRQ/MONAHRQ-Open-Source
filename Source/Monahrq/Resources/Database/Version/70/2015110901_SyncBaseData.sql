IF NOT EXISTS(SELECT TOP 1 [Id] FROM [Base_Sexes] WHERE UPPER([Name]) = 'MISSING')
BEGIN
	--SET IDENTITY_INSERT [Base_Sexes] ON;

	--INSERT INTO [Base_Sexes] ([Id], [Name],[Value])
	--VALUES (0, 'Missing',0);

	--SET IDENTITY_INSERT [Base_Sexes] OFF;

	TRUNCATE TABLE [dbo].[Base_Sexes];

	DELETE FROM [dbo].[SchemaVersions]
	WHERE UPPER([Name]) = UPPER('Base_Sexes');


END;

-- Insert missing target scope values if missing
IF NOT EXISTS(SELECT wsv.[Id] 
			  FROM [dbo].[Wings_ScopeValues] wsv LEFT JOIN [Wings_Scopes] ws on ws.[Id] = wsv.[Scope_Id]
			  WHERE UPPER(ws.[Name]) = 'SEX' and UPPER(wsv.[Name]) = 'MISSING')
 BEGIN
	INSERT	INTO [dbo].[Wings_ScopeValues] ([Name],[Description],[Value],[Scope_Id])
	SELECT	'Missing', 'Missing', 0, [ID]
	FROM	[Wings_Scopes] WHERE UPPER([Name]) = 'SEX';
END;

-- update wings dataset data if null then should be missing.
UPDATE [Targets_InpatientTargets]
SET [Sex] = 0
WHERE [Sex] = null;

UPDATE [Targets_TreatAndReleaseTargets]
SET [Sex] = 0
WHERE [Sex] = null;



IF(Object_Id(N'Base_CostToCharges')) IS NOT NULL
	BEGIN 
		TRUNCATE TABLE [dbo].[Base_CostToCharges];

		DELETE FROM [dbo].[SchemaVersions]
		WHERE UPPER([Name]) = UPPER('Base_CostToCharges');
	END


-- Inpatient Discharge Update default values for ICDCodeType to 9 for previously null values/
UPDATE [dbo].[Targets_InpatientTargets]
SET [ICDCodeType]=9
WHERE [ICDCodeType] IS NULL;


-- Treat And Release (ED) Update default values for ICDCodeType to 9 for previously null values/
UPDATE [dbo].[Targets_TreatAndReleaseTargets]
SET [ICDCodeType]=9
WHERE [ICDCodeType] IS NULL;


-- TopicCategories Update assign all CategoryTypes='Condition' to 'Topic'
UPDATE	[dbo].[TopicCategories]
SET		[CategoryType] = 'Topic'
WHERE	[CategoryType] = 'Condition';


UPDATE [dbo].[Wings]
SET WingGuiD = 'A83E3A02-2B84-417B-BF5E-8D334037D7C9'
WHERE Name = 'Base Data Loader'
AND [Description] = 'Loads the base data';


-- Remove SchemaVersions 'Topics' row so that Topics will be reimported.
DELETE FROM [dbo].[SchemaVersions]
WHERE UPPER([Name]) = UPPER('Topics');

-- Update IQI 12 to a SupportsCost measure.
UPDATE	[dbo].[Measures]
SET		[SupportsCost] = 1
WHERE	[Name] = 'IQI 12';

-- Remove Nursing Care Measure Topics
--select			*
delete			mt
from			TopicCategories tc
	inner join	Topics t on t.TopicCategory_id = tc.Id
	inner join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Nursing Care'

-- Remove"Deaths or returns to the hospital" Measure Topics
--select			*
delete			mt
from			TopicCategories tc
	inner join	Topics t on t.TopicCategory_id = tc.Id
	inner join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Deaths or returns to the hospital'


-- Remove TopicCategory: Prevention of Treatment, Topic: Blood Clot, and related
delete			mt
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Prevention and Treatment'

delete			t
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Prevention and Treatment'

delete			tc
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Prevention and Treatment'


-- Remove TopicCategory: [Infections, Nursing Care, Deaths or returns to the hospital]  and related
delete			mt
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name in ('Infections', 'Nursing Care', 'Deaths or returns to the hospital')

delete			t
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name in ('Infections', 'Nursing Care', 'Deaths or returns to the hospital')

delete			tc
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name in ('Infections', 'Nursing Care', 'Deaths or returns to the hospital')
	
-- Remove Topic "Results of care from from "Health Care Cost and Quality"
delete			mt
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Health Care Cost and Quality'
	and			t.Name = 'Results of care'

delete			t
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Health Care Cost and Quality'
	and			t.Name = 'Results of care'


-- Remove Topic "Children Surveys" from from "Medical Practice Ratings"
delete			mt
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Medical Practice Ratings'
	and			t.Name = 'Children Surveys'

delete			t
from			TopicCategories tc
	left join	Topics t on t.TopicCategory_id = tc.Id
	left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
where			tc.Name = 'Medical Practice Ratings'
	and			t.Name = 'Children Surveys'