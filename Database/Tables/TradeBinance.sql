declare @date datetime = getdate(), @sql nvarchar(max), @suffix varchar(6)
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
CREATE TABLE dbo.TradeBinance' + @suffix + '
(
	[Id]			[int] IDENTITY(1,1)	NOT NULL,
	[CreatedAt]		[datetime]			NOT NULL,
	[TradeAt]		[datetime]			NOT NULL,
	[Instrument1]	[nvarchar](5)		NOT NULL,
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

CREATE INDEX IDX_BinanceTrade' + @suffix + '_Time ON dbo.TradeBinance' + @suffix + ' ([CreatedAt],[TradeAt])
CREATE INDEX IDX_BinanceTrade' + @suffix + '_Instrument ON dbo.TradeBinance' + @suffix + ' ([Instrument1],[Instrument2])'

exec (@sql)
go
