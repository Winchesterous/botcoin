declare @hour int, @x1 money, @x2 money, @time1 datetime, @time2 datetime, @count int=0, @high decimal(18,8), @low decimal(18,8), @open decimal(18,8), @close decimal(18,8)
set @time1='2019-02-24 00:00:00.000'
set @hour=4

while @count<>24/@hour
begin 
	set @time2=dateadd(hour,@hour,@time1)

	select @x1 = sum(Quantity) from [dbo].[190224_TradeBitstamp] where Instrument1='btc' and TradeType='buy' and TradeAt between @time1 and @time2
	select @x2 = sum(Quantity) from [dbo].[190224_TradeBitstamp] where Instrument1='btc' and TradeType='sell' and TradeAt between @time1 and @time2
	select @x2 = sum(Quantity) from [dbo].[190224_TradeBitstamp] where Instrument1='btc' and TradeType='sell' and TradeAt between @time1 and @time2
	
	select top 1 @open = Price from [dbo].[190224_TradeBitstamp] where Instrument1='btc' and TradeAt between @time1 and @time2
	select top 1 @close = Price from [dbo].[190224_TradeBitstamp] where Instrument1='btc' and TradeAt between @time1 and @time2 order by id desc
	select @high = max(Price) from [dbo].[190224_TradeBitstamp] where Instrument1='btc' and TradeAt between @time1 and @time2
	select @low = min(Price) from [dbo].[190224_TradeBitstamp] where Instrument1='btc' and TradeAt between @time1 and @time2
	
	select @time1,@time2,@open 'PriceOpen',@high 'HighPrice', @low 'LowPrice',@close 'PriceClose', @x1/@x2 'Ratio', (@x1-@x2)/(@x1+@x2)*100 'Pcnt', @x1 'BUY', @x2 'SELL', @x1+@x2 'SUM', @x1-@x2 'DIFF'

	set @time1=@time2
	set @count=@count+1
end