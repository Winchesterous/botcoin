declare @date datetime = getdate(), @sql nvarchar(max), @suffix varchar(6)
set @suffix = 
	cast(datepart(year, @date)-2000 as varchar(2)) +
	case 
		when datepart(month, @date) < 10 then '0' + cast(datepart(month, @date) as varchar(2))
		when datepart(month, @date) >= 10 then cast(datepart(month, @date) as varchar(2))
	end +
	case 
		when datepart(day, @date) < 10 then '0' + cast(datepart(day, @date) as varchar(2))
		when datepart(day, @date) >= 10 then cast(datepart(day, @date) as varchar(2))
	end

set @sql='
CREATE TABLE dbo.OrderBookValueBinance' + @suffix + '
(
	[Id]			[int] IDENTITY(1,1)	NOT NULL,
	[OrderBookId]	[int]				NOT NULL,
	[PriceType]		[smallint]			NOT NULL,	-- 0 BID 1 ASK
	[Price]			[decimal](18, 8)	NOT NULL,
	[Amount]		[decimal](18, 8)	NOT NULL,
	
	CONSTRAINT PK_OrderBookValueBinance' + @suffix + ' PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]
ALTER TABLE dbo.OrderBookValueBinance' + @suffix + '  WITH CHECK ADD  CONSTRAINT FK_OrderBookValueBinance' + @suffix + '_OrderBook 
				FOREIGN KEY([OrderBookId]) REFERENCES dbo.OrderBookBinance' + @suffix + ' ([Id])
ALTER TABLE dbo.OrderBookValueBinance' + @suffix + ' CHECK CONSTRAINT FK_OrderBookValueBinance' + @suffix + '_OrderBook'

exec (@sql)
go