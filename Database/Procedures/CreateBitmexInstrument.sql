CREATE PROCEDURE CreateBitmexInstrument
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
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''dbo.[' + @suffix + '_BitmexInstrument]'') AND type in (N''U''))
		DROP TABLE dbo.[' + @suffix + '_BitmexInstrument]'
		
	if @dropOnly = 0
		set @sql = @sql + '
		CREATE TABLE dbo.[' + @suffix + '_BitmexInstrument]
		(
			[Id]					[int] IDENTITY(1,1)	NOT NULL,
			[Timestamp]				[datetime] NOT NULL,
			[Instrument]			[nvarchar](10) NOT NULL,
			[OpenInterest]			[bigint] NULL,
			[OpenValue]				[bigint] NULL,
			[TotalVolume]			[bigint] NULL,
			[Volume]				[bigint] NULL,
			[Volume24h]				[bigint] NULL,
			[TotalTurnover]			[bigint] NULL,
			[Turnover]				[bigint] NULL,
			[Turnover24h]			[bigint] NULL,
			[HomeNotional24h]		[nvarchar](100) NULL,
			[ForeignNotional24h]	[bigint] NULL,
			CONSTRAINT PK_BitmexInstrument' + @suffix + ' PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)
			WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) 
		ON [PRIMARY]
		CREATE INDEX IDX_BitmexInstrument' + @suffix + '_Time ON dbo.[' + @suffix + '_BitmexInstrument] ([Timestamp])
		CREATE INDEX IDX_BitmexInstrument' + @suffix + '_Instrument ON dbo.[' + @suffix + '_BitmexInstrument] ([Instrument])'

	exec (@sql)
END
GO
