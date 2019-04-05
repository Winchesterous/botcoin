declare @time1 datetime, @time2 datetime, @interval int, @lowPrice money, @highPrice money, 
		@closePrice money, @vol float, @typicalPrice float, @typVol float, @sumVol float,@vwap float
set @time1='2019-02-25 00:00:00'
set @interval=3
set @typVol=0
set @sumVol=0

while @time1 <= '2019-02-25 00:03:00' -- '2019-02-24 23:59:59'
begin
	set @time2=dateadd(minute,@interval,@time1)
	
	select @highPrice=max(Price),@lowPrice=min(Price),@vol=sum(Quantity) from [TradeData].[dbo].[190225_TradeBinance] with (nolock)
	where TradeAt between @time1 and @time2 and Instrument1='btc' and Instrument2='usd'

	select top 1 @closePrice=Price from [TradeData].[dbo].[190225_TradeBinance] with (nolock)
	where TradeAt between @time1 and @time2 and Instrument1='btc' and Instrument2='usd' order by id desc

	set @typicalPrice=(@highPrice + @lowPrice + @closePrice) / 3
	set @typVol=@typVol + (@typicalPrice*@vol)
	set @sumVol=@sumVol + @vol
	set @vwap=@typVol/@sumVol

	select @time1,@time2,@vwap
	set @time1=@time2
end
