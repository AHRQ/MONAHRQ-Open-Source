﻿
IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spInitNursingHomeReport_V3' 
)
   DROP PROCEDURE dbo.spInitNursingHomeReport_V3
GO

if exists(select * from sys.types where name = 'InitNursingHomeReportstringIdentifiers_V3')
    drop type InitNursingHomeReportstringIdentifiers_V3
go

create type InitNursingHomeReportstringIdentifiers_V3 AS TABLE 
( id nvarchar(max));
go


IF OBJECT_ID('temp_QM_overall') IS NOT NULL
    DROP TABLE temp_QM_overall

IF OBJECT_ID('temp_QM_percent') IS NOT NULL
    DROP TABLE temp_QM_percent

IF OBJECT_ID('QMscore_staging') IS NOT NULL
    DROP TABLE QMscore_staging
go


CREATE PROCEDURE dbo.spInitNursingHomeReport_V3
	@NursingHomes InitNursingHomeReportstringIdentifiers_V3 readonly,
	@DatasetID int
AS
BEGIN
    SET NOCOUNT ON;


DECLARE @sql VARCHAR(8000)=''

IF OBJECT_ID('temp_QM_overall') IS NOT NULL
    DROP TABLE temp_QM_overall

IF OBJECT_ID('temp_QM_percent') IS NOT NULL
    DROP TABLE temp_QM_percent

IF OBJECT_ID('QMscore_staging') IS NOT NULL
    DROP TABLE QMscore_staging

