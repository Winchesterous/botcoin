declare @t1 datetime, @t2 datetime, @end datetime, @i int
set @t1='2018-04-15 4:00'
set @end=dateadd(hour,1,@t1)
set @i=1

while @t1 < @end
begin
	set @t2=dateadd(minute, 1, @t1)

	select @i'Id',avg(BidPrice)as'BID' from OrderBookHitBtc where CreatedAt between @t1 and @t2 and Bidprice > 0
	select @i'Id',avg(AskPrice)as'ASK' from OrderBookBitstamp where CreatedAt between @t1 and @t2 and IsDeleted=0 and AskPrice > 0
	
	select @i 'Id',
		@t1 'T1',
		@t2 'T2',
		(
		select avg(BidPrice)
		from OrderBookHitBtc
		where CreatedAt between @t1 and @t2 and BidPrice > 0
		) - (
		select avg(AskPrice)
		from OrderBookBitstamp
		where CreatedAt between @t1 and @t2 and IsDeleted=0 and AskPrice > 0
		)
		as'Spread'

	set @t1 = @t2 
	set @i = @i+1
end
