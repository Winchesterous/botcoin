declare @time1 datetime='2019-02-18 00:00:00.000',@endDate datetime, @time2 datetime,
	@xbt decimal(18,3),@eth decimal(18,3),@ltc decimal(18,3),@xrp decimal(18,3),@eos decimal(18,3),@ada decimal(18,3)

set @endDate=dateadd(hour,24,@time1)
set @time2=DATEADD(minute,3,@time1)

create table #VwapIdx (Ts datetime, Idx decimal(18,3))

while @time1 <> @endDate
begin
	select @xbt=VwapGainPcnt from [dbo].[VwapIndex] where Instrument='xbtusd' and [Timestamp] between @time1 and @time2
	select @eth=VwapGainPcnt from [dbo].[VwapIndex] where Instrument='ethusd' and [Timestamp] between @time1 and @time2
	select @ltc=VwapGainPcnt from [dbo].[VwapIndex] where Instrument='ltch19' and [Timestamp] between @time1 and @time2
	select @xrp=VwapGainPcnt from [dbo].[VwapIndex] where Instrument='xrph19' and [Timestamp] between @time1 and @time2
	select @eos=VwapGainPcnt from [dbo].[VwapIndex] where Instrument='eosh19' and [Timestamp] between @time1 and @time2
	select @ada=VwapGainPcnt from [dbo].[VwapIndex] where Instrument='adah19' and [Timestamp] between @time1 and @time2

	--select @time1,(@xbt+@eth+@ltc+@xrp+@eos+@ada)/6
	insert into #VwapIdx values(@time1, (@xbt+@eth+@ltc+@xrp+@eos+@ada)/6)

	set @time1=@time2
	set @time2=DATEADD(minute,3,@time1)
end

select * from #VwapIdx
drop table #VwapIdx



