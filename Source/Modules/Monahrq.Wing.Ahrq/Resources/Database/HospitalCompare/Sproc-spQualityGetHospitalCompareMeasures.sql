/*
 *      Name:           spQualityGetHospitalCompareMeasures
 *      Version:        1.0
 *      Last Updated:   6/17/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to get all HC measure information for a particular type.
 */

IF EXISTS (
	SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
	WHERE SPECIFIC_SCHEMA = N'dbo' AND SPECIFIC_NAME = N'spQualityGetHospitalCompareMeasures' 
)
	DROP PROCEDURE dbo.spQualityGetHospitalCompareMeasures
GO

CREATE PROCEDURE [dbo].[spQualityGetHospitalCompareMeasures]
	@HospitalCompareDataset IDsTableType READONLY, @Hospitals IDsTableType READONLY, @Measures IDsTableType READONLY, @MeasureType VARCHAR(MAX), @RegionType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

-- Get Measure and HC data.
WITH
    HospitalMeasure AS
    (
		select			Hospitals.ID AS HospitalID
					,	Measures.ID AS MeasureID
		from			Hospitals
			cross join	Measures
		where			Hospitals.IsArchived = 0 
			and			Hospitals.IsDeleted = 0
			and			Hospitals.Id IN (		-- Make sure the hospital is one of the ones passed in.
							select		Id
							from		@Hospitals
						)
			and			Measures.Id in (		-- Make sure measure is one of the ones passed in.
												-- and only select one of them if multiple belong to the same 'MeasureName'
												-- also, prefer the Override vs the original.
							select		Id
							from		(
											select			Id
														,	ROW_NUMBER() OVER (PARTITION BY Name ORDER BY IsOverride DESC) AS RowNum
											from			Measures m
									 		where			m.Id in (select Id from @Measures)
										) t
							where		t.RowNum = 1
			)
		--	AND Measures.Id IN (        -- Make sure measure is one of the ones passed in.
        --        SELECT Id
        --        FROM @Measures
        --    )
        --    AND Measures.Id IN (        -- Get only overridden measures of those that haven't been overriden.
        --        SELECT Id               -- Both the original and overridden IDs are supposed to be passed in.
        --        FROM Measures
        --        WHERE IsOverride = 1 OR
        --            Id IN (
        --                SELECT Id
        --                FROM (
        --                    -- HACK: Sort by IsOverride descending (bit true > bit false) and grab the first row only.
        --                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY Name ORDER BY IsOverride DESC) AS RowNum
        --                    FROM Measures
        --                    ) t
        --                WHERE t.RowNum = 1
        --            )
        --    )   
            and			Measures.MeasureType = @MeasureType
    ),    HospitalTypes AS
    (
        select distinct	ParentTable.Hospital_Id AS HospitalID
					,	HospitalCategoryID =
							STUFF((
									SELECT	','+ CAST(SubTable.Category_Id AS NVARCHAR(MAX))
									FROM	dbo.Hospitals_HospitalCategories SubTable
									WHERE	SubTable.Hospital_Id = ParentTable.Hospital_Id
									FOR XML PATH('') 
								), 1, 1,'')
        from			dbo.Hospitals_HospitalCategories ParentTable
    )

select			-- Hospital fields
				Hospitals.Id AS HospitalID
			,	C.Id AS CountyID
			,	ISNULL([dbo].[fnGetHospitalRegion](Hospitals.Id, @RegionType),-1) AS RegionID
			,	Hospitals.Zip AS ZipCode
			,	HospitalTypes.HospitalCategoryID AS HospitalType
				-- Measure fields
				-- NOTE: ProvidedBenchmark is the override from the front-end
			,	Measures.Id AS MeasureID
			,	Measures.Name AS MeasureName
			,	Measures.MeasureType
			,	Measures.[Source] AS MeasureSource
			,	NationalBenchmark
			,	HigherScoresAreBetter
			,	ScaleBy
			,	ScaleTarget
			,	RiskAdjustedMethod
			,	SuppressionDenominator
			,	SuppressionNumerator
			,	PerformMarginSuppression
			,	UpperBound
			,	LowerBound
			,	ProvidedBenchmark
			,	CalculationMethod
				-- HC fields
			,	ConditionCode
			,	CategoryCode
			,	Rate
			,	[Sample]
			,	[Lower]
			,	[Upper]
			,	BenchmarkID
from			HospitalMeasure
    left join	Hospitals
	left join	dbo.Base_Counties C
					on	C.CountyFIPS = Hospitals.County
					on	HospitalMeasure.HospitalID = Hospitals.Id
					and	Hospitals.IsArchived = 0
					and	Hospitals.IsDeleted = 0
    left join	Measures on HospitalMeasure.MeasureID = Measures.Id
    left join	dbo.Targets_HospitalCompareTargets HC
					on	HC.CMSProviderID = Hospitals.CMSProviderID
					and	HC.MeasureCode = Measures.Name
					and	HC.Dataset_Id in (select Id from @HospitalCompareDataset)
    left join	[dbo].[Wings_Targets] t on t.Id = Measures.Target_id
    left join	HospitalTypes on HospitalMeasure.HospitalID = HospitalTypes.HospitalID
where			t.Name = 'Hospital Compare Data'         -- Make sure the dataset is a HC dataset

END
