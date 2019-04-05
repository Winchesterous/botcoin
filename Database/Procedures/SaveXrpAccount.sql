CREATE PROCEDURE [dbo].[SaveXrpAccount]
	@accountId			int,
	@currentAccountId	int,
	@amount				float
AS
SET NOCOUNT ON
BEGIN
	declare @balance float

	select top 1 @balance = XrpBalance from dbo.XrpAccount where AccountId = @currentAccountId
	set @balance = round(@balance + @amount, 8)

	insert into dbo.XrpAccount (AccountId, Amount, XrpBalance)
	values (@accountId, @amount, @balance)
END

GO