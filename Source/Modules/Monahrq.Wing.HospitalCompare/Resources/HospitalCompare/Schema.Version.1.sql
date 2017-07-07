--Importing Hospital Readmissions--
SELECT 

REPLACE ( [Provider Number],"'","") AS [CMSProviderID],

switch( 
Condition ="Heart Attack" ,1,
Condition ="Heart Failure" ,2,
Condition ="Pneumonia" ,3,
Condition ="Surgical" ,4,
Condition ="Children" ,5,
Condition ="Hospital-Wide All-Cause" ,7,
true,0
)
AS [ConditionCode],

switch(
ConditionCode =7,"READM-30-HOSP-WIDE",

ConditionCode=1 and Instr([Measure Name],"Mortality")>0 ,"MORT-30-AMI",
ConditionCode=1 and Instr([Measure Name],"Mortality")=0 ,"READM-30-AMI",

ConditionCode=2 and Instr([Measure Name],"Mortality")>0 ,"MORT-30-HF",
ConditionCode=2 and Instr([Measure Name],"Mortality")=0 ,"READM-30-HF",



Instr([Measure Name],"Mortality")>0,"MORT-30-PN",
Instr([Measure Name],"Mortality")=0,"READM-30-PN",


true ,[Measure Name]
)
 AS [MeasureCode], 

0 AS [CategoryCode],

switch( 
Mortality_Readm_Compl_Rate ="Not Available" ,-1,
true,Mortality_Readm_Compl_Rate )
AS [Rate], 

switch( 
[Number of Patients] ="Not Available" ,-1,
true,[Number of Patients] )
AS [Sample],

switch( 
 [Lower Mortality_Readm Estimate] ="Not Available" ,-1,
true, [Lower Mortality_Readm Estimate] )
AS [Lower], 

switch( 
[Upper Mortality_Readm Estimate]  ="Not Available" ,-1,
true,[Upper Mortality_Readm Estimate]  )
AS [Upper],

null AS [Note],
null AS [BenchmarkID],
null AS [Footnote_Id]
FROM dbo_vwHQI_HOSP_MORTALITY_READM_XWLK
WHERE Condition IN ('Heart Attack', 'Heart Failure', 'Pneumonia', 'Hospital-Wide All-Cause');


--Importing State Readmissions--
SELECT 

switch( 
Condition ="Heart Attack" ,1,
Condition ="Heart Failure" ,2,
Condition ="Pneumonia" ,3,
Condition ="Surgical" ,4,
Condition ="Children" ,5,
Condition ="Hospital-Wide All-Cause" ,7,
true,0
)
AS ConditionCode,

switch(
ConditionCode =7,"READM-30-HOSP-WIDE",

ConditionCode=1 and Instr([Measure Name],"Mortality")>0 ,"MORT-30-AMI",
ConditionCode=1 and Instr([Measure Name],"Mortality")=0 ,"READM-30-AMI",

ConditionCode=2 and Instr([Measure Name],"Mortality")>0 ,"MORT-30-HF",
ConditionCode=2 and Instr([Measure Name],"Mortality")=0 ,"READM-30-HF",



Instr([Measure Name],"Mortality")>0,"MORT-30-PN",
Instr([Measure Name],"Mortality")=0,"READM-30-PN",


true ,[Measure Name]
)
 AS MeasureCode, 
0 as [CategoryCode] ,
-1 as [Rate],
 [Number of Hospitals] AS Sample ,
-1 as [Lower],
-1 as [Upper],

State AS BenchmarkID
                        FROM [dbo_vwHQI_STATE_MORTALITY_READM_SCRE] 
                        WHERE Condition IN ('Heart Attack', 'Heart Failure', 'Pneumonia', 'Hospital-Wide All-Cause')


