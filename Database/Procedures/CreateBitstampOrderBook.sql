CREATE PROCEDURE CreateBitstampOrderBook
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_OrderBookBitstamp]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_OrderBookBitstamp]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_OrderBookBitstamp]
		(
			[Id]				[int] IDENTITY(1,1)	NOT NULL,
			[OrderId]			[nvarchar](20)		NOT	NULL,
			[IsDeleted]			[bit]				NOT	NULL,	
			[Timestamp]			[datetime]			NOT	NULL,	
			[CreatedAt]			[datetime]			NOT NULL,			
			[Instrument1]		[nvarchar](5)		NOT NULL,
			[Instrument2]		[nvarchar](5)		NOT NULL,
			[BidPrice]			[decimal](18, 8)	NOT NULL,
			[AskPrice]			[decimal](18, 8)	NOT NULL,
			[BidAmount]			[decimal](18, 8)	NOT NULL,
			[AskAmount]			[decimal](18, 8)	NOT NULL,
			[MicroTimestamp]	[bigint]			NOT	NULL,
			CONSTRAINT PK_OrderBookBitstamp' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]
		CREATE INDEX IDX_Bitstamp' + @suffix + '_Time ON dbo.[' + @suffix + '_OrderBookBitstamp] ([CreatedAt])
		CREATE INDEX IDX_Bitstamp' + @suffix + '_Order ON dbo.[' + @suffix + '_OrderBookBitstamp] ([OrderId])
		CREATE INDEX IDX_Bitstamp' + @suffix + '_Instrument ON dbo.[' + @suffix + '_OrderBookBitstamp] ([Instrument1],[Instrument2])
		CREATE INDEX IDX_Bitstamp' + @suffix + '_Price ON dbo.[' + @suffix + '_OrderBookBitstamp] ([BidPrice],[AskPrice])
		CREATE INDEX IDX_Bitstamp' + @suffix + '_Amount ON dbo.[' + @suffix + '_OrderBookBitstamp] ([BidAmount],[AskAmount])'

	exec (@sql)
END
GO
