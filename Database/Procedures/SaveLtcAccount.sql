CREATE PROCEDURE [dbo].[SaveLtcAccount]
	@accountId			int,
	@currentAccountId	int,
	@amount				float
AS
SET NOCOUNT ON
BEGIN
	declare @balance float

	select top 1 @balance = LtcBalance from dbo.LtcAccount where AccountId = @currentAccountId
	set @balance = round(@balance + @amount, 8)

	insert into dbo.LtcAccount (AccountId, Amount, LtcBalance)
	values (@accountId, @amount, @balance)
END

GO