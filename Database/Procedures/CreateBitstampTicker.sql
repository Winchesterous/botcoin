CREATE PROCEDURE CreateBitstampTicker
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_TickerBitstamp]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_TickerBitstamp]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_TickerBitstamp]
		(
			[Id]				[int] IDENTITY(1,1)	NOT NULL,
			[CreatedAt]			[datetime]			NOT NULL,
			[Instrument1]		[nvarchar](5)		NOT NULL,
			[Instrument2]		[nvarchar](5)		NOT NULL,
			[Vwap]				[decimal](18, 8)	NOT NULL,
			[HighPrice]			[decimal](18, 8)	NOT NULL,
			[OpenPrice]			[decimal](18, 8)	NOT NULL,
			[LowPrice]			[decimal](18, 8)	NOT NULL,
			[BestBidPrice]		[decimal](18, 8)	NOT NULL,
			[BestAskPrice]		[decimal](18, 8)	NOT NULL,
			[Volume]			[decimal](18, 8)	NOT NULL,
			CONSTRAINT PK_TickerBitstamp' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]

		CREATE INDEX IDX_BitstampTicker' + @suffix + '_Time ON dbo.[' + @suffix + '_TickerBitstamp] ([CreatedAt])
		CREATE INDEX IDX_BitstampTicker' + @suffix + '_Instrument ON dbo.[' + @suffix + '_TickerBitstamp] ([Instrument1],[Instrument2])'

	exec (@sql)
END
GO
