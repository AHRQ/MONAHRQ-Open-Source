
IF OBJECT_ID (N'dbo.fnFindCountyID', N'FN') IS NOT NULL
    DROP FUNCTION dbo.fnFindCountyID;
GO
CREATE FUNCTION fnFindCountyID 
(
	-- Add the parameters for the function here
	@CountyFIPS nvarchar(5)
	, @CountySSA nvarchar(3)
	, @CountyName nvarchar(250)
	, @CountyState nvarchar(5)
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @CountyID int

	--Get County ID based on FIPS
	if (@CountyFIPS is not null) Begin
		SELECT top 1 @CountyID = Id
		FROM    Base_Counties
		where CountyFIPS = @CountyFIPS
	End
	if (@CountyID is not null) return @CountyID

	--Get County ID based on SSA
	If (@CountySSA is not null and @CountyState is not null) Begin
		SELECT top 1 @CountyID = Id
		FROM    Base_Counties
		where CountySSA = @CountySSA and [State] = @CountyState
	end
	if (@CountyID is not null) return @CountyID

	--Get County ID based on County Name
	If (@CountyName is not null and Len(@CountyName) > 0 and @CountyState is not null) Begin
		SELECT top 1 @CountyID = Id
		FROM    Base_Counties
		where  [State] = @CountyState and (
		[Name] like  '%' + @CountyName + '%' 
		or
		[Name] like  '%' + Left(@CountyName,Len(@CountyName) -1) + '%' --remove 's' from Prince George's county
		)
	end
	if (@CountyID is not null) return @CountyID

	-- Return the result of the function
	RETURN NULL
	
	--TEST code
	--select top 1 dbo.fnFindCountyID('','160','Prince Georges','MD')
	--from Base_Counties

END
GO