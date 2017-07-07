BEGIN TRY 

	IF NOT EXISTS (SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID ('Targets_InpatientTargets') AND NAME='ICDCodeType') 
	BEGIN
		ALTER TABLE [dbo].[Targets_InpatientTargets] ADD [ICDCodeType] INT NULL;
	END

	IF NOT EXISTS (SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID ('Targets_TreatAndReleaseTargets') AND NAME='ICDCodeType') 
	BEGIN
		ALTER TABLE [dbo].[Targets_TreatAndReleaseTargets] ADD [ICDCodeType] INT NULL;
	END

	-- Physician Target Update
	IF EXISTS (SELECT TOP 1 * FROM [dbo].[Wings_Targets] WHERE UPPER([Name]) = UPPER('Physician Data')) 
	BEGIN
		UPDATE [dbo].[Wings_Targets]
		SET [ClrType] = 'Monahrq.Wing.Physician.Physicians.PhysicianTarget, Monahrq.Wing.Physician, Version=7.0.0000.00000, Culture=neutral, PublicKeyToken=null'
		WHERE UPPER([Name]) = UPPER('Physician Data');
	END 	

	-- Nursing Home Compare Target Update
	IF EXISTS (SELECT TOP 1 * FROM [dbo].[Wings_Targets] WHERE UPPER([Name]) = UPPER('Nursing Home Compare Data')) 
	BEGIN
		UPDATE [dbo].[Wings_Targets]
		SET [ClrType] = 'Monahrq.Wing.NursingHomeCompare.NHC.NursingHomeTarget, Monahrq.Wing.NursingHomeCompare, Version=7.0.0000.00000, Culture=neutral, PublicKeyToken=null'
		WHERE UPPER([Name]) = UPPER('Nursing Home Compare Data');
	END 	

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
