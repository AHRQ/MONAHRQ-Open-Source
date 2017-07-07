BEGIN TRY 

	
		UPDATE Measures 
		SET Target_Id = wt.Id 
		FROM Measures m
			inner join MeasureWingTargetTemp temp on temp.MeasureName = m.Name 
			inner join Wings_Targets wt on temp.TargetName = wt.Name
		WHERE m.Target_Id  is null

		IF(OBJECT_ID(N'MeasureWingTargetTemp')) IS NOT NULL 
			DROP TABLE MeasureWingTargetTemp


		
		--DELETE SchemaVersions WHERE [FILENAME] = 'CostToChargeRatio-2010'
		--DELETE FROM [Base_CostToCharges]

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