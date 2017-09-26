	select top 1	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'December',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'October',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'August',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'May',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'December',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'October',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'July',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'May',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'January',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'May',
						true,												'unknown'
					) as SchemaVersionMonth
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'2016',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'2016',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'2016',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'2016',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'2015',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'2015',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'2015',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'2015',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'2015',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'2014',
						true,												'unknown'
					) as SchemaVersionYear
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'Schema.Version.4',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'Schema.Version.4',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'Schema.Version.3',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'Schema.Version.2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'Schema.Version.2',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'Schema.Version.2',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'Schema.Version.2',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'Schema.Version.2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'Schema.Version.2',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'Schema.Version.1',
						true,												'unknown'
					) as SchemaVersion

			
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalReadmission
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'Heart Attack or Chest Pain,Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'Heart Attack,Heart Failure,Pneumonia,Hospital-Wide All-Cause',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateReadmission
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE,READM_30_CABG',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE,MORT_30_COPD,READM_30_COPD,READM_30_HIP_KNEE,COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'MORT_30_AMI,READM_30_AMI,MORT_30_HF,READM_30_HF,MORT_30_PN,READM_30_PN,READM_30_HOSP_WIDE',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_NationalReadmission
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalComp
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'COMP_HIP_KNEE',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_NationalComp

				,	switch
					(	
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',

						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',

						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_3B,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_INF_10,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_Hospital
				,	switch
					(	
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_3B,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_State1
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_State2
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'AMI_2,AMI_7a,AMI_8a,AMI_10,HF_1,HF_2,HF_3,OP_1,OP_2,OP_3b,OP_4,OP_5,OP_6,OP_7,PC_01,PN_3B,PN_6,' &
																			'SCIP_CARD_2,SCIP_INF_1,SCIP_INF_2,SCIP_INF_3,SCIP_INF_4,SCIP_INF_9,SCIP_VTE_1,SCIP_VTE_2,VTE_6',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_National1
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'SCIP_INF_10',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,								'unknown'
					) as Measures_National2
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,								'unknown'
					) as Measures_HospitalConsumerAssessment
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateHospitalConsumerAssessment
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN,' &
																			'H_COMP_7_SA',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'H_CLEAN_HSP_A_P,H_COMP_1_A_P,H_COMP_2_A_P,H_COMP_3_A_P,H_COMP_4_A_P,H_COMP_5_A_P,H_QUIET_HSP_A_P,' &
																			'H_CLEAN_HSP_SN_P,H_COMP_1_SN_P,H_COMP_2_SN_P,H_COMP_3_SN_P,H_COMP_4_SN_P,H_COMP_5_SN_P,H_QUIET_HSP_SN_P,' &
																			'H_CLEAN_HSP_U_P,H_COMP_1_U_P,H_COMP_2_U_P,H_COMP_3_U_P,H_COMP_4_U_P,H_COMP_5_U_P,H_QUIET_HSP_U_P,' &
																			'H_COMP_6_Y_P,H_HSP_RATING_0_6,H_HSP_RATING_7_8,H_HSP_RATING_9_10,H_RECMND_DY,H_RECMND_PY,H_RECMND_DN',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_NationalHospitalConsumerAssessment
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalImaging
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateImaging
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'OP_8,OP_10,OP_11,OP_13,OP_14',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_NationalImaging
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalHAI_1
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalHAI_2
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_3_SIR,HAI_3_CI_LOWER,HAI_3_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalHAI_3
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_4_SIR,HAI_4_CI_LOWER,HAI_4_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalHAI_4
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalHAI_5
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalHAI_6
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_1_SIR,HAI_1_CI_LOWER,HAI_1_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateHAI_1
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_2_SIR,HAI_2_CI_LOWER,HAI_2_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateHAI_2
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_5_SIR,HAI_5_CI_LOWER,HAI_5_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateHAI_5
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_6_SIR,HAI_6_CI_LOWER,HAI_6_CI_UPPER',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateHAI_6
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'HAI_1_SIR,HAI_2_SIR,HAI_3_SIR,HAI_4_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'HAI_1_SIR,HAI_2_SIR,HAI_5_SIR,HAI_6_SIR',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_NationalHAI_All
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalEmergencyDischarge
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateEmergencyDischarge
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'ED_1b,ED_2b,OP_18b,OP_20,OP_21,OP_22',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_NationalEmergencyDischarge
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'IMM_1a,IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalImmunizations
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'IMM_1a,IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_StateImmunizations
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'IMM_1a,IMM_2',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_NationalImmunizations
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'OP_25,SM_SS_CHECK',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as Measures_HospitalStructural
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'[HQI_HOSP_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'[HQI_HOSP_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'[HQI_HOSP_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'[HQI_HOSP_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'[HQI_HOSP_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'[HQI_HOSP_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'[HQI_HOSP_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'[HQI_HOSP_ReadmCompDeath]',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as MDB_TABLE_HospitalOutcome
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'[HQI_State_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'[HQI_State_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'[HQI_State_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'[HQI_State_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'[HQI_State_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'[HQI_State_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'[HQI_State_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'[HQI_State_ReadmCompDeath]',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as MDB_TABLE_StateOutcome
				,	switch
					(
						'[@@Table_Schema_Name@@]' alike '%_11_14_2016',		'[HQI_National_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_08_26_2016',		'[HQI_National_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_06_08_2016',		'[HQI_National_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2016',		'[HQI_National_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2015',		'[HQI_National_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_08_06_2015',		'[HQI_National_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_05_28_2015',		'[HQI_National_ReadmDeath]',
						'[@@Table_Schema_Name@@]' alike '%_02_18_2015',		'[HQI_National_ReadmCompDeath]',
						'[@@Table_Schema_Name@@]' alike '%_10_28_2014',		'unused',
						'[@@Table_Schema_Name@@]' alike '%_02_25_2014',		'unused',
						true,												'unknown'
					) as MDB_TABLE_NationalOutcome