--Importing National Readmissions--
SELECT
switch( 
Condition ="Heart Attack" ,1,
Condition ="Heart Failure" ,2,
Condition ="Pneumonia" ,3,
Condition ="Surgical" ,4,
Condition ="Children" ,5,
Condition ="Hospital-Wide All-Cause" ,7,
true,0
)
AS ConditionCode,
switch(
ConditionCode =7,"READM-30-HOSP-WIDE",

ConditionCode=1 and Instr([Measure Name],"Mortality")>0 ,"MORT-30-AMI",
ConditionCode=1 and Instr([Measure Name],"Mortality")=0 ,"READM-30-AMI",

ConditionCode=2 and Instr([Measure Name],"Mortality")>0 ,"MORT-30-HF",
ConditionCode=2 and Instr([Measure Name],"Mortality")=0 ,"READM-30-HF",



Instr([Measure Name],"Mortality")>0,"MORT-30-PN",
Instr([Measure Name],"Mortality")=0,"READM-30-PN",


true ,[Measure Name]
)
 AS MeasureCode, 
0 as [CategoryCode] ,
 [National Mortality_Readm Rate] AS Rate ,
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
"US" AS BenchmarkID
                        FROM [dbo_vwHQI_US_NATIONAL_MORTALITY_READM_RATE] 
                        WHERE Condition IN ('Heart Attack', 'Heart Failure', 'Pneumonia', 'Hospital-Wide All-Cause')


--Importing Hospital Measures--

SELECT 
REPLACE ( [Provider Number],"'","") AS [CMSProviderID],
switch( 
Instr(Condition ,"Heart Attack")=1 ,1,
Instr(Condition ,"Heart Failure")=1  ,2,
Instr(Condition ,"Pneumonia")=1 ,3,
Instr(Condition ,"Surgical")=1 ,4,
Instr(Condition ,"Children")=1 ,5,
Instr(Condition ,"Hospital-Wide")=1 ,7,
true,0
)
AS [ConditionCode],
REPLACE ( [Measure Code],"_","-")   AS MeasureCode, 
0 AS [CategoryCode],

switch( 
 [Score] ="Not Available" ,-1,
true, [Score] )
AS  Rate,
 switch( 
 [dbo_vwHQI_HOSP_MSR_XWLK].Sample ="Not Available" ,-1,
true, [dbo_vwHQI_HOSP_MSR_XWLK].Sample ) AS Sample,
-1 as [Lower],
-1 as [Upper]

                        FROM [dbo_vwHQI_HOSP_MSR_XWLK] 
                        WHERE [Measure Code] IN 
                        ('AMI_2', 'AMI_7a', 'AMI_8a', 'AMI_10', 'HF_1', 'HF_2', 'HF_3', 'OP_2', 'OP_3b', 'OP_4', 'OP_5', 'OP_6', 'OP_7', 'PC_01', 'PN_3B', 'PN_6', 
                        'SCIP_CARD_2', 'SCIP_INF_1', 'SCIP_INF_2', 'SCIP_INF_3', 'SCIP_INF_4', 'SCIP_INF_9', 'SCIP_INF_10', 'SCIP_VTE_2', 'VTE_6')


--Importing State Measures--

SELECT 
switch( 
Instr(Condition ,"Heart Attack")=1 ,1,
Instr(Condition ,"Heart Failure")=1  ,2,
Instr(Condition ,"Pneumonia")=1 ,3,
Instr(Condition ,"Surgical")=1 ,4,
Instr(Condition ,"Children")=1 ,5,
Instr(Condition ,"Hospital-Wide")=1 ,7,
true,0
)
AS [ConditionCode],

switch(
REPLACE ( [Measure Code],"_","-")="OP-3b","OP-3b",

true,
REPLACE ( [Measure Code],"_","-")
)
AS MeasureCode,

