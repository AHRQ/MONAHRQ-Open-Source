/*
 *      Name:           spQualityInitializeQIProvider
 *      Version:        1.0
 *      Last Updated:   10/8/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to prep all of the QI Volume measures.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityInitializeQIProvider' 
)
	DROP PROCEDURE dbo.spQualityInitializeQIProvider
GO

CREATE PROCEDURE [dbo].[spQualityInitializeQIProvider]
	@ReportID uniqueidentifier,
	@QIDataset IDsTableType READONLY,
	@Hospitals IDsTableType READONLY,
	@Measures IDsTableType READONLY,
	@RegionType nvarchar(50),
	@DoSuppression bit = 0
AS
BEGIN
    SET NOCOUNT ON;

/**********  Setup temporary tables  **********/

CREATE TABLE #Temp_Quality_Data
(
    -- Base measure data
    MeasureID int NOT NULL,
    HospitalID nvarchar(12) NULL,
    CountyID int NULL,
    RegionID int NULL,
    ZipCode nvarchar(12) NULL,
    HospitalType nvarchar(MAX) NULL,

    -- Measure display data
    RateAndCI nvarchar(255),
    NatRating nvarchar(20) NULL,
    NatFilled int NULL,
    PeerRating nvarchar(20) NULL,
    PeerFilled int NULL,
    Col1 nvarchar(100) NULL,
    Col2 nvarchar(100) NULL,
    Col3 nvarchar(100) NULL,
    Col4 nvarchar(100) NULL,
    Col5 nvarchar(100) NULL,

    -- Temporary fields for calculation
    MeasureName nvarchar(30) NOT NULL,
    ObservedNumerator int NULL,
    ObservedDenominator int NULL,
    ObservedRate decimal(19,7) NULL,
    ObservedLowerCI decimal(19,7) NULL,
    ObservedUpperCI decimal(19,7) NULL,
    NatBenchmarkRate decimal(19,7) NULL,
    PeerBenchmarkRate decimal(19,7) NULL,
    ScaleBy decimal(19,7) NULL,

    SuppressionDenominator decimal(19,7) NULL,
    SuppressionNumerator decimal(19,7) NULL,
    PerformMarginSuppression bit NULL,
	HigherScoresAreBetter bit NULL
)

CREATE TABLE #Temp_Quality_Measures
(
    HospitalID int NOT NULL,
    CountyID int NULL,
    RegionID int NULL,
    ZipCode nvarchar(12) NULL,
    HospitalType nvarchar(MAX) NULL,
    
    -- Measure fields
    -- NOTE: ProvidedBenchmark is the override from the front-end
    MeasureID int NOT NULL,
    MeasureName nvarchar(255) NOT NULL,
    MeasureType nvarchar(MAX) NULL,
    MeasureSource nvarchar(20) NULL,
    NationalBenchmark decimal(19,7) NULL,
    HigherScoresAreBetter bit NULL,
    ScaleBy decimal(19,7) NULL,
    ScaleTarget nvarchar(20) NULL,
    RiskAdjustedMethod nvarchar(20) NULL,
    SuppressionDenominator decimal(19,7) NULL,
    SuppressionNumerator decimal(19,7) NULL,
    PerformMarginSuppression bit NULL,
    UpperBound decimal(19,7) NULL,
    LowerBound decimal(19,7) NULL,
    ProvidedBenchmark decimal(19,7) NULL,
    CalculationMethod nvarchar(255) NULL,
    
    -- QI fields
	TargetType nvarchar(20) NULL,
	MeasureCode nvarchar(32) NULL,
	Stratification nvarchar(12) NULL,
	CountyFIPS nvarchar(12) NULL,
	ObservedNumerator int NULL,
	ObservedDenominator int NULL,
	ObservedRate decimal(19, 9) NULL,
	ObservedCIHigh decimal(19, 9) NULL,
	ObservedCILow decimal(19, 9) NULL,
	RiskAdjustedRate decimal(19, 9) NULL,
	RiskAdjustedCIHigh decimal(19, 9) NULL,
	RiskAdjustedCILow decimal(19, 9) NULL,
	ExpectedRate decimal(19, 9) NULL,
	StandardErr decimal(19, 9) NULL,
	Threshold int NULL,
	NatBenchmarkRate decimal(19, 7) NULL,
	NatRating nvarchar(30) NULL,
	PeerBenchmarkRate decimal(19, 7) NULL,
	PeerRating nvarchar(20) NULL,
	TotalCost decimal(19, 2) NULL
)


