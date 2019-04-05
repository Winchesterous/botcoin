
CREATE VIEW BalancesView
AS
	SELECT 
		a.ExchangeId as Exchange,
		sum(Amount) as Amount,
		(
			select sum(Amount) from Account a1 where a1.ExchangeId = a.ExchangeId and 
			(a1.OperationCode='SND' or a1.OperationCode='SYN') group by a1.ExchangeId
		) 
		as SND
		,(
			select sum(Amount) from Account a1 where a1.ExchangeId = a.ExchangeId and 
			a1.OperationCode='BUY' group by a1.ExchangeId
		) 
		as BUY
		,(
			select sum(Amount) from Account a1 where a1.ExchangeId = a.ExchangeId and 
			a1.OperationCode='SEL' group by a1.ExchangeId
		) 
		as SELL
		,(
			select sum(Amount) from Account a1 where a1.ExchangeId = a.ExchangeId and 
			a1.OperationCode='SEL' group by a1.ExchangeId
		) 
		+
		(
			select sum(Amount) from Account a1 where a1.ExchangeId = a.ExchangeId and 
			a1.OperationCode='BUY' group by a1.ExchangeId
		) 
		as Profit
	FROM 
		Account a
	GROUP BY 
		a.ExchangeId
GO