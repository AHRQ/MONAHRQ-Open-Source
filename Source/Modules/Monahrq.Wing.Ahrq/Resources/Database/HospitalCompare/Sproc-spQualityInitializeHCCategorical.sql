/*
 *      Name:           spQualityInitializeHCCategorical
 *      Version:        1.0
 *      Last Updated:   7/15/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to prep all of the HC Categorical measures. These are the patient experience measures.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityInitializeHCCategorical' 
)
	DROP PROCEDURE dbo.spQualityInitializeHCCategorical
GO

CREATE PROCEDURE [dbo].[spQualityInitializeHCCategorical]
	@ReportID uniqueidentifier,
	@HospitalCompareDataset IDsTableType READONLY,
	@Hospitals IDsTableType READONLY,
	@Measures IDsTableType READONLY,
	@RegionType NVARCHAR(50),
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
    MeasureType nvarchar(MAX) NOT NULL,
    ObservedDenominator int NULL,
    RiskAdjustedRate decimal(19,7) NULL,
    ObservedRate decimal(19,7) NULL,
    ExpectedRate decimal(19,7) NULL,
    Threshold int NULL,

	NatAvgRate decimal(19,7) NULL,
	NatTop10 decimal(19,7) NULL,
	PeerAvgRate decimal(19,7) NULL,
	PeerTop10 decimal(19,7) NULL,
                
    SuppressionDenominator decimal(19,7) NULL,
    SuppressionNumerator decimal(19,7) NULL,
	HigherScoresAreBetter bit NULL,
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
    
    -- HC fields
    ConditionCode int NULL,
    CategoryCode int NULL,              -- NatRating (varchar), Threshold
    Rate decimal(19,7) NULL,                     -- RiskAdjustedRate
    [Sample] int NULL,                  -- ObservedDenominator
    [Lower] decimal(19,7) NULL,                  -- RiskAdjCILow
    [Upper] decimal(19,7) NULL,                  -- RiskAdjCIHigh
    BenchmarkID nvarchar(15) NULL
)


/**********  Put the data into the temporary tables  **********/

-- Get the full measure data we need for this sproc and put it in the temp table.
INSERT INTO #Temp_Quality_Measures
EXEC dbo.spQualityGetHospitalCompareMeasures @HospitalCompareDataset, @Hospitals, @Measures, 'Categorical', @RegionType

-- Pull out the data we need into a working table.
INSERT INTO #Temp_Quality_Data (MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, MeasureName, MeasureType,
    ObservedDenominator, RiskAdjustedRate, Threshold, SuppressionDenominator, SuppressionNumerator, HigherScoresAreBetter)
SELECT MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, MeasureName, MeasureType,
    ISNULL([Sample], -1), ISNULL(Rate, -1), ISNULL(CategoryCode, -1),
    ISNULL(SuppressionDenominator, 10), ISNULL(SuppressionNumerator, 10), HigherScoresAreBetter
FROM #Temp_Quality_Measures


/**********  Get the national and peer average/mean and top 10 values  **********/

-- Get the national average.
UPDATE #Temp_Quality_Data
SET NatAvgRate = HC.Rate
FROM #Temp_Quality_Data
    JOIN dbo.Targets_HospitalCompareTargets HC ON #Temp_Quality_Data.MeasureName = HC.MeasureCode
WHERE HC.BenchmarkID = 'US'
    AND HC.Dataset_id IN (
        SELECT Id
        FROM @HospitalCompareDataset
    );

-- Get the national top 10.
WITH
    NationalPEX AS
    (
        SELECT Measures.Id AS MeasureID, Measures.Name AS MeasureName, HC.Rate
        FROM [dbo].[Wings_Targets] t
            JOIN dbo.Measures ON t.Id = Measures.Target_id
            JOIN dbo.Targets_HospitalCompareTargets HC ON Measures.Name = HC.MeasureCode
        WHERE t.Name = 'Hospital Compare Data'
            AND Measures.MeasureType IN ('Categorical')
            AND Measures.Id IN (
                SELECT Id
                FROM @Measures
            )
            AND HC.CMSProviderID IS NOT NULL    -- Skip the US, States, TOP10, etc.
            AND HC.Dataset_id IN (
                SELECT Id
                FROM @HospitalCompareDataset
            )
    ),
    RatesByMeasure AS
    (
        SELECT MeasureID, MeasureName, Rate, NTILE(10) OVER (PARTITION BY MeasureName ORDER BY Rate DESC) AS Tile10
        FROM NationalPEX
    ),
    -- This gives us 10 equal groups of scores for each Measure.
    Top10 AS
    (
        SELECT MeasureID, MeasureName, Rate, Tile10
        FROM RatesByMeasure
        WHERE Tile10 = 1
    ),
    -- This gets us the group of observations in the group in the top-10 percentile
    SecondBest AS
    (
        -- now we need to reverse rank all theses observations and get the last guy, #1 in reverse order
        -- must distinct since may be more than one ranked as 1 in ties. Less than this you are second-best
        SELECT DISTINCT MeasureID, MeasureName, Rate, RANK() OVER (PARTITION BY MeasureName ORDER BY Rate ASC) AS AddedRank
        FROM Top10
    )