select
	DISTINCT
	tnht.ProviderId,
	case when coalesce (
										 QM401score
										,QM401score
										,QM402score
										,QM403score
										,QM404score
										,QM405score
										,QM406score
										,QM407score
										,QM408score
										,QM409score
										,QM410score
										,QM411score
										,QM415score
										,QM419score
										,QM471score
										,QM521score
										,QM522score
										,QM523score
										) is null then 'SHORT STAY          '
						  when coalesce(
										 QM424score
										,QM425score
										,QM426score
										,QM430score
										,QM434score
										,QM451score
										,QM452score
										) is null then 'LONG STAY           '
						  else 'LONG/SHORT STAY' end as facility_type,
	nh.STATE,
	case 
		when cast(QM401score as real) is null 
		then case when cast(QM401state as real) IS null
			then CAST(QM401Nation as real)
			else CAST(QM401state as real) end
		else cast(QM401score as real) 
		end as QM401_adjscore,
	case 
		when cast(QM402score as real) is null 
		then case when cast(QM402state as real) IS null
			then CAST(QM402Nation as real)
			else CAST(QM402state as real) end
		else cast(QM402score as real) 
		end as QM402_adjscore,
	case 
		when cast(QM403score as real) is null 
		then case when cast(QM403state as real) IS null
			then CAST(QM403Nation as real)
			else CAST(QM403state as real) end
		else cast(QM403score as real) 
		end as QM403_adjscore,
	case 
		when cast(QM404score as real) is null 
		then case when cast(QM404state as real) IS null
			then CAST(QM404Nation as real)
			else CAST(QM404state as real) end
		else cast(QM404score as real) 
		end as QM404_adjscore,
	case 
		when cast(QM405score as real) is null 
		then case when cast(QM405state as real) IS null
			then CAST(QM405Nation as real)
			else CAST(QM405state as real) end
		else cast(QM405score as real) 
		end as QM405_adjscore,
	case 
		when cast(QM406score as real) is null 
		then case when cast(QM406state as real) IS null
			then CAST(QM406Nation as real)
			else CAST(QM406state as real) end
		else cast(QM406score as real) 
		end as QM406_adjscore,
	case 
		when cast(QM407score as real) is null 
		then case when cast(QM407state as real) IS null
			then CAST(QM407Nation as real)
			else CAST(QM407state as real) end
		else cast(QM407score as real) 
		end as QM407_adjscore,
	case 
		when cast(QM408score as real) is null 
		then case when cast(QM408state as real) IS null
			then CAST(QM408Nation as real)
			else CAST(QM408state as real) end
		else cast(QM408score as real) 
		end as QM408_adjscore,
	case 
		when cast(QM409score as real) is null 
		then case when cast(QM409state as real) IS null
			then CAST(QM409Nation as real)
			else CAST(QM409state as real) end
		else cast(QM409score as real) 
		end as QM409_adjscore,
	case 
		when cast(QM410score as real) is null 
		then case when cast(QM410state as real) IS null
			then CAST(QM410Nation as real)
			else CAST(QM410state as real) end
		else cast(QM410score as real) 
		end as QM410_adjscore,
	case 
		when cast(QM411score as real) is null 
		then case when cast(QM411state as real) IS null
			then CAST(QM411Nation as real)
			else CAST(QM411state as real) end
		else cast(QM411score as real) 
		end as QM411_adjscore,
	case 
		when cast(QM415score as real) is null 
		then case when cast(QM415state as real) IS null
			then CAST(QM415Nation as real)
			else CAST(QM415state as real) end
		else cast(QM415score as real) 
		end as QM415_adjscore,
	case 
		when cast(QM419score as real) is null 
		then case when cast(QM419state as real) IS null
			then CAST(QM419Nation as real)
			else CAST(QM419state as real) end
		else cast(QM419score as real) 
		end as QM419_adjscore,

	--[[ V3
	case 
		when cast(QM471score as real) is null 
		then case when cast(QM471state as real) IS null
			then CAST(QM471Nation as real)
			else CAST(QM471state as real) end
		else cast(QM471score as real) 
		end as QM471_adjscore,
	case 
		when cast(QM521score as real) is null 
		then case when cast(QM521state as real) IS null
			then CAST(QM521Nation as real)
			else CAST(QM521state as real) end
		else cast(QM521score as real) 
		end as QM521_adjscore,
	case 
		when cast(QM522score as real) is null 
		then case when cast(QM522state as real) IS null
			then CAST(QM522Nation as real)
			else CAST(QM522state as real) end
		else cast(QM522score as real) 
		end as QM522_adjscore,
	case 
		when cast(QM523score as real) is null 
		then case when cast(QM523state as real) IS null
			then CAST(QM523Nation as real)
			else CAST(QM523state as real) end
		else cast(QM523score as real) 
		end as QM523_adjscore,		
	case 
		when cast(QM451score as real) is null 
		then case when cast(QM451state as real) IS null
			then CAST(QM451Nation as real)
			else CAST(QM451state as real) end
		else cast(QM451score as real) 
		end as QM451_adjscore,
--	case 
--		when cast(QM452score as real) is null 
--		then case when cast(QM452state as real) IS null
--			then CAST(QM452Nation as real)
--			else CAST(QM452state as real) end
--		else cast(QM452score as real) 
--		end as QM452_adjscore,
	-- End V3 ]]

	case 
		when cast(QM424score as real) is null 
		then case when cast(QM424state as real) IS null
			then CAST(QM424Nation as real)
			else CAST(QM424state as real) end
		else cast(QM424score as real) 
		end as QM424_adjscore,
	case 
		when cast(QM425score as real) is null 
		then case when cast(QM425state as real) IS null
			then CAST(QM425Nation as real)
			else CAST(QM425state as real) end
		else cast(QM425score as real) 
		end as QM425_adjscore,
	case 
		when cast(QM426score as real) is null 
		then case when cast(QM426state as real) IS null
			then CAST(QM426Nation as real)
			else CAST(QM426state as real) end
		else cast(QM426score as real) 
		end as QM426_adjscore,
	case 
		when cast(QM430score as real) is null 
		then case when cast(QM430state as real) IS null
			then CAST(QM430Nation as real)
			else CAST(QM430state as real) end
		else cast(QM430score as real) 
		end as QM430_adjscore,
	case 
		when cast(QM434score as real) is null 
		then case when cast(QM434state as real) IS null
			then CAST(QM434Nation as real)
			else CAST(QM434state as real) end
		else cast(QM434score as real) 
		end as QM434_adjscore,
	cast(0 as real) as QM401_percentile,
	cast(0 as real) as QM402_percentile,
	cast(0 as real) as QM403_percentile,
	cast(0 as real) as QM406_percentile,
	cast(0 as real) as QM407_percentile,
	cast(0 as real) as QM409_percentile,
	cast(0 as real) as QM410_percentile,
	cast(0 as real) as QM419_percentile,
	cast(0 as real) as QM424_percentile,
	cast(0 as real) as QM425_percentile,
	cast(0 as real) as QM434_percentile,
	--[[ V3
	cast(0 as real) as QM451_percentile,
	cast(0 as real) as QM471_percentile,
	cast(0 as real) as QM521_percentile,
	cast(0 as real) as QM522_percentile,
	cast(0 as real) as QM523_percentile,
	-- End V3 ]]
	cast(0 as real) as QMtotal_raw,
	cast(0 as real) as QMtotal_percentile
