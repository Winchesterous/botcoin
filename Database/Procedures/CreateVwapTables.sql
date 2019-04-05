declare @dt datetime, @endDate datetime, @drop bit=0
set @dt='2019-04-01 00:00:00'
set @endDate='2019-04-04 00:00:00'

while @dt <> @endDate
begin
	--exec CreateBitmexTrade @dt, @drop
	exec CreateIndicatorVwap @dt, @drop
	exec CreateIndicatorVwapRatios @dt, @drop
	select @dt
	set @dt = dateadd(day,1,@dt)
end

