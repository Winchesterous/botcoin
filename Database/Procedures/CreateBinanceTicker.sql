CREATE PROCEDURE CreateBinanceTicker
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_TickerBinance]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_TickerBinance]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_TickerBinance]
		(
			[Id]				[int] IDENTITY(1,1)	NOT NULL,
			[CreatedAt]			[datetime]			NOT NULL,
			[Instrument1]		[nvarchar](6)		NOT NULL,
			[Instrument2]		[nvarchar](5)		NOT NULL,
			[Vwap]				[decimal](18, 8)	NOT NULL,
			[PriceChange]		[decimal](18, 8)	NOT NULL,
			[PriceChangePcnt]	[decimal](18, 2)	NOT NULL,
			[HighPrice]			[decimal](18, 8)	NOT NULL,
			[OpenPrice]			[decimal](18, 8)	NOT NULL,
			[LowPrice]			[decimal](18, 8)	NOT NULL,
			[QtyClose]			[decimal](18, 8)	NOT NULL,
			[BestBidPrice]		[decimal](18, 8)	NOT NULL,
			[BestAskPrice]		[decimal](18, 8)	NOT NULL,
			[BestBidQty]		[decimal](18, 8)	NOT NULL,
			[BestAskQty]		[decimal](18, 8)	NOT NULL,
			[ClosePricePrevDay]	[decimal](18, 8)	NOT NULL,
			[ClosePriceCurrDay]	[decimal](18, 8)	NOT NULL,
			[TotalBaseVolume]	[float]				NOT NULL,
			[TotalQuoteVolume]	[float]				NOT NULL,
			[OpenTimeStats]		[datetime]			NOT NULL,
			[CloseTimeStats]	[datetime]			NOT NULL,
			CONSTRAINT PK_TickerBinance' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]

		CREATE INDEX IDX_BinanceTicker' + @suffix + '_Time ON dbo.[' + @suffix + '_TickerBinance] ([CreatedAt])
		CREATE INDEX IDX_BinanceTicker' + @suffix + '_Instrument ON dbo.[' + @suffix + '_TickerBinance] ([Instrument1],[Instrument2])'

	exec (@sql)
END
GO
