declare @dt1 datetime='2018-08-01 0:00:00', @dt2 datetime, @minute int=1, @idx int=1

select * from OrderBookBitstamp180801 
where OrderId in (
	select x1.OrderId from (
	select OrderId from OrderBookBitstamp180801
	where CreatedAt between @dt1 and dateadd(second,10,@dt1) and IsDeleted=0
	)x1
	full join (
	select OrderId from OrderBookBitstamp180801
	where CreatedAt between @dt1 and dateadd(second,10,@dt1) and IsDeleted=1
	)x2
	on x1.OrderId=x2.OrderId
	where x2.OrderId is null
)
and CreatedAt between @dt1 and dateadd(second,10,@dt1)
order by Id 
--select * from OrderBookBitstamp180801
--where Orderid='1930207526'