DELETE FROM Base_AreaPopulationStrats
/*
-- By County / All Combined
INSERT INTO Base_AreaPopulationStrats
SELECT 
	0,
	0 AS CatID,
	0 AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
GROUP BY [Year]

--CASE WHEN (IP.Age < 18) THEN 1						-- AP Age = 1-4 (4 is 15-19)
--		WHEN (IP.Age >= 18 AND IP.Age <= 44) THEN 2		-- AP Age = 5-9
--		WHEN (IP.Age >= 45 AND IP.Age <= 64) THEN 3		-- AP Age = 10-13
--		WHEN (IP.Age >= 65) THEN 4						-- AP Age = 14-18
--		ELSE 0
--(AP.AgeGroup = IP.Age/5+1 OR (IP.Age > 85 AND AP.AgeGroup = 18)) AND

-- By County / By Age (under 18)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	0,
	1 AS CatID,
	1 AS CatVal,
	[Year],
	CAST(SUM(
		CASE
			WHEN AgeGroup = 4 THEN [Population] * .6
			ELSE [Population]
		END
	) AS INT) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup <= 4
GROUP BY [Year]

-- By County / By Age (18-44)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	0,
	1 AS CatID,
	2 AS CatVal,
	[Year],
	CAST(SUM(
		CASE
			WHEN AgeGroup = 4 THEN [Population] * .4
			ELSE [Population]
		END
	) AS INT) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup >= 4 AND AgeGroup <= 9
GROUP BY [Year]

-- By County / By Age (18-44)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	0,
	1 AS CatID,
	3 AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup >= 10 AND AgeGroup <= 13
GROUP BY [Year]

-- By County / By Age (18-44)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	0,
	1 AS CatID,
	4 AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup >= 14
GROUP BY [Year]

-- By County / By Sex
INSERT INTO Base_AreaPopulationStrats
SELECT 
	0,
	2 AS CatID,
	Sex AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
GROUP BY [Year], Sex

-- By County / By Race
INSERT INTO Base_AreaPopulationStrats
SELECT 
	0,
	4 AS CatID,
	Race AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
GROUP BY [Year], Race
*/
-- By County / All Combined
INSERT INTO Base_AreaPopulationStrats
SELECT 
	StateCountyFIPS,
	0 AS CatID,
	0 AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
GROUP BY StateCountyFIPS, [Year]

-- By County / By Age (under 18)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	StateCountyFIPS,
	1 AS CatID,
	1 AS CatVal,
	[Year],
	CAST(SUM(
		CASE
			WHEN AgeGroup = 4 THEN [Population] * .6
			ELSE [Population]
		END
	) AS INT) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup <= 4
GROUP BY StateCountyFIPS, [Year]

-- By County / By Age (18-44)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	StateCountyFIPS,
	1 AS CatID,
	2 AS CatVal,
	[Year],
	CAST(SUM(
		CASE
			WHEN AgeGroup = 4 THEN [Population] * .4
			ELSE [Population]
		END
	) AS INT) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup >= 4 AND AgeGroup <= 9
GROUP BY StateCountyFIPS, [Year]

-- By County / By Age (18-44)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	StateCountyFIPS,
	1 AS CatID,
	3 AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup >= 10 AND AgeGroup <= 13
GROUP BY StateCountyFIPS, [Year]

-- By County / By Age (18-44)
INSERT INTO Base_AreaPopulationStrats
SELECT 
	StateCountyFIPS,
	1 AS CatID,
	4 AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
WHERE AgeGroup >= 14
GROUP BY StateCountyFIPS, [Year]

-- By County / By Sex
INSERT INTO Base_AreaPopulationStrats
SELECT 
	StateCountyFIPS,
	2 AS CatID,
	Sex AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
GROUP BY StateCountyFIPS, [Year], Sex

-- By County / By Race
INSERT INTO Base_AreaPopulationStrats
SELECT 
	StateCountyFIPS,
	4 AS CatID,
	Race AS CatVal,
	[Year],
	SUM([Population]) AS [Population]
FROM Base_AreaPopulations
GROUP BY StateCountyFIPS, [Year], Race
