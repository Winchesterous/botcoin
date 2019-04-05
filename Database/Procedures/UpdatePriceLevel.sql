DROP PROCEDURE [dbo].[UpdatePriceLevel]
GO

CREATE PROCEDURE [dbo].[UpdatePriceLevel]
	@levelId		nvarchar(10),	
	@price			decimal(18, 2),
	@date			datetime,
	@isLevelUp		bit = 1
AS
SET NOCOUNT ON
BEGIN

	insert into [dbo].[PriceLevelDate](LevelId,LevelDate,Price,IsLevelUp)
	values(@levelId,@date,@price,@isLevelUp)

END
GO