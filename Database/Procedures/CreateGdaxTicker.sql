CREATE PROCEDURE CreateGdaxTicker
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_TickerGdax]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_TickerGdax]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_TickerGdax]
		(
			[Id]				[int] IDENTITY(1,1)	NOT NULL,
			[Sequence]			[bigint]			NOT NULL,
			[Instrument1]		[nvarchar](5)		NOT NULL,
			[Instrument2]		[nvarchar](5)		NOT NULL,
			[OrderSide]			[nvarchar](4)		NOT NULL,
			[LastQty]			[decimal](18, 8)		NULL,
			[Price]				[decimal](18, 8)		NULL,
			[BestBidPrice]		[decimal](18, 8)		NULL,
			[BestAskPrice]		[decimal](18, 8)		NULL,
			[CreatedAt]			[datetime]			NOT NULL,
			[Timestamp]			[datetime]			NOT NULL,
			CONSTRAINT PK_TickerGdax' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]

		CREATE INDEX IDX_GdaxTicker' + @suffix + '_Time ON dbo.[' + @suffix + '_TickerGdax] ([CreatedAt],[Timestamp])
		CREATE INDEX IDX_GdaxTicker' + @suffix + '_Instrument ON dbo.[' + @suffix + '_TickerGdax] ([Instrument1],[Instrument2])
		CREATE INDEX IDX_GdaxTicker' + @suffix + '_Side ON dbo.[' + @suffix + '_TickerGdax] ([OrderSide])
		CREATE INDEX IDX_GdaxTicker' + @suffix + '_Price ON dbo.[' + @suffix + '_TickerGdax] ([Price])'

	exec (@sql)
END
GO