0 as [CategoryCode] ,
switch( 
Score ="Not Available" ,-1,
true,Score )
AS [Rate],
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
State AS BenchmarkID
  
                        FROM [dbo_vwHQI_STATE_MSR_AVG] 
                        WHERE [Measure Code] IN 
                        ('AMI_2', 'AMI_7a', 'AMI_8a', 'AMI_10', 'HF_1', 'HF_2', 'HF_3', 'OP_1', 'OP_2', 'OP_3b', 'OP_4', 'OP_5', 'OP_6', 'OP_7', 'PC_01', 'PN_3B', 'PN_6', 
                        'SCIP_CARD_2', 'SCIP_INF_1', 'SCIP_INF_2', 'SCIP_INF_3', 'SCIP_INF_4', 'SCIP_INF_9', 'SCIP_VTE_1', 'SCIP_VTE_2', 'VTE_6')
UNION
SELECT 
switch( 
Instr(Condition ,"Heart Attack")=1 ,1,
Instr(Condition ,"Heart Failure")=1  ,2,
Instr(Condition ,"Pneumonia")=1 ,3,
Instr(Condition ,"Surgical")=1 ,4,
Instr(Condition ,"Children")=1 ,5,
Instr(Condition ,"Hospital-Wide")=1 ,7,
true,0
)
AS [ConditionCode],

REPLACE ( [Measure Code],"_","-")
AS MeasureCode,

0 as [CategoryCode] ,
switch( 
Score ="Not Available" ,-1,
true,Score )
AS [Rate],
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
State AS BenchmarkID
  
                        FROM [dbo_vwHQI_STATE_MSR_AVG] 
                        WHERE [Measure Code] IN 
                        ('SCIP_INF_10')

--Importing National Measures--
SELECT 
switch( 
Instr(Condition ,"Heart Attack")=1 ,1,
Instr(Condition ,"Heart Failure")=1  ,2,
Instr(Condition ,"Pneumonia")=1 ,3,
Instr(Condition ,"Surgical")=1 ,4,
Instr(Condition ,"Children")=1 ,5,
Instr(Condition ,"Hospital-Wide")=1 ,7,
true,0
)
AS [ConditionCode],
switch(
REPLACE ( [Measure Code],"_","-")="OP-3b","OP-3b",

true,
REPLACE ( [Measure Code],"_","-")
)
AS MeasureCode,
0 as [CategoryCode] ,
switch( 
Score ="Not Available" ,-1,
true,Score )
AS [Rate],
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
 switch(
Instr(Percentile,"Top 10%")>0,"TOP10",
true, "US")  AS BenchmarkID
                        FROM [dbo_vwHQI_PCTL_MSR_XWLK] 
                        WHERE [Measure Code] IN 
                        ('AMI_2', 'AMI_7a', 'AMI_8a', 'AMI_10', 'HF_1', 'HF_2', 'HF_3', 'OP_1', 'OP_2', 'OP_3b', 'OP_4', 'OP_5', 'OP_6', 'OP_7', 'PC_01', 'PN_3B', 'PN_6',
                        'SCIP_CARD_2', 'SCIP_INF_1', 'SCIP_INF_2', 'SCIP_INF_3', 'SCIP_INF_4', 'SCIP_INF_9', 'SCIP_VTE_1', 'SCIP_VTE_2', 'VTE_6')
UNION
SELECT 
switch( 
Instr(Condition ,"Heart Attack")=1 ,1,
Instr(Condition ,"Heart Failure")=1  ,2,
Instr(Condition ,"Pneumonia")=1 ,3,
Instr(Condition ,"Surgical")=1 ,4,
Instr(Condition ,"Children")=1 ,5,
Instr(Condition ,"Hospital-Wide")=1 ,7,
true,0
)
AS [ConditionCode],

REPLACE ( [Measure Code],"_","-")
AS MeasureCode,

0 as [CategoryCode] ,
switch( 
Score ="Not Available" ,-1,
true,Score )
AS [Rate],
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
 switch(
Instr(Percentile,"Top 10%")>0,"TOP10",
true, "US")  AS BenchmarkID
                        FROM [dbo_vwHQI_PCTL_MSR_XWLK] 
                        WHERE [Measure Code] IN 
                        ('SCIP_INF_10')

--Importing Hospital Hospital Consumer Assessment of Healthcare Providers and Systems--

 SELECT

