declare @ok float, @back float, @fees float, @profit float

select @ok=sum(Profit)
	from Trade t
	join Exchange e1 on t.ExchangeId1=e1.id
	join Exchange e2 on t.Exchangeid2=e2.id
	join Currency c on c.id=t.InstrumentId1
	where TransCode='ok'

select @back=sum(Profit)
	from Trade t
	join Exchange e1 on t.ExchangeId1=e1.id
	join Exchange e2 on t.Exchangeid2=e2.id
	join Currency c on c.id=t.InstrumentId1
	where TransCode='back'

if @back is null
	set @back = 0

set @profit = @ok + @back

select @fees=sum(Fees)
	from Trade t
	join Exchange e1 on t.ExchangeId1=e1.id
	join Exchange e2 on t.Exchangeid2=e2.id
	join Currency c on c.id=t.InstrumentId1
	where TransCode='ok' or TransCode='back'

select
	@fees 'Fees',
	@ok 'Ok',
	@back 'Back',
	round(100 * cast(abs(@back)/@ok as float),4) '% Back',
	round(@profit, 4) 'Profit',
	round(100 * cast(@profit/@ok as float), 4) 'Profit %'
	
select 
	(select top 1 CreatedAt from Trade order by id desc) -
	(select top 1 CreatedAt from Trade order by id)