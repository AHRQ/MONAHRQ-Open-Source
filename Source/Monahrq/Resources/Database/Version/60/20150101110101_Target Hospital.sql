-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0
-- Create date: 12-20-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Hospital Compare Targets to the new 
--              MONAHRQ 6.0 database schema.
--				'Hospital Compare Data'
-- =============================================

BEGIN TRY

IF NOT EXISTS (SELECT 1 FROM Wings_Datasets wt
WHERE wt.ContentType_Id= (SELECT id FROM Wings_Targets wt2
                          WHERE wt2.Name='Hospital Compare Data') )
BEGIN

SET IDENTITY_INSERT dbo.Wings_Datasets ON

INSERT INTO dbo.Wings_Datasets
(
	id,
	SummaryData,
	[File],
	[Description],
	DateImported,
	ReportingQuarter,
	ReportingYear,
	DRGMDCMappingStatus,
	DRGMDCMappingStatusMessage,
	IsFinished,
	ContentType_Id
)
SELECT cir.Id, cisr.[Data],cir.[File],cir.[Description],cir.DateImported,cir.ReportingQuarter,
       cir.ReportingYear,cir.DRGMDCMappingStatus,cir.DRGMDCMappingStatusMessage,
       cir.IsFinished, (SELECT TOP(1) wt.Id FROM dbo.Wings_Targets wt
						WHERE LTRIM(RTRIM(wt.Name))=(SELECT LTRIM(RTRIM(ctr.Name))
                               FROM dbo.ContentTypeRecords ctr
                             WHERE id=cir.ContentType_Id))
  FROM dbo.ContentItemRecords cir
LEFT OUTER JOIN dbo.ContentItemSummaryRecords cisr
ON cisr.Id = cir.Summary_Id
INNER JOIN ContentTypeRecords ctr
ON cir.ContentType_Id=ctr.Id
WHERE  ctr.Name in ('Hospital Compare Data');

SET IDENTITY_INSERT dbo.Wings_Datasets OFF

INSERT INTO [dbo].[Wings_DatasetTransactionRecords]
SELECT tr.[Code],
       tr.[Message],
       tr.[Extension],
       tr.[Data],
       tr.ContentItemRecord_Id
FROM   [dbo].[ContentPartTransactionRecords] tr
INNER JOIN dbo.ContentItemRecords cir
ON cir.Id = tr.ContentItemRecord_Id
INNER JOIN ContentTypeRecords ctr
ON cir.ContentType_Id=ctr.Id
WHERE  ctr.Name in ('Hospital Compare Data');

END

/*******************************************************************
 *  Update Dataset_id column of Targets_HospitalCompareTargets table
 *******************************************************************/

UPDATE Targets_HospitalCompareTargets
SET Dataset_Id = ContentItemRecord_Id

/*******************************************
 * Update Dataset_id column of Targets_HospitalCompareFootnotes table
 *******************************************/

UPDATE Targets_HospitalCompareFootnotes
SET Dataset_Id = ContentItemRecord_Id
 

 /*******************************************
 * Bug 4353. Update MeasureCode column of Targets_HospitalCompareTargets table
 *******************************************/

UPDATE [dbo].[Targets_HospitalCompareTargets]
SET MeasureCode = 'SCIP-INF-3'
WHERE MeasureCode = 'SCIP-INF-3a'

UPDATE [dbo].[Targets_HospitalCompareTargets]
SET MeasureCode = 'SCIP-INF-2'
WHERE MeasureCode = 'SCIP-INF-2a'

UPDATE [dbo].[Targets_HospitalCompareTargets]
SET MeasureCode = 'SCIP-INF-1'
WHERE MeasureCode = 'SCIP-INF-1a'


SELECT 1;

END TRY
BEGIN CATCH
    DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
END CATCH;