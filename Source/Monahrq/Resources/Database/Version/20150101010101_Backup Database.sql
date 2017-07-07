-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 6.0 Build 1
-- Create date: 12-22-2014
-- Description:	This is Backup database script of the existing database.
--  Before doing the upgrade, it may be required to backup 
--  database as a safe guard
-- =============================================

BEGIN TRY

DECLARE    @BackupDirectory varchar(1000),
           @sql VARCHAR(5000)

-- get the default backup directory

EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\MSSQLServer',N'BackupDirectory',@BackupDirectory OUTPUT ;

select @BackupDirectory

DECLARE @CurDate VARCHAR(20), @time varchar(6)

SET @time = REPLACE(CONVERT(TIME, CURRENT_TIMESTAMP),':','')
SELECT @CurDate = CONVERT(VARCHAR(10), GETDATE(), 112)

set @sql='BACKUP DATABASE [@@DESTINATION@@] TO DISK = '''+@BackupDirectory+'\@@DESTINATION@@'+@CurDate +'_'+ @time +'.bak '' WITH INIT'

EXEC(@sql);

SELECT 1;

END TRY
BEGIN CATCH
    DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;
    
    /*************************************************************
     *  Exception is NOT raised as database backup may face exception 
     *  Due to permission or storage
     *************************************************************/

    SELECT  'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           ERROR_SEVERITY(),
            ERROR_STATE();

    
END CATCH;