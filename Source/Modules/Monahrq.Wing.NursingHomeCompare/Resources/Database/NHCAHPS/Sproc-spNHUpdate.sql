/********************************************************************************************************************************/
/********************************************************************************************************************************/
/*																																*/
/*	Script: NH Update																										    */
/*  Version: 1.0																												*/
/*	Last modified: 4/28/2016																									*/
/*	Authors: Patrick McGrath <pmcgrath@healthdatadecisions.com> and John Hoff <jhoff@healthdatadecisions.com> 					*/
/*  Change History:																												*/
/*		4/28/2016 - version 1.0 created																						    */
/********************************************************************************************************************************/
/********************************************************************************************************************************/
/* 1. Buld a provider base and measure table for final ouput															        */
/* 2. Populate an all rates table to contain all needed rates from NH working tables										    */
/* 3. Update provider base and measure table established in step 1 with the rates from			step 2								    */
/* 4. Pull in response % breakdown by question by provider from the NH dataset				and			add to final output	                        */
/********************************************************************************************************************************/

/*1. Build a provider base and measure table for final ouput																	*/
/*Combine list of Provider ID's across for			NH*/
/*drop table [NH_Base]
drop table [NH_Provider_Base]
drop table [NH_Rates_All]
drop table nh_responsedetail*/


if (Object_id(N'spNHUpdate')) is not null
	drop procedure spNHUpdate
go