/**********  Put the data into the temporary tables  **********/

-- Get the full measure data we need for this sproc and put it in the temp table.
INSERT INTO #Temp_Quality_Measures
EXEC dbo.spQualityGetQIMeasures @QIDataset, @Hospitals, @Measures, 'QIprovider', @RegionType 

-- Pull out the data we need into a working table.
INSERT INTO #Temp_Quality_Data (MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, MeasureName,
    ObservedNumerator, ObservedDenominator, ObservedRate, ObservedLowerCI, ObservedUpperCI, ScaleBy,
    SuppressionDenominator, SuppressionNumerator, PerformMarginSuppression, HigherScoresAreBetter)
SELECT MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, MeasureName,
    ISNULL(ObservedNumerator, -1), ISNULL(ObservedDenominator, -1), ISNULL(ObservedRate * ScaleBy, -1),
    ISNULL(ObservedCILow * ScaleBy, -1), ISNULL(ObservedCIHigh * ScaleBy, -1), ScaleBy,
    ISNULL(SuppressionDenominator, 10), ISNULL(SuppressionNumerator, 10), PerformMarginSuppression, HigherScoresAreBetter
FROM #Temp_Quality_Measures
WHERE RiskAdjustedMethod <> 'mcmc'


/**********  Get the national and peer average/mean and top 10 values  **********/

-- Update the US benchmark if it wasn't overridden.
UPDATE #Temp_Quality_Data
SET #Temp_Quality_Data.NatBenchmarkRate = Measures.NationalBenchmark * Measures.ScaleBy
FROM #Temp_Quality_Data
    JOIN dbo.Measures ON #Temp_Quality_Data.MeasureName = Measures.Name
