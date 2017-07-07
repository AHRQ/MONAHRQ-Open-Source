-- Import Target Nursing Home --

select
L.provnum AS ProviderId
,L.overall_rating as OverallRating
,L.survey_rating AS SurveyRating
,L.quality_rating AS QualityRating
,L.staffing_rating AS StaffingRating
,L.QM401score
,L.QM402score
,L.QM403score
,L.QM404score
,L.QM405score
,L.QM406score
,L.QM407score
,L.QM408score
,L.QM409score
,L.QM410score
,L.QM411score
,L.QM415score
,L.QM419score
,L.QM424score
,L.QM425score
,L.QM426score
,L.QM430score
,L.QM434score
,L.QM401state
,L.QM402state
,L.QM403state
,L.QM404state
,L.QM405state
,L.QM406state
,L.QM407state
,L.QM408state
,L.QM409state
,L.QM410state
,L.QM411state
,L.QM415state
,L.QM419state
,L.QM424state
,L.QM425state
,L.QM426state
,L.QM430state
,L.QM434state
,QM401 as QM401nation
,QM402 as QM402nation
,QM403 as QM403nation
,QM404 as QM404nation
,QM405 as QM405nation
,QM406 as QM406nation
,QM407 as QM407nation
,QM408 as QM408nation
,QM409 as QM409nation
,QM410 as QM410nation
,QM411 as QM411nation
,QM415 as QM415nation
,QM419 as QM419nation
,QM424 as QM424nation
,QM425 as QM425nation
,QM426 as QM426nation
,QM430 as QM430nation
,QM434 as QM434nation
,L.adj_total as StaffingScore
,s.H_SURVEY_DATE AS HealthSurveyDate
,s.F_SURVEY_DATE AS FireSafetySurveyDate
,s.H_TOT_DFCNCY AS TotalHealthDeficiencies 
,s.F_TOT_DFCNCY AS TotalFireSafetyDeficiencies 
,s.H_SS_MAX AS MostSevereHealthDeficiency
,s.F_SS_MAX AS MostSevereFireSafetyDeficiency
,L.WEIGHTED_ALL_CYCLES_SCORE AS SurveyScore
,L.AIDHRD AS ReportedCNAStaffingHoursperResidentperDay
,L.VOCHRD AS ReportedLPNStaffingHoursperResidentperDay 
,L.RNHRD AS ReportedRNStaffingHoursperResidentperDay
,L.TOTLICHRD AS LicensedStaffingHoursperResidentperDay
,L.TOTHRD AS TotalNurseStaffingHoursperResidentperDay
,L.PTHRD AS PhysicalTherapistStaffingHoursperResidentPerDay
,s.FileDate
from (StateAverages a
Inner Join
(select	x.provnum
		,x.overall_rating
,x.survey_rating
,x.quality_rating
,x.staffing_rating
,x.AIDHRD
		,x.VOCHRD
		,x.RNHRD
		,x.TOTLICHRD
		,x.TOTHRD
		,x.PTHRD
		,x.adj_total
		,x.WEIGHTED_ALL_CYCLES_SCORE
		,'NATION' as country
	    ,IIF(Y.QM401score=-99,NULL,Y.QM401score) as QM401score
	    ,IIF(Y.QM402score=-99,NULL,Y.QM402score) as QM402score
		,IIF(Y.QM403score=-99,NULL,Y.QM403score) as QM403score
		,IIF(Y.QM404score=-99,NULL,Y.QM404score) as QM404score
		,IIF(Y.QM405score=-99,NULL,Y.QM405score) as QM405score
		,IIF(Y.QM406score=-99,NULL,Y.QM406score) as QM406score
		,IIF(Y.QM407score=-99,NULL,Y.QM407score) as QM407score
		,IIF(Y.QM408score=-99,NULL,Y.QM408score) as QM408score
		,IIF(Y.QM409score=-99,NULL,Y.QM409score) as QM409score
		,IIF(Y.QM410score=-99,NULL,Y.QM410score) as QM410score
		,IIF(Y.QM411score=-99,NULL,Y.QM411score) as QM411score
		,IIF(Y.QM415score=-99,NULL,Y.QM415score) as QM415score
		,IIF(Y.QM419score=-99,NULL,Y.QM419score) as QM419score
		,IIF(Y.QM424score=-99,NULL,Y.QM424score) as QM424score
		,IIF(Y.QM425score=-99,NULL,Y.QM425score) as QM425score
		,IIF(Y.QM426score=-99,NULL,Y.QM426score) as QM426score
		,IIF(Y.QM430score=-99,NULL,Y.QM430score) as QM430score
		,IIF(Y.QM434score=-99,NULL,Y.QM434score) as QM434score
		,Z.QM401 as QM401state
		,Z.QM402 as QM402state
		,Z.QM403 as QM403state
		,Z.QM404 as QM404state
		,Z.QM405 as QM405state
		,Z.QM406 as QM406state
		,Z.QM407 as QM407state
		,Z.QM408 as QM408state
		,Z.QM409 as QM409state
		,Z.QM410 as QM410state
		,Z.QM411 as QM411state
		,Z.QM415 as QM415state
		,Z.QM419 as QM419state
		,Z.QM424 as QM424state
		,Z.QM425 as QM425state
		,Z.QM426 as QM426state
		,Z.QM430 as QM430state
		,Z.QM434 as QM434state
		From (ProviderInfo as X
		Inner Join
		(select provnum, MAX(switch(msr_cd = "401",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM401score ,
						MAX(switch(msr_cd = "402",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM402score ,
						MAX(switch(msr_cd = "403",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM403score ,
						MAX(switch(msr_cd = "404",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM404score ,
						MAX(switch(msr_cd = "405",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM405score ,
						MAX(switch(msr_cd = "406",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM406score ,
						MAX(switch(msr_cd = "407",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM407score ,
						MAX(switch(msr_cd = "408",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM408score ,
						MAX(switch(msr_cd = "409",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM409score ,
						MAX(switch(msr_cd = "410",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM410score ,
						MAX(switch(msr_cd = "411",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM411score ,
						MAX(switch(msr_cd = "415",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM415score ,
						MAX(switch(msr_cd = "419",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM419score ,
						MAX(switch(msr_cd = "424",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM424score ,
						MAX(switch(msr_cd = "425",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM425score ,
						MAX(switch(msr_cd = "426",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM426score ,
						MAX(switch(msr_cd = "430",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM430score ,
						MAX(switch(msr_cd = "434",CDBL(IIF(ISNULL(measure_score_3qtr_avg),-99,measure_score_3qtr_avg)))) as QM434score  
		from QualityMsr_MDS
		group by provnum) Y
		on x.provnum=y.provnum) 
		Inner Join StateAverages Z
		on x.STATE = z.STATE) L
			on L.country = a.state)
		LEFT JOIN (select * from SurveySummary where cycle = 1)  AS s
		 on L.provnum = s.PROVNUM
 where a.state = 'NATION';