into dbo.QMscore_staging
FROM Targets_NursingHomeTargets tnht
INNER JOIN NursingHomes nh
ON nh.ProviderId = tnht.ProviderId
inner join @NursingHomes snh on snh.id=nh.ProviderId
where tnht.[Dataset_Id]=@DatasetID


IF OBJECT_ID('NursingHomeQMMeasureScoreFlagTmp') IS NOT NULL
DROP TABLE NursingHomeQMMeasureScoreFlagTmp 

/*******************************************
 *  Below table is created to make "Higher is Better"
 *  or "Lower is Better" dynamic. With help of this table,
 *  now it will only be required to change only
 *  HigherScoresAreBetter column value in Measures table
 *  When Quality measure wise "Higher is Better" or
 *  "Lower is better" flag will be changed
 *******************************************/

SELECT DISTINCT NAME,
CASE WHEN NAME LIKE 'NH-QM-SS%' THEN 1 ELSE 0 END IsSortStay,
CASE WHEN m.HigherScoresAreBetter=1 THEN 'DESC'
     ELSE 'ASC' END HigherLowerScoreOrder,
CASE WHEN NAME='NH-QM-SS-01' THEN 'QM424'
     WHEN NAME='NH-QM-SS-02' THEN 'QM425'
     WHEN NAME='NH-QM-SS-03' THEN 'QM426'
     WHEN NAME='NH-QM-SS-04' THEN 'QM430'
     WHEN NAME='NH-QM-SS-05' THEN 'QM434'
	 --[[ V3
     WHEN NAME='NH-QM-SS-06' THEN 'QM471'
     WHEN NAME='NH-QM-SS-07' THEN 'QM521'
     WHEN NAME='NH-QM-SS-08' THEN 'QM522'
     WHEN NAME='NH-QM-SS-09' THEN 'QM523'
	 -- End V3 ]]
     WHEN NAME='NH-QM-LS-01' THEN 'QM410'
     WHEN NAME='NH-QM-LS-02' THEN 'QM402'
     WHEN NAME='NH-QM-LS-03' THEN 'QM403'
     WHEN NAME='NH-QM-LS-04' THEN 'QM411'
     WHEN NAME='NH-QM-LS-05' THEN 'QM415'
     WHEN NAME='NH-QM-LS-06' THEN 'QM407'
     WHEN NAME='NH-QM-LS-07' THEN 'QM405'
     WHEN NAME='NH-QM-LS-08' THEN 'QM406'
     WHEN NAME='NH-QM-LS-09' THEN 'QM409'
     WHEN NAME='NH-QM-LS-10' THEN 'QM401'
     WHEN NAME='NH-QM-LS-11' THEN 'QM404'
     WHEN NAME='NH-QM-LS-12' THEN 'QM408'
     WHEN NAME='NH-QM-LS-13' THEN 'QM419'
	 --[[ V3
     WHEN NAME='NH-QM-LS-14' THEN 'QM451'
  -- WHEN NAME='NH-QM-LS-15' THEN 'QM452'
	 -- End V3 ]]
     END QmId
INTO NursingHomeQMMeasureScoreFlagTmp
FROM Measures m
WHERE m.ClassType='NURSINGHOME'
AND m.Source='QualityMsr'
AND m.UsedInCalculations=1 

DECLARE @rowcnt INT= (SELECT COUNT(*) from QMscore_staging)

SELECT @sql=@sql+'((RANK() over (order by '+QmId+'_adjscore '+HigherLowerScoreOrder+'))-1)/CAST ((@rowcnt-1) AS FLOAT)  * 100 as '+QmId+'_percentile,'
FROM NursingHomeQMMeasureScoreFlagTmp nhq
where nhq.QmId is not null

SELECT @sql='DECLARE @rowcnt INT= (SELECT COUNT(*) from QMscore_staging)
    select DISTINCT
	Providerid,
	facility_type,'
	+@sql+
	'cast(0 as real) as QMtotal_Raw
into temp_QM_percent
from QMscore_staging'

EXEC (@SQL)

