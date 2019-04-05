declare @t1 datetime, @t2 datetime, @i int, @minute int
set @t1='2018-04-15 4:00' -- '2018-04-12 10:17'
set @minute=2
set @i=1

while @i < 40
begin
	set @t2=dateadd(minute, @minute, @t1)

	select @i as Id,@t1,''as'Bid',sum(OrderBidAmount)as'Volume',max(OrderBidAmount)as'Max Vol',min(BidPrice)as'Min',avg(BidPrice)as'Avg Price',max(BidPrice)as'Max'--,avg(BidPrice)/max(BidPrice)as'Avg/Max'
	from OrderBookBinance 
	where CreatedAt between @t1 and @t2 and BidPrice > 0
		  and BidPrice > (select avg(BidPrice)*0.98728 from OrderBookBinance where CreatedAt between @t1 and @t2 and BidPrice > 0)

	select @i as Id,@t2,''as'Ask',sum(OrderAskAmount)as'Volume',max(OrderAskAmount)as'Max Vol',min(AskPrice)as'Min',avg(AskPrice)as'Avg Price',max(AskPrice)as'Max'--,min(AskPrice)/avg(AskPrice)as'Min/Avg'  
	from OrderBookBinance 
	where CreatedAt between @t1 and @t2 and AskPrice > 0
		  and AskPrice > (select avg(AskPrice)*0.98728 from OrderBookBinance where CreatedAt between @t1 and @t2 and AskPrice > 0)

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


