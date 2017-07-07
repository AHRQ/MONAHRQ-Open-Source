/*
 *      Name:           fnQualityScaleByFootnote
 *      Version:        1.0
 *      Last Updated:   4/30/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to calculate footnote for scale by.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].fnQualityScaleByFootnote') AND TYPE IN (N'FN', N'IF', N'TF'))
	DROP FUNCTION dbo.fnQualityScaleByFootnote
GO

CREATE FUNCTION [dbo].[fnQualityScaleByFootnote]
    (
        @ScaleBy int
    )
    RETURNS varchar(100)
AS
BEGIN

    DECLARE @ReturnVal varchar(MAX)

    SET @ReturnVal = ''

    IF @ScaleBy = -1
        SET @ReturnVal += 'minutes.'
    ELSE IF @ScaleBy = -2
        SET @ReturnVal += 'ratios of Observed to Expected.'
    ELSE IF @ScaleBy = 1
        SET @ReturnVal += 'counts.'
    ELSE 
        SET @ReturnVal += 'per ' + CONVERT(varchar(20), @ScaleBy) + ' cases.'

    RETURN @ReturnVal

END
