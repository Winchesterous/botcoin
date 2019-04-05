CREATE PROCEDURE [dbo].[CreateBreakDownLevel]
	@levelId		nvarchar(10),	
	@date			datetime,
	@isFalseBreak	bit,
	@isLevelUp		bit = 1
AS
SET NOCOUNT ON
BEGIN
	insert into [dbo].[PriceLevelDate](LevelId,IsLevelUp,LevelDate,IsFalseBreakdown)
	values(@levelId,@isLevelUp,@date,@isFalseBreak)
END
GO