REPLACE ( [Provider Number],"'","") AS [CMSProviderID],

0 AS [ConditionCode],

REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE ( [HCAHPS Measure Code],"_","-")
,"-A-P","")
,"-9-10","")
,"-DY","")
,"-Y-P","")
AS MeasureCode,

switch(
Instr([Number of Completed Surveys],"300")=1,3,
Instr([Number of Completed Surveys],"Between")=1,2,
Instr([Number of Completed Surveys],"Fewer")=1,1,
true,0)
AS CategoryCode,

switch( 
[HCAHPS Answer Percent]  ="Not Available" ,-1,
true,[HCAHPS Answer Percent]  )
AS [Rate],

switch( 
[Survey Response Rate Percent] ="Not Available" ,-1,
true,[Survey Response Rate Percent]  )
AS [Sample],
-1 as [Lower],
-1 as [Upper]

FROM [dbo_vwHQI_HOSP_HCAHPS_MSR] 

WHERE [HCAHPS Measure Code] IN 
('H_CLEAN_HSP_A_P', 'H_COMP_1_A_P', 'H_COMP_2_A_P', 'H_COMP_3_A_P', 'H_COMP_4_A_P', 'H_COMP_5_A_P', 'H_QUIET_HSP_A_P', 
'H_CLEAN_HSP_SN_P', 'H_COMP_1_SN_P', 'H_COMP_2_SN_P', 'H_COMP_3_SN_P', 'H_COMP_4_SN_P', 'H_COMP_5_SN_P', 'H_QUIET_HSP_SN_P', 
'H_CLEAN_HSP_U_P', 'H_COMP_1_U_P', 'H_COMP_2_U_P', 'H_COMP_3_U_P', 'H_COMP_4_U_P', 'H_COMP_5_U_P', 'H_QUIET_HSP_U_P', 
'H_COMP_6_Y_P', 'H_HSP_RATING_0_6', 'H_HSP_RATING_7_8', 'H_HSP_RATING_9_10', 'H_RECMND_DY', 'H_RECMND_PY', 'H_RECMND_DN')

--Importing State Hospital Consumer Assessment of Healthcare Providers and Systems--
SELECT 
0 AS [ConditionCode],

REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE ([HCAHPS Measure Code],"_","-")
,"-A-P","")
,"-9-10","")
,"-DY","")
,"-Y-P","")
AS MeasureCode,
0 AS CategoryCode,
switch( 
 [HCAHPS Answer Percent]  ="Not Available" ,-1,
true, [HCAHPS Answer Percent]  )
AS [Rate],
-1 AS [Sample],
-1 as [Lower],
-1 as [Upper],
State AS BenchmarkID 


                        FROM [dbo_vwHQI_STATE_HCAHPS_MSR] 
                       WHERE [HCAHPS Measure Code] IN 
                        ('H_CLEAN_HSP_A_P', 'H_COMP_1_A_P', 'H_COMP_2_A_P', 'H_COMP_3_A_P', 'H_COMP_4_A_P', 'H_COMP_5_A_P', 'H_QUIET_HSP_A_P', 
                       'H_CLEAN_HSP_SN_P', 'H_COMP_1_SN_P', 'H_COMP_2_SN_P', 'H_COMP_3_SN_P', 'H_COMP_4_SN_P', 'H_COMP_5_SN_P', 'H_QUIET_HSP_SN_P', 
                       'H_CLEAN_HSP_U_P', 'H_COMP_1_U_P', 'H_COMP_2_U_P', 'H_COMP_3_U_P', 'H_COMP_4_U_P', 'H_COMP_5_U_P', 'H_QUIET_HSP_U_P', 
                      'H_COMP_6_Y_P', 'H_HSP_RATING_0_6', 'H_HSP_RATING_7_8', 'H_HSP_RATING_9_10', 'H_RECMND_DY', 'H_RECMND_PY', 'H_RECMND_DN')


--Importing National Hospital Consumer Assessment of Healthcare Providers and Systems--
SELECT 

