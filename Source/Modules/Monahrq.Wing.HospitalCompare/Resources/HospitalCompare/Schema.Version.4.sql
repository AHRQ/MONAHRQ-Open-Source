--Importing Hospital Readmissions--
SELECT	RCD.[Provider ID] AS [CMSProviderID]
	,	-1 AS ConditionCode
	,	REPLACE ( RCD.[Measure ID],'_','-')  AS MeasureCode
	,	0 AS [CategoryCode]
	,	switch( 
			trim(RCD.[Score]) = 'Not Available', -1,
			true, [Score] 
		) AS [Rate]
	,	switch( 
			trim(RCD.[Denominator]) = 'Not Available', -1,
			true, [Denominator] 
		) AS [Sample]
	,	switch( 
			trim(RCD.[Lower Estimate]) = 'Not Available', -1,
			true, [Lower Estimate] 
		) AS [Lower]
	,	switch( 
			trim(RCD.[Higher Estimate]) = 'Not Available', -1,
			true,[Higher Estimate]
		) AS [Upper]
	,	null AS [Note]
	,	null AS [BenchmarkID]
	,	null AS [Footnote_Id]
FROM @@MDB_TABLE_HospitalOutcome@@ RCD
WHERE RCD.[Measure ID] IN (@@Measures_HospitalReadmission@@)


--Importing State Readmissions-- 
SELECT 
switch( 
Condition ='Heart Attack' ,1,
Condition ='Heart Failure' ,2,
Condition ='Pneumonia' ,3,
Condition ='Surgical' ,4,
Condition ='Children' ,5,
Condition ='Hospital-Wide All-Cause' ,7,
true,0
) AS ConditionCode,

switch(
ConditionCode =7,'READM-30-HOSP-WIDE',
ConditionCode=1 and Instr(1, [Measure Name],'Mortality', 1)>0 ,'MORT-30-AMI',
ConditionCode=1 and Instr(1, [Measure Name],'Mortality', 1)=0 ,'READM-30-AMI',
ConditionCode=2 and Instr(1, [Measure Name],'Mortality', 1)>0 ,'MORT-30-HF',
ConditionCode=2 and Instr(1, [Measure Name],'Mortality', 1)=0 ,'READM-30-HF',
Instr(1, [Measure Name],'Mortality', 1)>0,'MORT-30-PN',
Instr(1, [Measure Name],'Mortality', 1)=0,'READM-30-PN',
true ,[Measure Name]
) AS MeasureCode, 
0 as [CategoryCode] ,
switch( 
trim([Score]) ='Not Available' ,-1,
true,[Score] ) AS [Rate],
-1  as [Sample], 
-1 as [Lower],
-1 as [Upper],
State AS BenchmarkID
FROM [HQI_STATE_TimelyEffectiveCare]
WHERE Condition IN (@@Measures_StateReadmission@@)



--Importing National Readmissions--
SELECT	-1 AS ConditionCode
	,	REPLACE([Measure ID], '_', '-') AS MeasureCode
	,	0 as [CategoryCode]
	,	switch( 
			trim([National Rate]) = 'Not Available', -1,
			true, [National Rate] 
		) AS [Rate]
	,	-1 AS Sample
	,	-1 as [Lower]
	,	-1 as [Upper]
	,	'US' AS BenchmarkID
FROM @@MDB_TABLE_NationalOutcome@@
WHERE [Measure ID] IN (@@Measures_NationalReadmission@@)

