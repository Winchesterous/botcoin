select * from (
select price,amount,pricetype from OrderBookValueBinance180719 where orderbookid=740-- and pricetype=0
)x1
except
select * from (
select price,amount,pricetype from OrderBookValueBinance180719 where orderbookid=750-- and pricetype=0
)x2

select CreatedAt from OrderBookBinance180719 where id in (740,750)
select * from TradeBitstamp180719 where createdat between '2018-07-19 13:33:05' and '2018-07-19 13:33:16'