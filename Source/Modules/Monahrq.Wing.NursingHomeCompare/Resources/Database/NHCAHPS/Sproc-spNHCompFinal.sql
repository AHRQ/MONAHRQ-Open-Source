/********************************************************************************************************************************/
/********************************************************************************************************************************/
/*																																*/
/*	Script: Nursing Home CAHPS Composite																					    */
/*  Version: 1.0																												*/
/*	Last modified: 4/27/2016																									*/
/*	Authors: John Hoff <jhoff@healthdatadecisions.com> and			Patrick McGrath <pmcgrath@healthdatadecisions.com>					*/
/*  Change History:																												*/
/*		4/13/2016 - version 1.0 created																						    */
/*      4/27/2016 - Final version created																						*/
/********************************************************************************************************************************/
/*Builds NH measure level rates, composite rates, overall NH composite, stars and			percentiles for			peer group					*/

if (Object_id(N'spNHCompFinal')) is not null
	drop procedure spNHCompFinal
go

create procedure spNHCompFinal(@websiteId int)
as
begin
	declare @min_den int;

	set				@min_den = 20;

	select			[PROVIDERID]
					/*Q17*/
				,	case when sum(case when Q17 in ('1', '2') then 1 else 0 end) = 0 then null else (cast(sum(case when Q17 = '2' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q17 in ('1', '2') then 1 else 0 end) as decimal(10, 2))) end as Q17_Rate, count(Q17) as Q17_TOT, sum(case when Q17 in ('1', '2') then 1 else 0 end) as Q17_DENOM
					/*Q19*/
				,	 case when sum(case when Q19 in ('1', '2') then 1 else 0 end) = 0 then null else (cast(sum(case when Q19 = '2' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q19 in ('1', '2') then 1 else 0 end) as decimal(10, 2))) end as Q19_Rate, count(Q19) as Q19_TOT, sum(case when Q19 in ('1', '2') then 1 else 0 end) as Q19_DENOM
					/*Q21*/
				,	case when sum(case when Q21 in ('1', '2') then 1 else 0 end) = 0 then null else (cast(sum(case when Q21 = '2' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q21 in ('1', '2') then 1 else 0 end) as decimal(10, 2))) end as Q21_Rate, count(Q21) as Q21_TOT, sum(case when Q21 in ('1', '2') then 1 else 0 end) as Q21_DENOM
					/*Q12*/
				,	 case when sum(case when Q12 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q12 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q12 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q12_Rate, count(Q12) as Q12_TOT, sum(case when Q12 in ('1', '2', '3', '4') then 1 else 0 end) as Q12_DENOM
					/*Q13*/
				,	case when sum(case when Q13 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q13 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q13 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q13_Rate, count(Q13) as Q13_TOT, sum(case when Q13 in ('1', '2', '3', '4') then 1 else 0 end) as Q13_DENOM
					/*Q14*/
				,	case when sum(case when Q14 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q14 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q14 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q14_Rate, count(Q14) as Q14_TOT, sum(case when Q14 in ('1', '2', '3', '4') then 1 else 0 end) as Q14_DENOM
					/*Q24*/
				,	case when sum(case when Q24 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q24 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q24 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q24_Rate, count(Q24) as Q24_TOT, sum(case when Q24 in ('1', '2', '3', '4') then 1 else 0 end) as Q24_DENOM
					/*Q15*/
				,	case when sum(case when Q15 in ('1', '2') then 1 else 0 end) = 0 then null else (cast(sum(case when Q15 = '2' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q15 in ('1', '2') then 1 else 0 end) as decimal(10, 2))) end as Q15_Rate, count(Q15) as Q15_TOT, sum(case when Q15 in ('1', '2') then 1 else 0 end) as Q15_DENOM
					/*Q26*/
				,	case when sum(case when Q26 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q26 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q26 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q26_Rate, count(Q26) as Q26_TOT, sum(case when Q26 in ('1', '2', '3', '4') then 1 else 0 end) as Q26_DENOM
					/*Q27*/
				,	case when sum(case when Q27 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q27 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q27 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q27_Rate, count(Q27) as Q27_TOT, sum(case when Q27 in ('1', '2', '3', '4') then 1 else 0 end) as Q27_DENOM
					/*Q37*/
				,	case when sum(case when Q37 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q37 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q37 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q37_Rate, count(Q37) as Q37_TOT, sum(case when Q37 in ('1', '2', '3', '4') then 1 else 0 end) as Q37_DENOM
					/*Q42*/
				,	case when sum(case when Q42 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q42 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q42 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q42_Rate, count(Q42) as Q42_TOT, sum(case when Q42 in ('1', '2', '3', '4') then 1 else 0 end) as Q42_DENOM
					/*Q28*/
				,	case when sum(case when Q28 in ('1', '2') then 1 else 0 end) = 0 then null else (cast(sum(case when Q28 = '2' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q28 in ('1', '2') then 1 else 0 end) as decimal(10, 2))) end as Q28_Rate, count(Q28) as Q28_TOT, sum(case when Q28 in ('1', '2') then 1 else 0 end) as Q28_DENOM
					/*Q35*/
				,	case when sum(case when Q35 in ('1', '2') then 1 else 0 end) = 0 then null else (cast(sum(case when Q35 = '2' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q35 in ('1', '2') then 1 else 0 end) as decimal(10, 2))) end as Q35_Rate, count(Q35) as Q35_TOT, sum(case when Q35 in ('1', '2') then 1 else 0 end) as Q35_DENOM
					/*Q11*/
				,	case when sum(case when Q11 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q11 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q11 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q11_Rate, count(Q11) as Q11_TOT, sum(case when Q11 in ('1', '2', '3', '4') then 1 else 0 end) as Q11_DENOM
					/*Q40*/
				,	case when sum(case when Q40 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q40 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q40 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q40_Rate, count(Q40) as Q40_TOT, sum(case when Q40 in ('1', '2', '3', '4') then 1 else 0 end) as Q40_DENOM
					/*Q29*/
				,	case when sum(case when Q29 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q29 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q29 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q29_Rate, count(Q29) as Q29_TOT, sum(case when Q29 in ('1', '2', '3', '4') then 1 else 0 end) as Q29_DENOM
					/*Q22*/
				,	case when sum(case when Q22 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q22 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q22 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q22_Rate, count(Q22) as Q22_TOT, sum(case when Q22 in ('1', '2', '3', '4') then 1 else 0 end) as Q22_DENOM
					/*Q30*/
				,	case when sum(case when Q30 in ('1', '2', '3', '4') then 1 else 0 end) = 0 then null else (cast(sum(case when Q30 = '4' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q30 in ('1', '2', '3', '4') then 1 else 0 end) as decimal(10, 2))) end as Q30_Rate, count(Q30) as Q30_TOT, sum(case when Q30 in ('1', '2', '3', '4') then 1 else 0 end) as Q30_DENOM
					/*Q31*/
				,	case when sum(case when Q31 in ('1', '2', '3') then 1 else 0 end) = 0 then null else (cast(sum(case when Q31 = '1' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q31 in ('1', '2', '3') then 1 else 0 end) as decimal(10, 2))) end as Q31_Rate, count(Q31) as Q31_TOT, sum(case when Q31 in ('1', '2', '3') then 1 else 0 end) as Q31_DENOM
					/*Q33*/
				,	case when sum(case when Q33 in ('1', '2', '3') then 1 else 0 end) = 0 then null else (cast(sum(case when Q33 = '1' then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q33 in ('1', '2', '3') then 1 else 0 end) as decimal(10, 2))) end as Q33_Rate, count(Q33) as Q33_TOT, sum(case when Q33 in ('1', '2', '3') then 1 else 0 end) as Q33_DENOM
					/*Q38*/
				,	case when sum(case when Q38 in ('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '10') then 1 else 0 end) = 0 then null else (cast(sum(case when (Q38 = '9' or			Q38 = '10') then 1 else 0 end) as decimal(10, 2))) / (cast(sum(case when Q38 in ('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '10') then 1 else 0 end) as decimal(10, 2))) end as Q38_Rate, count(Q38) as Q38_TOT, sum(case when Q38 in ('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '10') then 1 else 0 end) as Q38_DENOM
	into			[NH CAHPS Sample_working]
	from			[Targets_NHCAHPSSurveyTargets] t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
--	where			PROVIDERID in (select distinct providerid from NH_Peer_Group)
	group by		[PROVIDERID]

	select			[PROVIDERID]
		/*COMP1*/
				,	 case when Q17_DENOM > @min_den then Q17_Rate else 0 end as Q17_Comp, case when Q19_DENOM > @min_den then Q19_Rate else 0 end as Q19_Comp, case when Q21_DENOM > @min_den then Q21_Rate else 0 end as Q21_Comp
		/*COMP2*/
				,	 case when Q12_DENOM > @min_den then Q12_Rate else 0 end as Q12_Comp, case when Q13_DENOM > @min_den then Q13_Rate else 0 end as Q13_Comp, case when Q14_DENOM > @min_den then Q14_Rate else 0 end as Q14_Comp, case when Q24_DENOM > @min_den then Q24_Rate else 0 end as Q24_Comp, case when Q15_DENOM > @min_den then Q15_Rate else 0 end as Q15_Comp
		/*COMP3*/
				,	 case when Q26_DENOM > @min_den then Q26_Rate else 0 end as Q26_Comp, case when Q27_DENOM > @min_den then Q27_Rate else 0 end as Q27_Comp, case when Q37_DENOM > @min_den then Q37_Rate else 0 end as Q37_Comp, case when Q42_DENOM > @min_den then Q42_Rate else 0 end as Q42_Comp, case when Q28_DENOM > @min_den then Q28_Rate else 0 end as Q28_Comp, case when Q35_DENOM > @min_den then Q35_Rate else 0 end as Q35_Comp
		/*COMP4*/
				,	 case when Q11_DENOM > @min_den then Q11_Rate else 0 end as Q11_Comp, case when Q40_DENOM > @min_den then Q40_Rate else 0 end as Q40_Comp, case when Q29_DENOM > @min_den then Q29_Rate else 0 end as Q29_Comp, case when Q22_DENOM > @min_den then Q22_Rate else 0 end as Q22_Comp, case when Q30_DENOM > @min_den then Q30_Rate else 0 end as Q30_Comp, case when Q31_DENOM > @min_den then Q31_Rate else 0 end as Q31_Comp, case when Q33_DENOM > @min_den then Q33_Rate else 0 end as Q33_Comp
		/*COMP5*/
				,	 case when Q38_DENOM > @min_den then Q38_Rate else 0 end as Q38_Comp
	into			[NH CAHPS Sample_working2]
	from			[NH CAHPS Sample_working]

	select			[PROVIDERID]
		/*COMP1*/
				,	 case when (sum(case when Q17_Comp <> 0 then 1 else 0 end) + sum(case when Q19_Comp <> 0 then 1 else 0 end) + sum(case when Q21_Comp <> 0 then 1 else 0 end)) = 0 then null else (Q17_Comp + Q19_Comp + Q21_Comp) / ((sum(case when Q17_Comp <> 0 then 1 else 0 end) + sum(case when Q19_Comp <> 0 then 1 else 0 end) + sum(case when Q21_Comp <> 0 then 1 else 0 end))) end as NH_COMP_01
		/*COMP2*/
				,	 case when (sum(case when Q12_Comp <> 0 then 1 else 0 end) + sum(case when Q13_Comp <> 0 then 1 else 0 end) + sum(case when Q14_Comp <> 0 then 1 else 0 end) + sum(case when Q24_Comp <> 0 then 1 else 0 end) + sum(case when Q15_Comp <> 0 then 1 else 0 end)) = 0 then null else (Q12_Comp + Q13_Comp + Q14_Comp + Q24_Comp + Q15_Comp) / ((sum(case when Q12_Comp <> 0 then 1 else 0 end) + sum(case when Q13_Comp <> 0 then 1 else 0 end) + sum(case when Q14_Comp <> 0 then 1 else 0 end) + sum(case when Q24_Comp <> 0 then 1 else 0 end) + sum(case when Q15_Comp <> 0 then 1 else 0 end))) end as NH_COMP_02
		/*COMP3*/
				,	 case when (sum(case when Q26_Comp <> 0 then 1 else 0 end) + sum(case when Q27_Comp <> 0 then 1 else 0 end) + sum(case when Q37_Comp <> 0 then 1 else 0 end) + sum(case when Q42_Comp <> 0 then 1 else 0 end) + sum(case when Q28_Comp <> 0 then 1 else 0 end) + sum(case when Q35_Comp <> 0 then 1 else 0 end)) = 0 then null else (Q26_Comp + Q27_Comp + Q37_Comp + Q42_Comp + Q28_Comp + Q35_Comp) / ((sum(case when Q26_Comp <> 0 then 1 else 0 end) + sum(case when Q27_Comp <> 0 then 1 else 0 end) + sum(case when Q37_Comp <> 0 then 1 else 0 end) + sum(case when Q42_Comp <> 0 then 1 else 0 end) + sum(case when Q28_Comp <> 0 then 1 else 0 end) + sum(case when Q35_Comp <> 0 then 1 else 0 end))) end as NH_COMP_03
		/*COMP4*/
				,	 case when (sum(case when Q11_Comp <> 0 then 1 else 0 end) + sum(case when Q40_Comp <> 0 then 1 else 0 end) + sum(case when Q29_Comp <> 0 then 1 else 0 end) + sum(case when Q22_Comp <> 0 then 1 else 0 end) + sum(case when Q30_Comp <> 0 then 1 else 0 end) + sum(case when Q31_Comp <> 0 then 1 else 0 end) + sum(case when Q33_Comp <> 0 then 1 else 0 end)) = 0 then null else (Q11_Comp + Q40_Comp + Q29_Comp + Q22_Comp + Q30_Comp + Q31_Comp + Q33_Comp) / ((sum(case when Q11_Comp <> 0 then 1 else 0 end) + sum(case when Q40_Comp <> 0 then 1 else 0 end) + sum(case when Q29_Comp <> 0 then 1 else 0 end) + sum(case when Q22_Comp <> 0 then 1 else 0 end) + sum(case when Q30_Comp <> 0 then 1 else 0 end) + sum(case when Q31_Comp <> 0 then 1 else 0 end) + sum(case when Q33_Comp <> 0 then 1 else 0 end))) end as NH_COMP_04
		/*COMP5*/
				,	 Q38_Comp as NH_COMP_05
	into			[NH CAHPS Sample_working3]
	from			[NH CAHPS Sample_working2]
	group by		[PROVIDERID], Q17_Comp, Q19_Comp, Q21_Comp, Q12_Comp, Q13_Comp, Q14_Comp, Q24_Comp, Q15_Comp, Q26_Comp, Q27_Comp, Q37_Comp, Q42_Comp, Q28_Comp, Q35_Comp, Q11_Comp, Q40_Comp, Q29_Comp, Q22_Comp, Q30_Comp, Q31_Comp, Q33_Comp, Q38_Comp

	/*FINAL NH COMPOSITE*/
	select			[PROVIDERID], case when (sum(case when NH_COMP_01 <> 0 then 1 else 0 end) + sum(case when NH_COMP_02 <> 0 then 1 else 0 end) + sum(case when NH_COMP_03 <> 0 then 1 else 0 end) + sum(case when NH_COMP_04 <> 0 then 1 else 0 end) + sum(case when NH_COMP_05 <> 0 then 1 else 0 end)) = 0 then null else (NH_COMP_01 + NH_COMP_02 + NH_COMP_03 + NH_COMP_04 + NH_COMP_05) / ((sum(case when NH_COMP_01 <> 0 then 1 else 0 end) + sum(case when NH_COMP_02 <> 0 then 1 else 0 end) + sum(case when NH_COMP_03 <> 0 then 1 else 0 end) + sum(case when NH_COMP_04 <> 0 then 1 else 0 end) + sum(case when NH_COMP_05 <> 0 then 1 else 0 end))) end as NH_COMP_OVERALL
	into			[NH CAHPS Sample_FinalComp]
	from			[NH CAHPS Sample_working3]
	group by		[PROVIDERID], NH_COMP_01, NH_COMP_02, NH_COMP_03, NH_COMP_04, NH_COMP_05

	/*Final Composite Star Ratings and			Individual Composite Star Ratings*/
	select			FC.PROVIDERID

					/*Final Overall NH Comp Star Rating*/
				,	FC.NH_COMP_OVERALL as NH_COMP_OVERALL
				,	PRANK.NH_COMP_OVERALL as [NH_%ile_Overall]				
				,	case
						when PRANK.NH_COMP_OVERALL < 10	then '1'
						when PRANK.NH_COMP_OVERALL between 10 and (10 + (80 / 3)) then '2'
						when PRANK.NH_COMP_OVERALL between (89 - (80 / 3)) and 89 then '4'
						when PRANK.NH_COMP_OVERALL >= 90 then '5'
						else '3'
					end as [NH_Star_Overall]

					 /*NH Comp1 Star Rating*/
				,	IC.NH_COMP_01 as NH_COMP1
				,	PRANK.NH_COMP_01 as [NH_%ile_COMP1]				
				,	case
						when PRANK.NH_COMP_01 < 10	then '1'
						when PRANK.NH_COMP_01 between 10 and (10 + (80 / 3)) then '2'
						when PRANK.NH_COMP_01 between (89 - (80 / 3)) and 89 then '4'
						when PRANK.NH_COMP_01 >= 90 then '5'
						else '3'
					end as [NH_Star_COMP1]

					/*NH Comp2 Star Rating*/
				,	IC.NH_COMP_02 as NH_COMP2
				,	PRANK.NH_COMP_02 as [NH_%ile_COMP2]				
				,	case
						when PRANK.NH_COMP_02 < 10	then '1'
						when PRANK.NH_COMP_02 between 10 and (10 + (80 / 3)) then '2'
						when PRANK.NH_COMP_02 between (89 - (80 / 3)) and 89 then '4'
						when PRANK.NH_COMP_02 >= 90 then '5'
						else '3'
					end as [NH_Star_COMP2]

					/*NH Comp3 Star Rating*/
				,	IC.NH_COMP_03 as NH_COMP3
				,	PRANK.NH_COMP_03 as [NH_%ile_COMP3]				
				,	case
						when PRANK.NH_COMP_03 < 10	then '1'
						when PRANK.NH_COMP_03 between 10 and (10 + (80 / 3)) then '2'
						when PRANK.NH_COMP_03 between (89 - (80 / 3)) and 89 then '4'
						when PRANK.NH_COMP_03 >= 90 then '5'
						else '3'
					end as [NH_Star_COMP3]

					/*NH Comp4 Star Rating*/					
				,	IC.NH_COMP_04 as NH_COMP4
				,	PRANK.NH_COMP_04 as [NH_%ile_COMP4]				
				,	case
						when PRANK.NH_COMP_04 < 10	then '1'
						when PRANK.NH_COMP_04 between 10 and (10 + (80 / 3)) then '2'
						when PRANK.NH_COMP_04 between (89 - (80 / 3)) and 89 then '4'
						when PRANK.NH_COMP_04 >= 90 then '5'
						else '3'
					end as [NH_Star_COMP4]

					/*NH Comp5 Star Rating*/
				,	IC.NH_COMP_05 as NH_COMP5
				,	PRANK.NH_COMP_05 as [NH_%ile_COMP5]				
				,	case
						when PRANK.NH_COMP_05 < 10	then '1'
						when PRANK.NH_COMP_05 between 10 and (10 + (80 / 3)) then '2'
						when PRANK.NH_COMP_05 between (89 - (80 / 3)) and 89 then '4'
						when PRANK.NH_COMP_05 >= 90 then '5'
						else '3'
					end as [NH_Star_COMP5]

	into			[NH CAHPS Sample_Final]
	from			[NH CAHPS Sample_FinalComp] FC
		inner join	[NH CAHPS Sample_working3] IC on FC.providerid = IC.providerid
		inner join	(
						select			IC.providerid
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by FC.NH_COMP_OVERALL) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as NH_COMP_OVERALL
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.NH_COMP_01) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as NH_COMP_01
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.NH_COMP_02) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as NH_COMP_02
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.NH_COMP_03) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as NH_COMP_03
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.NH_COMP_04) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as NH_COMP_04
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by IC.NH_COMP_05) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as NH_COMP_05
						from			[NH CAHPS Sample_FinalComp] FC
							inner join	[NH CAHPS Sample_working3] IC on FC.providerid = IC.providerid
					) PRANK on PRank.providerid = IC.providerid
	order by FC.PROVIDERID

	/*Calculate Average composite ratings across Peer Group*/
	select			avg(NH_COMP_OVERALL) as NH_Comp_Overall_Peer, avg(NH_COMP1) as NH_Comp1_Peer, avg(NH_COMP2) as NH_Comp2_Peer, avg(NH_COMP3) as NH_Comp3_Peer, avg(NH_COMP4) as NH_Comp4_Peer, avg(NH_COMP5) as NH_Comp5_Peer
	into			[NH_Peer]
	from			[NH CAHPS Sample_Final]
end