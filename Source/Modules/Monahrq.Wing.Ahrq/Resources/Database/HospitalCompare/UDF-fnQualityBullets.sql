/*
 *      Name:           fnQualityBullets
 *      Version:        1.0
 *      Last Updated:   4/30/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to text for the first bullet.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].fnQualityBullets') AND TYPE IN (N'FN', N'IF', N'TF'))
	DROP FUNCTION dbo.fnQualityBullets
GO

CREATE FUNCTION [dbo].[fnQualityBullets]
    (
        @MeasureType nvarchar(MAX),
        @MeasureName nvarchar(255),
        @HigherScoresAreBetter bit,
        @RiskAdjustedMethod nvarchar(20),
        @NQF bit,
        @ScaleBy int,
        @RateLabel nvarchar(20)
    )
    RETURNS nvarchar(MAX)
AS
BEGIN

    DECLARE @ReturnVal nvarchar(MAX)

    SET @ReturnVal = '<ul>'
    
    -- Bullet 1
    IF @MeasureType = 'Binary'
        SET @ReturnVal += '<li>A good score indicates patients often answered &quot;yes&quot; - higher is better.</li>'
    ELSE IF @MeasureType = 'Categorical'
        SET @ReturnVal += '<li>A good score indicates patients often answered &quot;always&quot; - higher is better.</li>'
    ELSE IF @MeasureType = 'Scale'
        SET @ReturnVal += '<li>A good score indicates patients often rated a hospital as a 9 or a 10 - higher is better.</li>'
    ELSE IF @MeasureType = 'YNM'
        SET @ReturnVal += '<li>A good score indicates patients often answered &quot;yes&quot; - higher is better.</li>'
    ELSE IF @MeasureType = 'QIvolume'
        SET @ReturnVal += '<li>Hospitals are not rated for this measure because there are currently no nationally agreed upon standards.</li>'
    ELSE IF @HigherScoresAreBetter = 1
        SET @ReturnVal += '<li>A higher score is better.</li>'
    ELSE
        SET @ReturnVal += '<li>A lower score is better.</li>'

    -- Bullet 2
    IF LEFT(@MeasureName, 2) = 'OP'
        SET @ReturnVal += '<li>This is an outpatient measure.</li>'

    -- Bullet 3
    IF @MeasureType = 'QIarea'
    BEGIN
        IF LEFT(@MeasureName, 3) = 'PQI'
            SET @ReturnVal += '<li>This is not a measure of hospital quality. Evidence shows these hospital stays are potentially avoidable when patients have access to high quality outpatient care.</li>'
        ELSE IF LEFT(@MeasureName, 3) = 'PSI'
            SET @ReturnVal += '<li>This is a measure of patient safety.  It captures hospital stays caused by potentially avoidable complications.  It also captures hospital stays in which potentially avoidable complications occur.</li>'
        ELSE IF LEFT(@MeasureName, 3) = 'IQI'
            SET @ReturnVal += '<li>This is not a measure of hospital quality. It is a measure of practice patterns in an area. There can be wide variation in practice patterns that might suggest overuse or underuse.</li>'
    END

    -- Bullet 4
    IF @MeasureType = 'QIarea'
        BEGIN
            IF LEFT(@MeasureName, 3) = 'IQI'
                SET @ReturnVal += '<li>Hospital scores presented are risk-standardized ratios summed across all conditions. The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer deaths due to these conditions than other hospitals nationwide with a similar case mix. For example, an overall score of 0.5 means that half as many patients died as expected.</li><li>A score of 1 means that the hospital had the same number of deaths due to these conditions as other hospitals nationwide with a similar case mix.</li><li>A score of more than 1 means the hospital had more deaths due to these conditions than other hospitals nationwide with a similar case mix. For example, an overall score of 2.0 means that twice as many patients died as expected.</li></ul><li>Each individual component of this score takes into account how sick patients were before they went to the hospital (the components are risk-adjusted).</li><li>Ratings include a significance test.</li>'
            ELSE IF LEFT(@MeasureName, 3) = 'PSI'
                SET @ReturnVal += '<li>Hospital scores presented are risk-standardized ratios summed across all conditions. The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer selected patient safety events than other hospitals nationwide with a similar case mix. For example, an overall score of 0.5 means that half as many discharges involved complications as expected.</li><li>A score of 1 means that the hospital had the same number of selected patient safety events as other hospitals nationwide with a similar case mix.</li><li>A score of more than 1 means the hospital had more selected patient safety events than other hospitals nationwide with a similar case mix. For example, an overall score of 2.0 means that twice as many discharges involved complications as expected.</li></ul><li>Each individual component of this score takes into account how sick patients were before they went to the hospital (the components are risk-adjusted).</li><li>Ratings include a significance test.</li>'
        END
    ELSE IF @MeasureType = 'Ratio'
		BEGIN
			IF @MeasureName = 'HAI-1'
				SET @ReturnVal += '<li>Hospital scores presented are risk-standardized infection ratios.The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer CLABSI infections than other hospitals nationwide of similar type and size. For example, an overall score of 0.5 means that half as many patients had CLABSI infections as expected.</li><li>A score of 1 means that the hospital had the same number of CLABSI infections as other hospitals nationwide of similar type and size.</li><li>A score of more than 1 means the hospital had more CLABSI infections than other hospitals nationwide of similar type and size. For example, an overall score of 2.0 means that twice as many patients had CLABSI infections as expected.</li></ul><li>This rating takes into account certain factors such as the type and size of a hospital or ICU (it is risk-adjusted).</li><li>Ratings do not include a significance test.</li>'
			ELSE IF @MeasureName = 'HAI-2'
				SET @ReturnVal += '<li>Hospital scores presented are risk-standardized infection ratios.The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer CAUTI infections than other hospitals nationwide of similar type and size. For example, an overall score of 0.5 means that half as many patients had CAUTI infections as expected.</li><li>A score of 1 means that the hospital had the same number of CAUTI infections as other hospitals nationwide of similar type and size.</li><li>A score of more than 1 means the hospital had more CAUTI infections than other hospitals nationwide of similar type and size. For example, an overall score of 2.0 means that twice as many patients had CAUTI infections as expected.</li></ul><li>This rating takes into account certain factors such as the type and size of a hospital or ICU (it is risk-adjusted).</li><li>Ratings do not include a significance test.</li>'
			ELSE IF @MeasureName = 'HAI-5'
				SET @ReturnVal += '<li>Hospital scores presented are risk-standardized infection ratios.The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer MRSA infections than other hospitals nationwide of similar type and size. For example, an overall score of 0.5 means that half as many patients had MRSA infections as expected.</li><li>A score of 1 means that the hospital had the same number of MRSA infections as other hospitals nationwide of similar type and size.</li><li>A score of more than 1 means the hospital had more MRSA infections than other hospitals nationwide of similar type and size. For example, an overall score of 2.0 means that twice as many patients had MRSA infections as expected.</li></ul><li>This rating takes into account certain factors such as the type and size of a hospital or ICU (it is risk-adjusted).</li><li>Ratings do not include a significance test.</li>'
			ELSE IF @MeasureName = 'HAI-6'
				SET @ReturnVal += '<li>Hospital scores presented are risk-standardized infection ratios.The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer C.diff infections than other hospitals nationwide of similar type and size. For example, an overall score of 0.5 means that half as many patients had C.diff infections as expected.</li><li>A score of 1 means that the hospital had the same number of C.diff infections as other hospitals nationwide of similar type and size.</li><li>A score of more than 1 means the hospital had more C.diff infections than other hospitals nationwide of similar type and size. For example, an overall score of 2.0 means that twice as many patients had C.diff infections as expected.</li></ul><li>This rating takes into account certain factors such as the type and size of a hospital or ICU (it is risk-adjusted).</li><li>Ratings do not include a significance test.</li>'
			ELSE
				SET @ReturnVal += '<li>Hospital scores presented are risk-standardized infection ratios.The expected overall score is 1.0.</li><ul><li>A score of less than 1 means that the hospital had fewer of this infection than other hospitals nationwide of similar type and size. For example, an overall score of 0.5 means that half as many patients had this infection as expected.</li><li>A score of 1 means that the hospital had the same number of this infections as other hospitals nationwide of similar type and size.</li><li>A score of more than 1 means the hospital had more of this infection than other hospitals nationwide of similar type and size. For example, an overall score of 2.0 means that twice as many patients had this infection as expected.</li></ul><li>This rating takes into account certain factors such as the type and size of a hospital or ICU (it is risk-adjusted).</li><li>Ratings do not include a significance test.</li>'
		END
    ELSE
        BEGIN
            IF @RiskAdjustedMethod = 'obsv' OR @RiskAdjustedMethod = 'no'
                SET @ReturnVal += '<li>The rate does not take into account how sick patients were before they went to the hospital (it is not risk-adjusted).</li><li>Ratings do not include a significance test.</li>'
            ELSE IF @RiskAdjustedMethod = 'hai'
                SET @ReturnVal += '<li>This rating takes into account certain factors such as the type and size of a hospital or ICU (it is risk-adjusted).</li><li>Ratings do not include a significance test.</li><li>Ratings are based on surveillance for CLABSI in at least one inpatient location in the hospital for one calendar month.</li>'
            ELSE IF @RiskAdjustedMethod = 'surv'
                SET @ReturnVal += '<li>Ratings do not include a significance test.</li>'
            ELSE
                SET @ReturnVal += '<li>The rate takes into account how sick patients were before they went to the hospital (it is risk-adjusted).</li><li>Ratings include a significance test that makes us more confident the hospital rating is accurate.</li>'
        END

    -- Bullet 5
    IF @NQF = 1
        SET @ReturnVal += '<li>This measure is endorsed by the National Quality Forum, an independent organization that sets standards for health care quality measurement.</li>'

    -- Bullet 6
    IF @ScaleBy = -2
        SET @ReturnVal += '<li>Figures presented are ratios of observed to expected.</li>'
    ELSE IF @ScaleBy = -1
        SET @ReturnVal += '<li>Figures presented are in minutes.</li>'
    ELSE IF @ScaleBy = 1
        SET @ReturnVal += '<li>Figures presented are counts.</li>'
    ELSE IF @ScaleBy > 1 AND @MeasureType = 'QIarea'
        BEGIN
            IF LEN(@RateLabel) <= 3
                SET @ReturnVal += '<li>The number of hospital stays is provided for every ' + REPLACE(CONVERT(varchar,CAST(@ScaleBy AS money),1), '.00','') + ' people who reside in that area (i.e., the population).</li>'
            ELSE
                SET @ReturnVal += '<li>The number of hospital stays is provided for every ' + REPLACE(CONVERT(varchar,CAST(@ScaleBy AS money),1), '.00','') + ' ' + @RateLabel + '.</li>'
        END
    ELSE IF @ScaleBy > 1
        SET @ReturnVal += '<li>Figures presented are events per ' + REPLACE(CONVERT(varchar,CAST(@ScaleBy AS money),1), '.00','') + ' cases.</li>'
    ELSE IF @MeasureType = 'QIcomposite' OR @MeasureType = 'QIvolume'
        SET @ReturnVal += '<li>Numbers in the measure details table are not scaled. These are raw statistics.</li>'


    SET @ReturnVal += '</ul>'

    RETURN @ReturnVal

END