create procedure spNHUpdate (@WebsiteId int)
as
begin

	select			distinct providerid as Provider_id
	into			[NH_Base]
	from			[Targets_NHCAHPSSurveyTargets] t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId

	/*Summarize into			Unique list of ID to join to NH Measure Lookup Table*/
	select			a.provider_id, b.[MeasureID], b.[MeasureType], b.[CAHPSQuestionType]
	into			[NH_Provider_Base]
	from			(
						select			provider_id, 'X' as join_key
						from			[NH_Base]
						group by		provider_id
					) a
		full join	(
						select			[MeasureID], [MeasureType], [CAHPSQuestionType], 'X' as Join_key
						from			[Base_NHCAHPSMeasureLookups]	--[NH_Measure_Lookup]
					) b on a.join_key = b.join_key
	--where			a.provider_id in (select distinct providerid from NH_Peer_Group)
	order by		a.provider_id, b.[MeasureID]

	alter table [NH_Provider_Base] add rating decimal(10, 2), peer_rating varchar(1), peer_rate decimal(10, 2), peer_percentile decimal(10, 2);

	/******************************************************************************************************************************************************************************************************/
	/* 2. Populate an all rates table to contain all needed rates from			the NHworking tables																										          */
	select			[PROVIDERID], 'Q17_Rate' as [Measure_Id], [Q17_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	into			[NH_Rates_All]
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q19_Rate' as [Measure_Id], [Q19_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q21_Rate' as [Measure_Id], [Q21_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q12_Rate' as [Measure_Id], [Q12_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q13_Rate' as [Measure_Id], [Q13_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q14_Rate' as [Measure_Id], [Q14_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q24_Rate' as [Measure_Id], [Q24_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q15_Rate' as [Measure_Id], [Q15_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q26_Rate' as [Measure_Id], [Q26_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q27_Rate' as [Measure_Id], [Q27_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q37_Rate' as [Measure_Id], [Q37_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q42_Rate' as [Measure_Id], [Q42_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q28_Rate' as [Measure_Id], [Q28_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q35_Rate' as [Measure_Id], [Q35_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q11_Rate' as [Measure_Id], [Q11_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q40_Rate' as [Measure_Id], [Q40_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q29_Rate' as [Measure_Id], [Q29_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q22_Rate' as [Measure_Id], [Q22_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q30_Rate' as [Measure_Id], [Q30_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q31_Rate' as [Measure_Id], [Q31_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q33_Rate' as [Measure_Id], [Q33_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'Q38_Rate' as [Measure_Id], [Q38_Rate] as rating, '' as peer_rating, 0.000 as peer_rate, 0.000 as peer_percentile
	from			[NH CAHPS Sample_working]

	union all

	select			[PROVIDERID], 'NH_COMP_01' as [Measure_Id], [NH_COMP1] as rating, [NH_Star_COMP1] as peer_rating, 0.000 as peer_rate, [NH_%ile_COMP1] as peer_percentile
	from			[NH CAHPS Sample_Final]

	union all

	select			[PROVIDERID], 'NH_COMP_02' as [Measure_Id], [NH_COMP2] as rating, [NH_Star_COMP2] as peer_rating, 0.000 as peer_rate, [NH_%ile_COMP2] as peer_percentile
	from			[NH CAHPS Sample_Final]

	union all

	select			[PROVIDERID], 'NH_COMP_03' as [Measure_Id], [NH_COMP3] as rating, [NH_Star_COMP3] as peer_rating, 0.000 as peer_rate, [NH_%ile_COMP3] as peer_percentile
	from			[NH CAHPS Sample_Final]

	union all

	select			[PROVIDERID], 'NH_COMP_04' as [Measure_Id], [NH_COMP4] as rating, [NH_Star_COMP4] as peer_rating, 0.000 as peer_rate, [NH_%ile_COMP4] as peer_percentile
	from			[NH CAHPS Sample_Final]

	union all

	select			[PROVIDERID], 'NH_COMP_05' as [Measure_Id], [NH_COMP5] as rating, [NH_Star_COMP5] as peer_rating, 0.000 as peer_rate, [NH_%ile_COMP5] as peer_percentile
	from			[NH CAHPS Sample_Final]

	union all

	select			[PROVIDERID], 'NH_COMP_OVERALL' as [Measure_Id], [NH_COMP_OVERALL] as rating, [NH_Star_Overall] as peer_rating, 0.000 as peer_rate, [NH_%ile_Overall] as peer_percentile
	from			[NH CAHPS Sample_Final]

	/*Update Statement for			Peer Percentile Field*/
	update			[NH_Rates_All]
	set				peer_rate = (
			select			isnull(nh_comp_overall_peer,0.000)
			from			NH_Peer
			)
	from			[NH_Rates_All]
	where			measure_id = 'NH_COMP_OVERALL'

	update			[NH_Rates_All]
	set				peer_rate = (
			select			isnull(nh_comp1_peer,0.000)
			from			NH_Peer
			)
	from			[NH_Rates_All]
	where			measure_id = 'NH_COMP_01'

	update			[NH_Rates_All]
	set				peer_rate = (
			select			isnull(nh_comp2_peer,0.000)
			from			NH_Peer
			)
	from			[NH_Rates_All]
	where			measure_id = 'NH_COMP_02'

	update			[NH_Rates_All]
	set				peer_rate = (
			select			isnull(nh_comp3_peer,0.000)
			from			NH_Peer
			)
	from			[NH_Rates_All]
	where			measure_id = 'NH_COMP_03'

	update			[NH_Rates_All]
	set				peer_rate = (
			select			isnull(nh_comp4_peer,0.000)
			from			NH_Peer
			)
	from			[NH_Rates_All]
	where			measure_id = 'NH_COMP_04'

	update			[NH_Rates_All]
	set				peer_rate = (
			select			isnull(nh_comp5_peer,0.000)
			from			NH_Peer
			)
	from			[NH_Rates_All]
	where			measure_id = 'NH_COMP_05'

	/*****************************************************************************************************************************************************************************************************************************/
	/* 3. Update provider base and			measure table established in step 1 with the rates from			step 2																																 */
	/*Update Statement for			Rating Field*/
	update			[NH_Provider_Base]
	set				rating = b.rating
	from			[NH_Provider_Base] a
		left join	[NH_Rates_All] b on a.provider_id = b.providerid and			a.MeasureId = b.measure_id

	/*Update Statement for			Peer Rating Field*/
	update			[NH_Provider_Base]
	set				peer_rating = b.peer_rating
	from			[NH_Provider_Base] a
		left join	[NH_Rates_All] b on a.provider_id = b.providerid and			a.MeasureId = b.measure_id

	/*Update Statement for			Peer Percentile Field*/
	update			[NH_Provider_Base]
	set				peer_percentile = b.peer_percentile
	from			[NH_Provider_Base] a
		left join	[NH_Rates_All] b on a.provider_id = b.providerid and			a.MeasureId = b.measure_id

	/*Update Statement for			Peer Rate*/
	update			[NH_Provider_Base]
	set				peer_rate = b.peer_rate
	from			[NH_Provider_Base] a
		left join	[NH_Rates_All] b on a.provider_id = b.providerid and			a.MeasureId = b.measure_id

	/*****************************************************************************************************************************************************************************************************************************/
	/* 4. Pull in response % breakdown by question by provider from			the NH dataset				and			add to final output																														 */
	select			[PROVIDERID], 'Q17_Rate' as measure_id, cast(sum(case when Q17 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q17 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q17 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q17 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q17 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q17 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	into			nh_responsedetail
	from			[Targets_NHCAHPSSurveyTargets]t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q19_Rate' as measure_id, cast(sum(case when Q19 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q19 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q19 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q19 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q19 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q19 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q21_Rate' as measure_id, cast(sum(case when Q21 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q21 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q21 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q21 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q21 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q21 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q12_Rate' as measure_id, cast(sum(case when Q12 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q12 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q12 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q12 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q12 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q12 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q12 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q12 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q12 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q12 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q12 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q12 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q12 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q13_Rate' as measure_id, cast(sum(case when Q13 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q13 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q13 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q13 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q13 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q13 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q13 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q13 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q13 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q13 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q13 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q13 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q13 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q14_Rate' as measure_id, cast(sum(case when Q14 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q14 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q14 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q14 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q14 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q14 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q14 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q14 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q14 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q14 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q14 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q14 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q14 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q24_Rate' as measure_id, cast(sum(case when Q24 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q24 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q24 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q24 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q24 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q24 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q24 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q24 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q24 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q24 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q24 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q24 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q24 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q15_Rate' as measure_id, cast(sum(case when Q15 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q15 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q15 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q15 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q15 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q15 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	--[NH CAHPS Sample]
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q26_Rate' as measure_id, cast(sum(case when Q26 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q26 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q26 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q26 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q26 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q26 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q26 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q26 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q26 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q26 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q26 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q26 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q26 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q27_Rate' as measure_id, cast(sum(case when Q27 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q27 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q27 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q27 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q27 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q27 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q27 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q27 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q27 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q27 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q27 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q27 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q27 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q37_Rate' as measure_id, cast(sum(case when Q37 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q37 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q37 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q37 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q37 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q37 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q37 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q37 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q37 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q37 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q37 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q37 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q37 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q42_Rate' as measure_id, cast(sum(case when Q42 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q42 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q42 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q42 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q42 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q42 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q42 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q42 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q42 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q42 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q42 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q42 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q42 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q28_Rate' as measure_id, cast(sum(case when Q28 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q28 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q28 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q28 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q28 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q28 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q35_Rate' as measure_id, cast(sum(case when Q35 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q35 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q35 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q35 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q35 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q35 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q11_Rate' as measure_id, cast(sum(case when Q11 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q11 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q11 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q11 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q11 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q11 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q11 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q11 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q11 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q11 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q11 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q11 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q11 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q40_Rate' as measure_id, cast(sum(case when Q40 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q40 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q40 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q40 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q40 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q40 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q40 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q40 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q40 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q40 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q40 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q40 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q40 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q29_Rate' as measure_id, cast(sum(case when Q29 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q29 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q29 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q29 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q29 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q29 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q29 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q29 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q29 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q29 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q29 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q29 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q29 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q22_Rate' as measure_id, cast(sum(case when Q22 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q22 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q22 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q22 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q22 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q22 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q22 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q22 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q22 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q22 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q22 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q22 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q22 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q30_Rate' as measure_id, cast(sum(case when Q30 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q30 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q30 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q30 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q30 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q30 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q30 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q30 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q30 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q30 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q30 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q30 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q30 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q31_Rate' as measure_id, cast(sum(case when Q31 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q31 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q31 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q31 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q31 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q31 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q31 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q31 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q31 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q31 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q31 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q31 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q31 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q33_Rate' as measure_id, cast(sum(case when Q33 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q33 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q33 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q33 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q33 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q33 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q33 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q33 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q33 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q33 = 99 
						then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q33 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q33 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q33 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	union

	select			[PROVIDERID], 'Q38_Rate' as measure_id, cast(sum(case when Q38 = 0 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response1, cast(sum(case when Q38 = 1 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response2, cast(sum(case when Q38 = 2 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response3, cast(sum(case when Q38 = 3 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response4, cast(sum(case when Q38 = 4 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response5, cast(sum(case when Q38 = 5 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response6, cast(sum(case when Q38 = 6 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response7, cast(sum(case when Q38 = 7 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response8, cast(sum(case when Q38 = 8 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response9, cast(sum(case when Q38 = 9 then 1 
					else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response10, cast(sum(case when Q38 = 10 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response11, cast(sum(case when Q38 = 98 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response12, cast(sum(case when Q38 = 99 then 1 else 0 end) as decimal(8, 2)) / cast(count([PROVIDERID]) as decimal(8, 2)) as response13
	from			[Targets_NHCAHPSSurveyTargets]	t	--[NH CAHPS Sample]
		inner join [Websites_WebsiteDatasets] w on w.DataSet_Id = t.Dataset_Id
	where w.Website_id = @websiteId
	group by		[PROVIDERID]

	/*Take NH response detail and			join to final output table*/
	select			s.*
				,	n.Id as NursingHomeId
				,	d.response1
				,	d.response2
				,	d.response3
				,	d.response4
				,	d.response5
				,	d.response6
				,	d.response7
				,	d.response8
				,	d.response9
				,	d.response10
				,	d.response11
				,	d.response12
				,	d.response13
				,	m.Id as 'MonMeasureId'
	from			NH_Provider_Base s
		inner join [NursingHomes] n on n.ProviderId = s.provider_id
		left join	nh_responsedetail d
						on	s.provider_id = d.providerid
						and	s.MeasureId = d.measure_id
		left join	Measures m on m.Name = replace(s.MeasureId,'_Rate','')

end