--Importing Hospital COMP Measures --
select			RCD.[Provider ID] AS [CMSProviderID]
			,	-1 AS ConditionCode
			,	REPLACE ( RCD.[Measure ID],'_','-')  AS MeasureCode
			,	0 AS [CategoryCode]
			,	switch
					( 
						trim(RCD.[Score]) ='Not Available' ,		-1,
						true,										[Score]
					) AS [Rate]
			,	switch
					( 
						trim(RCD.[Denominator]) ='Not Available' ,	-1,
						true,										[Denominator]
					) AS [Sample]
			,	switch
					( 
						trim(RCD.[Lower Estimate]) ='Not Available' ,-1,
						true,										[Lower Estimate]
					) AS [Lower]
			,	switch
					( 
						trim(RCD.[Higher Estimate])  ='Not Available' ,-1,
						true,										[Higher Estimate]
					) AS [Upper]
			,	null AS [Note]
			,	null AS [BenchmarkID]
			,	null AS [Footnote_Id]
FROM			HQI_HOSP_COMP RCD
WHERE			RCD.[Measure ID] IN (@@Measures_HospitalComp@@)

--Importing National COMP Measures--
select	-1 AS ConditionCode
	  ,	REPLACE([Measure ID], '_', '-') AS MeasureCode
	  ,	0 as [CategoryCode]
	  ,	switch( 
	  		trim([National Rate]) = 'Not Available', -1,
	  		true, [National Rate] 
	  	) AS [Rate]
	  ,	-1 AS Sample
	  ,	-1 as [Lower]
	  ,	-1 as [Upper]
	  ,	'US' AS BenchmarkID
from HQI_NATIONAL_Comp
where [Measure ID] IN (@@Measures_NationalComp@@)

