CREATE PROCEDURE CreateBitstampTrade
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_TradeBitstamp]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_TradeBitstamp]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE [dbo].[' + @suffix + '_TradeBitstamp]
		(
			[Id]			[int] IDENTITY(1,1)	NOT NULL,
			[CreatedAt]		[datetime]			NOT NULL,
			[TradeAt]		[datetime]			NOT NULL,
			[TradeType]		[nvarchar](4)		NOT NULL,
			[Instrument1]	[nvarchar](5)		NOT NULL,
			[Instrument2]	[nvarchar](5)		NOT NULL,
			[TradeId]		[nvarchar](15)		NOT NULL,
			[Price]			[decimal](18, 8)	NOT NULL,
			[Quantity]		[decimal](18, 8)	NOT NULL,
			[Volume]		[decimal](18, 8)	NOT NULL,
			CONSTRAINT [PK_TradeBitstamp' + @suffix + '] PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]

		CREATE INDEX IDX_BitstampTrade' + @suffix + '_Time ON [dbo].[' + @suffix + '_TradeBitstamp] ([CreatedAt],[TradeAt])
		CREATE INDEX IDX_BitstampTrade' + @suffix + '_Instrument ON [dbo].[' + @suffix + '_TradeBitstamp] ([Instrument1],[Instrument2])

		ALTER TABLE [dbo].[' + @suffix + '_TradeBitstamp] ADD CONSTRAINT [CK_TradeBitstamp' + @suffix + '] 
		CHECK ([TradeType] IN (''Buy'',''Sell''))'

	exec (@sql)
END
GO
