	declare @sell money, @buy money, @snd money, @profit money, @exchangeId int, @btc float, @count int
	set @exchangeId = 1
	set @profit = 0

	select @count = count(*) + 1 from Exchange 

	while @exchangeId < @count
	begin	
		select @snd = (sum(Amount) / a1.CurrencyRate) from Account a1 where a1.ExchangeId = @exchangeId and 
		a1.OperationCode='SND' or a1.OperationCode='SYN' group by a1.ExchangeId, a1.CurrencyRate

		select @sell = (sum(Amount) / a1.CurrencyRate) from Account a1 where a1.ExchangeId = @exchangeId and 
		a1.OperationCode='SEL' group by a1.ExchangeId, a1.CurrencyRate
		if @sell is null
			set @sell = 0

		select @buy = (sum(Amount) / a1.CurrencyRate) from Account a1 where a1.ExchangeId = @exchangeId and 
		a1.OperationCode='BUY' group by a1.ExchangeId, a1.CurrencyRate
		if @buy is null
			set @buy = 0

		select top 1 @btc = BtcBalance from BtcAccount btc
		join Account a on a.Id = btc.AccountId
		where ExchangeId = @exchangeId order by a.id desc

		set @profit = @sell + @buy + @profit

		if @sell + @buy <> 0
			select Name, 
					@sell SELL, 
					@buy BUY,
					@snd SND, 
					(@sell + @buy) 'SEL - BUY',
					@btc 'BTC',
					(@snd + @sell + @buy) 'Balance' 
			from 
				Exchange where Id = @exchangeId
	
		set @exchangeId = @exchangeId + 1
		set @sell = 0
		set @snd = 0
		set @buy = 0
	end
	--select @profit as USD