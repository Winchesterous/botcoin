declare @dt datetime, @i int=0, @start int=0, @days int=1, @drop bit=0
set @dt = dateadd(day,@start,getutcdate())

while @i < @days
begin
	exec CreateGdaxOrderBook @dt, @drop
	exec CreateBitstampOrderBook @dt, @drop
	exec CreateIndicatorVwap @dt, @drop
	exec CreateIndicatorVwapRatios @dt, @drop
	exec CreateBitmexLiquidation @dt, @drop
	exec CreateBitmexInstrument @dt, @drop
	exec CreateBitmexTrade @dt, @drop
	exec CreateBitmexTicker @dt, @drop
	exec CreateBitmexFunding @dt, @drop
	exec CreateBinanceTrade @dt, @drop
	exec CreateBinanceTicker @dt, @drop
	exec CreateGdaxTicker @dt, @drop
	exec CreateBitstampTicker @dt, @drop
	exec CreateBitstampTrade @dt, @drop
	set @dt = dateadd(day,1,@dt)
	set @i = @i + 1
end
set @i = 0
set @dt = dateadd(day,@start,getutcdate())
if @drop = 0
	begin
		while @i < @days
		begin
			exec CreateBinanceOrderBook @dt, @drop
			set @dt = dateadd(day,1,@dt)
			set @i = @i + 1
		end
		set @i = 0
		set @dt = dateadd(day,@start,getutcdate())
		while @i < @days
		begin
			exec CreateBinanceOrderBookValue @dt, @drop
			set @dt = dateadd(day,1,@dt)
			set @i = @i + 1
		end
	end
else
	begin
		while @i < @days
		begin
			exec CreateBinanceOrderBookValue @dt, @drop
			set @dt = dateadd(day,1,@dt)
			set @i = @i + 1
		end
		set @i = 0
		set @dt = dateadd(day,@start,getutcdate())
		while @i < @days
		begin
			exec CreateBinanceOrderBook @dt, @drop
			set @dt = dateadd(day,1,@dt)
			set @i = @i + 1
		end
	end