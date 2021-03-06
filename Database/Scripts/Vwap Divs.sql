SELECT [Timestamp]
      ,[Instrument1]
      ,[Instrument2]
	  ,round(VwapGain30Min1*10,2) as 'VwapGainRatio1'
	  ,VwapGain30Min1 as 'VwapGain1 %'
      ,[CumVwapGain30Min1] as 'CumVwapGain1 %'
	  ,round(VwapGain30Min2*10,2) as 'VwapGainRatio2'
	  ,VwapGain30Min2 as 'VwapGain2 %'
      ,[CumVwapGain30Min2] as 'CumVwapGain2 %'
	  ,[PriceVwapDiv] as '__PriceVwapSpread__'
      ,[CumVwapGainDiv] as '__CumVwapGainDiv__'
	  ,CumVwapGainDiv+PriceVwapDiv as 'SUM'
  FROM [TradeData].[dbo].[190301_IndicatorVwapRatios]
  where 
  CumVwapGainDiv is not null and
  --Instrument1 not in ('trxh19') and Instrument2 not in ('trxh19')
   Instrument1 in ('ltch19') and Instrument2 in ('adah19')
  --and round(VwapGain30Min1*10,2) > 1
  order by [Timestamp]