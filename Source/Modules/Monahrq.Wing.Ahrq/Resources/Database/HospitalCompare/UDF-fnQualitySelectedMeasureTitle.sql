/*
 *      Name:           fnQualitySelectedMeasureTitle
 *      Version:        1.0
 *      Last Updated:   4/22/14
 *      Used In:        QualityReportGenerator.cs
 *      Description:    Used to get the selected measure.
 */

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].fnQualitySelectedMeasureTitle') AND TYPE IN (N'FN', N'IF', N'TF'))
	DROP FUNCTION dbo.fnQualitySelectedMeasureTitle
GO

CREATE FUNCTION [dbo].[fnQualitySelectedMeasureTitle]
    (
        @PlainTitle varchar(MAX), @ClinicalTitle varchar(MAX), @SelectedTitle varchar(MAX)
    )
    RETURNS varchar(MAX)
AS
BEGIN

    IF @SelectedTitle = 'Clinical'
        RETURN @ClinicalTitle

    RETURN @PlainTitle

END
