
CREATE FUNCTION fnGetNewRegionId(@oldRegionId int, @RegionType nvarchar(max), @State VARCHAR(4)) RETURNS int
BEGIN
	DECLARE  @NewRegionId INT
	SELECT  TOP 1 @NewRegionId  = r.Id 
	FROM [dbo].[Regions] r
			Inner JOIN [dbo].[Hospitals_Regions] hr ON 
				(
					r.Name = hr.Name 
					AND r.RegionType = hr.RegionType 
					AND r.IsSourcedFromBaseData = hr.IsSourcedFromBaseData 
				)
	WHERE hr.Id = @oldRegionId AND UPPER(r.RegionType) = UPPER(@RegionType)
	AND r.State = @State

	RETURN @NewRegionId; 
END
