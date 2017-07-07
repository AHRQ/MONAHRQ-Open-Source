 BEGIN TRY	

		IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Websites'AND COLUMN_NAME = 'Audience') 
			BEGIN 
				UPDATE Websites
				SET Audiences = 'Consumers,Professionals'
				FROM Websites
				WHERE Audience = 'AllAudiences'

				UPDATE Websites
				Set Audiences = Audience
				FROM Websites
				WHERE Audience <> 'AllAudiences'
			END 

		IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Websites'AND COLUMN_NAME = 'Audience') 
				ALTER TABLE Websites DROP COLUMN Audience 

		UPDATE Reports
		SET Audiences = REPLACE(Audiences,'AllAudiences','Professionals')
		FROM [Reports]
		WHERE Audiences like '%AllAudiences%'
		
		UPDATE reports 
		SET SourceTemplateXml = CAST(REPLACE(CAST(SourceTemplateXml AS NVARCHAR(MAX)), 'AudienceType="AllAudiences"','AudienceType="Professionals"') AS XML)
		
		UPDATE Hospitals 
		SET employees = null
		WHERE Employees = 0
					
		UPDATE TopicCategories
		SET CategoryType = 'Condition'
		WHERE Name in 
			(	
				'Childbirth', 'Heart attack and chest pain',
				'Heart failure','Heart surgeries and procedures', 
				'Pneumonia', 'Stroke'
			)

		UPDATE websites
		SET DefaultAudience  = 'Professionals' 


			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-01', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-02', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-03', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-04', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-05', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-06', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-07', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-08', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-09', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-10', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-11', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-12', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-13', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-14', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IP-15', N'Inpatient Discharge')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'ED-01', N'ED Treat And Release')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'ED-02', N'ED Treat And Release')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'ED-03', N'ED Treat And Release')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'ED-04', N'ED Treat And Release')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 26', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 27', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 28', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 29', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 01', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 02', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 03', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 05', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 07', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 08', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 09', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 10', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 11', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 12', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 13', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 14', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 15', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 16', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 90', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 91', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PQI 92', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 21', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 22', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 23', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 24', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 25', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 26', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 27', N'AHRQ-QI Area Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 90', N'AHRQ-QI Composite Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 91', N'AHRQ-QI Composite Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 90', N'AHRQ-QI Composite Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 01', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 02', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 04', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 05', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 06', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 07', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 08', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 09', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 11', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 12', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 13', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 14', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 15', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 16', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 17', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 18', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 19', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 20', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 21', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 22', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 23', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 24', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 25', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 30', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 31', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 32', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 33', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 34', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 02', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 03', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 04', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 05', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 06', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 07', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 08', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 09', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 10', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 11', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 12', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 13', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 14', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 15', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 16', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 17', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 18', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PSI 19', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 14_QNTY', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IQI 14_COST', N'AHRQ-QI Provider Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HCUP 01', N'HCUP County Hospital Stays Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HCUP 02', N'HCUP County Hospital Stays Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HAI-3', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HAI-4', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'AMI-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'AMI-7a', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'AMI-8a', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'AMI-10', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'ED-1b', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'ED-2b', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-CLEAN-HSP', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-COMP-1', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-COMP-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-COMP-3', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-COMP-4', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-COMP-5', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-COMP-6', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-HSP-RATING', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-QUIET-HSP', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-RECMND', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HAI-1', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HAI-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HAI-5', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HAI-6', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HF-1', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HF-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'HF-3', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'IMM-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MORT-30-AMI', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MORT-30-HF', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MORT-30-PN', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-3b', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-4', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-5', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-6', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-7', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-8', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-10', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-11', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-13', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-14', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-18b', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-20', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-21', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-22', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PC-01', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'PN-6', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'READM-30-AMI', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'READM-30-HF', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'READM-30-HOSP-WIDE', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'READM-30-PN', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-CARD-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-INF-1', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-INF-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-INF-3', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-INF-4', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-INF-9', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-INF-10', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'SCIP-VTE-2', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'VTE-6', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MORT-30-COPD', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'READM-30-COPD', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'READM-30-HIP-KNEE', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'H-COMP-7-SA', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'OP-25', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'COMP-HIP-KNEE', N'Hospital Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MPCH-0', N'Medicare Provider Charge Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MPCH-1', N'Medicare Provider Charge Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MPCH-2', N'Medicare Provider Charge Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'MPCH-3', N'Medicare Provider Charge Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-OA-01', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-HI-01', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-01', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-HI-02', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-HI-03', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-HI-04', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-HI-05', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-SS-01', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-SS-02', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-SS-03', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-SS-04', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-SS-05', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-01', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-02', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-03', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-04', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-05', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-06', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-07', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-08', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-09', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-10', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-11', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-12', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-QM-LS-13', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-SD-01', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-SD-02', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-SD-03', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-SD-04', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-SD-05', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-SD-06', N'Nursing Home Compare Data')
			INSERT [dbo].[MeasureWingTargetTemp] ([MeasureName], [TargetName]) VALUES (N'NH-SD-07', N'Nursing Home Compare Data')


END TRY	
BEGIN CATCH
		DECLARE @ErrorMessage VARCHAR(5000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
		       @ErrorSeverity = ERROR_SEVERITY(),
		       @ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage,  @ErrorSeverity, @ErrorState); 
END CATCH