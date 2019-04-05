CREATE PROCEDURE [dbo].[SaveEthAccount]
	@accountId			int,
	@currentAccountId	int,
	@amount				float
AS
SET NOCOUNT ON
BEGIN
	declare @balance float

	select top 1 @balance = EthBalance from dbo.EthAccount where AccountId = @currentAccountId
	set @balance = round(@balance + @amount, 8)

	insert into dbo.EthAccount (AccountId, Amount, EthBalance)
	values (@accountId, @amount, @balance)
END

GO