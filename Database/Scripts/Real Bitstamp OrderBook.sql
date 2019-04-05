declare @dt1 datetime='2019-02-24 13:50:00', @dt2 datetime, @minute int=1, @idx int=1, @sec int=60

select * --sum(bidAmount)'Bid',sum(askamount)'Ask' 
from dbo.[190224_OrderBookBitstamp]
where OrderId in (
	select x1.OrderId from (
		select OrderId from dbo.[190224_OrderBookBitstamp]
		where CreatedAt between @dt1 and dateadd(second,@sec,@dt1) and IsDeleted=0
	)x1
	full join (
		select OrderId from dbo.[190224_OrderBookBitstamp]
		where CreatedAt between @dt1 and dateadd(second,@sec,@dt1) and IsDeleted=1
	)x2
	on x1.OrderId=x2.OrderId
	where x2.OrderId is null
)
and CreatedAt between @dt1 and dateadd(second,@sec,@dt1)
and Instrument1='BTC'
--order by Id 