--Importing Hospital Measures--
SELECT	REPLACE ( [Provider ID],'''','') AS [CMSProviderID]
	,	switch( 
			Instr(1, Condition ,'Heart Attack', 1)=1 ,1,
			Instr(1, Condition ,'Heart Failure', 1)=1  ,2,
			Instr(1, Condition ,'Pneumonia', 1)=1 ,3,
			Instr(1, Condition ,'Surgical', 1)=1 ,4,
			Instr(1, Condition ,'Children', 1)=1 ,5,
			Instr(1, Condition ,'Hospital-Wide', 1)=1 ,7,
			true,0
		) AS [ConditionCode]
	,	REPLACE ( [Measure ID],'_','-')   AS MeasureCode
	,	0 AS [CategoryCode]
	,	switch( 
			trim([Score]) ='Not Available' ,-1,
			true, [Score] 
		) AS  Rate
	,	switch( 
			trim([Sample]) ='Not Available' ,-1,
			true, [Sample] 
		) AS [Sample]
	,	-1 as [Lower]
	,	-1 as [Upper]
FROM [HQI_HOSP_TimelyEffectiveCare] 
WHERE [Measure ID] IN (@@Measures_Hospital@@)


--Importing State Measures--
SELECT 
switch( 
Instr(1, Condition ,'Heart Attack', 1)=1 ,1,
Instr(1, Condition ,'Heart Failure', 1)=1  ,2,
Instr(1, Condition ,'Pneumonia', 1)=1 ,3,
Instr(1, Condition ,'Surgical', 1)=1 ,4,
Instr(1, Condition ,'Children', 1)=1 ,5,
Instr(1, Condition ,'Hospital-Wide', 1)=1 ,7,
true,0
)
AS [ConditionCode],
switch(
REPLACE ( [Measure ID],'_','-')='OP-3b','OP-3b',
true,
REPLACE ( [Measure ID],'_','-')
)
AS MeasureCode,
0 as [CategoryCode] ,
switch( 
trim(Score) ='Not Available' ,-1,
true,Score )
AS [Rate],
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
[State]  AS BenchmarkID
                        FROM [HQI_STATE_TimelyEffectiveCare] 
                        WHERE [Measure ID] IN (@@Measures_State1@@)
UNION
SELECT 
switch( 
Instr(1, Condition ,'Heart Attack', 1)=1 ,1,
Instr(1, Condition ,'Heart Failure', 1)=1  ,2,
Instr(1, Condition ,'Pneumonia', 1)=1 ,3,
Instr(1, Condition ,'Surgical', 1)=1 ,4,
Instr(1, Condition ,'Children', 1)=1 ,5,
Instr(1, Condition ,'Hospital-Wide', 1)=1 ,7,
true,0
)
AS [ConditionCode],
REPLACE ( [Measure ID],'_','-')
AS MeasureCode,
0 as [CategoryCode] ,
switch( 
trim(Score) ='Not Available' ,-1,
true,Score )
AS [Rate],
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
[State]  AS BenchmarkID
                        FROM [HQI_STATE_TimelyEffectiveCare] 
                        WHERE [Measure ID] IN (@@Measures_State2@@)


--Importing National Measures-- 
SELECT 
				switch( 
					Instr(1, Condition ,'Heart Attack', 1)=1 ,1,
					Instr(1, Condition ,'Heart Failure', 1)=1  ,2,
					Instr(1, Condition ,'Pneumonia', 1)=1 ,3,
					Instr(1, Condition ,'Surgical', 1)=1 ,4,
					Instr(1, Condition ,'Children', 1)=1 ,5,
					Instr(1, Condition ,'Hospital-Wide', 1)=1 ,7,
					true,0) AS [ConditionCode],
				REPLACE ( [Measure ID],'_','-') AS MeasureCode,
				0 as [CategoryCode] ,
				switch( 
					trim(Score) ='Not Available' ,	-1,
					true,							Score ) AS [Rate],
				-1 AS Sample ,
				-1 as [Lower],
				-1 as [Upper],
				'US'  AS BenchmarkID
FROM			[HQI_NATIONAL_TimelyEffectiveCare] 
WHERE			[Measure ID] IN (@@Measures_National1@@)
	or			[Measure ID] IN (@@Measures_National2@@)



--Importing Hospital Consumer Assessment of Healthcare Providers and Systems--

SELECT
REPLACE ( [Provider ID],'''','') AS [CMSProviderID],
0 AS [ConditionCode],
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE ( [HCAHPS Measure ID],'_','-')
,'-A-P','')
,'-9-10','')
,'-DY','')
,'-Y-P','')
AS MeasureCode,
switch(
(isnumeric([Number of Completed Surveys]) and CInt([Number of Completed Surveys]) >= 300),3,
(isnumeric([Number of Completed Surveys]) and CInt([Number of Completed Surveys]) >= 100),2,
(isnumeric([Number of Completed Surveys]) and CInt([Number of Completed Surveys]) > 0),3,
Instr(1, [Number of Completed Surveys],'300', 1)=1,3,
Instr(1, [Number of Completed Surveys],'Between',1)=1,2,
Instr(1, [Number of Completed Surveys],'Fewer',1)=1,1,
true,0)
AS CategoryCode,

switch( 
trim([HCAHPS Answer Percent])  ='Not Available' ,-1,
true,[HCAHPS Answer Percent]  )
AS [Rate],

switch( 
trim([Survey Response Rate Percent]) ='Not Available' ,-1,
true,[Survey Response Rate Percent]  )
AS [Sample],
-1 as [Lower],
-1 as [Upper]

FROM [HQI_HOSP_HCAHPS] 

WHERE [HCAHPS Measure ID] IN (@@Measures_HospitalConsumerAssessment@@)


--Importing State Hospital Consumer Assessment of Healthcare Providers and Systems--
SELECT 
0 AS [ConditionCode],

REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE ([HCAHPS Measure ID],'_','-')
,'-A-P','')
,'-9-10','')
,'-DY','')
,'-Y-P','')
AS MeasureCode,
0 AS CategoryCode,
switch( 
trim([HCAHPS Answer Percent])  ='Not Available' ,-1,
true, [HCAHPS Answer Percent]  )
AS [Rate],
-1 AS [Sample],
-1 as [Lower],
-1 as [Upper],
State AS BenchmarkID 


                        FROM [HQI_STATE_HCAHPS] 
                       WHERE [HCAHPS Measure ID] IN (@@Measures_StateHospitalConsumerAssessment@@)

