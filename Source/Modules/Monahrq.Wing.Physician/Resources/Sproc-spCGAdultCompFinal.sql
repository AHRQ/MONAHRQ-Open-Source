/********************************************************************************************************************************/
/********************************************************************************************************************************/
/*																																*/
/*	Script: CG Adult Comp Final																									*/
/*  Version: 1.0																												*/
/*	Last modified: 4/13/2016																									*/
/*	Authors: John Hoff <jhoff@healthdatadecisions.com> and Patrick McGrath <pmcgrath@healthdatadecisions.com>					*/
/*  Change History:																												*/
/*		4/13/2016 - version 1.0 created																						    */
/*      4/27/2016 - Final version created																						*/
/********************************************************************************************************************************/
/*Builds adult measure level rates, composite rates, overall adult composite, stars and percentiles for peer group				*/


if(Object_id(N'spCGAdultCompFinal')) is not null 
	drop procedure spCGAdultCompFinal
go

create procedure spCGAdultCompFinal
	@WebsiteId as int
as
begin
	declare @min_den int;
	set @min_den = 20;
	
	select			[CGPracticeId]
				,	max(AdultPracticeSampleSize) as Adult_Samp
					/*AV 06*/
				,	case when sum(case when AV_06 in ('1','2','3','4') then 1 else 0 end) = 0
						then null
						else  (cast(sum(case when AV_06 = '4' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_06 in ('1','2','3','4')  then 1 else 0 end) as dec(10,2)))
					end as AV_06_Rate 
				,	count(AV_06) as AV_06_TOT
				,	sum(case when AV_06 in ('1','2','3','4') then 1 else 0 end) as AV_06_DENOM
				,	sum(case when AV_06 = '4' then 1 else 0 end) as AV_06_NUM
					/*AV 08*/
				,	case when sum(case when AV_08 in ('1','2','3','4') then 1 else 0 end) = 0
						then null
						else  (cast(sum(case when AV_08 = '4' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_08 in ('1','2','3','4')  then 1 else 0 end) as dec(10,2)))
					end as AV_08_Rate 
				,	count(AV_08) as AV_08_TOT
				,	sum(case when AV_08 in ('1','2','3','4') then 1 else 0 end) as AV_08_DENOM
				,	sum(case when AV_08 = '4' then 1 else 0 end) as AV_08_NUM
					/*AV 10*/
				,	case when sum(case when AV_10 in ('1','2','3','4') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_10 = '4' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_10 in ('1','2','3','4')  then 1 else 0 end) as dec(10,2)))
					end as AV_10_Rate 
				,	count(AV_10) as AV_10_TOT
				,	sum(case when AV_10 in ('1','2','3','4') then 1 else 0 end) as AV_10_DENOM
				,	sum(case when AV_10 = '4' then 1 else 0 end) as AV_10_NUM
					/*AV 12*/
				,	case when sum(case when AV_12 in ('1','2','3','4') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_12 = '4' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_12 in ('1','2','3','4')  then 1 else 0 end) as dec(10,2)))
					end as AV_12_Rate 
				,	count(AV_12) as AV_12_TOT
				,	sum(case when AV_12 in ('1','2','3','4') then 1 else 0 end) as AV_12_DENOM
				,	sum(case when AV_12 = '4' then 1 else 0 end) as AV_12_NUM
					/*AV 13*/
				,	case when sum(case when AV_13 in ('1','2','3','4') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_13 = '4' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_13 in ('1','2','3','4')  then 1 else 0 end) as dec(10,2)))
					end as AV_13_Rate 
				,	count(AV_13) as AV_13_TOT
				,	sum(case when AV_13 in ('1','2','3','4') then 1 else 0 end) as AV_13_DENOM
				,	sum(case when AV_13 = '4' then 1 else 0 end) as AV_13_NUM
					/*AV 16*/
				,	case when sum(case when AV_16 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_16 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_16 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_16_Rate 
				,	count(AV_16) as AV_16_TOT
				,	sum(case when AV_16 in ('1','2','3') then 1 else 0 end) as AV_16_DENOM
				,	sum(case when AV_16 = '1' then 1 else 0 end) as AV_16_NUM
					/*AV 17*/
				,	case when sum(case when AV_17 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_17 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_17 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_17_Rate 
				,	count(AV_17) as AV_17_TOT
				,	sum(case when AV_17 in ('1','2','3') then 1 else 0 end) as AV_17_DENOM
				,	sum(case when AV_17 = '1' then 1 else 0 end) as AV_17_NUM
					/*AV 19*/
				,	case when sum(case when AV_19 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_19 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_19 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_19_Rate 
				,	count(AV_19) as AV_19_TOT
				,	sum(case when AV_19 in ('1','2','3') then 1 else 0 end) as AV_19_DENOM
				,	sum(case when AV_19 = '1' then 1 else 0 end) as AV_19_NUM
					/*AV 20*/
				,	case when sum(case when AV_20 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_20 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_20 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_20_Rate 
				,	count(AV_20) as AV_20_TOT
				,	sum(case when AV_20 in ('1','2','3') then 1 else 0 end) as AV_20_DENOM
				,	sum(case when AV_20 = '1' then 1 else 0 end) as AV_20_NUM
					/*AV 21*/
				,	case when sum(case when AV_21 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_21 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_21 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_21_Rate 
				,	count(AV_21) as AV_21_TOT
				,	sum(case when AV_21 in ('1','2','3') then 1 else 0 end) as AV_21_DENOM
				,	sum(case when AV_21 = '1' then 1 else 0 end) as AV_21_NUM
					/*AV 22*/
				,	case when sum(case when AV_22 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_22 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_22 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_22_Rate 
				,	count(AV_22) as AV_22_TOT
				,	sum(case when AV_22 in ('1','2','3') then 1 else 0 end) as AV_22_DENOM
				,	sum(case when AV_22 = '1' then 1 else 0 end) as AV_22_NUM
					/*AV 24*/
				,	case when sum(case when AV_24 in ('1','2') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_24 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_24 in ('1','2')  then 1 else 0 end) as dec(10,2)))
					end as AV_24_Rate 
				,	count(AV_24) as AV_24_TOT
				,	sum(case when AV_24 in ('1','2') then 1 else 0 end) as AV_24_DENOM
				,	sum(case when AV_24 = '1' then 1 else 0 end) as AV_24_NUM
					/*AV 25*/
				,	case when sum(
						case when AV_25 in ('0','1','2','3','4','5','6','7','8','9','10') then 1 else 0 end) = 0
							then null
							else	(cast(sum(case when (AV_25 = '9' or AV_25 = '10') then 1 else 0 end) as dec(10,2))) / 
									(cast(sum(case when AV_25 in ('0','1','2','3','4','5','6','7','8','9','10')  then 1 else 0 end) as dec(10,2)))
					end as AV_25_Rate 
				,	count(AV_25) as AV_25_TOT
				,	sum(case when AV_25 in ('0','1','2','3','4','5','6','7','8','9','10') then 1 else 0 end) as AV_25_DENOM
				,	sum(case when (AV_25 = '9' or AV_25 = '10') then 1 else 0 end) as AV_25_NUM
					/*AV 26*/
				,	case when sum(case when AV_26 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_26 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_26 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_26_Rate 
				,	count(AV_26) as AV_26_TOT
				,	sum(case when AV_26 in ('1','2','3') then 1 else 0 end) as AV_26_DENOM
				,	sum(case when AV_26 = '1' then 1 else 0 end) as AV_26_NUM
					/*AV 27*/
				,	case when sum(case when AV_27 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_27 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_27 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_27_Rate 
				,	count(AV_27) as AV_27_TOT
				,	sum(case when AV_27 in ('1','2','3') then 1 else 0 end) as AV_27_DENOM
				,	sum(case when AV_27 = '1' then 1 else 0 end) as AV_27_NUM
					/*AV 28*/
				,	case when sum(case when AV_28 in ('1','2','3') then 1 else 0 end) = 0 then null else  (cast(sum(case when AV_28 = '1' then 1 else 0 end) as dec(10,2))) / (cast(sum(case when AV_28 in ('1','2','3')  then 1 else 0 end) as dec(10,2)))
					end as AV_28_Rate 
				,	count(AV_28) as AV_28_TOT
				,	sum(case when AV_28 in ('1','2','3') then 1 else 0 end) as AV_28_DENOM
				,	sum(case when AV_28 = '1' then 1 else 0 end) as AV_28_NUM
	into			[CG_adult_working]
	from			[Targets_CGCAHPSSurveyTargets]	--[CG_adult]
	where			[Dataset_Id] in (select wd.Dataset_Id from Websites_WebsiteDatasets wd where wd.Website_Id = @WebsiteId)
	group by		CGPracticeId


	select			CGPracticeId
				,	Adult_Samp
					/*COMP1*/
				,	case when AV_06_DENOM > @min_den then AV_06_Rate else 0 end as AV_06_Comp
				,	case when AV_08_DENOM > @min_den then AV_08_Rate else 0 end as AV_08_Comp
				,	case when AV_10_DENOM > @min_den then AV_10_Rate else 0 end as AV_10_Comp
				,	case when AV_12_DENOM > @min_den then AV_12_Rate else 0 end as AV_12_Comp
				,	case when AV_13_DENOM > @min_den then AV_13_Rate else 0 end as AV_13_Comp

					/*COMP2*/
				,	case when AV_16_DENOM > @min_den then AV_16_Rate else 0 end as AV_16_Comp
				,	case when AV_17_DENOM > @min_den then AV_17_Rate else 0 end as AV_17_Comp
				,	case when AV_19_DENOM > @min_den then AV_19_Rate else 0 end as AV_19_Comp
				,	case when AV_20_DENOM > @min_den then AV_20_Rate else 0 end as AV_20_Comp
				,	case when AV_21_DENOM > @min_den then AV_21_Rate else 0 end as AV_21_Comp
				,	case when AV_22_DENOM > @min_den then AV_22_Rate else 0 end as AV_22_Comp

					/*COMP3*/
				,	case when AV_24_DENOM > @min_den then AV_24_Rate else 0 end as AV_24_Comp

					/*COMP4*/
				,	case when AV_25_DENOM > @min_den then AV_25_Rate else 0 end as AV_25_Comp

					/*COMP5*/
				,	case when AV_26_DENOM > @min_den then AV_26_Rate else 0 end as AV_26_Comp

					/*COMP6*/
				,	case when AV_27_DENOM > @min_den then AV_27_Rate else 0 end as AV_27_Comp
				,	case when AV_28_DENOM > @min_den then AV_28_Rate else 0 end as AV_28_Comp

	into			[CG_adult_working2]
	from			[CG_adult_working]
 

	select			CGPracticeId
				,	Adult_Samp
					/*COMP1*/
				,	case when
						(	sum(case when AV_06_COMP <> 0 then 1 else 0 end) + 
							sum(case when AV_08_COMP <> 0 then 1 else 0 end) + 
							sum(case when AV_10_COMP <> 0 then 1 else 0 end) + 
							sum(case when AV_12_COMP <> 0 then 1 else 0 end) + 
							sum(case when AV_13_COMP <> 0 then 1 else 0 end)) = 0
								then null
								else	(AV_06_COMP + AV_08_COMP + AV_10_COMP + AV_12_COMP + AV_13_COMP) /
										((	sum(case when AV_06_COMP <> 0 then 1 else 0 end) + 
											sum(case when AV_08_COMP <> 0 then 1 else 0 end) + 
											sum(case when AV_10_COMP <> 0 then 1 else 0 end) + 
											sum(case when AV_12_COMP <> 0 then 1 else 0 end) + 
											sum(case when AV_13_COMP <> 0 then 1 else 0 end))) end as AV_COMP_01

					/*COMP2*/
				,	case when
					(	sum(case when AV_16_COMP <> 0 then 1 else 0 end) +
						sum(case when AV_17_COMP <> 0 then 1 else 0 end) + 
						sum(case when AV_19_COMP <> 0 then 1 else 0 end) + 
						sum(case when AV_20_COMP <> 0 then 1 else 0 end) + 
						sum(case when AV_21_COMP <> 0 then 1 else 0 end) + 
						sum(case when AV_22_COMP <> 0 then 1 else 0 end)) = 0
							then null
							else	(AV_16_COMP + AV_17_COMP + AV_19_COMP + AV_20_COMP + AV_21_COMP + AV_22_COMP) /
									((	sum(case when AV_16_COMP <> 0 then 1 else 0 end) + 
										sum(case when AV_17_COMP <> 0 then 1 else 0 end) + 
										sum(case when AV_19_COMP <> 0 then 1 else 0 end) + 
										sum(case when AV_20_COMP <> 0 then 1 else 0 end) + 
										sum(case when AV_21_COMP <> 0 then 1 else 0 end) +
										sum(case when AV_22_COMP <> 0 then 1 else 0 end))) end as AV_COMP_02

					/*COMP3*/
				,	case when
					(	sum(case when AV_27_COMP <> 0 then 1 else 0 end) +
						sum(case when AV_28_COMP <> 0 then 1 else 0 end)) = 0
							then null
							else	(AV_27_COMP + AV_28_COMP) /
									((	sum(case when AV_27_COMP <> 0 then 1 else 0 end) +
										sum(case when AV_28_COMP <> 0 then 1 else 0 end))) end as AV_COMP_03

					/*COMP3*/
				,	AV_24_Comp as AV_COMP_04
					/*COMP4*/
				,	AV_25_Comp as Av_COMP_05
					/*COMP5*/
				,	AV_26_Comp as Av_COMP_06

	into			[CG_adult_working3]
	from			[CG_adult_working2]
	group by		CGPracticeId
				,	Adult_Samp
				,	AV_06_Comp
				,	AV_08_Comp
				,	AV_10_Comp
				,	AV_12_Comp
				,	AV_13_Comp
				,	AV_16_Comp
				,	AV_17_Comp
				,	AV_19_Comp
				,	AV_20_Comp
				,	AV_21_Comp
				,	AV_22_Comp
				,	AV_24_Comp
				,	AV_25_Comp
				,	AV_26_Comp
				,	AV_27_Comp
				,	AV_28_Comp

	/*FINAL ADULT COMPOSITE*/
    select			CGPracticeId
				,	Adult_Samp
				,	case when
					(	sum(case when AV_COMP_01 <> 0 then 1 else 0 end) + 
						sum(case when AV_COMP_02 <> 0 then 1 else 0 end) + 
						sum(case when AV_COMP_03 <> 0 then 1 else 0 end) + 
						sum(case when AV_COMP_04 <> 0 then 1 else 0 end) + 
						sum(case when AV_COMP_05 <> 0 then 1 else 0 end) + 
						sum(case when AV_COMP_06 <> 0 then 1 else 0 end)) = 0
							then	null
							else	(AV_COMP_01 + AV_COMP_02 + AV_COMP_03 + AV_COMP_04 + AV_COMP_05 + AV_COMP_06) /
									((	sum(case when AV_COMP_01 <> 0 then 1 else 0 end) + 
										sum(case when AV_COMP_02 <> 0 then 1 else 0 end) + 
										sum(case when AV_COMP_03 <> 0 then 1 else 0 end) + 
										sum(case when AV_COMP_04 <> 0 then 1 else 0 end) + 
										sum(case when AV_COMP_05 <> 0 then 1 else 0 end) + 
										sum(case when AV_COMP_06 <> 0 then 1 else 0 end))) end as AV_COMP_OVERALL
	into			[CG_adult_FinalComp]
	from			[CG_adult_working3]
	group by		CGPracticeId,Adult_Samp
				,	AV_COMP_01
				,	AV_COMP_02
				,	AV_COMP_03
				,	AV_COMP_04
				,	AV_COMP_05
				,	AV_COMP_06
 
	/*Final Composite Star Ratings and Individual Composite Star Ratings*/
	select			FC.CGPracticeId
				,	FC.Adult_Samp

					/*Final Overall NH Comp Star Rating*/
				,	FC.AV_COMP_OVERALL as CGAdult_COMP_OVERALL
				,	PRANK.AV_COMP_OVERALL as [CGAdult_%ile_Overall]
				,	case
						when PRANK.AV_COMP_OVERALL >= 90	then '5'
						when PRANK.AV_COMP_OVERALL >= 70	then '4'
						when PRANK.AV_COMP_OVERALL >= 50	then '3'
						when PRANK.AV_COMP_OVERALL >= 25	then '2'
						else '1'
					end as [CGAdult_Star_Overall]

					/*CGAdult Comp1 Star Rating*/
				,	IC.AV_COMP_01 as CGAdult_COMP1
				,	PRANK.AV_COMP_01 as [CGAdult_%ile_COMP1]
				,	case
						when PRANK.AV_COMP_01 >= 90	then '5'
						when PRANK.AV_COMP_01 >= 70	then '4'
						when PRANK.AV_COMP_01 >= 50	then '3'
						when PRANK.AV_COMP_01 >= 25	then '2'
						else '1'
					end as [CGAdult_Star_COMP1]

					/*CGAdult Comp2 Star Rating*/
				,	IC.AV_COMP_02 as CGAdult_COMP2
				,	PRANK.AV_COMP_02 as [CGAdult_%ile_COMP2]
				,	case
						when PRANK.AV_COMP_02 >= 90	then '5'
						when PRANK.AV_COMP_02 >= 70	then '4'
						when PRANK.AV_COMP_02 >= 50	then '3'
						when PRANK.AV_COMP_02 >= 25	then '2'
						else '1'
					end as [CGAdult_Star_COMP2]

					/*CGAdult Comp3 Star Rating*/
				,	IC.AV_COMP_03 as CGAdult_COMP3
				,	PRANK.AV_COMP_03 as [CGAdult_%ile_COMP3]
				,	case
						when PRANK.AV_COMP_03 >= 90	then '5'
						when PRANK.AV_COMP_03 >= 70	then '4'
						when PRANK.AV_COMP_03 >= 50	then '3'
						when PRANK.AV_COMP_03 >= 25	then '2'
						else '1'
					end as [CGAdult_Star_COMP3]

					/*CGAdult Comp4 Star Rating*/
				,	IC.AV_COMP_04 as CGAdult_COMP4
				,	PRANK.AV_COMP_04 as [CGAdult_%ile_COMP4]
				,	case
						when PRANK.AV_COMP_04 >= 90	then '5'
						when PRANK.AV_COMP_04 >= 70	then '4'
						when PRANK.AV_COMP_04 >= 50	then '3'
						when PRANK.AV_COMP_04 >= 25	then '2'
						else '1'
					end as [CGAdult_Star_COMP4]

					/*CGAdult Comp5 Star Rating*/
				,	IC.AV_COMP_05 as CGAdult_COMP5
				,	PRANK.AV_COMP_05 as [CGAdult_%ile_COMP5]
				,	case
						when PRANK.AV_COMP_05 >= 90	then '5'
						when PRANK.AV_COMP_05 >= 70	then '4'
						when PRANK.AV_COMP_05 >= 50	then '3'
						when PRANK.AV_COMP_05 >= 25	then '2'
						else '1'
					end as [CGAdult_Star_COMP5]

					/*CGAdult Comp6 Star Rating*/
				,	IC.AV_COMP_06 as CGAdult_COMP6
				,	PRANK.AV_COMP_06 as [CGAdult_%ile_COMP6]
				,	case
						when PRANK.AV_COMP_06 >= 90	then '5'
						when PRANK.AV_COMP_06 >= 70	then '4'
						when PRANK.AV_COMP_06 >= 50	then '3'
						when PRANK.AV_COMP_06 >= 25	then '2'
						else '1'
					end as [CGAdult_Star_COMP6]

	into			[CG_adult_FINAL]
	from			[CG_adult_FinalComp] FC
		inner join	[CG_adult_working3] IC on FC.CGPracticeId = IC.CGPracticeId

		inner join	(
						select			IC.CGPracticeId
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by FC.AV_COMP_OVERALL) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_COMP_OVERALL
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.AV_COMP_01) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_COMP_01
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.AV_COMP_02) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_COMP_02
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.AV_COMP_03) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_COMP_03
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.AV_COMP_04) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_COMP_04
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.AV_COMP_05) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_COMP_05
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.AV_COMP_06) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_COMP_06
						from			[CG_adult_FinalComp] FC
							inner join	[CG_adult_working3] IC on FC.CGPracticeId = IC.CGPracticeId
					) PRANK on PRank.CGPracticeId = IC.CGPracticeId
	order by		FC.CGPracticeId


	/*Calculate Average composite ratings across Peer Group*/
	select			avg(CGAdult_COMP_OVERALL) as Adult_Comp_Overall_Peer
				,	avg(CGAdult_COMP1) as Adult_Comp1_Peer
				,	avg(CGAdult_COMP2) as Adult_Comp2_Peer
				,	avg(CGAdult_COMP3) as Adult_Comp3_Peer
				,	avg(CGAdult_COMP4) as Adult_Comp4_Peer
				,	avg(CGAdult_COMP5) as Adult_Comp5_Peer
				,	avg(CGAdult_COMP6) as Adult_Comp6_Peer
	into			[CG_Adult_Peer]
	from			[CG_adult_FINAL]


  end