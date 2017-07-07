/*
 *      Name:           spQualityInitializeQIComposite
 *      Version:        1.0
 *      Last Updated:   6/17/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to prep all of the QI Volume measures.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityInitializeQIComposite' 
)
	DROP PROCEDURE dbo.spQualityInitializeQIComposite
GO

CREATE PROCEDURE [dbo].[spQualityInitializeQIComposite]
	@ReportID uniqueidentifier,
	@QIDataset IDsTableType READONLY,
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

    -- Temporary fields for calculation
    MeasureName nvarchar(30) NOT NULL,
    CompositeRatio decimal(19,7) NULL,
    CompositeRatioLowerCI decimal(19,7) NULL,
    CompositeRatioUpperCI decimal(19,7) NULL,
    NatBenchmarkRate decimal(19,7) NULL,
    PeerBenchmarkRate decimal(19,7) NULL
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
EXEC dbo.spQualityGetQIMeasures @QIDataset, @Hospitals, @Measures, 'QIcomposite', @RegionType 

-- Pull out the data we need into a working table.
INSERT INTO #Temp_Quality_Data (MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, MeasureName, NatBenchmarkRate,
    CompositeRatio, CompositeRatioLowerCI, CompositeRatioUpperCI)
SELECT MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType, MeasureName, NationalBenchmark,
    ISNULL(RiskAdjustedRate, -1), ISNULL(RiskAdjustedCILow, -1), ISNULL(RiskAdjustedCIHigh, -1)
FROM #Temp_Quality_Measures


/**********  Get the national and peer average/mean and top 10 values  **********/

-- Update the US benchmark if it wasn't overridden.
UPDATE #Temp_Quality_Data
SET #Temp_Quality_Data.NatBenchmarkRate = Measures.NationalBenchmark
FROM #Temp_Quality_Data
    JOIN dbo.Measures ON #Temp_Quality_Data.MeasureName = Measures.Name
WHERE (#Temp_Quality_Data.NatBenchmarkRate <= 0
    OR #Temp_Quality_Data.NatBenchmarkRate IS NULL);

-- Get the peer benchmark
UPDATE #Temp_Quality_Data
SET PeerBenchmarkRate = MeanRate
FROM #Temp_Quality_Data
    JOIN (
        SELECT MeasureName, AVG(CompositeRatio) AS MeanRate
        FROM #Temp_Quality_Data 
        WHERE CompositeRatio <> -1
        GROUP BY MeasureName
    ) AS Means ON Means.MeasureName = #Temp_Quality_Data.MeasureName;

-- Update with peer benchmark rate if it's overridden in the measures table.
-- NOTE: Checking the RiskAdjsutedRate <> -1 does not matter here.
UPDATE #Temp_Quality_Data
SET PeerBenchmarkRate = Measures.ProvidedBenchmark
FROM dbo.Measures
    JOIN #Temp_Quality_Data ON Measures.Name = #Temp_Quality_Data.MeasureName
WHERE Measures.CalculationMethod = 'Provided';


/**********  Set the ratings for individual measures against the benchmarks  **********/
-- Calculate NatRating and PeerRating for #Temp_Quality_Data / Temp_Quality

UPDATE #Temp_Quality_Data
SET NatRating =
        CASE
			WHEN CompositeRatio < 0 THEN 0
            WHEN CompositeRatioUpperCI < NatBenchmarkRate THEN 1 
            WHEN CompositeRatioLowerCI > NatBenchmarkRate THEN 3
            ELSE 2
        END,
    PeerRating =
        CASE
			WHEN CompositeRatio < 0 THEN 0
            WHEN CompositeRatioUpperCI < PeerBenchmarkRate THEN 1 
            WHEN CompositeRatioLowerCI > PeerBenchmarkRate THEN 3
            ELSE 2
        END
FROM #Temp_Quality_Data;


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
SELECT MeasureID, MIN(CompositeRatio), MAX(CompositeRatio), AVG(NatBenchmarkRate), AVG(PeerBenchmarkRate)
FROM #Temp_Quality_Data
WHERE CompositeRatio <> -1
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
SET NatFilled = dbo.fnQualityBarChartPercentage(#Temp_Quality_Barcharts.MinNatBar, #Temp_Quality_Barcharts.MaxNatBar, CompositeRatio),
    PeerFilled = dbo.fnQualityBarChartPercentage(#Temp_Quality_Barcharts.MinPeerBar, #Temp_Quality_Barcharts.MaxPeerBar, CompositeRatio)
FROM #Temp_Quality_Data
    JOIN #Temp_Quality_Barcharts ON #Temp_Quality_Data.MeasureID = #Temp_Quality_Barcharts.MeasureID


/**********  Format the display fields based upon the calculated data  **********/

UPDATE #Temp_Quality_Data
SET RateAndCI = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),CompositeRatio)) + ' (' +
                CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),CompositeRatioLowerCI)) + ', ' +
                CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),CompositeRatioUpperCI)) + ')',
    Col1 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),CompositeRatio)),
    Col2 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),CompositeRatioLowerCI)),
    Col3 = CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),CompositeRatioUpperCI))
                

/**********  Perform suppression on the data  **********/
if (@DoSuppression = 1)
begin
	UPDATE #Temp_Quality_Data
	SET NatRating = 0, NatFilled = -1, PeerRating = 0, PeerFilled = -1, RateAndCI = '-', Col1 = '-', Col2 = '-', Col3 = '-'
	WHERE CompositeRatio < 0 OR CompositeRatio IS NULL OR
		CompositeRatioLowerCI < 0 OR CompositeRatioLowerCI IS NULL OR
		CompositeRatioUpperCI < 0 OR CompositeRatioUpperCI IS NULL
end

/**********  Put the data in the internal temp table into the output table  **********/

INSERT INTO Temp_Quality (
    ReportID, MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType,
    RateAndCI, NatRating, NatFilled, PeerRating, PeerFilled,
    Col1, Col2, Col3, Col4, Col5, Col6, Col7, Col8, Col9, Col10, Name)
SELECT @ReportID, MeasureID, HospitalID, CountyID, RegionID, ZipCode, HospitalType,
    RateAndCI, NatRating, NatFilled, PeerRating, PeerFilled,
    Col1, Col2, Col3, '', '', '', '', '', '', '', ''
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
        SELECT DISTINCT MeasureID, NatBenchmarkRate, PeerBenchmarkRate
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
    'Composite Ratio' AS ColDesc1,
    'Composite Lower-bound CI' AS ColDesc2,
    'Composite Upper-bound CI' AS ColDesc3,
    '' AS ColDesc4, '' AS ColDesc5, '' AS ColDesc6, '' AS ColDesc7, '' AS ColDesc8, '' AS ColDesc9, '' AS ColDesc10,
    -- National rates
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),MeasureStats.NatBenchmarkRate)) AS NatCol1,
    '' AS NatCol2, '' AS NatCol3, '' AS NatCol4, '' AS NatCol5, '' AS NatCol6, '' AS NatCol7, '' AS NatCol8, '' AS NatCol9, '' AS NatCol10,
    -- Peer rates
    CONVERT(VARCHAR, CONVERT(DECIMAL(8,4),MeasureStats.PeerBenchmarkRate)) AS PeerCol1,
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
    'Composite Ratio and CI' AS StatisticsAvailable,
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
