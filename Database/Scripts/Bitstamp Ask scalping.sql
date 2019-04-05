declare @t1 datetime, @t2 datetime, @avg decimal(18,8),@min decimal(18,8),@ratio float
set @t1=DATEADD(hour,-1,getdate())
set @t2=getdate()

select @avg=avg(askprice),@min=min(askprice),@ratio=round((1-min(askprice)/avg(askprice))*100,3) from OrderBookBitstamp 
where CreatedAt between @t1 and @t2
and AskPrice > 10

select @avg'Avg',@min'Min',@ratio'Ratio'

select CreatedAt-Timestamp,* from OrderBookBitstamp 
where CreatedAt between @t1 and @t2
and AskPrice>10 and AskPrice < @min * 1.02 and @ratio > 9
order by Timestamp