WHERE (#Temp_Quality_Data.NatBenchmarkRate <= 0
    OR #Temp_Quality_Data.NatBenchmarkRate IS NULL);

-- Get the peer benchmark
UPDATE #Temp_Quality_Data
SET PeerBenchmarkRate = MeanRate
FROM #Temp_Quality_Data
    JOIN (
        SELECT MeasureName, AVG(CONVERT(DECIMAL(19,7), ObservedRate)) AS MeanRate
        FROM #Temp_Quality_Data 
        WHERE ObservedRate >= 0
        GROUP BY MeasureName
    ) AS Means ON Means.MeasureName = #Temp_Quality_Data.MeasureName;

-- Update with peer benchmark rate if it's overridden in the measures table.
-- NOTE: Checking the RiskAdjsutedRate <> -1 does not matter here.
UPDATE #Temp_Quality_Data
SET PeerBenchmarkRate = Measures.ProvidedBenchmark
FROM dbo.Measures
    JOIN #Temp_Quality_Data ON Measures.Name = #Temp_Quality_Data.MeasureName
WHERE Measures.CalculationMethod = 'Provided';


/**********  Compute Upper and Lower CI **********/
if exists(	select		*
			from		#Temp_Quality_Data tqd
			where		tqd.ObservedLowerCI is not null and tqd.ObservedLowerCI <> -1
				and		tqd.ObservedUpperCI is not null and tqd.ObservedUpperCI <> -1)	-- There are Observed*CI values present; use old calc.
	begin
		/**********  Version < 6.0 **********/
		/**********  Set the ratings for individual measures against the benchmarks  **********/
		-- Calculate NatRating and PeerRating for #Temp_Quality_Data / Temp_Quality

		UPDATE #Temp_Quality_Data
		SET NatRating =
				CASE
					WHEN ObservedRate < 0 THEN 0
					WHEN ObservedUpperCI < NatBenchmarkRate THEN 3
					WHEN ObservedLowerCI > NatBenchmarkRate THEN 1
					ELSE 2
				END,
			PeerRating =
				CASE
					WHEN ObservedRate < 0 THEN 0
					WHEN ObservedUpperCI < PeerBenchmarkRate THEN 3 
					WHEN ObservedLowerCI > PeerBenchmarkRate THEN 1
					ELSE 2
				END
		FROM #Temp_Quality_Data
		WHERE HigherScoresAreBetter = 1;

		UPDATE #Temp_Quality_Data
		SET NatRating =
				CASE
					WHEN ObservedRate < 0 THEN 0
					WHEN ObservedUpperCI < NatBenchmarkRate THEN 1 
					WHEN ObservedLowerCI > NatBenchmarkRate THEN 3
					ELSE 2
				END,
			PeerRating =
				CASE
					WHEN ObservedRate < 0 THEN 0
					WHEN ObservedUpperCI < PeerBenchmarkRate THEN 1 
					WHEN ObservedLowerCI > PeerBenchmarkRate THEN 3
					ELSE 2
				END
		FROM #Temp_Quality_Data
		WHERE HigherScoresAreBetter = 0;
	end
else
	begin
		/**********  Version >= 6.0 **********/
		/**********  - Observed*CI values are gone, use new calc that will formulate the Observed*CI Bounds *********/	
		/**********  - For now, just hide everything in the row. **********/
		--(
				WITH
					RatesByMeasure AS
					(
						SELECT	MeasureID, MeasureName, ObservedRate, NTILE(10) OVER (PARTITION BY MeasureName ORDER BY ObservedRate DESC) AS Tile10
						FROM	#Temp_Quality_Data
						WHERE	ObservedRate <> -1 AND HigherScoresAreBetter = 1
					),
					--   This gives us 10 equal groups of scores for each Measure.
					Top10 AS
					(
						SELECT	MeasureID, MeasureName, ObservedRate, Tile10
						FROM	RatesByMeasure
						WHERE	Tile10 = 1
					),
					-- This gets us the group of observations in the group in the top-10 percentile
					SecondBest AS
					(
						-- now we need to reverse rank all theses observations and get the last guy, #1 in reverse order
						-- must distinct since may be more than one ranked as 1 in ties. Less than this you are second-best
						SELECT DISTINCT MeasureID, MeasureName, ObservedRate, RANK() OVER (PARTITION BY MeasureName ORDER BY ObservedRate ASC) AS AddedRank
						FROM			Top10
					)

				UPDATE		#Temp_Quality_Data
				SET			ObservedUpperCI = SecondBest.ObservedRate
				FROM		#Temp_Quality_Data
					JOIN	SecondBest ON #Temp_Quality_Data.MeasureID = SecondBest.MeasureID
				WHERE		SecondBest.AddedRank = 1;
		--)
		--(
				-- Now do the inverted measures
				WITH 
					RatesByMeasure AS
					(
						SELECT	MeasureID, MeasureName, ObservedRate, NTILE(10) OVER (PARTITION BY MeasureName ORDER BY ObservedRate ASC) AS Tile10
						FROM	#Temp_Quality_Data
						WHERE	ObservedRate <> -1 AND HigherScoresAreBetter = 0
					),
					--   This gives us 10 equal groups of scores for each Measure.
					Best10 AS
					(
						SELECT	MeasureID, MeasureName, ObservedRate, Tile10
						FROM	RatesByMeasure
						WHERE	Tile10 = 1
					),
					-- This gets us the group of observations in the group in the top-10 percentile
					SecondBest AS
					(
						-- Now we need to reverse rank all theses observations and get the last guy, #1 in reverse order
						-- Must distinct since may be more than one ranked as 1 in ties. Less than this you are second-best
						SELECT DISTINCT MeasureID, MeasureName, ObservedRate, RANK() OVER (PARTITION BY MeasureName ORDER BY ObservedRate DESC) AS AddedRank
						FROM			Best10
					)

				UPDATE		#Temp_Quality_Data
				SET			ObservedUpperCI = SecondBest.ObservedRate
				FROM		#Temp_Quality_Data
					JOIN	SecondBest ON #Temp_Quality_Data.MeasureID = SecondBest.MeasureID
				WHERE		SecondBest.AddedRank = 1;
		--)

		-- Get the Peer Average Rate
		UPDATE		#Temp_Quality_Data
		SET			ObservedLowerCI = MeanRate
		FROM		#Temp_Quality_Data
			JOIN (
				SELECT		MeasureName, AVG(ObservedRate) as MeanRate
				FROM		#Temp_Quality_Data
				WHERE		ObservedRate <> -1
				GROUP BY	MeasureName
			) AS Means ON Means.MeasureName = #Temp_Quality_Data.MeasureName;

		/**********  Set the ratings for individual measures against the benchmarks  **********/
		-- Calculate NatRating and PeerRating for #Temp_Quality_Data / Temp_Quality

		UPDATE		#Temp_Quality_Data
		SET			NatRating = -3					-- Ignore Value
				,	PeerRating =
						CASE
							WHEN 0 = 0 THEN -3		-- per Vivek/Client: For now, dont use the Calculated values.
							WHEN ObservedRate < 0 THEN 0
							WHEN ObservedRate >= ObservedUpperCI THEN 1
							WHEN ObservedRate >= ObservedLowerCI THEN 2
							ELSE 3
						END
				,	RateAndCI = -1					-- per Vivek: For now, dont show anything for these rows.
				,	ObservedNumerator = -1
				,	ObservedDenominator = -1
				,	ObservedRate = -1
				,	ObservedLowerCI = -1
				,	ObservedUpperCI = -1
				,	NatBenchmarkRate = -1
				,	PeerBenchmarkRate = -1
		FROM		#Temp_Quality_Data
		WHERE		HigherScoresAreBetter = 1;

		UPDATE		#Temp_Quality_Data
		SET			NatRating = -3					-- Ignore Value
				,	PeerRating =
						CASE
							WHEN 0 = 0 THEN -3		-- per Vivek/Client: For now, dont use the Calculated values.
							WHEN ObservedRate < 0 THEN 0
							WHEN ObservedRate < ObservedUpperCI THEN 1
							WHEN ObservedRate < ObservedLowerCI THEN 2
							ELSE 3
						END
				,	RateAndCI = -1					-- per Vivek: For now, dont show anything for these rows.
				,	ObservedNumerator = -1
				,	ObservedDenominator = -1
				,	ObservedRate = -1
				,	ObservedLowerCI = -1
				,	ObservedUpperCI = -1
				,	NatBenchmarkRate = -1
				,	PeerBenchmarkRate = -1
		FROM		#Temp_Quality_Data
		WHERE		HigherScoresAreBetter = 0;
	end


/**********  Figure out the bar chart fill percentage  **********/
-- Calculate NatFilled and PeerFilled for #Temp_Quality_Data / Temp_Quality

CREATE TABLE #Temp_Quality_Barcharts (
    MeasureID int NOT NULL,
    MinRiskAdjRate decimal(19,7) NULL,
    MaxRiskAdjRate decimal(19,7) NULL,
    NatBenchmarkRate decimal(19,7) NULL,
    PeerBenchmarkRate decimal(19,7) NULL,
    MinNatBar decimal(19,7) NULL,
    MaxNatBar decimal(19,7) NULL,
    MinPeerBar decimal(19,7) NULL,
    MaxPeerBar decimal(19,7) NULL
)
            
