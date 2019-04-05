declare @t1 datetime, @t2 datetime, @i int, @minute int
set @t1= '2018-04-12 10:17'
set @minute=2
set @i=1

while @i < 106
begin
	set @t2=dateadd(minute, @minute, @t1)

	select @i as Id,@t1,''as'Bid',round(sum(OrderBidAmount),4)as'Volume',max(OrderBidAmount)as'Max Vol',min(BidPrice)as'Min',avg(BidPrice)as'AVG',max(BidPrice)as'Max',max(BidPrice)-avg(BidPrice)as'Max-Avg'
	from OrderBookBitstamp
	where CreatedAt between @t1 and @t2 and BidPrice > 0
		  and BidPrice > (select avg(BidPrice)*0.98728 from OrderBookBitstamp where CreatedAt between @t1 and @t2 and BidPrice > 0)

	select @i as Id,@t2,''as'Ask',round(sum(OrderAskAmount),4)as'Volume',max(OrderAskAmount)as'Max Vol',min(AskPrice)as'Min',avg(AskPrice)as'AVG',max(AskPrice)as'Max',avg(AskPrice)-min(AskPrice)as'Avg-Min'
	from OrderBookBitstamp
	where CreatedAt between @t1 and @t2 and AskPrice > 0
		  and AskPrice > (select avg(AskPrice)*0.98728 from OrderBookBitstamp where CreatedAt between @t1 and @t2 and AskPrice > 0)

	select (
		select avg(AskPrice)
		from OrderBookBitstamp
		where CreatedAt between @t1 and @t2 and AskPrice > 0
				and AskPrice > (select avg(AskPrice)*0.98728 from OrderBookBitstamp where CreatedAt between @t1 and @t2 and AskPrice > 0)
		) - (
		select avg(BidPrice)
		from OrderBookBitstamp
		where CreatedAt between @t1 and @t2 and BidPrice > 0
			  and BidPrice > (select avg(BidPrice)*0.98728 from OrderBookBitstamp where CreatedAt between @t1 and @t2 and BidPrice > 0) 	  
		)
		as'Spread'

	set @i = @i + 1
	set @t1 = @t2 
end


