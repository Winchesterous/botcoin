declare @sec int = -6, @time datetime = '2018-07-02 19:14:00' --, @from datetime='2018-06-30 19:20:00'

while @time <= '2018-07-02 20:26:00'
begin
  select count(*) 'BID/ASK',sum(Amount)'Sum' from dbo.TradeBitstamp180702
  where CreatedAt between dateadd(second, @sec, @time) and @time
  and TradeType='BID'
union
  select count(*),sum(Amount) from dbo.TradeBitstamp180702
  where CreatedAt between dateadd(second, @sec, @time) and @time
  and TradeType='ASK'

  select max(Price)'High',min(Price)'Low',max(Price)-min(Price)'Len',dateadd(second, @sec, @time)'Time 1',@time'Time 2'
  from dbo.TradeBitstamp180702
  where CreatedAt between dateadd(second, @sec, @time) and @time
  
  set @time=dateadd(minute,1,@time)
end

--select count(*) 'BID/ASK',sum(Amount)'Sum' from dbo.TradeBitstamp180702
--where CreatedAt between @from and getutcdate()
--and TradeType='BID'
--union
--select count(*) 'BID/ASK',sum(Amount)'Sum' from dbo.TradeBitstamp180702
--where CreatedAt between @from and getutcdate()
--and TradeType='ASK'