INSERT INTO #Temp_Quality_Barcharts (MeasureID, MinRiskAdjRate, MaxRiskAdjRate, NatBenchmarkRate, PeerBenchmarkRate)
SELECT MeasureID, MIN(ObservedRate), MAX(ObservedRate), AVG(NatBenchmarkRate), AVG(PeerBenchmarkRate)
FROM #Temp_Quality_Data
WHERE ObservedRate <> -1
GROUP BY MeasureID

UPDATE #Temp_Quality_Barcharts
SET MinNatBar = (NatBenchmarkRate - (SELECT MAX(Boundary)
                                                FROM (VALUES (ABS(NatBenchmarkRate - MinRiskAdjRate)), 
                                                            (ABS(MaxRiskAdjRate - NatBenchmarkRate))
                                                ) AS AllBoundaries (Boundary))),
    MaxNatBar = (NatBenchmarkRate + (SELECT MAX(Boundary)
                                                FROM (VALUES (ABS(NatBenchmarkRate - MinRiskAdjRate)), 
                                                            (ABS(MaxRiskAdjRate - NatBenchmarkRate))
                                                ) AS AllBoundaries (Boundary))),
    MinPeerBar = (PeerBenchmarkRate - (SELECT MAX(Boundary)
                                                FROM (VALUES (ABS(PeerBenchmarkRate - MinRiskAdjRate)), 
                                                            (ABS(MaxRiskAdjRate - PeerBenchmarkRate))
                                                ) AS AllBoundaries (Boundary))),
    MaxPeerBar = (PeerBenchmarkRate + (SELECT MAX(Boundary)
                                                FROM (VALUES (ABS(PeerBenchmarkRate - MinRiskAdjRate)), 
                                                            (ABS(MaxRiskAdjRate - PeerBenchmarkRate))
                                                ) AS AllBoundaries (Boundary)))

