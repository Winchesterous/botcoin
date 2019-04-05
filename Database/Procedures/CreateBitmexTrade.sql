CREATE PROCEDURE CreateBitmexTrade
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_TradeBitmex]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_TradeBitmex]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_TradeBitmex]
		(
			[Id]					[int] IDENTITY(1,1)	NOT NULL,
			[Timestamp]				[datetime] NOT NULL,
			[Instrument]			[nvarchar](10) NOT NULL,
			[Side]					[nvarchar](4) NOT NULL,
			[Price]					[decimal](18, 8) NOT NULL,
			[Size]					[bigint] NOT NULL,
			[TickDirection]			[nvarchar](2) NOT NULL,
			[GrossValue]			[bigint] NOT NULL,
			CONSTRAINT PK_TradeBitmex' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]
		CREATE INDEX IDX_BitmexTrade' + @suffix + '_Time ON dbo.[' + @suffix + '_TradeBitmex] ([Timestamp])
		CREATE INDEX IDX_BitmexTrade' + @suffix + '_Side ON dbo.[' + @suffix + '_TradeBitmex] ([Side])
		CREATE INDEX IDX_BitmexTrade' + @suffix + '_Instrument ON dbo.[' + @suffix + '_TradeBitmex] ([Instrument])'

	exec (@sql)
END
GO
