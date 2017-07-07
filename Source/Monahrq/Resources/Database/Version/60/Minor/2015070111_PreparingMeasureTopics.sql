BEGIN TRY

MERGE INTO MeasureTopicTemp target 
USING(Values ('IP-01','Inpatient Hospital Utilization','Utilization'),
('IP-02','Inpatient Hospital Utilization','Utilization'),
('IP-03','Inpatient Hospital Utilization','Utilization'),
('IP-04','Inpatient Hospital Utilization','Utilization'),
('IP-05','Inpatient Hospital Utilization','Utilization'),
('IP-06','Inpatient Hospital Utilization','Utilization'),
('IP-07','Inpatient Hospital Utilization','Utilization'),
('IP-08','Inpatient Hospital Utilization','Utilization'),
('IP-09','Inpatient Hospital Utilization','Utilization'),
('IP-10','Inpatient Hospital Utilization','Utilization'),
('IP-11','Inpatient Hospital Utilization','Utilization'),
('IP-12','Inpatient Hospital Utilization','Utilization'),
('IP-13','Inpatient Hospital Utilization','Utilization'),
('IP-14','Inpatient Hospital Utilization','Utilization'),
('IP-15','Inpatient Hospital Utilization','Utilization'),
('ED-01','ED Utilization','Utilization'),
('ED-02','ED Utilization','Utilization'),
('ED-03','ED Utilization','Utilization'),
('ED-04','ED Utilization','Utilization'),
('IQI 26','Procedure Rates','Avoidable Stays Maps'),
('IQI 27','Procedure Rates','Avoidable Stays Maps'),
('IQI 28','Procedure Rates','Avoidable Stays Maps'),
('IQI 29','Procedure Rates','Avoidable Stays Maps'),
('PQI 01','Diabetes','Avoidable Stays Maps'),
('PQI 02','Other Conditions','Avoidable Stays Maps'),
('PQI 03','Diabetes','Avoidable Stays Maps'),
('PQI 05','Chronic Lung Conditions','Avoidable Stays Maps'),
('PQI 07','Heart Conditions','Avoidable Stays Maps'),
('PQI 08','Heart Conditions','Avoidable Stays Maps'),
('PQI 09','Other Conditions','Avoidable Stays Maps'),
('PQI 10','Other Conditions','Avoidable Stays Maps'),
('PQI 11','Other Conditions','Avoidable Stays Maps'),
('PQI 12','Other Conditions','Avoidable Stays Maps'),
('PQI 13','Heart Conditions','Avoidable Stays Maps'),
('PQI 14','Diabetes','Avoidable Stays Maps'),
('PQI 15','Chronic Lung Conditions','Avoidable Stays Maps'),
('PQI 16','Diabetes','Avoidable Stays Maps'),
('PQI 90','Summary scores','Avoidable Stays Maps'),
('PQI 91','Summary scores','Avoidable Stays Maps'),
('PQI 92','Summary scores','Avoidable Stays Maps'),
('PSI 21','Patient Safety','Avoidable Stays Maps'),
('PSI 22','Patient Safety','Avoidable Stays Maps'),
('PSI 23','Patient Safety','Avoidable Stays Maps'),
('PSI 24','Patient Safety','Avoidable Stays Maps'),
('PSI 25','Patient Safety','Avoidable Stays Maps'),
('PSI 26','Patient Safety','Avoidable Stays Maps'),
('PSI 27','Patient Safety','Avoidable Stays Maps'),
('IQI 90','Deaths','Summary Scores'),
('IQI 91','Deaths','Summary Scores'),
('PSI 90','Patient safety','Summary Scores'),
('IQI 01','Results of care','Other surgeries'),
('IQI 02','Results of care','Other surgeries'),
('IQI 04','Results of care','Other surgeries'),
('IQI 05','Results of care','Heart surgeries and procedures'),
('IQI 06','Results of care','Heart surgeries and procedures'),
('IQI 07','Results of care','Stroke'),
('IQI 08','Results of care','Other surgeries'),
('IQI 08','Other surgeries','Deaths or returns to the hospital'),
('IQI 09','Results of care','Other surgeries'),
('IQI 09','Other surgeries','Deaths or returns to the hospital'),
('IQI 11','Results of care','Other surgeries'),
('IQI 11','Other surgeries','Deaths or returns to the hospital'),
('IQI 12','Results of care','Heart surgeries and procedures'),
('IQI 12','Heart surgeries and procedures','Deaths or returns to the hospital'),
('IQI 13','Results of care','Other surgeries'),
('IQI 13','Other surgeries','Deaths or returns to the hospital'),
('IQI 14','Results of care','Other surgeries'),
('IQI 14','Other surgeries','Deaths or returns to the hospital'),
('IQI 15','Results of care','Heart attack and chest pain'),
('IQI 15','Heart attack and chest pain','Deaths or returns to the hospital'),
('IQI 16','Results of care','Heart failure'),
('IQI 16','Heart failure','Deaths or returns to the hospital'),
('IQI 17','Results of care','Stroke'),
('IQI 17','Stroke','Deaths or returns to the hospital'),
('IQI 18','Patient safety','Deaths or returns to the hospital'),
('IQI 18','Results of care - Deaths','Patient safety'),
('IQI 19','Patient safety','Deaths or returns to the hospital'),
('IQI 19','Results of care - Deaths','Patient safety'),
('IQI 20','Results of care','Pneumonia'),
('IQI 20','Pneumonia','Deaths or returns to the hospital'),
('IQI 21','Practice patterns','Childbirth'),
('IQI 22','Practice patterns','Childbirth'),
('IQI 23','Practice patterns','Other surgeries'),
('IQI 24','Recommended care','Other surgeries'),
('IQI 25','Recommended care','Heart surgeries and procedures'),
('IQI 30','Results of care','Heart surgeries and procedures'),
('IQI 30','Heart surgeries and procedures','Deaths or returns to the hospital'),
('IQI 31','Results of care','Stroke'),
('IQI 32','Results of care','Heart attack and chest pain'),
('IQI 32','Heart attack and chest pain','Deaths or returns to the hospital'),
('IQI 33','Practice patterns','Childbirth'),
('IQI 34','Practice patterns','Childbirth'),
('PSI 02','Patient safety','Deaths or returns to the hospital'),
('PSI 02','Results of care - Deaths','Patient safety'),
('PSI 02','Results of care - Deaths','Nursing care'),
('PSI 03','Results of care - Complications','Nursing care'),
('PSI 04','Surgical patient safety','Deaths or returns to the hospital'),
('PSI 04','Results of care','Surgical patient safety'),
('IQI 14','Hip replacement surgery','Health Care Cost and Quality'),
('PSI 04','Results of care - Deaths','Nursing care'),
('PSI 05','Results of care - Complications','Patient safety'),
('PSI 06','Results of care - Complications','Patient safety'),
('PSI 07','Results of care - Complications','Patient safety'),
('PSI 07','Results of care - Complications','Nursing care'),
('PSI 08','Results of care','Surgical patient safety'),
('PSI 08','Results of care - Complications','Nursing care'),
('PSI 09','Results of care','Surgical patient safety'),
('PSI 10','Results of care','Surgical patient safety'),
('PSI 10','Results of care - Complications','Nursing care'),
('PSI 11','Results of care','Surgical patient safety'),
('PSI 12','Results of care','Surgical patient safety'),
('PSI 12','Results of care - Complications','Nursing care'),
('PSI 13','Results of care','Surgical patient safety'),
('PSI 13','Results of care - Complications','Nursing care'),
('PSI 14','Results of care','Surgical patient safety'),
('PSI 15','Results of care - Complications','Patient safety'),
('PSI 16','Results of care - Complications','Patient safety'),
('PSI 17','Results of care','Childbirth'),
('PSI 18','Results of care','Childbirth'),
('PSI 19','Results of care','Childbirth'),
('IQI 14_QNTY','Hip replacement surgery','Health Care Cost and Quality'),
('IQI 14_COST','Hip replacement surgery','Health Care Cost and Quality'),
('HCUP 01','General Subtopic 1','General Topic 1'),
('HCUP 01','General Subtopic 2','General Topic 1'),
('HCUP 01','General Subtopic 3','General Topic 2'),
('HCUP 01','General Subtopic 4','General Topic 2'),
('HCUP 02','General Subtopic 1','General Topic 1'),
('HCUP 02','General Subtopic 2','General Topic 1'),
('HCUP 02','General Subtopic 3','General Topic 2'),
('HCUP 02','General Subtopic 4','General Topic 2'),
('AMI-2','Recommended care - Inpatient','Heart attack and chest pain'),
('AMI-7a','Recommended care - Inpatient','Heart attack and chest pain'),
('AMI-8a','Recommended care - Inpatient','Heart attack and chest pain'),
('AMI-10','Recommended care - Inpatient','Heart attack and chest pain'),
('ED-1b','Throughput','Emergency department (ED)'),
('ED-2b','Throughput','Emergency department (ED)'),
('H-CLEAN-HSP','Environment','Patient survey results'),
('H-COMP-1','Communication','Patient survey results'),
('H-COMP-2','Communication','Patient survey results'),
('H-COMP-3','Environment','Patient survey results'),
('H-COMP-4','Environment','Patient survey results'),
('H-COMP-5','Communication','Patient survey results'),
('H-COMP-6','Communication','Patient survey results'),
('H-HSP-RATING','Satisfaction overall','Patient survey results'),
('H-QUIET-HSP','Environment','Patient survey results'),
('H-RECMND','Satisfaction overall','Patient survey results'),
('HAI-1','Results of care - Complications','Patient safety'),
('HAI-2','Healthcare-associated','Infections'),
('HAI-5','Healthcare-associated','Infections'),
('HAI-6','Healthcare-associated','Infections'),
('HF-1','Recommended care','Heart failure'),
('HF-2','Recommended care','Heart failure'),
('HF-3','Recommended care','Heart failure'),
('IMM-2','Healthcare-associated','Infections'),
('MORT-30-AMI','Results of care','Heart attack and chest pain'),
('MORT-30-AMI','Heart attack and chest pain','Deaths or returns to the hospital'),
('MORT-30-HF','Results of care','Heart failure'),
('MORT-30-HF','Heart failure','Deaths or returns to the hospital'),
('MORT-30-PN','Results of care','Pneumonia'),
('MORT-30-PN','Pneumonia','Deaths or returns to the hospital'),
('OP-2','Recommended care - Outpatient','Heart attack and chest pain'),
('OP-3b','Recommended care - Outpatient','Heart attack and chest pain'),
('OP-4','Recommended care - Outpatient','Heart attack and chest pain'),
('OP-5','Recommended care - Outpatient','Heart attack and chest pain'),
('OP-6','Recommended care before surgery','Surgical patient safety'),
('OP-7','Recommended care before surgery','Surgical patient safety'),
('OP-8','Practice patterns','Imaging'),
('OP-10','Practice patterns','Imaging'),
('OP-11','Practice patterns','Imaging'),
('OP-13','Practice patterns','Imaging'),
('OP-14','Practice patterns','Imaging'),
('OP-18b','Throughput','Emergency department (ED)'),
('OP-20','Throughput','Emergency department (ED)'),
('OP-21','Throughput','Emergency department (ED)'),
('OP-22','Throughput','Emergency department (ED)'),
('PC-01','Practice patterns','Childbirth'),
('PN-6','Recommended care','Pneumonia'),
('READM-30-HF','Results of care','Heart failure'),
('READM-30-HF','Heart failure','Deaths or returns to the hospital'),
('READM-30-HOSP-WIDE','All Causes','Deaths or returns to the hospital'),
('READM-30-PN','Results of care','Pneumonia'),
('READM-30-PN','Pneumonia','Deaths or returns to the hospital'),
('SCIP-CARD-2','Recommended care before surgery','Surgical patient safety'),
('SCIP-INF-1','Recommended care before surgery','Surgical patient safety'),
('SCIP-INF-2','Recommended care before surgery','Surgical patient safety'),
('SCIP-INF-3','Recommended care after surgery','Surgical patient safety'),
('SCIP-INF-4','Recommended care','Heart surgeries and procedures'),
('SCIP-INF-4','Recommended care after surgery','Surgical patient safety'),
('SCIP-INF-9','Recommended care after surgery','Surgical patient safety'),
('SCIP-INF-10','Recommended care after surgery','Surgical patient safety'),
('SCIP-VTE-2','Recommended care before surgery','Surgical patient safety'),
('VTE-6','Blood Clot','Prevention and Treatment'),
('READM-30-AMI','Results of care','Heart failure'),
('READM-30-AMI','Heart failure','Deaths or returns to the hospital'),
('MORT-30-COPD','Results of care','COPD(Chronic Obstructive Pulmonary Disease)'),
('READM-30-COPD','Results of care','COPD(Chronic Obstructive Pulmonary Disease)'),
('READM-30-HIP-KNEE','Results of care','Hip or knee replacement surgery'),
('HAI-3','Healthcare-associated','Infections'),
('HAI-4','Healthcare-associated','Infections'),
('H-COMP-7-SA','Communication','Patient survey results'),
('COMP-HIP-KNEE','Results of care','Hip or knee replacement surgery'),
('NH-HI-02','Health Inspection','Nursing Home'),
('NH-HI-03','Health Inspection','Nursing Home'),
('NH-HI-04','Health Inspection','Nursing Home'),
('NH-HI-05','Health Inspection','Nursing Home'),
('NH-QM-SS-01','Quality Measures','Nursing Home'),
('NH-QM-SS-02','Quality Measures','Nursing Home'),
('NH-QM-SS-03','Quality Measures','Nursing Home'),
('NH-QM-SS-04','Quality Measures','Nursing Home'),
('NH-QM-SS-05','Quality Measures','Nursing Home'),
('NH-QM-LS-01','Quality Measures','Nursing Home'),
('NH-QM-LS-02','Quality Measures','Nursing Home'),
('NH-QM-LS-03','Quality Measures','Nursing Home'),
('NH-QM-LS-04','Quality Measures','Nursing Home'),
('NH-QM-LS-05','Quality Measures','Nursing Home'),
('NH-QM-LS-06','Quality Measures','Nursing Home'),
('NH-QM-LS-07','Quality Measures','Nursing Home'),
('NH-QM-LS-08','Quality Measures','Nursing Home'),
('NH-QM-LS-09','Quality Measures','Nursing Home'),
('NH-QM-LS-10','Quality Measures','Nursing Home'),
('NH-QM-LS-11','Quality Measures','Nursing Home'),
('NH-QM-LS-12','Quality Measures','Nursing Home'),
('NH-QM-LS-13','Quality Measures','Nursing Home'),
('NH-SD-02','Staffing','Nursing Home'),
('NH-SD-03','Staffing','Nursing Home'),
('NH-SD-04','Staffing','Nursing Home'),
('NH-SD-05','Staffing','Nursing Home'),
('NH-SD-06','Staffing','Nursing Home'),
('NH-SD-07','Staffing','Nursing Home')) 
as source (MeasureName,TopicName,TopicCategoryName) on source.MeasureName = target.MeasureName and target.TopicName = source.TopicName and target.TopicCategoryName = source.TopicCategoryName
WHEN MATCHED THEN 
UPDATE SET target.MeasureName = source.MeasureName,
		   target.TopicName = source.TopicName,
		   target.TopicCategoryName = source.TopicCategoryName 
WHEN NOT MATCHED THEN 
		   INSERT (MeasureName,TopicName,TopicCategoryName) 
		   VALUES(source.MeasureName,source.TopicName,source.TopicCategoryName);


INSERT INTO MeasureTopicTemp(MeasureName,TopicName,TopicCategoryName)
SELECT m.Name as [MeasureName], t.Name as TopicName, tc.Name as [TopicCategoryName]
FROM Measures m 
	inner join Measures_MeasureTopics mt on m.Id = mt.Measure_Id 
	inner join Topics t on t.Id = mt.Topic_Id
	inner join TopicCategories tc on tc.Id = t.TopicCategory_id
WHERE not exists
	 (
		SELECT [MeasureName],  TopicName, [TopicCategoryName]
		FROM MeasureTopicTemp mtt 
		WHERE  mtt.MeasureName = m.Name and mtt.[TopicName] = t.Name and 
		mtt.TopicCategoryName = tc.Name
	)


END TRY
BEGIN CATCH
DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 

END CATCH