UPDATE #Temp_Quality_Data
SET NatTop10 = SecondBest.Rate
FROM #Temp_Quality_Data
    JOIN SecondBest ON #Temp_Quality_Data.MeasureID = SecondBest.MeasureID
WHERE SecondBest.AddedRank = 1;

-- Get the peer average
UPDATE #Temp_Quality_Data
SET PeerAvgRate = MeanRate
FROM #Temp_Quality_Data
    JOIN (
        SELECT MeasureName, AVG(RiskAdjustedRate) as MeanRate
        FROM #Temp_Quality_Data
        WHERE RiskAdjustedRate <> -1
        GROUP BY MeasureName
    ) AS Means ON #Temp_Quality_Data.MeasureName = Means.MeasureName;

-- Get the peer top 10
WITH
    RatesByMeasure AS
    (
        SELECT  MeasureID, MeasureName, RiskAdjustedRate, NTILE(10) OVER (PARTITION BY MeasureName ORDER BY RiskAdjustedRate DESC) AS Tile10
        FROM #Temp_Quality_Data
    ),
    --   This gives us 10 equal groups of scores for each Measure.
    Top10 AS
    (
        SELECT MeasureID, MeasureName, RiskAdjustedRate, Tile10
        FROM RatesByMeasure
        WHERE Tile10 = 1
    ),
    -- This gets us the group of observations in the group in the top-10 percentile
    SecondBest AS
    (
        -- Now we need to reverse rank all theses observations and get the last guy, #1 in reverse order
        -- Must distinct since may be more than one ranked as 1 in ties. Less than this you are second-best
        SELECT DISTINCT MeasureID, MeasureName, RiskAdjustedRate, RANK() OVER (PARTITION BY MeasureName ORDER BY RiskAdjustedRate ASC) AS AddedRank
        FROM Top10
    )

UPDATE #Temp_Quality_Data
SET PeerTop10 = SecondBest.RiskAdjustedRate
FROM #Temp_Quality_Data
    JOIN SecondBest ON #Temp_Quality_Data.MeasureID = SecondBest.MeasureID
WHERE SecondBest.AddedRank = 1;
    

/**********  Set the ratings for individual measures against the benchmarks  **********/
-- Calculate NatRating and PeerRating for #Temp_Quality_Data / Temp_Quality

UPDATE #Temp_Quality_Data
SET
    NatRating =
        CASE
            WHEN RiskAdjustedRate < 0 THEN 0
            WHEN RiskAdjustedRate >= NatTop10 THEN 1
            WHEN RiskAdjustedRate >= NatAvgRate THEN 2
            ELSE 3
        END,
    PeerRating =
        CASE
            WHEN RiskAdjustedRate < 0 THEN 0
            WHEN RiskAdjustedRate >= PeerTop10 THEN 1
            WHEN RiskAdjustedRate >= PeerAvgRate THEN 2
            ELSE 3 
        END

-- Prep Category Measures for Detail Reporting
-- get the Sometimes/Never answers paired up with Always in observed-rate column
UPDATE #Temp_Quality_Data
SET #Temp_Quality_Data.ObservedRate = HC.Rate
FROM dbo.Targets_HospitalCompareTargets HC
    JOIN dbo.Hospitals Hosp ON HC.CMSProviderID = Hosp.CmsProviderID
WHERE HC.MeasureCode = #Temp_Quality_Data.MeasureName + '-SN-P'
    AND #Temp_Quality_Data.HospitalID = Hosp.Id
    AND #Temp_Quality_Data.MeasureType = 'Categorical'
    AND HC.Dataset_id IN (
        SELECT Id
        FROM @HospitalCompareDataset
    );

-- get the Usually answers paired up with Always in expected-rate column
UPDATE #Temp_Quality_Data
SET #Temp_Quality_Data.ExpectedRate = HC.Rate
FROM dbo.Targets_HospitalCompareTargets HC
    JOIN dbo.Hospitals Hosp ON HC.CMSProviderID = Hosp.CmsProviderID
WHERE HC.MeasureCode = #Temp_Quality_Data.MeasureName + '-U-P'
    AND #Temp_Quality_Data.HospitalID = Hosp.Id
    AND #Temp_Quality_Data.MeasureType = 'Categorical'
    AND HC.Dataset_id IN (
        SELECT Id
        FROM @HospitalCompareDataset
    );


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
SELECT MeasureID, MIN(RiskAdjustedRate), MAX(RiskAdjustedRate), AVG(NatAvgRate), AVG(PeerAvgRate)
FROM #Temp_Quality_Data
WHERE RiskAdjustedRate <> -1
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


/**********  Format the display fields based upon the calculated data  **********/