UPDATE #Temp_Quality_Data
SET NatFilled = dbo.fnQualityBarChartPercentage(#Temp_Quality_Barcharts.MinNatBar, #Temp_Quality_Barcharts.MaxNatBar, ObservedRate),
    PeerFilled = dbo.fnQualityBarChartPercentage(#Temp_Quality_Barcharts.MinPeerBar, #Temp_Quality_Barcharts.MaxPeerBar, ObservedRate)
FROM #Temp_Quality_Data
    JOIN #Temp_Quality_Barcharts ON #Temp_Quality_Data.MeasureID = #Temp_Quality_Barcharts.MeasureID


/**********  Format the display fields based upon the calculated data  **********/

UPDATE #Temp_Quality_Data
SET RateAndCI = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4), ObservedRate)) + ' (' +
                CONVERT(VARCHAR, CONVERT(DECIMAL(8,4), ObservedLowerCI)) + ', ' +
                CONVERT(VARCHAR, CONVERT(DECIMAL(8,4), ObservedUpperCI)) + ')',
    Col1 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), ObservedNumerator)),
    Col2 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), ObservedDenominator)),
    Col3 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4), ObservedRate)),
    Col4 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4), ObservedLowerCI)),
    Col5 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4), ObservedUpperCI))
       

/**********  Perform suppression on the data  **********/
if (@DoSuppression = 1)
begin

	-- Denominator suppression
	UPDATE #Temp_Quality_Data
	SET NatRating = 0, NatFilled = -1, PeerRating = 0, PeerFilled = -1,
		RateAndCI = '-', Col1 = '-', Col2 = '-', Col3 = '-', Col4 = '-', Col5 = '-'
	WHERE ObservedDenominator < 0
			OR ObservedDenominator IS NULL
			OR (
				ObservedDenominator > 0
				AND ObservedDenominator <= SuppressionDenominator
			)

	-- Numerator suppression
	UPDATE #Temp_Quality_Data
	SET NatRating = 0, NatFilled = -1, PeerRating = 0, PeerFilled = -1,
		RateAndCI = '-', Col1 = '-', Col3 = '-', Col4 = '-', Col5 = '-'
	WHERE ObservedNumerator < 0
			OR ObservedNumerator IS NULL

	UPDATE #Temp_Quality_Data
	SET NatRating = 0, NatFilled = -1, PeerRating = 0, PeerFilled = -1,
		RateAndCI = 'c', Col1 = 'c', Col3 = 'c', Col4 = 'c', Col5 = 'c'
	WHERE Col1 <> '-'
			AND ObservedNumerator > 0
			AND ObservedNumerator <= SuppressionNumerator

	-- Margin Suppression
	UPDATE #Temp_Quality_Data
	SET NatRating = 0, NatFilled = -1, PeerRating = 0, PeerFilled = -1,
		RateAndCI = 'c', Col1 = 'c', Col3 = 'c', Col4 = 'c', Col5 = 'c'
	WHERE Col1 <> '-'
			AND ObservedNumerator > 0
			AND ObservedDenominator > 0
			AND (ObservedDenominator - ObservedNumerator) <= SuppressionNumerator
			AND PerformMarginSuppression = 1

	-- Observed Rate, Lower CI and Upper CI suppression
	UPDATE #Temp_Quality_Data
	SET Col3 = '-'
	WHERE Col3 <> '-'
		AND Col3 <> 'c'
		AND ObservedRate < 0

	UPDATE #Temp_Quality_Data
	SET Col4 = '-'
	WHERE Col4 <> '-'
		AND Col4 <> 'c'
		AND ObservedLowerCI < 0

	UPDATE #Temp_Quality_Data
	SET Col5 = '-'
	WHERE Col5 <> '-'
		AND Col5 <> 'c'
		AND ObservedUpperCI < 0
