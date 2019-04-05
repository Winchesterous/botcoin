CREATE PROCEDURE CreateIndicatorVwapRatios
	@date		datetime,
	@dropOnly	bit = 0
AS
BEGIN
	SET NOCOUNT ON;

    declare @sql nvarchar(max), @suffix varchar(6)
	set @suffix = 
		cast(datepart(year, @date)-2000 as varchar(2)) 
		+
		case 
			when datepart(month, @date) < 10 then '0' + cast(datepart(month, @date) as varchar(2))
			when datepart(month, @date) >= 10 then cast(datepart(month, @date) as varchar(2))
		end 
		+
		case 
			when datepart(day, @date) < 10 then '0' + cast(datepart(day, @date) as varchar(2))
			when datepart(day, @date) >= 10 then cast(datepart(day, @date) as varchar(2))
		end

	set @sql='
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_IndicatorVwapRatios]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_IndicatorVwapRatios]' 
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_IndicatorVwapRatios]
		(
			[Id]				[int] IDENTITY(1,1)	NOT NULL,
			[Exchange]			[nvarchar](15)		NOT NULL,
			[TimePeriod]		[nvarchar](3)		NOT NULL,
			[Timestamp]			[datetime]			NOT NULL,
			[Instrument1]		[nvarchar](10)		NOT NULL,
			[Instrument2]		[nvarchar](10)		NOT NULL,
			[Vwaps15MinSpread]	[decimal](18,4)			NULL,
			[VwapGain15Min1]	[decimal](18,4)			NULL,
			[VwapGain15Min2]	[decimal](18,4)			NULL,
			[PriceVwapRatio1]	[decimal](18,4)		NOT NULL,
			[PriceVwapRatio2]	[decimal](18,4)		NOT NULL,
			[PriceVwapDiv]		[decimal](18,4)		NOT NULL,
			[CumVwapGain15Min1]	[decimal](18,4)			NULL,
			[CumVwapGain15Min2]	[decimal](18,4)			NULL,
			[CumVwapGainDiv]	[decimal](18,4)			NULL,
			CONSTRAINT PK_IndicatorVwapRatios' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]

		CREATE INDEX IDX_IndicatorVwapRatios' + @suffix + '_1 ON dbo.[' + @suffix + '_IndicatorVwapRatios] ([TimePeriod])
		CREATE INDEX IDX_IndicatorVwapRatios' + @suffix + '_2 ON dbo.[' + @suffix + '_IndicatorVwapRatios] ([Instrument1],[Instrument2])
		CREATE INDEX IDX_IndicatorVwapRatios' + @suffix + '_3 ON dbo.[' + @suffix + '_IndicatorVwapRatios] ([Exchange])
		
		ALTER TABLE dbo.[' + @suffix + '_IndicatorVwapRatios] ADD CONSTRAINT [CK_IndicatorVwapRatios' + @suffix + '] 
		CHECK ([TimePeriod] IN (''3m'',''5m'',''15m'',''30m'',''45m'',''1h'',''2h'',''4h'',''8h''))'
	exec (@sql)
END
GO