0 AS [ConditionCode],

REPLACE(
REPLACE(
REPLACE(
REPLACE(
REPLACE ([HCAHPS Measure Code],"_","-")
,"-A-P","")
,"-9-10","")
,"-DY","")
,"-Y-P","")
AS MeasureCode,
0 AS CategoryCode,
switch( 
 [HCAHPS Answer Percent]   ="Not Available" ,-1,
true, [HCAHPS Answer Percent]   )
AS [Rate],
-1 AS [Sample],
-1 as [Lower],
-1 as [Upper],
"US" AS BenchmarkID 

                        FROM [dbo_vwHQI_US_NATIONAL_HCAHPS_MSR] 
                        WHERE [HCAHPS Measure Code] IN 
                        ('H_CLEAN_HSP_A_P', 'H_COMP_1_A_P', 'H_COMP_2_A_P', 'H_COMP_3_A_P', 'H_COMP_4_A_P', 'H_COMP_5_A_P', 'H_QUIET_HSP_A_P', 
                        'H_CLEAN_HSP_SN_P', 'H_COMP_1_SN_P', 'H_COMP_2_SN_P', 'H_COMP_3_SN_P', 'H_COMP_4_SN_P', 'H_COMP_5_SN_P', 'H_QUIET_HSP_SN_P', 
                        'H_CLEAN_HSP_U_P', 'H_COMP_1_U_P', 'H_COMP_2_U_P', 'H_COMP_3_U_P', 'H_COMP_4_U_P', 'H_COMP_5_U_P', 'H_QUIET_HSP_U_P', 
                        'H_COMP_6_Y_P', 'H_HSP_RATING_0_6', 'H_HSP_RATING_7_8', 'H_HSP_RATING_9_10', 'H_RECMND_DY', 'H_RECMND_PY', 'H_RECMND_DN')

--Importing Hospital Imaging--
SELECT
REPLACE ( [Provider Number],"'","") AS [CMSProviderID],
6 AS [ConditionCode],
REPLACE ( [Measure Code],"_","-")  AS MeasureCode, 
0 AS CategoryCode,

switch( 
Score ="Not Available" ,-1,
true,Score )
AS [Rate],
  switch( 
[dbo_vwHQI_HOSP_IMG_XWLK].Sample ="Not Available" ,-1,
true, [dbo_vwHQI_HOSP_IMG_XWLK].Sample ) AS Sample ,
-1 as [Lower],
-1 as [Upper]
                        FROM [dbo_vwHQI_HOSP_IMG_XWLK] 
                        WHERE [Measure Code] IN ('OP_8', 'OP_10', 'OP_11', 'OP_13', 'OP_14')

--Importing State Imaging--
SELECT 
6 AS [ConditionCode],
REPLACE ( [Measure Code],"_","-")  AS MeasureCode, 
0 AS CategoryCode,

switch( 
(not (Score ="Not Available")) ,  val(Score),
Score ="Not Available" ,-1,
true,Score )
AS [Rate],

-1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
State AS BenchmarkID

                        FROM [dbo_vwHQI_STATE_IMG_AVG] 
                        WHERE [Measure Code] IN ('OP_8', 'OP_10', 'OP_11', 'OP_13', 'OP_14')


--Importing National Imaging--
SELECT 
6 AS [ConditionCode],
REPLACE ( [Measure Code],"_","-")  AS MeasureCode, 
0 AS CategoryCode,
switch( 
(not (Score ="Not Available")) ,  val(Score),
Score ="Not Available" ,-1,
true,Score )
AS [Rate],

-1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
"US" AS BenchmarkID
                        FROM [dbo_vwHQI_US_NATIONAL_IMG_AVG] 
                        WHERE [Measure Code] IN ('OP_8', 'OP_10', 'OP_11', 'OP_13', 'OP_14')

