DECLARE @Hospitals AS IDsTableType

INSERT INTO @Hospitals(ID)
SELECT Id
FROM Hospitals
WHERE (State_id = 30 OR State_id = 46)        -- NH and VT
and isarchived=0
and isdeleted=0

DECLARE @IPDataset AS IDsTableType

INSERT INTO @IPDataset(ID)
SELECT MAX(ContentItemRecord_id)
FROM Targets_InpatientTargets

DECLARE @ReportYear varchar(4)

set @ReportYear='2012'



------- meta info -----

select count(*) 
from Targets_InpatientTargets AS IP 
LEFT JOIN Hospitals AS Hosp ON IP.LocalHospitalID = Hosp.LocalHospitalID
where IP.DRG IS NOT NULL AND
                    Hosp.Id IN (
                        SELECT Id
                        FROM @Hospitals
                    ) AND
                    IP.ContentItemRecord_id IN (
                        SELECT Id
                        FROM @IPDataset
                    );

------- DRG -------

            SELECT      
	            Hosp.Id AS HospitalID,
	            Hosp.SELECTedRegion_id AS RegionID,
	            Hosp.County_id AS CountyID,
	            Hosp.Zip AS Zip,
	            '' AS HospitalType,

	            1 AS UtilTypeID,		-- UtilType is DRG
	            IP.DRG AS UtilID,
	
	            IP.DischargeYear AS DischargeYear,
	            CASE WHEN (IP.Age < 18) THEN 1
                        WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2
                        WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3
                        WHEN (IP.Age >= 65) THEN 4
                        ELSE 0
                END AS Age, ISNULL(Race.Id, 0) AS Race, Sex.Id AS Sex, ISNULL(Payer.Id, 0) AS PrimaryPayer,
                IP.LengthOfStay AS LengthOfStay, IP.TotalCharge AS TotalCharge,
	            dbo.fnGetCostToChargeRatio(@ReportYear,Hosp.CMSProviderID) * IP.TotalCharge AS TotalCost
            FROM Targets_InpatientTargets AS IP
                LEFT JOIN Hospitals AS Hosp ON IP.LocalHospitalID = Hosp.LocalHospitalID
                LEFT JOIN Base_Races AS Race ON ISNULL(IP.Race, 'Missing') = Race.Name
                LEFT JOIN Base_Sexes AS Sex ON IP.Sex = Sex.Name
                LEFT JOIN Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 'Missing') = Payer.Name
            WHERE IP.DRG IS NOT NULL AND
                    Hosp.Id IN (
                        SELECT Id
                        FROM @Hospitals
                    ) AND
                    IP.ContentItemRecord_id IN (
                        SELECT Id
                        FROM @IPDataset
                    );
------ MDC    ----------------------
SELECT      
		
		Hosp.Id AS HospitalID,
		ISNULL(Hosp.SelectedRegion_Id, -1) AS RegionID,
		ISNULL(Hosp.County_id, -1) AS CountyID,
		null as Zip,
		null as  HospitalType,
		2 AS UtilTypeID,		-- UtilType is MDC
		case when( IP.MDC=0)then 99 else IP.MDC end AS UtilID,
	
	IP.DischargeYear AS DischargeYear,
		
		CASE WHEN (IP.Age < 18) THEN 1
				WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2
				WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3
				WHEN (IP.Age >= 65) THEN 4
				ELSE 0
		END AS Age,
		ISNULL(Race.Id, 0) AS Race,
		Sex.Id AS Sex,
		ISNULL(Payer.Id, 0) AS PrimaryPayer,
		IP.LengthOfStay AS LengthOfStay,
		IP.TotalCharge AS TotalCharge,
		dbo.fnGetCostToChargeRatio(IP.DischargeYear, Hosp.CMSProviderID) * IP.TotalCharge AS TotalCost
	FROM Targets_InpatientTargets AS IP
		LEFT JOIN Hospitals AS Hosp ON IP.LocalHospitalID = Hosp.LocalHospitalID
		LEFT JOIN Base_Races AS Race ON ISNULL(IP.Race, 'Missing') = Race.Name
		LEFT JOIN Base_Sexes AS Sex ON IP.Sex = Sex.Name
		LEFT JOIN Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 'Missing') = Payer.Name
	WHERE IP.MDC IS NOT NULL AND
			Hosp.Id IN (
				SELECT Id
				FROM @Hospitals
			) AND
			IP.ContentItemRecord_id IN (
				SELECT Id
				FROM @IPDataset
			);
