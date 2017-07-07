IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spGetNursingHomeReport' 
)
   DROP PROCEDURE dbo.spGetNursingHomeReport
GO

if exists(select * from sys.types where name = 'stringIdentifiers')
    drop type stringIdentifiers
go

create type stringIdentifiers AS TABLE 
( id nvarchar(max));
go

if exists(select * from sys.types where name = 'intIdentifiers')
    drop type intIdentifiers
go

create type intIdentifiers AS TABLE 
( id int primary key);
go






CREATE PROCEDURE dbo.spGetNursingHomeReport
 @States stringIdentifiers readonly,
 @Counties stringIdentifiers readonly,
 @NursingHomes stringIdentifiers readonly,
 @DatasetID int
	
AS
BEGIN
    SET NOCOUNT ON;
/****************************************************************/
/* MONARHQ Nursing Home Compare                                 */
/* Peer Group Comparison - proof of concept code                */
/*                                                              */
/* Use variables below to set up sample comparisons             */
/* In where statement below, uncomment used groupings			*/
/* and comment unused groupings.                                */
/*                                                              */
/* Keith Bell                                                   */
/* Copyright 2014 Health Data Decisions                         */
/****************************************************************/


declare @ownership nvarchar(64);
declare @residents varchar(5);
declare @beds varchar(5);
declare @CCRC nvarchar(1);
declare @factype varchar(20);
declare @inhosp nvarchar(3);
declare @sff nvarchar(1);
declare @county nvarchar(50);
set @beds = '1-100';
DECLARE @rowcnt INT=(SELECT COUNT(1) FROM Targets_NursingHomeTargets nm
                 INNER join QMscore_staging qs on nm.ProviderId = qs.ProviderId
                 INNER JOIN NursingHomes nh ON nm.ProviderId=nh.ProviderId
                 inner join @States s on nh.STATE =s.id
				 inner join @Counties c on nh.CountyName =c.id
				 inner join @NursingHomes snh on snh.id=nh.ProviderId
				where 
				nm.[Dataset_Id]=@DatasetID
			
				 );--(SELECT * FROM @States));

if @rowcnt=1 return

select
	nh.id as nhid,
	nm.ProviderId,
	nh.NAME,
	nm.OverallRating,
	nm.qualityrating,
	qs.QMtotal_raw as [QM SCore],
	(((RANK() over (order by qs.QMtotal_raw))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 as [QM %ile],
	case
		when (((RANK() over (order by qs.QMtotal_raw))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 < 10
			then 2
		when (((RANK() over (order by qs.QMtotal_raw))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 >= 90
			then 4
		else 3 end
		as [QM Comparison],
	case
		when (((RANK() over (order by qs.QMtotal_raw))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 < 10
			then 1
		when (((RANK() over (order by qs.QMtotal_raw))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 between 10 and (10+(80/3))
			then 2
		when (((RANK() over (order by qs.QMtotal_raw))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 between (89-(80/3)) and 89
			then 4
		when (((RANK() over (order by qs.QMtotal_raw))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 >= 90
			then 5
		else 3 end
		as [qualityrating Comparison 5pt],
	nm.staffingrating,
	nm.StaffingScore as [Staffing Score],
	(((RANK() over (order by nm.StaffingScore))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 as [Staffing %ile],
	case
		when (((RANK() over (order by nm.StaffingScore))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 < 10
			then 2
		when (((RANK() over (order by nm.StaffingScore))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 >= 90
			then 4
		else 3 end
		as [Staffing Comparison],
	case
		when (((RANK() over (order by nm.StaffingScore))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 < 10
			then 1
		when (((RANK() over (order by nm.StaffingScore))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 between 10 and (10+(80/3))
			then 2
		when (((RANK() over (order by nm.StaffingScore))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 between (89-(80/3)) and 89
			then 4
		when (((RANK() over (order by nm.StaffingScore))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 >= 90
			then 5
		else 3 end as [staffingrating Comparison 5pt],
	nm.surveyrating,
	nm.SurveyScore as [Survey Score],
	(((RANK() over (order by nm.SurveyScore desc))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 as [Survey %ile],
	case
		when (((RANK() over (order by nm.SurveyScore desc))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 < 10
			then 2
		when (((RANK() over (order by nm.SurveyScore desc))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 >= 90
			then 4
		else 3 end
		as [Survey Comparison],
	case
		when (((RANK() over (order by nm.SurveyScore desc))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 < 10
			then 1
		when (((RANK() over (order by nm.SurveyScore desc))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 between 10 and (10+(80/3))
			then 2
		when (((RANK() over (order by nm.SurveyScore desc))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 between (89-(80/3)) and 89
			then 4
		when (((RANK() over (order by nm.SurveyScore desc))-1)/CAST ((@rowcnt-1) AS FLOAT))*100 >= 90
			then 5
		else 3 end
		as [surveyrating Comparison 5pt],
	nh.[State],
	nh.Ownership,
	case when nh.NumberCertBeds <= 100 then '1-100'
						  when nh.NumberCertBeds > 100 then '>100'
						  else null end as BedsSize,
	case when nh.NumberResidCertBeds <= 80 then '1-80'
						  when nh.NumberResidCertBeds > 80 then '>80'
						  else null END ResSize,
	qs.facility_type,
	nh.IsCCRCFacility,
	nh.InHospital,
	nh.IsSFFacility,
	nh.CountyName
FROM Targets_NursingHomeTargets nm
INNER join QMscore_staging qs on nm.ProviderId = qs.ProviderId
INNER JOIN NursingHomes nh ON nm.ProviderId=nh.ProviderId

inner join @States s on nh.STATE =s.id
inner join @Counties c on nh.CountyName =c.id
inner join @NursingHomes snh on snh.id=nh.ProviderId
where 
	nm.[Dataset_Id]=@DatasetID 

	END
GO