--Importing Hospital Healthcare-Associated Infections < HAI-1 >--
SELECT left(prvdr_id,6) as CMSProviderID ,
0 AS [ConditionCode],
"HAI-1" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_1_SIR" and scr="Not Available",-1,
msr_cd="HAI_1_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_1_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_1_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_1_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_1_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
vwHQI_HOSP_HAI
where msr_cd in ("HAI_1_SIR","HAI_1_CI_LOWER","HAI_1_CI_UPPER")
group by  (left(prvdr_id,6) )

--Importing Hospital Healthcare-Associated Infections < HAI-2 >--
SELECT left(prvdr_id,6) as CMSProviderID ,
0 AS [ConditionCode],
"HAI-2" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_2_SIR" and scr="Not Available",-1,
msr_cd="HAI_2_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_2_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_2_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_2_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_2_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
vwHQI_HOSP_HAI
where msr_cd in ("HAI_2_SIR","HAI_2_CI_LOWER","HAI_2_CI_UPPER")
group by  (left(prvdr_id,6) )

--Importing Hospital Healthcare-Associated Infections < HAI-5 >--
SELECT left(prvdr_id,6) as CMSProviderID ,
0 AS [ConditionCode],
"HAI-5" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_5_SIR" and scr="Not Available",-1,
msr_cd="HAI_5_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_5_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_5_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_5_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_5_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
vwHQI_HOSP_HAI
where msr_cd in ("HAI_5_SIR","HAI_5_CI_LOWER","HAI_5_CI_UPPER")
group by  (left(prvdr_id,6) )


--Importing Hospital Healthcare-Associated Infections < HAI-6 >--
SELECT left(prvdr_id,6) as CMSProviderID ,
0 AS [ConditionCode],
"HAI-6" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_6_SIR" and scr="Not Available",-1,
msr_cd="HAI_6_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_6_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_6_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_6_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_6_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
vwHQI_HOSP_HAI
where msr_cd in ("HAI_6_SIR","HAI_6_CI_LOWER","HAI_6_CI_UPPER")
group by  (left(prvdr_id,6) )

--Importing State Healthcare-Associated Infections < HAI-1 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
"HAI-1" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_1_SIR" and scr="Not Available",-1,
msr_cd="HAI_1_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_1_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_1_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_1_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_1_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
[vwHQI_HOSP_HAI_STATE]
where msr_cd in ("HAI_1_SIR","HAI_1_CI_LOWER","HAI_1_CI_UPPER")
group by  state


--Importing State Healthcare-Associated Infections < HAI-2 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
"HAI-2" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_2_SIR" and scr="Not Available",-1,
msr_cd="HAI_2_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_2_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_2_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_2_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_2_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
[vwHQI_HOSP_HAI_STATE]
where msr_cd in ("HAI_2_SIR","HAI_2_CI_LOWER","HAI_2_CI_UPPER")
group by  state

--Importing State Healthcare-Associated Infections < HAI-5 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
"HAI-5" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_5_SIR" and scr="Not Available",-1,
msr_cd="HAI_5_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_5_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_5_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_5_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_5_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
[vwHQI_HOSP_HAI_STATE]
where msr_cd in ("HAI_5_SIR","HAI_5_CI_LOWER","HAI_5_CI_UPPER")
group by  state
--Importing State Healthcare-Associated Infections < HAI-6 >--
SELECT state AS BenchmarkID ,
0 AS [ConditionCode],
"HAI-6" AS MeasureCode,
0 AS CategoryCode,
-1 AS Sample ,
sum(
switch(
msr_cd="HAI_6_SIR" and scr="Not Available",-1,
msr_cd="HAI_6_SIR",val(scr),
true,0
)
) as Rate,
sum(
switch(
msr_cd="HAI_6_CI_LOWER" and (scr="Not Available" or scr="-") ,-1,
msr_cd="HAI_6_CI_LOWER",val(scr),
true,0
)
) as [Lower],
sum(
switch(
msr_cd="HAI_6_CI_UPPER" and scr="Not Available",-1,
msr_cd="HAI_6_CI_UPPER",val(scr),
true,0
)
) as [Upper]
from
[vwHQI_HOSP_HAI_STATE]
where msr_cd in ("HAI_6_SIR","HAI_6_CI_LOWER","HAI_6_CI_UPPER")
group by  state
--Importing National Healthcare-Associated Infections--
SELECT 
0 AS ConditionCode,
Replace(LEFT(msr_cd, LEN(msr_cd) - 4),"_","-") AS MeasureCode,
0 as [CategoryCode] ,
 scr AS Rate ,
 -1 AS Sample ,
