BEGIN TRY 
	
	IF NOT EXISTS(SELECT * FROM Information_Schema.COLUMNS WHERE Column_NAME ='DischargeQuarter' and Table_Name = 'Targets_TreatAndReleaseTargets')
 		ALTER TABLE [Targets_TreatAndReleaseTargets] ADD  DischargeQuarter int 


		
	;WITH Wings_ElementsTemp (Name,Description,IsRequired,Hints,LongDescription,Ordinal,Type,Scope_id,DependsOn_id, TargetName)
	AS
	(
		SELECT 'DischargeQuarter','Discharge Quarter',1,'DQTRÔDISCHARGEQUARTERÔQUARTER','Quarter of Discharge (1 = Jan-Mar, 2=Apr-Jun, 3=Jul-Sep, 4=Oct-Dec) Discharge Date must be set if Discharge Quarter is not.',14,NULL,NULL,NULL,'ED Treat And Release'
	
	)
	
	MERGE Wings_Elements wtTemp
	USING
	(
		SELECT temp.Name, temp.Description, temp.IsRequired, temp.Hints, temp.LongDescription, temp.Ordinal, temp.Type, temp.Scope_id, temp.DependsOn_id, wt.Id
		FROM Wings_ElementsTemp temp
			INNER JOIN Wings_Targets wt on temp.TargetName = wt.Name
	)
	temp (Name, Description, IsRequired, Hints, LongDescription, Ordinal, Type, Scope_id, DependsOn_id, Target_Id)
	ON Temp.Name =  wtTemp.Name and 
		temp.Description =  wtTemp.Description and 
		temp.IsRequired = wtTemp.IsRequired and 
		temp.Hints = wtTemp.Hints and 
		temp.LongDescription = wtTemp.LongDescription and
		temp.Ordinal = wtTemp.Ordinal and 
		temp.Type = wtTemp.Type and 
		temp.Scope_id =  wtTemp.Scope_id and
		temp.DependsOn_id =  wtTemp.DependsOn_id and  
		temp.Target_Id = wtTemp.Target_Id
	WHEN NOT MATCHED THEN 
		INSERT (Name,Description,IsRequired,Hints,LongDescription,Ordinal,Type,Scope_id,DependsOn_id, Target_Id)
		VALUES(temp.Name, temp.Description, temp.IsRequired, temp.Hints, temp.LongDescription, temp.Ordinal, temp.Type, temp.Scope_id, temp.DependsOn_id, temp.Target_Id);


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