UPDATE #Temp_Quality_Data
SET NatFilled = dbo.fnQualityBarChartPercentage(#Temp_Quality_Barcharts.MinNatBar, #Temp_Quality_Barcharts.MaxNatBar, RiskAdjustedRate),
    PeerFilled = dbo.fnQualityBarChartPercentage(#Temp_Quality_Barcharts.MinPeerBar, #Temp_Quality_Barcharts.MaxPeerBar, RiskAdjustedRate),
    RateAndCI = CONVERT(VARCHAR, CONVERT(DECIMAL(8,0),RiskAdjustedRate)),
    Col1 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,0),RiskAdjustedRate)),
    Col2 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,0),ExpectedRate)),
    Col3 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,0),ObservedRate)),
    Col5 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,0),ObservedDenominator))
FROM #Temp_Quality_Data
    LEFT JOIN #Temp_Quality_Barcharts ON #Temp_Quality_Data.MeasureID = #Temp_Quality_Barcharts.MeasureID

UPDATE #Temp_Quality_Data
SET Col4 = (
    CASE
        WHEN Threshold = 1 THEN 'Fewer than 100'
        WHEN Threshold = 2 THEN 'Between 100 and 299'
        WHEN Threshold = 3 THEN '300 or more'
        ELSE '-'
    END
)


/**********  Perform suppression on the data  **********/
if (@DoSuppression = 1)
begin
	UPDATE #Temp_Quality_Data
	SET NatRating = 0, NatFilled = -1, PeerRating = 0, PeerFilled = -1, RateAndCI = '-', Col1 = '-'
	WHERE RiskAdjustedRate < 0
			OR RiskAdjustedRate IS NULL

	UPDATE #Temp_Quality_Data
	SET Col2 = '-'
	WHERE ExpectedRate < 0
			OR ExpectedRate IS NULL

	UPDATE #Temp_Quality_Data
	SET Col3 = '-'
	WHERE ObservedRate < 0
			OR ObservedRate IS NULL

	UPDATE #Temp_Quality_Data
	SET Col5 = '-'
	WHERE ObservedDenominator < 0
			OR ObservedDenominator IS NULL
end

/**********  Put the data in the internal temp table into the output table  **********/

INSERT INTO Temp_Quality (
    ReportID, MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, RateAndCI, NatRating, NatFilled, PeerRating, PeerFilled,
    Col1, Col2, Col3, Col4, Col5, Col6, Col7, Col8, Col9, Col10, Name)
SELECT @ReportID, MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, RateAndCI, NatRating, NatFilled, PeerRating, PeerFilled,
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
        SELECT DISTINCT MeasureID, NatAvgRate, NatTop10, PeerAvgRate, PeerTop10
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
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), MeasureStats.NatAvgRate)) AS NatRateAndCI,
    'Nationwide best 10%' AS NatTop10Label,
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), MeasureStats.NatTop10)) AS NatTop10,
    'State Mean' AS PeerLabel,
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), MeasureStats.PeerAvgRate)) AS PeerRateAndCI,
    'Peer best 10%' AS PeerTop10Label,
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), MeasureStats.PeerTop10)) AS PeerTop10,
    Measures.Footnotes AS Footnote,
    '' AS BarHeader,
    dbo.fnQualityScaleByFootnote(Measures.ScaleBy) AS BarFooter,
    -- Columns descriptions
    'Patients responding &quot;Always&quot;' AS ColDesc1,
    'Patients responding &quot;Usually&quot;' AS ColDesc2,
    'Patients responding &quot;Sometimes or Never&quot;' AS ColDesc3,
    'Surveys completed' AS ColDesc4,
    'Response rate' AS ColDesc5,
    '' AS ColDesc6, '' AS ColDesc7, '' AS ColDesc8, '' AS ColDesc9, '' AS ColDesc10,
    -- National rates
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), MeasureStats.NatAvgRate)) AS NatCol1,
    '' AS NatCol2, '' AS NatCol3, '' AS NatCol4, '' AS NatCol5, '' AS NatCol6, '' AS NatCol7, '' AS NatCol8, '' AS NatCol9, '' AS NatCol10,
    -- Peer rates
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,0), MeasureStats.PeerAvgRate)) AS PeerCol1,
    '' AS PeerCol2, '' AS PeerCol3, '' AS PeerCol4, '' AS PeerCol5, '' AS PeerCol6, '' AS PeerCol7, '' AS PeerCol8, '' AS PeerCol9, '' AS PeerCol10,
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
    'Observed (by answer type), Response Rate (collapsed)' AS StatisticsAvailable,
    Measures.MoreInformation AS MoreInformation,
    Measures.Url AS URL,
    Measures.UrlTitle AS URLTitle,
    'http://www.hospitalcompare.hhs.gov/' AS DataSourceURL,
    'CMS Hospital Compare' AS DataSourceURLTitle
FROM dbo.Measures
    JOIN MeasureTopics ON dbo.Measures.Id = MeasureTopics.MeasureID
    JOIN MeasureStats ON dbo.Measures.Id = MeasureStats.MeasureID
WHERE dbo.Measures.Id IN (
    SELECT DISTINCT MeasureID AS Id
    FROM #Temp_Quality_Data
)
END
