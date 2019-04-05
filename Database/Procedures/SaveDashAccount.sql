CREATE PROCEDURE [dbo].[SaveDashAccount]
	@accountId			int,
	@currentAccountId	int,
	@amount				float
AS
SET NOCOUNT ON
BEGIN
	declare @balance float

	select top 1 @balance = DashBalance from dbo.DashAccount where AccountId = @currentAccountId
	set @balance = round(@balance + @amount, 8)

	insert into dbo.DashAccount (AccountId, Amount, DashBalance)
	values (@accountId, @amount, @balance)
END

GO