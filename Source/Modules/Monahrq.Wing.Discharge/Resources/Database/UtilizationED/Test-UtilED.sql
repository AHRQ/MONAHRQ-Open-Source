-- Script for testing new measure inserts
DELETE FROM Temp_UtilED

-- Get the hospitals list
DECLARE @Hospitals AS IDsTableType

INSERT INTO @Hospitals(ID)
SELECT Id
FROM Hospitals
WHERE State_id = 30 OR State_id = 46;       -- NH and VT

-- Get the IP dataset
DECLARE @IPDataset AS IDsTableType

INSERT INTO @IPDataset(ID)
SELECT MIN(Dataset_id)
FROM Targets_InpatientTargets

--Get the ED dataset
DECLARE @EDDataset AS IDsTableType

INSERT INTO @EDDataset(ID)
SELECT MIN(Dataset_id)
FROM Targets_TreatAndReleaseTargets

-- Test the initialization sproc
EXEC spUtilEDInitializeIPVisits '00000000-0000-0000-0000-000000000000', '2012', @Hospitals, @IPDataset, @EDDataset, 'HealthReferralRegion'
EXEC spUtilEDInitializeEDVisits '00000000-0000-0000-0000-000000000000', '2012', @Hospitals, @IPDataset, @EDDataset, 'HealthReferralRegion'

SELECT * FROM Temp_UtilED_Prep