----- ccs ------------------
SELECT      
	
		Hosp.Id AS HospitalID,
		ISNULL(Hosp.SelectedRegion_Id, -1) AS RegionID,
		ISNULL(Hosp.County_id, -1) AS CountyID,
		null as Zip,
		null as  HospitalType,

		3 AS UtilTypeID,		-- UtilType is DXCCS
		DXCCS.DXCCSID AS UtilID,
	
		IP.DischargeYear AS DischargeYear,
		CASE WHEN (IP.Age < 18) THEN 1
				WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2
				WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3
				WHEN (IP.Age >= 65) THEN 4
				ELSE 0
		END AS Age,
		ISNULL(Race.Id, 0) AS Race,
		Sex.Id AS Sex,
		ISNULL(Payer.Id, 0) AS PrimaryPayer,
		IP.LengthOfStay AS LengthOfStay,
		IP.TotalCharge AS TotalCharge,
		dbo.fnGetCostToChargeRatio(IP.DischargeYear, Hosp.CMSProviderID) * IP.TotalCharge AS TotalCost
	FROM Targets_InpatientTargets AS IP
		LEFT JOIN Hospitals AS Hosp ON IP.LocalHospitalID = Hosp.LocalHospitalID
	    LEFT JOIN Base_ICD9toDXCCSCrosswalks AS DXCCS ON IP.PrincipalDiagnosis = DXCCS.ICD9ID
		LEFT JOIN Base_Races AS Race ON ISNULL(IP.Race, 'Missing') = Race.Name
		LEFT JOIN Base_Sexes AS Sex ON IP.Sex = Sex.Name
		LEFT JOIN Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 'Missing') = Payer.Name
	WHERE DXCCS.DXCCSID IS NOT NULL AND
			Hosp.Id IN (
				SELECT Id
				FROM @Hospitals
			) AND
			IP.ContentItemRecord_id IN (
				SELECT Id
				FROM @IPDataset
			);

------ PRCCS-----
SELECT      

		Hosp.Id AS HospitalID,
		ISNULL(Hosp.SelectedRegion_Id, -1) AS RegionID,
		ISNULL(Hosp.County_id, -1) AS CountyID,
				null as Zip,
		null as  HospitalType,

		4 AS UtilTypeID,		-- UtilType is PRCCS
		PRCCS.PRCCSID AS UtilID,
	
		IP.DischargeYear AS DischargeYear,
		CASE WHEN (IP.Age < 18) THEN 1
				WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2
				WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3
				WHEN (IP.Age >= 65) THEN 4
				ELSE 0
		END AS Age,
		ISNULL(Race.Id, 0) AS Race,
		Sex.Id AS Sex,
		ISNULL(Payer.Id, 0) AS PrimaryPayer,
		IP.LengthOfStay AS LengthOfStay,
		IP.TotalCharge AS TotalCharge,
		dbo.fnGetCostToChargeRatio(IP.DischargeYear, Hosp.CMSProviderID) * IP.TotalCharge AS TotalCost
	FROM Targets_InpatientTargets AS IP
		LEFT JOIN Hospitals AS Hosp ON IP.LocalHospitalID = Hosp.LocalHospitalID
	    LEFT JOIN Base_ICD9toPRCCSCrosswalks AS PRCCS ON IP.PrincipalDiagnosis = PRCCS.ICD9ID
		LEFT JOIN Base_Races AS Race ON ISNULL(IP.Race, 'Missing') = Race.Name
		LEFT JOIN Base_Sexes AS Sex ON IP.Sex = Sex.Name
		LEFT JOIN Base_Payers AS Payer ON ISNULL(IP.PrimaryPayer, 'Missing') = Payer.Name
	WHERE PRCCS.PRCCSID IS NOT NULL AND
			Hosp.Id IN (
				SELECT Id
				FROM @Hospitals
			) AND
			IP.ContentItemRecord_id IN (
				SELECT Id
				FROM @IPDataset
			);