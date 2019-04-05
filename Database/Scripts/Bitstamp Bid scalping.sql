declare @t1 datetime, @t2 datetime, @avg decimal(18,8),@max decimal(18,8),@ratio float
set @t1=DATEADD(hour,-1,getdate())
set @t2=getdate()

select @avg=avg(bidprice),@max=max(bidprice),@ratio=round((1-avg(bidprice)/max(bidprice))*100,3) from OrderBookBitstamp 
where CreatedAt between @t1 and @t2
and BidPrice > 10

select @avg'Avg',@max'Max',@ratio'Ratio'

select OrderId,CreatedAt-Timestamp,CreatedAt,BidPrice,BidAmount from OrderBookBitstamp 
where CreatedAt between @t1 and @t2
and BidPrice>0 and BidPrice > @max * 0.98 and @ratio > 9
--order by BidAmount desc

