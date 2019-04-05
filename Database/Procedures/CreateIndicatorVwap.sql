CREATE PROCEDURE CreateIndicatorVwap
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_IndicatorVwap]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_IndicatorVwap]' 
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_IndicatorVwap]
		(
			[Id]					[int] IDENTITY(1,1)	NOT NULL,
			[Exchange]				[nvarchar](15)		NOT NULL,
			[TimePeriod]			[nvarchar](3)		NOT NULL,
			[Timestamp]				[datetime]			NOT NULL,
			[Instrument]			[nvarchar](10)		NOT NULL,
			[SumVolume]				[float]				NOT NULL,
			[SumTypeVol]			[float]				NOT NULL,
			[Vwap]					[decimal](18, 8)	NOT NULL,
			[VwapRatioPcnt]			[float]				NOT NULL,
			[TradesCount]			[int]				NOT NULL,
			[TradesCountRatio]		[decimal](18, 2)	NOT NULL,
			[TotalTradesCount]		[int]				NOT NULL,
			[TotalTradesRatio]		[decimal](18, 2)	NOT NULL,
			[OpenPrice]				[decimal](18, 8)	NOT NULL,
			[ClosePrice]			[decimal](18, 8)	NOT NULL,
			[ExtremClosePrice]		[int]				NOT	NULL,
			[VwapGain15Min]			[decimal](18, 4)		NULL,
			[CumVwapGain15Min]		[decimal](18, 4)		NULL,
			[VwapGainRatio15Min]	[decimal](18, 4)		NULL,
			CONSTRAINT PK_IndicatorVwap' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]

		CREATE INDEX IDX_IndicatorVwap' + @suffix + '_1 ON dbo.[' + @suffix + '_IndicatorVwap] ([TimePeriod])
		CREATE INDEX IDX_IndicatorVwap' + @suffix + '_2 ON dbo.[' + @suffix + '_IndicatorVwap] ([Instrument])
		CREATE INDEX IDX_IndicatorVwap' + @suffix + '_3 ON dbo.[' + @suffix + '_IndicatorVwap] ([Exchange])
		
		ALTER TABLE dbo.[' + @suffix + '_IndicatorVwap] ADD CONSTRAINT [CK_IndicatorVwap' + @suffix + '] 
		CHECK ([TimePeriod] IN (''3m'',''5m'',''15m'',''30m'',''45m'',''1h'',''2h'',''4h'',''8h''))'
	exec (@sql)
END
GO
