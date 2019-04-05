CREATE PROCEDURE [dbo].[CreatePriceLevel]
	@date			datetime,
	@price			decimal(18, 2),
	@isLevelUp		bit = 1,
	@timeFrame		nvarchar(3)='3m'
AS
SET NOCOUNT ON
BEGIN
	declare @date2 datetime, @levelId nvarchar(10)

	set @levelId = 'lvl' + convert(varchar,convert(decimal(8,2),@price))
	set @date2 = dateadd(day,7,@date)

	begin try
		begin transaction
			insert into [dbo].[PriceLevel](IsActual,LevelId,Date2,TimeFrame)
			values(1,@levelId,@date2,@timeFrame)

			insert into [dbo].[PriceLevelDate](LevelId,IsLevelUp,LevelDate,Price)
			values(@levelId,@isLevelUp,@date,@price)
		commit transaction
	end try
	begin catch
		if @@trancount > 0
			rollback transaction;

		declare @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		declare @ErrorSeverity INT = ERROR_SEVERITY();
		declare @ErrorState INT = ERROR_STATE();

		raiserror(@ErrorMessage, @ErrorSeverity, @ErrorState);
	end catch
END
GO