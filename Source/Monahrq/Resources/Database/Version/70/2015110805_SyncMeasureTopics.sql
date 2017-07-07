BEGIN TRY 


--select 'insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('''+ m.Name +'''', ''''+ t.Name+'''', '''' + m.ClassType    +''')'
--from mon71..Topics t
--	inner join Measures_MeasureTopics mt on mt.Topic_Id = t.Id
--	inner join mon71..Measures m on m.Id = mt.Measure_Id
--where t.Name not in(select t2 .Name from MONAHRQ50..Topics t2)

IF(OBJECT_ID(N'MeasureTopicTemp')) IS NOT NULL
	DROP	TABLE MeasureTopicTemp

CREATE TABLE MeasureTopicTemp
(
	MeasureCode VARCHAR(50), 
	TopicName VARCHAR(50), 
	ClassType nvarchar(50)
)

	
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q46','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q17','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q19','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q21','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q50','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q12','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q13','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q14','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q24','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q15','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q57','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q26','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q27','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q37','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q42','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q28','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q35','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q64','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q11','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q40','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q29','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q22','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q30','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q31','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q33','Rating of Care by Residents’ Family','NURSINGHOME')
insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('Q38','Rating of Care by Residents’ Family','NURSINGHOME')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_COMP_01','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_06','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_08','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_10','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_12','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_13','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_COMP_02','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_16','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_17','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_19','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_20','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_21','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_22','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_COMP_03','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_27','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_28','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_COMP_04','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_24','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_COMP_05','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_25','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_COMP_06','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('AV_26','Adult Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_COMP_01','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_38','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_39','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_40','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_41','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_44','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_47','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_COMP_02','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_42','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_43','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_45','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_46','Children Surveys','PHYSICIAN')
--insert into dbo.MeasureTopicTemp (MeasureCode, TopicName,ClassType )  values('CD_48','Children Surveys','PHYSICIAN')



MERGE INTO Measures_MeasureTopics MT
USING( SELECT m.Id AS Measure_Id, m.Name AS MeasrureCode, t.Name AS TopicName, t.Id AS Topic_Id
		FROM dbo.Measures m
			INNER JOIN MeasureTopicTemp mp ON mp.MeasureCode = m.Name 
			INNER JOIN	dbo.Topics t  ON t.Name = mp.TopicName
		WHERE m.ClassType = mp.ClassType
 ) AS Temp ON MT.Measure_Id = Temp.Measure_Id AND MT.Topic_Id = Temp.Topic_Id
 WHEN	NOT MATCHED THEN 
	INSERT (Measure_Id, Topic_Id)
	VALUES (Temp.Measure_Id, Temp.Topic_Id);	

DROP TABLE dbo.MeasureTopicTemp

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