-1 as [Lower],
-1 as [Upper],
"US" AS BenchmarkID
                        FROM [vwHQI_HOSP_HAI_National] WHERE RIGHT(msr_cd, 4) = '_SIR' 
                        AND msr_cd IN ('HAI_1_SIR', 'HAI_2_SIR', 'HAI_5_SIR', 'HAI_6_SIR')

--Importing Hospital Emergency Discharge--
SELECT prvdr_id AS CMSProviderID, 
Replace(msr_cd,"_","-") AS MeasureCode,

switch( 
scr  ="Not Available" ,-1,
true,scr  )
AS [Rate], 

switch( 
[dbo_vwHQI_HOSP_ED].Sample  ="Not Available" ,-1,
true,[dbo_vwHQI_HOSP_ED].Sample  )
AS [Sample],

-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode] 
                        FROM [dbo_vwHQI_HOSP_ED] 
                        WHERE msr_cd IN ('ED_1b', 'ED_2b', 'OP_18b', 'OP_20', 'OP_21', 'OP_22')


--Importing State Emergency Discharge--
SELECT 
prvdr_id AS BenchmarkID,
Replace(msr_cd,"_","-") AS MeasureCode,
switch( 
scr  ="Not Available" ,-1,
true,scr  )
AS [Rate],
-1 AS [Sample],
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode]  
                                      
FROM [vwHQI_HOSP_ED_State] 
                                      
WHERE msr_cd IN ('ED_1b', 'ED_2b', 'OP_18b', 'OP_20', 'OP_21', 'OP_22')

--Importing National Emergency Discharge--
SELECT 
Replace(msr_cd,"_","-") AS MeasureCode,
 scr AS Rate,
-1 AS [Sample],
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode] ,
"US" AS BenchmarkID  
                        FROM [vwHQI_HOSP_ED_National] 
                        WHERE msr_cd IN ('ED_1b', 'ED_2b', 'OP_18b', 'OP_20', 'OP_21', 'OP_22')

--Importing Hospital Immunizations--
SELECT 
prvdr_id AS CMSProviderID,
Replace(msr_cd,"_","-") AS MeasureCode,

switch( 
scr  ="Not Available" ,-1,
true,scr  )
AS [Rate],
switch( 
[dbo_vwHQI_HOSP_IMM].Sample  ="Not Available" ,-1,
true,[dbo_vwHQI_HOSP_IMM].Sample  )
AS [Sample], 
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode]
                                   
FROM [dbo_vwHQI_HOSP_IMM] 
                                   
WHERE msr_cd IN ('IMM_1a', 'IMM_2')

--Importing State Immunizations--
SELECT 
prvdr_id AS BenchmarkID,
Replace(msr_cd,"_","-") AS MeasureCode,

switch( 
scr  ="Not Available" ,-1,
true,scr  )
AS [Rate],
-1 AS [Sample], 
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode]

FROM [vwHQI_HOSP_IMM_State] 
WHERE msr_cd IN ('IMM_1a', 'IMM_2')

--Importing National Immunizations--
SELECT 
Replace(msr_cd,"_","-") AS MeasureCode,
scr AS Rate ,
-1 AS [Sample], 
-1 as [Lower],
-1 as [Upper] ,
0 AS ConditionCode,
0 as [CategoryCode],
"US" AS BenchmarkID
FROM [vwHQI_HOSP_IMM_National] 

WHERE msr_cd IN ('IMM_1a', 'IMM_2')