IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGenerateHedisReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGenerateHedisReport]
GO


CREATE PROCEDURE [dbo].[spGenerateHedisReport]
	@csvStates varchar(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRY

		if(OBJECT_ID(N'tempdb..#HedisTargetToMeasureMap')) is not null 
	drop table #HedisTargetToMeasureMap
		
		Create table #HedisTargetToMeasureMap(PhysicianRateColumn nvarchar(30), MeasureName nvarchar(30), PeerRateColumn nvarchar(30))
		
		insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('DiabHbA1cTest','PHY-HEDIS-01','DiabHbA1CTestSTAVG')
		insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('DiabHbA1cControl','PHY-HEDIS-02','DiabHbA1CControlSTAVG')
		insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('DiabBPControl','PHY-HEDIS-03','DiabBPControlSTAVG')
		insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('AsthMedicationRatio','PHY-HEDIS-04','AsthMedicationRatioSTAVG')			
		--insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('CardCondLDLCScreening','PHY-HEDIS-05','CardCondLdlCScreeningSTAVG')			
		--insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('CardConditionsLDLCControl','PHY-HEDIS-06','CardConditionsLdlCControlSTAVG')
		insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('HyperBPControl','PHY-HEDIS-07','HyperBPControlSTAVG')	
		insert into #HedisTargetToMeasureMap(PhysicianRateColumn, MeasureName, PeerRateColumn) values('COPD','PHY-HEDIS-08','COPDSTAVG')
			
		;with MappingValues
		as
		(
			select p.MedicalPracticeId as PracticeId, map.Id as MeasureId, case  
				when map.PhysicianRateColumn = 'DiabHbA1cTest' and map.PeerRateColumn ='DiabHbA1CTestSTAVG' then (select top 1 DiabHbA1cTest from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'DiabHbA1cControl' and map.PeerRateColumn ='DiabHbA1CControlSTAVG' then (select top 1 DiabHbA1cControl from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'DiabBPControl' and map.PeerRateColumn ='DiabBPControlSTAVG'  then (select top 1 DiabBPControl from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'AsthMedicationRatio' and map.PeerRateColumn ='AsthMedicationRatioSTAVG'  then (select top 1 AsthMedicationRatio from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
	--			when map.PhysicianRateColumn = 'CardCondLDLCScreening' and map.PeerRateColumn ='CardCondLdlCScreeningSTAVG' then (select top 1 CardCondLDLCScreening from Targets_PhysicianHEDISTargets th where th.PhyNpi = p.PhyNpi)
	--			when map.PhysicianRateColumn = 'CardConditionsLDLCControl' and map.PeerRateColumn ='CardConditionsLdlCControlSTAVG' then (select top 1 CardConditionsLDLCControl from Targets_PhysicianHEDISTargets th where th.PhyNpi = p.PhyNpi)
				when map.PhysicianRateColumn = 'HyperBPControl' and map.PeerRateColumn ='HyperBPControlSTAVG' then (select top 1 HyperBPControl from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'COPD' and map.PeerRateColumn ='COPDSTAVG' then (select top 1 COPD from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				end as PhysicianRate,
				case 
				when map.PhysicianRateColumn = 'DiabHbA1cTest' and map.PeerRateColumn ='DiabHbA1CTestSTAVG' then (select top 1 DiabBPControlSTAVG from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'DiabHbA1cControl' and map.PeerRateColumn ='DiabHbA1CControlSTAVG' then (select top 1 DiabHbA1CControlSTAVG from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'DiabBPControl' and map.PeerRateColumn ='DiabBPControlSTAVG'  then (select top 1 DiabBPControlSTAVG from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'AsthMedicationRatio' and map.PeerRateColumn ='AsthMedicationRatioSTAVG'  then (select top 1 AsthMedicationRatioSTAVG from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
	--			when map.PhysicianRateColumn = 'CardCondLDLCScreening' and map.PeerRateColumn ='CardCondLdlCScreeningSTAVG' then (select top 1 CardCondLdlCScreeningSTAVG from Targets_PhysicianHEDISTargets th where th.PhyNpi = p.PhyNpi)
	--			when map.PhysicianRateColumn = 'CardConditionsLDLCControl' and map.PeerRateColumn ='CardConditionsLdlCControlSTAVG' then (select top 1 CardConditionsLdlCControlSTAVG from Targets_PhysicianHEDISTargets th where th.PhyNpi = p.PhyNpi)
				when map.PhysicianRateColumn = 'HyperBPControl' and map.PeerRateColumn ='HyperBPControlSTAVG' then (select top 1 HyperBPControlSTAVG from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				when map.PhysicianRateColumn = 'COPD' and map.PeerRateColumn ='COPDSTAVG' then (select top 1  COPDSTAVG from Targets_PhysicianHEDISTargets th where th.MedicalPracticeId = p.MedicalPracticeId)
				end as PeerRate
			from (
					select m.Id, map.PhysicianRateColumn, map.PeerRateColumn
					from  Measures m 
						inner join #HedisTargetToMeasureMap map on m.Name = map.MeasureName
				) as map
				cross join Targets_PhysicianHEDISTargets p
			group by p.PhyNpi, p.MedicalPracticeId, map.Id, map.PhysicianRateColumn, map.PeerRateColumn 
		)
		
		
		select p.Npi, PracticeId,  MeasureId, PhysicianRate, PeerRate 
		from MappingValues	mv
			inner join MedicalPractices mp on mv.PracticeId = mp.GroupPracticePacId
			inner join Physicians_MedicalPractices pmp on pmp.MedicalPractice_Id = mp.Id
			inner join Physicians p on p.Id = pmp.Physician_Id
			inner join Addresses a on a.MedicalPractice_Id = mp.Id
		where Npi is not null and MeasureId is not null 
			and  exists(SELECT * from dbo.fnList2Table(@csvStates, ',') s where s.Item = a.State)
		group by p.Npi, PracticeId,  MeasureId, PhysicianRate, PeerRate 
		order by Npi


	END TRY

	BEGIN CATCH
		
		SELECT 
		 ERROR_NUMBER() AS ErrorNumber
		,ERROR_MESSAGE() AS ErrorMessage;
       
	END CATCH
	
END


GO