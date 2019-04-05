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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TradeOkEx]') AND type in (N'U'))
	DROP TABLE [dbo].[TradeOkEx]

set @sql='
CREATE TABLE dbo.OrderBookBitmex' + @suffix + '
(
	[Id]			[int] IDENTITY(1,1)	NOT NULL,
	[CreatedAt]		[datetime]			NOT NULL,
	[Instrument1]	[nvarchar](5)		NOT NULL,
	[Instrument2]	[nvarchar](5)		NOT NULL,
	[BidPrice]		[decimal](18, 8)	NOT NULL,
	[AskPrice]		[decimal](18, 8)	NOT NULL,
	[BidAmount]		[decimal](18, 8)	NOT NULL,
	[AskAmount]		[decimal](18, 8)	NOT NULL,
	
	CONSTRAINT PK_OrderBookBitmex' + @suffix + ' PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
ON [PRIMARY]

CREATE INDEX IDX_Bitmex' + @suffix + '_Time ON dbo.OrderBookBitmex' + @suffix + ' ([CreatedAt])
CREATE INDEX IDX_Bitmex' + @suffix + '_Instrument ON dbo.OrderBookBitmex' + @suffix + ' ([Instrument1],[Instrument2])
CREATE INDEX IDX_Bitmex' + @suffix + '_Price ON dbo.OrderBookBitmex' + @suffix + ' ([BidPrice],[AskPrice])
CREATE INDEX IDX_Bitmex' + @suffix + '_Amount ON dbo.OrderBookBitmex' + @suffix + ' ([BidAmount],[AskAmount])'

exec (@sql)
go
