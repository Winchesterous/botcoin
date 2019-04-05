CREATE PROCEDURE CreateGdaxOrderBook
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_OrderBookGdax]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_OrderBookGdax]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_OrderBookGdax]
		(
			[Id]			[int] IDENTITY(1,1)	NOT NULL,
			[Timestamp]		[datetime]			NOT	NULL,	
			[CreatedAt]		[datetime]			NOT NULL,
			[Instrument1]	[nvarchar](5)		NOT NULL,
			[Instrument2]	[nvarchar](5)		NOT NULL,
			[OrderType]		[nvarchar](6)		NOT	NULL,
			[OrderState]	[nvarchar](6)		NOT	NULL,
			[BidPrice]		[decimal](18, 8)		NULL,
			[AskPrice]		[decimal](18, 8)		NULL,
			[BidQty]		[decimal](18, 8)		NULL,
			[AskQty]		[decimal](18, 8)		NULL,
			[IsFullyFilled] [bit]					NULL,
			[BidFunds]		[decimal](18, 8)		NULL,
			[AskFunds]		[decimal](18, 8)		NULL,
			[StopPrice]		[decimal](18, 8)		NULL,
			[StopType]		[nvarchar](20)			NULL,
			[OrderReason]	[nvarchar](10)			NULL,
			[OrderId]		[nvarchar](50)			NULL,
			[MakerOrderId]	[nvarchar](50)			NULL,
			[TakerOrderId]	[nvarchar](50)			NULL,
			[Sequence]		[bigint]			NOT NULL,
			CONSTRAINT PK_OrderBookGdax' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]
		CREATE INDEX IDX_Gdax' + @suffix + '_Time ON dbo.[' + @suffix + '_OrderBookGdax] ([CreatedAt],[Timestamp])
		CREATE INDEX IDX_Gdax' + @suffix + '_Instrument ON dbo.[' + @suffix + '_OrderBookGdax] ([Instrument1],[Instrument2])
		CREATE INDEX IDX_Gdax' + @suffix + '_Price ON dbo.[' + @suffix + '_OrderBookGdax] ([BidPrice],[AskPrice])
		CREATE INDEX IDX_Gdax' + @suffix + '_Amount ON dbo.[' + @suffix + '_OrderBookGdax] ([BidQty],[AskQty])
		CREATE INDEX IDX_Gdax' + @suffix + '_Order ON dbo.[' + @suffix + '_OrderBookGdax] ([OrderState],[OrderType],[OrderReason])
		CREATE INDEX IDX_Gdax' + @suffix + '_Ids ON dbo.[' + @suffix + '_OrderBookGdax] ([OrderId],[MakerOrderId],[TakerOrderId])

		ALTER TABLE [dbo].[' + @suffix + '_OrderBookGdax] ADD CONSTRAINT [CK_OrderBookGdax1_'+ @suffix +'] 
		CHECK ([OrderState] IN (''open'',''recv'',''done'',''match'',''change'',''active''))
		
		ALTER TABLE [dbo].[' + @suffix + '_OrderBookGdax] ADD CONSTRAINT [CK_OrderBookGdax2_'+ @suffix +']
		CHECK ([OrderType] IN (''limit'',''market'',''stop''))'

	exec (@sql)
END
GO