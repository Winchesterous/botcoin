declare @dt datetime, @endDate datetime, @entryXrp float=0.00008426, @entryEos float=0.0008066,
		@pricePcnt1 float, @pricePcnt2 float, @ts datetime
	
set @dt='2019-02-12 18:42:00'
set @endDate='2019-02-12 23:59:00'

while @dt < @endDate
begin
	select @ts=v1.Timestamp,@pricePcnt1=(1-v2.ClosePrice/@entryEos)*100 
	from [dbo].[190212_IndicatorVWAP_Ratios] v1
	join [dbo].[190212_IndicatorVWAP] v2 on v1.Timestamp=v2.Timestamp and v1.Instrument1=v2.Instrument 
	where v1.Timestamp=@dt and v1.Instrument1='eosh19' and v1.Instrument2='xrph19'

	select @pricePcnt2=(1-@entryXrp/v2.ClosePrice)*100 
	from [dbo].[190212_IndicatorVWAP_Ratios] v1
	join [dbo].[190212_IndicatorVWAP] v2 on v1.Timestamp=v2.Timestamp and v1.Instrument2=v2.Instrument 
	where v1.Timestamp=@dt and v1.Instrument1='eosh19' and v1.Instrument2='xrph19'

	select @ts'Time',@pricePcnt1+@pricePcnt2 'Profit'--,@pricePcnt1'Pcnt 1',@pricePcnt2'Pcnt 2'
	set @dt = dateadd(minute,3,@dt)
end

