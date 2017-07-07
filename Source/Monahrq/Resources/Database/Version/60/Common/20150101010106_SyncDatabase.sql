BEGIN TRY
	IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'fnToProperCase')
		drop function fnToProperCase;
	IF(OBJECT_ID(N'fnToProperCase')) IS NULL
	BEGIN
		declare @sql as nvarchar(max);
		set @sql = 'CREATE FUNCTION [dbo].[fnToProperCase]
		(
			@string NVARCHAR(4000),
			@delimitersex nvarchar(100) = ''''	-- Use <DEFAULT>
		) 
		RETURNS NVARCHAR(4000)
		AS
		BEGIN
		  DECLARE @i INT           -- index
		  DECLARE @l INT           -- input length
		  DECLARE @c NCHAR(1)      -- current char
		  DECLARE @f INT           -- first letter flag (1/0)
		  DECLARE @o VARCHAR(255)  -- output string
		  DECLARE @w VARCHAR(20)   -- characters considered as white space

		  SET @w = ''['' + CHAR(13) + CHAR(10) + CHAR(9) + CHAR(160) + '' '' + @delimitersex + '']''
		  SET @i = 0
		  SET @l = LEN(@string)
		  SET @f = 1
		  SET @o = ''''

		  WHILE @i <= @l
		  BEGIN
			SET @c = SUBSTRING(@string, @i, 1);
			IF (@f = 1)
			BEGIN
			 SET @o = @o + @c;
			 SET @f = 0;
			END
			ELSE
			BEGIN
			 SET @o = @o + LOWER(@c);
			END

			IF @c LIKE @w SET @f = 1;

			SET @i = @i + 1;
		  END

		  RETURN @o;
		END';
		exec(@sql);
	END
END TRY

BEGIN CATCH
DECLARE @ERRORMESSAGE0 VARCHAR(5000);
    DECLARE @ERRORSEVERITY0 INT;
    DECLARE @ERRORSTATE0 INT;

    SELECT @ERRORMESSAGE0 = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY0 = ERROR_SEVERITY(),
           @ERRORSTATE0 = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE0, @ERRORSEVERITY0, @ERRORSTATE0);
END CATCH

BEGIN TRY
	update			bps
	set				bps.Name = dbo.fnToProperCase(bps.Name,'\/')
	from			Base_ProviderSpecialities bps
	where			bps.Name like  '%[\/ ][A-Z][A-Z]%'  Collate Latin1_General_Bin

END TRY

BEGIN CATCH
DECLARE @ERRORMESSAGE VARCHAR(5000);
    DECLARE @ERRORSEVERITY INT;
    DECLARE @ERRORSTATE INT;

    SELECT @ERRORMESSAGE = 'ERROR LINE: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' ERROR: ' + ERROR_MESSAGE(),
           @ERRORSEVERITY = ERROR_SEVERITY(),
           @ERRORSTATE = ERROR_STATE();

    RAISERROR (@ERRORMESSAGE, @ERRORSEVERITY, @ERRORSTATE);
END CATCH