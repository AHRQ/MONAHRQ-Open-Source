-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0 Build 1
-- Create date: 12-22-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Measures table to the new 
--              MONAHRQ 6.0 database schema.
--				'Topic Category'
-- =============================================

BEGIN TRY

UPDATE TopicCategories
SET 
	TopicType = 'Hospital'
WHERE NAME NOT IN ('Utilization','Child health','Women health','Nursing Home')

/*******************************************
 *  Insert Nursing Home Topic
 *******************************************/

IF NOT EXISTS (SELECT 1 FROM TopicCategories WHERE NAME='Nursing Home')
BEGIN
INSERT INTO TopicCategories
(
	-- Id -- this column value is auto-generated
	Name,
	TopicType
)
VALUES
(
	'Nursing Home',
	'NursingHome'
)
END

DECLARE @TopicCategoryId INT 

SELECT @TopicCategoryId = Id FROM TopicCategories WHERE Name = 'Nursing Home' AND TopicType = 'NursingHome'

MERGE INTO dbo.Topics T
USING (VALUES('Health Inspection',@TopicCategoryId),
			 ('Quality Measures',@TopicCategoryId),
			 ('Staffing',@TopicCategoryId)) AS Val (Col1, Col2) ON T.Name = Val.Col1 AND T.TopicCategory_id = Val.Col2
WHEN NOT MATCHED THEN	
	INSERT(Name, TopicCategory_id)
	VALUES(Val.Col1, Val.Col2);

/*
*	Associate NursingHome Measures to Topics
*/
IF(OBJECT_ID(N'MeasureTopicTemp')) IS NOT NULL
	DROP	TABLE MeasureTopicTemp

CREATE TABLE MeasureTopicTemp
(
	MeasureCode VARCHAR(50), 
	TopicName VARCHAR(50)
)

insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-HI-02','Health Inspection')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-HI-03','Health Inspection')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-HI-04','Health Inspection')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-HI-05','Health Inspection')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-SS-01','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-SS-02','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-SS-03','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-SS-04','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-SS-05','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-01','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-02','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-03','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-04','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-05','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-06','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-07','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-08','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-09','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-10','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-11','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-12','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-QM-LS-13','Quality Measures' )
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-SD-02','Staffing')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-SD-03','Staffing')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-SD-04','Staffing')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-SD-05','Staffing')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-SD-06','Staffing')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName )  values('NH-SD-07','Staffing')


MERGE INTO Measures_MeasureTopics MT
USING( SELECT m.Id AS Measure_Id, m.Name AS MeasrureCode, t.Name AS TopicName, t.Id AS Topic_Id
		FROM dbo.Measures m
			INNER JOIN MeasureTopicTemp mp ON mp.MeasureCode = m.Name 
			INNER JOIN	dbo.Topics t  ON t.Name = mp.TopicName
		WHERE m.ClassType = 'NursingHome'
 ) AS Temp ON MT.Measure_Id = Temp.Measure_Id AND MT.Topic_Id = Temp.Topic_Id
 WHEN	NOT MATCHED THEN 
	INSERT (Measure_Id, Topic_Id)
	VALUES (Temp.Measure_Id, Temp.Topic_Id);	

DROP TABLE dbo.MeasureTopicTemp

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