--Importing National Hospital Consumer Assessment of Healthcare Providers and Systems--
SELECT 
0 AS [ConditionCode],
REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE ([HCAHPS Measure ID],'_','-')
,'-A-P','')
,'-9-10','')
,'-DY','')
,'-Y-P','')
AS MeasureCode,
0 AS CategoryCode,
switch( 
 trim([HCAHPS Answer Percent])  ='Not Available' ,-1,
true, [HCAHPS Answer Percent]   )
AS [Rate],
-1 AS [Sample],
-1 as [Lower],
-1 as [Upper],
'US' AS BenchmarkID 
                        FROM [HQI_NATIONAL_HCAHPS] 
                        WHERE [HCAHPS Measure ID] IN (@@Measures_NationalHospitalConsumerAssessment@@)

--Importing Hospital Imaging-- 
SELECT
REPLACE ( [Provider ID],'''','') AS [CMSProviderID],
6 AS [ConditionCode],
REPLACE ( [Measure ID],'_','-')  AS MeasureCode, 
0 AS CategoryCode,

switch( 
trim(Score) ='Not Available' ,-1,
true,Score )
AS [Rate],

-1 AS Sample ,
-1 as [Lower],
-1 as [Upper]
                        FROM [HQI_HOSP_IMG] 
                        WHERE [Measure ID] IN (@@Measures_HospitalImaging@@)

--Importing State Imaging--
SELECT 
6 AS [ConditionCode],
REPLACE ( [Measure ID],'_','-')  AS MeasureCode, 
0 AS CategoryCode,
switch( 
(not (trim(Score) ='Not Available')) ,  val(Score),
trim(Score) ='Not Available' ,-1,
true,Score )
AS [Rate],
-1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
State AS BenchmarkID
                        FROM [HQI_STATE_IMG_AVG] 
                        WHERE [Measure ID] IN (@@Measures_StateImaging@@)


--Importing National Imaging--
SELECT 
6 AS [ConditionCode],
REPLACE ( [Measure ID],'_','-')  AS MeasureCode, 
0 AS CategoryCode,
switch( 
(not (trim(Score) ='Not Available')) , val(Score),
trim(Score) ='Not Available' ,-1,
true,Score )
AS [Rate],

-1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
'US' AS BenchmarkID
                        FROM [HQI_NATIONAL_IMG_AVG] 
                        WHERE [Measure ID] IN (@@Measures_NationalImaging@@)

--Importing Hospital Healthcare-Associated Infections < HAI-1 >--
SELECT left([Provider ID],6) as CMSProviderID ,
0 AS [ConditionCode],
'HAI-1' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
 [Measure ID]='HAI_1_SIR' and [score]='Not Available',-1,
 [Measure ID]='HAI_1_SIR' and Iif(IsNull([score]), 0, -1)=0,-1,
 [Measure ID]='HAI_1_SIR',Iif(IsNull([score]), 0, [score]),
true,0
)
) as Rate,
sum(
switch(
 [Measure ID]='HAI_1_CI_LOWER' and  [score]='Not Available' ,-1,
 [Measure ID]='HAI_1_CI_LOWER' and (Iif(IsNull([score]), 0, -1)=0 or [Score]='-') ,-1,
 [Measure ID]='HAI_1_CI_LOWER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Lower],
sum(
switch(
 [Measure ID]='HAI_1_CI_UPPER' and [score]='Not Available' ,-1,
 [Measure ID]='HAI_1_CI_UPPER' and Iif(IsNull([score]), 0, -1)=0 ,-1,
 [Measure ID]='HAI_1_CI_UPPER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Upper]
from
HQI_HOSP_HAI
where [Measure ID] IN (@@Measures_HospitalHAI_1@@)
group by  (left([Provider ID],6) )

--Importing Hospital Healthcare-Associated Infections < HAI-2 >--
SELECT left([Provider ID],6) as CMSProviderID ,
0 AS [ConditionCode],
'HAI-2' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_2_SIR' and [score]='Not Available',-1,
[Measure ID]='HAI_2_SIR' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_2_SIR',Iif(IsNull([score]), 0, [score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_2_CI_LOWER' and [score]='Not Available',-1,
[Measure ID]='HAI_2_CI_LOWER' and (Iif(IsNull([score]), 0, -1)=0 or [Score]='-') ,-1,
[Measure ID]='HAI_2_CI_LOWER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_2_CI_UPPER' and [score]='Not Available',-1,
[Measure ID]='HAI_2_CI_UPPER' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_2_CI_UPPER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Upper]
from
HQI_HOSP_HAI
where [Measure ID] IN (@@Measures_HospitalHAI_2@@)
group by  (left([Provider ID],6) )

--Importing Hospital Healthcare-Associated Infections < HAI-3 >--
SELECT left([Provider ID],6) as CMSProviderID ,
0 AS [ConditionCode],
'HAI-3' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_3_SIR' and [score]='Not Available',-1,
[Measure ID]='HAI_3_SIR' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_3_SIR',Iif(IsNull([score]), 0, [score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_3_CI_LOWER' and [score]='Not Available',-1,
[Measure ID]='HAI_3_CI_LOWER' and (Iif(IsNull([score]), 0, -1)=0 or [Score]='-') ,-1,
[Measure ID]='HAI_3_CI_LOWER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_3_CI_UPPER' and [score]='Not Available',-1,
[Measure ID]='HAI_3_CI_UPPER' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_3_CI_UPPER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Upper]
from
HQI_HOSP_HAI
where [Measure ID] IN (@@Measures_HospitalHAI_3@@)
group by  (left([Provider ID],6) )

--Importing Hospital Healthcare-Associated Infections < HAI-4 >--
SELECT left([Provider ID],6) as CMSProviderID ,
0 AS [ConditionCode],
'HAI-4' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_4_SIR' and [score]='Not Available',-1,
[Measure ID]='HAI_4_SIR' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_4_SIR',Iif(IsNull([score]), 0, [score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_4_CI_LOWER' and [score]='Not Available',-1,
[Measure ID]='HAI_4_CI_LOWER' and (Iif(IsNull([score]), 0, -1)=0 or [Score]='-') ,-1,
[Measure ID]='HAI_4_CI_LOWER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_4_CI_UPPER' and [score]='Not Available',-1,
[Measure ID]='HAI_4_CI_UPPER' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_4_CI_UPPER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Upper]
from
HQI_HOSP_HAI
where [Measure ID] IN (@@Measures_HospitalHAI_4@@)
group by  (left([Provider ID],6) )

--Importing Hospital Healthcare-Associated Infections < HAI-5 >--
SELECT left([Provider ID],6) as CMSProviderID ,
0 AS [ConditionCode],
'HAI-5' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_5_SIR' and [score]='Not Available',-1,
[Measure ID]='HAI_5_SIR' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_5_SIR',Iif(IsNull([score]), 0, [score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_5_CI_LOWER' and [score]='Not Available',-1,
[Measure ID]='HAI_5_CI_LOWER' and (Iif(IsNull([score]), 0, -1)=0 or [Score]='-') ,-1,
[Measure ID]='HAI_5_CI_LOWER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_5_CI_UPPER' and [score]='Not Available',-1,
[Measure ID]='HAI_5_CI_UPPER' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_5_CI_UPPER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Upper]
from
HQI_HOSP_HAI
where [Measure ID] IN (@@Measures_HospitalHAI_5@@)
group by  (left([Provider ID],6) )

--Importing Hospital Healthcare-Associated Infections < HAI-6 >--
SELECT left([Provider ID],6) as CMSProviderID ,
0 AS [ConditionCode],
'HAI-6' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_6_SIR' and [score]='Not Available',-1,
[Measure ID]='HAI_6_SIR' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_6_SIR',Iif(IsNull([score]), 0, [score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_6_CI_LOWER' and [score]='Not Available',-1,
[Measure ID]='HAI_6_CI_LOWER' and (Iif(IsNull([score]), 0, -1)=0 or [Score]='-') ,-1,
[Measure ID]='HAI_6_CI_LOWER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_6_CI_UPPER' and [score]='Not Available',-1,
[Measure ID]='HAI_6_CI_UPPER' and Iif(IsNull([score]), 0, -1)=0,-1,
[Measure ID]='HAI_6_CI_UPPER',Iif(IsNull([score]), 0, [score]),
true,0
)
) as [Upper]
from
HQI_HOSP_HAI
where [Measure ID] IN (@@Measures_HospitalHAI_6@@)
group by  (left([Provider ID],6) )

--Importing State Healthcare-Associated Infections < HAI-1 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
'HAI-1' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_1_SIR' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_1_SIR',val([Score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_1_CI_LOWER' and (trim([Score])='Not Available' or [Score]='-') ,-1,
[Measure ID]='HAI_1_CI_LOWER',val([Score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_1_CI_UPPER' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_1_CI_UPPER',val([Score]),
true,0
)
) as [Upper]
from
[HQI_STATE_HAI]
where [Measure ID] IN (@@Measures_StateHAI_1@@)
group by  state

--Importing State Healthcare-Associated Infections < HAI-2 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
'HAI-2' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_2_SIR' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_2_SIR',val([Score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_2_CI_LOWER' and (trim([Score])='Not Available' or [Score]='-') ,-1,
[Measure ID]='HAI_2_CI_LOWER',val([Score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_2_CI_UPPER' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_2_CI_UPPER',val([Score]),
true,0
)
) as [Upper]
from
[HQI_STATE_HAI]
where [Measure ID] IN (@@Measures_StateHAI_2@@)
group by  state

--Importing State Healthcare-Associated Infections < HAI-5 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
'HAI-5' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_5_SIR' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_5_SIR',val([Score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_5_CI_LOWER' and (trim([Score])='Not Available' or [Score]='-') ,-1,
[Measure ID]='HAI_5_CI_LOWER',val([Score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_5_CI_UPPER' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_5_CI_UPPER',val([Score]),
true,0
)
) as [Upper]
from
[HQI_STATE_HAI]
where [Measure ID] IN (@@Measures_StateHAI_5@@)
group by  state

--Importing State Healthcare-Associated Infections < HAI-6 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
'HAI-6' AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
[Measure ID]='HAI_6_SIR' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_6_SIR',val([Score]),
true,0
)
) as Rate,
sum(
switch(
[Measure ID]='HAI_6_CI_LOWER' and (trim([Score])='Not Available' or [Score]='-') ,-1,
[Measure ID]='HAI_6_CI_LOWER',val([Score]),
true,0
)
) as [Lower],
sum(
switch(
[Measure ID]='HAI_6_CI_UPPER' and trim([Score])='Not Available',-1,
[Measure ID]='HAI_6_CI_UPPER',val([Score]),
true,0
)
) as [Upper]
from
[HQI_STATE_HAI]
where [Measure ID] IN (@@Measures_StateHAI_6@@)
group by  state

--Importing National Healthcare-Associated Infections--

SELECT	0 AS ConditionCode
	,	Replace(LEFT([Measure ID], LEN([Measure ID]) - 4),'_','-') AS MeasureCode
	,	0 as [CategoryCode]
	,	switch( 
			trim([Score]) = 'Not Available', -1,
			true, [Score] 
		) AS [Rate]
	,	-1 AS Sample
	,	-1 as [Lower]
	,	-1 as [Upper]
	,	'US' AS BenchmarkID
FROM [HQI_NATIONAL_HAI] 
WHERE RIGHT([Measure ID], 4) = '_SIR' 
AND [Measure ID] IN (@@Measures_NationalHAI_All@@)

--Importing Hospital Emergency Discharge--
SELECT [Provider ID] AS CMSProviderID, 
Replace([Measure ID],'_','-') AS MeasureCode,

switch( 
trim([Score])  ='Not Available' ,-1,
true,[Score]  )
AS [Rate], 

switch( 
trim([Sample])  ='Not Available' ,-1,
true,[Sample]  )
AS [Sample],
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode] 
                        FROM [HQI_HOSP_TimelyEffectiveCare] 
                        WHERE [Measure ID] IN (@@Measures_HospitalEmergencyDischarge@@)

--Importing Hospital Overall Ratings-- 
SELECT	[Provider ID] as CMSProviderID
	,	switch( 
			trim([Hospital overall rating]) is null, 0,
			trim([Hospital overall rating]) = 'Not Available', 0,
			true,[Hospital overall rating]
		) as [Sample]
	,	'CMS-OVERALL-STAR' as MeasureCode
from [HQI_HOSP]


--Importing State Emergency Discharge-- 
SELECT 
[Provider ID] AS CMSProviderID,
Replace([Measure ID],'_','-') AS MeasureCode,
switch( 
trim([Score])  ='Not Available' ,-1,
true,[Score]  )
AS [Rate],
-1 AS [Sample],
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode]  
FROM [HQI_HOSP_TimelyEffectiveCare] 
WHERE [Measure ID] IN (@@Measures_StateEmergencyDischarge@@)

--Importing National Emergency Discharge--
SELECT	Replace([Measure ID],'_','-') AS MeasureCode
	,	switch( 
			trim([Score]) = 'Not Available', -1,
			true, [Score] 
		) AS [Rate]
	,	-1 AS [Sample]
	,	-1 as [Lower]
	,	-1 as [Upper]
	,	0 AS ConditionCode
	,	0 as [CategoryCode]
	,	'US' AS BenchmarkID  
FROM [HQI_NATIONAL_TimelyEffectiveCare] 
WHERE [Measure ID] IN (@@Measures_NationalEmergencyDischarge@@)

--Importing Hospital Immunizations--
SELECT 
[Provider ID] AS CMSProviderID,
Replace([Measure ID],'_','-') AS MeasureCode,

switch( 
trim([Score])  ='Not Available' ,-1,
true,[Score]  )
AS [Rate],
switch( 
trim([Sample])  ='Not Available' ,-1,
true,[Sample]  )
AS [Sample], 
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode]
                                   
FROM [HQI_HOSP_TimelyEffectiveCare] 
 
WHERE [Measure ID] IN (@@Measures_HospitalImmunizations@@)

--Importing State Immunizations--
SELECT 
[State] AS BenchmarkID,
Replace([Measure ID],'_','-') AS MeasureCode,
switch( 
trim([Score])  ='Not Available' ,-1,
true,[Score]  )
AS [Rate],
-1 AS [Sample], 
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode]
FROM [HQI_STATE_TimelyEffectiveCare] 
WHERE [Measure ID] IN (@@Measures_StateImmunizations@@)

--Importing National Immunizations--
SELECT	Replace([Measure ID],'_','-') AS MeasureCode
	,	switch( 
			trim([Score]) = 'Not Available', -1,
			true, [Score] 
		) AS [Rate]
	,	-1 AS [Sample] 
	,	-1 as [Lower]
	,	-1 as [Upper]
	,	0 AS ConditionCode
	,	0 as [CategoryCode]
	,	'US' AS BenchmarkID
FROM [HQI_NATIONAL_TimelyEffectiveCare] 
WHERE [Measure ID] IN (@@Measures_NationalImmunizations@@)

--Importing Structural--			
SELECT	[Provider ID] AS [CMSProviderID]
	  ,	Replace([Measure ID],'_','-') as MeasureCode
	  ,	0 as Rate
	  ,	switch
	  	(
	  		[Measure Response] = 'Y',	1,
	  		[Measure Response] = 'Yes',	1,
	  		true,						0
	  	) as [Sample]
	  ,	-1 as [Lower]
	  ,	-1 as [Upper]
	  ,	0 as ConditionCode
	  ,	0 as [CategoryCode]
FROM [HQI_HOSP_STRUCTURAL] 
WHERE [Measure ID] IN (@@Measures_HospitalStructural@@)


--Importing National Measures - Post Processs - [MONAHRQ-DB]--

	with
		Tile1Data as
		(
			select			MeasureCode
						,	NTILE(10) over (partition by MeasureCode order by Rate desc) as Tile
						,	Rate
			from			Targets_HospitalCompareTargets thct 
				inner join	Measures m on m.Name = thct.MeasureCode
			where		(	REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_National1@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_National2@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalImaging@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalReadmission@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalEmergencyDischarge@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalImmunizations@@))
				and			thct.Rate <> -1
				and			m.HigherScoresAreBetter = 1
				and			thct.Dataset_Id = @@Dataset_id@@
		),
		Top10 as
		(
			select			MeasureCode
						,	Rate
			from			Tile1Data
			where			Tile = 1
		),
		SecondBest AS
		(
			select distinct MeasureCode
						,	Rate
						,	RANK() over (partition by MeasureCode order by Rate asc) AS AddedRank
			from			Top10
		)
insert			Targets_HospitalCompareTargets(CMSProviderID,ConditionCode,MeasureCode,CategoryCode,Rate,[Sample],[Lower],[Upper],Note,BenchmarkID,Dataset_Id,Footnote_Id)
select			null as CMSProviderID
			,	0 as ConditionCode
			,	SecondBest.MeasureCode as MeasureCode
			,	0 as CategoryCode
			,	SecondBest.Rate as Rate
			,	-1 as Sample
			,	-1 as Lower
			,	-1 as Upper
			,	null as Note
			,	'TOP10' as BenchmarkID
			,	@@Dataset_id@@ as Dataset_id
			,	null as Footnote_Id
from			SecondBest
where			SecondBest.AddedRank = 1;


	with
		Tile1Data as
		(
			select			MeasureCode
						,	NTILE(10) over (partition by MeasureCode order by Rate asc) as Tile
						,	Rate
			from			Targets_HospitalCompareTargets thct 
				inner join	Measures m on m.Name = thct.MeasureCode
			where		(	REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_National1@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_National2@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalImaging@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalReadmission@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalEmergencyDischarge@@)
					or		REPLACE(thct.MeasureCode,'-','_') IN (@@Measures_NationalImmunizations@@))
				and			thct.Rate <> -1
				and			m.HigherScoresAreBetter = 0
				and			thct.Dataset_Id = @@Dataset_id@@
		),
		Top10 as
		(
			select			MeasureCode
						,	Rate
			from			Tile1Data
			where			Tile = 1
		),
		SecondBest AS
		(
			select distinct MeasureCode
						,	Rate
						,	RANK() over (partition by MeasureCode order by Rate desc) AS AddedRank
			from			Top10
		)
insert			Targets_HospitalCompareTargets(CMSProviderID,ConditionCode,MeasureCode,CategoryCode,Rate,[Sample],[Lower],[Upper],Note,BenchmarkID,Dataset_Id,Footnote_Id)
select			null as CMSProviderID
			,	0 as ConditionCode
			,	SecondBest.MeasureCode as MeasureCode
			,	0 as CategoryCode
			,	SecondBest.Rate as Rate
			,	-1 as Sample
			,	-1 as Lower
			,	-1 as Upper
			,	null as Note
			,	'TOP10' as BenchmarkID
			,	@@Dataset_id@@ as Dataset_id
			,	null as Footnote_Id
from			SecondBest
where			SecondBest.AddedRank = 1;