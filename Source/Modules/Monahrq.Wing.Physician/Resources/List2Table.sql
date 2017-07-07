IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnList2Table]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnList2Table]
GO

CREATE FUNCTION [dbo].[fnList2Table]
(
@List VARCHAR(MAX),
@Delim CHAR
)
RETURNS
@ParsedList TABLE
(
item VARCHAR(MAX)
 )
AS
 BEGIN
 DECLARE @item VARCHAR(MAX), @Pos INT
 SET @List = LTRIM(RTRIM(@List))+ @Delim
SET @Pos = CHARINDEX(@Delim, @List, 1)
WHILE @Pos > 0
BEGIN
 SET @item = LTRIM(RTRIM(LEFT(@List, @Pos - 1)))
IF @item <> ''
BEGIN
 INSERT INTO @ParsedList (item)
VALUES (CAST(@item AS VARCHAR(MAX)))
END
 SET @List = RIGHT(@List, LEN(@List) - @Pos)
SET @Pos = CHARINDEX(@Delim, @List, 1)
END
 RETURN
END

GO


