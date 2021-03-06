/***********************************************************************************************************************************************************/
/***********************************************************************************************************************************************************/
/*																																						   */
/*	Script: CG Child Comp Final																															   */
/*  Version: 1.0																																		   */
/*	Last modified: 4/27/2016																															   */
/*	Authors: John Hoff <jhoff@healthdatadecisions.com> and Patrick McGrath <pmcgrath@healthdatadecisions.com>											   */
/*  Change History:																																		   */
/*		4/13/2016 - version 1.0 created																													   */
/*      4/27/2016 - Final version created																												   */
/***********************************************************************************************************************************************************/
/*Builds child measure level rates, composite rates, overall adult composite, overall adult-child composite, stars and percentiles for peer group		   */
if (Object_id(N'spCGChildCompFinal')) is not null
	drop procedure spCGChildCompFinal
go

create procedure spCGChildCompFinal
	@WebsiteId as int
as
begin
	declare @min_den int;

	set				@min_den = 1;

	select			[CGPracticeId]
				,	max(ChildPracticeSampleSize) as Child_Samp
					/*CD_38*/
				,	case
						when sum(case when CD_38 in ('1', '2') then 1 else 0 end) = 0 then null
					else 
						(cast(sum(case when CD_38 = '1' then 1 else 0 end) as decimal(10, 2))) /
						(cast(sum(case when CD_38 in ('1','2') then 1 else 0 end) as decimal(10, 2))
					)
		 end as CD_38_Rate
				,	count(CD_38) as CD_38_TOT
				,	sum(case
						when CD_38 in ('1', '2') then 1
						else 0 end) as CD_38_DENOM
					/*CD_39*/
				,	case
						when sum(case when CD_39 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case when CD_39 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_39 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_39_Rate
				,	count(CD_39) as CD_39_TOT
				,	sum(case
						when CD_39 in ('1', '2') then 1 else 0 end) as CD_39_DENOM
					/*CD_40*/
				,	case
						when sum(case
						when CD_40 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_40 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_40 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_40_Rate
				,	count(CD_40) as CD_40_TOT
				,	sum(case
						when CD_40 in ('1', '2') then 1 else 0 end) as CD_40_DENOM
					/*CD_41*/
				,	case
						when sum(case
						when CD_41 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_41 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_41 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_41_Rate
				,	count(CD_41) as CD_41_TOT
				,	sum(case
						when CD_41 in ('1', '2') then 1 else 0 end) as CD_41_DENOM
					/*CD_42*/
				,	case
						when sum(case
						when CD_42 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_42 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_42 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_42_Rate
				,	count(CD_42) as CD_42_TOT
				,	sum(case
						when CD_42 in ('1', '2') then 1 else 0 end) as CD_42_DENOM
					/*CD_43*/
				,	case
						when sum(case
						when CD_43 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_43 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_43 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_43_Rate
				,	count(CD_43) as CD_43_TOT
				,	sum(case
						when CD_43 in ('1', '2') then 1 else 0 end) as CD_43_DENOM
					/*CD_44*/
				,	case
						when sum(case
						when CD_44 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_44 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_44 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_44_Rate
				,	count(CD_44) as CD_44_TOT
				,	sum(case
						when CD_44 in ('1', '2') then 1 else 0 end) as CD_44_DENOM
					/*CD_45*/
				,	case
						when sum(case
						when CD_45 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_45 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_45 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_45_Rate
				,	count(CD_45) as CD_45_TOT
				,	sum(case
						when CD_45 in ('1', '2') then 1 else 0 end) as CD_45_DENOM
					/*CD_46*/
				,	case
						when sum(case
						when CD_46 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_46 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_46 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_46_Rate
				,	count(CD_46) as CD_46_TOT
				,	sum(case
						when CD_46 in ('1', '2') then 1 else 0 end) as CD_46_DENOM
					/*CD_47*/
				,	case
						when sum(case
						when CD_47 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_47 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_47 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_47_Rate
				,	count(CD_47) as CD_47_TOT
				,	sum(case
						when CD_47 in ('1', '2') then 1 else 0 end) as CD_47_DENOM
					/*CD_48*/
				,	case
						when sum(case
						when CD_48 in ('1', '2') then 1 else 0 end) = 0 then null
						else (cast(sum(case
						when CD_48 = '1' then 1 else 0 end) as decimal(10, 2))	) / (cast(sum(case
						when CD_48 in ('1', '2') then 1 else 0 end) as decimal(10, 2))	) end as CD_48_Rate
				,	count(CD_48) as CD_48_TOT
				,	sum(case
						when CD_48 in ('1', '2') then 1 else 0 end) as CD_48_DENOM
	into			[CG_child_working]
	from			[Targets_CGCAHPSSurveyTargets]	--[CG_child]
	where			Dataset_Id in (select wd.Dataset_Id from Websites_WebsiteDatasets wd where wd.Website_Id = @WebsiteId)
	group by		[CGPracticeId]

	select			[CGPracticeId]
				,	Child_Samp
					/*COMP1*/ 
				,	case  when CD_38_DENOM > @min_den then CD_38_Rate else 0  end as CD_38_Comp 
				,	case  when CD_39_DENOM > @min_den then CD_39_Rate else 0  end as CD_39_Comp 
				,	case  when CD_40_DENOM > @min_den then CD_40_Rate else 0  end as CD_40_Comp 
				,	case  when CD_41_DENOM > @min_den then CD_41_Rate else 0  end as CD_41_Comp 
				,	case  when CD_44_DENOM > @min_den then CD_44_Rate else 0  end as CD_44_Comp 
				,	case  when CD_47_DENOM > @min_den then CD_47_Rate else 0  end as CD_47_Comp
					/*COMP2*/ 
				,	case  when CD_42_DENOM > @min_den then CD_42_Rate else 0  end as CD_42_Comp 
				,	case  when CD_43_DENOM > @min_den then CD_43_Rate else 0  end as CD_43_Comp 
				,	case  when CD_45_DENOM > @min_den then CD_45_Rate else 0  end as CD_45_Comp 
				,	case  when CD_46_DENOM > @min_den then CD_46_Rate else 0  end as CD_46_Comp 
				,	case  when CD_48_DENOM > @min_den then CD_48_Rate else 0  end as CD_48_Comp
	into			[CG_child_working2]
	from			[CG_child_working]

	select			[CGPracticeId]
				,	Child_Samp
		/*COMP1*/
				,	case 
						when (
							sum(case when CD_38_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_39_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_40_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_41_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_44_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_47_Comp <> 0 then 1 else 0 end)) = 0
							then null
						else (CD_38_Comp + CD_39_Comp + CD_40_Comp + CD_41_Comp + CD_44_Comp + CD_47_Comp) / (
							(
								sum(case when CD_38_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_39_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_40_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_41_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_44_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_47_Comp <> 0 then 1 else 0 end))) end as CD_COMP_01
		/*COMP2*/
				,	case 
						when (
							sum(case when CD_42_Comp <> 0 then 1 else 0 end) + 
							sum(case when CD_43_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_45_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_46_Comp <> 0 then 1 else 0 end) +
							sum(case when CD_48_Comp <> 0 then 1 else 0 end)) = 0
							then null
						else (CD_42_Comp + CD_43_Comp + CD_45_Comp + CD_46_Comp + CD_48_Comp) / (
							(	sum(case when CD_42_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_43_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_45_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_46_Comp <> 0 then 1 else 0 end) +
								sum(case when CD_48_Comp <> 0 then 1 else 0 end))
							) end as CD_COMP_02
	into			[CG_child_working3]
	from			[CG_child_working2]
	group by		[CGPracticeId]
				,	Child_Samp
				,	CD_38_Comp
				,	CD_39_Comp
				,	CD_40_Comp
				,	CD_41_Comp
				,	CD_44_Comp
				,	CD_47_Comp
				,	CD_42_Comp
				,	CD_43_Comp
				,	CD_45_Comp
				,	CD_46_Comp
				,	CD_48_Comp

	/*FINAL CHILD COMPOSITE*/
	select			[CGPracticeId]
				,	Child_Samp
				,	case 
						when (
							sum(case when CD_COMP_01 <> 0 then 1 else 0 end) +
							sum(case when CD_COMP_02 <> 0 then 1 else 0 end)) = 0
							then null
						else 
							(CD_COMP_01 + CD_COMP_02) / (
							(	sum(case when CD_COMP_01 <> 0 then 1 else 0 end) +
								sum(case when CD_COMP_02 <> 0 then 1 else 0 end)))
		 end as CD_COMP_OVERALL
	into			[CG_child_FinalComp]
	from			[CG_child_working3]
	group by		[CGPracticeId]
				,	Child_Samp
				,	CD_COMP_01
				,	CD_COMP_02

	/*Final Composite Star Ratings and Individual Composite Star Ratings*/
	select			FC.[CGPracticeId]
				,	FC.Child_Samp
		
					/*Final Overall NH Comp Star Rating*/
				,	FC.CD_COMP_OVERALL as CGChild_COMP_OVERALL
				,	PRANK.CD_COMP_OVERALL as [CGChild_%ile_Overall]
				,	case
						when PRANK.CD_COMP_OVERALL >= 90	then '5'
						when PRANK.CD_COMP_OVERALL >= 70	then '4'
						when PRANK.CD_COMP_OVERALL >= 50	then '3'
						when PRANK.CD_COMP_OVERALL >= 25	then '2'
						else '1'
					end as [CGChild_Star_Overall]					
					/*CGChild Comp1 Star Rating*/
				,	IC.CD_COMP_01 as CGChild_COMP1
				,	PRANK.CD_COMP_01 as [CGChild_%ile_COMP1]
				,	case
						when PRANK.CD_COMP_01 >= 90	then '5'
						when PRANK.CD_COMP_01 >= 70	then '4'
						when PRANK.CD_COMP_01 >= 50	then '3'
						when PRANK.CD_COMP_01 >= 25	then '2'
						else '1'
					end as [CGChild_Star_COMP1]
					
					/*CGChild Comp2 Star Rating*/
				,	IC.CD_COMP_02 as CGChild_COMP2
				,	PRANK.CD_COMP_02 as [CGChild_%ile_COMP2]
				,	case
						when PRANK.CD_COMP_02 >= 90	then '5'
						when PRANK.CD_COMP_02 >= 70	then '4'
						when PRANK.CD_COMP_02 >= 50	then '3'
						when PRANK.CD_COMP_02 >= 25	then '2'
						else '1'
					end as [CGChild_Star_COMP2]

	into			[CG_child_Final]
	from			[CG_child_FinalComp] FC
		inner join	[CG_child_working3] IC on FC.[CGPracticeId] = IC.[CGPracticeId]
		inner join	(
				select			IC.CGPracticeId
							,	case when count(*) over (partition by 1) = 1
									then	0
									else	(cast(rank()   over (partition by 1 order by FC.CD_COMP_OVERALL) as decimal) - 1.0) /
											(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
								end as CD_COMP_OVERALL
							,	case when count(*) over (partition by 1) = 1
									then	0
									else	(cast(rank()   over (partition by 1 order by IC.CD_COMP_01) as decimal) - 1.0) /
											(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
								end as CD_COMP_01
							,	case when count(*) over (partition by 1) = 1
									then	0
									else	(cast(rank()   over (partition by 1 order by IC.CD_COMP_02) as decimal) - 1.0) /
											(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
								end as CD_COMP_02
				from			[CG_child_FinalComp] FC
					inner join	[CG_child_working3] IC on FC.[CGPracticeId] = IC.[CGPracticeId]
			) PRANK on PRank.CGPracticeId = IC.CGPracticeId


	order by		FC.[CGPracticeId]

	/*OVERALL ADULT-CHILD COMPOSITE*/
	select			a.CGPracticeId
				,	a.adult_samp
				,	a.AV_COMP_OVERALL
				,	c.Child_Samp as child_samp
				,	c.CD_COMP_OVERALL
				,	((cast(a.adult_samp as int) * a.AV_COMP_OVERALL) + (cast(c.Child_Samp as int) * c.CD_COMP_OVERALL)) / (cast(a.adult_samp as int) + cast(c.Child_Samp as int)) as AV_CD_COMP_OVERALL
	into			[CG_AdultChild_FinalComp]
	from			[CG_adult_FinalComp] a
		inner join	[CG_child_FinalComp] c on a.CGPracticeId = c.CGPracticeId

	/*Final Overall Adult-Child Composite Stars*/
	select			FC.[CGPracticeId]				 
				,	FC.AV_CD_COMP_OVERALL as AdultChild_COMP_OVERALL
				,	PRANK.AV_CD_COMP_OVERALL as [AdultChild_%ile_Overall]
				,	case
						when PRANK.AV_CD_COMP_OVERALL >= 90	then '5'
						when PRANK.AV_CD_COMP_OVERALL >= 70	then '4'
						when PRANK.AV_CD_COMP_OVERALL >= 50	then '3'
						when PRANK.AV_CD_COMP_OVERALL >= 25	then '2'
						else '1'
					end as [AdultChild_Star_Overall]

	into			[CG_AdultChild_COMP_FINAL]
	from			[CG_AdultChild_FinalComp] FC
		inner join	(
						select			FC.CGPracticeId
									,	case when count(*) over (partition by 1) = 1
											then	0
											else	(cast(rank()   over (partition by 1 order by FC.AV_CD_COMP_OVERALL) as decimal) - 1.0) /
													(cast(count(*) over (partition by 1) as decimal) - 1.0) * 100.0
										end as AV_CD_COMP_OVERALL
						from			[CG_AdultChild_FinalComp] FC
					) PRANK on PRank.CGPracticeId = FC.CGPracticeId
	order by		FC.[CGPracticeId]

	/*Calculate Average composite ratings across Peer Group*/
	select			avg(CGChild_COMP_OVERALL) as Child_Comp_Overall_Peer
				,	avg(CGChild_COMP1) as Child_Comp1_Peer
				,	avg(CGChild_COMP2) as Child_Comp2_Peer
	into			[CG_Child_Peer]
	from			[CG_child_Final]

	/*Calculate Average Final Adult-Child composite ratings across Peer Group*/
	select			avg(AdultChild_COMP_OVERALL) as AdultChild_Comp_Overall_Peer
	into			[CG_AdultChild_Peer]
	from			[CG_AdultChild_COMP_FINAL]
end
