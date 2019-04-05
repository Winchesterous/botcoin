CREATE PROCEDURE [dbo].[SaveBtcAccount]
	@accountId			int,
	@currentAccountId	int,
	@amount				float
AS
SET NOCOUNT ON
BEGIN
	declare @balance float

	select top 1 @balance = BtcBalance from dbo.BtcAccount where AccountId = @currentAccountId
	set @balance = round(@balance + @amount, 8)

	insert into dbo.BtcAccount (AccountId, Amount, BtcBalance)
	values (@accountId, @amount, @balance)
END

GO