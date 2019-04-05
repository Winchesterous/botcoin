CREATE PROCEDURE [dbo].[SaveBchAccount]
	@accountId			int,
	@currentAccountId	int,
	@amount				float
AS
SET NOCOUNT ON
BEGIN
	declare @balance float

	select top 1 @balance = BchBalance from dbo.BchAccount where AccountId = @currentAccountId
	set @balance = round(@balance + @amount, 8)

	insert into dbo.BchAccount (AccountId, Amount, BchBalance)
	values (@accountId, @amount, @balance)
END

GO