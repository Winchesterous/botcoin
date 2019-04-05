CREATE PROCEDURE CreateBinanceOrderBook
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_OrderBookBinance]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_OrderBookBinance]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_OrderBookBinance]
		(
			[Id]			[int] IDENTITY(1,1)	NOT NULL,
			[CreatedAt]		[datetime]			NOT NULL,
			[Instrument1]	[nvarchar](6)		NOT NULL,
			[Instrument2]	[nvarchar](5)		NOT NULL,	
			[Spread]		[decimal](18, 8)	NOT NULL,	
			[BidPrice]		[decimal](18, 8)	NOT NULL,
			[AskPrice]		[decimal](18, 8)	NOT NULL,
			[SumBid]		[money]				NOT NULL,
			[SumAsk]		[money]				NOT NULL,
			[AvgBid]		[money]				NOT NULL,
			[AvgAsk]		[money]				NOT NULL,
			[MinBid]		[money]				NOT NULL,
			[MinAsk]		[money]				NOT NULL,
			[MaxBid]		[money]				NOT NULL,
			[MaxAsk]		[money]				NOT NULL,
	
			CONSTRAINT PK_OrderBookBinance' + @suffix +' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]
		CREATE INDEX IDX_Binance' + @suffix + '_Time ON dbo.[' + @suffix + '_OrderBookBinance] ([CreatedAt])
		CREATE INDEX IDX_Binance' + @suffix + '_Instrument ON dbo.[' + @suffix + '_OrderBookBinance] ([Instrument1],[Instrument2])
		CREATE INDEX IDX_Binance' + @suffix + '_Price ON dbo.[' + @suffix + '_OrderBookBinance] ([BidPrice],[AskPrice])
		CREATE INDEX IDX_Binance' + @suffix + '_Sum ON dbo.[' + @suffix + '_OrderBookBinance] ([SumBid],[SumAsk])
		CREATE INDEX IDX_Binance' + @suffix + '_Avg ON dbo.[' + @suffix + '_OrderBookBinance] ([AvgBid],[AvgAsk])
		CREATE INDEX IDX_Binance' + @suffix + '_Min ON dbo.[' + @suffix + '_OrderBookBinance] ([MinBid],[MinAsk])
		CREATE INDEX IDX_Binance' + @suffix + '_Max ON dbo.[' + @suffix + '_OrderBookBinance] ([MaxBid],[MaxAsk])'

	exec (@sql)
END
GO