--select
--	Providerid,
--	facility_type,
--	((RANK() over (order by QM401_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM401_percentile,
--	((RANK() over (order by QM402_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM402_percentile,
--	((RANK() over (order by QM403_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM403_percentile,
--	((RANK() over (order by QM406_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM406_percentile,
--	((RANK() over (order by QM407_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM407_percentile,
--	((RANK() over (order by QM409_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM409_percentile,
--	((RANK() over (order by QM410_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM410_percentile,
--	((RANK() over (order by QM419_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM419_percentile,
--	((RANK() over (order by QM424_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM424_percentile,
--	((RANK() over (order by QM425_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM425_percentile,
--	((RANK() over (order by QM434_adjscore desc))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QM434_percentile,
--	cast(0 as real) as QMtotal_Raw
--into temp_QM_percent
--from QMscore_staging


/* Create Summary Percentile */
/* 2015 update: added QM419 and QM434, adjusted long/short ratios */


/*	Use the variable @msr_wgt to set the weighting of the new measures in the final results.
	Use fraction value of percentage weight: 50% = .5 and 100% = 1								*/
declare @msr_wgt real = .5;

update temp_QM_percent
set QMtotal_Raw = 
	(QM401_percentile + QM402_percentile + QM403_percentile + QM406_percentile + QM407_percentile + QM409_percentile + QM410_percentile + QM419_percentile + QM451_percentile)*(11+(5*@msr_wgt))/(8+(1*@msr_wgt))		--11/8
where ltrim(rtrim(Facility_Type)) = 'LONG STAY'

update temp_QM_percent
set QMtotal_Raw = 
	(QM424_percentile + QM425_percentile + QM434_percentile + 		--)*11/3
--[[ V3
	 QM471_percentile + QM521_percentile + QM522_percentile + QM523_percentile)*(11+(5*@msr_wgt))/(3+(4*@msr_wgt))
--]] End V3

where ltrim(rtrim(Facility_Type)) = 'SHORT STAY'

update temp_QM_percent
set QMtotal_Raw = 
	(QM401_percentile + QM402_percentile + QM403_percentile + QM406_percentile + QM407_percentile + QM409_percentile + 
		QM410_percentile + QM419_percentile + QM424_percentile + QM425_percentile + QM434_percentile +
--[[ V3
		QM451_percentile + QM471_percentile + QM521_percentile + QM522_percentile + QM523_percentile)
--]] End V3
where ltrim(rtrim(Facility_Type)) = 'LONG/SHORT STAY'

/* Create Overall QM Score */

SET @rowcnt = (SELECT COUNT(*) from temp_QM_percent)

select
	ProviderId,
	((RANK() over (order by QMTotal_Raw))-1)/CAST ((@rowcnt-1) AS FLOAT) * 100 as QMtotal_percentile
into temp_QM_overall
from temp_QM_percent


/* Merge into main QM score table */
/* 2015 update: added QM419 and QM434 */


UPDATE QMscore_staging
set	
	QM401_percentile = tp.QM401_percentile,
	QM402_percentile = tp.QM402_percentile,
	QM403_percentile = tp.QM403_percentile,
	QM406_percentile = tp.QM406_percentile,
	QM407_percentile = tp.QM407_percentile,
	QM409_percentile = tp.QM409_percentile,
	QM410_percentile = tp.QM410_percentile,
	QM419_percentile = tp.QM419_percentile,
	QM424_percentile = tp.QM424_percentile,
	QM425_percentile = tp.QM425_percentile,
	QM434_percentile = tp.QM434_percentile,
--[[ V3
	QM451_percentile = tp.QM451_percentile,
	QM471_percentile = tp.QM471_percentile,
	QM521_percentile = tp.QM521_percentile,
	QM522_percentile = tp.QM522_percentile,
	QM523_percentile = tp.QM523_percentile,
--]] End V3
	QMTotal_raw = tp.QMTotal_raw
FROM QMscore_staging qs
	INNER JOIN temp_QM_percent tp
	on qs.providerid = tp.providerid


UPDATE QMscore_staging
set	
	QMtotal_percentile = tv.QMtotal_percentile
FROM QMscore_staging qs
	INNER JOIN temp_QM_overall tv
	on qs.providerid = tv.providerid


/* Remove Temp Tables */

IF OBJECT_ID('temp_QM_overall') IS NOT null
drop table temp_QM_overall

IF OBJECT_ID('temp_QM_percent') IS NOT null
drop table temp_QM_percent


END
GO