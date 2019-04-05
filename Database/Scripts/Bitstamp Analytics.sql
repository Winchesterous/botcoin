declare @time datetime='2018-07-16 19:07:00' --'2018-07-16 17:33:00'
		,@m1 int=0, @m2 int=3,@count int=50, 
		@i int=0, @t1 datetime, @t2 datetime

while @i < @count
begin
	set @t1=dateadd(second,@m1,@time)
	set @t2=dateadd(second,@m2,@time)
		
	select @t1,min(AskPrice) 'Min',max(AskPrice) 'Max',avg(AskPrice)'Avg',count(AskPrice)'Count',sum(AskAmount)'Sum','Ask' from OrderBookBitstamp180716 
	where CreatedAt between @t1 and @t2 and Instrument1='btc' and AskPrice>10
	union
	select @t1,min(Bidprice) 'Min',max(Bidprice) 'Max',avg(Bidprice) 'Avg',count(BidPrice) 'Count',sum(BidAmount) 'Sum','Bid' from OrderBookBitstamp180716 
	where CreatedAt between @t1 and @t2 and Instrument1='btc' and BidPrice>10

	set @i=@i + 1
	set @m1=@m1 + 3
	set @m2=@m2 + 3
end