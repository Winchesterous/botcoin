
select * from (select top 1 * from dbo.[181230_TickerBitmex] where Instrument='XBTUSD' and Price is not null order by id desc)x1
union
select * from (select top 1 * from dbo.[181230_TickerBitmex] where Instrument='XBTH19' and Price is not null order by id desc)x2
union
select * from (select top 1 * from dbo.[181230_TickerBitmex] where Instrument='XBTM19' and Price is not null order by id desc)x3

select 
--	(
	(select * from (
		select top 1 Price from dbo.[181230_TickerBitmex] where Instrument='XBTUSD' and Price is not null order by id desc
		)x1)
	-
	(select * from (
		select top 1 Price from dbo.[181230_TickerBitmex] where Instrument='XBTH19' and Price is not null order by id desc
		)x2)
--		) as 'XBTUSD - XBTH19'

select (
	(select * from (
		select top 1 Price from dbo.[181230_TickerBitmex] where Instrument='XBTUSD' and Price is not null order by id desc
		)x1)
	-
	(select * from (
		select top 1 Price from dbo.[181230_TickerBitmex] where Instrument='XBTM19' and Price is not null order by id desc
		)x2)
		) as 'XBTUSD - XBTM19'