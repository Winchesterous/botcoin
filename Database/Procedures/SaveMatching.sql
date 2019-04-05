CREATE PROCEDURE [dbo].[SaveMatching]
	@createdAt			datetime,
	@exchange1			smallint,
	@exchange2			smallint,	
	@profitRatio		float,
	@profit				float,
	@dataAmount			float,
	@askAmount			float,
	@bidAmount			float,
	@askPrice1			money,
	@bidPrice2			money,
	@fees				money,
	@buyUsdAmount		money,
	@sellUsdAmount		money,
	@currencyRate1		money,
	@currencyRate2		money,
	@instrument1		smallint,
	@instrument2		smallint,
	@transCode			[nvarchar](15),
	@order1				[nvarchar](50) = null,
	@order2				[nvarchar](50) = null,
	@failReason1		[nvarchar](2500) = null,
	@failReason2		[nvarchar](2500) = null
AS
SET NOCOUNT ON
BEGIN
	declare @tradeId int, @cryptoAmount float, @amount float, @balance float, @accountId int, 
			@newAccountId int, @accountBalance money, @insName nvarchar(3)

	begin try
		begin transaction Matching;

		insert into dbo.Trade (
 			CreatedAt, ExchangeId1, ExchangeId2, OrderId1, OrderId2, ProfitRatio, Profit,
			Fees, AskAmount, BidAmount, Amount, BidPrice2, AskPrice1, InstrumentId1, 
			InstrumentId2, TransCode
		)
		values (
			@createdAt, @exchange1, @exchange2, @order1, @order2, @profitRatio, @profit, 
			@fees, @askAmount, @bidAmount, @dataAmount, @bidPrice2, @askPrice1, @instrument1, 
			@instrument2, @transCode
		)
		set @tradeId = cast(scope_identity() as int)

		if @transCode = 'Fail2' or @transCode = 'Ok' or @transCode = 'Back'
		begin
			set @cryptoAmount = @dataAmount
			set @amount = -(@buyUsdAmount * @currencyRate1)
			select top 1 @accountId = Id, @accountBalance = Balance from dbo.Account where ExchangeId = @exchange1 order by Id desc
			set @balance = round(@accountBalance + @amount, 4)

			insert into dbo.Account (CreatedAt, ExchangeId, OperationCode, TradeId, CurrencyRate, Amount, Balance) 
			values (@createdAt, @exchange1, 'BUY', @tradeId, @currencyRate1, @amount, @balance)

			set @newAccountId = cast(scope_identity() as int)
			select @insName = Code from Currency where Id = @instrument1

			if @insName = 'BTC'
				exec dbo.SaveBtcAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'BCH'
				exec dbo.SaveBchAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'LTC'
				exec dbo.SaveLtcAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'ETH'
				exec dbo.SaveEthAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'XRP'
				exec dbo.SaveXrpAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'DSH'
				exec dbo.SaveDashAccount @newAccountId, @accountId, @cryptoAmount
		end

		if @transCode = 'Fail1' or @transCode = 'Ok' or @transCode = 'Back'
		begin
			set @cryptoAmount = -@dataAmount
			set @amount = @sellUsdAmount * @currencyRate2
			select top 1 @accountId = Id, @accountBalance = Balance from dbo.Account where ExchangeId = @exchange2 order by Id desc
			set @balance = round(@accountBalance + @amount, 4)

			insert into dbo.Account (CreatedAt, ExchangeId, OperationCode, TradeId, CurrencyRate, Amount, Balance) 
			values (@createdAt, @exchange2, 'SEL', @tradeId, @currencyRate2, @amount, @balance)

			set @newAccountId = cast(scope_identity() as int)
			select @insName = Code from Currency where Id = @instrument2

			if @insName = 'BTC'
				exec dbo.SaveBtcAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'BCH'
				exec dbo.SaveBchAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'LTC'
				exec dbo.SaveLtcAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'ETH'
				exec dbo.SaveEthAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'XRP'
				exec dbo.SaveXrpAccount @newAccountId, @accountId, @cryptoAmount
			if @insName = 'DSH'
				exec dbo.SaveDashAccount @newAccountId, @accountId, @cryptoAmount
		end

		if @failReason1 is not null
			insert into dbo.FailTrade(TradeId, Reason) values(@tradeId, @failReason1)

		if @failReason2 is not null
			insert into dbo.FailTrade(TradeId, Reason) values(@tradeId, @failReason2)

		commit transaction Matching;
	end try
	begin catch
		if (@@TRANCOUNT > 0)
			rollback transaction Matching;
		throw
	end catch
END

GO