end

/**********  Put the data in the internal temp table into the output table  **********/

INSERT INTO Temp_Quality (
    ReportID, MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, RateAndCI,
    NatRating, NatFilled, PeerRating, PeerFilled,
    Col1, Col2, Col3, Col4, Col5, Col6, Col7, Col8, Col9, Col10, Name)
SELECT @ReportID, MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, RateAndCI,
    NatRating, NatFilled, PeerRating, PeerFilled,
    Col1, Col2, Col3, Col4, Col5, '', '', '', '', '', ''
FROM #Temp_Quality_Data;


/**********  Put the measure base data into the output table  **********/

WITH
    MeasureTopics AS (
        SELECT ParentTable.Measure_Id AS MeasureID,
            TopicsID =
                STUFF((
                        SELECT ','+ CAST(SubTable.Topic_Id AS NVARCHAR(MAX))
                        FROM Measures_MeasureTopics SubTable
                        WHERE SubTable.Measure_Id = ParentTable.Measure_Id
                        FOR XML PATH('') 
                    ), 1, 1,'')
        FROM Measures_MeasureTopics ParentTable
    ),
    MeasureStats AS (
        SELECT DISTINCT MeasureID, NatBenchmarkRate, PeerBenchmarkRate, ScaleBy
        FROM #Temp_Quality_Data
    )
INSERT INTO Temp_Quality_Measures(
    ReportID, MeasureID, MeasureName, MeasureSource, MeasureType, HigherScoresAreBetter, HigherScoresAreBetterDescription, TopicsID,
    -- National and peer info
    NatLabel, NatRateAndCI, NatTop10Label, NatTop10, PeerLabel, PeerRateAndCI, PeerTop10Label, PeerTop10,
    Footnote, BarHeader, BarFooter,
    -- Columns descriptions
    ColDesc1, ColDesc2, ColDesc3, ColDesc4, ColDesc5, ColDesc6, ColDesc7, ColDesc8, ColDesc9, ColDesc10,
    -- National rates
    NatCol1, NatCol2, NatCol3, NatCol4, NatCol5, NatCol6, NatCol7, NatCol8, NatCol9, NatCol10,
    -- Peer rates
    PeerCol1, PeerCol2, PeerCol3, PeerCol4, PeerCol5, PeerCol6, PeerCol7, PeerCol8, PeerCol9, PeerCol10,
    -- Extra stuff for help popup
    SelectedTitle, PlainTitle, ClinicalTitle, MeasureDescription,
	SelectedTitleConsumer, PlainTitleConsumer, MeasureDescriptionConsumer,
	Bullets, StatisticsAvailable,
    MoreInformation, Url, UrlTitle, DataSourceURL, DataSourceURLTitle
    )
