CREATE PROCEDURE CreateBinanceTrade
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_TradeBinance]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_TradeBinance]' 
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_TradeBinance]
		(
			[Id]			[int] IDENTITY(1,1)	NOT NULL,
			[CreatedAt]		[datetime]			NOT NULL,
			[TradeAt]		[datetime]			NOT NULL,
			[Instrument1]	[nvarchar](6)		NOT NULL,
			[Instrument2]	[nvarchar](5)		NOT NULL,
			[TradeId]		[nvarchar](15)		NOT NULL,
			[BuyerOrderId]	[nvarchar](15)		NOT NULL,
			[SellerOrderId]	[nvarchar](15)		NOT NULL,
			[Price]			[decimal](18, 8)	NOT NULL,
			[Quantity]		[decimal](18, 8)	NOT NULL,
			[Volume]		[decimal](18, 8)	NOT NULL,
			[IsBuyerMMaker]	[bit]				NOT NULL,
			CONSTRAINT PK_TradeBinance' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]

		CREATE INDEX IDX_BinanceTrade' + @suffix + '_Time ON dbo.[' + @suffix + '_TradeBinance] ([CreatedAt],[TradeAt])
		CREATE INDEX IDX_BinanceTrade' + @suffix + '_Instrument ON dbo.[' + @suffix + '_TradeBinance] ([Instrument1],[Instrument2])'

	exec (@sql)
END
GO
