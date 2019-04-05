
CREATE VIEW AccountView
AS
	SELECT 
		a.ExchangeId as Exchange,
		round(sum(BTC), 8) as BTC, 
		round(sum(BCH), 8) as BCH,
		round(sum(ETH), 8) as ETH, 
		round(sum(LTC), 8) as LTC,
		sum(Amount) as Amount
	FROM 
		Account a
	GROUP BY 
		a.ExchangeId
GO