declare @dt datetime='2018-07-30 18:46:15.000', @sec int=-5

select max(Price)'High',min(Price)'Low'
from 
( 
	select Price from [dbo].[TradeBitstamp180730]
	where TradeAt between DATEADD(SECOND,@sec,@dt) and @dt
)x

select top 1 Price from [dbo].[TradeBitstamp180730]
where TradeAt between DATEADD(SECOND,@sec,@dt) and @dt
order by TradeAt

select top 1 TradeAt,Price from [dbo].[TradeBitstamp180730]
where TradeAt between DATEADD(SECOND,@sec,@dt) and @dt
order by TradeAt desc

