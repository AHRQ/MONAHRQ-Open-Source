/*
 *      Name:           fnQualityBarChartPercentage
 *      Version:        1.0
 *      Last Updated:   10/8/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to calculate percentage of bar chart filled.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].fnQualityBarChartPercentage') AND TYPE IN (N'FN', N'IF', N'TF'))
	DROP FUNCTION dbo.fnQualityBarChartPercentage
GO

CREATE FUNCTION [dbo].[fnQualityBarChartPercentage]
    (
        @Min decimal(19, 7), @Max decimal(19, 7), @Value decimal(19, 7)
    )
    RETURNS int
AS
BEGIN

    DECLARE @Num decimal(19, 7)
    DECLARE @Den decimal(19, 7)
    DECLARE @Percent decimal(19, 7)

    SET @Num = @Value - @Min
    Set @Den = @Max - @Min

    IF @Den <> 0 AND @Num IS NOT NULL
        BEGIN
            SET @Percent = @Num / @Den * 100
            IF @Percent < 0
                SET @Percent = 0
            ELSE IF @Percent > 100
                SET @Percent = 100
        END
    ELSE
        RETURN -1

    RETURN ROUND(@Percent, 0)

END
