declare @time datetime='2018-06-23 18:58:00', @m1 int=0, @m2 int=6,@count int=100, 
		@i int=0, @t1 datetime, @t2 datetime

while @i < @count
begin
	set @t1=dateadd(second,@m1,@time)
	set @t2=dateadd(second,@m2,@time)
		
	--insert into AnalyticsBitstamp (CreatedAt,AvgPrice,OrdersCount,OrderSide)
	select @t1,min(AskPrice) 'Min',max(AskPrice) 'Max',avg(AskPrice),count(AskPrice),sum(AskAmount),'Ask' from OrderBookBitstamp where
	CreatedAt between @t1 and @t2
	and Instrument1='btc' and AskPrice>10
	union
	--insert into AnalyticsBitstamp (CreatedAt,AvgPrice,OrdersCount,OrderSide)
	select @t1,min(Bidprice) 'Min',max(Bidprice) 'Max',avg(Bidprice) 'Avg',count(BidPrice) 'Count',sum(BidAmount) 'Sum','Bid' from OrderBookBitstamp where
	CreatedAt between @t1 and @t2
	and Instrument1='btc' and BidPrice>10

	set @i=@i + 1
	set @m1=@m1 + 6
	set @m2=@m2 + 6
end

-- 2018-04-26 18:57:48.380 -- 2018-04-27 00:00:00.000
-- 2018-04-27 00:00:00.000 -- 2018-04-27 23:59:59.973
-- 2018-04-28 00:00:00.020 -- 2018-04-28 19:33:28.740
-- 2018-04-29 15:54:06.933 -- 2018-04-29 23:59:59.993
-- 2018-04-30 00:00:00.040 -- 2018-04-30 08:52:50.233
-- 2018-04-30 ???