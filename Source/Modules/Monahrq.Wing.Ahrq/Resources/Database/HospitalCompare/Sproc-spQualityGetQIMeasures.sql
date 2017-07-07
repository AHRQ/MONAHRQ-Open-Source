/*
 *      Name:           spQualityGetQIMeasures
 *      Version:        1.0
 *      Last Updated:   10/8/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to get all QI measure information for a particular type.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityGetQIMeasures' 
)
	DROP PROCEDURE dbo.spQualityGetQIMeasures
GO

CREATE PROCEDURE [dbo].[spQualityGetQIMeasures]
	@QICompareDataset IDsTableType READONLY, @Hospitals IDsTableType READONLY, @Measures IDsTableType READONLY, @MeasureType VARCHAR(MAX), @RegionType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

-- Get Measure and QI data.
WITH
    HospitalMeasure AS
    (
        SELECT DISTINCT Hospitals.ID AS HospitalID, Measures.ID AS MeasureID
        FROM Hospitals CROSS JOIN Measures
        WHERE Hospitals.IsArchived=0 AND Hospitals.IsDeleted=0 AND Hospitals.Id IN (                                           -- Make sure the hospital is one of the ones passed in.
                SELECT DISTINCT Id
                FROM @Hospitals
            )
            AND Measures.Id IN (                                            -- Make sure measure is one of the ones passed in.
                SELECT Id
                FROM @Measures
            )
            --AND Measures.Id IN (                                            -- Get only overridden measures of those that haven't been overriden.
            --    SELECT Id                                               -- Both the original and overridden IDs are supposed to be passed in.
            --    FROM Measures												 -- We are only passing measures that we need to report on. 
            --    WHERE IsOverride = 1 OR
            --        Id IN (
            --            SELECT Id
            --            FROM (
            --                -- HACK: Sort by IsOverride descending (bit true > bit false) and grab the first row only.
            --                SELECT Id, ROW_NUMBER() OVER (PARTITION BY Name ORDER BY IsOverride DESC) AS RowNum
            --                FROM Measures
            --                ) t
            --            WHERE t.RowNum = 1
            --        )
            --)   
            AND Measures.MeasureType = @MeasureType
    ),
    HospitalTypes AS
    (
        SELECT DISTINCT ParentTable.Hospital_Id AS HospitalID,
            HospitalCategoryID =
                STUFF((
                        SELECT ','+ CAST(SubTable.Category_Id AS NVARCHAR(MAX))
                        FROM Hospitals_HospitalCategories SubTable
                        WHERE SubTable.Hospital_Id = ParentTable.Hospital_Id
                        FOR XML PATH('') 
                    ), 1, 1,'')
        FROM Hospitals_HospitalCategories ParentTable
    )

SELECT  -- Hospital fields
        Hospitals.Id AS HospitalID, C.Id AS CountyID,
        CASE
			WHEN (@RegionType = 'HealthReferralRegion') THEN ISNULL(Hospitals.HealthReferralRegion_Id, -1)
			WHEN (@RegionType = 'HospitalServiceArea') THEN ISNULL(Hospitals.HospitalServiceArea_Id, -1)
			WHEN (@RegionType = 'CustomRegion') THEN ISNULL(Hospitals.CustomRegion_Id, -1)
			ELSE -1
		END AS RegionID,                    
        Hospitals.Zip AS ZipCode,
        ISNULL(HospitalTypes.HospitalCategoryID, '') AS HospitalType,
        -- Measure fields
        -- NOTE: ProvidedBenchmark is the override from the front-end
        Measures.Id AS MeasureID, Measures.Name AS MeasureName, Measures.MeasureType, Measures.[Source] AS MeasureSource,
        NationalBenchmark, HigherScoresAreBetter, ScaleBy, ScaleTarget, RiskAdjustedMethod, SuppressionDenominator, SuppressionNumerator,
        PerformMarginSuppression, UpperBound, LowerBound, ProvidedBenchmark, CalculationMethod,
        -- QI fields
        TargetType, MeasureCode, Stratification, QI.CountyFIPS, ObservedNumerator, ObservedDenominator, 
        ObservedRate, ObservedCIHigh, ObservedCILow, RiskAdjustedRate, RiskAdjustedCIHigh, RiskAdjustedCILow, 
        ExpectedRate, StandardErr, Threshold, NatBenchmarkRate, NatRating, PeerBenchmarkRate, PeerRating, TotalCost
FROM HospitalMeasure
    LEFT JOIN (Hospitals LEFT OUTER JOIN Base_Counties C ON C.CountyFips = Hospitals.County ) ON HospitalMeasure.HospitalID = Hospitals.Id AND Hospitals.IsArchived=0 AND Hospitals.IsDeleted=0 
    LEFT JOIN Measures ON HospitalMeasure.MeasureID = Measures.Id
    LEFT JOIN Targets_AHRQTargets QI ON QI.LocalHospitalID = Hospitals.LocalHospitalId
        AND QI.MeasureCode = Measures.Name
    LEFT JOIN [dbo].[Wings_Targets] t ON t.Id = Measures.Target_id
    LEFT JOIN HospitalTypes ON HospitalMeasure.HospitalID = HospitalTypes.HospitalID
WHERE t.Name LIKE 'AHRQ-QI%'               -- Make sure the dataset is a QI dataset
    AND (
            QI.Dataset_Id IN (                        -- Make sure the QI dataset is one of the ones passed in.
                SELECT Id
                FROM @QICompareDataset
            )
            OR QI.Dataset_Id IS NULL
        )
					
END