SELECT DISTINCT
    -- Base measure information
    @ReportID, Measures.Id AS MeasureID, Measures.Name AS MeasureName, Measures.Source AS MeasureSource, Measures.MeasureType,
    Measures.HigherScoresAreBetter,
    '' AS HigherScoresAreBetterDescription,
    MeasureTopics.TopicsID,
    -- National and peer info
    'Nationwide Mean' AS NatLabel,
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),MeasureStats.NatBenchmarkRate)) AS NatRateAndCI,
    '' AS NatTop10Label,
    '' AS NatTop10,
    'State Mean' AS PeerLabel,
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),MeasureStats.PeerBenchmarkRate)) AS PeerRateAndCI,
    '' AS PeerTop10Label,
    '' AS PeerTop10,
    Measures.Footnotes AS Footnote,
    '' AS BarHeader,
    dbo.fnQualityScaleByFootnote(Measures.ScaleBy) AS BarFooter,
    -- Columns descriptions
    'Numerator' AS ColDesc1,
    'Denominator' AS ColDesc2,
    'Observed Rate' AS ColDesc3,
    'Observed Lower-bound CI' AS ColDesc4,
    'Observed Upper-bound CI' AS ColDesc5,
    '' AS ColDesc6, '' AS ColDesc7, '' AS ColDesc8, '' AS ColDesc9, '' AS ColDesc10,
    -- National rates
    '' AS NatCol1, '' AS NatCol2,
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),MeasureStats.NatBenchmarkRate)) AS NatCol3,
    '' AS NatCol4, '' AS NatCol5, '' AS NatCol6, '' AS NatCol7, '' AS NatCol8, '' AS NatCol9, '' AS NatCol10,
    -- Peer rates
    '' AS PeerCol1, '' AS PeerCol2,
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),MeasureStats.PeerBenchmarkRate)) AS PeerCol3,
    '' AS PeerCol4, '' AS PeerCol5, '' AS PeerCol6, '' AS PeerCol7, '' AS PeerCol8, '' AS PeerCol9, '' AS PeerCol10,
    -- Extra stuff for help popup -- Professional
    dbo.fnQualitySelectedMeasureTitle(Measures.PlainTitle, Measures.ClinicalTitle, Measures.SelectedTitle) AS SelectedTitle,
    Measures.PlainTitle,
    Measures.ClinicalTitle,
    Measures.[Description] AS MeasureDescription,
    -- Extra stuff for help popup -- Consumer
    dbo.fnQualitySelectedMeasureTitle(Measures.ConsumerPlainTitle, Measures.ClinicalTitle, Measures.SelectedTitle) AS SelectedTitleConsumer,
    Measures.ConsumerPlainTitle as PlainTitleConsumer,
    Measures.[ConsumerDescription] AS MeasureDescriptionConsumer,
    -- Extra stuff for help popup
    dbo.fnQualityBullets(Measures.MeasureType, Measures.Name, Measures.HigherScoresAreBetter,
        Measures.RiskAdjustedMethod, Measures.NQFEndorsed, Measures.ScaleBy, Measures.RateLabel) AS Bullets,
    'Numerator, Denominator, Observed Rate and CI' AS StatisticsAvailable,
    Measures.MoreInformation AS MoreInformation,
    Measures.Url AS URL,
    Measures.UrlTitle AS URLTitle,
    'http://www.qualityindicators.ahrq.gov/' AS DataSourceURL,
    'AHRQ Quality Indicator' AS DataSourceURLTitle
FROM dbo.Measures
    JOIN MeasureTopics ON dbo.Measures.Id = MeasureTopics.MeasureID
    JOIN MeasureStats ON dbo.Measures.Id = MeasureStats.MeasureID
WHERE dbo.Measures.Id IN (
    SELECT DISTINCT MeasureID AS Id
    FROM #Temp_Quality_Data
)

END
